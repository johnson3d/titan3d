using System;
using System.Runtime.InteropServices;

namespace EngineNS
{
    [Flags]
    public enum ImPlotFlags_
    {
        ImPlotFlags_None = 0,
        ImPlotFlags_NoTitle = 1 << 0,
        ImPlotFlags_NoLegend = 1 << 1,
        ImPlotFlags_NoMouseText = 1 << 2,
        ImPlotFlags_NoInputs = 1 << 3,
        ImPlotFlags_NoMenus = 1 << 4,
        ImPlotFlags_NoBoxSelect = 1 << 5,
        ImPlotFlags_NoChild = 1 << 6,
        ImPlotFlags_NoFrame = 1 << 7,
        ImPlotFlags_Equal = 1 << 8,
        ImPlotFlags_Crosshairs = 1 << 9,
        ImPlotFlags_CanvasOnly = ImPlotFlags_NoTitle | ImPlotFlags_NoLegend | ImPlotFlags_NoMenus | ImPlotFlags_NoBoxSelect | ImPlotFlags_NoMouseText,
    }

    [Flags]
    public enum ImPlotAxisFlags_
    {
        ImPlotAxisFlags_None = 0,
        ImPlotAxisFlags_NoLabel = 1 << 0,
        ImPlotAxisFlags_NoGridLines = 1 << 1,
        ImPlotAxisFlags_NoTickMarks = 1 << 2,
        ImPlotAxisFlags_NoTickLabels = 1 << 3,
        ImPlotAxisFlags_NoInitialFit = 1 << 4,
        ImPlotAxisFlags_NoMenus = 1 << 5,
        ImPlotAxisFlags_NoSideSwitch = 1 << 6,
        ImPlotAxisFlags_NoHighlight = 1 << 7,
        ImPlotAxisFlags_Opposite = 1 << 8,
        ImPlotAxisFlags_Foreground = 1 << 9,
        ImPlotAxisFlags_Invert = 1 << 10,
        ImPlotAxisFlags_AutoFit = 1 << 11,
        ImPlotAxisFlags_RangeFit = 1 << 12,
        ImPlotAxisFlags_PanStretch = 1 << 13,
        ImPlotAxisFlags_LockMin = 1 << 14,
        ImPlotAxisFlags_LockMax = 1 << 15,
        ImPlotAxisFlags_Lock = ImPlotAxisFlags_LockMin | ImPlotAxisFlags_LockMax,
        ImPlotAxisFlags_NoDecorations = ImPlotAxisFlags_NoLabel | ImPlotAxisFlags_NoGridLines | ImPlotAxisFlags_NoTickMarks | ImPlotAxisFlags_NoTickLabels,
        ImPlotAxisFlags_AuxDefault = ImPlotAxisFlags_NoGridLines | ImPlotAxisFlags_Opposite,
    }

    [Flags]
    public enum ImPlotLineFlags_
    {
        ImPlotLineFlags_None = 0,
        ImPlotLineFlags_Segments = 1 << 10,
        ImPlotLineFlags_Loop = 1 << 11,
        ImPlotLineFlags_SkipNaN = 1 << 12,
        ImPlotLineFlags_NoClip = 1 << 13,
        ImPlotLineFlags_Shaded = 1 << 14,
    }

    [Flags]
    public enum ImPlotBarsFlags_
    {
        ImPlotBarsFlags_None = 0,
        ImPlotBarsFlags_Horizontal = 1 << 10,
    }

    public enum ImPlotCond_
    {
        ImPlotCond_None = 0,
        ImPlotCond_Always = 1,
        ImPlotCond_Once = 2,
    }

    public unsafe static class ImPlotAPI
    {
        private const string ModuleNC = EngineNS.CoreSDK.CoreModule;
        private static bool mNativeEntryPointsAvailable = true;

        public static bool IsAvailable
        {
            get
            {
                if (!mNativeEntryPointsAvailable)
                    return false;
                try
                {
                    return TitanImPlot_EnsureContext() != 0;
                }
                catch (DllNotFoundException)
                {
                    mNativeEntryPointsAvailable = false;
                    return false;
                }
                catch (EntryPointNotFoundException)
                {
                    mNativeEntryPointsAvailable = false;
                    return false;
                }
            }
        }

        public static bool BeginPlot(string title, in Vector2 size, ImPlotFlags_ flags = ImPlotFlags_.ImPlotFlags_None)
        {
            if (!IsAvailable)
                return false;
            fixed (Vector2* pinnedSize = &size)
            {
                return TitanImPlot_BeginPlot(title, pinnedSize, (int)flags) != 0;
            }
        }

        public static void EndPlot()
        {
            if (mNativeEntryPointsAvailable)
                TitanImPlot_EndPlot();
        }

        public static void SetupAxes(string xLabel, string yLabel,
            ImPlotAxisFlags_ xFlags = ImPlotAxisFlags_.ImPlotAxisFlags_None,
            ImPlotAxisFlags_ yFlags = ImPlotAxisFlags_.ImPlotAxisFlags_None)
        {
            if (mNativeEntryPointsAvailable)
                TitanImPlot_SetupAxes(xLabel, yLabel, (int)xFlags, (int)yFlags);
        }

        public static void SetupAxesLimits(double xMin, double xMax, double yMin, double yMax, ImPlotCond_ cond = ImPlotCond_.ImPlotCond_Once)
        {
            if (mNativeEntryPointsAvailable)
                TitanImPlot_SetupAxesLimits(xMin, xMax, yMin, yMax, (int)cond);
        }

        public static void SetNextAxesToFit()
        {
            if (mNativeEntryPointsAvailable)
                TitanImPlot_SetNextAxesToFit();
        }

        public static void PlotLine(string label, float[] xs, float[] ys, int count, ImPlotLineFlags_ flags = ImPlotLineFlags_.ImPlotLineFlags_None)
        {
            if (!mNativeEntryPointsAvailable || xs == null || ys == null)
                return;
            count = Math.Min(count, Math.Min(xs.Length, ys.Length));
            if (count <= 0)
                return;
            fixed (float* pinnedXs = xs)
            fixed (float* pinnedYs = ys)
            {
                TitanImPlot_PlotLineFloat(label, pinnedXs, pinnedYs, count, (int)flags);
            }
        }

        public static void PlotBars(string label, float[] xs, float[] ys, int count, double barSize = 0.67, ImPlotBarsFlags_ flags = ImPlotBarsFlags_.ImPlotBarsFlags_None)
        {
            if (!mNativeEntryPointsAvailable || xs == null || ys == null)
                return;
            count = Math.Min(count, Math.Min(xs.Length, ys.Length));
            if (count <= 0)
                return;
            fixed (float* pinnedXs = xs)
            fixed (float* pinnedYs = ys)
            {
                TitanImPlot_PlotBarsFloat(label, pinnedXs, pinnedYs, count, barSize, (int)flags);
            }
        }

        [DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        private static extern int TitanImPlot_EnsureContext();

        [DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        private static extern void TitanImPlot_DestroyContext();

        [DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        private static extern int TitanImPlot_BeginPlot([MarshalAs(UnmanagedType.LPUTF8Str)] string title, Vector2* size, int flags);

        [DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        private static extern void TitanImPlot_EndPlot();

        [DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        private static extern void TitanImPlot_SetupAxes([MarshalAs(UnmanagedType.LPUTF8Str)] string xLabel, [MarshalAs(UnmanagedType.LPUTF8Str)] string yLabel, int xFlags, int yFlags);

        [DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        private static extern void TitanImPlot_SetupAxesLimits(double xMin, double xMax, double yMin, double yMax, int cond);

        [DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        private static extern void TitanImPlot_SetNextAxesToFit();

        [DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        private static extern void TitanImPlot_PlotLineFloat([MarshalAs(UnmanagedType.LPUTF8Str)] string label, float* xs, float* ys, int count, int flags);

        [DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        private static extern void TitanImPlot_PlotBarsFloat([MarshalAs(UnmanagedType.LPUTF8Str)] string label, float* xs, float* ys, int count, double barSize, int flags);
    }
}
