# AUTO-Gia-Lap-LD

V0.1 là tool test độc lập cho **LDPlayer hidden control**. Mục tiêu duy nhất của bản này là xác minh cơ chế đã bóc từ tool Thần Long: bind từng giả lập bằng `HWND`, chụp riêng HWND đó và gửi click ẩn bằng `PostMessage` mà không di chuyển chuột Windows.

## Cơ chế V0.1

- Quét các cửa sổ LDPlayer (`dnplayer` / title hoặc class LDPlayer).
- Lưu `PID`, `Main HWND`, `Target HWND`, class của render target và kích thước.
- Ưu tiên child window có dấu hiệu `render/surface/opengl/subwin/player`; nếu không có thì chọn child lớn nhất đủ diện tích.
- Capture đúng `Target HWND` bằng `GetWindowDC + BitBlt`.
- Click đúng `Target HWND` bằng:
  - `WM_ACTIVATE`
  - `WM_LBUTTONDOWN`
  - `WM_LBUTTONUP`
- Không dùng `SetCursorPos`, `mouse_event`, `SendInput`, ADB, inject DLL hay đọc/ghi RAM game trong V0.1.

## Cách test

1. Mở 2 hoặc nhiều tab/cửa sổ LDPlayer và vào game.
2. Chạy `AUTO-Gia-Lap-LD-v0.1.exe`.
3. Bấm **Refresh LD**. Mỗi LD phải hiện thành một dòng riêng với PID/HWND.
4. Chọn một LD, bấm **Capture Target**.
5. Click lên ảnh preview để tool tự lấy X/Y của điểm tương ứng.
6. Chọn một nút vô hại trong game rồi bấm **Hidden Click**.
7. Kiểm tra:
   - chỉ LD đang chọn nhận click;
   - LD khác không nhận click;
   - con trỏ Windows không di chuyển;
   - log ghi `physicalCursorUnchanged=True`.
8. Có thể bật **Click ngay trên preview** để click trực tiếp theo điểm trên ảnh capture.

## Điều cần gửi lại nếu không hoạt động

Chụp màn hình tool sao cho thấy hàng LD đang chọn và log. Đặc biệt cần các giá trị `Main HWND`, `Target HWND`, `Target class`, `Size`, và dòng log khi Capture/Hidden Click. Từ đó có thể xác định LD version của bạn dùng child render class nào.

## Giới hạn V0.1

- Capture `BitBlt` có thể đen hoặc cũ nếu một số phiên bản LDPlayer dùng hardware surface đặc biệt hoặc cửa sổ bị minimize.
- `PostMessage` có thể cần gửi tới một child render khác ở một số bản LDPlayer; vì vậy V0.1 hiển thị đầy đủ target để chẩn đoán.
- Chưa có ADB fallback, auto scan game, OCR hay logic Thần Long.

## Build

Project: C# WinForms, .NET Framework 4.8, x86.

GitHub Actions build Release, chạy `--self-test`, sau đó mới tạo artifact `AUTO-Gia-Lap-LD-v0.1-win-x86` chứa `AUTO-Gia-Lap-LD-v0.1.exe`.

Thiết kế và implementation plan nằm trong `docs/superpowers/`.
