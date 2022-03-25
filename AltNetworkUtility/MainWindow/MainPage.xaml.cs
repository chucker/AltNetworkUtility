using System;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

using AltNetworkUtility.Services;
using AltNetworkUtility.Tabs.Info;

using Xamarin.Forms;

namespace AltNetworkUtility.MainWindow
{
    public partial class MainPage : TabbedPage
    {
        readonly Serilog.ILogger Log = Serilog.Log.ForContext<MainPage>();

        readonly PreferencesService Preferences =
            PreferencesService.GetInstance<MainPage>();
        private const string CurrentTabPagePreferencesKey = "CurrentTabPage";

        public MainPage()
        {
            InitializeComponent();

            HideQualityIfOsTooOld();
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

        void TabbedPage_PagesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Children.Count < _ExpectedTabs)
                return;

            RestoreSelectedTabFromPreferences();
        }

        private void RestoreSelectedTabFromPreferences()
        {
            var previouslySelectedTabPage = Preferences.Get(CurrentTabPagePreferencesKey, nameof(InfoPage));

            _TabHasBeenRestored = true;

            var matchingPage = Children.FirstOrDefault(p => p.GetType().Name == previouslySelectedTabPage);

            if (matchingPage != null)
                CurrentPage = matchingPage;
        }

        /// <summary>
        /// we need to require macOS 12, since `networkQuality` is new to that
        /// OSVersion returns the Darwin release. We could probably check for
        /// 21.0 instead of 21.4.
        /// </summary>
        private void HideQualityIfOsTooOld()
        {
            var darwinVersion = Environment.OSVersion.Version;

            if (darwinVersion < new Version(21, 4))
            {
                Log.Warning($"Network Quality is unavailable because the OS appears to be too old. Darwin version: {darwinVersion}");

                /*
                 * binding to Visible doesn't work as of XF 5.0.0.2337
                 */
                var qualityPage = Children.OfType<Tabs.Quality.QualityPage>().FirstOrDefault();
                if (qualityPage != null)
                    Children.Remove(qualityPage);
            }
        }
    }
}
