using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace GoldenRuler
{
    internal static class Win32
    {
        [DllImport("user32.dll")] public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")] public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")] public static extern bool SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);
        [DllImport("user32.dll")] public static extern bool SetCapture(IntPtr hWnd);
        [DllImport("user32.dll")] public static extern bool ReleaseCapture();
        [DllImport("user32.dll")] public static extern short GetAsyncKeyState(int vKey);
        public const int VK_LBUTTON = 0x01;
        public const int VK_MENU = 0x12;
        public const int VK_CONTROL = 0x11;
        public static bool IsMouseLeftDown() { return (GetAsyncKeyState(VK_LBUTTON) & 0x8000) != 0; }
        public static bool IsAltDown() { return (GetAsyncKeyState(VK_MENU) & 0x8000) != 0; }
        public static bool IsCtrlDown() { return (GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0; }
        [DllImport("user32.dll")] public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")] public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_SHOWWINDOW = 0x0040;
        public const uint SWP_HIDEWINDOW = 0x0080;
        [DllImport("gdi32.dll")] public static extern IntPtr CreateSolidBrush(uint crColor);
        [DllImport("user32.dll", EntryPoint = "SetClassLong")] public static extern uint SetClassLong32(IntPtr hWnd, int nIndex, uint dwNewLong);
        [DllImport("user32.dll", EntryPoint = "SetClassLongPtr")] public static extern IntPtr SetClassLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        public const int GCLP_HBRBACKGROUND = -10;
        public static void SetBgBrush(IntPtr hWnd, Color c)
        {
            uint color = (uint)(c.R | (c.G << 8) | (c.B << 16));
            IntPtr hbr = CreateSolidBrush(color);
            if (IntPtr.Size == 8) SetClassLongPtr64(hWnd, GCLP_HBRBACKGROUND, hbr);
            else SetClassLong32(hWnd, GCLP_HBRBACKGROUND, (uint)hbr.ToInt32());
        }
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_LAYERED = 0x00080000;
        public const int WS_EX_TOPMOST = 0x00000008;
        public const uint LWA_COLORKEY = 0x00000001;
        public const uint LWA_ALPHA = 0x00000002;
        public const int WM_HOTKEY = 0x0312;
        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        public const uint MOD_WIN = 0x0008;
        public const uint MOD_NOREPEAT = 0x4000;

    }

    internal enum GridMode { Classic, GoldenSection, Square, Spiral, Circles, FixedGrid }
    public enum LineMode { DualColor, SingleColor }
    internal enum GridOrigin { TopLeft, TopRight, BottomLeft, BottomRight }

    [Serializable]
    public class HotkeyEntry
    {
        public string Action = "";
        public string Key = "";
        public HotkeyEntry() { }
        public HotkeyEntry(string a, string k) { Action = a; Key = k; }
    }

    [Serializable]
    public class RulerSettings
    {
        public int MoveStep = 10;
        public int ShiftMoveStep = 1;
        public double RotateStep = 1.0;
        public int GridStep = 1;
        public int GripStep = 5;
        public int Transparency = 210;
        public float LineWidth = 1.0f;
        public bool ShowLabels = true;
        public int LineMode = 0;
        public int LineColorA = 255; public int LineColorR = 255; public int LineColorG = 0; public int LineColorB = 0;
        public int GoldenLineColorA = 255; public int GoldenLineColorR = 0; public int GoldenLineColorG = 255; public int GoldenLineColorB = 0;
        public int FocusBorderColorA = 255; public int FocusBorderColorR = 0; public int FocusBorderColorG = 128; public int FocusBorderColorB = 255;
        public int GridMode = 5;
        public int GridOrigin = 0;
        public int GridCellSize = 30;
        public List<HotkeyEntry> Hotkeys = new List<HotkeyEntry>();

        [XmlIgnore] public Color LineColor
        {
            get { return Color.FromArgb(LineColorA, LineColorR, LineColorG, LineColorB); }
            set { LineColorA = value.A; LineColorR = value.R; LineColorG = value.G; LineColorB = value.B; }
        }
        [XmlIgnore] public Color GoldenLineColor
        {
            get { return Color.FromArgb(GoldenLineColorA, GoldenLineColorR, GoldenLineColorG, GoldenLineColorB); }
            set { GoldenLineColorA = value.A; GoldenLineColorR = value.R; GoldenLineColorG = value.G; GoldenLineColorB = value.B; }
        }
        [XmlIgnore] public Color FocusBorderColor
        {
            get { return Color.FromArgb(FocusBorderColorA, FocusBorderColorR, FocusBorderColorG, FocusBorderColorB); }
            set { FocusBorderColorA = value.A; FocusBorderColorR = value.R; FocusBorderColorG = value.G; FocusBorderColorB = value.B; }
        }
        [XmlIgnore] public LineMode LineModeEnum { get { return (LineMode)LineMode; } set { LineMode = (int)value; } }

        public RulerSettings() { }
        public RulerSettings(RulerSettings o)
        {
            MoveStep=o.MoveStep; ShiftMoveStep=o.ShiftMoveStep; RotateStep=o.RotateStep;
            GridStep=o.GridStep; GripStep=o.GripStep; Transparency=o.Transparency; LineWidth=o.LineWidth;
            ShowLabels=o.ShowLabels; LineMode=o.LineMode;
            LineColorA=o.LineColorA; LineColorR=o.LineColorR; LineColorG=o.LineColorG; LineColorB=o.LineColorB;
            GoldenLineColorA=o.GoldenLineColorA; GoldenLineColorR=o.GoldenLineColorR; GoldenLineColorG=o.GoldenLineColorG; GoldenLineColorB=o.GoldenLineColorB;
            FocusBorderColorA=o.FocusBorderColorA; FocusBorderColorR=o.FocusBorderColorR; FocusBorderColorG=o.FocusBorderColorG; FocusBorderColorB=o.FocusBorderColorB;
            GridMode=o.GridMode; GridOrigin=o.GridOrigin; GridCellSize=o.GridCellSize;
            Hotkeys = new List<HotkeyEntry>();
            foreach (var h in o.Hotkeys) Hotkeys.Add(new HotkeyEntry(h.Action, h.Key));
            DeduplicateHotkeys();
        }

        public void InitDefaultHotkeys()
        {
            if (Hotkeys == null) Hotkeys = new List<HotkeyEntry>();
            DeduplicateHotkeys();
            var existing = new HashSet<string>();
            foreach (var h in Hotkeys) existing.Add(h.Action);
            var defs = new[] {
                new HotkeyEntry("MoveLeft","A"), new HotkeyEntry("MoveRight","D"),
                new HotkeyEntry("MoveUp","W"), new HotkeyEntry("MoveDown","S"),
                new HotkeyEntry("FineLeft","Shift+A"), new HotkeyEntry("FineRight","Shift+D"),
                new HotkeyEntry("FineUp","Shift+W"), new HotkeyEntry("FineDown","Shift+S"),
                new HotkeyEntry("GripLeft","A"), new HotkeyEntry("GripRight","D"),
                new HotkeyEntry("GripUp","W"), new HotkeyEntry("GripDown","S"),
                new HotkeyEntry("NextMode","Enter"), new HotkeyEntry("NextOrigin","Space"),
                new HotkeyEntry("RotateCCW","Q"), new HotkeyEntry("RotateCW","E"),
                new HotkeyEntry("ResetRotation","R"), new HotkeyEntry("RotateInput","G"),
                new HotkeyEntry("TransparencyUp","V"), new HotkeyEntry("TransparencyDown","C"),
                new HotkeyEntry("Mode1","D1"), new HotkeyEntry("Mode2","D2"), new HotkeyEntry("Mode3","D3"),
                new HotkeyEntry("Mode4","D4"), new HotkeyEntry("Mode5","D5"), new HotkeyEntry("Mode6","D6"),
                new HotkeyEntry("ZoomIn","X"), new HotkeyEntry("ZoomOut","Z"),
                new HotkeyEntry("GoldenHLine","Alt+Q"), new HotkeyEntry("GoldenVLine","Alt+W"),
                new HotkeyEntry("ToggleTitleBar","H"),
                new HotkeyEntry("ToggleSettings","Tab"), new HotkeyEntry("Exit","Escape"),
                new HotkeyEntry("ToggleLineMode","T"),
                new HotkeyEntry("GlobalFocus","Alt+D2"), new HotkeyEntry("GlobalFocusAtMouse","Alt+D1"),
            };
            foreach (var d in defs) if (!existing.Contains(d.Action)) Hotkeys.Add(new HotkeyEntry(d.Action, d.Key));
        }

        public void DeduplicateHotkeys()
        {
            if (Hotkeys == null || Hotkeys.Count <= 1) return;
            var seen = new HashSet<string>();
            var unique = new List<HotkeyEntry>();
            foreach (var h in Hotkeys) { if (seen.Add(h.Action)) unique.Add(h); }
            Hotkeys = unique;
        }

        public string GetHotkey(string action)
        {
            foreach (var h in Hotkeys) if (h.Action == action) return h.Key;
            return "";
        }
        public void SetHotkey(string action, string key)
        {
            foreach (var h in Hotkeys) if (h.Action == action) { h.Key = key; return; }
            Hotkeys.Add(new HotkeyEntry(action, key));
        }
    }

    internal class WheelFilter : IMessageFilter
    {
        private RulerForm _form;
        public WheelFilter(RulerForm f) { _form = f; }
        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == 0x020A && _form.IsMouseOverGripOrTitle() && _form.Mode == GridMode.FixedGrid)
            {
                int d = (short)((long)m.WParam >> 16 & 0xFFFF);
                _form.GridCellSize = Math.Max(5, Math.Min(500, _form.GridCellSize + (d > 0 ? _form.Settings.GridStep : -_form.Settings.GridStep)));
                _form.Invalidate();
                return true;
            }
            return false;
        }
    }

    internal class RulerForm : Form
    {
        private GridMode _mode = GridMode.GoldenSection;
        private RulerSettings _settings;
        private double _rotation = 0.0;
        private int _gridCellSize = 55;
        private GridOrigin _origin = GridOrigin.TopLeft;
        private bool _rotationInputMode, _showTitleBar = true;
        private double _rotationInputPreview, _preInputRotation;
        private string _rotationInputText = "";
        private SettingsForm _settingsForm;
        private Rectangle _gripRect, _gripTLRect;
        private bool _isDragging, _isGripDragging, _isGripHeld, _isGripTLDragging, _shiftDown;
        private Point _dragOffset, _gripStartScreen, _gripStartLoc;
        private Size _gripStartSize;
        private double _lockRatio = 0.0;
        private bool _showGoldenHLine, _showGoldenVLine;
        private Timer _goldenHTimer, _goldenVTimer;
        private GoldenOverlayForm _goldenOverlay;
        private Rectangle _closeRect, _modeRect, _settingsRect, _hideTitleRect;
        private int _ghkFocusId = 100, _ghkFocusMouseId = 101;
        private GripPadForm _gripPadBR;
        private GripPadForm _gripPadTL;

        public bool IsGripHeld { get { return _isGripHeld; } }
        public bool ShowGoldenH { get { return _showGoldenHLine; } }
        public bool ShowGoldenV { get { return _showGoldenVLine; } }
        public double Rotation { get { return _rotationInputMode ? _rotationInputPreview : _rotation; } }
        public GridOrigin CurrentOrigin { get { return _origin; } }
        public bool HasTitleBar { get { return _showTitleBar; } }
        public GridMode Mode { get { return _mode; } set { _mode = value; } }
        public int GridCellSize { get { return _gridCellSize; } set { _gridCellSize = value; } }
        public RulerSettings Settings { get { return _settings; } }

        public bool IsMouseOverGripOrTitle()
        {
            Point mp = this.PointToClient(Control.MousePosition);
            return _gripRect.Contains(mp) || _gripTLRect.Contains(mp) || (_showTitleBar && mp.Y >= 0 && mp.Y < 20);
        }

        private void UpdateGripPads()
        {
            if (_gripPadBR == null || _gripPadBR.Handle == IntPtr.Zero) return;
            int w = this.ClientSize.Width, h = this.ClientSize.Height;
            // 右下角把手热区 72x72
            Win32.SetWindowPos(_gripPadBR.Handle, Win32.HWND_TOPMOST, this.Right - 72, this.Bottom - 72, 72, 72, Win32.SWP_NOACTIVATE | Win32.SWP_SHOWWINDOW);
            // 左上角把手热区 72x72
            Win32.SetWindowPos(_gripPadTL.Handle, Win32.HWND_TOPMOST, this.Left, this.Top, 72, 72, Win32.SWP_NOACTIVATE | Win32.SWP_SHOWWINDOW);
        }

        public void HandleGripMouseDown(bool isTopLeft, MouseEventArgs e)
        {
            this.Focus();
            if (isTopLeft)
            {
                _isGripHeld = true; _isGripTLDragging = true;
                _gripStartScreen = Control.MousePosition;
                _gripStartSize = this.Size; _gripStartLoc = this.Location;
                Win32.SetCapture(this.Handle);
                if (_shiftDown) _lockRatio = (double)this.Width / this.Height;
            }
            else
            {
                _isGripHeld = true; _isGripDragging = true;
                _gripStartScreen = Control.MousePosition;
                _gripStartSize = this.Size; _gripStartLoc = this.Location;
                Win32.SetCapture(this.Handle);
                if (_shiftDown) _lockRatio = (double)this.Width / this.Height;
            }
        }

        public void HandleGripMouseMove(bool isTopLeft, MouseEventArgs e)
        {
            // 交给主窗口的OnMouseMove逻辑处理（通过SetCapture后事件已经在主窗口了）
            // 这里只更新光标
            this.Cursor = Cursors.SizeNWSE;
        }

        public void HandleGripMouseUp(bool isTopLeft, MouseEventArgs e)
        {
            // 主窗口的OnMouseUp会处理（通过SetCapture）
        }

        private static string SettingsPath { get { return Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "settings.xml"); } }

        public RulerForm()
        {
            _settings = LoadSettings();
            _mode = (GridMode)_settings.GridMode;
            _origin = (GridOrigin)_settings.GridOrigin;
            _gridCellSize = _settings.GridCellSize;
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Size = new Size(400, 247);
            this.Location = new Point(200, 200);
            this.MinimumSize = new Size(80, 60);
            this.TopMost = true;
            this.DoubleBuffered = true;
            this.BackColor = Color.Fuchsia;
            this.TransparencyKey = Color.Fuchsia;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            int ex = Win32.GetWindowLong(this.Handle, Win32.GWL_EXSTYLE);
            ex |= Win32.WS_EX_LAYERED | Win32.WS_EX_TOPMOST;
            Win32.SetWindowLong(this.Handle, Win32.GWL_EXSTYLE, ex);
            UpdateTransparency();
            _gripRect = new Rectangle(0, 0, 16, 16);
            _gripTLRect = new Rectangle(0, 0, 16, 16);
            Application.AddMessageFilter(new WheelFilter(this));
            _gripPadBR = new GripPadForm(this, false);
            _gripPadTL = new GripPadForm(this, true);
            this.Load += (s, e) => { RegisterGlobalHotkeys(); _gripPadBR.Show(); _gripPadTL.Show(); UpdateGripPads(); };
            this.HandleCreated += (s, e) => Win32.SetBgBrush(this.Handle, Color.Fuchsia);
        }

        private void RegisterGlobalHotkeys()
        {
            Win32.UnregisterHotKey(this.Handle, _ghkFocusId);
            Win32.UnregisterHotKey(this.Handle, _ghkFocusMouseId);
            RegisterOneGlobal("GlobalFocus", _ghkFocusId);
            RegisterOneGlobal("GlobalFocusAtMouse", _ghkFocusMouseId);
        }

        private void RegisterOneGlobal(string action, int id)
        {
            string hk = _settings.GetHotkey(action);
            if (string.IsNullOrEmpty(hk)) return;
            string[] parts = hk.Split('+');
            string keyPart = parts[parts.Length - 1].Trim();
            Keys k;
            if (!Enum.TryParse(keyPart, out k)) return;
            uint mods = Win32.MOD_NOREPEAT;
            if (hk.Contains("Alt")) mods |= Win32.MOD_ALT;
            if (hk.Contains("Ctrl")) mods |= Win32.MOD_CONTROL;
            if (hk.Contains("Shift")) mods |= Win32.MOD_SHIFT;
            if (hk.Contains("Win")) mods |= Win32.MOD_WIN;
            Win32.RegisterHotKey(this.Handle, id, mods, (uint)k);
        }

        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_CANCELMODE = 0x001F;
        private const int WM_CAPTURECHANGED = 0x0215;
        private const int WM_NCHITTEST = 0x0084;
        private const int HTCLIENT = 1;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCHITTEST)
            {
                int x = (int)((short)(m.LParam.ToInt32() & 0xFFFF));
                int y = (int)((short)(m.LParam.ToInt32() >> 16));
                var screen = new Rectangle(this.Location, this.Size);
                if (screen.Contains(x, y))
                {
                    m.Result = (IntPtr)HTCLIENT;
                    return;
                }
            }
            // Intercept spurious WM_LBUTTONUP / WM_CANCELMODE that Windows generates
            // when certain keys are pressed during mouse capture. Check physical mouse
            // state via GetAsyncKeyState to distinguish real releases from fake ones.
            if (_isGripHeld && Win32.IsMouseLeftDown())
            {
                if (m.Msg == WM_LBUTTONUP || m.Msg == WM_CANCELMODE)
                    return;
            }
            // Also intercept WM_CAPTURECHANGED — re-establish capture if physical button still down
            if (m.Msg == WM_CAPTURECHANGED && _isGripHeld && Win32.IsMouseLeftDown())
            {
                Win32.SetCapture(this.Handle);
                return;
            }
            if (m.Msg == Win32.WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                if (id == _ghkFocusId) { this.Activate(); this.Focus(); this.TopMost = true; }
                else if (id == _ghkFocusMouseId)
                {
                    Point mp = Control.MousePosition;
                    int nx = mp.X - this.Width / 2;
                    int ny = mp.Y - this.Height / 2;
                    this.Location = new Point(nx, ny);
                    this.Activate(); this.Focus(); this.TopMost = true;
                }
            }
            base.WndProc(ref m);
        }

        private static RulerSettings LoadSettings()
        {
            try { if (File.Exists(SettingsPath)) { var s = new XmlSerializer(typeof(RulerSettings)); using (var f = File.OpenRead(SettingsPath)) { var r = (RulerSettings)s.Deserialize(f); r.InitDefaultHotkeys(); return r; } } }
            catch { }
            var d = new RulerSettings(); d.InitDefaultHotkeys(); return d;
        }
        private static void SaveSettings(RulerSettings s) { try { s.DeduplicateHotkeys(); var ser = new XmlSerializer(typeof(RulerSettings)); using (var f = File.Create(SettingsPath)) ser.Serialize(f, s); } catch { } }
        private void UpdateTransparency() { uint c = (uint)(Color.Fuchsia.R | (Color.Fuchsia.G << 8) | (Color.Fuchsia.B << 16)); Win32.SetLayeredWindowAttributes(this.Handle, c, (byte)_settings.Transparency, Win32.LWA_COLORKEY | Win32.LWA_ALPHA); }
        protected override void OnLocationChanged(EventArgs e) { base.OnLocationChanged(e); this.Invalidate(); UpdateGripPads(); if (_goldenOverlay != null && _goldenOverlay.Handle != IntPtr.Zero) { Win32.SetWindowPos(_goldenOverlay.Handle, Win32.HWND_TOPMOST, this.Left, this.Top, this.Width, this.Height, Win32.SWP_NOACTIVATE); _goldenOverlay.Invalidate(); } }
        protected override void OnResize(EventArgs e) { base.OnResize(e); this.Invalidate(); UpdateGripPads(); if (_goldenOverlay != null && _goldenOverlay.Handle != IntPtr.Zero) { Win32.SetWindowPos(_goldenOverlay.Handle, Win32.HWND_TOPMOST, this.Left, this.Top, this.Width, this.Height, Win32.SWP_NOACTIVATE); _goldenOverlay.Invalidate(); } }
        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); this.Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); this.Invalidate(); }

        private bool MatchHotkey(KeyEventArgs e, string hk)
        {
            if (string.IsNullOrEmpty(hk)) return false;
            string[] parts = hk.Split('+');
            string keyPart = parts[parts.Length - 1].Trim();
            Keys k;
            if (!Enum.TryParse(keyPart, out k)) return false;
            if (e.KeyCode != k) return false;
            bool needAlt = hk.Contains("Alt");
            bool needCtrl = hk.Contains("Ctrl");
            bool needShift = hk.Contains("Shift");
            if (needAlt != e.Alt) return false;
            if (needCtrl != e.Control) return false;
            if (needShift != e.Shift) return false;
            return true;
        }

        private GridMode NextMode(GridMode m) { switch (m) { case GridMode.Classic: return GridMode.GoldenSection; case GridMode.GoldenSection: return GridMode.FixedGrid; case GridMode.FixedGrid: return GridMode.Square; case GridMode.Square: return GridMode.Spiral; case GridMode.Spiral: return GridMode.Circles; default: return GridMode.Classic; } }
        private string ModeName(GridMode m) { switch (m) { case GridMode.Classic: return "经典"; case GridMode.GoldenSection: return "黄金分割"; case GridMode.Square: return "方块"; case GridMode.Spiral: return "螺旋"; case GridMode.Circles: return "圆环"; case GridMode.FixedGrid: return "固定" + _gridCellSize + "px"; default: return "?"; } }
        private string OriginName(GridOrigin o) { switch (o) { case GridOrigin.TopLeft: return "左上"; case GridOrigin.TopRight: return "右上"; case GridOrigin.BottomLeft: return "左下"; default: return "右下"; } }
        private GridOrigin NextOrigin(GridOrigin o) { switch (o) { case GridOrigin.TopLeft: return GridOrigin.TopRight; case GridOrigin.TopRight: return GridOrigin.BottomLeft; case GridOrigin.BottomLeft: return GridOrigin.BottomRight; default: return GridOrigin.TopLeft; } }

        protected override void OnPaint(PaintEventArgs e)
        {
            int w = this.ClientSize.Width, h = this.ClientSize.Height;
            if (w <= 0 || h <= 0) return;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            g.Clear(Color.Fuchsia);

            int barH = _showTitleBar ? 20 : 0;
            int gridY = barH;
            int gridH = h - gridY;
            if (gridH <= 0) { if (_showTitleBar) DrawTitleBar(g, w); DrawGrip(g); return; }

            float cx = w / 2f, cy = gridY + gridH / 2f;
            float diag = (float)Math.Sqrt(w * w + gridH * gridH);
            int ext = (int)((diag - Math.Min(w, gridH)) / 2f + 20f);
            float fcx = cx, fcy = cy;
            double curRot = _rotationInputMode ? _rotationInputPreview : _rotation;
            g.TranslateTransform(fcx, fcy);
            g.RotateTransform((float)curRot);
            g.TranslateTransform(-fcx, -fcy);
            g.TranslateTransform(0, gridY);

            float lw = _settings.LineWidth;
            Color dc = _settings.LineModeEnum == LineMode.SingleColor ? _settings.LineColor : Color.Black;

            using (Pen pen = new Pen(dc, lw))
            {
                switch (_mode)
                {
                    case GridMode.Classic: DrawClassicGrid(g, pen, w, gridH, ext); break;
                    case GridMode.GoldenSection: DrawGoldenSection(g, pen, w, gridH, ext); break;
                    case GridMode.Square: DrawSquareGrid(g, pen, w, gridH, ext); break;
                    case GridMode.Spiral: DrawSpiral(g, pen, w, gridH); break;
                    case GridMode.Circles: DrawCircles(g, pen, w, gridH); break;
                    case GridMode.FixedGrid: DrawFixedGrid(g, pen, w, gridH, ext); break;
                }
                if (_mode != GridMode.GoldenSection) g.DrawRectangle(pen, -ext, -ext, w + 2 * ext - 1, gridH + 2 * ext - 1);
            }
            if (_settings.LineModeEnum == LineMode.DualColor)
            {
                using (Pen wp = new Pen(Color.White, lw)) { wp.DashStyle = DashStyle.Dash; wp.DashPattern = new float[] { 3f, 3f };
                    switch (_mode) {
                        case GridMode.Classic: DrawClassicGrid(g, wp, w, gridH, ext); break;
                        case GridMode.GoldenSection: DrawGoldenSection(g, wp, w, gridH, ext); break;
                        case GridMode.Square: DrawSquareGrid(g, wp, w, gridH, ext); break;
                        case GridMode.Spiral: DrawSpiral(g, wp, w, gridH); break;
                        case GridMode.Circles: DrawCircles(g, wp, w, gridH); break;
                        case GridMode.FixedGrid: DrawFixedGrid(g, wp, w, gridH, ext); break;
                    }
                    if (_mode != GridMode.GoldenSection) g.DrawRectangle(wp, -ext, -ext, w + 2 * ext - 1, gridH + 2 * ext - 1);
                }
            }
            g.ResetTransform();
            if (_showTitleBar) DrawTitleBar(g, w);
            DrawGrip(g);
            if (_settings.ShowLabels) DrawLabels(g, w, h);
            if (_rotationInputMode) DrawRotationInputBox(g, w, h);
        }

        private void DrawFixedGrid(Graphics g, Pen pen, int w, int h, int ext)
        {
            int cs = _gridCellSize; if (cs < 2) cs = 2;
            int x0, y0, dx, dy;
            switch (_origin)
            {
                case GridOrigin.TopLeft:    x0 = cs;  dx = cs;  y0 = cs;  dy = cs;  break;
                case GridOrigin.TopRight:   x0 = w-cs; dx = -cs; y0 = cs;  dy = cs;  break;
                case GridOrigin.BottomLeft:  x0 = cs;  dx = cs;  y0 = h-cs; dy = -cs; break;
                default:                     x0 = w-cs; dx = -cs; y0 = h-cs; dy = -cs; break;
            }
            int x = x0;
            while (x > -ext) { g.DrawLine(pen, x, -ext, x, h + ext); x -= cs; }
            x = x0 + cs;
            while (x < w + ext) { g.DrawLine(pen, x, -ext, x, h + ext); x += cs; }
            int y = y0;
            while (y > -ext) { g.DrawLine(pen, -ext, y, w + ext, y); y -= cs; }
            y = y0 + cs;
            while (y < h + ext) { g.DrawLine(pen, -ext, y, w + ext, y); y += cs; }
        }

        private void DrawClassicGrid(Graphics g, Pen pen, int w, int h, int ext)
        {
            g.DrawLine(pen, w / 3, -ext, w / 3, h + ext);
            g.DrawLine(pen, 2 * w / 3, -ext, 2 * w / 3, h + ext);
            g.DrawLine(pen, -ext, h / 3, w + ext, h / 3);
            g.DrawLine(pen, -ext, 2 * h / 3, w + ext, 2 * h / 3);
        }

        private void DrawGoldenSection(Graphics g, Pen pen, int w, int h, int ext)
        {
            double phi = 1.6180339887, inv = 1.0 / phi, big = 1.0 - inv;
            float xa, xb, ya, yb;
            switch (_origin)
            {
                case GridOrigin.TopLeft:    xa=(float)(w*inv);  xb=(float)(w*big);  ya=(float)(h*inv);  yb=(float)(h*big); break;
                case GridOrigin.TopRight:   xa=(float)(w*big);  xb=(float)(w*inv);  ya=(float)(h*inv);  yb=(float)(h*big); break;
                case GridOrigin.BottomLeft:  xa=(float)(w*inv);  xb=(float)(w*big);  ya=(float)(h*big);  yb=(float)(h*inv); break;
                default:                     xa=(float)(w*big);  xb=(float)(w*inv);  ya=(float)(h*big);  yb=(float)(h*inv); break;
            }

            g.DrawRectangle(pen, 0, 0, w - 1, h - 1);

            g.DrawLine(pen, xa, 0, xa, h - 1);
            g.DrawLine(pen, 0, ya, w - 1, ya);

            using (Pen dp = new Pen(pen.Color, pen.Width))
            {
                dp.DashStyle = DashStyle.Dot;
                g.DrawLine(dp, 0, 0, w - 1, h - 1);
                g.DrawLine(dp, 0, h - 1, w - 1, 0);
            }

            DrawDiamond(g, pen.Color, xa, ya, 4);
            DrawDiamond(g, pen.Color, xb, ya, 4);
            DrawDiamond(g, pen.Color, xa, yb, 4);
            DrawDiamond(g, pen.Color, xb, yb, 4);
        }

        private void DrawDiamond(Graphics g, Color c, float cx, float cy, float r)
        {
            PointF[] pts = { new PointF(cx, cy - r), new PointF(cx + r, cy), new PointF(cx, cy + r), new PointF(cx - r, cy) };
            using (SolidBrush b = new SolidBrush(c)) g.FillPolygon(b, pts);
            using (Pen p = new Pen(c, 1)) g.DrawPolygon(p, pts);
        }

        private void DrawSquareGrid(Graphics g, Pen pen, int w, int h, int ext)
        {
            double phi = 1.6180339887;
            float curW = w, curH = h, ox = 0, oy = 0;
            bool horiz = w >= h;
            for (int i = 0; i < 8; i++)
            {
                if (horiz) { float sq = curH; g.DrawLine(pen, ox + sq, oy - ext, ox + sq, oy + curH + ext); curW -= sq; ox += sq; horiz = curW >= curH * phi; if (curW < 5) break; }
                else { float sq = curW; g.DrawLine(pen, ox - ext, oy + sq, ox + curW + ext, oy + sq); curH -= sq; oy += sq; horiz = curW >= curH * phi; if (curH < 5) break; }
            }
        }

        private void DrawSpiral(Graphics g, Pen pen, int w, int h)
        {
            double phi = 1.6180339887; float curW=w, curH=h, ox=0, oy=0; bool horiz = w >= h; int dir = 0;
            for (int i = 0; i < 8; i++)
            {
                if (horiz) { float sq=curH; RectangleF ar; switch(dir){case 0:ar=new RectangleF(ox,oy,sq,sq);break;case 1:ar=new RectangleF(ox,oy,sq,sq);break;case 2:ar=new RectangleF(ox+curW-sq,oy,sq,sq);break;default:ar=new RectangleF(ox,oy,sq,sq);break;} g.DrawArc(pen,ar,dir*90f,90f); g.DrawLine(pen,ox+sq,oy,ox+sq,oy+curH); curW-=sq; if(dir==0){ox+=sq;dir=1;}else if(dir==1){ox+=sq;dir=2;}else if(dir==2){dir=3;}else{dir=0;} horiz=curW>=curH*phi; if(curW<5)break; }
                else { float sq=curW; g.DrawLine(pen,ox,oy+sq,ox+curW,oy+sq); g.DrawArc(pen,new RectangleF(ox,oy,sq,sq),dir*90f,90f); curH-=sq; oy+=sq; horiz=curW>=curH*phi; if(curH<5)break; }
            }
        }

        private void DrawCircles(Graphics g, Pen pen, int w, int h)
        {
            double phi = 1.6180339887; float cx=w/2f, cy=h/2f, r=Math.Min(w,h)/2f;
            for (int i=0;i<5;i++){g.DrawEllipse(pen,cx-r,cy-r,r*2,r*2); r=(float)(r/phi); if(r<5)break;}
            float x1=(float)(w/phi); g.DrawLine(pen,x1,0,x1,h); float y1=(float)(h/phi); g.DrawLine(pen,0,y1,w,y1);
        }

        private void DrawGoldenGuideLines(Graphics g, int w, int h, int ext) { }

        private void ShowGoldenHLine()
        {
            _showGoldenHLine = true;
            if (_goldenHTimer == null) { _goldenHTimer = new Timer(); _goldenHTimer.Interval = 3000; _goldenHTimer.Tick += (s, e) => { _showGoldenHLine = false; _goldenHTimer.Stop(); UpdateGoldenOverlay(); }; }
            _goldenHTimer.Stop(); _goldenHTimer.Start();
            UpdateGoldenOverlay();
        }

        private void ShowGoldenVLine()
        {
            _showGoldenVLine = true;
            if (_goldenVTimer == null) { _goldenVTimer = new Timer(); _goldenVTimer.Interval = 3000; _goldenVTimer.Tick += (s, e) => { _showGoldenVLine = false; _goldenVTimer.Stop(); UpdateGoldenOverlay(); }; }
            _goldenVTimer.Stop(); _goldenVTimer.Start();
            UpdateGoldenOverlay();
        }

        private void UpdateGoldenOverlay()
        {
            if (!_showGoldenHLine && !_showGoldenVLine)
            {
                if (_goldenOverlay != null && _goldenOverlay.Handle != IntPtr.Zero)
                    Win32.SetWindowPos(_goldenOverlay.Handle, IntPtr.Zero, 0, 0, 0, 0, Win32.SWP_NOMOVE | Win32.SWP_NOSIZE | Win32.SWP_NOACTIVATE | Win32.SWP_HIDEWINDOW);
                return;
            }
            if (_goldenOverlay == null) { _goldenOverlay = new GoldenOverlayForm(); _goldenOverlay.SetOwner(this); }
            Win32.SetWindowPos(_goldenOverlay.Handle, Win32.HWND_TOPMOST, this.Left, this.Top, this.Width, this.Height, Win32.SWP_NOACTIVATE | Win32.SWP_SHOWWINDOW);
            _goldenOverlay.Invalidate();
        }

        private void DrawTitleBar(Graphics g, int w)
        {
            int barH = 20;
            using (SolidBrush b = new SolidBrush(Color.FromArgb(40, 40, 40))) g.FillRectangle(b, 0, 0, w, barH);
            if (this.ContainsFocus)
            {
                Color fc = _settings.FocusBorderColor;
                using (Pen p = new Pen(Color.FromArgb(fc.A, fc.R, fc.G, fc.B), 1f))
                    g.DrawRectangle(p, 0, 0, w - 1, barH - 1);
            }
            int tx = 18;
            string modeText = ModeName(_mode) + " [" + OriginName(_origin) + "]";
            using (Font f = new Font("Segoe UI", 8.25f))
            using (SolidBrush tb = new SolidBrush(Color.White))
            { g.DrawString(modeText, f, tb, tx, 3); string rt = _rotation != 0 ? "  " + _rotation.ToString("F0") + "\u00b0" : ""; g.DrawString(rt, f, tb, tx + (int)g.MeasureString(modeText, f).Width, 3); }
            int bx = w - 16;
            _closeRect = new Rectangle(bx, 2, 14, 14); bx -= 16;
            _hideTitleRect = new Rectangle(bx, 2, 14, 14); bx -= 16;
            _settingsRect = new Rectangle(bx, 2, 14, 14); bx -= 16;
            _modeRect = new Rectangle(bx, 2, 14, 14);
            using (Font sf = new Font("Segoe UI", 7f))
            using (SolidBrush wb = new SolidBrush(Color.White))
            using (StringFormat sfmt = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            { g.DrawString("X", sf, wb, _closeRect, sfmt); g.DrawString("M", sf, wb, _hideTitleRect, sfmt); g.DrawString("S", sf, wb, _settingsRect, sfmt); g.DrawString("G", sf, wb, _modeRect, sfmt); }
        }

        private void DrawGrip(Graphics g)
        {
            int w = this.ClientSize.Width, h = this.ClientSize.Height;
            _gripRect = new Rectangle(w - 72, h - 72, 72, 72);
            Rectangle vis = new Rectangle(w - 14, h - 14, 9, 9);
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(60, 60, 60))) g.FillRectangle(bg, vis);
            using (Pen bp = new Pen(Color.White, 1f)) g.DrawRectangle(bp, vis.X, vis.Y, vis.Width - 1, vis.Height - 1);
            using (Pen b = new Pen(Color.White, 1f))
            { g.DrawLine(b, vis.X + 2, vis.Bottom - 2, vis.Right - 2, vis.Y + 2); }

            _gripTLRect = new Rectangle(0, 0, 72, 72);
            if (!_showTitleBar) DrawTLGripVisible(g);
        }

        private void DrawTLGripVisible(Graphics g)
        {
            Rectangle vis = new Rectangle(4, 4, 9, 9);
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(60, 60, 60))) g.FillRectangle(bg, vis);
            using (Pen bp = new Pen(Color.White, 1f)) g.DrawRectangle(bp, vis.X, vis.Y, vis.Width - 1, vis.Height - 1);
            using (Pen b = new Pen(Color.White, 1f))
            { g.DrawLine(b, vis.X + 2, vis.Y + 2, vis.Right - 2, vis.Bottom - 2); g.DrawLine(b, vis.X + 2, vis.Bottom - 2, vis.Right - 2, vis.Y + 2); }
        }

        private void DrawLabels(Graphics g, int w, int h)
        {
            int barH = _showTitleBar ? 20 : 0;
            int gridH = h - barH;
            using (Font f = new Font("Segoe UI", 7f))
            using (SolidBrush b = new SolidBrush(Color.White))
            { int lx = _showTitleBar ? 4 : 18; int ly = _showTitleBar ? 22 : 4; g.DrawString(w + " x " + gridH, f, b, lx, ly); double phi = 1.6180339887; g.DrawString("phi=" + phi.ToString("F3") + " | " + Math.Round(w / phi) + ":" + Math.Round(gridH / (phi * phi)), f, b, lx, ly + 12); }
        }

        private void DrawRotationInputBox(Graphics g, int w, int h)
        {
            int bw = 180, bh = 40, bx = w / 2 - bw / 2, by = h / 2 - bh / 2;
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(220, 30, 30, 30))) g.FillRectangle(bg, bx, by, bw, bh);
            using (Pen bp = new Pen(Color.White, 1)) g.DrawRectangle(bp, bx, by, bw - 1, bh - 1);
            string dt = "旋转: " + _rotationInputText + "_";
            using (Font f = new Font("Consolas", 14))
            using (SolidBrush tc = new SolidBrush(Color.White))
            { SizeF ts = g.MeasureString(dt, f); g.DrawString(dt, f, tc, bx + (bw - (int)ts.Width) / 2, by + (bh - (int)ts.Height) / 2); }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e); this.Focus();
            if (_gripTLRect.Contains(e.Location))
            { _isGripHeld = true; _isGripTLDragging = true; _gripStartScreen = Control.MousePosition; _gripStartSize = this.Size; _gripStartLoc = this.Location; Win32.SetCapture(this.Handle); if (_shiftDown) _lockRatio = (double)this.Width / this.Height; return; }
            if (_showTitleBar && e.Y < 20)
            {
                if (_closeRect.Contains(e.Location)) { this.Close(); return; }
                if (_hideTitleRect.Contains(e.Location)) { _showTitleBar = false; this.Invalidate(); return; }
                if (_settingsRect.Contains(e.Location)) { ToggleSettings(); return; }
                if (_modeRect.Contains(e.Location)) { _mode = NextMode(_mode); this.Invalidate(); return; }
            }
            if (_gripRect.Contains(e.Location))
            { _isGripHeld = true; _isGripDragging = true; _gripStartScreen = Control.MousePosition; _gripStartSize = this.Size; _gripStartLoc = this.Location; Win32.SetCapture(this.Handle); if (_shiftDown) _lockRatio = (double)this.Width / this.Height; return; }
            if (_showTitleBar && e.Y < 20) { _isDragging = true; _dragOffset = e.Location; Win32.SetCapture(this.Handle); return; }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_isGripHeld && Win32.IsMouseLeftDown()) Win32.SetCapture(this.Handle);
            if (_gripRect.Contains(e.Location) || _gripTLRect.Contains(e.Location)) this.Cursor = Cursors.SizeNWSE;
            else if (_showTitleBar && e.Y < 20) this.Cursor = Cursors.SizeAll;
            else this.Cursor = Cursors.Default;
            if (_isDragging) { Point s = Control.MousePosition; this.Location = new Point(s.X - _dragOffset.X, s.Y - _dragOffset.Y); }
            if (_isGripDragging && _isGripHeld)
            {
                Point cur = Control.MousePosition;
                int dx = cur.X - _gripStartScreen.X;
                int dy = cur.Y - _gripStartScreen.Y;
                if (Win32.IsCtrlDown())
                {
                    this.Location = new Point(_gripStartLoc.X + dx, _gripStartLoc.Y + dy);
                }
                else if (Win32.IsAltDown())
                {
                    int nw = _gripStartSize.Width + 2 * dx;
                    int nh = _gripStartSize.Height + 2 * dy;
                    if (nw > 60) { this.Width = nw; this.Left = _gripStartLoc.X - dx; }
                    if (nh > 60) { this.Height = nh; this.Top = _gripStartLoc.Y - dy; }
                }
                else if (_shiftDown && _lockRatio > 0) { int dl = Math.Max(dx, dy); int nw = _gripStartSize.Width + dl; int nh = (int)(nw / _lockRatio); if (nw > 60 && nh > 60) this.Size = new Size(nw, nh); }
                else { int nw = _gripStartSize.Width + dx; int nh = _gripStartSize.Height + dy; if (nw > 60) this.Width = nw; if (nh > 60) this.Height = nh; }
                this.Update();
            }
            if (_isGripTLDragging && _isGripHeld)
            {
                Point cur = Control.MousePosition;
                int dx = cur.X - _gripStartScreen.X;
                int dy = cur.Y - _gripStartScreen.Y;
                if (Win32.IsCtrlDown())
                {
                    this.Location = new Point(_gripStartLoc.X + dx, _gripStartLoc.Y + dy);
                }
                else if (Win32.IsAltDown())
                {
                    int nw = _gripStartSize.Width - 2 * dx;
                    int nh = _gripStartSize.Height - 2 * dy;
                    if (nw > 60) { this.Width = nw; this.Left = _gripStartLoc.X + dx; }
                    if (nh > 60) { this.Height = nh; this.Top = _gripStartLoc.Y + dy; }
                }
                else if (_shiftDown && _lockRatio > 0) { int dl = Math.Max(Math.Abs(dx), Math.Abs(dy)); int sign = (dx + dy < 0) ? 1 : -1; int nw = _gripStartSize.Width + dl * sign; int nh = (int)(nw / _lockRatio); if (nw > 60 && nh > 60) { this.Size = new Size(nw, nh); this.Location = new Point(_gripStartLoc.X + _gripStartSize.Width - nw, _gripStartLoc.Y + _gripStartSize.Height - nh); } }
                else { int nw = _gripStartSize.Width - dx; int nh = _gripStartSize.Height - dy; if (nw > 60) { this.Width = nw; this.Left = _gripStartLoc.X + dx; } if (nh > 60) { this.Height = nh; this.Top = _gripStartLoc.Y + dy; } }
                this.Update();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (Win32.IsMouseLeftDown()) return;
            _isDragging = false; _isGripDragging = false; _isGripTLDragging = false; _isGripHeld = false; Win32.ReleaseCapture();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Shift) _shiftDown = true;
            if (_settingsForm != null && _settingsForm.Visible) return;

            if (_rotationInputMode)
            {
                if (e.KeyCode == Keys.Escape) { _rotationInputMode = false; _rotationInputText = ""; _rotationInputPreview = _preInputRotation; e.Handled = true; }
                else if (e.KeyCode == Keys.Enter) { double v; if (double.TryParse(_rotationInputText, out v)) { _rotation = v; if (_rotation > 360) _rotation -= 360; if (_rotation < -360) _rotation += 360; } _rotationInputMode = false; _rotationInputText = ""; e.Handled = true; }
                else if (e.KeyCode == Keys.Back) { if (_rotationInputText.Length > 0) _rotationInputText = _rotationInputText.Substring(0, _rotationInputText.Length - 1); double v; _rotationInputPreview = double.TryParse(_rotationInputText, out v) ? v : _preInputRotation; e.Handled = true; }
                else if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus) { if (!_rotationInputText.StartsWith("-")) _rotationInputText = "-" + _rotationInputText; double v; _rotationInputPreview = double.TryParse(_rotationInputText, out v) ? v : _preInputRotation; e.Handled = true; }
                this.Invalidate(); return;
            }

            var s = _settings;
            int step = s.MoveStep;
            int fstep = s.ShiftMoveStep;
            int gstep = s.GripStep;

            if (MatchHotkey(e, s.GetHotkey("RotateInput"))) { _rotationInputMode = true; _rotationInputText = ""; _preInputRotation = _rotation; _rotationInputPreview = _rotation; e.Handled = true; this.Invalidate(); return; }
            if (MatchHotkey(e, s.GetHotkey("GoldenHLine"))) { ShowGoldenHLine(); e.Handled = true; e.SuppressKeyPress = true; return; }
            if (MatchHotkey(e, s.GetHotkey("GoldenVLine"))) { ShowGoldenVLine(); e.Handled = true; e.SuppressKeyPress = true; return; }
            if (_isGripHeld)
            {
                Win32.SetCapture(this.Handle);
                if (MatchHotkey(e, s.GetHotkey("GripLeft")))
                {
                    if (_isGripTLDragging) { if (this.Width - gstep > 60) { this.Width -= gstep; _gripStartSize = this.Size; } }
                    else { this.Left -= gstep; this.Width += gstep; _gripStartSize = this.Size; }
                    _gripStartScreen = Control.MousePosition; _gripStartLoc = this.Location; e.Handled = true;
                }
                else if (MatchHotkey(e, s.GetHotkey("GripRight")))
                {
                    if (_isGripTLDragging) { this.Width += gstep; _gripStartSize = this.Size; }
                    else if (this.Width - gstep > 60) { this.Left += gstep; this.Width -= gstep; _gripStartSize = this.Size; }
                    _gripStartScreen = Control.MousePosition; _gripStartLoc = this.Location; e.Handled = true;
                }
                else if (MatchHotkey(e, s.GetHotkey("GripUp")))
                {
                    if (_isGripTLDragging) { if (this.Height - gstep > 60) this.Height -= gstep; _gripStartSize = this.Size; }
                    else { this.Top -= gstep; this.Height += gstep; _gripStartSize = this.Size; }
                    _gripStartScreen = Control.MousePosition; _gripStartLoc = this.Location; e.Handled = true;
                }
                else if (MatchHotkey(e, s.GetHotkey("GripDown")))
                {
                    if (_isGripTLDragging) { this.Height += gstep; _gripStartSize = this.Size; }
                    else if (this.Height - gstep > 60) { this.Top += gstep; this.Height -= gstep; _gripStartSize = this.Size; }
                    _gripStartScreen = Control.MousePosition; _gripStartLoc = this.Location; e.Handled = true;
                }
                if (e.Handled) { Win32.SetCapture(this.Handle); this.Invalidate(); return; }
                return;
            }
            if (MatchHotkey(e, s.GetHotkey("MoveLeft"))) { this.Location = new Point(this.Left - step, this.Top); e.Handled = true; }
            else if (MatchHotkey(e, s.GetHotkey("MoveRight"))) { this.Location = new Point(this.Left + step, this.Top); e.Handled = true; }
            else if (MatchHotkey(e, s.GetHotkey("MoveUp"))) { this.Location = new Point(this.Left, this.Top - step); e.Handled = true; }
            else if (MatchHotkey(e, s.GetHotkey("MoveDown"))) { this.Location = new Point(this.Left, this.Top + step); e.Handled = true; }
            else if (MatchHotkey(e, s.GetHotkey("FineLeft"))) { this.Location = new Point(this.Left - fstep, this.Top); e.Handled = true; }
            else if (MatchHotkey(e, s.GetHotkey("FineRight"))) { this.Location = new Point(this.Left + fstep, this.Top); e.Handled = true; }
            else if (MatchHotkey(e, s.GetHotkey("FineUp"))) { this.Location = new Point(this.Left, this.Top - fstep); e.Handled = true; }
            else if (MatchHotkey(e, s.GetHotkey("FineDown"))) { this.Location = new Point(this.Left, this.Top + fstep); e.Handled = true; }
            else if (MatchHotkey(e, s.GetHotkey("NextMode"))) { _mode = NextMode(_mode); e.Handled = true; }
            else if (MatchHotkey(e, s.GetHotkey("NextOrigin"))) { _origin = NextOrigin(_origin); UpdateGoldenOverlay(); e.Handled = true; }
            else if (MatchHotkey(e, s.GetHotkey("RotateCCW"))) { _rotation -= s.RotateStep; if (_rotation < -360) _rotation += 360; UpdateGoldenOverlay(); e.Handled = true; }
            else if (MatchHotkey(e, s.GetHotkey("RotateCW"))) { _rotation += s.RotateStep; if (_rotation > 360) _rotation -= 360; UpdateGoldenOverlay(); e.Handled = true; }
            else if (MatchHotkey(e, s.GetHotkey("ResetRotation"))) { _rotation = 0; UpdateGoldenOverlay(); e.Handled = true; }
            else if (MatchHotkey(e, s.GetHotkey("TransparencyUp"))) { s.Transparency = Math.Min(255, s.Transparency + 10); UpdateTransparency(); e.Handled = true; }
            else if (MatchHotkey(e, s.GetHotkey("TransparencyDown"))) { s.Transparency = Math.Max(30, s.Transparency - 10); UpdateTransparency(); e.Handled = true; }
            else if (MatchHotkey(e, s.GetHotkey("Mode1"))) { _mode = GridMode.Classic; e.Handled = true; }
            else if (MatchHotkey(e, s.GetHotkey("Mode2"))) { _mode = GridMode.GoldenSection; e.Handled = true; }
            else if (MatchHotkey(e, s.GetHotkey("Mode3"))) { _mode = GridMode.Square; e.Handled = true; }
            else if (MatchHotkey(e, s.GetHotkey("Mode4"))) { _mode = GridMode.Spiral; e.Handled = true; }
            else if (MatchHotkey(e, s.GetHotkey("Mode5"))) { _mode = GridMode.Circles; e.Handled = true; }
            else if (MatchHotkey(e, s.GetHotkey("Mode6"))) { _mode = GridMode.FixedGrid; e.Handled = true; }
            else if (MatchHotkey(e, s.GetHotkey("ZoomIn"))) { if (_mode == GridMode.FixedGrid) _gridCellSize = Math.Min(500, _gridCellSize + s.GridStep); else this.Size = new Size((int)(this.Width * 1.1), (int)(this.Height * 1.1)); e.Handled = true; }
            else if (MatchHotkey(e, s.GetHotkey("ZoomOut"))) { if (_mode == GridMode.FixedGrid) _gridCellSize = Math.Max(5, _gridCellSize - s.GridStep); else if (this.Width > 50 && this.Height > 50) this.Size = new Size((int)(this.Width * 0.9), (int)(this.Height * 0.9)); e.Handled = true; }
            else if (MatchHotkey(e, s.GetHotkey("ToggleTitleBar"))) { _showTitleBar = !_showTitleBar; e.Handled = true; }
            else if (MatchHotkey(e, s.GetHotkey("ToggleSettings"))) { ToggleSettings(); e.Handled = true; }
            else if (MatchHotkey(e, s.GetHotkey("ToggleLineMode"))) { s.LineModeEnum = s.LineModeEnum == LineMode.DualColor ? LineMode.SingleColor : LineMode.DualColor; e.Handled = true; }
            else if (MatchHotkey(e, s.GetHotkey("Exit"))) { this.Close(); e.Handled = true; }

            if (e.Handled) this.Invalidate();
        }

        protected override void OnKeyUp(KeyEventArgs e) { base.OnKeyUp(e); if (e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.Shift) _shiftDown = false; }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (_rotationInputMode)
            { if (char.IsDigit(e.KeyChar) || e.KeyChar == '.') { _rotationInputText += e.KeyChar; double v; _rotationInputPreview = double.TryParse(_rotationInputText, out v) ? v : _preInputRotation; e.Handled = true; this.Invalidate(); } else e.Handled = true; }
        }

        private void ToggleSettings()
        {
            if (_settingsForm != null && _settingsForm.Visible) { _settingsForm.Hide(); return; }
            if (_settingsForm != null && !_settingsForm.IsDisposed) _settingsForm.Dispose();
            _settings = new RulerSettings(_settings);
            _settingsForm = new SettingsForm(_settings, this);
            _settingsForm.Show(); _settingsForm.BringToFront();
        }

        public void ApplySettings(RulerSettings s) { _settings = new RulerSettings(s); _settings.GridMode = (int)_mode; _settings.GridOrigin = (int)_origin; _settings.GridCellSize = _gridCellSize; UpdateTransparency(); SaveSettings(_settings); RegisterGlobalHotkeys(); this.Invalidate(); }
        protected override void OnFormClosing(FormClosingEventArgs e) { Win32.UnregisterHotKey(this.Handle, _ghkFocusId); Win32.UnregisterHotKey(this.Handle, _ghkFocusMouseId); _settings.GridMode = (int)_mode; _settings.GridOrigin = (int)_origin; _settings.GridCellSize = _gridCellSize; SaveSettings(_settings); base.OnFormClosing(e); }
    }

    // ── 设置窗口 (TabControl: 参数 + 热键) ──
    internal class SettingsForm : Form
    {
        private RulerSettings _settings;
        private RulerForm _form;
        private NumericUpDown _moveStep, _shiftMoveStep, _rotateStep, _gridStep, _gripStep;
        private TrackBar _transparency;
        private CheckBox _showLabels;
        private ComboBox _lineModeCombo;
        private Button _btnColor;
        private Button _btnGoldenColor, _btnFocusColor;
        private TabControl _tabs;
        private HotkeyButton _capturingBtn;
        private bool _capturingShift, _capturingCtrl, _capturingAlt;

        private static readonly string[][] HKDefs = new[] {
            new[] {"移动", "左移 (MoveLeft)", "MoveLeft"}, new[] {"移动", "右移 (MoveRight)", "MoveRight"},
            new[] {"移动", "上移 (MoveUp)", "MoveUp"}, new[] {"移动", "下移 (MoveDown)", "MoveDown"},
            new[] {"微调", "微调左移 (FineLeft)", "FineLeft"}, new[] {"微调", "微调右移 (FineRight)", "FineRight"},
            new[] {"微调", "微调上移 (FineUp)", "FineUp"}, new[] {"微调", "微调下移 (FineDown)", "FineDown"},
            new[] {"把手扩展", "向左扩展 (GripLeft)", "GripLeft"}, new[] {"把手扩展", "向右收缩 (GripRight)", "GripRight"},
            new[] {"把手扩展", "向上扩展 (GripUp)", "GripUp"}, new[] {"把手扩展", "向下收缩 (GripDown)", "GripDown"},
            new[] {"模式", "切换模式 (NextMode)", "NextMode"}, new[] {"模式", "经典 (Mode1)", "Mode1"},
            new[] {"模式", "黄金分割 (Mode2)", "Mode2"}, new[] {"模式", "方块 (Mode3)", "Mode3"},
            new[] {"模式", "螺旋 (Mode4)", "Mode4"}, new[] {"模式", "圆环 (Mode5)", "Mode5"},
            new[] {"模式", "固定网格 (Mode6)", "Mode6"}, new[] {"模式", "切换原点 (NextOrigin)", "NextOrigin"},
            new[] {"旋转", "逆时针 (RotateCCW)", "RotateCCW"}, new[] {"旋转", "顺时针 (RotateCW)", "RotateCW"},
            new[] {"旋转", "复位 (ResetRotation)", "ResetRotation"}, new[] {"旋转", "输入角度 (RotateInput)", "RotateInput"},
            new[] {"缩放", "放大 (ZoomIn)", "ZoomIn"}, new[] {"缩放", "缩小 (ZoomOut)", "ZoomOut"},
            new[] {"黄金线", "横向黄金线 (GoldenHLine)", "GoldenHLine"}, new[] {"黄金线", "竖向黄金线 (GoldenVLine)", "GoldenVLine"},
            new[] {"UI", "透明度+ (TransparencyUp)", "TransparencyUp"}, new[] {"UI", "透明度- (TransparencyDown)", "TransparencyDown"},
            new[] {"UI", "标题栏 (ToggleTitleBar)", "ToggleTitleBar"}, new[] {"UI", "设置 (ToggleSettings)", "ToggleSettings"},
            new[] {"UI", "切换线条模式 (ToggleLineMode)", "ToggleLineMode"},
            new[] {"UI", "退出 (Exit)", "Exit"},
            new[] {"全局", "获取焦点 (GlobalFocus)", "GlobalFocus"}, new[] {"全局", "移至鼠标 (GlobalFocusAtMouse)", "GlobalFocusAtMouse"},
        };

        public SettingsForm(RulerSettings settings, RulerForm form)
        {
            _settings = settings; _form = form;
            this.Text = "GoldenRuler 设置";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false; this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(form.Location.X + form.Width + 10, form.Location.Y);
            this.ClientSize = new Size(360, 610);
            this.TopMost = true;
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;
            this.KeyPreview = true;
            this.KeyDown += OnCaptureKeyDown;
            this.PreviewKeyDown += (s, e) => { if (_capturingBtn != null && (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab || e.KeyCode == Keys.Space)) e.IsInputKey = true; };

            _tabs = new TabControl { Location = new Point(0, 0), Size = new Size(360, 560), Appearance = TabAppearance.Normal };
            var tp1 = new TabPage("参数");
            var tp2 = new TabPage("热键");
            tp1.BackColor = tp2.BackColor = Color.FromArgb(45, 45, 48);
            tp1.ForeColor = tp2.ForeColor = Color.White;
            _tabs.TabPages.Add(tp1);
            _tabs.TabPages.Add(tp2);
            this.Controls.Add(_tabs);
            BuildParamsTab(tp1);
            BuildHotkeysTab(tp2);

            Button btnApply = new Button { Text = "保存并关闭", Location = new Point(10, 570), Width = 120, Height = 28, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnApply.Click += (s, e) => { _form.ApplySettings(_settings); this.Hide(); };
            this.Controls.Add(btnApply);
            Button btnReset = new Button { Text = "重置默认", Location = new Point(145, 570), Width = 90, Height = 28, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnReset.Click += (s, e) => { var d = new RulerSettings(); _settings.Hotkeys.Clear(); d.InitDefaultHotkeys(); foreach (var h in d.Hotkeys) _settings.Hotkeys.Add(new HotkeyEntry(h.Action, h.Key)); _moveStep.Value = d.MoveStep; _shiftMoveStep.Value = d.ShiftMoveStep; _rotateStep.Value = (decimal)d.RotateStep; _gridStep.Value = d.GridStep; _gripStep.Value = d.GripStep; _transparency.Value = d.Transparency; _lineModeCombo.SelectedIndex = (int)d.LineModeEnum; _btnColor.BackColor = d.LineColor; _btnGoldenColor.BackColor = d.GoldenLineColor; _showLabels.Checked = d.ShowLabels; _form.ApplySettings(_settings); RebuildHotkeyButtons(); };
            this.Controls.Add(btnReset);
        }

        private void BuildParamsTab(TabPage tp)
        {
            int y = 15, labelW = 130, inputX = 145;
            AddLabel(tp, "移动步长 (px):", labelW, y); _moveStep = AddNumeric(tp, inputX, y, 1, 500, _settings.MoveStep); _moveStep.ValueChanged += (s, e) => { _settings.MoveStep = (int)_moveStep.Value; _form.ApplySettings(_settings); }; y += 32;
            AddLabel(tp, "微调步长 (px):", labelW, y); _shiftMoveStep = AddNumeric(tp, inputX, y, 1, 500, _settings.ShiftMoveStep); _shiftMoveStep.ValueChanged += (s, e) => { _settings.ShiftMoveStep = (int)_shiftMoveStep.Value; _form.ApplySettings(_settings); }; y += 32;
            AddLabel(tp, "旋转步长 (度):", labelW, y); _rotateStep = AddNumeric(tp, inputX, y, 1, 90, (decimal)_settings.RotateStep); _rotateStep.ValueChanged += (s, e) => { _settings.RotateStep = (double)_rotateStep.Value; _form.ApplySettings(_settings); }; y += 32;
            AddLabel(tp, "网格步进 (px):", labelW, y); _gridStep = AddNumeric(tp, inputX, y, 1, 100, _settings.GridStep); _gridStep.ValueChanged += (s, e) => { _settings.GridStep = (int)_gridStep.Value; _form.ApplySettings(_settings); }; y += 32;
            AddLabel(tp, "把手扩展步进 (px):", labelW, y); _gripStep = AddNumeric(tp, inputX, y, 1, 500, _settings.GripStep); _gripStep.ValueChanged += (s, e) => { _settings.GripStep = (int)_gripStep.Value; _form.ApplySettings(_settings); }; y += 35;
            AddLabel(tp, "线条粗细 (px):", labelW, y); var lwNum = new NumericUpDown { Location = new Point(inputX, y - 2), Width = 60, Minimum = 0.5m, Maximum = 5m, DecimalPlaces = 1, Increment = 0.5m, Value = (decimal)_settings.LineWidth, BackColor = Color.White, ForeColor = Color.Black }; lwNum.ValueChanged += (s, e) => { _settings.LineWidth = (float)lwNum.Value; _form.ApplySettings(_settings); }; tp.Controls.Add(lwNum); y += 35;
            AddLabel(tp, "透明度:", labelW, y); _transparency = new TrackBar { Location = new Point(inputX, y - 3), Width = 150, Minimum = 30, Maximum = 255, Value = _settings.Transparency, BackColor = Color.FromArgb(45, 45, 48) }; _transparency.Scroll += (s, e) => { _settings.Transparency = _transparency.Value; _form.ApplySettings(_settings); }; tp.Controls.Add(_transparency); y += 38;
            AddLabel(tp, "线条模式:", labelW, y); _lineModeCombo = new ComboBox { Location = new Point(inputX, y - 2), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.White, ForeColor = Color.Black }; _lineModeCombo.Items.AddRange(new object[] { "双色交替 (黑白)", "单色实线" }); _lineModeCombo.SelectedIndex = (int)_settings.LineModeEnum; _lineModeCombo.SelectedIndexChanged += (s, e) => { _settings.LineModeEnum = (LineMode)_lineModeCombo.SelectedIndex; _form.ApplySettings(_settings); }; tp.Controls.Add(_lineModeCombo); y += 32;
            AddLabel(tp, "单色线颜色:", labelW, y); _btnColor = new Button { Location = new Point(inputX, y - 2), Width = 60, Height = 24, BackColor = _settings.LineColor, FlatStyle = FlatStyle.Flat }; _btnColor.Click += (s, e) => { using (var cd = new ColorDialog()) { cd.Color = _settings.LineColor; if (cd.ShowDialog() == DialogResult.OK) { _settings.LineColor = cd.Color; _btnColor.BackColor = cd.Color; _form.ApplySettings(_settings); } } }; tp.Controls.Add(_btnColor); y += 32;
            AddLabel(tp, "黄金辅助线颜色:", labelW, y); _btnGoldenColor = new Button { Location = new Point(inputX, y - 2), Width = 60, Height = 24, BackColor = _settings.GoldenLineColor, FlatStyle = FlatStyle.Flat }; _btnGoldenColor.Click += (s, e) => { using (var cd = new ColorDialog()) { cd.Color = _settings.GoldenLineColor; if (cd.ShowDialog() == DialogResult.OK) { _settings.GoldenLineColor = cd.Color; _btnGoldenColor.BackColor = cd.Color; _form.ApplySettings(_settings); } } }; tp.Controls.Add(_btnGoldenColor); y += 32;
            AddLabel(tp, "焦点描边颜色:", labelW, y); _btnFocusColor = new Button { Location = new Point(inputX, y - 2), Width = 60, Height = 24, BackColor = _settings.FocusBorderColor, FlatStyle = FlatStyle.Flat }; _btnFocusColor.Click += (s, e) => { using (var cd = new ColorDialog()) { cd.Color = _settings.FocusBorderColor; if (cd.ShowDialog() == DialogResult.OK) { _settings.FocusBorderColor = cd.Color; _btnFocusColor.BackColor = cd.Color; _form.ApplySettings(_settings); } } }; tp.Controls.Add(_btnFocusColor); y += 32;
            _showLabels = new CheckBox { Location = new Point(15, y), Width = 200, Text = "显示像素标注", ForeColor = Color.White, BackColor = Color.FromArgb(45, 45, 48), Checked = _settings.ShowLabels }; _showLabels.CheckedChanged += (s, e) => { _settings.ShowLabels = _showLabels.Checked; _form.ApplySettings(_settings); }; tp.Controls.Add(_showLabels);
        }

        private List<HotkeyButton> _hkButtons = new List<HotkeyButton>();

        private void BuildHotkeysTab(TabPage tp)
        {
            var panel = new Panel { Location = new Point(0, 0), Size = new Size(355, 500), AutoScroll = true, BackColor = Color.FromArgb(45, 45, 48) };
            tp.Controls.Add(panel);
            int y = 5; string curCat = "";
            foreach (var def in HKDefs)
            {
                if (def[0] != curCat) { curCat = def[0]; var cl = new Label { Text = "[" + curCat + "]", Location = new Point(10, y), Width = 330, ForeColor = Color.FromArgb(100, 160, 255), BackColor = Color.FromArgb(45, 45, 48) }; panel.Controls.Add(cl); y += 22; }
                var lbl = new Label { Text = def[1], Location = new Point(15, y), Width = 200, ForeColor = Color.White, BackColor = Color.FromArgb(45, 45, 48) };
                panel.Controls.Add(lbl);
                var btn = new HotkeyButton { Action = def[2], Hotkey = _settings.GetHotkey(def[2]), Location = new Point(230, y - 2), Width = 100, Height = 22, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                btn.Click += (s, e) => { if (_capturingBtn != null) _capturingBtn.Capturing = false; _capturingBtn = btn; btn.Capturing = true; };
                _hkButtons.Add(btn);
                panel.Controls.Add(btn);
                y += 26;
            }
        }

        private void RebuildHotkeyButtons()
        {
            foreach (var b in _hkButtons) b.Hotkey = _settings.GetHotkey(b.Action);
        }

        private void OnCaptureKeyDown(object sender, KeyEventArgs e)
        {
            if (_capturingBtn != null)
            {
                if (e.KeyCode == Keys.Escape) { _capturingBtn.Capturing = false; _capturingBtn = null; e.Handled = true; e.SuppressKeyPress = true; return; }
                if (e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.Shift) { _capturingShift = true; e.Handled = true; e.SuppressKeyPress = true; return; }
                if (e.KeyCode == Keys.ControlKey || e.KeyCode == Keys.Control) { _capturingCtrl = true; e.Handled = true; e.SuppressKeyPress = true; return; }
                if (e.KeyCode == Keys.Menu || e.KeyCode == Keys.Alt) { _capturingAlt = true; e.Handled = true; e.SuppressKeyPress = true; return; }
                string hk = "";
                if (_capturingAlt || e.Alt) hk += "Alt+";
                if (_capturingCtrl || e.Control) hk += "Ctrl+";
                if (_capturingShift || e.Shift) hk += "Shift+";
                hk += e.KeyCode.ToString();
                _capturingBtn.Hotkey = hk;
                _capturingBtn.Capturing = false;
                _settings.SetHotkey(_capturingBtn.Action, hk);
                _form.ApplySettings(_settings);
                _capturingBtn = null;
                _capturingShift = _capturingCtrl = _capturingAlt = false;
                e.Handled = true; e.SuppressKeyPress = true;
            }
        }

        private void AddLabel(TabPage tp, string t, int w, int y) { var l = new Label { Location = new Point(15, y), Width = w, Text = t, ForeColor = Color.White, BackColor = Color.FromArgb(45, 45, 48) }; tp.Controls.Add(l); }
        private NumericUpDown AddNumeric(TabPage tp, int x, int y, int min, int max, decimal v) { var n = new NumericUpDown { Location = new Point(x, y - 2), Width = 65, Minimum = min, Maximum = max, Value = v, BackColor = Color.White, ForeColor = Color.Black }; tp.Controls.Add(n); return n; }
    }

    // ── 热键捕获按钮 ──
    internal class HotkeyButton : Button
    {
        public string Action = "";
        private string _hk = "";
        private bool _cap = false;

        public string Hotkey { get { return _hk; } set { _hk = value; if (!_cap) this.Text = FormatDisplay(value); } }
        public bool Capturing { get { return _cap; } set { _cap = value; this.Text = value ? "按键..." : FormatDisplay(_hk); this.BackColor = value ? Color.FromArgb(0, 122, 204) : Color.FromArgb(60, 60, 60); } }

        private static string FormatDisplay(string h)
        {
            if (string.IsNullOrEmpty(h)) return "无";
            string r = h;
            r = r.Replace("Oemplus", "+").Replace("OemMinus", "-").Replace("Oem6", "]").Replace("OemOpenBrackets", "[");
            r = r.Replace("D1", "1").Replace("D2", "2").Replace("D3", "3").Replace("D4", "4").Replace("D5", "5").Replace("D6", "6");
            r = r.Replace("Space", "空格").Replace("Enter", "回车").Replace("Escape", "Esc");
            return r;
        }
    }

    internal class GripPadForm : Form
    {
        private RulerForm _owner;
        private bool _isTopLeft;
        public GripPadForm(RulerForm owner, bool isTopLeft)
        {
            _owner = owner;
            _isTopLeft = isTopLeft;
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.TopMost = true;
            this.DoubleBuffered = true;
            this.BackColor = Color.Black;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }
        protected override bool ShowWithoutActivation { get { return true; } }
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x08000000 | 0x00080000 | 0x00000008; // WS_EX_NOACTIVATE | WS_EX_LAYERED | WS_EX_TOPMOST
                return cp;
            }
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Win32.SetLayeredWindowAttributes(this.Handle, 0, 1, Win32.LWA_ALPHA);
        }
        protected override void OnPaint(PaintEventArgs e) { }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            // 转发到主窗口 - 计算主窗口客户区坐标
            Point p = this.PointToScreen(e.Location);
            p = _owner.PointToClient(p);
            _owner.HandleGripMouseDown(_isTopLeft, new MouseEventArgs(e.Button, e.Clicks, p.X, p.Y, e.Delta));
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            Point p = this.PointToScreen(e.Location);
            p = _owner.PointToClient(p);
            _owner.HandleGripMouseMove(_isTopLeft, new MouseEventArgs(e.Button, e.Clicks, p.X, p.Y, e.Delta));
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            Point p = this.PointToScreen(e.Location);
            p = _owner.PointToClient(p);
            _owner.HandleGripMouseUp(_isTopLeft, new MouseEventArgs(e.Button, e.Clicks, p.X, p.Y, e.Delta));
        }
    }

    internal class GoldenOverlayForm : Form
    {
        private RulerForm _owner;
        public void SetOwner(RulerForm rf) { _owner = rf; }

        public GoldenOverlayForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.TopMost = true;
            this.DoubleBuffered = true;
            this.BackColor = Color.Fuchsia;
            this.TransparencyKey = Color.Fuchsia;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override bool ShowWithoutActivation { get { return true; } }
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x00000020 | 0x08000000 | 0x00080000 | 0x00000008;
                return cp;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Win32.SetLayeredWindowAttributes(this.Handle, (uint)(Color.Fuchsia.R | (Color.Fuchsia.G << 8) | (Color.Fuchsia.B << 16)), 255, Win32.LWA_COLORKEY);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;
            int w = this.ClientSize.Width, h = this.ClientSize.Height;
            if (w <= 0 || h <= 0) return;
            g.Clear(Color.Fuchsia);

            RulerForm rf = _owner;
            if (rf == null) return;
            if (!rf.ShowGoldenH && !rf.ShowGoldenV) return;

            int barH = rf.HasTitleBar ? 20 : 0;
            int gridY = barH;
            int gridH = h - gridY;
            if (gridH <= 0) return;

            double phi = 1.6180339887, inv = 1.0 / phi, big = 1.0 - inv;
            Color gc = rf.Settings.GoldenLineColor;
            double curRot = rf.Rotation;
            float cx = w / 2f, cy = gridY + gridH / 2f;

            g.TranslateTransform(cx, cy);
            g.RotateTransform((float)curRot);
            g.TranslateTransform(-cx, -cy);
            g.TranslateTransform(0, gridY);

            using (Pen gp = new Pen(Color.FromArgb(255, gc.R, gc.G, gc.B), Math.Max(2f, rf.Settings.LineWidth * 2f)))
            {
                if (rf.ShowGoldenH)
                {
                    float ya;
                    switch (rf.CurrentOrigin)
                    {
                        case GridOrigin.TopLeft: case GridOrigin.TopRight: ya = (float)(gridH * inv); break;
                        default: ya = (float)(gridH * big); break;
                    }
                    g.DrawLine(gp, -w, ya, 2 * w, ya);
                }
                if (rf.ShowGoldenV)
                {
                    float xa;
                    switch (rf.CurrentOrigin)
                    {
                        case GridOrigin.TopLeft: case GridOrigin.BottomLeft: xa = (float)(w * inv); break;
                        default: xa = (float)(w * big); break;
                    }
                    g.DrawLine(gp, xa, -gridH, xa, 2 * gridH);
                }
            }
            g.ResetTransform();
        }
    }

    internal static class Program
    {
        [STAThread]
        static void Main() { Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false); Application.Run(new RulerForm()); }
    }
}
