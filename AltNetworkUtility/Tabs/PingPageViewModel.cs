using AltNetworkUtility.Services;
using AltNetworkUtility.ViewModels;

using CliWrap.Builders;

namespace AltNetworkUtility.Tabs
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

                UpdateCommandLine();
            }
        }

        public PreferencesService Preferences { get; }

        private void UpdateCommandLine()
        {
            var arguments = new ArgumentsBuilder();

            arguments.Add(Host);

            DebufferedCommandViewModel.SetArguments(arguments);
        }

        public PingPageViewModel()
        {
            Preferences = PreferencesService.GetInstance<PingPageViewModel>();

            DebufferedCommandViewModel = new DebufferedCommandViewModel(PingBinary);

            // TODO MAYBE validate DebufferedCommandViewModel's Run command
            // e.g. !string.IsNullOrWhiteSpace(Host)
        }
    }
}

