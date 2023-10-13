using Jither.OpenEXR.Attributes;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Jither.OpenEXR;

public class EXRReadOptions
{
    /// <summary>
    /// Although not specified in OpenEXR documentation, a few attributes are "shared"/"common", meaning they should have
    /// the same value for all parts in a multi-part file.
    /// </summary>
    public bool StrictSharedAttributes { get; set; } = true;

    /// <summary>
    /// OpenEXR specs require some attributes (<see cref="EXRPart.StrictlyRequiredAttributes"/> which, nonetheless,
    /// aren't checked, and which some - otherwise valid - EXR files are missing. This settings allows disabling
    /// validation of these attributes, instead providing default values for them. If true, validation will throw
    /// when reading a file with any of these attributes missing.
    /// </summary>
    public bool StrictAttributeRequirements { get; set; } = true;

    /// <summary>
    /// Maximum image (data window) width in pixels. This protects against applications allocating large amounts of memory for
    /// undetected corrupt files. Set to 0 for no restrictions.
    /// </summary>
    public int MaxImageWidth { get; set; } = 1024 * 4096;

    /// <summary>
    /// Maximum image (data window) height in pixels. This protects against applications allocating large amounts of memory for
    /// undetected corrupt files. Set to 0 for no restrictions.
    /// </summary>
    public int MaxImageHeight { get; set; } = 1024 * 4096;
}

public class EXRFile : IDisposable
{
    private readonly List<EXRPart> parts = new();
    private readonly Dictionary<string, EXRPart> partsByName = new();
    private EXRReader? reader;
    private EXRWriter? writer;
    private EXRVersion? originalVersion;

    /// <summary>
    /// List of parts contained in the file, in the order their headers appear in the file.
    /// </summary>
    public IReadOnlyList<EXRPart> Parts => parts;

    /// <summary>
    /// List of part names in the order they appear in the file. Note that any unnamed single part will not appear in this list.
    /// </summary>
    public IEnumerable<string?> PartNames => parts.Select(p => p.Name);

    /// <summary>
    /// Parts contained in the file, indexed by name. Note that any unnamed single part will not appear here.
    /// </summary>
    public IReadOnlyDictionary<string, EXRPart> PartsByName => partsByName;

    /// <summary>
    /// Forces the OpenEXR version to 2 when writing this file, regardless of whether it uses version 2 features. True by default, since
    /// commonly used applications like DJV do not support version 1.
    /// </summary>
    public bool ForceVersion2 { get; set; } = true;

    /// <summary>
    /// File version information for files read from file or stream.
    /// This will throw for files that have not been read from an external source (i.e. files created from scratch).
    /// </summary>
    public EXRVersion OriginalVersion => originalVersion ?? throw new InvalidOperationException($"{nameof(OriginalVersion)} is only available on files that have been read.");

    /// <summary>
    /// Creates a new, empty OpenEXR file.
    /// </summary>
    public EXRFile()
    {

    }

    /// <summary>
    /// Opens an existing OpenEXR file for reading. File and part headers will be read and available after construction.
    /// Use <see cref="EXRPart.DataReader"/> to read the pixel data of a part.
    /// </summary>
    public EXRFile(string path, EXRReadOptions? readOptions = null) : this(new FileStream(path, FileMode.Open, FileAccess.Read), readOptions)
    {

    }

    /// <summary>
    /// Opens a OpenEXR file from a stream for reading. File and part headers will be read and available after construction.
    /// Use <see cref="EXRPart.DataReader"/> to read the pixel data of a part.
    /// </summary>
    public EXRFile(Stream stream, EXRReadOptions? readOptions = null) : this(new EXRReader(stream), readOptions)
    {

    }

    private EXRFile(EXRReader reader, EXRReadOptions? readOptions)
    {
        readOptions ??= new EXRReadOptions();
        this.reader = reader;
        ReadHeaders(reader, readOptions);
    }

    /// <summary>
    /// Writes OpenEXR header and part headers to the given file path, overwriting any existing file at that path.
    /// Use <see cref="EXRPart.DataWriter"/> of the file's parts to write the pixel data.
    /// </summary>
    public void Write(string path)
    {
        Write(new FileStream(path, FileMode.Create, FileAccess.Write));
    }

    /// <summary>
    /// Writes OpenEXR header and part headers to a stream.
    /// Use <see cref="EXRPart.DataWriter"/> of the file's parts to write the pixel data.
    /// </summary>
    public void Write(Stream stream)
    {
        var version = DetermineVersionForWriting();
        writer = new EXRWriter(stream, version.MaxNameLength);
        WriteHeaders(writer, version);
    }

    /// <summary>
    /// Adds a part to the file.
    /// </summary>
    public void AddPart(EXRPart part)
    {
        if (part.Name != null)
        {
            if (partsByName.ContainsKey(part.Name))
            {
                throw new ArgumentException($"A part with the name '{part.Name}' already exists in this EXR file.");
            }
        }
        else
        {
            if (parts.Any(p => p.Name == null))
            {
                throw new ArgumentException($"A nameless part already exists in this EXR file.");
            }
        }
        parts.Add(part);
        part.AssignFile(this);
        if (part.Name != null)
        {
            partsByName.Add(part.Name, part);
        }
    }

    /// <summary>
    /// Removes any path with the given name from the file. Note that <c>null</c> may be passed to delete any unnamed single part.
    /// </summary>
    public void RemovePart(string name)
    {
        if (name == null)
        {
            parts.RemoveAll(p => p.Name == null);
        }
        else
        {
            parts.RemoveAll(p => p.Name == name);
            partsByName.Remove(name);
        }
    }

    internal int GetPartNumber(EXRPart part)
    {
        return parts.IndexOf(part);
    }

    private void ReadHeaders(EXRReader reader, EXRReadOptions readOptions)
    {
        var magicNumber = reader.ReadInt();
        if (magicNumber != 20000630)
        {
            throw new EXRFormatException("Magic number not found.");
        }

        var version = originalVersion = EXRVersion.ReadFrom(reader);

        var headers = new List<EXRHeader>();

        if (version.IsMultiPart)
        {
            while (true)
            {
                var header = EXRHeader.ReadFrom(reader, version.MaxNameLength);
                if (header.IsEmpty)
                {
                    break;
                }
                headers.Add(header);
            }
        }
        else
        {
            var header = EXRHeader.ReadFrom(reader, version.MaxNameLength);
            headers.Add(header);
        }

        long partOffsetTableOffset = reader.Position;
        foreach (var header in headers)
        {
            var part = new EXRPart(header, version.IsSinglePartTiled, readOptions);
            var dataReader = new EXRPartDataReader(part, version, reader, partOffsetTableOffset);
            part.AssignDataReader(dataReader);
            AddPart(part);
            // Find offset table for next part. This is only relevant for multipart, where a chunkCount attribute is required
            // for each part - but we might as well just get it from the calculated chunk count, which uses the chunkCount attribute
            // when it's present.
            partOffsetTableOffset += dataReader.ChunkCount * 8;
        }

        ValidateAfterReadHeaders(readOptions);
    }

    private void WriteHeaders(EXRWriter writer, EXRVersion version)
    {
        foreach (var part in parts)
        {
            part.PrepareForWriting(version.IsMultiPart, version.HasNonImageParts);
        }
        Validate(version);

        writer.WriteInt(20000630);

        version.WriteTo(writer);

        foreach (var part in parts)
        {
            part.WriteHeaderTo(writer);
        }
        
        if (version.IsMultiPart)
        {
            writer.WriteByte(0);
        }

        foreach (var part in parts)
        {
            var dataWriter = new EXRPartDataWriter(part, version, writer);
            dataWriter.WriteOffsetPlaceholders();
            part.AssignDataWriter(dataWriter);
        }
    }

    private EXRVersion DetermineVersionForWriting()
    {
        byte versionNumber = 1;
        EXRVersionFlags flags = EXRVersionFlags.None;
        Debug.Assert(parts.Count > 0);

        if (parts.Count > 1)
        {
            versionNumber = 2;
            flags |= EXRVersionFlags.MultiPart;
        }
        
        if (parts.Count == 1 && parts[0].Type == PartType.TiledImage)
        {
            flags |= EXRVersionFlags.IsSinglePartTiled;
        }

        if (parts.Any(p => p.Type == PartType.DeepScanLine || p.Type == PartType.DeepTiled))
        {
            versionNumber = 2;
            flags |= EXRVersionFlags.NonImageParts;
        }

        if (parts.Any(p => p.HasLongNames))
        {
            flags |= EXRVersionFlags.LongNames;
        }

        if (ForceVersion2)
        {
            versionNumber = 2;
        }

        return new EXRVersion(versionNumber, flags);
    }

    /// <summary>
    /// Does rudimentary validation of the setup of the file and its parts in preparation for writing.
    /// This method is called by the library before writing files, and will throw <see cref="EXRFormatException"/>
    /// in case of any issues.
    /// </summary>
    public void Validate(EXRVersion version)
    {
        if (parts.Count == 0)
        {
            throw new EXRFormatException($"File must have at least one part.");
        }

        if (parts.Count > 1 && parts.Any(p => p.Name == null))
        {
            throw new EXRFormatException($"All parts in multipart file must have a name.");
        }

        foreach (var part in parts)
        {
            part.ValidateAttributes(version.IsMultiPart, version.HasNonImageParts);
        }

        CheckSharedAttributes();
    }

    private void ValidateAfterReadHeaders(EXRReadOptions options)
    {
        if (options.StrictSharedAttributes)
        {
            CheckSharedAttributes();
        }
    }

    private void CheckSharedAttributes()
    {
        CheckSharedAttribute<Box2i>(AttributeNames.DisplayWindow);
        CheckSharedAttribute<float>(AttributeNames.PixelAspectRatio);
        CheckSharedAttribute<TimeCode>(AttributeNames.TimeCode);
        CheckSharedAttribute<Chromaticities>(AttributeNames.Chromaticities);
    }

    private void CheckSharedAttribute<T>(string attributeName)
    {
        var first = parts[0].GetAttribute<T>(attributeName);
        for (int i = 1; i < parts.Count; i++)
        {
            var value = parts[i].GetAttribute<T>(attributeName);
            if (!Object.Equals(value, first))
            {
                throw new EXRFormatException($"Shared attribute '{attributeName}' must have the same value in all parts in a file. Found '{value}' in part {i}, which does not agree with '{first}' in part 0.");
            }
        }
    }

    private bool disposed;

    /// <summary>
    /// Closes the file. No further reading or writing can occur. Alias of <seealso cref="Dispose"/>
    /// </summary>
    public void Close()
    {
        Dispose();
    }

    /// <summary>
    /// Closes the file. No further reading or writing can occur.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }
        if (disposing)
        {
            writer?.Dispose();
            writer = null;
            reader?.Dispose();
            reader = null;
        }
        disposed = true;
    }
}
