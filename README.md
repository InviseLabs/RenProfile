# RenProfile
### Tool to Rename Profile Folder Path in Windows Without Creating New User Account
#### Rename Windows Profile Path - Registry Find and Replace All

RenProfile is a tool to rename the Windows profile folder path of your user account. Run from a separate administrator account. It recursively performs a `find and replace all` operation within the registry – a method I developed for this specific purpose – changing every instance of your old profile path to your new profile path. **Sounds dangerous, and it might be, but I've tested this successfully on Windows 11.**

Technically, there's no reason why this *should* cause problems, except in poorly written programs that don't properly check the user account path using relevant APIs (like `%userprofile%`) or programs that hard-code this path in settings files (which no program *should* ever do). While some developers might follow these bad practices, in my testing, this seems rare. (Windows Server environments, for example, offer built-in user profile migration.) In such cases, you might need to inform the affected program of the new path.

**If RenProfile doesn't work or causes problems, follow these troubleshooting steps:**

<br/>

## Troubleshooting Steps
* **Double-Check Paths:** Carefully verify the old and new profile paths you entered. Even a small typo can cause issues. The paths should be the full, absolute paths (e.g., `C:\Users\OldUsername` and `C:\Users\NewUsername`), not relative paths.
* **Run RenPrfoile Again:** It won't harm the system to reboot and run it again to ensure that all registry paths have been successfully modified. You could also use `msconfig`, select 🠆 Boot 🠆 Boot options 🠆 Safe mode 🠆 Network, which will reboot into Safe Mode with Networking, and from there you can run RenProfile again without having to worry about locking programs.
* **Check for Open Files/Processes:** Before running RenProfile, close all programs running under the user account you're modifying, then reboot. Open files or running processes can lock registry keys and prevent changes. Use Task Manager to ensure no lingering processes are associated with the target user. 
* **Check Event Viewer:** Look in the Windows Event Viewer for any errors or warnings related to user profiles, applications, or the registry around the time you ran RenProfile.  This can give clues about the cause of the problem.
* **Hidden Files/Folders:** Some profile data might be stored in hidden locations.
* **Long Paths:** Be mindful of long file paths. Windows has limitations on path lengths, so extremely long profile paths can cause issues. Might also be helpful to enable Long Path support in Windows to be sure this isn't causing you an issue.
* **Reinstall the Problematic Application:** If a specific application is misbehaving after renaming the profile, check their documentation, but we at Invise Labs and Invise Solutions, have had success with backing up the program's locations, such as it's folder in `Program Files`, `C:\ProgramData`, `%localappdata%`, and `%appdata` (roaming), then after the backup, check if the program has a backup or export option. Then reinstall the program. We've even had success with just using the relevant program's backup and restore method and then reinstalling the program. Such steps would take you less than 30 min and if the user really wants their name changed or it's in the best interest that it is changed, for standardization, then this one of the best methods to go about this.

**Important Considerations for User Profile Paths:**
* **NTFS Permissions:** In rare cases, incorrect NTFS permissions on the profile folder can cause problems.  Check the permissions to ensure the user account has the necessary access rights. If permissions are suspected to be an issue, use a Take Onwership right-click registry file to create this option for you. You can also change the owner to `Everyone` and add `Everyone` with full control, then after resolving the issues, remove `Everyone` and add the new username as the Onwer and Full Control. It's bad practice to leave any user profile with Full Control by `Everyone`.
* **Special Characters:** Avoid using special characters in user profile names or paths.  This can also lead to compatibility problems.

**Reversing the Changes**
* **Run RenProfile Again (Reversal):** The quickest way to undo changes is to run RenProfile again, specifying the *old* profile path as the *new* path. This effectively reverses the initial operation.
* **System Restore (If Available):** If you have a recent system restore point created *before* running RenProfile, restoring to that point can revert all system changes, including registry modifications.  This is a more drastic step, but it can be effective.
