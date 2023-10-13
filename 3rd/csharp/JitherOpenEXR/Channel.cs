using Jither.OpenEXR.Attributes;
using Jither.OpenEXR.Drawing;

namespace Jither.OpenEXR;

public class Channel
{
    public static Channel R_Half { get; } = new Channel("R", EXRDataType.Half, PerceptualTreatment.Logarithmic);
    public static Channel G_Half { get; } = new Channel("G", EXRDataType.Half, PerceptualTreatment.Logarithmic);
    public static Channel B_Half { get; } = new Channel("B", EXRDataType.Half, PerceptualTreatment.Logarithmic);
    public static Channel A_Half { get; } = new Channel("A", EXRDataType.Half, PerceptualTreatment.Logarithmic);
    public static Channel R_Float { get; } = new Channel("R", EXRDataType.Float, PerceptualTreatment.Logarithmic);
    public static Channel G_Float { get; } = new Channel("G", EXRDataType.Float, PerceptualTreatment.Logarithmic);
    public static Channel B_Float { get; } = new Channel("B", EXRDataType.Float, PerceptualTreatment.Logarithmic);
    public static Channel A_Float { get; } = new Channel("A", EXRDataType.Float, PerceptualTreatment.Logarithmic);

    public static Channel R_HalfLinear { get; } = new Channel("R", EXRDataType.Half, PerceptualTreatment.Linear);
    public static Channel G_HalfLinear { get; } = new Channel("G", EXRDataType.Half, PerceptualTreatment.Linear);
    public static Channel B_HalfLinear { get; } = new Channel("B", EXRDataType.Half, PerceptualTreatment.Linear);
    public static Channel A_HalfLinear { get; } = new Channel("A", EXRDataType.Half, PerceptualTreatment.Linear);
    public static Channel R_FloatLinear { get; } = new Channel("R", EXRDataType.Float, PerceptualTreatment.Linear);
    public static Channel G_FloatLinear { get; } = new Channel("G", EXRDataType.Float, PerceptualTreatment.Linear);
    public static Channel B_FloatLinear { get; } = new Channel("B", EXRDataType.Float, PerceptualTreatment.Linear);
    public static Channel A_FloatLinear { get; } = new Channel("A", EXRDataType.Float, PerceptualTreatment.Linear);

    /// <summary>
    /// The channel’s name is a text string, for example R, Z or yVelocity. The name tells programs that read the image file how to interpret the data in the channel.
    /// </summary>
    /// <remarks>
    /// For a few channel names, interpretation of the data is predefined:
    /// 
    /// * R = red intensity
    /// * G = green intensity
    /// * B = blue intensity
    /// * A = alpha/opacity: 0.0 means the pixel is transparent; 1.0 means the pixel is opaque.
    /// 
    /// By convention, all color channels are premultiplied by alpha, so that foreground + (1-alpha) xbackground performs a correct “over” operation. 
    /// </remarks>
    public string Name { get; }

    /// <summary>
    /// The channel's data type
    /// </summary>
    public EXRDataType Type { get; }

    /// <summary>
    /// OpenEXR source docs: "Hint for lossy compression methods about how to treat values (logarithmic or linear), meaning a human sees values like R, G, B, luminance difference
    /// between 0.1 and 0.2 as about the same as 1.0 to 2.0 (logarithmic), where chroma coordinates are closer to linear
    /// (0.1 and 0.2 is about the same difference as 1.0 and 1.1)."
    /// </summary>
    public PerceptualTreatment PerceptualTreatment { get; }

    /// <summary>
    /// The channel's x sampling rate. Determines for which of the pixels in the image’s data window data are stored in the file. Data for a pixel at pixel space coordinates (x, y)
    /// are only stored if <c>x mod XSampling = 0</c> (and <c>y mod YSampling = 0</c>).
    /// </summary>
    /// <remarks>
    /// For RGBA images, <c>XSampling</c> is 1 for all channels - each channel contains data for every pixel.
    /// Note that OpenEXR does not support subsampling of any channel for tiled images.
    /// </remarks>
    public int XSampling { get; }

    /// <summary>
    /// The channel's y sampling rate. Determines for which of the pixels in the image’s data window data are stored in the file. Data for a pixel at pixel space coordinates (x, y)
    /// are only stored if <c>y mod YSampling = 0</c> (and <c>x mod XSampling = 0</c>).
    /// </summary>
    /// <remarks>
    /// For RGBA images, <c>YSampling</c> is 1 for all channels - each channel contains data for every pixel.
    /// Note that OpenEXR does not support subsampling of any channel for tiled images.
    /// </remarks>
    public int YSampling { get; }

    public byte Reserved0 { get; }
    public byte Reserved1 { get; }
    public byte Reserved2 { get; }

    public bool IsSubsampled => XSampling != 1 || YSampling != 1;

    internal int BytesPerPixelNoSubSampling => Type.GetBytesPerPixel();

    public Channel(string name, EXRDataType type, PerceptualTreatment perceptualTreatment, int xSampling = 1, int ySampling = 1)
    {
        Name = name;
        Type = type;
        PerceptualTreatment = perceptualTreatment;
        Reserved0 = 0;
        Reserved1 = 0;
        Reserved2 = 0;
        XSampling = xSampling;
        YSampling = ySampling;
    }

    internal Channel(string name, EXRDataType type, PerceptualTreatment perceptualTreatment, byte reserved0, byte reserved1, byte reserved2, int xSampling, int ySampling)
    {
        Name = name;
        Type = type;
        PerceptualTreatment = perceptualTreatment;
        Reserved0 = reserved0;
        Reserved1 = reserved1;
        Reserved2 = reserved2;
        XSampling = xSampling;
        YSampling = ySampling;
    }

    /// <summary>
    /// Returns the number of pixels sampled in the channel for a given area (e.g. a block or tile)
    /// </summary>
    /// <returns></returns>
    public Dimensions<int> GetSubsampledResolution(Bounds<int> area)
    {
        // TODO: This isn't actually accurate. Consider an area from y = 2 to y = 4 with YSampling = 5, which would only sample y = 0 and y = 5
        return new Dimensions<int>(area.Width / XSampling, area.Height / YSampling);
    }

    /// <summary>
    /// Returns the number of bytes occupied by the channel for a given area, taking into account sub-sampling and value type (16-bit or 32-bit).
    /// </summary>
    public int GetByteCount(Bounds<int> area)
    {
        return GetSubsampledResolution(area).Area * Type.GetBytesPerPixel();
    }

    /// <summary>
    /// Returns the number of bytes occupied by the channel for a given area, taking into account sub-sampling and value type (16-bit or 32-bit). Used for *very* large images (exceeding the int 2GB limit).
    /// </summary>
    public ulong GetByteCountLarge(Bounds<int> area)
    {
        var dims = GetSubsampledResolution(area);
        // OpenEXR defines width and height as being contained in a signed integer, so results above 2GB don't enter until this point, with pixel area:
        ulong pixels = (uint)dims.Width * (uint)dims.Height;
        return pixels * (ulong)Type.GetBytesPerPixel();
    }

    public override string ToString()
    {
        return $"{Name}:{Type} ({PerceptualTreatment}, Sx = {XSampling}, Sy = {YSampling})";
    }
}
