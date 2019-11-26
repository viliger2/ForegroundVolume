using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace VolumeManagmentApp
{
    class VolumeControls
    {
        // gets volume of process in percent out of global volume
        // Example: if global volume is 40 and process volume is 20, then method will return 50)
        public static float GetApplicationVolume(uint processId)
        {
            ISimpleAudioVolume volume = GetVolumeObject(processId);
            if (volume == null)
                return -1;

            float level;
            volume.GetMasterVolume(out level);
            Marshal.ReleaseComObject(volume);
            return level * 100;
        }

        // gets mute status of process
        public static bool? GetApplicationMute(uint processId)
        {
            ISimpleAudioVolume volume = GetVolumeObject(processId);
            if (volume == null)
                return null;

            bool mute;
            volume.GetMute(out mute);
            Marshal.ReleaseComObject(volume);
            return mute;
        }

        // sets volume of process in percent out of global volume
        public static void SetApplicationVolume(uint processId, float level)
        {
            ISimpleAudioVolume volume = GetVolumeObject(processId);
            if (volume == null)
                return;

            Guid guid = Guid.Empty;
            volume.SetMasterVolume(level / 100, ref guid);
            Marshal.ReleaseComObject(volume);
        }

        // sets mute status of process
        public static void SetApplicationMute(uint processId, bool mute)
        {
            ISimpleAudioVolume volume = GetVolumeObject(processId);
            if (volume == null)
                return;

            Guid guid = Guid.Empty;
            volume.SetMute(mute, ref guid);
            Marshal.ReleaseComObject(volume);
        }

        // gets global volume in values from 0 to 100
        public static float GetGlobalVolume()
        {
            MMDeviceEnumerator deviceEnumerator;
            IMMDeviceEnumerator iDeviceEnumerator;
            IMMDevice speakers;
            IAudioEndpointVolume aev;

            //GetGlobalControls(out deviceEnumerator, out speakers, out aev);
            deviceEnumerator = new MMDeviceEnumerator();
            iDeviceEnumerator = (IMMDeviceEnumerator)deviceEnumerator;
            iDeviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);
            Guid IID_IAudioEndpointVolume = typeof(IAudioEndpointVolume).GUID;
            speakers.Activate(ref IID_IAudioEndpointVolume, 0, IntPtr.Zero, out object o);
            aev = (IAudioEndpointVolume)o;

            if (aev == null)
            {
                Marshal.ReleaseComObject(o);
                Marshal.ReleaseComObject(speakers);
                Marshal.ReleaseComObject(iDeviceEnumerator);
                Marshal.ReleaseComObject(deviceEnumerator);
                return -1;
            }

            aev.GetMasterVolumeLevelScalar(out float level);

            Marshal.ReleaseComObject(aev);
            Marshal.ReleaseComObject(o);
            Marshal.ReleaseComObject(speakers);
            Marshal.ReleaseComObject(iDeviceEnumerator);
            Marshal.ReleaseComObject(deviceEnumerator);

            return level * 100;

        }

        // sets global volume in values from 0 too 100
        public static void SetGlobalVolume(float level)
        {
            if (level < 0 || level > 100)
                return;

            IMMDeviceEnumerator deviceEnumerator;
            IMMDevice speakers;
            IAudioEndpointVolume aev;

            //GetGlobalControls(out deviceEnumerator, out speakers, out aev);
            deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
            deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);
            Guid IID_IAudioEndpointVolume = typeof(IAudioEndpointVolume).GUID;
            speakers.Activate(ref IID_IAudioEndpointVolume, 0, IntPtr.Zero, out object o);
            aev = (IAudioEndpointVolume)o;

            if (aev == null)
            {
                Marshal.ReleaseComObject(o);
                Marshal.ReleaseComObject(speakers);
                Marshal.ReleaseComObject(deviceEnumerator);
                return;
            }

            aev.SetMasterVolumeLevelScalar(level / 100, IntPtr.Zero);

            Marshal.ReleaseComObject(aev);
            Marshal.ReleaseComObject(o);
            Marshal.ReleaseComObject(speakers);
            Marshal.ReleaseComObject(deviceEnumerator);
        }

        // gets volume object needed for public methods to operate
        private static ISimpleAudioVolume GetVolumeObject(uint processId)
        {
            MMDeviceEnumerator deviceEnumerator;
            IMMDeviceEnumerator iDeviceEnumerator;
            IMMDevice speakers;
            IAudioSessionManager2 mgr;
            IAudioSessionEnumerator sessionEnumerator;
            int count;

            //GetVolumeControls(out deviceEnumerator, out speakers, out mgr, out sessionEnumerator, out count);
            deviceEnumerator = new MMDeviceEnumerator();
            iDeviceEnumerator = (IMMDeviceEnumerator)deviceEnumerator;
            iDeviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);
            Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
            speakers.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out object o);
            mgr = (IAudioSessionManager2)o;
            mgr.GetSessionEnumerator(out sessionEnumerator);
            sessionEnumerator.GetCount(out count);

            ISimpleAudioVolume volumeControl = null;
            for (int i = 0; i < count; i++)
            {
                IAudioSessionControl asc;
                sessionEnumerator.GetSession(i, out asc);

                IAudioSessionControl2 asc2 = GetAudioControl2(asc);

                uint currentProcessId;

                asc2.GetProcessId(out currentProcessId);

                if (currentProcessId == processId)
                {
                    volumeControl = asc as ISimpleAudioVolume;
                    Marshal.ReleaseComObject(asc2);
                    break;
                }
                Marshal.ReleaseComObject(asc2);
                Marshal.ReleaseComObject(asc);
            }
            Marshal.ReleaseComObject(sessionEnumerator);
            Marshal.ReleaseComObject(mgr);
            Marshal.ReleaseComObject(o);
            Marshal.ReleaseComObject(speakers);
            Marshal.ReleaseComObject(iDeviceEnumerator);
            Marshal.ReleaseComObject(deviceEnumerator);
            return volumeControl;
        }

        // getting IAudioSessionControl2 out of IAudioSessionControl via QueryInterface
        private static IAudioSessionControl2 GetAudioControl2(IAudioSessionControl ctl)
        {
            IntPtr pUnk = Marshal.GetIUnknownForObject(ctl);
            IntPtr pI;
            Guid baseGuid = typeof(IAudioSessionControl).GUID;
            Marshal.QueryInterface(pUnk, ref baseGuid, out pI);
            object asc = Marshal.GetObjectForIUnknown(pI);
            IAudioSessionControl2 asc2 = (IAudioSessionControl2)asc;
            return asc2;
        }

        private static void GetVolumeControls(out IMMDeviceEnumerator deviceEnumerator, out IMMDevice speakers, out IAudioSessionManager2 mgr, out IAudioSessionEnumerator sessionEnumerator, out int count)
        {
            GetDeviceEnumerator(out deviceEnumerator);
            GetSpeakers(deviceEnumerator, out speakers);
            GetSessionManager(speakers, out mgr);
            GetSessionEnumerator(mgr, out sessionEnumerator, out count);
        }

        private static void GetGlobalControls(out IMMDeviceEnumerator deviceEnumerator, out IMMDevice speakers, out IAudioEndpointVolume aev)
        {
            GetDeviceEnumerator(out deviceEnumerator);
            GetSpeakers(deviceEnumerator, out speakers);
            GetAudioEndpointVolume(speakers, out aev);
        }

        // getting a new device enumerator for finding first output device
        private static void GetDeviceEnumerator(out IMMDeviceEnumerator deviceEnumerator)
        {
            deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
        }

        // getting default sound output device of the system (1st render + multimedia)
        private static void GetSpeakers(IMMDeviceEnumerator deviceEnumerator, out IMMDevice speakers)
        {
            deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);
        }

        // activate the session manager. we need the enumerator
        private static void GetSessionManager(IMMDevice speakers, out IAudioSessionManager2 mgr)
        {
            Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
            object o;
            speakers.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out o);
            mgr = (IAudioSessionManager2)o;
        }

        // gets global volume endpoint
        private static void GetAudioEndpointVolume(IMMDevice speakers, out IAudioEndpointVolume aev)
        {
            Guid IID_IAudioEndpointVolume = typeof(IAudioEndpointVolume).GUID;
            object o;
            speakers.Activate(ref IID_IAudioEndpointVolume, 0, IntPtr.Zero, out o);
            aev = (IAudioEndpointVolume)o;
        }

        // enumerate sessions for on this device
        private static void GetSessionEnumerator(IAudioSessionManager2 mgr, out IAudioSessionEnumerator sessionEnumerator, out int count)
        {
            mgr.GetSessionEnumerator(out sessionEnumerator);
            sessionEnumerator.GetCount(out count);
        }
    }

    #region VolumeHooks

    [ComImport]
    [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    internal class MMDeviceEnumerator
    {
    }

    internal enum EDataFlow
    {
        eRender,
        eCapture,
        eAll,
        EDataFlow_enum_count
    }

    internal enum ERole
    {
        eConsole,
        eMultimedia,
        eCommunications,
        ERole_enum_count
    }

    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceEnumerator
    {
        int NotImpl1();

        [PreserveSig]
        int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppDevice);

        // the rest is not implemented
    }

    [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDevice
    {
        [PreserveSig]
        int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);

        // the rest is not implemented
    }

    [Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionManager2
    {
        int NotImpl1();
        int NotImpl2();

        [PreserveSig]
        int GetSessionEnumerator(out IAudioSessionEnumerator SessionEnum);

        // the rest is not implemented
    }

    [Guid("5CDF2C82-841E-4546-9722-0CF74078229A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioEndpointVolume
    {
        int NotImpl1();
        int NotImpl2();

        [PreserveSig]
        int GetChannelCount(out uint pnChannelCount);

        [PreserveSig]
        int SetMasterVolumeLevel(float pfLevelDB, IntPtr pguidEventContext);

        [PreserveSig]
        int SetMasterVolumeLevelScalar(float pfLevel, IntPtr pguidEventContext);

        [PreserveSig]
        int GetMasterVolumeLevel(out float pfLevelDB);

        [PreserveSig]
        int GetMasterVolumeLevelScalar(out float pfLevel);
    }

    [Guid("BFB7FF88-7239-4FC9-8FA2-07C950BE9C6D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionControl2
    {

        // skip everything we don't need
        int NotImpl1();
        int NotImpl2();
        int NotImpl3();
        int NotImpl4();
        int NotImpl5();
        int NotImpl6();
        int NotImpl7();
        int NotImpl8();
        int NotImpl9();
        int NotImpl10();
        int NotImpl11();

        [PreserveSig]
        int GetProcessId(out uint ProccessId);
    }

    [Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionEnumerator
    {
        [PreserveSig]
        int GetCount(out int SessionCount);

        [PreserveSig]
        int GetSession(int SessionCount, out IAudioSessionControl Session);
    }

    [Guid("F4B1A599-7266-4319-A8CA-E70ACB11E8CD"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionControl
    {
        int NotImpl1();

        [PreserveSig]
        int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

        // the rest is not implemented
    }

    [Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISimpleAudioVolume
    {
        [PreserveSig]
        int SetMasterVolume(float fLevel, ref Guid EventContext);

        [PreserveSig]
        int GetMasterVolume(out float pfLevel);

        [PreserveSig]
        int SetMute(bool bMute, ref Guid EventContext);

        [PreserveSig]
        int GetMute(out bool pbMute);
    }

    #endregion
}
