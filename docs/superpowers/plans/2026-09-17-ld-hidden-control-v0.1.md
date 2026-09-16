# LD Hidden Control V0.1 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build and verify a Windows EXE that discovers LDPlayer instances and sends hidden clicks to the selected instance by HWND without moving the physical cursor.

**Architecture:** Enumerate LDPlayer top-level windows, choose a likely render child HWND, capture and click that exact HWND, and expose all bindings in a WinForms diagnostic UI. Keep the first version independent of ADB and game memory access so the HWND mechanism can be validated in isolation.

**Tech Stack:** C# / .NET Framework 4.8 / WinForms / user32.dll / gdi32.dll / GitHub Actions Windows runner.

**Spec:** `docs/superpowers/specs/2026-09-17-ld-hidden-control-design.md`

## Global Constraints
- Target .NET Framework 4.8 x86.
- Produce one EXE with no external NuGet/runtime package dependency beyond .NET Framework 4.8.
- Do not use `SetCursorPos`, `mouse_event`, or `SendInput` for the V0.1 click path.
- Do not use ADB, process injection, ReadProcessMemory, or WriteProcessMemory in V0.1.
- Capture and click must use the same target HWND.

---

### Task 1: Contract tests

**Files:**
- Modify: `src/AutoGiaLapLD/SelfTests.cs`
- Modify: `src/AutoGiaLapLD/Core/LDProcessClassifier.cs`
- Modify: `src/AutoGiaLapLD/Core/PreviewCoordinateMapper.cs`

**Interfaces:**
- Produces: `LDProcessClassifier.IsLDPlayerWindow(...)`, `PreviewCoordinateMapper.TryMap(...)`.

- [x] **Step 1: Write failing self-tests** for LD classification and PictureBox Zoom coordinate mapping.
- [x] **Step 2: Run CI and verify RED.** Build must succeed and the self-test step must fail because the production methods are stubs.
- [ ] **Step 3: Implement the minimal classifier and mapper.**
- [ ] **Step 4: Run CI and verify the contract tests pass.**

### Task 2: HWND discovery and target selection

**Files:**
- Create: `src/AutoGiaLapLD/Interop/NativeMethods.cs`
- Create: `src/AutoGiaLapLD/Models/LDInstance.cs`
- Create: `src/AutoGiaLapLD/Services/LDWindowScanner.cs`

**Interfaces:**
- Produces: `List<LDInstance> LDWindowScanner.Scan()` where each instance exposes main HWND, target HWND, PID, process name, title, classes, and target size.

- [ ] **Step 1:** Enumerate visible top-level windows with `EnumWindows` and resolve process IDs/names.
- [ ] **Step 2:** Filter LDPlayer candidates using the tested classifier.
- [ ] **Step 3:** Enumerate descendant child windows and score likely render targets by size plus render/player class markers.
- [ ] **Step 4:** Keep main and selected target HWND metadata for diagnostics.

### Task 3: KAuto-style capture and hidden click

**Files:**
- Create: `src/AutoGiaLapLD/Services/KAutoCompat.cs`
- Modify: `src/AutoGiaLapLD/SelfTests.cs`

**Interfaces:**
- Produces: `Bitmap KAutoCompat.CaptureWindow(IntPtr hwnd)` and `bool KAutoCompat.SendClickOnPosition(IntPtr hwnd, int x, int y)`.

- [ ] **Step 1:** Add a failing LPARAM packing test.
- [ ] **Step 2:** Implement `MakeLParamFromXY` and verify the test passes.
- [ ] **Step 3:** Implement capture with `GetWindowDC + CreateCompatibleDC + CreateCompatibleBitmap + BitBlt`.
- [ ] **Step 4:** Implement hidden click with `PostMessage(WM_ACTIVATE/WM_LBUTTONDOWN/WM_LBUTTONUP)` and no physical cursor APIs.

### Task 4: Diagnostic WinForms UI

**Files:**
- Create: `src/AutoGiaLapLD/UI/MainForm.cs`
- Modify: `src/AutoGiaLapLD/Program.cs`

**Interfaces:**
- Consumes: `LDWindowScanner`, `KAutoCompat`, `PreviewCoordinateMapper`.

- [ ] **Step 1:** Build a process/handle grid with refresh and current-target diagnostics.
- [ ] **Step 2:** Add capture preview for the selected target HWND.
- [ ] **Step 3:** Add X/Y controls and `Hidden Click` button.
- [ ] **Step 4:** Map preview clicks to capture coordinates and optionally send the click.
- [ ] **Step 5:** Log target HWND, coordinates, PostMessage result, and whether the physical cursor position changed.

### Task 5: Build, artifact, and documentation

**Files:**
- Modify: `README.md`
- Modify: `.github/workflows/build.yml`

**Interfaces:**
- Produces: GitHub Actions artifact `AUTO-Gia-Lap-LD-v0.1-win-x86` containing `AUTO-Gia-Lap-LD-v0.1.exe`.

- [ ] **Step 1:** Build Release on `windows-latest` with MSBuild.
- [ ] **Step 2:** Run `--self-test` and block the artifact on failure.
- [ ] **Step 3:** Upload the EXE as a workflow artifact.
- [ ] **Step 4:** Document the two-LD test procedure and V0.1 limitations.
- [ ] **Step 5:** Verify final CI is green, inspect artifact, create PR, and merge to `main`.
