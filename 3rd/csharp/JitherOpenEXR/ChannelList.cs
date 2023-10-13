using Jither.OpenEXR.Attributes;
using Jither.OpenEXR.Drawing;
using System.Collections;
using System.Diagnostics;

namespace Jither.OpenEXR;

/// <summary>
/// List of channels for a part. Note that, per OpenEXR specification, the channel list is always kept
/// alphabetically sorted by name.
/// </summary>
public class ChannelList : IReadOnlyList<Channel>
{
    private readonly List<Channel> channels = new();

    public Channel this[int index] => channels[index];

    public Channel? this[string name] => channels.SingleOrDefault(c => c.Name == name);

    public IEnumerable<string> Names => channels.Select(c => c.Name);

    public int Count => channels.Count;

    public bool AreSubsampled => channels.Any(c => c.IsSubsampled);

    internal int BytesPerPixelNoSubSampling => channels.Sum(c => c.BytesPerPixelNoSubSampling);

    public static ChannelList CreateRGBHalf(bool linear = false)
    {
        var result = new ChannelList
        {
            linear ? Channel.R_HalfLinear : Channel.R_Half,
            linear ? Channel.G_HalfLinear : Channel.G_Half,
            linear ? Channel.B_HalfLinear : Channel.B_Half
        };
        return result;
    }

    public static ChannelList CreateRGBFloat(bool linear = false)
    {
        var result = new ChannelList
        {
            linear ? Channel.R_FloatLinear : Channel.R_Float,
            linear ? Channel.G_FloatLinear : Channel.G_Float,
            linear ? Channel.B_FloatLinear : Channel.B_Float
        };
        return result;
    }

    public static ChannelList CreateRGBAHalf(bool linear = false)
    {
        var result = CreateRGBHalf(linear);
        result.Add(linear ? Channel.A_HalfLinear : Channel.A_Half);
        return result;
    }

    public static ChannelList CreateRGBAFloat(bool linear = false)
    {
        var result = CreateRGBFloat(linear);
        result.Add(linear ? Channel.A_FloatLinear : Channel.A_Float);
        return result;
    }

    public int IndexOf(string name)
    {
        return channels.FindIndex(c => c.Name == name);
    }

    /// <summary>
    /// Adds a channel definition to the channel list.
    /// </summary>
    public void Add(Channel channel)
    {
        channels.Add(channel);
        UpdateSorting();
    }

    /// <summary>
    /// Removes a channel with the given name from the channel list.
    /// </summary>
    public void Remove(string name)
    {
        channels.RemoveAll(c => c.Name == name);
    }

    public static ChannelList ReadFrom(EXRReader reader, int size)
    {
        long totalSize = 0;

        void CheckSize()
        {
            if (totalSize > size)
            {
                throw new EXRFormatException($"Read {totalSize} in channel list, but declared size was {size}");
            }
        }

        var result = new ChannelList();
        long bytesRead;
        while (ReadChannel(reader, out var channel, out bytesRead))
        {
            Debug.Assert(channel != null, "ReadChannel returned true, although channel is null!");
            result.Add(channel);
            totalSize += bytesRead;

            CheckSize();
        }
        totalSize += bytesRead;
        CheckSize();

        return result;
    }

    public void WriteTo(EXRWriter writer)
    {
        writer.WriteInt(
            channels.Count * 16 + // Numeric values
            channels.Sum(c => c.Name.Length + 1) // Null-terminated names
            + 1 // Channel null terminator
            );

        foreach (var channel in channels)
        {
            writer.WriteStringZ(channel.Name);
            writer.WriteInt((int)channel.Type);
            writer.WriteByte((byte)channel.PerceptualTreatment);
            writer.WriteByte(0);
            writer.WriteByte(0);
            writer.WriteByte(0);
            writer.WriteInt(channel.XSampling);
            writer.WriteInt(channel.YSampling);
        }
        writer.WriteByte(0);
    }

    public int GetByteCount(Bounds<int> area)
    {
        return channels.Sum(c => c.GetByteCount(area));
    }

    public ulong GetByteCountLarge(Bounds<int> area)
    {
        ulong result = 0;
        foreach (var channel in channels)
        {
            result += channel.GetByteCountLarge(area);
        }
        return result;
    }

    private static bool ReadChannel(EXRReader reader, out Channel? channel, out long bytesRead)
    {
        var start = reader.Position;
        var name = reader.ReadStringZ(255);
        if (name == "")
        {
            channel = null;
            bytesRead = reader.Position - start;
            return false;
        }

        channel = new Channel(
            name, 
            (EXRDataType)reader.ReadInt(),
            perceptualTreatment: (PerceptualTreatment)reader.ReadByte(),
            reserved0: reader.ReadByte(),
            reserved1: reader.ReadByte(),
            reserved2: reader.ReadByte(),
            xSampling: reader.ReadInt(),
            ySampling: reader.ReadInt()
        );

        bytesRead = reader.Position - start;
        return true;
    }

    public void Validate()
    {
        int index = 0;
        foreach (var channel in channels)
        {
            if (channel.Name == String.Empty)
            {
                throw new EXRFormatException($"Channel {index} has an empty name");
            }
            index++;
        }
    }

    private void UpdateSorting()
    {
        // OpenEXR specifies that "Channels are stored in alphabetical order, according to channel names" [within the pixel data]
        // This actually also applies to the channel list attributes - and the actual sorting is ordinal (and case sensitive) - i.e.,
        // R < r
        channels.Sort((a, b) => String.Compare(a.Name, b.Name, StringComparison.Ordinal));
    }

    public IEnumerator<Channel> GetEnumerator()
    {
        return channels.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override string ToString()
    {
        return String.Join("; ", channels);
    }
}
