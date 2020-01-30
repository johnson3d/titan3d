using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;
using System.IO;

namespace EngineNS
{
    public partial class CIPlatform
    {
        public CIPlatform()
        {
            Instance = this;
            mPlatformType = EPlatformType.PLATFORM_WIN;
            mProfileType = Profiler.TimeScope.EProfileFlag.Windows;
        }

        internal static class ExternDll
        {
            public const string Activeds = "activeds.dll";
            public const string Advapi32 = "advapi32.dll";
            public const string Comctl32 = "comctl32.dll";
            public const string Comdlg32 = "comdlg32.dll";
            public const string DwmAPI = "dwmapi.dll";
            public const string Gdi32 = "gdi32.dll";
            public const string Gdiplus = "gdiplus.dll";
            public const string Hhctrl = "hhctrl.ocx";
            public const string Imm32 = "imm32.dll";
            public const string Kernel32 = "kernel32.dll";
            public const string Loadperf = "Loadperf.dll";
            public const string Mqrt = "mqrt.dll";
            public const string Mscoree = "mscoree.dll";
            public const string MsDrm = "msdrm.dll";
            public const string Mshwgst = "mshwgst.dll";
            public const string Msi = "msi.dll";
            public const string NaturalLanguage6 = "naturallanguage6.dll";
            public const string Ntdll = "ntdll.dll";
            public const string Ole32 = "ole32.dll";
            public const string Oleacc = "oleacc.dll";
            public const string Oleaut32 = "oleaut32.dll";
            public const string Olepro32 = "olepro32.dll";
            public const string Penimc = "penimc2_v0400.dll";
            public const string PresentationHostDll = "PresentationHost_v0400.dll";
            public const string PresentationNativeDll = "PresentationNative_v0400.dll";
            public const string Psapi = "psapi.dll";
            public const string Shcore = "shcore.dll";
            public const string Shell32 = "shell32.dll";
            public const string Shfolder = "shfolder.dll";
            public const string Urlmon = "urlmon.dll";
            public const string User32 = "user32.dll";
            public const string Uxtheme = "uxtheme.dll";
            public const string Version = "version.dll";
            public const string Vsassert = "vsassert.dll";
            public const string Wininet = "wininet.dll";
            public const string Winmm = "winmm.dll";
            public const string Winspool = "winspool.drv";
            public const string WtsApi32 = "wtsapi32.dll";
        }

        byte[] KeyBoardStats = new byte[256];
        public bool IsKeyDown(Input.Device.Keyboard.Keys key)
        {
            return (KeyBoardStats[(int)key] & 0x80)==0x80;
        }

        public enum WindowMessage
        {
            WM_NULL = 0x0000,
            WM_CREATE = 0x0001,
            WM_DESTROY = 0x0002,
            WM_MOVE = 0x0003,
            WM_SIZE = 0x0005,
            WM_ACTIVATE = 0x0006,
            WM_SETFOCUS = 0x0007,
            WM_KILLFOCUS = 0x0008,
            WM_ENABLE = 0x000A,
            WM_SETREDRAW = 0x000B,
            WM_SETTEXT = 0x000C,
            WM_GETTEXT = 0x000D,
            WM_GETTEXTLENGTH = 0x000E,
            WM_PAINT = 0x000F,
            WM_CLOSE = 0x0010,
            WM_QUERYENDSESSION = 0x0011,
            WM_QUIT = 0x0012,
            WM_QUERYOPEN = 0x0013,
            WM_ERASEBKGND = 0x0014,
            WM_SYSCOLORCHANGE = 0x0015,
            WM_ENDSESSION = 0x0016,
            WM_SHOWWINDOW = 0x0018,
            WM_CTLCOLOR = 0x0019,
            WM_WININICHANGE = 0x001A,
            WM_SETTINGCHANGE = 0x001A,
            WM_DEVMODECHANGE = 0x001B,
            WM_ACTIVATEAPP = 0x001C,
            WM_FONTCHANGE = 0x001D,
            WM_TIMECHANGE = 0x001E,
            WM_CANCELMODE = 0x001F,
            WM_SETCURSOR = 0x0020,
            WM_MOUSEACTIVATE = 0x0021,
            WM_CHILDACTIVATE = 0x0022,
            WM_QUEUESYNC = 0x0023,
            WM_GETMINMAXINFO = 0x0024,
            WM_PAINTICON = 0x0026,
            WM_ICONERASEBKGND = 0x0027,
            WM_NEXTDLGCTL = 0x0028,
            WM_SPOOLERSTATUS = 0x002A,
            WM_DRAWITEM = 0x002B,
            WM_MEASUREITEM = 0x002C,
            WM_DELETEITEM = 0x002D,
            WM_VKEYTOITEM = 0x002E,
            WM_CHARTOITEM = 0x002F,
            WM_SETFONT = 0x0030,
            WM_GETFONT = 0x0031,
            WM_SETHOTKEY = 0x0032,
            WM_GETHOTKEY = 0x0033,
            WM_QUERYDRAGICON = 0x0037,
            WM_COMPAREITEM = 0x0039,
            WM_GETOBJECT = 0x003D,
            WM_COMPACTING = 0x0041,
            WM_COMMNOTIFY = 0x0044,
            WM_WINDOWPOSCHANGING = 0x0046,
            WM_WINDOWPOSCHANGED = 0x0047,
            WM_POWER = 0x0048,
            WM_COPYDATA = 0x004A,
            WM_CANCELJOURNAL = 0x004B,
            WM_NOTIFY = 0x004E,
            WM_INPUTLANGCHANGEREQUEST = 0x0050,
            WM_INPUTLANGCHANGE = 0x0051,
            WM_TCARD = 0x0052,
            WM_HELP = 0x0053,
            WM_USERCHANGED = 0x0054,
            WM_NOTIFYFORMAT = 0x0055,

            WM_CONTEXTMENU = 0x007B,
            WM_STYLECHANGING = 0x007C,
            WM_STYLECHANGED = 0x007D,
            WM_DISPLAYCHANGE = 0x007E,
            WM_GETICON = 0x007F,
            WM_SETICON = 0x0080,
            WM_NCCREATE = 0x0081,
            WM_NCDESTROY = 0x0082,
            WM_NCCALCSIZE = 0x0083,
            WM_NCHITTEST = 0x0084,
            WM_NCPAINT = 0x0085,
            WM_NCACTIVATE = 0x0086,
            WM_GETDLGCODE = 0x0087,
            WM_SYNCPAINT = 0x0088,
            WM_MOUSEQUERY = 0x009B,
            WM_NCMOUSEMOVE = 0x00A0,
            WM_NCLBUTTONDOWN = 0x00A1,
            WM_NCLBUTTONUP = 0x00A2,
            WM_NCLBUTTONDBLCLK = 0x00A3,
            WM_NCRBUTTONDOWN = 0x00A4,
            WM_NCRBUTTONUP = 0x00A5,
            WM_NCRBUTTONDBLCLK = 0x00A6,
            WM_NCMBUTTONDOWN = 0x00A7,
            WM_NCMBUTTONUP = 0x00A8,
            WM_NCMBUTTONDBLCLK = 0x00A9,
            WM_NCXBUTTONDOWN = 0x00AB,
            WM_NCXBUTTONUP = 0x00AC,
            WM_NCXBUTTONDBLCLK = 0x00AD,
            WM_INPUT = 0x00FF,
            WM_KEYFIRST = 0x0100,
            WM_KEYDOWN = 0x0100,
            WM_KEYUP = 0x0101,
            WM_CHAR = 0x0102,
            WM_DEADCHAR = 0x0103,

            WM_SYSKEYDOWN = 0x0104,
            WM_SYSKEYUP = 0x0105,
            WM_SYSCHAR = 0x0106,
            WM_SYSDEADCHAR = 0x0107,
            WM_KEYLAST = 0x0108,
            WM_IME_STARTCOMPOSITION = 0x010D,
            WM_IME_ENDCOMPOSITION = 0x010E,
            WM_IME_COMPOSITION = 0x010F,
            WM_IME_KEYLAST = 0x010F,
            WM_INITDIALOG = 0x0110,

            WM_COMMAND = 0x0111,
            WM_SYSCOMMAND = 0x0112,
            WM_TIMER = 0x0113,
            WM_HSCROLL = 0x0114,
            WM_VSCROLL = 0x0115,
            WM_INITMENU = 0x0116,
            WM_INITMENUPOPUP = 0x0117,
            WM_MENUSELECT = 0x011F,
            WM_MENUCHAR = 0x0120,
            WM_ENTERIDLE = 0x0121,
            WM_UNINITMENUPOPUP = 0x0125,
            WM_CHANGEUISTATE = 0x0127,
            WM_UPDATEUISTATE = 0x0128,
            WM_QUERYUISTATE = 0x0129,
            WM_CTLCOLORMSGBOX = 0x0132,
            WM_CTLCOLOREDIT = 0x0133,
            WM_CTLCOLORLISTBOX = 0x0134,
            WM_CTLCOLORBTN = 0x0135,
            WM_CTLCOLORDLG = 0x0136,
            WM_CTLCOLORSCROLLBAR = 0x0137,
            WM_CTLCOLORSTATIC = 0x0138,

            WM_MOUSEMOVE = 0x0200,
            WM_MOUSEFIRST = WM_MOUSEMOVE,
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_LBUTTONDBLCLK = 0x0203,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205,
            WM_RBUTTONDBLCLK = 0x0206,
            WM_MBUTTONDOWN = 0x0207,
            WM_MBUTTONUP = 0x0208,
            WM_MBUTTONDBLCLK = 0x0209,
            WM_MOUSEWHEEL = 0x020A,
            WM_XBUTTONDOWN = 0x020B,
            WM_XBUTTONUP = 0x020C,
            WM_XBUTTONDBLCLK = 0x020D,
            WM_MOUSEHWHEEL = 0x020E,
            WM_MOUSELAST = WM_MOUSEHWHEEL,
            WM_PARENTNOTIFY = 0x0210,
            WM_ENTERMENULOOP = 0x0211,
            WM_EXITMENULOOP = 0x0212,
            WM_NEXTMENU = 0x0213,
            WM_SIZING = 0x0214,
            WM_CAPTURECHANGED = 0x0215,
            WM_MOVING = 0x0216,
            WM_POWERBROADCAST = 0x0218,
            WM_DEVICECHANGE = 0x0219,
            WM_POINTERDEVICECHANGE = 0X0238,
            WM_POINTERDEVICEINRANGE = 0x0239,
            WM_POINTERDEVICEOUTOFRANGE = 0x023A,
            WM_POINTERUPDATE = 0x0245,
            WM_POINTERDOWN = 0x0246,
            WM_POINTERUP = 0x0247,
            WM_POINTERENTER = 0x0249,
            WM_POINTERLEAVE = 0x024A,
            WM_POINTERACTIVATE = 0x024B,
            WM_POINTERCAPTURECHANGED = 0x024C,
            WM_IME_SETCONTEXT = 0x0281,
            WM_IME_NOTIFY = 0x0282,
            WM_IME_CONTROL = 0x0283,
            WM_IME_COMPOSITIONFULL = 0x0284,
            WM_IME_SELECT = 0x0285,
            WM_IME_CHAR = 0x0286,
            WM_IME_REQUEST = 0x0288,
            WM_IME_KEYDOWN = 0x0290,
            WM_IME_KEYUP = 0x0291,
            WM_MDICREATE = 0x0220,
            WM_MDIDESTROY = 0x0221,
            WM_MDIACTIVATE = 0x0222,
            WM_MDIRESTORE = 0x0223,
            WM_MDINEXT = 0x0224,
            WM_MDIMAXIMIZE = 0x0225,
            WM_MDITILE = 0x0226,
            WM_MDICASCADE = 0x0227,
            WM_MDIICONARRANGE = 0x0228,
            WM_MDIGETACTIVE = 0x0229,
            WM_MDISETMENU = 0x0230,
            WM_ENTERSIZEMOVE = 0x0231,
            WM_EXITSIZEMOVE = 0x0232,
            WM_DROPFILES = 0x0233,
            WM_MDIREFRESHMENU = 0x0234,
            WM_MOUSEHOVER = 0x02A1,
            WM_NCMOUSELEAVE = 0x02A2,
            WM_MOUSELEAVE = 0x02A3,

            WM_WTSSESSION_CHANGE = 0x02b1,

            WM_TABLET_DEFBASE = 0x02C0,
            WM_DPICHANGED = 0x02E0,
            WM_TABLET_MAXOFFSET = 0x20,

            WM_TABLET_ADDED = WM_TABLET_DEFBASE + 8,
            WM_TABLET_DELETED = WM_TABLET_DEFBASE + 9,
            WM_TABLET_FLICK = WM_TABLET_DEFBASE + 11,
            WM_TABLET_QUERYSYSTEMGESTURESTATUS = WM_TABLET_DEFBASE + 12,

            WM_CUT = 0x0300,
            WM_COPY = 0x0301,
            WM_PASTE = 0x0302,
            WM_CLEAR = 0x0303,
            WM_UNDO = 0x0304,
            WM_RENDERFORMAT = 0x0305,
            WM_RENDERALLFORMATS = 0x0306,
            WM_DESTROYCLIPBOARD = 0x0307,
            WM_DRAWCLIPBOARD = 0x0308,
            WM_PAINTCLIPBOARD = 0x0309,
            WM_VSCROLLCLIPBOARD = 0x030A,
            WM_SIZECLIPBOARD = 0x030B,
            WM_ASKCBFORMATNAME = 0x030C,
            WM_CHANGECBCHAIN = 0x030D,
            WM_HSCROLLCLIPBOARD = 0x030E,
            WM_QUERYNEWPALETTE = 0x030F,
            WM_PALETTEISCHANGING = 0x0310,
            WM_PALETTECHANGED = 0x0311,
            WM_HOTKEY = 0x0312,
            WM_PRINT = 0x0317,
            WM_PRINTCLIENT = 0x0318,
            WM_APPCOMMAND = 0x0319,
            WM_THEMECHANGED = 0x031A,

            WM_DWMCOMPOSITIONCHANGED = 0x031E,
            WM_DWMNCRENDERINGCHANGED = 0x031F,
            WM_DWMCOLORIZATIONCOLORCHANGED = 0x0320,
            WM_DWMWINDOWMAXIMIZEDCHANGE = 0x0321,
            WM_HANDHELDFIRST = 0x0358,
            WM_HANDHELDLAST = 0x035F,
            WM_AFXFIRST = 0x0360,
            WM_AFXLAST = 0x037F,
            WM_PENWINFIRST = 0x0380,
            WM_PENWINLAST = 0x038F,

            #region Windows 7
            WM_DWMSENDICONICTHUMBNAIL = 0x0323,
            WM_DWMSENDICONICLIVEPREVIEWBITMAP = 0x0326,
            #endregion

            WM_USER = 0x0400,

            WM_APP = 0x8000,
        }

        #region WinAPI
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
        public static extern bool PeekMessage([In, Out] ref System.Windows.Interop.MSG msg, HandleRef hwnd, WindowMessage msgMin, WindowMessage msgMax, int remove);
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern bool TranslateMessage([In, Out] ref System.Windows.Interop.MSG msg);
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
        public static extern IntPtr DispatchMessage([In] ref System.Windows.Interop.MSG msg);
        [SecurityCritical]
        [SuppressUnmanagedCodeSecurity]
        [DllImport(ExternDll.User32, CharSet = CharSet.Auto)]
        public static extern unsafe int GetKeyboardState(byte* lpKeyState);
        #endregion

        public IntPtr GameWindowHwnd;
        public void WindowsRun(Dispatcher dispatcher)
        {
            System.Windows.Interop.MSG msg = new System.Windows.Interop.MSG();
            SynchronizationContext oldSyncContext = null;
            SynchronizationContext newSyncContext = null;

            oldSyncContext = SynchronizationContext.Current;
            newSyncContext = new DispatcherSynchronizationContext(dispatcher);
            SynchronizationContext.SetSynchronizationContext(newSyncContext);
            while (true)
            {
                if (PeekMessage(ref msg, new HandleRef(this, IntPtr.Zero), (WindowMessage)0, (WindowMessage)0, 1))
                {
                    if (msg.message == (int)WindowMessage.WM_QUIT)
                        break;
                    bool handled = System.Windows.Interop.ComponentDispatcher.RaiseThreadMessage(ref msg);
                    if (GameWindowHwnd!=IntPtr.Zero && msg.hwnd == GameWindowHwnd)
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                    if (handled == false)
                    {
                        TranslateMessage(ref msg);
                        DispatchMessage(ref msg);
                    }
                }
                else
                {
                    unsafe
                    {
                        fixed (byte* p = &KeyBoardStats[0])
                        {
                            GetKeyboardState(p);
                        }
                    }
                    System.Windows.Interop.ComponentDispatcher.RaiseIdle();
                }
            }

            SynchronizationContext.SetSynchronizationContext(oldSyncContext);
        }

        public bool RunWinForm = true;
        public void WindowFormRun()
        {
            System.Windows.Interop.MSG msg = new System.Windows.Interop.MSG();
            
            while (RunWinForm)
            {
                if (PeekMessage(ref msg, new HandleRef(this, IntPtr.Zero), (WindowMessage)0, (WindowMessage)0, 1))
                {
                    if (msg.message == (int)WindowMessage.WM_QUIT)
                        break;
                    if (GameWindowHwnd != IntPtr.Zero && msg.hwnd == GameWindowHwnd)
                    {
                        System.Windows.Forms.Application.DoEvents();
                    }
                    TranslateMessage(ref msg);
                    DispatchMessage(ref msg);
                }
                else
                {
                    unsafe
                    {
                        fixed (byte* p = &KeyBoardStats[0])
                        {
                            GetKeyboardState(p);
                        }
                    }

                    System.Windows.Forms.Application.RaiseIdle(null);// new EventArgs());
                }
            }
        }

        public bool AttachCLRProfiler()
        {
            Guid guid;
            Guid.TryParse("A347C588-0436-471C-991A-31C70507AA09", out guid);
            var process = Process.GetCurrentProcess();
            return SDK_CLRProfiler_CallAttachProfiler(process.Handle, (UInt32)process.Id, 1000, 
                guid, "E:/Engine/binaries/CLRProfiler.dll");
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(@"CLRProfiler.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public extern static vBOOL SDK_CLRProfiler_CallAttachProfiler(IntPtr hProcess, UInt32 dwProfileeProcID, UInt32 dwMillisecondsTimeout, Guid profilerCLSID, string wszProfilerPath);
        #endregion
    }

    public partial class CShaderResourceView
    {
        private struct PixelDesc
        {
            public int Width;
            public int Height;
            public int Stride;
            public EPixelFormat Format;
            public PixelDesc(int width,int height, int stride, EPixelFormat format)
            {
                Width = width;
                Height = height;
                Stride = stride;
                Format = format;
            }
        }
        public static bool Save2File(string fileName, int width, int height, int stride, byte[] tagBytes, EIMAGE_FILE_FORMAT format)
        {
            unsafe
            {
                fixed (byte* tagData = &tagBytes[0])
                {
                    var bmp = new System.Drawing.Bitmap(width, height, stride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, (IntPtr)tagData);
                    var fmt = System.Drawing.Imaging.ImageFormat.Bmp;
                    switch (format)
                    {
                        case EIMAGE_FILE_FORMAT.BMP:
                            fmt = System.Drawing.Imaging.ImageFormat.Bmp;
                            break;
                        case EIMAGE_FILE_FORMAT.DDS:
                            throw new InvalidOperationException();
                        case EIMAGE_FILE_FORMAT.GIF:
                            fmt = System.Drawing.Imaging.ImageFormat.Gif;
                            break;
                        case EIMAGE_FILE_FORMAT.JPG:
                            fmt = System.Drawing.Imaging.ImageFormat.Jpeg;
                            break;
                        case EIMAGE_FILE_FORMAT.PNG:
                            fmt = System.Drawing.Imaging.ImageFormat.Png;
                            break;
                        case EIMAGE_FILE_FORMAT.TIFF:
                            fmt = System.Drawing.Imaging.ImageFormat.Tiff;
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                    //var bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, desc->Width, desc->Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    //System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, tagBytes, 0, desc->Stride * desc->Height);
                    //bmp.UnlockBits(bmpData);
                    bmp.Save(fileName, fmt);
                }
            }
            return true;
        }
        public bool Save2File(string fileName, Support.CBlobObject data, EIMAGE_FILE_FORMAT format)
        {
            unsafe
            {
                var dataPtr = (byte*)data.Data.ToPointer();
                PixelDesc* desc = (PixelDesc*)dataPtr;

                var descSize = sizeof(PixelDesc);

                var srcBytes = data.ToBytes();
                var tagBytes = new byte[srcBytes.Length - descSize];
                for (int i = 0; i < tagBytes.Length; i += 4)
                {
                    // bgra         rgba
                    tagBytes[i] = srcBytes[i + 2 + descSize];
                    tagBytes[i + 1] = srcBytes[i + 1 + descSize];
                    tagBytes[i + 2] = srcBytes[i + descSize];
                    tagBytes[i + 3] = srcBytes[i + 3 + descSize];
                }

                return Save2File(fileName, desc->Width, desc->Height, desc->Stride, tagBytes, format);
            }
        }

        public static void SaveSnap(string fileName, Support.CBlobObject[] dataArray)
        {
            var xnd = EngineNS.IO.XndHolder.NewXNDHolder();
            EngineNS.IO.XndNode node = xnd.Node;
            for (int k = 0; k < dataArray.Length; k++)
            {
                var data = dataArray[k];
                var attr = node.AddAttrib($"Frame{k}");
                
                unsafe
                {
                    var dataPtr = (byte*)data.Data.ToPointer();
                    PixelDesc* desc = (PixelDesc*)dataPtr;

                    var descSize = sizeof(PixelDesc);

                    var srcBytes = data.ToBytes();
                    var tagBytes = new byte[srcBytes.Length - descSize];
                    for (int i = 0; i < tagBytes.Length; i += 4)
                    {
                        // bgra         rgba
                        tagBytes[i] = srcBytes[i + 2 + descSize];
                        tagBytes[i + 1] = srcBytes[i + 1 + descSize];
                        tagBytes[i + 2] = srcBytes[i + descSize];
                        tagBytes[i + 3] = srcBytes[i + 3 + descSize];
                    }

                    fixed (byte* tagData = &tagBytes[0])
                    {
                        var bitmap = new System.Drawing.Bitmap(desc->Width, desc->Height, desc->Stride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, (IntPtr)tagData);
                        var memStream = new System.IO.MemoryStream();
                        bitmap.Save(memStream, System.Drawing.Imaging.ImageFormat.Png);

                        attr.BeginWrite();
                        var memArray = memStream.ToArray();
                        attr.Write(memArray, memArray.Length);
                        attr.EndWrite();
                    }
                }
            }
            var tmp = fileName + ".tmp";
            EngineNS.IO.XndHolder.SaveXND(tmp, xnd);
            CEngine.Instance.FileManager.MoveFile(tmp, fileName);
        }

        public static System.IO.MemoryStream[] LoadSnap(string fileName)
        {
            using (var xnd = EngineNS.IO.XndHolder.SyncLoadXND(fileName))
            {
                if (xnd == null)
                    return null;
                var attrs = xnd.Node.GetAttribs();
                if (attrs.Count == 0)
                    return null;
                var result = new System.IO.MemoryStream[attrs.Count];
                for (int k = 0; k < attrs.Count; k++)
                {
                    unsafe
                    {
                        attrs[k].BeginRead();
                        byte[] srcBytes;
                        attrs[k].Read(out srcBytes, (int)attrs[k].Length);
                        attrs[k].EndRead();

                        result[k] = new System.IO.MemoryStream(srcBytes);
                    }
                }

                return result;
            }   
        }

        public bool Save2File4x4(string fileName, Support.CBlobObject[] dataArray, EIMAGE_FILE_FORMAT format)
        {
            unsafe
            {
                var data = dataArray[0];
                var dataPtr = (byte*)data.Data.ToPointer();
                PixelDesc* desc = (PixelDesc*)dataPtr;

                var descSize = sizeof(PixelDesc);

                int imageW = desc->Width * 4;
                int imageH = desc->Height * 4;
                int imageStride = imageW * 4;
                var tagBytes = new byte[imageW * imageH * 4 ];
                for (int k=0; k<16; k++)
                {
                    data = dataArray[k];
                    dataPtr = (byte*)data.Data.ToPointer();

                    int x = k % 4;
                    int y = k / 4;

                    var srcBytes = data.ToBytes();
                    int startX = x * desc->Stride;
                    for (int i = 0; i < desc->Height; i++)
                    {
                        int startY = (y * desc->Height + i) * imageStride;
                        for (int j = 0; j < desc->Width; j++)
                        {
                            tagBytes[startY + startX + j * 4 + 0] = srcBytes[descSize + (i * desc->Width + j) * 4 + 2];
                            tagBytes[startY + startX + j * 4 + 1] = srcBytes[descSize + (i * desc->Width + j) * 4 + 1];
                            tagBytes[startY + startX + j * 4 + 2] = srcBytes[descSize + (i * desc->Width + j) * 4 + 0];
                            tagBytes[startY + startX + j * 4 + 3] = srcBytes[descSize + (i * desc->Width + j) * 4 + 3];
                        }
                        //// bgra         rgba
                        //tagBytes[i] = srcBytes[i + 2 + descSize];
                        //tagBytes[i + 1] = srcBytes[i + 1 + descSize];
                        //tagBytes[i + 2] = srcBytes[i + descSize];
                        //tagBytes[i + 3] = srcBytes[i + 3 + descSize];
                    }
                }
                return Save2File(fileName, desc->Width*4, desc->Height*4, desc->Stride * 4, tagBytes, format);
            }
        }

        public bool Save2File(CRenderContext rc, string fileName, EIMAGE_FILE_FORMAT Type)
        {
            unsafe
            {
                Support.CBlobObject data = new Support.CBlobObject();
                Save2Memory(rc, data, EIMAGE_FILE_FORMAT.BMP);

                return Save2File(fileName, data, Type);
            }
            //return (bool)SDK_IShaderResourceView_Save2File(CoreObject, rc.CoreObject, fileName, Type);
        }
    }
}

namespace EngineNS.IO
{
    public partial class FileManager
    {
        public static string UseCooked = null;
        partial void InitPaths()
        {
            mBin = AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/");
            mBin = mBin.TrimEnd('/');
            mBin = mBin.ToLower();
            var mRoot = mBin.Substring(0, mBin.LastIndexOf('/') + 1);
            mBin += "/";

            if (UseCooked != null)
            {
                mProjectRoot = mRoot + "cooked/" + UseCooked + "/";
                mEngineRoot = mRoot + "cooked/" + UseCooked + "/";
                mCooked = mRoot + "cooked/";
            }
            else
            {
                if (CIPlatform.Instance.PlayMode != CIPlatform.enPlayMode.Game)
                {//如果是编辑器状态这里其实要根据启动参数获得游戏内容目录mContent
                 //以后应该从命令行获取
                    mProjectRoot = mRoot;
                    mEngineRoot = mRoot;
                }
                else
                {
                    mProjectRoot = mRoot;
                    mEngineRoot = mRoot;
                }

                mCooked = mProjectRoot + "cooked/";
            }

            //临时代码，以后要从配置读取
            mProjectSourceRoot = mProjectRoot + "execute/games/batman/";

            mEngineContent = mEngineRoot + "enginecontent/";
            mEditorContent = mEngineRoot + "editcontent/";
            mProjectContent = mProjectRoot + "content/";

            mDDCDirectory = mProjectRoot + "deriveddatacache/";

            if (UseCooked == null)
            {
                var smStr = CRenderContext.ShaderModelString;

                CreateDirectory(mDDCDirectory);
                CreateDirectory(mDDCDirectory + "shaderinfo/");
                CreateDirectory(mDDCDirectory + "shaderinfo/cs/");
                CreateDirectory(mDDCDirectory + "shaderinfo/vs/");
                CreateDirectory(mDDCDirectory + "shaderinfo/ps/");
                CreateDirectory(mDDCDirectory + smStr + "/cs/");
                CreateDirectory(mDDCDirectory + smStr + "/vs/");
                CreateDirectory(mDDCDirectory + smStr + "/ps/");

                if (CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Editor)
                {
                    CreateDirectory(mCooked);
                    foreach (var i in CookedPlatforms)
                    {
                        CreateDirectory(mCooked + i);
                    }
                }
            }
        }
        private string[] mCookedPlatforms = new string[]
        {
            "windows",
            "android",
        };
        public string[] CookedPlatforms
        {
            get { return mCookedPlatforms; }
        }
        //public string[] GetDirectories(string path, string searchPattern = "*", SearchOption searchOp = SearchOption.TopDirectoryOnly)
        //{
        //    return System.IO.Directory.GetDirectories(path, searchPattern, searchOp);
        //}
        //public string[] GetFiles(string absDir, string fileKeyName, SearchOption searOp)
        //{
        //    return System.IO.Directory.GetFiles(absDir, fileKeyName, searOp);
        //}
    }
}
