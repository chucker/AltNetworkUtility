using Xamarin.Forms;

namespace AltNetworkUtility.Tabs.Traceroute
{
    public partial class TraceroutePage : ContentPage
    {
        public TraceroutePage()
        {
            InitializeComponent();
        }

        async void ContentPage_BindingContextChanged(System.Object sender, System.EventArgs e)
        {
            if (BindingContext is TraceroutePageViewModel viewModel)
                await viewModel.Init(Map);
        }
    }
}
