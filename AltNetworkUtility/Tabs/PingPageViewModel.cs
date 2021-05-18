using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using AltNetworkUtility.Services;

using CliWrap;
using CliWrap.Builders;

using Microsoft.Toolkit.Mvvm.Input;

using Xamarin.Forms;

namespace AltNetworkUtility.Tabs
{
    public class PingPageViewModel : ViewModelBase
    {
        private const string PingBinary = "/sbin/ping";

        readonly Serilog.ILogger Log = Serilog.Log.ForContext<PingPageViewModel>();

        public string Arguments { get; set; } = "";

        public ICommand CancelCommand { get; }

        public CancellationTokenSource? CancellationTokenSource;

        private string _CommandLine = "";
        public string CommandLine
        {
            get => _CommandLine;
            set => SetProperty(ref _CommandLine, value);
        }

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

        public PreferencesService Preferences { get; }

        public IAsyncRelayCommand RunCommand { get; }

        private void UpdateCommandLine()
        {
            var arguments = new ArgumentsBuilder();

            arguments.Add(Host);
            
            Arguments = arguments.Build();

            CommandLine = $"{PingBinary} {Arguments}";
        }

        public PingPageViewModel()
        {
            Preferences = PreferencesService.GetInstance<PingPageViewModel>();

            RunCommand = new AsyncRelayCommand(Ping);
                // TODO MAYBE () => !string.IsNullOrWhiteSpace(Host));

            CancelCommand = new RelayCommand(Cancel);
        }

        public void Cancel()
        {
            CancellationTokenSource?.Cancel();
            IsBusy = false;
        }

        public async Task Ping()
        {
            IsBusy = true;
            CancellationTokenSource = new CancellationTokenSource();

            Output = "";

            await Cli.Wrap("/usr/bin/script")
                     .WithArguments(ab => ab.Add("-q")
                                            .Add("/dev/null")
                                            .Add(PingBinary)
                                            .Add(Arguments, false))
                     .WithStandardOutputPipe(PipeTarget.ToDelegate(async s =>
                     {
                         await Device.InvokeOnMainThreadAsync(() =>
                         {
                             // UGLY: need to filter out ^D from `script`, apparently
                             return Output += s.Replace("^D", "") + Environment.NewLine;
                         });
                     }))
                     .ExecuteAsync(CancellationTokenSource.Token);

            IsBusy = false;
        }
    }
}

