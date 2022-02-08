using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;

using AltNetworkUtility.Services;
using AltNetworkUtility.ViewModels;

using CliWrap.Builders;

using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace AltNetworkUtility.Tabs.Traceroute
{
    public class TraceroutePageViewModel : ViewModelBase
    {
        readonly Serilog.ILogger Log = Serilog.Log.ForContext<TraceroutePageViewModel>();

        public class IpApiResult
        {
#nullable disable
            public string Status { get; set; }
            public double Lat { get; set; }
            public double Lon { get; set; }
            public string As { get; set; }
#nullable restore
        }

        private const string TracerouteBinary = "/usr/sbin/traceroute";

        public PreferencesService Preferences { get; }

        public DebufferedCommandViewModel DebufferedCommandViewModel { get; }

        public Map Map { get; set; }

        private List<System.Net.IPAddress> _Addresses;
        Polyline _Polyline;

        private string _Host = "";
        public string Host
        {
            get => _Host;
            set
            {
                SetProperty(ref _Host, value);

                Preferences.Set(nameof(Host), value);

                _Addresses = new();
                _Polyline.Geopath.Clear();
                Map.Pins.Clear();

                UpdateCommandLine();
                DebufferedCommandViewModel.RefreshRunCommandCanExecute();
            }
        }

        private void UpdateCommandLine()
        {
            var arguments = new ArgumentsBuilder();

            arguments.Add("-n");

            arguments.Add(Host);

            DebufferedCommandViewModel.SetArguments(arguments);
        }

        public TraceroutePageViewModel()
        {
            DebufferedCommandViewModel = new DebufferedCommandViewModel(TracerouteBinary)
            {
                RunCommandCanExecute =
                () => !string.IsNullOrWhiteSpace(Host)
            };

            DebufferedCommandViewModel.OutputAppended += DebufferedCommandViewModel_OutputAppended;

            Preferences = PreferencesService.GetInstance<TraceroutePageViewModel>();
        }

        private static bool IsIpAdressPrivate(IPAddress address)
        {
            if (IPAddress.IsLoopback(address))
                return true;
            if (address.IsIPv6LinkLocal)
                return true;
            else if (address.ToString() == "::1") return false;

            byte[] bytes = address.GetAddressBytes();
            switch (bytes[0])
            {
                case 10:
                    return true;
                case 172:
                    return bytes[1] < 32 && bytes[1] >= 16;
                case 192:
                    return bytes[1] == 168;
                default:
                    return false;
            }
        }

        bool _BusyAdding = false;
        bool _BusyPanning = false;

        private async void DebufferedCommandViewModel_OutputAppended(object? sender, DebufferedCommandViewModel.OutputAppendedEventArgs e)
        {
            if (!TracerouteOutputParser.TryParse(e.NewOutput, out var results))
                return;

            _Addresses.AddRange(results);

            var httpClient = new System.Net.Http.HttpClient();

            //var random = new Random();
            foreach (var ipAddress in results)
            {
                if (IsIpAdressPrivate(ipAddress))
                    continue;

                //var randN = random.Next(45, 60);
                //var randE = random.Next(8, 10);

                var apiResult = await httpClient.GetFromJsonAsync<IpApiResult>($"http://ip-api.com/json/{ipAddress}?fields=status,lat,lon,as");

                if (apiResult == null)
                    continue;

                if (apiResult.Lat == 0 || apiResult.Lon == 0)
                    continue;

                //_Polyline.Geopath.Add(new Position(randN, randE));

                if (!_Polyline.Geopath.Any(p => p.Latitude == apiResult.Lat && p.Longitude == apiResult.Lon))
                {
                    //Log.Debug($"Adding {apiResult.Lat}, {apiResult.Lon}");
                    Console.WriteLine($"Adding {apiResult.Lat}, {apiResult.Lon}");

                    //await Task.Delay(100);

                    while (_BusyAdding)
                        await Task.Delay(100);

                    _BusyAdding = true;

                    await Device.InvokeOnMainThreadAsync(() =>
                    {
                        var pin = MakePin(new Position(apiResult.Lat, apiResult.Lon));
                        Map.Pins.Add(pin);

                        _Polyline.Geopath.Add(new Position(apiResult.Lat, apiResult.Lon));
                    });

                    _BusyAdding = false;
                }
            }

            while (_BusyPanning)
                await Task.Delay(100);

            _BusyPanning = true;

            if (_Polyline.Geopath.Any())
            {
                Position center = new Position(_Polyline.Geopath.Average(p => p.Latitude), _Polyline.Geopath.Average(p => p.Longitude));
                double latitudeDegrees = _Polyline.Geopath.Max(p => p.Latitude) - _Polyline.Geopath.Min(p => p.Latitude);
                double longitudeDegrees = _Polyline.Geopath.Max(p => p.Longitude) - _Polyline.Geopath.Min(p => p.Longitude);
                var span = new MapSpan(center,
                    Math.Min(Math.Max(latitudeDegrees, 0.1), 90),
                    Math.Min(Math.Max(longitudeDegrees, 0.1), 90));

                //await Task.Delay(100);

                await Device.InvokeOnMainThreadAsync(() => Map.MoveToRegion(span));
            }

            _BusyPanning = false;
        }

        public async Task Init(Map map)
        {
            this.Map = map;

            _Polyline = new Polyline();
            //polyline.Geopath.Add(new Position(50.1109, 8.68213)); // p3e9bf30b.dip0.t-ipconnect.de (62.155.243.11)
            //polyline.Geopath.Add(new Position(50.1109, 8.68213)); // f-ed11-i.f.de.net.dtag.de (62.154.1.193)
            //polyline.Geopath.Add(new Position(50.1109, 8.68213)); // 62.157.250.38
            //polyline.Geopath.Add(new Position(45.4642, 9.18998)); // ae9.francoforte73.fra.seabone.net(89.221.34.225)
            //polyline.Geopath.Add(new Position(50.1109, 8.68213)); // ae8.francoforte73.fra.seabone.net(195.22.211.57)

            //var pin = MakePin(new Position(-27.4766, 153.0166));
            //map.Pins.Add(pin);
            //polyline.Geopath.Add(new Position(-27.4766, 153.0166)); // 1.1.1.1

            //Circle item = new Circle
            //{
            //    StrokeColor = Xamarin.Forms.Color.ForestGreen,
            //    StrokeWidth = 8,
            //    Radius = new Distance(1_000),
            //    Center = new Position(52, 9)
            //};
            //map.MapElements.Add(item);

            //var mapSpanA = new MapSpan(new Position(38, -122), 1, 1);
            //MapSpan mapSpanB = new MapSpan(item.Center, 1, 1);

            //while (true)
            //{
            //    await Task.Delay(3_000);

            //    map.MoveToRegion(mapSpanA);

            //    await Task.Delay(3_000);

            //    map.MoveToRegion(mapSpanB);
            //}

            map.MapElements.Add(_Polyline);
            
            //await Task.Delay(3_000);

            //map.MoveToRegion(polyline.)
        }

        private static Pin MakePin(Position position)
        {
            return new Pin
            {
                Position = position,
                Label = "test",
                Type = PinType.Place
            };
        }
    }
}
