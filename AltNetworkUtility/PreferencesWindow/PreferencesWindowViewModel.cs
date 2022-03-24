using System;

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
        readonly Serilog.ILogger Log = Serilog.Log.ForContext<PreferencesWindowViewModel>();

        private object _AppendOutputMode = AppendOutputMode.Clear;
        public object AppendOutputMode_O
        {
            get => _AppendOutputMode;
            set
            {
                AppendOutputMode appendOutputMode;

                if (value is AppendOutputMode)
                    appendOutputMode = (AppendOutputMode)value;
                else if (value is string strValue && Enum.TryParse<AppendOutputMode>(strValue, out var _appendOutputMode))
                    appendOutputMode = _appendOutputMode;
                else
                    appendOutputMode = AppendOutputMode.Clear;

                Preferences.Set(nameof(AppendOutputMode), (int)appendOutputMode);

                SetProperty(ref _AppendOutputMode, appendOutputMode);

                Log.Debug($"Mode: {appendOutputMode}");
            }
        }
        public AppendOutputMode AppendOutputMode
        {
            get
            {
                if (AppendOutputMode_O is AppendOutputMode mode)
                    return mode;

                return 0;
            }
        }

        public PreferencesService Preferences { get; }

        public PreferencesWindowViewModel()
        {
            Preferences = PreferencesService.GetInstance<PreferencesWindowViewModel>();

            AppendOutputMode_O = Preferences.Get(nameof(AppendOutputMode), (int)AppendOutputMode.Clear);
        }
    }
}
