using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AltNetworkUtility.Tabs
{
    public class InfoPageViewModel : ViewModelBase
    {
        // TODO: should probably not be a string later
        public ObservableCollection<string> AvailableNetworkInterfaces = new();

        internal Task InitAsync()
        {
            throw new NotImplementedException();
        }
    }
}
