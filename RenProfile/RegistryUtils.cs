using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RenProfileConsole
{
    public static class RegistryUtils
    {
        public static void IterateKeys(this RegistryKey root, Action<RegistryKey> action)
        {
            if (root == null)
            {
                return;
            }

            Parallel.ForEach(root.GetSubKeyNames(), keyname =>
            {
                try
                {
                    using (RegistryKey key = root.OpenSubKey(keyname, true))
                    {
                        key.IterateKeys(action);
                    }
                }
                catch (Exception e)
                {
                }
            });

            action(root);
        }

        public static void RenameValue(this RegistryKey key, string oldName, string newName)
        {
            key.SetValue(newName, key.GetValue(oldName), key.GetValueKind(oldName));
            key.DeleteValue(oldName);
        }

        public static void RenameSubKey(this RegistryKey root, string oldName, string newName)
        {
            using (RegistryKey subKey = root.OpenSubKey(oldName))
            {
                CloneSubKey(root, subKey, newName);
            }

            root.DeleteSubKeyTree(oldName);
        }

        public static void CloneSubKey(RegistryKey root, RegistryKey source, string newName)
        {
            using (RegistryKey target = root.CreateSubKey(newName))
            {

                foreach (string name in source.GetValueNames())
                {
                    target.SetValue(name, source.GetValue(name), source.GetValueKind(name));
                }

                foreach (string name in source.GetSubKeyNames())
                {
                    using (RegistryKey subKey = source.OpenSubKey(name))
                    {
                        CloneSubKey(target, subKey, name);
                    }
                }
            }
        }

        /// <summary>
        /// Load a registry hive file under HKEY_USERS
        /// </summary>
        /// <param name="hiveFilePath">The path to the registry hive file to load</param>
        /// <param name="subKeyName">The name of the subkey of HKEY_CURRENT_USER to load the hive under</param>
        /// <returns>0 on success, an error code on failure as defined by RegLoadKey in the Windows APIs</returns>
        public static int LoadUserHive(string hiveFilePath, string subKeyName)
        {
            return NativeMethods.Registry.RegLoadKey(NativeMethods.Registry.HKEY_USERS, subKeyName, hiveFilePath);
        }

        /// <summary>
        /// Unload a registry hive file under HKEY_USERS
        /// </summary>
        /// <param name="subKeyName">The name of the subkey of HKEY_CURRENT_USER to unload</param>
        /// <returns>0 on success, an error code on failure as defined by RegUnLoadKey in the Windows APIs</returns>
        public static int UnloadUserHive(string subKeyName)
        {
            return NativeMethods.Registry.RegUnLoadKey(NativeMethods.Registry.HKEY_USERS, subKeyName);
        }
    }
}
