using System.Linq;
using System.Reflection;

using AltNetworkUtility.Services;
using AltNetworkUtility.Tabs.Info;

using Xamarin.Forms;

namespace AltNetworkUtility
{
    public partial class MainPage : TabbedPage
    {
        readonly PreferencesService Preferences =
            PreferencesService.GetInstance<MainPage>();
        private const string CurrentTabPagePreferencesKey = "CurrentTabPage";

        public MainPage()
        {
            InitializeComponent();
        }

        /*
         * binding to TabbedPage.SelectedItem or CurrentPage doesn't work as of
         * XF 5.0.0.2012
         */

        void TabbedPage_CurrentPageChanged(object sender, System.EventArgs e)
        {
            if (!_TabHasBeenRestored)
                return;

            var type = CurrentPage?.GetType().Name;

            if (type != null)
                Preferences.Set(CurrentTabPagePreferencesKey, type);
        }

        bool _TabHasBeenRestored = false;
        int _ExpectedTabs = Assembly.GetExecutingAssembly().GetTypes()
                                    .Count(t => t.Namespace?.Contains("Tabs") == true &&
                                                t.IsSubclassOf(typeof(Page)));

        void TabbedPage_PagesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Children.Count < _ExpectedTabs)
                return;

            var previouslySelectedTabPage = Preferences.Get(CurrentTabPagePreferencesKey, nameof(InfoPage));

            _TabHasBeenRestored = true;

            var matchingPage = Children.FirstOrDefault(p => p.GetType().Name == previouslySelectedTabPage);

            if (matchingPage != null)
                CurrentPage = matchingPage;
        }
    }
}
