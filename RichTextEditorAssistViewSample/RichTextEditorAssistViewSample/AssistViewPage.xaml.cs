namespace RichTextEditorAssistViewSample;

[QueryProperty(nameof(ViewModel), "vm")]
public partial class AssistViewPage : ContentPage
{
    #region Field

    /// <summary>
    /// Field to hold the ViewModel instance.
    /// </summary>
    private AssistViewViewModel? viewModel;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the AssistViewPage class.
    /// </summary>
    public AssistViewPage()
    {
        InitializeComponent();
    }

    #endregion

    #region Properties

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

    #endregion
}