using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

using AltNetworkUtility.Services;
using AltNetworkUtility.ViewModels;

using CliWrap.Builders;

using Microsoft.Toolkit.Mvvm.Input;

namespace AltNetworkUtility.Tabs.Ping
{
    public enum PingCountMode
    {
        Unlimited,
        Specific
    }

    public class PingPageViewModel : ViewModelBase
    {
        private const string PingBinary = "/sbin/ping";

        readonly Serilog.ILogger Log = Serilog.Log.ForContext<PingPageViewModel>();

        public ObservableCollection<NetworkInterfaceViewModel> AvailableNetworkInterfaces { get; } = new();

        // WORKAROUND for https://github.com/chucker/AltNetworkUtility/issues/10
        private object _CountMode = PingCountMode.Unlimited;
        public object CountMode
        {
            get => _CountMode;
            set
            {
                PingCountMode pingCountMode;

                if (value is PingCountMode)
                    pingCountMode = (PingCountMode)value;
                else if (value is string strValue && Enum.TryParse<PingCountMode>(strValue, out var _pingCountMode))
                    pingCountMode = _pingCountMode;
                else
                    pingCountMode = PingCountMode.Unlimited;

                Preferences.Set(nameof(UseSpecificCount),
                                pingCountMode == PingCountMode.Specific);

                SetProperty(ref _CountMode, pingCountMode);

                Log.Debug($"Mode: {pingCountMode}");

                UpdateCommandLine();
            }
        }

        private PingCountMode PingCountMode
        {
            get
            {
                if (CountMode is PingCountMode netstatMode)
                    return netstatMode;

                return 0;
            }
        }

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

        public bool UseSpecificCount => PingCountMode == PingCountMode.Specific;

        private bool _UseSpecificInterface = false;
        public bool UseSpecificInterface
        {
            get => _UseSpecificInterface;
            set
            {
                SetProperty(ref _UseSpecificInterface, value);

                Preferences.Set(nameof(UseSpecificInterface), value);
            }
        }

        public ICommand ToggleUseSpecificInterfaceCommand { get; }

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

            CountMode = Preferences.Get(nameof(UseSpecificCount), false) ? PingCountMode.Specific : PingCountMode.Unlimited;
            SpecificCount = Preferences.Get(nameof(SpecificCount), 10);

            UseSpecificInterface = Preferences.Get(nameof(UseSpecificInterface), false);

            ToggleUseSpecificInterfaceCommand = new RelayCommand(() =>
                UseSpecificInterface = !UseSpecificInterface);

            // FIXME: this should be a repository. we shouldn't fetch this stuff twice.
            var svc = Xamarin.Forms.DependencyService.Get<INetworkInterfacesService>();

            foreach (var item in svc.GetAvailableInterfaces())
            {
                AvailableNetworkInterfaces.Add(item);
            }
        }
    }
}

