# ADB Connect release roadmap

[فارسی](RELEASE-ROADMAP-FA.md)

Baseline: 1.6.4. Order agreed on 2026-09-13. This replaces the version sequence in LAB-UPGRADE-PLAN-EN.md; its original code findings remain useful. This is a development plan, not a claim that future features have shipped. See CHANGELOG-1.6.5.md for the implemented maintenance release.

## Shared rules

- Retain WinForms and the existing product direction.
- Keep Windows-based screenshots. Record the image displayed in Windows; neither Android-side recording nor scrcpy's own recording option is the basis of this plan.
- Bind every operation to a fixed device serial from the first stage; introduce the multi-device interface at its scheduled stage.
- Record device identity, timestamp, artifact type and unique output filenames from the start so sessions can be added without rewriting the tools.
- Versions are proposed milestones. Set reliable dates after prototyping capture and testing the reference computer/device.

## 1.6.5 — Fix existing problems

Scope: truthful per-app install/uninstall/start/stop results, partial-success reporting and package refresh; separate connection and busy state; revalidate after reboot/disconnection and retain the target throughout an operation; consistent errors, timeouts, cancellation and owned-process cleanup; remove or consolidate the unused AdbProgressRunner; correct cross-thread Logcat access, bound display buffering and continuously save raw logs independently of display filters; separate clearing the view and device buffer; align installer discovery, signing documentation and versions; use consistent ADB for the application and scrcpy; extract only the command/device foundations needed for later work, without a full UI rewrite.

Acceptance: failed deletion never reports success, disconnection never changes the target, one hour of dense logging does not freeze the UI, closing the app terminates its processes, and existing features plus x64/x86 installation/upgrade are tested.

## 1.7.0 — Windows screen recording for one device

- Start/Stop Recording, elapsed time and recording state, output selection and opening the output folder.
- Initial target: MP4/H.264 at proposed 15/30 fps, fixed output dimensions during recording, preserved aspect ratio and defined resize behavior.
- Screenshot while recording, disk-space/encoder/disconnection handling, and finalization on Stop or normal window closure.
- Initially record video without audio. System audio and device-specific audio are separate requirements; unrelated Windows applications must not be recorded accidentally.

Initial technical investigation:

1. Build a small Windows video-area capture prototype that produces a playable file.
2. CopyFromScreen captures visible desktop coordinates. An overlapping window enters the recording; minimization and locking require detection/testing. Explain the limitation in the UI.
3. Evaluate Windows.Graphics.Capture for window frames, including embedded scrcpy, cropping, Windows versions and x86. Replacing existing Screenshot is outside this stage. Record the backend decision after testing; do not promise covered/minimized-window capture beforehand.
4. Evaluate a Windows encoder first. FFmpeg DLLs bundled with scrcpy do not imply an independent ffmpeg.exe is present. Evaluate and package any added dependency explicitly.
5. On device loss, scrcpy exit or compatibility fallback, stop or segment recording explicitly; never silently present a frozen frame as healthy recording.

Acceptance: a 30-minute reference recording is playable with correct duration; screenshots work during capture; resize, occlusion, minimization, disconnection and normal stop have defined behavior. Recovery from an abrupt crash is not guaranteed by MP4 generally and needs its own test.

## 1.8.0 — Device file manager

- Browse ADB-accessible folders with current path, Back, Refresh and available name/type/size/time metadata.
- Pull to PC and Push to device, multi-selection, queued transfers, genuine progress where available, cancellation and per-file results.
- Create folders, rename and delete, displaying exact device/path and confirming deletion; define filename-conflict behavior.
- Support Persian names, spaces and special characters; distinguish Windows and Android paths and validate/escape shell arguments.
- Use temporary files and final replacement after success where supported; identify incomplete files on failure. Resume is not promised in the first version.
- Clearly report Permission denied without assuming root or universal system-folder access; identify symlinks to avoid unintended traversal.

Acceptance: test small/large transfers with hashes where available, Unicode/spaces, conflicting names, insufficient space and interruptions. Failed transfers must not report success or silently replace existing files. Test recording and Logcat during transfers.

## 1.9.0 — Multi-device screenshots and recording

- Separate DeviceContext per device: serial, ScrcpyRunner, window, recorder, state and output path.
- Multiple control windows with independent Screenshot and Start/Stop Recording.
- Group screenshots and Start/Stop for selected devices, with independent timestamps and results.
- Concurrent means near-simultaneous starts, not frame synchronization; record start offsets.
- Keep file-manager targets explicit. Direct device-to-device transfer is not included.
- Per-device mutation queues, independent log/capture streams, and concurrency limits matching PC capacity.
- Start with two devices, then test four on reference hardware. Determine supported capacity from CPU/GPU/RAM/disk measurements.
- With desktop-region capture, arrange visible, nonoverlapping windows. Offer covered-window capture only after verifying a window backend. Existing Screenshot has the same limitation; grouping alone does not remove it.

Acceptance: two independent 30-minute recordings with screenshots; stopping/disconnecting one does not affect the other; evidence is never assigned to another device; resizing, movement, dropped frames and resource contention are handled and reported.

## 2.0.0 — Laboratory sessions

Create sessions with name, devices, firmware, app under test, notes and manual result. Attach logs, screenshots, videos and Bugreports; provide history, search and HTML/ZIP reports. Transferred files should be optional attachments, not automatically treated as evidence. Make data location configurable and independent of installation.

Acceptance: reopening the app restores sessions and artifact references, evidence never mixes between sessions, and upgrades preserve old data.

## 2.1.0 — Repeatable scenarios

Versioned steps with preconditions, waits, timeouts, cancellation, repetition counts and failure evidence. Begin with reboot and app launch/stop loops. Report Passed/Failed/Error/Skipped/Canceled. Use 1.9's group execution and 2.0's sessions. Define measurement methods before specialized AV or external-equipment tests.

Acceptance: 20 cycles on a reference device with per-cycle results; cancellation preserves incomplete results; uncertain mutating steps are not automatically repeated after a crash.

## Routine for each release

1. Freeze scope and acceptance criteria; retain a recoverable baseline and track source changes.
2. Prototype uncertain components, then implement small reviewable changes.
3. Build, run focused automated checks for sensitive logic and test actual devices.
4. Recheck previous features, especially Pairing, Logcat, Screenshot and scrcpy fallback.
5. Produce a laboratory test build, record problems and fix release blockers.
6. Produce x64/x86 outputs, test installation/upgrades, document changes and release after acceptance.

## Windows capture references

- https://learn.microsoft.com/en-us/windows/uwp/audio-video-camera/screen-capture
- https://learn.microsoft.com/en-us/windows/uwp/audio-video-camera/screen-capture-video
- https://github.com/MicrosoftDocs/SimpleRecorder
