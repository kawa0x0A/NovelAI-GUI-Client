using CommunityToolkit.Maui.Storage;
using NovelAI_API;

namespace NovelAI_GUI_Client;

public partial class OptionPage : ContentPage
{
	private readonly MainPage.OptionDataSet optionDataSet;

	public OptionPage(MainPage.OptionDataSet optionDataSet)
	{
		InitializeComponent();

		this.optionDataSet = optionDataSet;

		BindingContext = optionDataSet;
	}

    private void ContentPage_Unloaded(object sender, EventArgs e)
    {
		optionDataSet.SaveOption();
    }

    private async void Button_Clicked(object sender, EventArgs e)
    {
		var result = await FolderPicker.Default.PickAsync();

		if (result.IsSuccessful)
		{
			optionDataSet.OutputPath = result.Folder.Path;
		}
    }
}