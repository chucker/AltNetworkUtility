using System;
using System.Threading.Tasks;

using CliWrap.Buffered;

using Microsoft.Toolkit.Mvvm.Input;

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
        private NetstatMode _Mode = NetstatMode.RoutingTable;
        public NetstatMode Mode
        {
            get => _Mode;
            set => SetProperty(ref _Mode, value);
        }

        private bool _What;
        public bool What
        {
            get => _What;
            set => SetProperty(ref _What, value);
        }

        private bool _The;
        public bool The
        {
            get => _The;
            set => SetProperty(ref _The, value);
        }

        private bool _Hell;
        public bool Hell
        {
            get => _Hell;
            set => SetProperty(ref _Hell, value);
        }

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
                () => Mode > 0);
        }

        public async Task InitAsync()
        {
            //throw new NotImplementedException();
        }

        public async Task Netstat()
        {
            var result = await CliWrap.Cli.Wrap("/usr/sbin/netstat")
                                      .WithArguments("-nr")
                                      .ExecuteBufferedAsync();
            Output = result.StandardOutput;
        }
    }
}
