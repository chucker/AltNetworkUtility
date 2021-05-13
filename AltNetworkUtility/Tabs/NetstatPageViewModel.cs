using System;
using System.Threading.Tasks;

using CliWrap;
using CliWrap.Builders;

using Microsoft.Toolkit.Mvvm.Input;

using Xamarin.Forms;

namespace AltNetworkUtility.Tabs
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

        public string Arguments { get; set; } = "";

        private string _CommandLine = "";
        public string CommandLine
        {
            get => _CommandLine;
            set => SetProperty(ref _CommandLine, value);
        }

        private bool _DisableHostnameLookup;
        public bool DisableHostnameLookup
        {
            get => _DisableHostnameLookup;
            set
            {
                SetProperty(ref _DisableHostnameLookup, value);

                UpdateCommandLine();
            }
        }

        private bool _IsBusy;
        public bool IsBusy
        {
            get => _IsBusy;
            set
            {
                SetProperty(ref _IsBusy, value);
                OnPropertyChanged(nameof(IsNotBusy));
            }
        }

        public bool IsNotBusy => !IsBusy;

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

        public IAsyncRelayCommand NetstatCommand { get; }

        private NetstatMode NetstatMode
        {
            get
            {
                if (Mode is NetstatMode netstatMode)
                    return netstatMode;

                return 0;
            }
        }

        private string _Output = "";
        public string Output
        {
            get => _Output;
            set => SetProperty(ref _Output, value);
        }

        private void UpdateCommandLine()
        {
            switch (NetstatMode)
            {
                case NetstatMode.RoutingTable:
                    var arguments = new ArgumentsBuilder();

                    if (DisableHostnameLookup)
                        arguments.Add("-n");

                    arguments.Add("-r");

                    Arguments = arguments.Build();

                    break;
                default:
                    break;
            }

            CommandLine = $"{NetstatBinary} {Arguments}";
        }

        public NetstatPageViewModel()
        {
            NetstatCommand = new AsyncRelayCommand(
                Netstat,
                () => NetstatMode > 0);
        }

        public async Task Netstat()
        {
            IsBusy = true;

            Output = "";

            // we wrap this in 'script -q /dev/null' because it buffers too much:
            // https://github.com/Tyrrrz/CliWrap/discussions/113#discussioncomment-731047
            await Cli.Wrap("/usr/bin/script")
                     .WithArguments(ab => ab.Add("-q")
                                            .Add("/dev/null")
                                            .Add(NetstatBinary)
                                            .Add(Arguments, false))
                     .WithStandardOutputPipe(PipeTarget.ToDelegate(async s =>
                     {
                         await Device.InvokeOnMainThreadAsync(() =>
                         {
                             // UGLY: need to filter out ^D from `script`, apparently
                             return Output += s.Replace("^D", "") + Environment.NewLine;
                         });
                     }))
                     .ExecuteAsync();

            IsBusy = false;
        }
    }
}
