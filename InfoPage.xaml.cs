using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
// using MetadataExtractor;

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

    // TODO: .NET 10 migration - MetadataExtractor API updated
    // FileParameters type not found in MetadataExtractor library
    // Update this after MetadataExtractor library migration is researched
    public InfoPage()
    {
        InitializeComponent();
        BindingContext = infoDataSet;
    }

    // STUB: FileParameters - migrate from MetadataExtractorCustom
    // private void SetImageInfo(string imagePath, FileParameters fileParameters)
    // {
    //     infoDataSet.Image = ImageSource.FromFile(imagePath);
    //     infoDataSet.Prompt = fileParameters.Prompt;
    //     // ... other properties
    // }

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
            // TODO: Implement image metadata extraction using MetadataExtractor library
            // For now, just load the image without metadata parsing
            try
            {
                infoDataSet.Image = ImageSource.FromFile(path);
            }
            catch (Exception ex)
            {
                // Log error - metadata extraction not yet implemented for .NET 10
                Debug.WriteLine($"Error loading image: {ex.Message}");
            }
        }
    }
}
