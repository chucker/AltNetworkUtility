using System;
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

        public bool CanSkipHostnameLookup => NetstatMode switch
        {
            NetstatMode.RoutingTable => true,
            NetstatMode.SocketState => true,
            _ => false
        };

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
                NetstatMode netstatMode;

                if (value is NetstatMode)
                    netstatMode = (NetstatMode)value;
                else if (value is string strValue && Enum.TryParse<NetstatMode>(strValue, out var _netstatMode))
                    netstatMode = _netstatMode;
                else
                    netstatMode = NetstatMode.RoutingTable;

                SetProperty(ref _Mode, netstatMode);

                OnPropertyChanged(nameof(CanSkipHostnameLookup));

                Log.Debug($"Mode: {netstatMode}");

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
            var arguments = new ArgumentsBuilder();

            switch (NetstatMode)
            {
                case NetstatMode.RoutingTable:
                default:
                    if (DisableHostnameLookup)
                        arguments.Add("-n");

                    arguments.Add("-r");

                    break;
                case NetstatMode.NetworkStatistics:
                    arguments.Add("-s");

                    break;
                case NetstatMode.MulticastInformation:
                    arguments.Add("-g");
                    arguments.Add("-s");

                    break;
                case NetstatMode.SocketState:
                    if (DisableHostnameLookup)
                        arguments.Add("-n");

                    arguments.Add("-a");

                    break;
            }

            DebufferedCommandViewModel.SetArguments(arguments);
        }

        public NetstatPageViewModel()
        {
            DebufferedCommandViewModel = new DebufferedCommandViewModel(NetstatBinary);

            Preferences = PreferencesService.GetInstance<NetstatPageViewModel>();

            DisableHostnameLookup = Preferences.Get(nameof(DisableHostnameLookup), false);

            DebufferedCommandViewModel.RunCommandCanExecute =
                () => NetstatMode > 0;

            ToggleDisableHostnameLookupCommand = new RelayCommand(() =>
            {
                if (CanSkipHostnameLookup)
                    DisableHostnameLookup = !DisableHostnameLookup;
            });

            Mode = NetstatMode.RoutingTable;
        }
    }
}
