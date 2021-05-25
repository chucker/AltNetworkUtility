using System.Threading.Tasks;

using AltNetworkUtility.Services;

using AudioToolbox;

using Foundation;

#nullable enable

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
