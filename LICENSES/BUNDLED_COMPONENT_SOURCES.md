# Bundled component source references

ADB Connect release packages may include the following prebuilt components.
The source versions below correspond to the official scrcpy Windows runtimes
used by ADB Connect 1.6.4.

## Primary scrcpy 4.0 runtime

| Component | Version | Official source | SHA-256 when fixed by the scrcpy build |
| --- | --- | --- | --- |
| scrcpy | 4.0 | <https://github.com/Genymobile/scrcpy/releases/tag/v4.0> | See the signed scrcpy release |
| FFmpeg | 8.1.1 | <https://ffmpeg.org/releases/ffmpeg-8.1.1.tar.xz> | `b6863adde98898f42602017462871b5f6333e65aec803fdd7a6308639c52edf3` |
| SDL | 3.4.8 | <https://github.com/libsdl-org/SDL/releases/tag/release-3.4.8> | `429a9f38483f834da5727a63dd7b5127a7c9d06a16439d7c6de4b7ebbfdb6374` |
| libusb | 1.0.29 | <https://github.com/libusb/libusb/releases/tag/v1.0.29> | `7c2dd39c0b2589236e48c93247c986ae272e27570942b4163cb00a060fcf1b74` |
| dav1d | 1.5.3 | <https://code.videolan.org/videolan/dav1d/-/tags/1.5.3> | `cbe212b02faf8c6eed5b6d55ef8a6e363aaab83f15112e960701a9c3df813686` |
| Android Platform-Tools | 37.0.0 | <https://dl.google.com/android/repository/platform-tools_r37.0.0-win.zip> | `4fe305812db074cea32903a489d061eb4454cbc90a49e8fea677f4b7af764918` |

## Compatibility scrcpy 3.3.4 runtime

| Component | Version | Official source | SHA-256 when fixed by the scrcpy build |
| --- | --- | --- | --- |
| scrcpy | 3.3.4 | <https://github.com/Genymobile/scrcpy/releases/tag/v3.3.4> | See the signed scrcpy release |
| FFmpeg | 7.1.1 | <https://ffmpeg.org/releases/ffmpeg-7.1.1.tar.xz> | `733984395e0dbbe5c046abda2dc49a5544e7e0e1e2366bba849222ae9e3a03b1` |
| SDL | 2.32.8 | <https://github.com/libsdl-org/SDL/releases/tag/release-2.32.8> | `dd35e05644ae527848d02433bec24dd0ea65db59faecf1a0e5d1880c533dac2c` |
| libusb | 1.0.29 | <https://github.com/libusb/libusb/releases/tag/v1.0.29> | `7c2dd39c0b2589236e48c93247c986ae272e27570942b4163cb00a060fcf1b74` |
| dav1d | 1.5.0 | <https://code.videolan.org/videolan/dav1d/-/tags/1.5.0> | `78b15d9954b513ea92d27f39362535ded2243e1b0924fde39f37a31ebed5f76b` |
| Android Platform-Tools | 36.0.0 | <https://dl.google.com/android/repository/platform-tools_r36.0.0-win.zip> | `12c2841f354e92a0eb2fd7bf6f0f9bf8538abce7bd6b060ac8349d6f6a61107c` |

The release process downloads the unmodified FFmpeg 8.1.1 and 7.1.1 source
archives, verifies the hashes above and publishes them beside the binary release
assets. The relevant scrcpy build definitions are available in the corresponding
scrcpy source tags under `app/deps/`.
