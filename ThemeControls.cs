using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Toki;

internal static class UiTheme
{
    internal static readonly Color Canvas = Color.FromArgb(8, 9, 11);
    internal static readonly Color Sidebar = Color.FromArgb(12, 13, 16);
    internal static readonly Color Panel = Color.FromArgb(20, 22, 27);
    internal static readonly Color Field = Color.FromArgb(29, 31, 38);
    internal static readonly Color Border = Color.FromArgb(42, 45, 53);
    internal static readonly Color Accent = Color.FromArgb(148, 128, 255);
    internal static readonly Color Selected = Color.FromArgb(40, 34, 63);
    internal static readonly Color Hover = Color.FromArgb(33, 30, 46);
    internal static readonly Color Muted = Color.FromArgb(163, 168, 181);
    [DllImport("uxtheme.dll", EntryPoint = "#135")] private static extern int SetPreferredAppMode(int mode);
    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)] private static extern int SetWindowTheme(IntPtr hwnd, string name, string? list);
    [DllImport("dwmapi.dll")] private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);
    internal static void Initialize()
    {
        // The dark scrollbar theme is available starting with Windows 10 1903.
        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18362))
            try { SetPreferredAppMode(2); } catch (EntryPointNotFoundException) { }
    }
    internal static void DarkScrollbars(Control control) => SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
    internal static void DarkWindow(Form form) { var enabled = 1; DwmSetWindowAttribute(form.Handle, 20, ref enabled, sizeof(int)); }
    internal static GraphicsPath Round(Rectangle bounds, int radius = 8)
    {
        var path = new GraphicsPath();
        var d = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
        if (d <= 0) return path;
        path.AddArc(bounds.Left, bounds.Top, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Top, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}

// Keep native text editing, selection, IME and keyboard support inside a padded surface.
internal sealed class TokiTextBox : UserControl
{
    private readonly TextBox editor = new() { BorderStyle = BorderStyle.None };
    public bool Flat { get; set; }
    public override string Text { get => editor.Text; set => editor.Text = value; }
    public int TextLength => editor.TextLength;
    public string PlaceholderText { get => editor.PlaceholderText; set { editor.PlaceholderText = value; editor.AccessibleName = value; } }
    public bool Multiline { get => editor.Multiline; set { editor.Multiline = value; PerformLayout(); } }
    public bool UseSystemPasswordChar { get => editor.UseSystemPasswordChar; set => editor.UseSystemPasswordChar = value; }
    public bool ReadOnly { get => editor.ReadOnly; set => editor.ReadOnly = value; }
    public ScrollBars ScrollBars { get => editor.ScrollBars; set => editor.ScrollBars = value; }
    public TokiTextBox()
    {
        DoubleBuffered = true;
        BackColor = UiTheme.Field; ForeColor = Color.FromArgb(237, 241, 250);
        Height = 42; Margin = new Padding(0, 0, 0, 12);
        editor.BackColor = BackColor; editor.ForeColor = ForeColor;
        editor.Enter += (_, _) => Invalidate(); editor.Leave += (_, _) => Invalidate();
        editor.KeyDown += (_, e) => OnKeyDown(e);
        editor.TextChanged += (_, _) => OnTextChanged(EventArgs.Empty);
        editor.HandleCreated += (_, _) => UiTheme.DarkScrollbars(editor);
        Controls.Add(editor);
    }
    public void Clear() => editor.Clear();
    public void SelectAll() => editor.SelectAll();
    public new bool Focus() => editor.Focus();
    protected override void OnEnter(EventArgs e) { base.OnEnter(e); editor.Focus(); }
    protected override void OnBackColorChanged(EventArgs e) { base.OnBackColorChanged(e); if (editor != null) editor.BackColor = BackColor; }
    protected override void OnForeColorChanged(EventArgs e) { base.OnForeColorChanged(e); if (editor != null) editor.ForeColor = ForeColor; }
    protected override void OnFontChanged(EventArgs e) { base.OnFontChanged(e); if (editor != null) { editor.Font = Font; PerformLayout(); } }
    protected override void OnLayout(LayoutEventArgs e)
    {
        base.OnLayout(e);
        if (editor == null) return;
        var inset = Flat ? 0 : 12;
        editor.SetBounds(inset, Multiline ? (Flat ? 0 : 12) : Math.Max(0, (Height - editor.PreferredHeight) / 2),
            Math.Max(1, Width - inset * 2), Multiline ? Math.Max(1, Height - inset * 2) : editor.PreferredHeight);
    }
    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? BackColor);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = UiTheme.Round(new Rectangle(0, 0, Width - 1, Height - 1));
        using var fill = new SolidBrush(BackColor);
        if (Flat) e.Graphics.Clear(BackColor); else e.Graphics.FillPath(fill, path);
        if (!Flat) { using var pen = new Pen(ContainsFocus ? UiTheme.Accent : UiTheme.Border); e.Graphics.DrawPath(pen, path); }
    }
}

internal sealed class TokiComboBox : ComboBox
{
    public TokiComboBox()
    {
        BackColor = UiTheme.Field; ForeColor = Color.White; FlatStyle = FlatStyle.Flat;
        DrawMode = DrawMode.OwnerDrawFixed; ItemHeight = 28; DropDownStyle = ComboBoxStyle.DropDownList;
        Margin = new Padding(0, 0, 0, 10);
    }
    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        using var fill = new SolidBrush((e.State & DrawItemState.Selected) != 0 ? UiTheme.Selected : BackColor);
        e.Graphics.FillRectangle(fill, e.Bounds);
        if (e.Index >= 0) TextRenderer.DrawText(e.Graphics, GetItemText(Items[e.Index]), Font,
            Rectangle.Inflate(e.Bounds, -10, 0), ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);
        if (m.Msg != 0x000F || !IsHandleCreated) return;
        using var g = Graphics.FromHwnd(Handle);
        g.Clear(Parent?.BackColor ?? BackColor); g.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = UiTheme.Round(new Rectangle(0, 0, Width - 1, Height - 1));
        using var fill = new SolidBrush(BackColor); g.FillPath(fill, path);
        using var border = new Pen(Focused ? UiTheme.Accent : UiTheme.Border); g.DrawPath(border, path);
        TextRenderer.DrawText(g, Text, Font, new Rectangle(12, 0, Math.Max(1, Width - 42), Height), ForeColor,
            TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        using var arrow = new Pen(Color.FromArgb(170, 183, 205), 1.5f);
        g.DrawLines(arrow, new[] { new Point(Width - 23, Height / 2 - 2), new Point(Width - 18, Height / 2 + 3), new Point(Width - 13, Height / 2 - 2) });
    }
}

internal sealed class TokiButton : Button
{
    private bool hover;
    protected override void OnMouseEnter(EventArgs e) { hover = true; base.OnMouseEnter(e); Invalidate(); }
    protected override void OnMouseLeave(EventArgs e) { hover = false; base.OnMouseLeave(e); Invalidate(); }
    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.Clear(Parent?.BackColor ?? BackColor); e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = UiTheme.Round(new Rectangle(0, 0, Width - 1, Height - 1));
        using var fill = new SolidBrush(!Enabled ? UiTheme.Sidebar : hover ? FlatAppearance.MouseOverBackColor : BackColor);
        e.Graphics.FillPath(fill, path);
        if (Focused) { using var pen = new Pen(UiTheme.Accent); e.Graphics.DrawPath(pen, path); }
        var alignment = TextAlign switch
        {
            ContentAlignment.TopLeft or ContentAlignment.MiddleLeft or ContentAlignment.BottomLeft => TextFormatFlags.Left,
            ContentAlignment.TopRight or ContentAlignment.MiddleRight or ContentAlignment.BottomRight => TextFormatFlags.Right,
            _ => TextFormatFlags.HorizontalCenter
        };
        TextRenderer.DrawText(e.Graphics, Text, Font, Rectangle.Inflate(ClientRectangle, -12, 0), Enabled ? ForeColor : Color.FromArgb(134, 147, 169),
            alignment | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}
