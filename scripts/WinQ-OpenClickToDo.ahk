#Requires AutoHotkey v2.0
#SingleInstance Force

InstallDir := A_ScriptDir
if RegExMatch(InstallDir, "\\scripts$")
    InstallDir := RegExReplace(InstallDir, "\\scripts$")

ExePath := InstallDir "\OpenClickToDo.exe"
SetWorkingDir InstallDir

#q:: {
    global ExePath, InstallDir

    if !FileExist(ExePath) {
        MsgBox "OpenClickToDo.exe was not found.`n`nExpected path:`n" ExePath, "Open Click to Do", "Icon!"
        return
    }

    try {
        Run '"' ExePath '" --capture', InstallDir
    } catch as err {
        MsgBox "Open Click to Do could not start.`n`n" err.Message, "Open Click to Do", "Icon!"
    }
}

