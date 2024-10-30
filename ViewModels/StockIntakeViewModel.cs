using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using INVApp.Models;
using INVApp.Services;
using ZXing;
using ZXing.Net.Maui.Controls;

namespace INVApp.ViewModels
{
    public class StockIntakeViewModel : BaseViewModel
    {
        //Declare database
        private readonly DatabaseService _databaseService;

        public bool _isCameraOn;
        private string _selectedCategory;
        private string _scannedBarcode;

        // Properties to track initial values to flag for change
        private string _initialProductName;
        private string _initialCategory;
        private string _initialProductWeight;
        private decimal _initialWholesalePrice;
        private decimal _initialPrice;

        // Events
        public event Action<Product> ProductAdded;

        public StockIntakeViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;

            // Initilize Commands
            AddStockCommand = new Command(async () => await AddStock());
            ResetCommand = new Command(ResetFields);
            ToggleCameraCommand = new Command(ToggleCamera);

            IsStockReduction = false;

            Categories = new ObservableCollection<string>();

            LoadCategories();
            LoadDefaultCategory();
        }

        // Declare Variables
        public Product? SelectedProduct { get; set; }
        public string ProductName { get; set; }
        public string ProductWeight { get; set; }
        public int CurrentStockLevel { get; set; }
        public decimal WholesalePrice { get; set; } 
        public decimal Price { get; set; } 

        public ObservableCollection<string> Categories { get; set; }

        #region Property Change Flags

        //Flags
        public bool IsCategoryChanged => SelectedCategory != _initialCategory;
        public bool IsWeightChanged => ProductWeight != _initialProductWeight;
        public bool IsNameChanged => ProductName != _initialProductName;
        public bool IsWholesalePriceChanged => WholesalePrice != _initialWholesalePrice;
        public bool IsSalePriceChanged => Price != _initialPrice;

        // Selected Category Property
        public string ScannedBarcode
        {
            get => _scannedBarcode;
            set
            {
                _scannedBarcode = value;
                OnPropertyChanged(); 
            }
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged(nameof(IsCategoryChanged));
                OnPropertyChanged(nameof(SelectedCategory));
            }
        }

        private int _stockAdjustment;
        public int StockAdjustment
        {
            get => _stockAdjustment;
            set
            {
                _stockAdjustment = value;
                OnPropertyChanged();

                IsStockReduction = _stockAdjustment < 0;
                OnPropertyChanged(nameof(IsStockReduction));
            }
        }

        private bool _isStockReduction;
        public bool IsStockReduction
        {
            get => _isStockReduction;
            set
            {
                _isStockReduction = value;
                OnPropertyChanged();
            }
        }

        private string _stockReductionReason;
        public string StockReductionReason
        {
            get => _stockReductionReason;
            set
            {
                _stockReductionReason = value;
                OnPropertyChanged();
            }
        }

        #endregion

        // Declare Commands
        public ICommand AddStockCommand { get; }
        public ICommand IncreaseStockCommand { get; }
        public ICommand DecreaseStockCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand ToggleCameraCommand { get; }

        // Process Scanned Barcode
        public async Task ProcessScannedBarcode(string barcode)
        {
            var product = await _databaseService.GetProductByBarcodeAsync(barcode);
            ToggleCamera();

            if (product != null) // if product found in db
            {
                SelectedProduct = product;

                ScannedBarcode = barcode;
                ProductName = product.ProductName;
                SelectedCategory = product.Category;
                ProductWeight = product.ProductWeight;
                CurrentStockLevel = product.CurrentStockLevel;
                WholesalePrice = product.WholesalePrice; 
                Price = product.Price; 

                _initialProductName = product.ProductName;
                _initialCategory = product.Category;
                _initialProductWeight = product.ProductWeight;
                _initialWholesalePrice = product.WholesalePrice; 
                _initialPrice = product.Price; 

                Application.Current.Dispatcher.Dispatch(() =>
                {
                    App.NotificationService.Notify($"Match found: {product.ProductName}.");
                });
            }
            else // no exisiting product in db
            {

                ResetFields();
                ScannedBarcode = barcode;
                
                LoadDefaultCategory();

                Application.Current.Dispatcher.Dispatch(() =>
                {
                    App.NotificationService.Notify($"No match found. Create new entry.");
                });
            }
            OnPropertyChanged(nameof(ScannedBarcode));
            OnPropertyChanged(nameof(ProductName));
            OnPropertyChanged(nameof(SelectedCategory));
            OnPropertyChanged(nameof(ProductWeight));
            OnPropertyChanged(nameof(CurrentStockLevel));
            OnPropertyChanged(nameof(WholesalePrice));
            OnPropertyChanged(nameof(Price));
        }

        private async void LoadCategories()
        {
            var categories = await _databaseService.GetCategoriesAsync();
            Application.Current.Dispatcher.Dispatch(() =>
            {
                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category.CategoryName);
                }
            });
        }

        private async void LoadDefaultCategory()
        {
            var defaultCategory = await _databaseService.GetDefaultCategoryAsync();
            if (!string.IsNullOrEmpty(defaultCategory))
            {
                SelectedCategory = defaultCategory;
            }
        }

        #region Buttons
        public async Task AddStock()
        {
            #region Data validation
            // Check if fields are empty
            if (string.IsNullOrEmpty(ScannedBarcode) || string.IsNullOrEmpty(ProductName) || string.IsNullOrEmpty(SelectedCategory) || string.IsNullOrEmpty(ProductWeight))
            {
                App.NotificationService.Notify("Please fill in all fields before updating inventory.");
                return;
            }

            //Check if stock reduction reason string exists
            if (StockAdjustment < 0 && string.IsNullOrEmpty(StockReductionReason))
            {
                App.NotificationService.Notify("Please provide a reason for stock reduction.");
                return;
            }

            #endregion

            // If product exists in db
            if (SelectedProduct != null)
            {
                #region Data Validation
                // Check stock isnt going below 0
                if (SelectedProduct.CurrentStockLevel + StockAdjustment < 0)
                {
                    App.NotificationService.Notify("Stock level cannot go below zero.");
                    return;
                }

                //Check if details were changed
                if (IsCategoryChanged || IsWeightChanged || IsNameChanged || IsWholesalePriceChanged || IsSalePriceChanged)
                {
                    bool confirm = await Application.Current.MainPage.DisplayAlert(
                        "Confirm Changes",
                        "You have modified some product details. Are you sure you want to update?",
                        "Yes", "No");

                    if (!confirm) return; 
                }
                #endregion

                SelectedProduct.ProductName = ProductName;
                SelectedProduct.Category = SelectedCategory;
                SelectedProduct.ProductWeight = ProductWeight;
                SelectedProduct.CurrentStockLevel += StockAdjustment;
                SelectedProduct.WholesalePrice = WholesalePrice; 
                SelectedProduct.Price = Price; 

                await _databaseService.UpdateProductAsync(SelectedProduct, StockAdjustment);

                App.NotificationService.Notify("Inventory updated successfully.");
            }
            else // no existing product in db
            {
                var newProduct = new Product
                {
                    EAN13Barcode = ScannedBarcode,
                    ProductName = ProductName,
                    Category = SelectedCategory,
                    ProductWeight = ProductWeight,
                    CurrentStockLevel = StockAdjustment,
                    WholesalePrice = WholesalePrice, 
                    Price = Price 
                };

                await _databaseService.SaveProductAsync(newProduct);

                ProductAdded?.Invoke(newProduct);
                App.NotificationService.Notify("Product added successfully.");
            }
            
            ResetFields();
        }

        // Reset button
        public void ResetFields()
        {
            // String input fields
            SelectedProduct = null;
            ScannedBarcode = string.Empty;
            ProductName = string.Empty;
            SelectedCategory = string.Empty;
            ProductWeight = string.Empty;
            StockReductionReason = string.Empty;

            // int input fields
            CurrentStockLevel = 0;
            StockAdjustment = 0;
            WholesalePrice = 0; 
            Price = 0; 

            // Flags
            _initialProductName = string.Empty;
            _initialCategory = string.Empty;
            _initialProductWeight = string.Empty;
            _initialWholesalePrice = 0;
            _initialPrice = 0;
            IsStockReduction = false;

            // Notify property changed
            OnPropertyChanged(nameof(IsStockReduction));
            OnPropertyChanged(nameof(ScannedBarcode));
            OnPropertyChanged(nameof(ProductName));
            OnPropertyChanged(nameof(SelectedCategory));
            OnPropertyChanged(nameof(ProductWeight));
            OnPropertyChanged(nameof(CurrentStockLevel));
            OnPropertyChanged(nameof(StockAdjustment));
            OnPropertyChanged(nameof(WholesalePrice)); 
            OnPropertyChanged(nameof(Price));
        }

        // Toggle camera visibility
        public void ToggleCamera()
        {
            _isCameraOn = !_isCameraOn;
            OnPropertyChanged(nameof(IsCameraOn));
        }
        public bool IsCameraOn => _isCameraOn;

        #endregion
    }
}
