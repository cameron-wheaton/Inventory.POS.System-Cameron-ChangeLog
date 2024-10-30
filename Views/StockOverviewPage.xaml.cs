using INVApp.ViewModels;
using INVApp.Services;

namespace INVApp.Views;

public partial class StockOverviewPage : ContentPage
{
	public StockOverviewPage(StockOverviewViewModel viewModel)
	{
		InitializeComponent();

        //var databaseService = new DatabaseService();
        //var stockIntakeViewModel = new StockIntakeViewModel(databaseService);
        //var stockOverviewViewModel = new StockOverviewViewModel(databaseService, stockIntakeViewModel);

        BindingContext = viewModel;
    }

    
}