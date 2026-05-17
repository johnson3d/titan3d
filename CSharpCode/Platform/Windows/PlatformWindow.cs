using EngineNS.Graphics.Pipeline;
using SDL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

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
        private const int ICON_SMALL = 0;
        private const int ICON_BIG = 1;
        private const uint IMAGE_ICON = 1;
        private const uint LR_LOADFROMFILE = 0x00000010;

        private static readonly object WindowIconLock = new object();
        private static bool WindowIconLoadTried;
        private static IntPtr SmallWindowIcon = IntPtr.Zero;
        private static IntPtr BigWindowIcon = IntPtr.Zero;

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
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr LoadImageW(IntPtr hInst, string name, uint type, int cx, int cy, uint fuLoad);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr SendMessageW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        [DllImport("dwmapi.dll", PreserveSig = true)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attr, ref int attrValue, int attrSize);

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
