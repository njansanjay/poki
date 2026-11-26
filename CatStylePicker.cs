using System.Drawing.Drawing2D;

namespace Poki;

// Five discrete illustrations selected by an inset pill slider.
internal sealed class CatStylePicker : Control
{
    private int selected = 2;
    private bool dragging;
    public event EventHandler? ValueChanged;
    public int Value
    {
        get => selected;
        set
        {
            value = Math.Clamp(value, 0, CatSprites.Styles.Length - 1);
            if (selected == value) return;
            selected = value; AccessibleDescription = CatSprites.Styles[value] + " cat";
            Invalidate(); ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    public CatStylePicker()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
        Size = new Size(560, 160); TabStop = true; Cursor = Cursors.Hand;
        AccessibleName = "Cat style"; AccessibleDescription = "Original cat";
        AccessibleRole = AccessibleRole.Slider;
    }
    private float LeftStop => 30;
    private float Span => Math.Max(1, Width - 60);
    private float Stop(int index) => LeftStop + index * Span / 4;
    private void SelectAt(int x) => Value = (int)Math.Round(Math.Clamp((x - LeftStop) / Span, 0, 1) * 4);
    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left) return;
        Focus(); dragging = true; Capture = true; SelectAt(e.X);
    }
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e); if (dragging) SelectAt(e.X);
    }
    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e); if (e.Button == MouseButtons.Left) { dragging = false; Capture = false; }
    }
    protected override void OnMouseCaptureChanged(EventArgs e)
    {
        base.OnMouseCaptureChanged(e); if (!Capture) dragging = false;
    }
    protected override bool IsInputKey(Keys keyData) => keyData is Keys.Left or Keys.Right or Keys.Home or Keys.End || base.IsInputKey(keyData);
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        switch (e.KeyCode)
        {
            case Keys.Left: case Keys.Down: Value--; break;
            case Keys.Right: case Keys.Up: Value++; break;
            case Keys.Home: Value = 0; break;
            case Keys.End: Value = 4; break;
            default: return;
        }
        e.Handled = true;
    }
    protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); Invalidate(); }
    protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); Invalidate(); }
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var graphics = e.Graphics;
        var previewSize = Math.Min(88, (int)(Span / 4 + 12));
        for (var i = 0; i < CatSprites.Styles.Length; i++)
            CatSprites.Draw(graphics, new Rectangle((int)Stop(i) - previewSize / 2, 94 - previewSize, previewSize, previewSize), style: CatSprites.Styles[i]);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var track = new RectangleF(10, 116, Math.Max(1, Width - 20), 28);
        using var outline = Pill(track);
        using var gradient = new LinearGradientBrush(track, Color.FromArgb(132, 132, 132), Color.FromArgb(219, 219, 219), 90);
        graphics.FillPath(gradient, outline);
        using var rim = new Pen(Color.FromArgb(75, 75, 75)); graphics.DrawPath(rim, outline);
        var knob = new RectangleF(Stop(Value) - 21, 109, 42, 42);
        using var shadow = new SolidBrush(Color.FromArgb(85, 0, 0, 0));
        graphics.FillEllipse(shadow, knob.X + 1, knob.Y + 3, knob.Width + 1, knob.Height + 1);
        using var face = new LinearGradientBrush(knob, Color.White, Color.FromArgb(236, 236, 236), 90);
        graphics.FillEllipse(face, knob);
        using var border = new Pen(Focused ? UiTheme.Accent : Color.FromArgb(205, 205, 205), Focused ? 2 : 1);
        graphics.DrawEllipse(border, knob);
    }
    private static GraphicsPath Pill(RectangleF rectangle)
    {
        var path = new GraphicsPath(); var diameter = rectangle.Height;
        path.AddArc(rectangle.X, rectangle.Y, diameter, diameter, 90, 180);
        path.AddArc(rectangle.Right - diameter, rectangle.Y, diameter, diameter, 270, 180);
        path.CloseFigure(); return path;
    }
}
