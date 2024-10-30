using INVApp.ContentViews;
using INVApp.Services;
using INVApp.ViewModels;

namespace INVApp.Views;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();

        var databaseService = new DatabaseService(); 
        BindingContext = new SettingsViewModel(databaseService);

        App.NotificationService.OnNotify += message => NotificationBanner.Show(message);
    }
}