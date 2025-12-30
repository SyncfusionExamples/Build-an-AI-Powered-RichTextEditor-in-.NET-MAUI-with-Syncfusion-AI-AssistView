namespace RichTextEditorAssistViewSample;

[QueryProperty(nameof(ViewModel), "vm")]
public partial class AssistViewPage : ContentPage
{
	public AssistViewPage()
	{
		InitializeComponent();
	}

    /// <summary>
    /// Field to hold the ViewModel instance.
    /// </summary>
    private AssistViewViewModel? viewModel;

    /// <summary>
    /// Gets or sets the view model associated with the assist view.
    /// </summary>
    public AssistViewViewModel? ViewModel
    {
        get => viewModel;
        set
        {
            viewModel = value;
            BindingContext = viewModel;
        }
    }
}