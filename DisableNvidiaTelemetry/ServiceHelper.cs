#region

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.ServiceProcess;

#endregion

namespace DisableNvidiaTelemetry
{
    public static class ServiceHelper
    {
        private const uint SERVICE_NO_CHANGE = 0xFFFFFFFF;
        private const uint SERVICE_QUERY_CONFIG = 0x00000001;
        private const uint SERVICE_CHANGE_CONFIG = 0x00000002;
        private const uint SC_MANAGER_ALL_ACCESS = 0x000F003F;

        private const int ERROR_INSUFFICIENT_BUFFER = 122;

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern Boolean ChangeServiceConfig(
            IntPtr hService,
            uint nServiceType,
            uint nStartType,
            uint nErrorControl,
            string lpBinaryPathName,
            string lpLoadOrderGroup,
            IntPtr lpdwTagId,
            [In] char[] lpDependencies,
            string lpServiceStartName,
            string lpPassword,
            string lpDisplayName);

        [DllImport("advapi32.dll", EntryPoint = "QueryServiceConfigW", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool QueryServiceConfig(SafeHandle hService, IntPtr lpServiceConfig, int cbBufSize, out int pcbBytesNeeded);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr OpenSCManager(string machineName, string databaseName, uint dwAccess);

        [DllImport("advapi32.dll", EntryPoint = "CloseServiceHandle")]
        public static extern int CloseServiceHandle(IntPtr hSCObject);

        public static void ChangeStartMode(ServiceController svc, ServiceStartMode mode)
        {
            var scManagerHandle = OpenSCManager(null, null, SC_MANAGER_ALL_ACCESS);
            if (scManagerHandle == IntPtr.Zero)
                throw new ExternalException("Open Service Manager Error");

            var serviceHandle = OpenService(scManagerHandle, svc.ServiceName, SERVICE_QUERY_CONFIG | SERVICE_CHANGE_CONFIG);

            if (serviceHandle == IntPtr.Zero)
                throw new ExternalException("Open Service Error");

            var result = ChangeServiceConfig(
                serviceHandle,
                SERVICE_NO_CHANGE,
                (uint) mode,
                SERVICE_NO_CHANGE,
                null,
                null,
                IntPtr.Zero,
                null,
                null,
                null,
                null);

            if (result == false)
            {
                var nError = Marshal.GetLastWin32Error();
                var win32Exception = new Win32Exception(nError);
                throw new ExternalException($"Could not change service start type: {win32Exception.Message}");
            }

            CloseServiceHandle(serviceHandle);
            CloseServiceHandle(scManagerHandle);
        }

        internal static ServiceStartMode GetServiceStartMode(ServiceController svc)
        {
            int neededBytes = 0;

            bool result = QueryServiceConfig(svc.ServiceHandle, IntPtr.Zero, 0, out neededBytes);
            int win32err = Marshal.GetLastWin32Error();
            if (win32err == ERROR_INSUFFICIENT_BUFFER) //122
            {
                IntPtr ptr = IntPtr.Zero;
                try
                {
                    ptr = Marshal.AllocCoTaskMem(neededBytes);
                    result = QueryServiceConfig(svc.ServiceHandle, ptr, neededBytes, out neededBytes);
                    if (result)
                    {
                        QUERY_SERVICE_CONFIG config = (QUERY_SERVICE_CONFIG) Marshal.PtrToStructure(ptr, typeof(QUERY_SERVICE_CONFIG));
                        return config.dwStartType;
                    }
                    else
                    {
                        win32err = Marshal.GetLastWin32Error();
                        throw new Win32Exception(win32err, "QueryServiceConfig failed");
                    }
                }
                finally
                {
                    Marshal.FreeCoTaskMem(ptr);
                }
            }
            else
            {
                throw new Win32Exception(win32err, "QueryServiceConfig failed");
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct QUERY_SERVICE_CONFIG
        {
            [MarshalAs(UnmanagedType.U4)] internal readonly ServiceType dwServiceType;
            [MarshalAs(UnmanagedType.U4)] internal readonly ServiceStartMode dwStartType;
            internal readonly int dwErrorControl;
            [MarshalAs(UnmanagedType.LPWStr)] internal readonly string lpBinaryPathName;
            [MarshalAs(UnmanagedType.LPWStr)] internal readonly string lpLoadOrderGroup;
            internal readonly int dwTagId;
            [MarshalAs(UnmanagedType.LPWStr)] internal readonly string lpDependencies;
            [MarshalAs(UnmanagedType.LPWStr)] internal readonly string lpServiceStartName;
            [MarshalAs(UnmanagedType.LPWStr)] internal readonly string lpDisplayName;
        }
    }
}