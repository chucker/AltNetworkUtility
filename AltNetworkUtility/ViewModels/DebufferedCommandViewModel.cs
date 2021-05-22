using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using AltNetworkUtility.Services;

using CliWrap;
using CliWrap.Buffered;
using CliWrap.Builders;
using CliWrap.EventStream;

using Microsoft.Toolkit.Mvvm.Input;

using Xamarin.Forms;

namespace AltNetworkUtility.ViewModels
{
    public class DebufferedCommandViewModel : ViewModelBase
    {
        readonly Serilog.ILogger Log = Serilog.Log.ForContext<DebufferedCommandViewModel>();

        public string Arguments { get; set; } = "";

        public ICommand CancelCommand { get; }
        public ISystemSoundService SystemSoundService { get; }

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

        public int? ProcessId { get; private set; }

        public IAsyncRelayCommand RunCommand { get; }

        public Func<bool> RunCommandCanExecute { get; set; } = () => true;

        public void RefreshRunCommandCanExecute()
            => RunCommand.NotifyCanExecuteChanged();

        public void SetArguments(ArgumentsBuilder arguments)
        {
            Arguments = arguments.Build();

            CommandLine = $"{Binary} {Arguments}";
        }

        public DebufferedCommandViewModel(string binary)
        {
            Binary = binary;

            RunCommand = new AsyncRelayCommand(Run,
                () => RunCommandCanExecute?.Invoke() ?? true);

            CancelCommand = new AsyncRelayCommand(Cancel);

            SystemSoundService = DependencyService.Get<ISystemSoundService>();
        }

        /// <summary>
        /// Ping (and maybe others?) will print a summary after receiving
        /// SIGINT, so let's send that.
        /// </summary>
        private async Task SendSigIntToChild()
        {
            if (ProcessId == null)
            {
                Log.Warning("Don't have a PID, so we can't SIGINT");

                return;
            }

            /* 
             * we need the _child_ process ID. The one CliWrap gets is the one
             * for `script`. E.g.:
             * 
             * ~> pstree -p 1450
             * -+= 00001 root /sbin/launchd
             *  \-+= 01417 chucker /Users/chucker/Projects/AltNetworkUtility/AltNetworkUtility
             *    \-+- 01450 chucker /usr/bin/script -q /dev/null /sbin/ping ix.de
             *      \--= 01451 chucker /sbin/ping ix.de
             */

            var findChildCmd = await Cli.Wrap("/usr/bin/pgrep")
                                        .WithArguments(ab => ab.Add("-P")
                                                               .Add(ProcessId))
                                        .ExecuteBufferedAsync();

            if (!int.TryParse(findChildCmd.StandardOutput, out var childPid))
            {
                Log.Warning($"Couldn't find child PID for parent {ProcessId}, so we can't SIGINT");

                return;
            }

            await Cli.Wrap("/bin/kill")
                     .WithArguments(ab => ab.Add("-s")
                                            .Add("INT")
                                            .Add(childPid))
                     .ExecuteAsync();
        }

        public async Task Cancel()
        {
            await SendSigIntToChild();

            // give SigInt 3 seconds' worth of grace period

            await Task.Delay(3000);

            CancellationTokenSource?.Cancel();
            IsBusy = false;
        }

        public async Task Run()
        {
            IsBusy = true;
            CancellationTokenSource = new CancellationTokenSource();

            Output = "";

            PipeTarget outputPipe = PipeTarget.ToDelegate(async s =>
            {
                if (s.Contains("\a"))
                    await SystemSoundService.PlayAsync();

                await Device.InvokeOnMainThreadAsync(() =>
                {
                    // UGLY: need to filter out ^D from `script`, apparently
                    return Output += s.Replace("^D", "") + Environment.NewLine;
                });
            });

            var cmd = Cli.Wrap("/usr/bin/script")
                         .WithArguments(ab => ab.Add("-q")
                                                .Add("/dev/null")
                                                .Add(Binary)
                                                .Add(Arguments, false))
                         .WithStandardOutputPipe(outputPipe);

            await foreach (var cmdEvent in cmd.ListenAsync(CancellationTokenSource.Token))
            {
                switch (cmdEvent)
                {
                    case StartedCommandEvent started:
                        ProcessId = started.ProcessId;
                        Log.Debug($"Process started; ID: {started.ProcessId}");
                        break;
                    case StandardErrorCommandEvent stdErr:
                        Log.Debug($"Err> {stdErr.Text}");
                        break;
                }
            }

            IsBusy = false;
        }
    }
}

