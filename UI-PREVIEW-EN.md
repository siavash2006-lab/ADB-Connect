# Compact UI and theme preview 3

[فارسی](UI-PREVIEW-FA.md)

The main window has been reduced from 720 × 790 to 620 × 646 logical pixels. Connections, power controls, properties, app actions and log filters now use aligned rows with smaller gaps.

Three icon buttons provide System (monitor, default), Light (sun) and Dark (moon), with tooltips and a visible selection indicator. Tabs and device dropdowns use muted surfaces and subtle borders instead of raised white frames. System follows Windows' app color preference and responds to preference-change notifications. Explicit choices are stored in %LOCALAPPDATA%/ADB Connect/theme.txt. Pairing, TV toolbar, property/results windows and confirmations share the theme. Native Windows file pickers retain their system-managed appearance.

Run publish/v1.6.5-ui-preview3/x64/ADB Connect.exe with its accompanying folders. This preview includes fix3; released assets are unchanged.

Validation: both palettes rendered across all four tabs, pairing, results, confirmations and the TV toolbar (without launching scrcpy). Live theme propagation and the default No choice for destructive confirmations were checked. A 150% programmatic layout scaling check was also rendered; this does not replace physical multi-monitor DPI testing. All 23 existing regression checks passed. Device operations were not exercised.


## Preview 4: inline properties
All 27 property actions display values, progress and errors inside the Properties tab. Results include the queried device, support selection, Copy and Save, and scroll for long values. Read All Properties now reads first; saving is optional. Run publish/v1.6.5-ui-preview4/x64/ADB Connect.exe. Short and long sample values were rendered in both themes; physical-device queries still need verification.


## Preview 5: single theme toggle
One sun/moon button shows the effective current theme and toggles to the opposite theme. On first use, Windows supplies the theme; after a manual toggle the choice is saved. No computer icon is shown. Build: publish/v1.6.5-ui-preview5/x64/ADB Connect.exe.
