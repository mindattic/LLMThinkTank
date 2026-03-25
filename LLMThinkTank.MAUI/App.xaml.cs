namespace LLMThinkTank.MAUI
{
    /// <summary>
    /// MAUI application shell. Creates the main window hosting the Blazor WebView
    /// that renders the LLM Think Tank conversation interface.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>Initializes XAML-defined resources and components.</summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>Creates the application window with the main Blazor WebView page.</summary>
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "LLM Think Tank" };
        }
    }
}
