# RenProfile

A tool to rename the profile folder of your user account within Windows. Run from separate Admin account. **It recursively performs a `find and replace all` operation within the registry** – a method I developed for this specific purpose – in which it changes every mention of your old profile path to your new profile path. **Sounds dangerous, and it might be, but I tested this as working on Windows 11.**

Technically, there is no reason why this should cause any problems, except in poorly written programs that do not check the user account path or `%userprofile` or later hard-code this path in a settings file – which no program should ever do, but I suppose there will be some programmers out there that may do this. In such events, it would likely be as easy as letting the program know the new path. **If it doesn't work, or causes you problems, you can always use RenProfile and reverse the changes you made.**
