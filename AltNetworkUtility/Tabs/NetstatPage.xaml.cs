using Xamarin.Forms;

namespace AltNetworkUtility.Tabs
{
    public partial class NetstatPage : ContentPage
    {
        public NetstatPage()
        {
            InitializeComponent();
        }

        async void ContentPage_BindingContextChanged(object sender, System.EventArgs e)
        {
            if (BindingContext is NetstatPageViewModel viewModel)
                await viewModel.InitAsync();
        }
    }
}
