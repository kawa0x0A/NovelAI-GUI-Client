using CommunityToolkit.Mvvm.ComponentModel;
using System.IO.Compression;
using NovelAI_API;

namespace NovelAI_GUI_Client
{
    public partial class MainPage : ContentPage
    {
        private readonly NovelAiApi novelAiApi;

        private partial class DataSet : ObservableObject
        {
            [ObservableProperty]
            private string prompt = "";

            [ObservableProperty]
            private string negativePrompt = "";

            [ObservableProperty]
            private ImageSource? generatedImageSource = null;
        }

        public partial class OptionDataSet : ObservableObject
        {
            private const string ApiKeyPreferenceName = "ApiKey";
            private const string OutputPathPreferenceName = "OutputPath";

            [ObservableProperty]
            private string apiKey = "";

            [ObservableProperty]
            private string outputPath = "";

            public void LoadOption()
            {
                ApiKey = Preferences.Get(ApiKeyPreferenceName, string.Empty);
                OutputPath = Preferences.Get(OutputPathPreferenceName, Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            }

            public void SaveOption()
            {
                Preferences.Set(ApiKeyPreferenceName, ApiKey);
                Preferences.Set(OutputPathPreferenceName, OutputPath);
            }
        }

        private readonly DataSet dataSet = new();
        private readonly OptionDataSet optionDataSet = new();

        public MainPage(IHttpClientFactory httpClientFactory)
        {
            InitializeComponent();

            BindingContext = dataSet;

            optionDataSet.LoadOption();

            novelAiApi = new NovelAiApi(httpClientFactory);

            novelAiApi.SetApiKey(optionDataSet.ApiKey);
        }

        private async void Button_Clicked_Option(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new OptionPage(novelAiApi, optionDataSet));
        }

        private async void Button_Clicked_GenerateImage(object sender, EventArgs e)
        {
            var parameters = new NovelAiApi.ImageGenerateParameters()
            {
                Width = 1024,
                Height = 1024,
                SamplerType = NovelAiApi.SamplerType.k_euler_ancestral,
                Scale = 5.0f,
                Step = 28,
                Seed = NovelAiApi.GetRandomSeed(),
                IsEnableAddQualityPrompt = true,
                NegativePromptPreset = NovelAiApi.NegativePromptPresetType.Heavy,
            };

            var httpResponseMessage = await novelAiApi.GenerateImageAsync(NovelAiApi.ImageModelType.AnimeV3, dataSet.Prompt, dataSet.NegativePrompt, parameters);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var path = Path.Combine(optionDataSet.OutputPath, $"{DateTime.Now:yyyyMMddHHmmss}_{parameters.Seed}.png");

                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    var stream = await httpResponseMessage.Content.ReadAsStreamAsync();

                    var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read);

                    var zipArchiveEntry = zipArchive.Entries.First();

                    var zipArchiveEntryStream = zipArchiveEntry.Open();

                    zipArchiveEntryStream.CopyTo(fileStream);
                }

                dataSet.GeneratedImageSource = ImageSource.FromFile(path);
            }
        }
    }
}
