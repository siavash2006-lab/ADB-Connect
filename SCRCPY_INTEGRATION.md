# scrcpy integration

ADB Connect 1.6.3 adds a fixed global device selector plus a **TV Control** tab that
starts scrcpy inside a dedicated WinForms host window.

## Included behavior

- Lists online ADB devices in a fixed selector below Reboot and Recovery.
- Routes commands from every tab to the globally selected device.
- Starts only one scrcpy process at a time.
- Selects the device explicitly with `--serial`.
- Defaults to Original video size.
- Reads display 0 through the primary runtime's `--list-displays` command and falls
  back to `adb shell wm size` on vendor builds that do not return a display list.
- Compares that size with the `Texture: WIDTHxHEIGHT` reported by scrcpy.
- If an Original stream contains less than 20% of the expected pixel area, automatically
  restarts the same device with a bundled scrcpy 3.x compatibility runtime.
- The fallback is based on the selected device's live measurements, not an IP address,
  serial number, manufacturer, model or fixed resolution.
- Provides `1056 (TV Compatibility)`, which passes `--max-size 1056` (`-m1056`)
  for TV models that show a corrupted image at other sizes.
- Also supports 1920, 1280 and 1024 maximum video sizes.
- Disables audio by default; the operator may enable it before starting.
- Captures scrcpy stdout/stderr in the existing ADB Connect log.
- Embeds the native scrcpy window in a resizable WinForms Panel through the Win32
  `SetParent` API and re-embeds it after automatic compatibility fallback.
- Provides **Screenshot** and **Stop** on the host window toolbar; these buttons are
  intentionally removed from the TV Control tab.
- Stops scrcpy from the host toolbar, on disconnect, when the host closes, and when
  ADB Connect closes.
- Shows clear errors when the scrcpy runtime folder is missing.
- Captures screenshots from the visible host Panel with Windows `CopyFromScreen`.
  The toolbar is excluded from the PNG. This captures menus visible in scrcpy even when
  Android `screencap` cannot capture a hardware video/secure source layer.
- Captures the frame first, then opens a Save dialog. After the PNG is written, a
  confirmation message displays the final path; canceling the dialog discards the frame.
- Uses the same application icon as the main form on the embedded TV Control host.
- Disables toolbar item tooltips so hover help cannot appear in Windows screenshots.
- Clears the main IP TextBox only after a successful direct Connect or Pair & Connect.
- Uses a six-size transparent application icon and loads the executable icon at runtime,
  so the main form, embedded host form, executable and installer share the same icon.

## Add the official scrcpy runtime

Download the complete Windows packages only from:

https://github.com/Genymobile/scrcpy/releases

Extract them without removing any DLL, server, ADB, license or notice file:

- Current 32-bit package -> `scrcpy\win-x86`
- Current 64-bit package -> `scrcpy\win-x64`
- scrcpy 3.x 32-bit package -> `scrcpy\win-x86\compat`
- scrcpy 3.x 64-bit package -> `scrcpy\win-x64\compat`

scrcpy 3.3.4 is recommended for the compatibility folders. Do not mix files from
different releases in one folder; each `scrcpy.exe` must use its matching server and DLLs.

The project maps the selected runtime folder to `scrcpy` in the build/publish output.
The Apache 2.0 license and upstream notices must remain in the distributed package.

## Publish

Run `Publish-x86.cmd` or `Publish-x64.cmd`. Each script fails if the matching
primary or compatibility `scrcpy.exe` is not present in its output, which prevents
creating an incomplete setup.

After publishing, compile the matching Inno Setup script in `Installer`.

## Acceptance check

1. Connect a TV over USB or TCP/IP and confirm it is shown as `device` by ADB.
2. Press **Refresh** in the fixed Active Device panel and select the TV serial.
3. Open **TV Control**, keep Original selected and press **Start Control**.
4. On a normal device, verify that the primary runtime remains active.
5. On a device affected by the scrcpy 4.0 low-resolution regression, verify that the
   log reports the expected and received sizes, then starts the compatibility runtime.
6. For devices that require a fixed maximum size, select `1056 (TV Compatibility)`.
7. Verify that scrcpy is embedded in the new host form and accepts mouse and keyboard input.
8. Press **Stop** on the host toolbar, then repeat with Sound enabled on an Android 11+ device.
9. Close ADB Connect while scrcpy is running and confirm that the child process exits.
10. Press **Screenshot** on the host toolbar, select a PNG path and verify that only the
    visible scrcpy display area is saved (without the toolbar).

## Wireless Debugging pairing

Use **Pair Device** for devices that reject a direct `adb connect` request. The dialog
keeps these values separate:

- IP address
- Pairing Port from **Pair device with pairing code**
- 6-digit Pairing Code
- Connection Port from the main **Wireless Debugging** screen

The Pairing Port and Connection Port are not assumed to be equal. ADB Connect runs
`adb pair IP:PAIR_PORT`, writes the code through standard input without logging it,
then runs `adb connect IP:CONNECTION_PORT`. On success it refreshes the global Active
Device selector and selects the new device when its endpoint is listed by ADB.

Both port fields are TextBox controls with a five-character limit. Keyboard input and
pasted content are restricted to digits, and submission validates the range 1-65535.
The pairing-code TextBox is likewise limited to exactly six numeric characters.
