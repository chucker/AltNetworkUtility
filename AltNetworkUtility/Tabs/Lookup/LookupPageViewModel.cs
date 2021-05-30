using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

    public class DigMode
    {
        public string Name { get; }
        public string Description { get; }
        public string Argument { get; }

        public DigMode(string name, string description) :
            this(name, description, name)
        { }

        public DigMode(string name, string description, string argument)
        {
            if (Fields == null)
                Fields = typeof(DigMode).GetFields(BindingFlags.Static | BindingFlags.Public);

            Name = name;
            Description = description;

            Argument = argument;
        }

        public static readonly DigMode A = new("A", "A record (IPv4 host name)");
        public static readonly DigMode AAAA = new("AAAA", "AAAA record (IPv6 host name)");
        public static readonly DigMode MX = new("MX", "MX record (mail exchange)");
        public static readonly DigMode PTR = new("PTR", "PTR (reverse lookup)", "-x");

        private static FieldInfo[]? Fields;

        public static DigMode GetFromString(string s)
            => (Fields?.FirstOrDefault(f => f.Name == s)?.GetValue(null)) as DigMode ?? A;

        public static DigMode[] GetAll()
        {
            var fieldValues = new List<DigMode>();

            foreach (var item in Fields!)
                fieldValues.Add((DigMode)item.GetValue(null)!);

            return fieldValues.ToArray();
        }
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

                OnPropertyChanged(nameof(ShowDigOptions));

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

        public bool ShowDigOptions => LookupBinaryMode == LookupBinaryMode.Dig;

        public DigMode[] DigModes { get; }

        private DigMode _DigMode = DigMode.A;
        public DigMode DigMode
        {
            get => _DigMode;
            set
            {
                SetProperty(ref _DigMode, value);

                Preferences.Set(nameof(DigMode), value.Name);

                UpdateCommandLine();
            }
        }

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

                    arguments.Add(DigMode.Argument);

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

            if (Enum.TryParse<LookupBinaryMode>(Preferences.Get(nameof(BinaryMode), nameof(LookupBinaryMode.Dig)), out var lookupBinary))
                BinaryMode = lookupBinary;

            DigModes = DigMode.GetAll();
            DigMode = DigMode.GetFromString(Preferences.Get(nameof(DigMode), DigMode.A.Name));

            Host = Preferences.Get(nameof(Host), "");

            UseLookupBinaryCommand = new RelayCommand<string>(s =>
            {
                if (Enum.TryParse<LookupBinaryMode>(s, out var lookupBinary))
                    BinaryMode = lookupBinary;
            }, _ => true);
        }
    }
}
