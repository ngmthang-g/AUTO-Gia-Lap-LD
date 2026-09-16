using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using AutoGiaLapLD.Core;
using AutoGiaLapLD.Interop;
using AutoGiaLapLD.Models;
using AutoGiaLapLD.Services;

namespace AutoGiaLapLD.UI
{
    internal sealed class MainForm : Form
    {
        private readonly LDWindowScanner _scanner = new LDWindowScanner();

        private readonly DataGridView _grid = new DataGridView();
        private readonly PictureBox _preview = new PictureBox();
        private readonly NumericUpDown _x = new NumericUpDown();
        private readonly NumericUpDown _y = new NumericUpDown();
        private readonly TextBox _log = new TextBox();
        private readonly Label _selectedInfo = new Label();
        private readonly CheckBox _clickOnPreview = new CheckBox();
        private readonly CheckBox _captureAfterClick = new CheckBox();

        public MainForm()
        {
            Text = "AUTO Gia Lap LD v0.1 - HWND Hidden Click Tester";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1050, 700);
            Size = new Size(1320, 840);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

            BuildUi();

            Shown += delegate
            {
                AppendLog("Ready. V0.1 uses HWND + BitBlt + PostMessage only; no physical mouse movement.");
                RefreshInstances();
            };

            FormClosed += delegate
            {
                if (_preview.Image != null)
                {
                    _preview.Image.Dispose();
                    _preview.Image = null;
                }
            };
        }

        private void BuildUi()
        {
            var root = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 590,
                FixedPanel = FixedPanel.Panel1
            };
            Controls.Add(root);

            var left = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(8)
            };
            left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            left.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            left.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            left.RowStyles.Add(new RowStyle(SizeType.Absolute, 170F));
            root.Panel1.Controls.Add(left);

            var topButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = false,
                Padding = new Padding(0, 0, 0, 6)
            };
            var refresh = new Button { Text = "Refresh LD", AutoSize = true };
            var capture = new Button { Text = "Capture Target", AutoSize = true };
            refresh.Click += delegate { RefreshInstances(); };
            capture.Click += delegate { CaptureSelected(); };
            topButtons.Controls.Add(refresh);
            topButtons.Controls.Add(capture);
            left.Controls.Add(topButtons, 0, 0);

            ConfigureGrid();
            left.Controls.Add(_grid, 0, 1);

            _selectedInfo.Dock = DockStyle.Fill;
            _selectedInfo.AutoSize = true;
            _selectedInfo.Padding = new Padding(0, 6, 0, 6);
            _selectedInfo.Text = "No LDPlayer selected.";
            left.Controls.Add(_selectedInfo, 0, 2);

            var actions = BuildActions();
            left.Controls.Add(actions, 0, 3);

            _log.Dock = DockStyle.Fill;
            _log.Multiline = true;
            _log.ReadOnly = true;
            _log.ScrollBars = ScrollBars.Vertical;
            _log.BackColor = Color.White;
            left.Controls.Add(_log, 0, 4);

            var right = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(8)
            };
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.Panel2.Controls.Add(right);

            _preview.Dock = DockStyle.Fill;
            _preview.BackColor = Color.FromArgb(28, 28, 28);
            _preview.BorderStyle = BorderStyle.FixedSingle;
            _preview.SizeMode = PictureBoxSizeMode.Zoom;
            _preview.MouseClick += Preview_MouseClick;
            right.Controls.Add(_preview, 0, 0);

            var help = new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                Padding = new Padding(0, 8, 0, 0),
                Text = "Click vào ảnh preview để lấy X/Y. Bật 'Click ngay trên preview' nếu muốn gửi hidden click ngay.\r\n" +
                       "Nên test trước bằng một nút vô hại trong game. Con trỏ Windows phải đứng nguyên."
            };
            right.Controls.Add(help, 0, 1);
        }

        private void ConfigureGrid()
        {
            _grid.Dock = DockStyle.Fill;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AllowUserToResizeRows = false;
            _grid.ReadOnly = true;
            _grid.MultiSelect = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.AutoGenerateColumns = false;
            _grid.RowHeadersVisible = false;

            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Title", HeaderText = "LD / Title", Width = 145 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "PID", HeaderText = "PID", Width = 60 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Main", HeaderText = "Main HWND", Width = 95 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Target", HeaderText = "Target HWND", Width = 95 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Class", HeaderText = "Target class", Width = 115 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Size", HeaderText = "Size", Width = 70 });

            _grid.SelectionChanged += delegate { UpdateSelectedInfo(); };
        }

        private Control BuildActions()
        {
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = true,
                Padding = new Padding(0, 4, 0, 4)
            };

            panel.Controls.Add(new Label { Text = "X:", AutoSize = true, Margin = new Padding(0, 8, 3, 0) });
            _x.Minimum = 0;
            _x.Maximum = 65535;
            _x.Width = 75;
            panel.Controls.Add(_x);

            panel.Controls.Add(new Label { Text = "Y:", AutoSize = true, Margin = new Padding(10, 8, 3, 0) });
            _y.Minimum = 0;
            _y.Maximum = 65535;
            _y.Width = 75;
            panel.Controls.Add(_y);

            var click = new Button { Text = "Hidden Click", AutoSize = true, Margin = new Padding(12, 3, 3, 3) };
            click.Click += delegate { PerformHiddenClick(); };
            panel.Controls.Add(click);

            _clickOnPreview.Text = "Click ngay trên preview";
            _clickOnPreview.AutoSize = true;
            _clickOnPreview.Margin = new Padding(12, 7, 3, 3);
            panel.Controls.Add(_clickOnPreview);

            _captureAfterClick.Text = "Capture sau click";
            _captureAfterClick.AutoSize = true;
            _captureAfterClick.Margin = new Padding(12, 7, 3, 3);
            panel.Controls.Add(_captureAfterClick);

            return panel;
        }

        private void RefreshInstances()
        {
            List<LDInstance> instances;
            try
            {
                instances = _scanner.Scan();
            }
            catch (Exception ex)
            {
                AppendLog("Refresh failed: " + ex.Message);
                return;
            }

            IntPtr previouslySelected = IntPtr.Zero;
            LDInstance old = GetSelectedInstance();
            if (old != null) previouslySelected = old.MainHandle;

            _grid.Rows.Clear();
            int selectIndex = -1;

            for (int i = 0; i < instances.Count; i++)
            {
                LDInstance item = instances[i];
                int index = _grid.Rows.Add(
                    item.Title,
                    item.ProcessId,
                    item.MainHandleHex,
                    item.TargetHandleHex,
                    item.TargetClass,
                    item.TargetSize);
                _grid.Rows[index].Tag = item;
                if (item.MainHandle == previouslySelected) selectIndex = index;
            }

            if (_grid.Rows.Count > 0)
            {
                if (selectIndex < 0) selectIndex = 0;
                _grid.ClearSelection();
                _grid.Rows[selectIndex].Selected = true;
                _grid.CurrentCell = _grid.Rows[selectIndex].Cells[0];
            }

            AppendLog("Refresh: found " + instances.Count + " LDPlayer top-level window(s).");
            UpdateSelectedInfo();
        }

        private void UpdateSelectedInfo()
        {
            LDInstance item = GetSelectedInstance();
            if (item == null)
            {
                _selectedInfo.Text = "No LDPlayer selected.";
                return;
            }

            _selectedInfo.Text = string.Format(
                "PID {0} | process {1} | main {2} [{3}] | target {4} [{5}] | {6}",
                item.ProcessId,
                item.ProcessName,
                item.MainHandleHex,
                item.MainClass,
                item.TargetHandleHex,
                item.TargetClass,
                item.TargetSize);

            if (item.TargetWidth > 0) _x.Maximum = Math.Max(1, item.TargetWidth - 1);
            else _x.Maximum = 65535;

            if (item.TargetHeight > 0) _y.Maximum = Math.Max(1, item.TargetHeight - 1);
            else _y.Maximum = 65535;
        }

        private LDInstance GetSelectedInstance()
        {
            if (_grid.CurrentRow == null) return null;
            return _grid.CurrentRow.Tag as LDInstance;
        }

        private void CaptureSelected()
        {
            LDInstance item = GetSelectedInstance();
            if (item == null)
            {
                AppendLog("Capture: no selected LDPlayer instance.");
                return;
            }

            if (!NativeMethods.IsWindow(item.TargetHandle))
            {
                AppendLog("Capture: target HWND is no longer valid; refresh the list.");
                return;
            }

            try
            {
                Bitmap image = KAutoCompat.CaptureWindow(item.TargetHandle);
                if (image == null)
                {
                    AppendLog("Capture returned null for " + item.TargetHandleHex + ".");
                    return;
                }

                Image old = _preview.Image;
                _preview.Image = image;
                if (old != null) old.Dispose();

                _x.Maximum = Math.Max(1, image.Width - 1);
                _y.Maximum = Math.Max(1, image.Height - 1);

                AppendLog(string.Format(
                    "Captured target {0} [{1}] -> {2}x{3}.",
                    item.TargetHandleHex, item.TargetClass, image.Width, image.Height));
            }
            catch (Exception ex)
            {
                AppendLog("Capture exception: " + ex.Message);
            }
        }

        private void Preview_MouseClick(object sender, MouseEventArgs e)
        {
            if (_preview.Image == null)
            {
                AppendLog("Preview has no capture yet. Click 'Capture Target' first.");
                return;
            }

            Point mapped;
            if (!PreviewCoordinateMapper.TryMap(
                _preview.ClientSize,
                _preview.Image.Size,
                e.Location,
                out mapped))
            {
                AppendLog("Preview click was outside the displayed image area.");
                return;
            }

            if (mapped.X <= (int)_x.Maximum) _x.Value = mapped.X;
            if (mapped.Y <= (int)_y.Maximum) _y.Value = mapped.Y;

            AppendLog("Preview coordinate -> X=" + mapped.X + ", Y=" + mapped.Y + ".");

            if (_clickOnPreview.Checked)
                PerformHiddenClick();
        }

        private void PerformHiddenClick()
        {
            LDInstance item = GetSelectedInstance();
            if (item == null)
            {
                AppendLog("Hidden click: no selected LDPlayer instance.");
                return;
            }

            if (!NativeMethods.IsWindow(item.TargetHandle))
            {
                AppendLog("Hidden click: target HWND is no longer valid; refresh the list.");
                return;
            }

            int x = (int)_x.Value;
            int y = (int)_y.Value;
            Point cursorBefore = Cursor.Position;

            bool posted;
            try
            {
                posted = KAutoCompat.SendClickOnPosition(item.TargetHandle, x, y);
            }
            catch (Exception ex)
            {
                AppendLog("Hidden click exception: " + ex.Message);
                return;
            }

            Point cursorAfter = Cursor.Position;
            bool cursorUnchanged = cursorBefore == cursorAfter;

            AppendLog(string.Format(
                "Hidden click target={0} [{1}] x={2} y={3} PostMessage={4} physicalCursorUnchanged={5}",
                item.TargetHandleHex,
                item.TargetClass,
                x,
                y,
                posted,
                cursorUnchanged));

            if (_captureAfterClick.Checked)
            {
                var timer = new Timer { Interval = 180 };
                timer.Tick += delegate
                {
                    timer.Stop();
                    timer.Dispose();
                    CaptureSelected();
                };
                timer.Start();
            }
        }

        private void AppendLog(string message)
        {
            string line = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] " + message;
            _log.AppendText(line + Environment.NewLine);
        }
    }
}
