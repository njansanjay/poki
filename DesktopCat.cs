using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Poki;

internal static class CatSprites
{
    private static readonly Lazy<Bitmap> sheet = new(() =>
    {
        using var stream = typeof(CatSprites).Assembly.GetManifestResourceStream("Poki.Assets.cat-sprites.png")!;
        using var bitmap = new Bitmap(stream);
        return new Bitmap(bitmap);
    });
    public static Bitmap Sheet => sheet.Value;
    public static Rectangle Cell(int frame)
    {
        frame = Math.Clamp(frame, 0, 7);
        var left = (int)Math.Round(frame % 4 * Sheet.Width / 4.0);
        var top = (int)Math.Round(frame / 4 * Sheet.Height / 2.0);
        return Rectangle.FromLTRB(left, top, (int)Math.Round((frame % 4 + 1) * Sheet.Width / 4.0),
            (int)Math.Round((frame / 4 + 1) * Sheet.Height / 2.0));
    }
    public static void Draw(Graphics graphics, Rectangle destination, int frame = 4, bool left = false)
    {
        var state = graphics.Save();
        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        graphics.PixelOffsetMode = PixelOffsetMode.Half;
        if (left)
        {
            graphics.TranslateTransform(destination.Left * 2 + destination.Width, 0);
            graphics.ScaleTransform(-1, 1);
        }
        graphics.DrawImage(Sheet, destination, Cell(frame), GraphicsUnit.Pixel);
        graphics.Restore(state);
    }
}

// Time-based movement is independent of the Windows timer's frequency.
internal sealed class CatMotion
{
    public double X { get; private set; }
    public bool FacingLeft { get; private set; }
    public bool Walking { get; private set; }
    public int Frame { get; private set; } = 4;
    private double elapsed;
    private double stateTime = 2.5;
    private int trip;
    public void Place(double x) => X = x;
    public void Advance(double delta, Rectangle area, int size, bool animate, bool paused)
    {
        delta = Math.Clamp(delta, 0, 0.1);
        var min = area.Left;
        var max = Math.Max(min, area.Right - size);
        X = Math.Clamp(X, min, max);
        if (!animate || paused) { Frame = 4; return; }
        elapsed += delta;
        stateTime -= delta;
        if (stateTime <= 0)
        {
            Walking = !Walking;
            stateTime = Walking ? 7 + trip % 5 : 2.5 + trip % 3;
            if (Walking) trip++;
        }
        if (Walking)
        {
            X += (FacingLeft ? -1 : 1) * Math.Clamp(size * 0.36, 20, 70) * delta;
            if (X >= max) { X = max; FacingLeft = true; }
            if (X <= min) { X = min; FacingLeft = false; }
            Frame = (int)(elapsed * 7) % 4;
        }
        else
        {
            var blink = elapsed % 3.7;
            Frame = blink < 0.12 ? 5 : blink < 0.25 ? 6 : blink < 0.36 ? 5 : (int)(elapsed * 0.65) % 3 == 0 ? 7 : 4;
        }
    }
    public static Point GroundLocation(Rectangle area, int size, double x) => new(
        (int)Math.Round(Math.Clamp(x, area.Left, Math.Max(area.Left, area.Right - size))),
        Math.Max(area.Top, area.Bottom - size));
}

internal static class FullscreenDetector
{
    [StructLayout(LayoutKind.Sequential)] private struct NativeRect { public int Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)] private struct MonitorInfo
    {
        public int Size;
        public NativeRect Monitor;
        public NativeRect Work;
        public uint Flags;
    }
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr window, out NativeRect rectangle);
    [DllImport("user32.dll")] private static extern IntPtr MonitorFromWindow(IntPtr window, uint flags);
    [DllImport("user32.dll", CharSet = CharSet.Auto)] private static extern bool GetMonitorInfo(IntPtr monitor, ref MonitorInfo info);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);
    [DllImport("user32.dll")] private static extern bool IsIconic(IntPtr window);

    internal static bool CoversMonitor(Rectangle window, Rectangle monitor, int tolerance = 2) =>
        window.Left <= monitor.Left + tolerance && window.Top <= monitor.Top + tolerance &&
        window.Right >= monitor.Right - tolerance && window.Bottom >= monitor.Bottom - tolerance;

    internal static bool IsForegroundFullscreen(Rectangle catMonitor, int ownProcessId)
    {
        var foreground = GetForegroundWindow();
        if (foreground == IntPtr.Zero || IsIconic(foreground)) return false;
        GetWindowThreadProcessId(foreground, out var processId);
        if (processId == ownProcessId) return false;
        var monitorHandle = MonitorFromWindow(foreground, 2);
        var info = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() };
        if (monitorHandle == IntPtr.Zero || !GetMonitorInfo(monitorHandle, ref info) || !GetWindowRect(foreground, out var native)) return false;
        var monitor = Rectangle.FromLTRB(info.Monitor.Left, info.Monitor.Top, info.Monitor.Right, info.Monitor.Bottom);
        if (monitor != catMonitor) return false;
        return CoversMonitor(Rectangle.FromLTRB(native.Left, native.Top, native.Right, native.Bottom), monitor);
    }
}

internal sealed class LayeredCat : IDisposable
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)] private struct Blend { public byte Operation, Flags, Alpha, Format; }
    [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr window);
    [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr window, IntPtr dc);
    [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleDC(IntPtr dc);
    [DllImport("gdi32.dll")] private static extern bool DeleteDC(IntPtr dc);
    [DllImport("gdi32.dll")] private static extern IntPtr SelectObject(IntPtr dc, IntPtr item);
    [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr item);
    [DllImport("user32.dll", SetLastError = true)] private static extern bool UpdateLayeredWindow(IntPtr window, IntPtr dc,
        ref Point destination, ref Size size, IntPtr sourceDC, ref Point source, int key, ref Blend blend, int flags);
    private readonly Dictionary<int, IntPtr> frames = new();
    private Size canvas;
    private IntPtr memory, original;
    private int displayed = -1;
    private Point previousLocation;
    public void Render(Form window, int frame = 4, bool left = false, bool force = false)
    {
        if (canvas != window.Size || memory == IntPtr.Zero)
        {
            Dispose(); canvas = window.Size; memory = CreateCompatibleDC(IntPtr.Zero);
        }
        var key = frame + (left ? 8 : 0);
        if (!force && key == displayed && previousLocation == window.Location) return;
        if (!frames.TryGetValue(key, out var handle))
        {
            using var bitmap = new Bitmap(canvas.Width, canvas.Height, PixelFormat.Format32bppPArgb);
            using (var graphics = Graphics.FromImage(bitmap))
                CatSprites.Draw(graphics, new Rectangle(0, (int)(canvas.Height * 0.13), canvas.Width, canvas.Height), frame, left);
            handle = bitmap.GetHbitmap(Color.FromArgb(0)); frames.Add(key, handle);
        }
        var selected = SelectObject(memory, handle);
        if (original == IntPtr.Zero) original = selected;
        var dc = GetDC(IntPtr.Zero);
        try
        {
            var destination = window.Location; var size = window.Size; var source = Point.Empty;
            var blend = new Blend { Alpha = 255, Format = 1 };
            if (!UpdateLayeredWindow(window.Handle, dc, ref destination, ref size, memory, ref source, 0, ref blend, 2))
                throw new Win32Exception(Marshal.GetLastWin32Error());
            displayed = key; previousLocation = destination;
        }
        finally { ReleaseDC(IntPtr.Zero, dc); }
    }
    public void Dispose()
    {
        if (memory != IntPtr.Zero)
        {
            if (original != IntPtr.Zero) SelectObject(memory, original);
            foreach (var frame in frames.Values) DeleteObject(frame);
            DeleteDC(memory);
        }
        frames.Clear(); memory = original = IntPtr.Zero; displayed = -1;
    }
}
