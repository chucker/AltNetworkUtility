using AltNetworkUtility.Services;
using AltNetworkUtility.ViewModels;

using CliWrap.Builders;

namespace AltNetworkUtility.Tabs.Ping
{
    public class PingPageViewModel : ViewModelBase
    {
        private const string PingBinary = "/sbin/ping";

        readonly Serilog.ILogger Log = Serilog.Log.ForContext<PingPageViewModel>();

        public DebufferedCommandViewModel DebufferedCommandViewModel { get; }

        private string _Host = "";
        public string Host
        {
            get => _Host;
            set
            {
                SetProperty(ref _Host, value);

                Preferences.Set(nameof(Host), value);

                UpdateCommandLine();
                DebufferedCommandViewModel.RefreshRunCommandCanExecute();
            }
        }

        public PreferencesService Preferences { get; }

        private int _SpecificCount;
        public int SpecificCount
        {
            get => _SpecificCount;
            set
            {
                SetProperty(ref _SpecificCount, value);

                Preferences.Set(nameof(SpecificCount), value);

                UpdateCommandLine();
            }
        }

        private bool _UseSpecificCount;
        public bool UseSpecificCount
        {
            get => _UseSpecificCount;
            set
            {
                // FIXME binding is broken
                SetProperty(ref _UseSpecificCount, value);

                Preferences.Set(nameof(UseSpecificCount), value);

                UpdateCommandLine();
            }
        }

        private void UpdateCommandLine()
        {
            var arguments = new ArgumentsBuilder();

            if (UseSpecificCount && SpecificCount > 0)
            {
                arguments.Add("-c");
                arguments.Add(SpecificCount);
            }

            arguments.Add(Host);

            DebufferedCommandViewModel.SetArguments(arguments);
        }

        public PingPageViewModel()
        {
            DebufferedCommandViewModel = new DebufferedCommandViewModel(PingBinary)
            {
                RunCommandCanExecute =
                () => !string.IsNullOrWhiteSpace(Host)
            };

            Preferences = PreferencesService.GetInstance<PingPageViewModel>();

            Host = Preferences.Get(nameof(Host), "");
            SpecificCount = Preferences.Get(nameof(SpecificCount), 10);
            UseSpecificCount = Preferences.Get(nameof(UseSpecificCount), false);
        }
    }
}

