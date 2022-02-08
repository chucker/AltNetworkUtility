using System;
using System.Threading.Tasks;

using Xamarin.Forms.Maps;

namespace AltNetworkUtility.Tabs.Traceroute
{
    public class TraceroutePageViewModel : ViewModelBase
    {
        public TraceroutePageViewModel()
        {
        }

        public async Task Init(Map map)
        {
            var polyline = new Polyline();
            polyline.Geopath.Add(new Position(50.1109, 8.68213)); // p3e9bf30b.dip0.t-ipconnect.de (62.155.243.11)
            polyline.Geopath.Add(new Position(50.1109, 8.68213)); // f-ed11-i.f.de.net.dtag.de (62.154.1.193)
            polyline.Geopath.Add(new Position(50.1109, 8.68213)); // 62.157.250.38
            polyline.Geopath.Add(new Position(45.4642, 9.18998)); // ae9.francoforte73.fra.seabone.net(89.221.34.225)
            polyline.Geopath.Add(new Position(50.1109, 8.68213)); // ae8.francoforte73.fra.seabone.net(195.22.211.57)

            var pin = MakePin(new Position(-27.4766, 153.0166));
            map.Pins.Add(pin);
            polyline.Geopath.Add(new Position(-27.4766, 153.0166)); // 1.1.1.1

            Circle item = new Circle
            {
                StrokeColor = Xamarin.Forms.Color.ForestGreen,
                StrokeWidth = 8,
                Radius = new Distance(1_000),
                Center = new Position(52, 9)
            };
            map.MapElements.Add(item);

            //var mapSpanA = new MapSpan(new Position(38, -122), 1, 1);
            //MapSpan mapSpanB = new MapSpan(item.Center, 1, 1);

            //while (true)
            //{
            //    await Task.Delay(3_000);

            //    map.MoveToRegion(mapSpanA);

            //    await Task.Delay(3_000);

            //    map.MoveToRegion(mapSpanB);
            //}

            map.MapElements.Add(polyline);

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
