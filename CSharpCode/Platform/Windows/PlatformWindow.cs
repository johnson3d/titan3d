using EngineNS.Graphics.Pipeline;
using SDL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace EngineNS
{
    partial class TtEngine
    {
        public EPlatformType CurrentPlatform
        {
            get
            {
                return EPlatformType.PLTF_Windows;
            }
        }
    }

    public partial class TtNativeWindow
    {
        private const uint WM_SETICON = 0x0080;
        private const uint WM_DESTROY = 0x0002;
        private const uint WM_CLOSE = 0x0010;
        private const int GWLP_WNDPROC = -4;
        private const int ICON_SMALL = 0;
        private const int ICON_BIG = 1;
        private const uint IMAGE_ICON = 1;
        private const uint LR_LOADFROMFILE = 0x00000010;

        private static readonly object WindowIconLock = new object();
        private static bool WindowIconLoadTried;
        private static IntPtr SmallWindowIcon = IntPtr.Zero;
        private static IntPtr BigWindowIcon = IntPtr.Zero;
        private IntPtr PreviousWndProc = IntPtr.Zero;
        private WndProcDelegate WindowCloseHook;
        private bool MainWindowCloseWatcherStarted;

        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        public enum DwmWindowAttribute : uint
        {
            NCRenderingEnabled = 1,
            NCRenderingPolicy,
            TransitionsForceDisabled,
            AllowNCPaint,
            CaptionButtonBounds,
            NonClientRtlLayout,
            ForceIconicRepresentation,
            Flip3DPolicy,
            ExtendedFrameBounds,
            HasIconicBitmap,
            DisallowPeek,
            ExcludedFromPeek,
            Cloak,
            Cloaked,
            FreezeRepresentation,
            PassiveUpdateMode,
            UseHostBackdropBrush,
            UseImmersiveDarkMode = 20,
            WindowCornerPreference = 33,
            BorderColor,
            CaptionColor,
            TextColor,
            VisibleFrameBorderThickness,
            SystemBackdropType,
            Last
        }

        [DllImport("kernel32.dll")]
        public static extern int SetDllDirectoryA(string path);
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibraryA(string path);
        [DllImport("user32.dll")]
        public static extern int MessageBoxA(IntPtr hWnd, string lpText,string lpCaption, int uType);
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        static extern bool IsWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr LoadImageW(IntPtr hInst, string name, uint type, int cx, int cy, uint fuLoad);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr SendMessageW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
        static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
        static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr CallWindowProcW(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetClassNameW(IntPtr hWnd, StringBuilder className, int maxCount);
        [DllImport("dwmapi.dll", PreserveSig = true)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attr, ref int attrValue, int attrSize);

        public bool IsPlatformWindowDestroyed
        {
            get
            {
                var hwnd = HWindow;
                return hwnd != IntPtr.Zero && !IsWindow(hwnd);
            }
        }

        public bool IsPlatformWindowVisible
        {
            get
            {
                var hwnd = HWindow;
                return hwnd != IntPtr.Zero && IsWindowVisible(hwnd);
            }
        }

        unsafe partial void ApplyDefaultWindowIcon()
        {
            var hwnd = HWindow;
            if (hwnd == IntPtr.Zero)
                return;

            EnsureWindowIconsLoaded();
            if (SmallWindowIcon != IntPtr.Zero)
                SendMessageW(hwnd, WM_SETICON, (IntPtr)ICON_SMALL, SmallWindowIcon);
            if (BigWindowIcon != IntPtr.Zero)
                SendMessageW(hwnd, WM_SETICON, (IntPtr)ICON_BIG, BigWindowIcon);
        }

        partial void InstallWindowCloseHook()
        {
            StartMainWindowCloseWatcher();

            var hwnd = HWindow;
            if (hwnd == IntPtr.Zero || PreviousWndProc != IntPtr.Zero)
                return;

            WindowCloseHook = WindowProc;
            var hookPtr = Marshal.GetFunctionPointerForDelegate(WindowCloseHook);
            PreviousWndProc = IntPtr.Size == 8
                ? SetWindowLongPtr64(hwnd, GWLP_WNDPROC, hookPtr)
                : new IntPtr(SetWindowLong32(hwnd, GWLP_WNDPROC, hookPtr.ToInt32()));

        }

        private IntPtr WindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WM_CLOSE || msg == WM_DESTROY)
            {
                TtEngine.Instance?.PostQuitMessage();

                if (msg == WM_DESTROY && hWnd == mHWindow)
                {
                    Window = IntPtr.Zero;
                    mWindowID = 0;
                }
            }

            if (PreviousWndProc != IntPtr.Zero)
                return CallWindowProcW(PreviousWndProc, hWnd, msg, wParam, lParam);

            return IntPtr.Zero;
        }

        private void StartMainWindowCloseWatcher()
        {
            if (MainWindowCloseWatcherStarted)
                return;
            if (this is TtPresentWindow presentWindow && presentWindow.IsCreatedByImGui)
                return;

            MainWindowCloseWatcherStarted = true;
            var weakWindow = new WeakReference<TtNativeWindow>(this);
            var thread = new System.Threading.Thread(() =>
            {
                var sawMainWindow = false;
                while (weakWindow.TryGetTarget(out _))
                {
                    try
                    {
                        if (CurrentProcessHasVisibleSdlWindow())
                        {
                            sawMainWindow = true;
                        }
                        else if (sawMainWindow)
                        {
                            TtEngine.Instance?.PostQuitMessage();
                            for (int i = 0; i < 10; i++)
                            {
                                System.Threading.Thread.Sleep(100);
                                if (CurrentProcessHasVisibleSdlWindow())
                                    return;
                            }
                            Environment.Exit(0);
                            return;
                        }
                    }
                    catch
                    {
                        return;
                    }

                    System.Threading.Thread.Sleep(200);
                }
            });
            thread.IsBackground = true;
            thread.Name = "TitanMainWindowCloseWatcher";
            thread.Start();
        }

        private static bool CurrentProcessHasVisibleSdlWindow()
        {
            var targetProcessId = (uint)Process.GetCurrentProcess().Id;
            var found = false;
            EnumWindowsProc proc = (hWnd, lParam) =>
            {
                GetWindowThreadProcessId(hWnd, out var windowProcessId);
                if (windowProcessId != targetProcessId || IsWindowVisible(hWnd) == false)
                    return true;

                var className = new StringBuilder(256);
                GetClassNameW(hWnd, className, className.Capacity);
                if (className.ToString() == "SDL_app")
                {
                    found = true;
                    return false;
                }

                return true;
            };
            EnumWindows(proc, IntPtr.Zero);
            return found;
        }

        private static void EnsureWindowIconsLoaded()
        {
            if (WindowIconLoadTried)
                return;

            lock (WindowIconLock)
            {
                if (WindowIconLoadTried)
                    return;

                var iconPath = GetDefaultWindowIconPath();
                if (iconPath != null)
                {
                    SmallWindowIcon = LoadImageW(IntPtr.Zero, iconPath, IMAGE_ICON, 16, 16, LR_LOADFROMFILE);
                    BigWindowIcon = LoadImageW(IntPtr.Zero, iconPath, IMAGE_ICON, 32, 32, LR_LOADFROMFILE);
                }
                WindowIconLoadTried = true;
            }
        }

        private static string GetDefaultWindowIconPath()
        {
            var baseDirectory = AppContext.BaseDirectory;
            var candidates = new[]
            {
                Path.Combine(baseDirectory, "Resources", "TitanEditor.ico"),
                Path.Combine(baseDirectory, "TitanEditor.ico"),
            };

            foreach (var i in candidates)
            {
                if (File.Exists(i))
                    return i;
            }
            return null;
        }
    }
}
