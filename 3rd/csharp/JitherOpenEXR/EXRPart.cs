using Jither.OpenEXR.Attributes;
using Jither.OpenEXR.Drawing;
using Jither.OpenEXR.Helpers;
using System.Diagnostics.CodeAnalysis;

namespace Jither.OpenEXR;

public class EXRPart
{
    private readonly EXRHeader header;
    private readonly bool isSinglePartTiled;
    private readonly EXRReadOptions? readOptions;
    private EXRFile? file;
    private EXRPartDataReader? dataReader;
    private EXRPartDataWriter? dataWriter;
    private TilingInformation? tilingInformation;

    /// <summary>
    /// Attributes required by spec and which will be required even in lax mode.
    /// </summary>
    public static readonly string[] RequiredAttributes = new[] {
        AttributeNames.Channels,
        AttributeNames.Compression,
        AttributeNames.DataWindow
    };

    /// <summary>
    /// These attributes are strictly required by the spec, but sometimes left out.
    /// </summary>
    public static readonly string[] StrictlyRequiredAttributes = new[]
    {
        AttributeNames.DisplayWindow,
        AttributeNames.LineOrder,
        AttributeNames.PixelAspectRatio,
        AttributeNames.ScreenWindowCenter,
        AttributeNames.ScreenWindowWidth
    };

    public static readonly string[] RequiredMultiPartAttributes = new[] {
        AttributeNames.Name,
        AttributeNames.Type,
        AttributeNames.ChunkCount,
    };

    private static readonly Dictionary<string, EXRAttribute> defaultAttributes = new()
    {
        [AttributeNames.LineOrder] = new EXRAttribute<LineOrder>(AttributeNames.LineOrder, LineOrder.IncreasingY),
        [AttributeNames.PixelAspectRatio] = new EXRAttribute<float>(AttributeNames.PixelAspectRatio, 1),
        [AttributeNames.ScreenWindowCenter] = new EXRAttribute<V2f>(AttributeNames.ScreenWindowCenter, new V2f(0, 0)),
        [AttributeNames.ScreenWindowWidth] = new EXRAttribute<float>(AttributeNames.ScreenWindowWidth, 1)
    };

    public IReadOnlyList<EXRAttribute> Attributes => header.Attributes;

    /// <summary>
    /// The name attribute defines the name of each part. The name of each part must be unique. Names may contain ‘.’ characters to present a tree-like structure of the parts in a file.
    /// Required if the file is either MultiPart or NonImage.
    /// </summary>
    public string? Name
    {
        get => header.GetAttributeOrDefault<string>(AttributeNames.Name);
        set
        {
            if (value == null)
            {
                header.RemoveAttribute(AttributeNames.Name);
            }
            else
            {
                header.SetAttribute(new EXRAttribute<string>(AttributeNames.Name, value));
            }
        }
    }

    /// <summary>
    /// Data types are defined by the type attribute. There are four types:
    /// 
    /// 1. Scan line images: indicated by a type attribute of scanlineimage.
    /// 2. Tiled images: indicated by a type attribute of tiledimage.
    /// 3. Deep scan line images: indicated by a type attribute of deepscanline.
    /// 4. Deep tiled images: indicated by a type attribute of deeptile.
    /// 
    /// Required if the file is either MultiPart or NonImage.
    /// </summary>
    /// <remarks>
    /// This value must agree with the version field’s tile bit (9) and non-image (deep data) bit (11) settings.
    /// </remarks>
    public PartType Type
    {
        get => header.GetAttributeOrDefault<string>(AttributeNames.Type) switch
        {
            "scanlineimage" => PartType.ScanLineImage,
            "tiledimage" => PartType.TiledImage,
            "deepscanline" => PartType.DeepScanLine,
            "deeptile" => PartType.DeepTiled,
            _ => PartType.Unknown
        };
        set
        {
            var type = value switch
            {
                PartType.ScanLineImage => "scanlineimage",
                PartType.TiledImage => "tiledimage",
                PartType.DeepScanLine => "deepscanline",
                PartType.DeepTiled => "deeptile",
                _ => null,
            };
            if (type == null)
            {
                header.RemoveAttribute(AttributeNames.Type);
            }
            else
            {
                header.SetAttribute(new EXRAttribute<string>(AttributeNames.Type, type));
            }
        }
    }

    /// <summary>
    /// Description of the image channels stored in the part.
    /// </summary>
    public ChannelList Channels
    {
        get => header.GetAttributeOrThrow<ChannelList>(AttributeNames.Channels);
        set => header.SetAttribute(new EXRAttribute<ChannelList>(AttributeNames.Channels, value));
    }

    /// <summary>
    /// Specifies the compression method applied to the pixel data of all channels in the part.
    /// </summary>
    public EXRCompression Compression
    {
        get => header.GetAttributeOrThrow<EXRCompression>(AttributeNames.Compression);
        set => header.SetAttribute(new EXRAttribute<EXRCompression>(AttributeNames.Compression, value));
    }

    /// <summary>
    /// The boundaries of an OpenEXR image are given as an axis-parallel rectangular region in pixel space, the display window.
    /// The display window is defined by the positions of the pixels in the upper left and lower right corners, (xMin, yMin) and (xMax, yMax).
    /// </summary>
    public Box2i DisplayWindow
    {
        get => header.GetAttributeOrThrow<Box2i>(AttributeNames.DisplayWindow);
        set => header.SetAttribute(new EXRAttribute<Box2i>(AttributeNames.DisplayWindow, value));
    }

    /// <summary>
    /// An OpenEXR file may not have pixel data for all the pixels in the display window, or the file may have pixel data beyond the boundaries of the display window.
    /// The region for which pixel data are available is defined by a second axis-parallel rectangle in pixel space, the data window.
    /// </summary>
    public Box2i DataWindow
    {
        get => header.GetAttributeOrThrow<Box2i>(AttributeNames.DataWindow);
        set => header.SetAttribute(new EXRAttribute<Box2i>(AttributeNames.DataWindow, value));
    }

    /// <summary>
    /// Specifies in what order the scan lines in the file are stored in the file (increasing Y, decreasing Y, or, for tiled images, also random Y).
    /// </summary>
    public LineOrder LineOrder
    {
        get => header.GetAttributeOrThrow<LineOrder>(AttributeNames.LineOrder);
        set => header.SetAttribute(new EXRAttribute<LineOrder>(AttributeNames.LineOrder, value));
    }

    /// <summary>
    /// Width divided by height of a pixel when the image is displayed with the correct aspect ratio.
    /// A pixel’s width (height) is the distance between the centers of two horizontally (vertically) adjacent pixels on the display.
    /// </summary>
    public float PixelAspectRatio
    {
        get => header.GetAttributeOrThrow<float>(AttributeNames.PixelAspectRatio);
        set => header.SetAttribute(new EXRAttribute<float>(AttributeNames.PixelAspectRatio, value));
    }

    /// <summary>
    /// With <seealso cref="ScreenWindowWidth"/> describes the perspective projection that produced the image. Programs that deal with images as purely
    /// two-dimensional objects may not be able so generate a description of a perspective projection. Those programs should set screenWindowWidth to 1,
    /// and screenWindowCenter to (0, 0).
    /// </summary>
    public V2f ScreenWindowCenter
    {
        get => header.GetAttributeOrThrow<V2f>(AttributeNames.ScreenWindowCenter);
        set => header.SetAttribute(new EXRAttribute<V2f>(AttributeNames.ScreenWindowCenter, value));
    }

    /// <summary>
    /// With <seealso cref="ScreenWindowCenter"/> describes the perspective projection that produced the image. Programs that deal with images as purely
    /// two-dimensional objects may not be able so generate a description of a perspective projection. Those programs should set screenWindowWidth to 1,
    /// and screenWindowCenter to (0, 0).
    /// </summary>
    public float ScreenWindowWidth
    {
        get => header.GetAttributeOrThrow<float>(AttributeNames.ScreenWindowWidth);
        set => header.SetAttribute(new EXRAttribute<float>(AttributeNames.ScreenWindowWidth, value));
    }

    /// <summary>
    /// Determines the size of the tiles and the number of resolution levels in the file. Null for non-tiled parts. See <see cref="IsTiled"/>
    /// </summary>
    /// <remarks>
    /// The OpenEXR library ignores tile description attributes in scan line based files. The decision whether the file contains scan lines or tiles 
    /// is based on the value of bit 9 in the file’s version field, not on the presence of a tile description attribute.
    /// </remarks>
    public TileDesc? Tiles
    {
        get => header.GetAttributeOrDefault<TileDesc>(AttributeNames.Tiles);
        set
        {
            if (value != null)
            {
                header.SetAttribute(new EXRAttribute<TileDesc>(AttributeNames.Tiles, value));
            }
            else
            {
                header.RemoveAttribute(AttributeNames.Tiles);
            }
        }
    }

    /// <summary>
    /// Specifies the view this part is associated with (mostly used for files which stereo views).
    /// 
    /// * A value of left indicate the part is associated with the left eye.
    /// * A value of right indicates the right eye
    /// 
    /// If there is no view attribute in the header, the entire part contains information not dependent on a particular eye.
    /// 
    /// This attribute can be used in the header for multi-part files.
    /// </summary>
    public string? View
    {
        get => header.GetAttributeOrDefault<string>(AttributeNames.View);
        set
        {
            if (value != null)
            {
                header.SetAttribute(new EXRAttribute<string>(AttributeNames.View, value));
            }
            else
            {
                header.RemoveAttribute(AttributeNames.View);
            }
        }
    }

    /// <summary>
    /// Version is required for deep data (deepscanline and deeptile) parts. If not specified for other parts, 1 is assumed.
    /// </summary>
    /// <remarks>
    /// When writing, Jither.OpenEXR will automatically add this attribute if needed. Hence, it cannot be set.
    /// </remarks>
    public int Version => header.GetAttributeOrDefault<int?>(AttributeNames.Version) ?? 1;

    /// <summary>
    /// Indicates whether the part is tiled - that is, whether it has a tiled type attribute or it's in a file with a "single part tiled" version bit.
    /// </summary>
    [MemberNotNullWhen(true, nameof(TilingInformation))]
    public bool IsTiled => Type == PartType.TiledImage || Type == PartType.DeepTiled || isSinglePartTiled;

    /// <summary>
    /// Indicates whether the part is scanline - that is, whether it has a tiled type attribute or it's in a single part file without a "single part tiled" version bit.
    /// </summary>
    public bool IsScanLine => !IsTiled;

    /// <summary>
    /// Indicates whether the part has R, G and B channels (of any type)
    /// </summary>
    public bool IsRGB => HasChannel("R") && HasChannel("G") && HasChannel("B");

    /// <summary>
    /// Indicates whether the part has an A channel (of any type)
    /// </summary>
    public bool HasAlpha => HasChannel("A");

    /// <summary>
    /// Indicates whether the part has a long (> 31 characters) name or any long attribute names or attribute types.
    /// </summary>
    public bool HasLongNames => Name?.Length > 31 || header.Attributes.Any(attr => attr.Name.Length > 31 || attr.Type.Length > 31);

    /// <summary>
    /// Provides access to reading the data from the part. Accessing before file headers have been read (during construction of file) will throw.
    /// </summary>
    public EXRPartDataReader DataReader => dataReader ?? throw new InvalidOperationException("Cannot access data reader before file headers have been read.");

    /// <summary>
    /// Provides access to writing data for a part. Will be null unless <see cref="EXRFile.Write"/> has been called and headers have been written.
    /// </summary>
    public EXRPartDataWriter DataWriter => dataWriter ?? throw new InvalidOperationException("Cannot access data writer before headers have been written (that is, the file's Write() method has been called)");

    /// <summary>
    /// Gets the part number of the part. -1 if the part isn't assigned to a file.
    /// </summary>
    public int PartNumber => file?.GetPartNumber(this) ?? -1;

    /// <summary>
    /// Provides tiling information for the current part, including information on all tiling levels. Null for parts where <see cref="IsTiled"/> is false.
    /// </summary>
    public TilingInformation? TilingInformation
    {
        get
        {
            if (!IsTiled)
            {
                return null;
            }
            return tilingInformation ??= new TilingInformation(this);
        }
    }

    /// <summary>
    /// Creates a new part with default required attributes.
    /// </summary>
    public EXRPart(Box2i dataWindow, Box2i? displayWindow = null, string? name = null, PartType type = PartType.Unknown)
    {
        header = new EXRHeader();
        if (name != null)
        {
            Name = name;
        }
        Type = type;

        // Set default required headers:
        DataWindow = dataWindow;
        DisplayWindow = displayWindow ?? dataWindow;
        Compression = EXRCompression.None;
        LineOrder = LineOrder.IncreasingY;
        PixelAspectRatio = 1;
        ScreenWindowCenter = new V2f(0, 0);
        ScreenWindowWidth = 1;
    }

    public EXRPart(Bounds<int> dataWindow, Bounds<int>? displayWindow = null, string? name = null, PartType type = PartType.Unknown)
        : this(new Box2i(dataWindow), displayWindow != null ? new Box2i(displayWindow) : null, name, type)
    {

    }

    internal EXRPart(EXRHeader header, bool isSinglePartTiled, EXRReadOptions readOptions)
    {
        this.header = header;
        this.readOptions = readOptions;
        this.isSinglePartTiled = isSinglePartTiled;
    }

    internal void AssignFile(EXRFile file)
    {
        this.file = file;
    }

    /// <summary>
    /// Calculates the chunk count based on the current part setup (width, height, tiling, compression etc.)
    /// </summary>
    internal int GetCalculatedChunkCount()
    {
        if (IsTiled)
        {
            return TilingInformation.TotalChunkCount;
        }

        return MathHelpers.DivAndRoundUp(DataWindow.Height, Compression.GetScanLinesPerChunk());
    }

    /// <summary>
    /// Returns the value of the attribute with the given name. If the attribute doesn't exist, returns the default value for the type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public T? GetAttribute<T>(string name)
    {
        if (header.TryGetAttribute<T>(name, out var result))
        {
            return result;
        }
        return default;
    }

    /// <summary>
    /// Retrieves the value of the attribute with the given name, storing it into the value out parameter. The return value indicates whether the attribute existed.
    /// When the return value is false, value will be the default value for the type of attribute.
    /// </summary>
    public bool TryGetAttribute<T>(string name, out T? value)
    {
        return header.TryGetAttribute<T>(name, out value);
    }

    /// <summary>
    /// Retrieves the value of the attribute with the given name, and throws if the attribute wasn't found.
    /// </summary>
    public T GetAttributeOrThrow<T>(string name)
    {
        return header.GetAttributeOrThrow<T>(name);
    }

    /// <summary>
    /// Creates or updates an attribute with the given name, using the given value.
    /// </summary>
    public void SetAttribute<T>(string name, T value)
    {
        header.SetAttribute(new EXRAttribute<T>(name, value));
    }

    internal void WriteHeaderTo(EXRWriter writer)
    {
        header.WriteTo(writer);
    }

    internal void AssignDataReader(EXRPartDataReader reader)
    {
        dataReader = reader;
    }

    internal void AssignDataWriter(EXRPartDataWriter writer)
    {
        dataWriter = writer;
    }

    /// <summary>
    /// Called before writing the part to calculate required attributes.
    /// </summary>
    internal void PrepareForWriting(bool fileIsMultiPart, bool fileHasDeepData)
    {
        if (fileIsMultiPart)
        {
            SetAttribute(AttributeNames.ChunkCount, GetCalculatedChunkCount());
        }
        if (fileHasDeepData)
        {
            SetAttribute(AttributeNames.Version, 1);
        }
    }

    /// <summary>
    /// Does validation of the part's attributes in preparation for reading its data or writing a file.
    /// This method is called by the library before reading data from a part or writing a file, and
    /// will throw <see cref="EXRFormatException"/> in case of any issues.
    /// </summary>
    /// <remarks>
    /// Note that the point of validation is intentionally asymmetric between reading and writing: The library will
    /// only validate parts when their contained data is read, but, at write time, will validate parts early - before
    /// writing any file contents at all. This allows opening and analyzing files with e.g. attribute issues, but will
    /// fail fast if trying to write a file with issues in its parts.
    /// </remarks>
    public void ValidateAttributes(bool fileIsMultiPart, bool fileHasDeepData)
    {
        foreach (var requiredAttribute in RequiredAttributes)
        {
            if (!header.HasAttribute(requiredAttribute))
            {
                throw new EXRFormatException($"Part '{Name}' is missing required attribute '{requiredAttribute}'.");
            }
        }

        foreach (var requiredAttribute in StrictlyRequiredAttributes)
        {
            if (!header.HasAttribute(requiredAttribute))
            {
                // When reading, we allow the user to disable the strict requirements
                if (readOptions?.StrictAttributeRequirements == false)
                {
                    // Set the attribute to its default value. It's created here, so will also be added to any output.
                    if (requiredAttribute == AttributeNames.DisplayWindow)
                    {
                        // We already checked that DataWindow exists
                        header.SetAttribute(new EXRAttribute<Box2i>(AttributeNames.DisplayWindow, DataWindow));
                    }
                    else
                    {
                        header.SetAttribute(defaultAttributes[requiredAttribute]);
                    }
                }
                else
                {
                    throw new EXRFormatException($"Part '{Name}' is missing required attribute '{requiredAttribute}'.");
                }
            }
        }

        if (fileIsMultiPart || fileHasDeepData)
        {
            foreach (var requiredAttribute in RequiredMultiPartAttributes)
            {
                if (!header.HasAttribute(requiredAttribute))
                {
                    throw new EXRFormatException($"Part '{Name}' is missing '{requiredAttribute}' required for multi-part and deep data files.");
                }
            }
        }

        if (fileIsMultiPart)
        {
            if (!header.HasAttribute(AttributeNames.ChunkCount))
            {
                throw new EXRFormatException($"Part '{Name}' is missing '{AttributeNames.ChunkCount}' attribute required for multi-part files.");
            }
        }

        if (Type == PartType.TiledImage || Type == PartType.DeepTiled)
        {
            if (!header.HasAttribute(AttributeNames.Tiles))
            {
                throw new EXRFormatException($"Part '{Name}' is missing '{AttributeNames.Tiles}' attribute required for tiled parts.");
            }
        }

        if (Type == PartType.DeepScanLine || Type == PartType.DeepTiled)
        {
            if (!header.HasAttribute(AttributeNames.Version))
            {
                throw new EXRFormatException($"Part '{Name}' is missing '{AttributeNames.Version}' attribute required for deep data parts.");
            }
            if (!header.HasAttribute(AttributeNames.MaxSamplesPerPixel))
            {
                throw new EXRFormatException($"Part '{Name}' is missing '{AttributeNames.MaxSamplesPerPixel}' attribute required for deep data parts.");
            }
        }

        Channels.Validate();
        DisplayWindow.Validate("DisplayWindow");
        DataWindow.Validate("DataWindow");

        if (readOptions?.MaxImageWidth > 0 && DataWindow.Width > readOptions?.MaxImageWidth)
        {
            throw new EXRFormatException($"Image width ({DataWindow.Width}) exceeds the maximum width of {readOptions?.MaxImageWidth}.");
        }

        if (readOptions?.MaxImageHeight > 0 && DataWindow.Height > readOptions?.MaxImageHeight)
        {
            throw new EXRFormatException($"Image height ({DataWindow.Height}) exceeds the maximum height of {readOptions?.MaxImageHeight}.");
        }

        if (IsTiled)
        {
            if (Channels.AreSubsampled)
            {
                throw new EXRFormatException($"Tiled images cannot use sub-sampling.");
            }
        }
        else
        {
            if (LineOrder == LineOrder.Random)
            {
                throw new EXRFormatException($"Scanline based images cannot use random chunk order.");
            }
        }
    }

    /// <summary>
    /// Checks whether a channel with the given name exists in the part.
    /// </summary>
    public bool HasChannel(string name)
    {
        return Channels.Any(c => c.Name == name);
    }
}
