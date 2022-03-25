using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using AltNetworkUtility.Repositories.NetworkInterfaceRepository;
using AltNetworkUtility.ViewModels;

using CliWrap.Builders;

using Microsoft.Toolkit.Mvvm.Input;

namespace AltNetworkUtility.MainWindow.Tabs.Quality
{
        public enum SequentialMode
        {
            InParallel,
            Sequentially
        }

    public class QualityPageViewModel : ViewModelBase
    {
        private const string NetworkQualityBinary = "/usr/bin/networkQuality";

        readonly Serilog.ILogger Log = Serilog.Log.ForContext<QualityPageViewModel>();

        public ObservableCollection<NetworkInterfaceViewModel> AvailableNetworkInterfaces { get; }

        public DebufferedCommandViewModel DebufferedCommandViewModel { get; }

        private NetworkInterfaceViewModel? _SpecificInterface;
        public NetworkInterfaceViewModel? SpecificInterface
        {
            get => _SpecificInterface;
            set
            {
                SetProperty(ref _SpecificInterface, value);

                //if (value?.BsdName != null)
                //    Preferences.Set(nameof(SpecificInterface), value.BsdName);
                //else
                //    Preferences.Remove(nameof(SpecificInterface));

                UpdateCommandLine();
            }
        }

        private bool _UseSpecificInterface = false;
        public bool UseSpecificInterface
        {
            get => _UseSpecificInterface;
            set
            {
                SetProperty(ref _UseSpecificInterface, value);

                //Preferences.Set(nameof(UseSpecificInterface), value);

                UpdateCommandLine();
            }
        }

        public ICommand ToggleUseSpecificInterfaceCommand { get; }

        public QualityPageViewModel()
        {
            DebufferedCommandViewModel = new DebufferedCommandViewModel(NetworkQualityBinary);

            var repo = Xamarin.Forms.DependencyService.Get<Repository>();
            AvailableNetworkInterfaces = repo.AsObservable;

            ToggleUseSpecificInterfaceCommand = new RelayCommand(() =>
                UseSpecificInterface = !UseSpecificInterface);
        }

        private void UpdateCommandLine()
        {
            var arguments = new ArgumentsBuilder();

            static bool isIPv4(System.Net.IPAddress ad) =>
                ad.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;

            if (UseSpecificInterface &&
                !string.IsNullOrEmpty(SpecificInterface?.BsdName) &&
                SpecificInterface?.IPAddresses?.Any(isIPv4) == true)
            {
                arguments.Add("-I");
                arguments.Add(SpecificInterface.BsdName);
            }

            //arguments.Add(Host);

            DebufferedCommandViewModel.SetArguments(arguments);
        }
    }
}
