namespace NovelAI_GUI_Client
{
    public partial class App : Application
    {
        public App(IHttpClientFactory httpClientFactory)
        {
            InitializeComponent();

            MainPage = new NavigationPage(new MainPage(httpClientFactory));
        }
    }
}
