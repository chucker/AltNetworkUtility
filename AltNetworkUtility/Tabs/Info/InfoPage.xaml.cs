using Rotorsoft.Forms;

using Xamarin.Forms;

namespace AltNetworkUtility.Tabs.Info
{
    public partial class InfoPage : ContentPage
    {
        public InfoPage()
        {
            InitializeComponent();
        }

        void ContentPage_BindingContextChanged(object sender, System.EventArgs e)
        {
            if (BindingContext is InfoPageViewModel viewModel)
            {
                viewModel.Init();
            }
        }
    }
}
