# LD Hidden Control V0.1 Design

## Goal
Build a small Windows EXE that discovers running LDPlayer windows, binds each emulator instance to its own HWND, captures that HWND, and sends a hidden click to the selected instance without moving the physical Windows cursor.

## Origin of the mechanism
The supplied Than Long tool was found to use the KAutoHelper-style pattern `CaptureWindow(HWND)` plus `PostMessage(HWND, WM_LBUTTONDOWN/WM_LBUTTONUP, ...)`. V0.1 intentionally reproduces that mechanism for LDPlayer rather than using ADB, injection, ReadProcessMemory, or physical mouse APIs.

## Architecture
1. `LDWindowScanner` enumerates visible top-level Windows windows and identifies LDPlayer candidates by process/title/class.
2. For each candidate it enumerates descendant child windows and selects the largest likely render surface, preferring class/text containing render/player markers.
3. `KAutoCompat.CaptureWindow` captures the selected target HWND with `GetWindowDC + BitBlt`.
4. `KAutoCompat.SendClickOnPosition` posts `WM_ACTIVATE`, `WM_LBUTTONDOWN`, and `WM_LBUTTONUP` directly to the selected target HWND.
5. `MainForm` shows the bound instances, capture preview, PID/HWND/class, X/Y controls, and an event log.
6. Clicking the preview maps the PictureBox zoomed coordinate back to the captured image coordinate before sending the hidden click.

## Success criteria
- Two or more LDPlayer windows are listed independently.
- Selecting LD #2 and clicking sends input only to LD #2's target HWND.
- The physical Windows cursor does not move.
- The preview corresponds to the same HWND used for the click.
- Build produces one `AUTO-Gia-Lap-LD-v0.1.exe` targeting .NET Framework 4.8 x86.
- Self-tests cover LD window classification, preview coordinate mapping, and LPARAM packing.

## Deliberate V0.1 limitations
- No ADB binding yet.
- No game-specific image recognition or automation state machine yet.
- BitBlt capture can be black or stale for some minimized/hardware-rendered windows; the log must make this diagnosable.
- LDPlayer versions can use different child-window classes, so target selection is heuristic and the UI exposes both main and target HWND/class for diagnosis.

## Safety/diagnostics
The tool does not inject code, read/write game memory, install drivers, or send network traffic. It only enumerates Windows windows, captures a selected HWND, and posts mouse messages to that HWND.
