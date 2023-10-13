using Jither.OpenEXR.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jither.OpenEXR.Converters;

/// <summary>
/// Converts between OpenEXR pixel data structure and pixel interleaved structure. That is: AAAAA BBBBB GGGGG RRRR -> (e.g.) RGBA RGBA RGBA RGBA RGBA
/// </summary>
internal class PixelInterleaveConverter : PixelConverter
{
    private readonly List<int> channelOrder;
    private readonly int interleavedBytesPerPixel;
    private readonly ChannelList channels;

    public PixelInterleaveConverter(ChannelList channels, params string[] channelOrder)
    {
        this.channels = channels;
        this.channelOrder = BuildChannelOrder(channels, channelOrder, out interleavedBytesPerPixel);
    }

    public override void ToEXR(Bounds<int> bounds, ReadOnlySpan<byte> source, Span<byte> dest)
    {
        // We don't allow missing channels when converting *to* EXR - the part's channel list needs to match the pixel data.
        for (int i = 0; i < channels.Count; i++)
        {
            if (channelOrder[i] < 0)
            {
                throw new InvalidOperationException($"Channel order for interleaved chunk is missing channel '{channels[i].Name}'.");
            }
        }

        int destOffset = 0;
        int scanlineCount = bounds.Height;

        for (int scanline = 0; scanline < scanlineCount; scanline++)
        {
            int channelIndex = 0;
            int scanlineOffset = scanline * bounds.Width * interleavedBytesPerPixel;

            foreach (var channel in channels)
            {
                int channelBytesPerPixel = channel.Type.GetBytesPerPixel();
                int sourceOffset = channelOrder[channelIndex++];

                sourceOffset += scanlineOffset;

                for (int i = 0; i < bounds.Width; i++)
                {
                    for (int j = 0; j < channelBytesPerPixel; j++)
                    {
                        dest[destOffset++] = source[sourceOffset + j];
                    }
                    sourceOffset += interleavedBytesPerPixel;
                }
            }
        }
    }

    public override void FromEXR(Bounds<int> bounds, ReadOnlySpan<byte> source, Span<byte> dest)
    {
        int sourceOffset = 0;
        int scanlineCount = bounds.Height;

        for (int scanline = 0; scanline < scanlineCount; scanline++)
        {
            int channelIndex = 0;
            int scanlineOffset = scanline * bounds.Width * interleavedBytesPerPixel;

            foreach (var channel in channels)
            {
                int channelBytesPerPixel = channel.Type.GetBytesPerPixel();
                int destOffset = channelOrder[channelIndex++];

                if (destOffset < 0)
                {
                    // Skip this channel - not part of output
                    sourceOffset += bounds.Width * channelBytesPerPixel;
                    continue;
                }

                destOffset += scanlineOffset;

                for (int i = 0; i < bounds.Width; i++)
                {
                    for (int j = 0; j < channelBytesPerPixel; j++)
                    {
                        dest[destOffset + j] = source[sourceOffset++];
                    }
                    destOffset += interleavedBytesPerPixel;
                }
            }
        }
    }

    private static List<int> BuildChannelOrder(ChannelList channels, IEnumerable<string> channelOrder, out int bytesPerPixel)
    {
        var offsets = new List<int>(channels.Count);

        for (int i = 0; i < channels.Count; i++)
        {
            offsets.Add(-1);
        }

        int startOffset = 0;
        foreach (var outputChannel in channelOrder)
        {
            var channelIndex = channels.IndexOf(outputChannel);
            if (channelIndex < 0)
            {
                throw new ArgumentException($"Unknown channel name in interleaved channel order: {outputChannel}. Should be one of: {String.Join(", ", channels.Select(c => c.Name))}", nameof(channelOrder));
            }
            var inputChannel = channels[channelIndex];
            offsets[channelIndex] = startOffset;
            startOffset += inputChannel.Type.GetBytesPerPixel();
        }

        // startOffset is now also "magically" the number of bytes per interleaved pixel
        bytesPerPixel = startOffset;
        return offsets;
    }
}
