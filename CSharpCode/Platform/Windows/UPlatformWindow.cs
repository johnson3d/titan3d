using EngineNS.Graphics.Pipeline;
using SDL;
using System;
using System.Collections.Generic;
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
        [DllImport("dwmapi.dll", PreserveSig = true)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attr, ref int attrValue, int attrSize);
    }
}
