using System;
using Windows.UI.Xaml;

namespace Music
{
    public class TenFootTrigger : StateTriggerBase
    {

        public TenFootTrigger()
        {
            var deviceFamily = Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily;
            SetActive(deviceFamily == "Windows.Xbox");
        }
    }
}