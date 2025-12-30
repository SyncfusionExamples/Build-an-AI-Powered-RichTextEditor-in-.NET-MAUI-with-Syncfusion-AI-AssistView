namespace RichTextEditorAssistViewSample
{
    using Syncfusion.Maui.AIAssistView;
    using System.Threading.Tasks;

    public partial class MainPage : ContentPage
    {
        /// <summary>
        /// Represents the view model used to manage the state and behavior of the assist view.
        /// </summary>
        AssistViewViewModel assistViewViewModel;
        public MainPage()
        {
            InitializeComponent();
            assistViewViewModel = new AssistViewViewModel();
            this.BindingContext = assistViewViewModel;
        }

        /// <summary>
        /// Handles the click event for the button, performing platform-specific navigation or UI updates in response to
        /// user interaction.
        /// </summary>
        private async void Button_Clicked(object sender, EventArgs e)
        {
#if ANDROID || IOS
            // On Android, navigate to a dedicated page showing the AssistView
            var navParams = new Dictionary<string, object>
            {
                { "vm", assistViewViewModel }
            };
            await Shell.Current.GoToAsync(nameof(AssistViewPage), true, navParams);
#else
            // Keep existing behavior (Windows and other platforms)
            headerView.IsVisible = true;
            AssistView.IsVisible = true;
#endif
        }

        /// <summary>
        /// Handles the click event for the close button, hiding the header and assist views.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void close_Clicked(object sender, EventArgs e)
        {
            headerView.IsVisible = false;
            AssistView.IsVisible = false;
        }
    }
}
