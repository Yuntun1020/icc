# Screen Exposure

Windows desktop tool for game-oriented screen tone adjustment. It provides a Photoshop-like curve editor, exposure, contrast, and gamma controls, then applies the result to the active display through the Windows gamma ramp. It can also export a derived ICC profile with a `vcgt` calibration table and associate it with the selected display.

## Features

- RGB master curve plus per-channel R, G, and B curves
- Exposure stop, contrast, and gamma controls
- Realtime Windows `SetDeviceGammaRamp` application
- Restore original gamma ramp on demand or process exit
- Outdoor dimming, night, and default presets
- ICC export based on the system sRGB profile with a generated `vcgt` tag
- Display selection for ICC association

## 中文文档

中文使用说明见 [docs/zh-CN.md](docs/zh-CN.md)。

## Run

```powershell
dotnet run --project .\ScreenExposure.App\ScreenExposure.App.csproj
```

## Build And Test

```powershell
dotnet build .\ScreenExposure.slnx
dotnet test .\ScreenExposure.slnx
```

## Package Single EXE

```powershell
.\build-single-exe.ps1
```

The distributable files are written to `dist/`:

- `ScreenExposure.App.exe`
- `SHA256SUMS.txt`

## Notes

- Disable HDR before testing. HDR, driver color controls, protected output, and exclusive fullscreen games may ignore or override the gamma ramp.
- ICC profiles are not realtime post-processing filters. The app applies realtime changes through the gamma ramp, then writes the same ramp into ICC `vcgt` when saving.
- Saving ICC profiles writes to the Windows color profile directory and may require user permissions depending on system policy.
- If the screen looks wrong, click **恢复** or close the app. The app keeps the original gamma ramp in memory and attempts to restore it on exit.

## Open Source References

This implementation follows the same capability boundaries used by tools such as DisplayCAL/ArgyllCMS: realtime display calibration is done through video LUT/gamma ramp, while ICC `vcgt` stores calibration data for color-management-aware workflows. It does not copy external project source code.
