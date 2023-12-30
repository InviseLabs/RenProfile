using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RenProfileConsole
{
    public class Privileges
    {
        /// <summary>
        /// Request extra privileges for the process
        /// </summary>
        /// <param name="privilege">The string value specifying the desired privilege as specified in the Windows headers</param>
        /// <returns></returns>
        public static bool EnablePrivileges(string privilege)
        {
            NativeMethods.Privileges.LUID luid = new NativeMethods.Privileges.LUID();
            if (NativeMethods.Privileges.LookupPrivilegeValue(null, privilege, ref luid))
            {
                var tokenPrivileges = new NativeMethods.Privileges.TOKEN_PRIVILEGES();
                tokenPrivileges.PrivilegeCount = 1;
                tokenPrivileges.Privileges = new NativeMethods.Privileges.LUID_AND_ATTRIBUTES[1];
                tokenPrivileges.Privileges[0].Attributes = NativeMethods.Privileges.SE_PRIVILEGE_ENABLED;
                tokenPrivileges.Privileges[0].Luid = luid;

                IntPtr hProc = NativeMethods.Process.GetCurrentProcess();

                if (hProc != null)
                {
                    if (NativeMethods.Process.OpenProcessToken(hProc, (uint)NativeMethods.Process.TokenAccess.AdjustPrivileges, out IntPtr hToken))
                    {
                        return NativeMethods.Privileges.AdjustTokenPrivileges(hToken, false, ref tokenPrivileges, 0, IntPtr.Zero, IntPtr.Zero);
                    }
                }
            }
            return false;
        }
    }
}
