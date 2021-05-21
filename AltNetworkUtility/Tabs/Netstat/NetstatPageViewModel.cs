using System.Windows.Input;

using AltNetworkUtility.Services;
using AltNetworkUtility.ViewModels;

using CliWrap.Builders;

using Microsoft.Toolkit.Mvvm.Input;

namespace AltNetworkUtility.Tabs.Netstat
{
    public enum NetstatMode
    {
        _ = 0,
        RoutingTable,
        NetworkStatistics,
        MulticastInformation,
        SocketState
    }

    public class NetstatPageViewModel : ViewModelBase
    {
        private const string NetstatBinary = "/usr/sbin/netstat";

        readonly Serilog.ILogger Log = Serilog.Log.ForContext<NetstatPageViewModel>();

        public DebufferedCommandViewModel DebufferedCommandViewModel { get; }

        private bool _DisableHostnameLookup;
        public bool DisableHostnameLookup
        {
            get => _DisableHostnameLookup;
            set
            {
                SetProperty(ref _DisableHostnameLookup, value);

                Preferences.Set(nameof(DisableHostnameLookup), value);

                UpdateCommandLine();
            }
        }

        // WORKAROUND for https://github.com/chucker/AltNetworkUtility/issues/10
        private object _Mode = NetstatMode.RoutingTable;
        public object Mode
        {
            get => _Mode;
            set
            {
                if (value is not NetstatMode netstatMode)
                    value = NetstatMode.RoutingTable;

                SetProperty(ref _Mode, value);

                Log.Debug($"Mode: {value}");

                UpdateCommandLine();
            }
        }

        private NetstatMode NetstatMode
        {
            get
            {
                if (Mode is NetstatMode netstatMode)
                    return netstatMode;

                return 0;
            }
        }

        public PreferencesService Preferences { get; }

        public ICommand ToggleDisableHostnameLookupCommand { get; }

        private void UpdateCommandLine()
        {
            switch (NetstatMode)
            {
                case NetstatMode.RoutingTable:
                    var arguments = new ArgumentsBuilder();

                    if (DisableHostnameLookup)
                        arguments.Add("-n");

                    arguments.Add("-r");

                    DebufferedCommandViewModel.SetArguments(arguments);

                    break;
                default:
                    break;
            }
        }

        public NetstatPageViewModel()
        {
            DebufferedCommandViewModel = new DebufferedCommandViewModel(NetstatBinary);

            Preferences = PreferencesService.GetInstance<NetstatPageViewModel>();

            DisableHostnameLookup = Preferences.Get(nameof(DisableHostnameLookup), false);

            DebufferedCommandViewModel.RunCommandCanExecute =
                () => NetstatMode > 0;

            ToggleDisableHostnameLookupCommand = new RelayCommand(() =>
                DisableHostnameLookup = !DisableHostnameLookup);
        }
    }
}
