using AltNetworkUtility.Services;

namespace AltNetworkUtility.PreferencesWindow
{
    public enum AppendOutputMode
    {
        Clear,
        Append
    }

    public class PreferencesWindowViewModel : ViewModelBase
    {
        private AppendOutputMode _AppendOutputMode;
        public AppendOutputMode AppendOutputMode
        {
            get => _AppendOutputMode;
            set
            {
                SetProperty(ref _AppendOutputMode, value);

                Preferences.Set("AppendOutput", value == AppendOutputMode.Append);
            }
        }

        public PreferencesService Preferences { get; }

        public PreferencesWindowViewModel()
        {
            Preferences = PreferencesService.GetInstance<PreferencesWindowViewModel>();

            AppendOutputMode = Preferences.GetEnum(nameof(AppendOutputMode), AppendOutputMode.Clear);
        }
    }
}
