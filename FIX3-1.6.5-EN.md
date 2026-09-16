# 1.6.5 fix3 preview: packages remaining after uninstall

[فارسی](FIX3-1.6.5-FA.md)

The uninstall command targets Android user 0. The package list previously omitted a user, which can list packages belonging to other users on Android builds. This caused a removed package to reappear and subsequent uninstall attempts to report `not installed for 0`. Multiple selected packages are processed sequentially, not concurrently.

All, System and Third-party package lists now explicitly query installed packages for user 0, without `-u`. The cached list is cleared before refresh so failed refreshes cannot restore stale entries through search. Each successful uninstall is checked against the unfiltered installed-package list for user 0, and its result explicitly states that scope. This does not remove packages from other users or erase preloaded APKs from system partitions.

The preview is in `publish/v1.6.5-fix3/x64`. Published 1.6.5 release assets are unchanged. No uninstall command was run against a physical device during development.

## Device verification

1. Run the preview and refresh Apps Management. Previously removed packages should no longer appear for user 0.
2. Install two disposable test apps for user 0, select both, and uninstall them.
3. Confirm both results say `Uninstalled for user 0 (verified)`.
4. Refresh, change All/System/Third-party filters and search: the removed packages must remain absent.
5. If a removal fails, its result must identify that package and the error; other successful removals must remain absent.

Keep test apps backed up if needed: uninstall removes their user data. If a device still reproduces the issue, capture the current filter, result text and output of `adb -s SERIAL shell pm list packages --user 0` for diagnosis.
