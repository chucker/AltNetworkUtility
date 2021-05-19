using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using CliWrap;
using CliWrap.Builders;

using Microsoft.Toolkit.Mvvm.Input;

using Xamarin.Forms;

namespace AltNetworkUtility.ViewModels
{
    public class DebufferedCommandViewModel : ViewModelBase
    {
        readonly Serilog.ILogger Log = Serilog.Log.ForContext<DebufferedCommandViewModel>();

        public string Arguments { get; set; } = "";

        public ICommand CancelCommand { get; }

        public CancellationTokenSource? CancellationTokenSource;

        public string Binary { get; }

        private string _CommandLine = "";
        public string CommandLine
        {
            get => _CommandLine;
            set => SetProperty(ref _CommandLine, value);
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
        public IAsyncRelayCommand RunCommand { get; }

        public void SetArguments(ArgumentsBuilder arguments)
        {
            Arguments = arguments.Build();

            CommandLine = $"{Binary} {Arguments}";
        }

        public DebufferedCommandViewModel(string binary)
        {
            Binary = binary;

            RunCommand = new AsyncRelayCommand(Run);
            // TODO MAYBE () => !string.IsNullOrWhiteSpace(Host));

            CancelCommand = new RelayCommand(Cancel);
        }

        public void Cancel()
        {
            CancellationTokenSource?.Cancel();
            IsBusy = false;
        }

        public async Task Run()
        {
            IsBusy = true;
            CancellationTokenSource = new CancellationTokenSource();

            Output = "";

            await Cli.Wrap("/usr/bin/script")
                     .WithArguments(ab => ab.Add("-q")
                                            .Add("/dev/null")
                                            .Add(Binary)
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

