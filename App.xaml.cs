
using INVApp.Services;

namespace INVApp
{
    public partial class App : Application
    {
        public static DatabaseService DatabaseService { get; private set; }
        public static NotificationService NotificationService { get; private set; }

        public App()
        {
            InitializeComponent();

            DatabaseService = new DatabaseService();
            NotificationService = new NotificationService();
            MainPage = new AppShell();
        }
    }
}
