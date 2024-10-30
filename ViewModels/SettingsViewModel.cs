using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using INVApp.Services;
using INVApp.Models;

namespace INVApp.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;

        private Category _defaultCategory;
        public Category DefaultCategory
        {
            get => _defaultCategory;
            set
            {
                if (_defaultCategory != value)
                {
                    _defaultCategory = value;
                    OnPropertyChanged();
                }
            }
        }

        public SettingsViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Categories = new ObservableCollection<Category>();
            LoadCategoriesCommand = new Command(async () => await LoadCategories());
            AddCategoryCommand = new Command(async () => await AddCategory());
            RemoveCategoryCommand = new Command<Category>(async (category) => await RemoveCategory(category));
            SaveDefaultCategoryCommand = new Command(async () => await SaveDefaultCategory());

            SaveAudioSettingsCommand = new Command(async () => await SaveAudioSettings());
            LoadAudioSettingsCommand = new Command(async () => await LoadAudioSettings());

            Task.Run(async () => await LoadCategories());
            Task.Run(async () => await LoadDefaultCategory());
            Task.Run(async () => await LoadAudioSettings());
        }

        // Observable collection for categories
        public ObservableCollection<Category> Categories { get; }

        // New Category input
        private string _newCategory;
        public string NewCategory
        {
            get => _newCategory;
            set
            {
                _newCategory = value;
                OnPropertyChanged();
            }
        }

        // Audio settings properties
        private double _soundVolume;
        public double SoundVolume
        {
            get => _soundVolume;
            set
            {
                if (_soundVolume != value)
                {
                    _soundVolume = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isSoundEnabled;
        public bool IsSoundEnabled
        {
            get => _isSoundEnabled;
            set
            {
                if (_isSoundEnabled != value)
                {
                    _isSoundEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        // Commands
        public ICommand LoadCategoriesCommand { get; }
        public ICommand AddCategoryCommand { get; }
        public ICommand RemoveCategoryCommand { get; }
        public ICommand SaveDefaultCategoryCommand { get; }

        // New commands for audio settings
        public ICommand SaveAudioSettingsCommand { get; }
        public ICommand LoadAudioSettingsCommand { get; }

        // Load categories from database
        private async Task LoadCategories()
        {
            var categories = await _databaseService.GetCategoriesAsync();
            Categories.Clear();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
        }

        private async Task LoadDefaultCategory()
        {
            var categoryName = await _databaseService.GetDefaultCategoryAsync();
            DefaultCategory = Categories.FirstOrDefault(c => c.CategoryName == categoryName);
        }

        private async Task SaveDefaultCategory()
        {
            if (DefaultCategory != null)
            {
                await _databaseService.SaveDefaultCategoryAsync(DefaultCategory.CategoryName);
            }
        }

        // Load audio settings from database
        private async Task LoadAudioSettings()
        {
            var settings = await _databaseService.GetAudioSettingsAsync();
            SoundVolume = settings.Volume;
            IsSoundEnabled = settings.IsEnabled;
        }

        // Save audio settings to database
        private async Task SaveAudioSettings()
        {
            var settings = new AudioSettings
            {
                Volume = SoundVolume,
                IsEnabled = IsSoundEnabled
            };

            App.NotificationService.Notify("Audio settings saved!");
            await _databaseService.SaveAudioSettingsAsync(settings);
        }


        // Add a new category
        private async Task AddCategory()
        {
            if (string.IsNullOrWhiteSpace(NewCategory))
            {
                App.NotificationService.Notify("Category name cannot be empty.");
                return;
            }

            if (Categories.Any(c => string.Equals(c.CategoryName, NewCategory, StringComparison.OrdinalIgnoreCase)))
            {
                App.NotificationService.Notify("This category already exists.");
                return;
            }

            var category = new Category { CategoryName = NewCategory };
            await _databaseService.SaveCategoryAsync(category);

            Categories.Add(category);
            NewCategory = string.Empty;
        }

        // Remove a category
        private async Task RemoveCategory(Category category)
        {
            if (Categories.Contains(category))
            {
                App.NotificationService.Notify($"Category {category.CategoryName} removed.");

                Categories.Remove(category);
                await _databaseService.DeleteCategoryAsync(category);
            }
        }
    }
}
