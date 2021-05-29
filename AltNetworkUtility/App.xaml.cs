using System.IO;
using System.Reflection;
using System.Threading;

using Xamarin.Forms;
using Xamarin.Forms.StyleSheets;

namespace AltNetworkUtility
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();

#if Hacky_StyleSheet_HotReload
            new Timer(_ =>
            {
                base.Dispatcher.BeginInvokeOnMainThread(() =>
                {
                    var prop = Resources.GetType().GetProperty("StyleSheets", BindingFlags.NonPublic | BindingFlags.Instance);
                    prop!.SetValue(Resources, null);

                    const string StyleSheetPath = "../../../../../../AltNetworkUtility/AppStylesheet.css";

                    using var sr = new StreamReader(StyleSheetPath);
                    Resources.Add(StyleSheet.FromReader(sr));
                });
            }, null, 1_000, 1_000);
#endif
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
