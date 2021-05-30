using System;
using System.Windows.Input;

using AltNetworkUtility.Services;
using AltNetworkUtility.ViewModels;

using CliWrap.Builders;

using Microsoft.Toolkit.Mvvm.Input;

namespace AltNetworkUtility.Tabs.Lookup
{
    public enum LookupBinaryMode
    {
        DsCacheUtil,
        Dig
    }

    public class LookupPageViewModel : ViewModelBase
    {
        private const string DigBinary = "/usr/bin/dig";
        private const string DsCacheUtilBinary = "/usr/bin/dscacheutil";

        readonly Serilog.ILogger Log = Serilog.Log.ForContext<LookupPageViewModel>();

        private object _BinaryMode = LookupBinaryMode.DsCacheUtil;
        public object BinaryMode
        {
            get => _BinaryMode;
            set
            {
                LookupBinaryMode LookupBinaryMode;

                if (value is LookupBinaryMode)
                    LookupBinaryMode = (LookupBinaryMode)value;
                else if (value is string strValue && Enum.TryParse<LookupBinaryMode>(strValue, out var _pingCountMode))
                    LookupBinaryMode = _pingCountMode;
                else
                    LookupBinaryMode = LookupBinaryMode.DsCacheUtil;

                Preferences.Set(nameof(BinaryMode), (int)LookupBinaryMode);

                SetProperty(ref _BinaryMode, LookupBinaryMode);

                Log.Debug($"Mode: {LookupBinaryMode}");

                UpdateCommandLine();
            }
        }

        private LookupBinaryMode LookupBinaryMode
        {
            get
            {
                if (BinaryMode is LookupBinaryMode mode)
                    return mode;

                return 0;
            }
        }

        public ICommand UseLookupBinaryCommand { get; }

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

        private void UpdateCommandLine()
        {
            var arguments = new ArgumentsBuilder();

            switch (LookupBinaryMode)
            {
                case LookupBinaryMode.Dig:
                    DebufferedCommandViewModel.Binary = DigBinary;

                    arguments.Add(Host);
                    break;
                case LookupBinaryMode.DsCacheUtil:
                default:
                    DebufferedCommandViewModel.Binary = DsCacheUtilBinary;

                    arguments.Add("-q");
                    arguments.Add("host");
                    arguments.Add("-a");
                    arguments.Add("name");
                    arguments.Add(Host);
                    break;
            }

            DebufferedCommandViewModel.SetArguments(arguments);
        }

        public LookupPageViewModel()
        {
            DebufferedCommandViewModel = new DebufferedCommandViewModel(DigBinary)
            {
                RunCommandCanExecute =
                () => !string.IsNullOrWhiteSpace(Host)
            };

            Preferences = PreferencesService.GetInstance<LookupPageViewModel>();

            Host = Preferences.Get(nameof(Host), "");

            UseLookupBinaryCommand = new RelayCommand<string>(s =>
            {
                if (Enum.TryParse<LookupBinaryMode>(s, out var lookupBinary))
                    BinaryMode = lookupBinary;
            }, _ => true);
        }
    }
}
