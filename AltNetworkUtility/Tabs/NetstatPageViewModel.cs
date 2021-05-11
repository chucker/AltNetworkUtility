using System;
using System.Threading.Tasks;

using CliWrap;

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
        readonly Serilog.ILogger Log = Serilog.Log.ForContext<NetstatPageViewModel>();

        // WORKAROUND for https://github.com/chucker/AltNetworkUtility/issues/10
        private object _Mode = NetstatMode.RoutingTable;
        public object Mode
        {
            get => _Mode;
            set
            {
                SetProperty(ref _Mode, value);

                Log.Debug($"Mode: {value}");
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

        private string _Output = "";
        public string Output
        {
            get => _Output;
            set => SetProperty(ref _Output, value);
        }

        public IAsyncRelayCommand NetstatCommand { get; }

        public NetstatPageViewModel()
        {
            NetstatCommand = new AsyncRelayCommand(
                Netstat,
                () => NetstatMode > 0);
        }

        public async Task InitAsync()
        {
            //throw new NotImplementedException();
        }

        public async Task Netstat()
        {
            IsBusy = true;

            Output = "";

            await Cli.Wrap("/usr/sbin/netstat")
                     .WithArguments("-r")
                     .WithStandardOutputPipe(PipeTarget.ToDelegate(async s =>
                     {
                         await Device.InvokeOnMainThreadAsync(() => Output += s + Environment.NewLine);
                     }))
                     .ExecuteAsync();

            IsBusy = false;
        }
    }
}
