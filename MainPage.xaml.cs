using CommunityToolkit.Mvvm.ComponentModel;
using System.IO.Compression;
using NovelAI_API;

namespace NovelAI_GUI_Client;

public partial class MainPage : ContentPage
{
    private readonly NovelAiApi novelAiApi;

    private partial class CanNavigateInfoPageCommand : System.Windows.Input.ICommand
    {
#pragma warning disable CS0067 // Event is not used in implementation
        public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

        public bool CanExecute(object? parameter)
        {
            return parameter is not null;
        }

        public void Execute(object? parameter)
        {
        }
    }

    private partial class DataSet : ObservableObject
    {
        [ObservableProperty]
        private string prompt = "";

        [ObservableProperty]
        private string negativePrompt = "";

        [ObservableProperty]
        private int generateImageWidth;

        [ObservableProperty]
        private int generateImageHeight;

        [ObservableProperty]
        private int generateImageResolutionIndex;

        [ObservableProperty]
        private ImageSource? generatedImageSource = null;

        [ObservableProperty]
        private string responseMessage = "";

        [ObservableProperty]
        private InfoPage? currentInfoPage = null;

        public string[] ImageResolutionTypes { get; set; } =
        [
            .. Enum.GetValues<NovelAiApi.ImageResolutionType>().Cast<NovelAiApi.ImageResolutionType>().Select(type => $"{type} {NovelAiApi.GetImageResolutionPixel(type)}"),
            "Custom",
        ];

        private CanNavigateInfoPageCommand CanNavigateInfoPageCommand { get; set; } = new();
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
    private static bool IsFirstLoaded = true;

    private (int width, int height)[] ImageResolutionTable { get; } = [.. Enum.GetValues<NovelAiApi.ImageResolutionType>().Cast<NovelAiApi.ImageResolutionType>().Select(type => NovelAiApi.GetImageResolutionPixel(type))];


    public MainPage(IHttpClientFactory httpClientFactory)
    {
        InitializeComponent();

        BindingContext = dataSet;

        optionDataSet.LoadOption();

        novelAiApi = new NovelAiApi(httpClientFactory);
    }

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        if (IsFirstLoaded && string.IsNullOrEmpty(optionDataSet.ApiKey))
        {
            var result = await DisplayAlertAsync("Api Key", "set Api Key.", "OK", "Cancel");

            if (result)
            {
                await Navigation.PushAsync(new OptionPage(optionDataSet));
            }
        }
        else
        {
            novelAiApi.SetApiKey(optionDataSet.ApiKey);
        }

        IsFirstLoaded = false;
    }

    private async void Button_Clicked_Option(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new OptionPage(optionDataSet));
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

        dataSet.ResponseMessage = httpResponseMessage.ToString();
    }

    private async void DropGestureRecognizer_Drop(object sender, DropEventArgs e)
    {
        var path = string.Empty;

#if WINDOWS
        var items = await e.PlatformArgs!.DragEventArgs.DataView.GetStorageItemsAsync();

        if (items.Count > 0)
        {
            var item = items[0];

            path = item.Path;
        }
#else
        await Task.FromResult(0);
#endif

        if ((!string.IsNullOrEmpty(path)) && (File.Exists(path)))
        {
            // TODO: Update MetadataExtractor integration for .NET 10
            // Metadata.ReadFromStreamAsync is not available in updated library
            // using var stream = new FileStream(path, FileMode.Open);
            // var fileParameters = await MetadataExtractor.Metadata.ReadFromStreamAsync(Path.GetExtension(path), stream);
            // if (fileParameters is not null)
            // {
            //     dataSet.CurrentInfoPage = new InfoPage(path, fileParameters);
            // }

            // Temporary solution - open InfoPage without metadata
            dataSet.CurrentInfoPage = new InfoPage();

            await Navigation.PushAsync(dataSet.CurrentInfoPage);
        }
    }

    private async void ToolbarItem_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(dataSet.CurrentInfoPage);
    }

    private void Picker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (sender is not Picker picker)
        {
            return;
        }

        int index = picker.SelectedIndex;

        if (index < ImageResolutionTable.Length)
        {
            (var width, var height) = ImageResolutionTable[index];

            dataSet.GenerateImageWidth = width;
            dataSet.GenerateImageHeight = height;
        }
    }

    private void Entry_TextChanged(object sender, TextChangedEventArgs e)
    {
        var resolution = ImageResolutionTable.SingleOrDefault(resolution => (resolution.width == dataSet.GenerateImageWidth) && (resolution.height == dataSet.GenerateImageHeight));

        if (resolution is (0, 0))
        {
            dataSet.GenerateImageResolutionIndex = ImageResolutionTable.Length;
        }
        else
        {
            dataSet.GenerateImageResolutionIndex = Array.IndexOf(ImageResolutionTable, resolution);
        }
    }

    private void Entry_TextChanged_1(object sender, TextChangedEventArgs e)
    {

    }
}
