using System;
using System.ComponentModel;

namespace EngineNS.Graphics.Pipeline.Common.ColorGrading
{
    /// <summary>
    /// Color Grading parameters, modeled after UE's PostProcessSettings color grading section.
    /// All color correction fields use (R, G, B, Intensity) layout — .xyz per-channel, .w overall multiplier.
    /// Default values produce an identity (no-op) color grade.
    /// </summary>
    [Rtti.Meta("")]
    public class TtColorGradingSettings : IO.BaseSerializer
    {
        // ── White Balance ──
        [Category("WhiteBalance")]
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(100f)]
        public float WhiteTemp { get; set; } = 6500.0f;

        [Category("WhiteBalance")]
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.1f)]
        public float WhiteTint { get; set; } = 0.0f;

        // ── Global ──
        [Category("Global")]
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtColorGradingWheelEditor]
        public Vector4 ColorSaturation { get; set; } = new Vector4(1, 1, 1, 1);

        [Category("Global")]
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtColorGradingWheelEditor]
        public Vector4 ColorContrast { get; set; } = new Vector4(1, 1, 1, 1);

        [Category("Global")]
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtColorGradingWheelEditor]
        public Vector4 ColorGamma { get; set; } = new Vector4(1, 1, 1, 1);

        [Category("Global")]
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtColorGradingWheelEditor]
        public Vector4 ColorGain { get; set; } = new Vector4(1, 1, 1, 1);

        [Category("Global")]
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtColorGradingWheelEditor]
        public Vector4 ColorOffset { get; set; } = new Vector4(0, 0, 0, 0);

        // ── Shadows ──
        [Category("Shadows")]
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtColorGradingWheelEditor]
        public Vector4 ColorSaturationShadows { get; set; } = new Vector4(1, 1, 1, 1);

        [Category("Shadows")]
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtColorGradingWheelEditor]
        public Vector4 ColorContrastShadows { get; set; } = new Vector4(1, 1, 1, 1);

        [Category("Shadows")]
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtColorGradingWheelEditor]
        public Vector4 ColorGammaShadows { get; set; } = new Vector4(1, 1, 1, 1);

        [Category("Shadows")]
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtColorGradingWheelEditor]
        public Vector4 ColorGainShadows { get; set; } = new Vector4(1, 1, 1, 1);

        [Category("Shadows")]
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtColorGradingWheelEditor]
        public Vector4 ColorOffsetShadows { get; set; } = new Vector4(0, 0, 0, 0);

        // ── Midtones ──
        [Category("Midtones")]
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtColorGradingWheelEditor]
        public Vector4 ColorSaturationMidtones { get; set; } = new Vector4(1, 1, 1, 1);

        [Category("Midtones")]
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtColorGradingWheelEditor]
        public Vector4 ColorContrastMidtones { get; set; } = new Vector4(1, 1, 1, 1);

        [Category("Midtones")]
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtColorGradingWheelEditor]
        public Vector4 ColorGammaMidtones { get; set; } = new Vector4(1, 1, 1, 1);

        [Category("Midtones")]
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtColorGradingWheelEditor]
        public Vector4 ColorGainMidtones { get; set; } = new Vector4(1, 1, 1, 1);

        [Category("Midtones")]
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtColorGradingWheelEditor]
        public Vector4 ColorOffsetMidtones { get; set; } = new Vector4(0, 0, 0, 0);

        // ── Highlights ──
        [Category("Highlights")]
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtColorGradingWheelEditor]
        public Vector4 ColorSaturationHighlights { get; set; } = new Vector4(1, 1, 1, 1);

        [Category("Highlights")]
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtColorGradingWheelEditor]
        public Vector4 ColorContrastHighlights { get; set; } = new Vector4(1, 1, 1, 1);

        [Category("Highlights")]
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtColorGradingWheelEditor]
        public Vector4 ColorGammaHighlights { get; set; } = new Vector4(1, 1, 1, 1);

        [Category("Highlights")]
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtColorGradingWheelEditor]
        public Vector4 ColorGainHighlights { get; set; } = new Vector4(1, 1, 1, 1);

        [Category("Highlights")]
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtColorGradingWheelEditor]
        public Vector4 ColorOffsetHighlights { get; set; } = new Vector4(0, 0, 0, 0);

        // ── Shadow/Midtone/Highlight boundaries ──
        [Category("Misc")]
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.01f)]
        public float ShadowsMax { get; set; } = 0.09f;

        [Category("Misc")]
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.01f)]
        public float HighlightsMin { get; set; } = 0.5f;

        [Category("Misc")]
        [Rtti.Meta("")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.01f)]
        public float HighlightsMax { get; set; } = 1.0f;

        // ── LUT configuration ──
        [Category("Misc")]
        [Rtti.Meta("")]
        public int LutSize { get; set; } = 32;

        /// <summary>
        /// Returns a snapshot hash of all parameters for dirty-checking.
        /// When the hash changes the LUT needs to be regenerated.
        /// </summary>
        public int GetSettingsHash()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + WhiteTemp.GetHashCode();
                hash = hash * 31 + WhiteTint.GetHashCode();
                hash = hash * 31 + ColorSaturation.GetHashCode();
                hash = hash * 31 + ColorContrast.GetHashCode();
                hash = hash * 31 + ColorGamma.GetHashCode();
                hash = hash * 31 + ColorGain.GetHashCode();
                hash = hash * 31 + ColorOffset.GetHashCode();
                hash = hash * 31 + ColorSaturationShadows.GetHashCode();
                hash = hash * 31 + ColorContrastShadows.GetHashCode();
                hash = hash * 31 + ColorGammaShadows.GetHashCode();
                hash = hash * 31 + ColorGainShadows.GetHashCode();
                hash = hash * 31 + ColorOffsetShadows.GetHashCode();
                hash = hash * 31 + ColorSaturationMidtones.GetHashCode();
                hash = hash * 31 + ColorContrastMidtones.GetHashCode();
                hash = hash * 31 + ColorGammaMidtones.GetHashCode();
                hash = hash * 31 + ColorGainMidtones.GetHashCode();
                hash = hash * 31 + ColorOffsetMidtones.GetHashCode();
                hash = hash * 31 + ColorSaturationHighlights.GetHashCode();
                hash = hash * 31 + ColorContrastHighlights.GetHashCode();
                hash = hash * 31 + ColorGammaHighlights.GetHashCode();
                hash = hash * 31 + ColorGainHighlights.GetHashCode();
                hash = hash * 31 + ColorOffsetHighlights.GetHashCode();
                hash = hash * 31 + ShadowsMax.GetHashCode();
                hash = hash * 31 + HighlightsMin.GetHashCode();
                hash = hash * 31 + HighlightsMax.GetHashCode();
                hash = hash * 31 + LutSize.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Linearly interpolate between two settings. Used for blending between PostProcessVolumes.
        /// </summary>
        static Vector4 LerpV4(Vector4 start, Vector4 end, float t)
        {
            return Vector4.Lerp(in start, in end, t);
        }

        public static TtColorGradingSettings Lerp(TtColorGradingSettings a, TtColorGradingSettings b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            var result = new TtColorGradingSettings();

            result.WhiteTemp = MathHelper.Lerp(a.WhiteTemp, b.WhiteTemp, t);
            result.WhiteTint = MathHelper.Lerp(a.WhiteTint, b.WhiteTint, t);

            result.ColorSaturation = LerpV4(a.ColorSaturation, b.ColorSaturation, t);
            result.ColorContrast = LerpV4(a.ColorContrast, b.ColorContrast, t);
            result.ColorGamma = LerpV4(a.ColorGamma, b.ColorGamma, t);
            result.ColorGain = LerpV4(a.ColorGain, b.ColorGain, t);
            result.ColorOffset = LerpV4(a.ColorOffset, b.ColorOffset, t);

            result.ColorSaturationShadows = LerpV4(a.ColorSaturationShadows, b.ColorSaturationShadows, t);
            result.ColorContrastShadows = LerpV4(a.ColorContrastShadows, b.ColorContrastShadows, t);
            result.ColorGammaShadows = LerpV4(a.ColorGammaShadows, b.ColorGammaShadows, t);
            result.ColorGainShadows = LerpV4(a.ColorGainShadows, b.ColorGainShadows, t);
            result.ColorOffsetShadows = LerpV4(a.ColorOffsetShadows, b.ColorOffsetShadows, t);

            result.ColorSaturationMidtones = LerpV4(a.ColorSaturationMidtones, b.ColorSaturationMidtones, t);
            result.ColorContrastMidtones = LerpV4(a.ColorContrastMidtones, b.ColorContrastMidtones, t);
            result.ColorGammaMidtones = LerpV4(a.ColorGammaMidtones, b.ColorGammaMidtones, t);
            result.ColorGainMidtones = LerpV4(a.ColorGainMidtones, b.ColorGainMidtones, t);
            result.ColorOffsetMidtones = LerpV4(a.ColorOffsetMidtones, b.ColorOffsetMidtones, t);

            result.ColorSaturationHighlights = LerpV4(a.ColorSaturationHighlights, b.ColorSaturationHighlights, t);
            result.ColorContrastHighlights = LerpV4(a.ColorContrastHighlights, b.ColorContrastHighlights, t);
            result.ColorGammaHighlights = LerpV4(a.ColorGammaHighlights, b.ColorGammaHighlights, t);
            result.ColorGainHighlights = LerpV4(a.ColorGainHighlights, b.ColorGainHighlights, t);
            result.ColorOffsetHighlights = LerpV4(a.ColorOffsetHighlights, b.ColorOffsetHighlights, t);

            result.ShadowsMax = MathHelper.Lerp(a.ShadowsMax, b.ShadowsMax, t);
            result.HighlightsMin = MathHelper.Lerp(a.HighlightsMin, b.HighlightsMin, t);
            result.HighlightsMax = MathHelper.Lerp(a.HighlightsMax, b.HighlightsMax, t);

            // LutSize: use the larger of the two (not interpolated — must be integer)
            result.LutSize = Math.Max(a.LutSize, b.LutSize);

            return result;
        }

        /// <summary>
        /// Returns the identity (no-op) settings — equivalent to default constructor.
        /// </summary>
        public static TtColorGradingSettings Identity => new TtColorGradingSettings();
    }
}
