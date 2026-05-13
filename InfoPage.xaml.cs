using CommunityToolkit.Mvvm.ComponentModel;
using MetadataExtractor;

namespace NovelAI_GUI_Client;

public partial class InfoPage : ContentPage
{
    private partial class InfoDataSet : ObservableObject
    {
        [ObservableProperty]
        private ImageSource? image = null;

        [ObservableProperty]
        private string prompt = "";

        [ObservableProperty]
        private string negativePrompt = "";

        [ObservableProperty]
        private int steps;

        [ObservableProperty]
        private string sampler = "";

        [ObservableProperty]
        private decimal cfgScale;

        [ObservableProperty]
        private long seed;

        [ObservableProperty]
        private int width;

        [ObservableProperty]
        private int height;

        [ObservableProperty]
        private string modelHash = "";

        [ObservableProperty]
        private string model = "";

        [ObservableProperty]
        private int batchSize;

        [ObservableProperty]
        private int batchPos;

        [ObservableProperty]
        private string otherParameters = "";

        [ObservableProperty]
        private string parameters = "";

        [ObservableProperty]
        private decimal? aestheticScore;

        [ObservableProperty]
        private string hyperNetwork = "";

        [ObservableProperty]
        private decimal? hyperNetworkStrength;

        [ObservableProperty]
        private int? clipSkip;

        [ObservableProperty]
        private int? ensd;

        [ObservableProperty]
        private decimal? promptStrength;
    }

    private readonly InfoDataSet infoDataSet = new();

    public InfoPage(string imagePath, FileParameters fileParameters)
    {
        InitializeComponent();

        SetImageInfo(imagePath, fileParameters);

        BindingContext = infoDataSet;
    }

    private void SetImageInfo(string imagePath, FileParameters fileParameters)
    {
        infoDataSet.Image = ImageSource.FromFile(imagePath);

        infoDataSet.Prompt = fileParameters.Prompt;
        infoDataSet.NegativePrompt = fileParameters.NegativePrompt;
        infoDataSet.Steps = fileParameters.Steps;
        infoDataSet.Sampler = fileParameters.Sampler;
        infoDataSet.CfgScale = fileParameters.CFGScale;
        infoDataSet.Seed = fileParameters.Seed;
        infoDataSet.Width = fileParameters.Width;
        infoDataSet.Height = fileParameters.Height;
        infoDataSet.ModelHash = fileParameters.ModelHash;
        infoDataSet.Model = fileParameters.Model;
        infoDataSet.BatchSize = fileParameters.BatchSize;
        infoDataSet.BatchPos = fileParameters.BatchPos;
        infoDataSet.OtherParameters = fileParameters.OtherParameters;
        infoDataSet.Parameters = fileParameters.Parameters;
        infoDataSet.AestheticScore = fileParameters.AestheticScore;
        infoDataSet.HyperNetwork = fileParameters.HyperNetwork;
        infoDataSet.HyperNetworkStrength = fileParameters.HyperNetworkStrength;
        infoDataSet.ClipSkip = fileParameters.ClipSkip;
        infoDataSet.Ensd = fileParameters.ENSD;
        infoDataSet.PromptStrength = fileParameters.PromptStrength;
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
            using var stream = new FileStream(path, FileMode.Open);

            var fileParameters = await MetadataExtractor.Metadata.ReadFromStreamAsync(Path.GetExtension(path), stream);

            if (fileParameters is not null)
            {
                SetImageInfo(path, fileParameters);
            }
        }
    }
}