namespace NovelAI_GUI_Client
{
    public partial class App : Application
    {
        private readonly IHttpClientFactory httpClientFactory;

        public App(IHttpClientFactory httpClientFactory)
        {
            InitializeComponent();

            this.httpClientFactory = httpClientFactory;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new NavigationPage(new MainPage(httpClientFactory)));
        }
    }
}
