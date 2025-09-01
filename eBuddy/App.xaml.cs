namespace eBuddy
{
    public partial class App : Application
    {
        public static ServiceDatabase Database { get; private set; }

        public App()
        {
            InitializeComponent();

            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "services.db3");
            Database = new ServiceDatabase(dbPath);
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}