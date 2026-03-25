namespace LLMThinkTank.MAUI
{
    /// <summary>
    /// Root content page hosting the <c>BlazorWebView</c> that renders the Razor-based
    /// conversation UI. The XAML counterpart defines the WebView configuration and host page.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        /// <summary>Initializes the Blazor WebView host page from XAML.</summary>
        public MainPage()
        {
            InitializeComponent();
        }
    }
}
