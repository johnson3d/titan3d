namespace StbImageSharp
{
#if !STBSHARP_INTERNAL
	public
#else
	internal
#endif
	class AnimatedFrameResult : TtMemImage
	{
		public int DelayInMs { get; set; }
	}
}