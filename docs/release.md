# Release Notes

## Screen Exposure

This package contains a Windows x64 single-file executable:

- `ScreenExposure.App.exe`
- `SHA256SUMS.txt`

Run the executable directly. The app starts in a neutral state and does not write a display curve until you click **应用** or a preset.

Important notes:

- Use **恢复** before closing if you want to restore the original gamma ramp immediately.
- The app also tries to restore the original gamma ramp on process exit.
- HDR, driver-level color controls, protected output, and exclusive fullscreen games may ignore or override gamma ramp changes.
- ICC saving writes a derived profile into the Windows color profile directory and may require user permissions.
