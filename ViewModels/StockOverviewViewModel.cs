using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using INVApp.Models;
using INVApp.Services;
using INVApp.Views;
using INVApp.ViewModels;
using System.Windows.Input;
using System.ComponentModel;

namespace INVApp.ViewModels
{
    public class StockOverviewViewModel
    {
        // Databse
        private readonly DatabaseService _databaseService;

        // Filters
        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
                SearchProducts();
            }
        }
        private string _selectedCategory;
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                SearchProducts();
            }
        }

        // Products and Categories
        public ObservableCollection<Product> Products { get; set; }
        public ObservableCollection<string> Categories { get; set; }

        // Commands
        public ICommand OpenProductDetailCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearFiltersCommand { get; }

        public StockOverviewViewModel(DatabaseService databaseService, StockIntakeViewModel intakeViewModel)
        {
            _databaseService = databaseService;
            Products = new ObservableCollection<Product>();
            Categories = new ObservableCollection<string>();

            OpenProductDetailCommand = new Command<Product>(OpenProductDetail);
            SearchCommand = new Command(SearchProducts);
            ClearFiltersCommand = new Command(ClearFilters);

            intakeViewModel.ProductAdded += OnProductAdded;

            LoadCategories();
            _ = LoadProducts();
        }

        // Products
        public async Task LoadProducts()
        {
            Products.Clear();

            var productsFromDb = await _databaseService.GetProductsAsync();

            foreach (var product in productsFromDb)
            {
                Products.Add(product);
            }
        }

        private async void OnProductAdded(Product newProduct)
        {
            await LoadProducts();
        }

        // Categories
        private async void LoadCategories()
        {
            var categoriesFromDb = await _databaseService.GetCategoriesAsync();
            Categories.Clear();

            foreach (var category in categoriesFromDb)
            {
                Categories.Add(category.CategoryName);
            }
        }

        private async void OpenProductDetail(Product selectedProduct)
        {
            var detailPage = new ProductDetailsPopup(selectedProduct, this);

            if (detailPage.BindingContext is ProductDetailViewModel viewModel)
            {
                viewModel.ProductUpdated += async () => await LoadProducts();
            }

            await Application.Current.MainPage.Navigation.PushModalAsync(detailPage);
        }

        // Filters
        public async void SearchProducts()
        {
            var productsFromDb = await _databaseService.GetProductsAsync();

            var filteredProducts = productsFromDb
                .Where(p =>
                    (string.IsNullOrEmpty(SearchQuery) || p.ProductName.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)) &&
                    (string.IsNullOrEmpty(SelectedCategory) || p.Category == SelectedCategory)
                );

            Products.Clear();
            foreach (var product in filteredProducts)
            {
                Products.Add(product);
            }
        }
        public void ClearFilters()
        {
            SearchQuery = string.Empty;
            SelectedCategory = null;
            SearchProducts();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
