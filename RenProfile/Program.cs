using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Win32;

namespace RenProfileConsole
{
    /// <summary>
    /// A command line tool to rename a user profile by updating all references to the profile folder in the registry
    /// and renaming the folder.
    /// </summary>
    class Program
    {
        //- Define Variables
        public static bool logErrs = false;
        public static int errors = 0;
        public static int successful = 0;
        public static int processed = 0;
        //- Define Paths
        public static String oldDir="";
        public static String newDir="";
        public static String errPath="";

        private static void Main(string[] args)
        {
            try
            {
                //- Parameter error checking
                if (args.Length == 0) { Console.WriteLine("Missing all parameters."); ExitConsole(-1); return; }
                else if (args.Length == 1)
                {
                    if (args[0] == "?") { Console.WriteLine(@"Syntax: C:\Full\Path\To\Profile C:\Path\To\Desired\Profile C:\Optional\Path\To\Write\Errors.txt"); ExitConsole(1); return; }
                    else { Console.WriteLine("Missing 1 parameter."); ExitConsole(-1); return; }
                }
                else if (args.Length == 3)
                {
                    errPath = args[2]; if (!String.IsNullOrWhiteSpace(errPath) || errPath.Length > 4) { logErrs = true; }
                    else { Console.WriteLine("Error log was specified but path is invalid."); ExitConsole(-1); return; }
                }
                else if (args.Length > 3)
                { Console.WriteLine("Missing 1 parameter."); ExitConsole(-1); }

                //- Set variables
                oldDir = args[0];
                newDir = args[1];

                //- Path error checking
                if (String.IsNullOrWhiteSpace(oldDir) || oldDir.Length < 4) { Console.WriteLine("Old user profile path is invalid."); ExitConsole(-1); return; }
                else if (String.IsNullOrWhiteSpace(oldDir) || oldDir.Length < 4) { Console.WriteLine("New user profile path is invalid."); ExitConsole(-1); return; }
                else if (!Directory.Exists(oldDir)) { Console.WriteLine("Old user profile path does not exist."); ExitConsole(-1); return; }
                else if (Directory.Exists(newDir)) { Console.WriteLine("New user profile path already exists. Use another name."); ExitConsole(-1); return; }
            }
            catch (Exception ex)
            { 
                if (logErrs) { LogError(ex.ToString()); Console.WriteLine("Unexpected error occurred, cannot continue. Check error log."); } ExitConsole(-1); return; 
            }

            //- No errors and parameters are all as expected.
            //- Let's get started!
            Start();
        }

        public static void Start()
        {
            //- Moved meat of the code into Start mathod to making it cleaner when creating UI tool next.

            string tempUserHiveName = $"__tmpMoveUserProfile";

            // Even running as administrator, the functionality to load and unload registry hives
            // is restricted without requesting extra privilges.
            if (Privileges.EnablePrivileges("SeRestorePrivilege"))
            {

                int loadCode = RegistryUtils.LoadUserHive($"{oldDir}\\NTUSER.dat", tempUserHiveName);

                using (RegistryKey userHive = Registry.Users.OpenSubKey(tempUserHiveName))
                {
                    // Only need to deal with two trees - HKEY_LOCAL_MACHINE and HKEY_USERS.
                    // HKEY_CURRENT_USER won't be the user being modified (that hive is mounted under
                    // HKEY_USER, and other trees that may contain the path are apparently reflections of
                    // subtrees of one of the trees being modified.
                    Console.WriteLine("==========Updating HKLM==========");
                    try { Registry.LocalMachine.IterateKeys((key) => UpdateKey(key, oldDir, newDir)); }
                    catch { Console.WriteLine("Error occurred when updating HKLM keys. Check output."); }
                    Console.WriteLine("==========Updating HKU==========");
                    try { Registry.Users.IterateKeys((key) => UpdateKey(key, oldDir, newDir)); }
                    catch { Console.WriteLine("Error occurred when updating HKU keys. Check output."); }
                    Console.WriteLine($"Processed: {processed}, Updated: {successful}, Failed: {errors}");
                }

                RegistryUtils.UnloadUserHive(tempUserHiveName);
                try { Directory.Move(oldDir, newDir); }
                catch { Console.WriteLine("Error renaming physical profile directories, another process might have a lock still."); }

                ExitConsole(1);
            }
            else
            {
                Console.WriteLine("Load key privilege not granted.");
                ExitConsole(-1);
            }
        }

        static void ExitConsole(int exitCode)
        {
            Console.WriteLine("");
            Console.WriteLine("Exit code is " + exitCode + " (-1=fatal error, 0=fail, 1=success");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(exitCode);
        }

        static void LogError(string errorMessage)
        {
            try
            {
                //- Condition ? True : False
                using (var file = File.Exists(errPath) ? File.Open(errPath, FileMode.Append) : File.Open(errPath, FileMode.CreateNew))
                using (var sw = new StreamWriter(file))
                { sw.WriteLine($"{DateTime.Now.ToString()}: {errorMessage} \n==========="); }
            }
            catch { /*Error writing to error log*/}
        }


        static void UpdateKey(RegistryKey key, string oldDir, string newDir)
        {
            try
            {
                StringBuilder sb = new StringBuilder(300);
                int n = NativeMethods.FileSystem.GetShortPathName(oldDir, sb, 300);
                string shortOldDir = sb.ToString();

                string pattern = $"({Regex.Escape(oldDir)}|{Regex.Escape(shortOldDir)})";

                string[] names = key.GetValueNames();
                Console.WriteLine($"={{{key.ToString()}}}=");
                foreach (string name in names)
                {
                    if (String.IsNullOrWhiteSpace(name)) { continue; }
                    processed++;

                    try
                    {
                        if (key.GetValueKind(name) == RegistryValueKind.MultiString)
                        {
                            string[] oldValues = (string[])key.GetValue(name);
                            string[] newValues = oldValues.Select(value => Regex.Replace(value, pattern, newDir, RegexOptions.IgnoreCase)).ToArray();

                            if (!newValues.SequenceEqual(oldValues))
                            {
                                try
                                {
                                    key.SetValue(name, newValues);
                                    Console.WriteLine(String.Join(":", newValues));
                                    successful++;
                                }
                                catch (Exception ex) { if (logErrs) { LogError($"Error accessing/writing {name}\n{ex.ToString()}"); } Console.WriteLine($"Error setting: {newValues}"); errors++; }
                            }
                        }
                        else if (key.GetValueKind(name) == RegistryValueKind.String || key.GetValueKind(name) == RegistryValueKind.ExpandString)
                        {
                            string oldValue = (string)key.GetValue(name);
                            string newValue = Regex.Replace(oldValue, pattern, newDir, RegexOptions.IgnoreCase);

                            if (oldValue != newValue)
                            {
                                try
                                {
                                    key.SetValue(name, newValue);
                                    Console.WriteLine($"Set: {newValue}");
                                    successful++;
                                }
                                catch (Exception ex) { if (logErrs) { LogError($"Error accessing/writing {name}\n{ex.ToString()}"); } Console.WriteLine($"Error setting: {newValue}"); errors++; }
                            }
                        }

                        string newValueName = Regex.Replace(name, pattern, newDir, RegexOptions.IgnoreCase);

                        if (newValueName != name)
                        {
                            try
                            {
                                key.RenameValue(name, newValueName);
                                Console.WriteLine($"Renamed: {newValueName}");
                                successful++;
                            }
                            catch { Console.WriteLine($"Error renaming: {newValueName}"); errors++; }
                        }
                    }
                    catch (Exception ex) { if (logErrs) { LogError($"Error accessing/writing {name}\n{ex.ToString()}"); } Console.WriteLine($"Error accessing: {name}"); errors++; }
                }

                string keypattern = pattern.Replace(@"\", @"%5C");
                string keyNewDir = newDir.Replace(@"\", @"%5C");
                foreach (string keyName in key.GetSubKeyNames())
                {
                    if (String.IsNullOrWhiteSpace(keyName)) { continue; }
                    processed++;

                    string newKeyName = Regex.Replace(keyName, keypattern, keyNewDir, RegexOptions.IgnoreCase);
                    if (keyName != newKeyName)
                    {
                        try
                        {
                            key.RenameSubKey(keyName, newKeyName);
                            Console.WriteLine($"Updated: {newKeyName}");
                            successful++;
                        }
                        catch (Exception ex) { if (logErrs) { LogError($"Error accessing/writing {keyName}\n{ex.ToString()}"); } Console.WriteLine($"Error updating: {newKeyName}"); errors++; }
                    }
                }
            }
            catch (Exception ex) { if (logErrs) { LogError($"Error accessing/writing {key.ToString()}\n{ex.ToString()}"); } errors++; }
        }
    }
}
