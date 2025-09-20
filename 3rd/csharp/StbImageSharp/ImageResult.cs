using EngineNS;
using Hebron.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace StbImageSharp
{
#if !STBSHARP_INTERNAL
	public
#else
	internal
#endif
    partial class TtMemImage
	{
		public int Width { get; set; }
		public int Height { get; set; }
		public ColorComponents SourceComp { get; set; }
		public ColorComponents Comp { get; set; }
        byte[] mData;
        public byte[] Data { get => mData; set => mData = value; }

        internal static unsafe TtMemImage FromResult(byte* result, int width, int height, ColorComponents comp,
			ColorComponents req_comp)
		{
			if (result == null)
				throw new InvalidOperationException(StbImage.stbi__g_failure_reason);

			var image = new TtMemImage
			{
				Width = width,
				Height = height,
				SourceComp = comp,
				Comp = req_comp == ColorComponents.Default ? comp : req_comp
			};

			// Convert to array
			image.Data = new byte[width * height * (int)image.Comp];
			Marshal.Copy(new IntPtr(result), image.Data, 0, image.Data.Length);

			return image;
		}

        public static unsafe TtMemImage FromStream(Stream stream,
			ColorComponents requiredComponents = ColorComponents.Default)
		{
			byte* result = null;

			try
			{
				int x, y, comp;

				var context = new StbImage.stbi__context(stream);

				result = StbImage.stbi__load_and_postprocess_8bit(context, &x, &y, &comp, (int)requiredComponents);

				return FromResult(result, x, y, (ColorComponents)comp, requiredComponents);
			}
			finally
			{
				if (result != null)
					CRuntime.free(result);
			}
		}

		public static TtMemImage FromMemory(byte[] data, ColorComponents requiredComponents = ColorComponents.Default)
		{
			using (var stream = new MemoryStream(data))
			{
				return FromStream(stream, requiredComponents);
			}
		}

		public static IEnumerable<AnimatedFrameResult> AnimatedGifFramesFromStream(Stream stream,
			ColorComponents requiredComponents = ColorComponents.Default)
		{
			return new AnimatedGifEnumerable(stream, requiredComponents);
		}
    }
}