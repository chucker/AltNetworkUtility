using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using AltNetworkUtility.Repositories.NetworkInterfaceRepository;
using AltNetworkUtility.Services;
using AltNetworkUtility.ViewModels;

using CliWrap.Builders;

using Microsoft.Toolkit.Mvvm.Input;

namespace AltNetworkUtility.Tabs.Ping
{
    public enum PingAudibleMode
    {
        Never = 0,
        OnSuccess,
        OnFailure
    }

    public enum PingCountMode
    {
        Unlimited,
        Specific
    }

    public class PingPageViewModel : ViewModelBase
    {
        private const string PingBinary = "/sbin/ping";

        readonly Serilog.ILogger Log = Serilog.Log.ForContext<PingPageViewModel>();

        public ObservableCollection<NetworkInterfaceViewModel> AvailableNetworkInterfaces { get; }

        private object _AudibleMode = PingAudibleMode.Never;
        public object AudibleMode
        {
            get => _AudibleMode;
            set
            {
                PingAudibleMode pingAudibleMode;

                if (value is PingAudibleMode)
                    pingAudibleMode = (PingAudibleMode)value;
                else if (value is string strValue && Enum.TryParse<PingAudibleMode>(strValue, out var _pingCountMode))
                    pingAudibleMode = _pingCountMode;
                else
                    pingAudibleMode = PingAudibleMode.Never;

                Preferences.Set(nameof(AudibleMode), (int)pingAudibleMode);

                SetProperty(ref _AudibleMode, pingAudibleMode);

                Log.Debug($"Mode: {pingAudibleMode}");

                UpdateCommandLine();
            }
        }

        private PingAudibleMode PingAudibleMode
        {
            get
            {
                if (AudibleMode is PingAudibleMode mode)
                    return mode;

                return 0;
            }
        }

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
                if (CountMode is PingCountMode mode)
                    return mode;

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

                UpdateCommandLine();
            }
        }

        private NetworkInterfaceViewModel? _SpecificInterface;
        public NetworkInterfaceViewModel? SpecificInterface
        {
            get => _SpecificInterface;
            set
            {
                SetProperty(ref _SpecificInterface, value);

                if (value?.BsdName != null)
                    Preferences.Set(nameof(SpecificInterface), value.BsdName);
                else
                    Preferences.Remove(nameof(SpecificInterface));

                UpdateCommandLine();
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

            switch (PingAudibleMode)
            {
                case PingAudibleMode.OnSuccess:
                    arguments.Add("-a");
                    break;
                case PingAudibleMode.OnFailure:
                    arguments.Add("-A");
                    break;
                case PingAudibleMode.Never:
                default:
                    break;
            }

            static bool isIPv4(System.Net.IPAddress ad) =>
                ad.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;

            if (UseSpecificInterface &&
                !string.IsNullOrEmpty(SpecificInterface?.BsdName) &&
                SpecificInterface?.IPAddresses.Any(isIPv4) == true)
            {
                arguments.Add("-S");
                arguments.Add(SpecificInterface.IPAddresses.First(isIPv4).ToString());
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

            var repo = Xamarin.Forms.DependencyService.Get<NetworkInterfaceRepository>();
            AvailableNetworkInterfaces = repo.AsObservable;

            //var specificInterface = Preferences.Get(nameof(SpecificInterface), "");
            //if (repo.TryFindInterfaceByName(specificInterface, out var netIf))
            //    SpecificInterface = netIf;

            AudibleMode = (PingAudibleMode)Preferences.Get(nameof(AudibleMode), 0);
        }
    }
}

