# Second 1.6.5 fix

[فارسی](FIX2-1.6.5-FA.md)

Cancel operation no longer appears for Properties, app installation/uninstallation/launch, connection, result dialogs or connection revalidation. It is visible and clickable only during actual Bugreport generation. It is hidden on completion, failure or cancellation, before a result or save dialog opens.

Historical portable builds: `publish/v1.6.5-fix2/x64` and `publish/v1.6.5-fix2/x86`. These include fix1's sound/display and layout changes. The installers available at that time did not include fix2; the final 1.6.5 packages subsequently incorporate both fixes.

Validation: both architectures were published. User acceptance check: opening Properties and Uninstall results must not leave a cancel button behind the dialog. Bugreport cancellation is available only while generation is running.
