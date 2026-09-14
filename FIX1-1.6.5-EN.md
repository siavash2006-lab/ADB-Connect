# UI and log display fix for 1.6.5

[فارسی](FIX1-1.6.5-FA.md)

Historical test build: `publish/v1.6.5-fix1/x64` and `publish/v1.6.5-fix1/x86`, with informational version `1.6.5-fix1`. At the time of this fix, installers and `publish/v1.6.5` still contained the previous build. The final 1.6.5 packages subsequently incorporate this fix and fix2.

- Removed the extra strip below the main form and retained its original size.
- Moved Clear Device... into the Log tab alongside other log actions; confirmation is still required.
- At this stage Cancel operation appeared in the middle of the existing footer while an operation was active. Fix2 later restricted it to actual Bugreport generation.
- Text/color changes and RichTextBox trimming now happen in a synchronous scope that temporarily lifts ReadOnly and restores it in finally. No user input is processed during that scope. This addresses the suspected RichEdit warning path; it does not mute Windows sounds.
- scrcpy messages enter a bounded display queue instead of scheduling one BeginInvoke per message.
- Undo history is cleared after display updates. Raw Logcat storage is unchanged.

Validation at the time: the 18 earlier regression assertions passed. A separate WinForms test checked 60,000 lines, a 2,000-line limit, character limits, restored ReadOnly and no undo growth. It ran without showing a window or connecting to Android; audible behavior had not yet been confirmed. The user subsequently reported that the fix worked.

Reproduction check: close the previous build, run the new EXE, continue Logcat past 2,000 lines and test Start Control. If the sound recurs, report whether it happens without clicking/typing or only during interaction, and whether Log contains a new error.
