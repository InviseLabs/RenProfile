# RenProfile
### Tool to Rename Profile Folder Path in Windows Without Creating New User Account
#### Rename Windows Profile Path - Registry Find and Replace All

(For usage info, see below the introduction.)

## Introduction to RenProfile:  Use Cases and Considerations 

RenProfile is a tool to rename the Windows profile folder path of your user account. Run from a separate administrator account. It recursively performs a `find and replace all` operation within the registry – a method I developed for this specific purpose – changing every instance of your old profile path to your new profile path. **Sounds dangerous, and it might be, but we've tested this successfully many times on Windows 10 and Windows 11, but it should work on Windows 7 and higher.**

Technically, there's no reason why this *should* cause problems, except in poorly written programs that don't properly check the user account path using relevant APIs (like `%userprofile%`) or programs that hard-code this path in settings files (which no program *should* ever do). While some developers might follow these bad practices, in my testing, this seems rare. (Windows Server environments, for example, offer built-in user profile migration.) In such cases, you might need to inform the affected program of the new path.

## Usage Info
RenProfile is a CLI app. It must be executed from a a separate Administrator account and run from an Admin terminal / CMD Prompt. It takes 2 required arguments and 1 optional argument. No other steps are necessary, RenProfile handles the `Find & Replace All` operation in the registry and renames the physical target user folder. However, after the operation navigate to `C:\Users` and verify the user's profile folder name was changed. If, for some reason, the user's folder name was not changed, then manually rename the folder, and reboot. Why this could happen: Occasionally, RenProfile may fail to rename the physical user folder due to NTFS permissions; in such circumstances, this is considered intended behavior, as we will not change NTFS permissions for you due to the implications this could cause. This is for the administrator, ie you, to figure out, if it comes to that. (You may want to use the move command to do this, *if* RenProfile experiences this issue.)

Required Arguments:<br/>
RenProfile C:\Users\OldUserPath C:\Users\NewDesiredUserPath

Optional Arguments:<br/>
3rd argument, specify log file path where you want the log file saved. Example: `C:\IT\RenProfile.log`

<br/>

**If RenProfile doesn't work or causes problems, follow these troubleshooting steps:**

<br/>

## Troubleshooting Steps
#### If renaming the user profile folder path causes issues with some applications, or otherwise doesn't work as expected, go through these troubleshooting steps. If you need to reverse the changes, you can find steps to do this further below.
* **Double-Check Paths:** Carefully verify the old and new profile paths you entered. Even a small typo can cause issues. The paths should be the full, absolute paths (e.g., `C:\Users\OldUsername` and `C:\Users\NewUsername`), not relative paths.
* **Check for Open Files/Processes:** Before running RenProfile, close all programs running under the user account you're modifying, then reboot. Open files or running processes can lock registry keys and prevent changes. Use Task Manager to ensure no lingering processes are associated with the target user.
* **Run RenProfile Again:** It won't harm the system to reboot and run it again to ensure that all registry paths have been successfully modified. You could also use `msconfig`, select 🠆 Boot 🠆 Boot options 🠆 Safe mode 🠆 Network, which will reboot into Safe Mode with Networking, and from there you can run RenProfile again without having to worry about locking programs.
* **Check Event Viewer:** Look in the Windows Event Viewer for any errors or warnings related to user profiles, applications, or the registry around the time you ran RenProfile.  This can give clues about the cause of the problem.
* **Hidden Files/Folders:** Some profile data might be stored in hidden folders. Check `%AppData%` (Roaming AppData), `%LocalAppData%` (Local AppData), and `%ProgramData%` for config files related to the application
* **Long Paths:** Be mindful of long file paths. Windows has limitations on path lengths, so extremely long profile paths can cause issues. Might also be helpful to enable Long Path support in Windows to be sure this isn't causing you an issue.
* **Reinstall the Problematic Application:** If a specific application is misbehaving after renaming the profile, check their documentation, but we at Invise Labs and Invise Solutions, have had success with backing up the program's locations, such as it's folder in `%ProgramFiles%` (should be `C:\Program Files`), `%ProgramData%` (should be `C:\ProgramData`), `%LocalAppData%` (Local AppData), and `%AppData%` (Roaming AppData), then after the backup, check if the program has a backup or export option. Then reinstall the program. We've even had success with just using the relevant program's backup and restore method and then reinstalling the program, but you should backup the app's data locations to be on the safe side. Such steps would take you less than 30 min and if the user really wants their name changed or it's in the best interest that it is changed, such as for standardization, etc.
* **Open an Issue Report:** We've never had a case of RenProfile not working but if it doesn't, feel free to open a discussion or issue report with the relevant details. When doing so, you will need to provide a good detailed description of the problem, screenshot snippet of any errors, and any relevant logs. This would be worst case scenario, as we have yet to come across any situations where renaming the Windows user's profile path did not work.

**Important Considerations for User Profile Paths:**
* **NTFS Permissions:** In rare cases, incorrect NTFS permissions on the profile folder can cause problems.  Check the permissions to ensure the user account has the necessary access rights. If permissions are suspected to be an issue, use a Take Onwership right-click registry file to create this option for you.
  * Check that your username has Full Control of the user folder. Go to `C:\Users` right-click on the user folder 🠆 Security 🠆 Edit 🠆 check `Replace all child object permission entries with inheritable permission entries from this object` and ensure that your username has full permissions, including all special permissions. Then click OK and OK. Windows should have these permissions set, but this will take Full Control of all files over again, which can ensure that any applications configuration data is owned by the user. You can also download a right-click Take Ownership registry modification and forcibly take onwership of the profile folder.
* **Special Characters:** Avoid using special characters in user profile names or paths.  This can also lead to compatibility problems.

**Reversing the Changes**
* **Run RenProfile Again (Reversal):** The quickest way to undo changes is to run RenProfile again, specifying the *old* profile path as the *new* path. This effectively reverses the initial operation.
* **System Restore (If Available):** If you have a recent system restore point created *before* running RenProfile, restoring to that point can revert all system changes, including registry modifications.  This is a more drastic step, but it can be effective.
