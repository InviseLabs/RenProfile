using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RenProfileConsole
{
    internal class NativeMethods
    {
        internal class Privileges
        {
            internal const int SE_PRIVILEGE_ENABLED = 0x00000002;

            internal struct TOKEN_PRIVILEGES
            {
                public int PrivilegeCount;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
                public LUID_AND_ATTRIBUTES[] Privileges;
            }

            [StructLayout(LayoutKind.Sequential, Pack = 4)]
            internal struct LUID_AND_ATTRIBUTES
            {
                public LUID Luid;
                public UInt32 Attributes;
            }

            internal struct LUID
            {

                public UInt32 LowPart;
                public Int32 HighPart;
            }

            [DllImport("advapi32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool AdjustTokenPrivileges(IntPtr hTok,
                [MarshalAs(UnmanagedType.Bool)]bool DisableAllPrivileges,
                ref TOKEN_PRIVILEGES NewState,
                UInt32 BufferLengthInBytes,
                ref TOKEN_PRIVILEGES PreviousState,
                out UInt32 ReturnLengthInBytes);

            [DllImport("advapi32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool AdjustTokenPrivileges(IntPtr hTok,
                [MarshalAs(UnmanagedType.Bool)]bool DisableAllPrivileges,
                ref TOKEN_PRIVILEGES NewState,
                UInt32 BufferLengthInBytes,
                IntPtr PreviousState,
                IntPtr ReturnLengthInBytes);

            [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool LookupPrivilegeValue(string lpsystemname, string lpname, [MarshalAs(UnmanagedType.Struct)] ref LUID lpLuid);
        }

        internal class Process
        {
            [Flags]
            internal enum TokenAccess : uint
            {
                AdjustPrivileges = 0x0020
            }

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern IntPtr GetCurrentProcess();

            [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);
        }

        internal class Registry
        {
            [DllImport("advapi32.dll")]
            public static extern int RegLoadKey(IntPtr hkey, string lpSubKey, string lpFile);

            [DllImport("advapi32.dll")]
            public static extern int RegUnLoadKey(IntPtr hkey, string lpSubKey);

            internal static readonly IntPtr HKEY_USERS = new IntPtr(unchecked((int)0x80000003));
        }

        internal class FileSystem
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern int GetShortPathName(String pathName, StringBuilder shortName, int cbShortName);
        }
    }
}
