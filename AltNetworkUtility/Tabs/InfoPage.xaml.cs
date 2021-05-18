using Rotorsoft.Forms;

using Xamarin.Forms;

namespace AltNetworkUtility.Tabs
{
    public partial class InfoPage : ContentPage
    {
        public InfoPage()
        {
            InitializeComponent();
        }

        void ContentPage_BindingContextChanged(object sender, System.EventArgs e)
        {
            if (BindingContext is InfoPageViewModel viewModel &&
                Resources.TryGetValue("NetworkInterfaces", out var resource) &&
                resource is CollectionViewSource collectionViewSource)
            {
                viewModel.Init(collectionViewSource.View);
            }
        }
    }
}
