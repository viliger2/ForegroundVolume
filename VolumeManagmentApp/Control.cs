using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolumeManagmentApp
{
    class Control
    {
        public static void VolumeDown()
        {
            float currentGlobalVolume = VolumeControls.GetGlobalVolume();
            ProcessInfo.GetWindowThreadProcessId(ProcessInfo.GetForegroundWindow(), out uint processId);
            float applicationVolume = VolumeControls.GetApplicationVolume(processId);
            if (applicationVolume != 0.0f && applicationVolume != -1)
            {
                float oneStep = (4 * applicationVolume) / currentGlobalVolume;
                float newApplicationVolume = applicationVolume - oneStep;
                if (newApplicationVolume < 0f)
                {
                    newApplicationVolume = 0f;
                }
                VolumeControls.SetApplicationVolume(processId, newApplicationVolume);
            }
            //VolumeControls.SetGlobalVolume(currentGlobalVolume);
        }

        public static void VolumeUp()
        {
            float currentGlobalVolume = VolumeControls.GetGlobalVolume();
            ProcessInfo.GetWindowThreadProcessId(ProcessInfo.GetForegroundWindow(), out uint processId);
            float applicationVolume = VolumeControls.GetApplicationVolume(processId);
            if (applicationVolume != 100.0f && applicationVolume != -1)
            {
                float oneStep = (4 * applicationVolume) / currentGlobalVolume;
                float newApplicationVolume = applicationVolume + oneStep;
                if (newApplicationVolume > 100f)
                {
                    newApplicationVolume = 100f;
                }
                VolumeControls.SetApplicationVolume(processId, newApplicationVolume);
            }
           //VolumeControls.SetGlobalVolume(currentGlobalVolume);
        }

    }
}
