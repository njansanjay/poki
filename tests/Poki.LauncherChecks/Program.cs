using System.Text.Json;
using System.Runtime.InteropServices;
using Poki;

internal static class Program
{
    private sealed class RobotCanvas : Form
    {
        protected override CreateParams CreateParams { get { var value = base.CreateParams; value.ExStyle |= 0x80000; return value; } }
    }
    [DllImport("user32.dll")] private static extern int GetGuiResources(IntPtr process, int flags);
    [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr window, int message, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr window, int command);
    [DllImport("user32.dll")] private static extern bool IsIconic(IntPtr window);
    [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr window, int index);
    [StructLayout(LayoutKind.Sequential)] private struct NativeRect { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr window, out NativeRect rect);
    private static int checks;
    private static void Check(bool result, string name)
    {
        if (!result) throw new Exception("FAILED: " + name);
        checks++; Console.WriteLine("PASS: " + name);
    }
    [STAThread]
    private static void Main(string[] args)
    {
        if (args.Contains("--cat-window")) { CheckCatWindow(); return; }
        if (args.Contains("--render-ui")) { RenderUi(); return; }
        CheckCatWindow();
        // No account access, app launches, network calls or writes to the user's POKI data.
        using (var field = new PokiTextBox { Size = new Size(320, 44), Text = "Draft", PlaceholderText = "Message" })
        {
            var editor = field.Controls.OfType<TextBox>().Single();
            field.PerformLayout();
            Check(editor.BorderStyle == BorderStyle.None && editor.BackColor == field.BackColor,
                "Text editor uses the field surface without a second native border");
            Check(editor.Left >= 10 && editor.Right <= field.Width - 10 && editor.Top >= 0 && editor.Bottom <= field.Height,
                "Single-line text has padding and fits its field");
            var changed = false;
            field.TextChanged += (_, _) => changed = true;
            editor.Text = "Edited draft";
            Check(changed && field.Text == "Edited draft", "Native editing updates the field value and change event");
            field.Multiline = true; field.Height = 120; field.PerformLayout();
            Check(editor.Height >= 90 && editor.Bottom <= field.Height - 10, "Multiline editor fills its padded field");
            field.Clear();
            Check(editor.TextLength == 0 && field.TextLength == 0, "Clearing the composer clears the native editor");
        }
        using (var choice = new PokiComboBox())
        {
            choice.Items.AddRange(new object[] { "None", "Daily", "Weekly" }); choice.SelectedIndex = 1;
            Check(choice.Text == "Daily" && choice.DrawMode == DrawMode.OwnerDrawFixed,
                "Themed dropdown preserves selected values and custom drawing");
        }
        using (var date = new DarkDateTimeInput())
        {
            var due = new DateTime(2027, 3, 17, 15, 45, 0); date.Value = due;
            Check(date.Value == due, "Dark date field preserves the selected date and time");
        }
        using (var number = new PokiNumber { Minimum = 40, Maximum = 192, Value = 96 })
        {
            number.Value = 500; Check(number.Value == 192, "Cat size control caps large values");
            number.Value = -1; Check(number.Value == 40, "Cat size control caps small values");
        }
        var oldSettings = JsonSerializer.Deserialize<AppData>("{}");
        Check(oldSettings?.PrimaryView == "Shortcuts", "Existing installations default to the shortcut launcher");
        Check(oldSettings?.WindowOpacity == 90, "Existing installations get a readable translucent window");
        Check(oldSettings?.HideCatInFullscreen == true, "Existing installations hide the cat during fullscreen use");
        var appearance = JsonSerializer.Deserialize<AppData>(JsonSerializer.Serialize(new AppData { WindowOpacity = 70 }));
        Check(appearance?.WindowOpacity == 70, "Window opacity persists with app settings");
        foreach (var name in PrimaryViews.Names)
        {
            var data = new AppData { PrimaryView = name };
            Check(JsonSerializer.Deserialize<AppData>(JsonSerializer.Serialize(data))?.PrimaryView == name, "Saved primary view: " + name);
        }
        Check(PrimaryViews.PageIndex("Chat") == 0 && PrimaryViews.PageIndex("Settings") == 4 && PrimaryViews.PageIndex("Shortcuts") == 5, "Primary views map to their actual pages");
        Check(PrimaryViews.PageIndex("obsolete") == 5, "Unknown preference falls back to Shortcuts");
        foreach (var area in new[] { new Rectangle(0, 0, 1366, 728), new Rectangle(-1920, -200, 1920, 1080) })
        {
            foreach (var anchor in new[] { new Rectangle(area.Left, area.Top, 64, 64), new Rectangle(area.Right - 64, area.Bottom - 64, 64, 64) })
            {
                var size = new Size(350, 480);
                Check(area.Contains(new Rectangle(PrimaryViews.PopupLocation(anchor, size, area), size)), "Popup remains within work area at " + anchor.Location);
            }
        }
        using (var robot = new Bitmap(BrandAssets.Robot))
        {
            Check(robot.GetPixel(0, 0).A == 0 && robot.GetPixel(robot.Width - 1, robot.Height - 1).A == 0, "Robot PNG has transparent corners");
            Check(robot.GetPixel(robot.Width / 2, robot.Height / 2).B > 150, "Robot PNG contains the blue logo");
        }
        using (var mark = new Bitmap(BrandAssets.CatMark))
        {
            Check(mark.GetPixel(0, 0).A == 0 && mark.GetPixel(mark.Width - 1, mark.Height - 1).A == 0, "Cat mark has transparent corners");
            var center = mark.GetPixel(mark.Width / 2, mark.Height / 2);
            Check(center.R > center.B, "Cat mark contains the warm orange identity");
        }
        using (var icon = BrandAssets.CreateIcon()) Check(icon.Width > 0, "Embedded Windows icon loads");
        for (var i = 0; i < 8; i++)
        {
            var cell = CatSprites.Cell(i);
            Check(cell.Width > 0 && new Rectangle(Point.Empty, CatSprites.Sheet.Size).Contains(cell), "Cat sprite frame is inside atlas: " + i);
        }
        var cat = new CatMotion(); cat.Place(230);
        var catArea = new Rectangle(-400, -100, 800, 600);
        var leftSeen = false; var rightSeen = false;
        for (var i = 0; i < 12000; i++)
        {
            cat.Advance(0.05, catArea, 96, true, false);
            if (cat.FacingLeft) leftSeen = true; else rightSeen = true;
            if (!catArea.Contains(new Rectangle(CatMotion.GroundLocation(catArea, 96, cat.X), new Size(96, 96))))
                throw new Exception("Cat escaped the working area");
        }
        Check(leftSeen && rightSeen, "Cat walks both ways and remains above the taskbar");
        var pausedX = cat.X;
        cat.Advance(0.1, catArea, 96, true, true);
        Check(cat.X == pausedX && cat.Frame == 4, "Cat stops under the pointer for easy clicking");
        cat.Advance(0.1, catArea, 96, false, false);
        Check(cat.X == pausedX && cat.Frame == 4, "Disabling motion leaves a seated cat");
        var monitor = new Rectangle(0, 0, 1920, 1080);
        Check(FullscreenDetector.CoversMonitor(new Rectangle(0, 0, 1920, 1080), monitor), "Fullscreen window covers the monitor");
        Check(!FullscreenDetector.CoversMonitor(new Rectangle(0, 0, 1920, 1040), monitor), "Maximized work-area window is not fullscreen");
        Check(FullscreenDetector.CoversMonitor(new Rectangle(-1, -1, 1922, 1082), monitor), "Bordered fullscreen window is detected with tolerance");
        Check(Math.Abs(LayeredRobot.FloatOffset(Math.PI / 2) - 3) < 0.001, "Floating motion stays within three pixels");
        Check(LayeredRobot.FloatOffset(0.1) > 0 && LayeredRobot.FloatOffset(0.1) < 1, "Floating motion retains fractional-pixel positions");
        using (var menuButton = new LauncherActionButton(true))
        using (var closeButton = new LauncherActionButton(false))
            Check(menuButton.Size == closeButton.Size, "Menu and close controls have identical hit areas");
        using (var canvas = new RobotCanvas { AutoScaleMode = AutoScaleMode.None, FormBorderStyle = FormBorderStyle.None, MinimumSize = new Size(64, 64), MaximumSize = new Size(64, 64), Size = new Size(64, 64) })
        {
            _ = canvas.Handle;
            Check(canvas.Width == canvas.Height, "Native floating canvas remains square");
            var location = canvas.Location;
            var resourcesBefore = GetGuiResources(System.Diagnostics.Process.GetCurrentProcess().Handle, 0);
            using (var renderer = new LayeredRobot())
            {
                for (var i = -24; i <= 24; i++) renderer.Render(canvas, i / 8f);
                var clock = System.Diagnostics.Stopwatch.StartNew();
                for (var i = 0; i < 600; i++) renderer.Render(canvas, LayeredRobot.FloatOffset(i * 0.03));
                Console.WriteLine($"Cached animation: {clock.Elapsed.TotalMilliseconds / 600:0.000} ms/frame (600 frames)");
                Check(canvas.Location == location, "Animation does not reposition the native window");
            }
            Check(GetGuiResources(System.Diagnostics.Process.GetCurrentProcess().Handle, 0) <= resourcesBefore + 2, "Animation releases its cached GDI frames");
        }
        foreach (var canvas in new[] { new Size(112, 40), new Size(40, 112), new Size(64, 64) })
        {
            var fit = BrandAssets.FitRobot(canvas);
            Check(Math.Abs((double)fit.Width / fit.Height - (double)BrandAssets.Robot.Width / BrandAssets.Robot.Height) < 0.02,
                "Robot keeps its proportions on canvas " + canvas);
            Check(new Rectangle(Point.Empty, canvas).Contains(fit), "Robot fits inside canvas " + canvas);
        }
        var testDirectory = Path.Combine(Path.GetTempPath(), "PokiLauncherChecks-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(testDirectory);
        try
        {
            var exe = Path.Combine(testDirectory, "example.exe"); File.WriteAllText(exe, "validation only; never executed");
            Check(ShortcutService.ValidatePath(exe) == exe, "Existing executable path accepted");
            var script = Path.Combine(testDirectory, "script.cmd"); File.WriteAllText(script, "never executed");
            try { ShortcutService.ValidatePath(script); throw new Exception("Script accepted"); } catch (ArgumentException) { checks++; }
            try { ShortcutService.ValidatePath(Path.Combine(testDirectory, "missing.exe")); throw new Exception("Missing file accepted"); } catch (FileNotFoundException) { checks++; }
            ShortcutService.PreloadAsync(new[] { new AppShortcut { Name = "POKI", Path = Environment.ProcessPath! } }).GetAwaiter().GetResult();
            using var applicationIcon = ShortcutService.LoadIconAsync(Environment.ProcessPath!).GetAwaiter().GetResult();
            using var cachedIconClone = ShortcutService.LoadIconAsync(Environment.ProcessPath!).GetAwaiter().GetResult();
            Check(applicationIcon.Width > 0, "Windows executable icon resolves asynchronously");
            Check(!ReferenceEquals(applicationIcon, cachedIconClone) && applicationIcon.Size == cachedIconClone.Size, "Shortcut cache returns independent ready-to-paint icon copies");
            var shellType = Type.GetTypeFromProgID("WScript.Shell")!;
            dynamic shell = Activator.CreateInstance(shellType)!;
            dynamic link = shell.CreateShortcut(Path.Combine(testDirectory, "Poki.lnk"));
            try
            {
                link.TargetPath = Environment.ProcessPath!; link.Save();
                var linkPath = ShortcutService.ValidatePath(Path.Combine(testDirectory, "Poki.lnk"));
                using var linkIcon = ShortcutService.LoadIconAsync(linkPath).GetAwaiter().GetResult();
                Check(linkIcon.Width > 0, "Windows .lnk icon resolves without launching its target");
            }
            finally { Marshal.FinalReleaseComObject(link); Marshal.FinalReleaseComObject(shell); }
            using var fallback = ShortcutService.LoadIconAsync(Path.Combine(testDirectory, "missing.exe")).GetAwaiter().GetResult();
            Check(fallback.Width > 0, "Moved app has a usable fallback icon");
        }
        finally
        {
            // Delete only files created in this uniquely named test directory.
            foreach (var file in Directory.EnumerateFiles(testDirectory)) File.Delete(file);
            Directory.Delete(testDirectory);
        }
        Console.WriteLine($"{checks} launcher checks passed.");
    }
    private static void CheckCatWindow()
    {
        var checksBefore = checks;
        // Only a cat window with fixture settings: no user data, accounts, tray, or startup writes.
        var app = (PokiApp)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(PokiApp));
        typeof(PokiApp).GetField("Data", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(app, new AppData { OrbSize = 80, OrbX = 400, OrbY = 400, OpenHotkey = "Off", HideCatInFullscreen = false, TopMost = false });
        using var orb = new OrbForm(app);
        typeof(PokiApp).GetField("orb", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(app, orb);
        orb.Show(); Pump(100);
        GetWindowRect(orb.Handle, out var initial);
        Check(initial.Right - initial.Left == 80 && initial.Bottom - initial.Top == 80, "Cat starts at configured native dimensions");
        SendMessage(orb.Handle, 0x0112, new IntPtr(0xF020), IntPtr.Zero); // SC_MINIMIZE: taskbar/dock toggle.
        Pump(100);
        GetWindowRect(orb.Handle, out var minimized);
        Console.WriteLine($"After shell minimize: state={orb.WindowState}, iconic={IsIconic(orb.Handle)}, native={minimized.Right - minimized.Left}x{minimized.Bottom - minimized.Top}");
        Check(!IsIconic(orb.Handle), "Taskbar minimize cannot minimize the desktop cat");
        for (var i = 0; i < 3; i++)
        {
            ShowWindow(orb.Handle, 6); // A dock can bypass WM_SYSCOMMAND with SW_MINIMIZE.
            Pump(120);
            GetWindowRect(orb.Handle, out var restored);
            Check(!IsIconic(orb.Handle) && orb.WindowState == FormWindowState.Normal &&
                restored.Right - restored.Left == 80 && restored.Bottom - restored.Top == 80 &&
                Math.Abs(restored.Left - initial.Left) <= 5 && restored.Top == initial.Top,
                "Direct native minimize recovers size and walking position: " + i);
        }
        SendMessage(orb.Handle, 0x0112, new IntPtr(0xF030), IntPtr.Zero); // SC_MAXIMIZE.
        Pump(100);
        Check(orb.WindowState == FormWindowState.Normal && orb.Size == new Size(80, 80), "Shell maximize cannot stretch the desktop cat");
        Check((GetWindowLong(orb.Handle, -20) & 0x80) != 0 && !orb.ShowInTaskbar, "Cat is a tool window, not a taskbar application window");
        var beforeWalk = orb.Left;
        Pump(3500);
        Check(orb.Left > beforeWalk && orb.Top == initial.Top, "Walking resumes along the same ground after shell commands");
        var launcherAnchor = orb.Bounds;
        app.ShowLauncher();
        Pump(500);
        using var launcher = (ShortcutLauncherForm)typeof(PokiApp)
            .GetField("launcher", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(app)!;
        Check(launcher.Visible && orb.Visible && orb.Bounds == launcherAnchor,
            "Opening shortcuts keeps the cat visible at the same size and position");
        var motion = (CatMotion)typeof(OrbForm)
            .GetField("motion", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(orb)!;
        Check(motion.Frame == 4 && !launcher.Bounds.IntersectsWith(orb.Bounds),
            "Cat sits idle below the launcher without being covered");
        Pump(500);
        Check(orb.Bounds == launcherAnchor && motion.Frame == 4, "Cat stays idle for as long as shortcuts remain open");
        launcher.Hide();
        Pump(500);
        Check(orb.Visible && orb.Left > launcherAnchor.Left && orb.Top == launcherAnchor.Top,
            "Closing shortcuts resumes walking without hiding the cat");
        Console.WriteLine($"{checks - checksBefore} cat window checks passed.");
    }
    private static void Pump(int milliseconds)
    {
        var clock = System.Diagnostics.Stopwatch.StartNew();
        while (clock.ElapsedMilliseconds < milliseconds) { Application.DoEvents(); Thread.Sleep(5); }
    }
    private static void RenderUi()
    {
        // Construct only the view. No tray, timers, account data, startup writes or app context are started.
        UiTheme.Initialize();
        var app = (PokiApp)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(PokiApp));
        typeof(PokiApp).GetField("Data", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .SetValue(app, new AppData { WindowOpacity = 100 });
        using var form = new ChatForm(app);
        // A non-top-level view hosted in a control can render without opening a desktop window.
        using var host = new Panel { Size = form.Size };
        form.TopLevel = false; host.Controls.Add(form); form.Visible = true;
        host.CreateControl(); form.CreateControl();
        Directory.CreateDirectory(".dist/ui-renders");
        foreach (var page in new[] { 4, 5, 0, 1, 2, 3 })
        {
            form.SelectTab(page); form.PerformLayout();
            using var bitmap = new Bitmap(form.Width, form.Height);
            form.DrawToBitmap(bitmap, form.ClientRectangle);
            bitmap.Save($".dist/ui-renders/page-{page}.png");
        }
        form.SelectTab(4);
        foreach (var category in new[] { "Appearance", "Desktop cat" })
        {
            typeof(ChatForm).GetMethod("ShowSettingsCategory", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(form, new object[] { category });
            form.PerformLayout(); using var image = new Bitmap(form.Width, form.Height);
            form.DrawToBitmap(image, form.ClientRectangle); image.Save($".dist/ui-renders/{category.Replace(' ', '-')}.png");
        }
        typeof(ChatForm).GetMethod("ShowSettingsCategory", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(form, new object[] { "General" });
        form.SelectTab(4); form.Size = form.MinimumSize; form.PerformLayout();
        using var small = new Bitmap(form.Width, form.Height);
        form.DrawToBitmap(small, form.ClientRectangle); small.Save(".dist/ui-renders/settings-small.png");
        Console.WriteLine("Rendered isolated UI with empty fixture data.");
    }
}
