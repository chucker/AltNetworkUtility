using System.Threading.Tasks;

using AltNetworkUtility.Services;

using AudioToolbox;

using Foundation;

namespace AltNetworkUtility.macOS.Services
{
    public class MacSystemSoundService : ISystemSoundService
    {
        public async Task PlayAsync()
        {
            var systemSound = new SystemSound(new NSUrl("/System/Library/Sounds/Ping.aiff", false));
            await systemSound.PlayAlertSoundAsync();
        }
    }
}
