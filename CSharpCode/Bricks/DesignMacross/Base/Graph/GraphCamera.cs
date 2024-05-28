namespace EngineNS.DesignMacross.Base.Graph
{
    public class TtGraphCamera
    {
        public Vector2 Location { get; set; } = Vector2.Zero;
        public SizeF Size { get; set; } = new SizeF(100, 100);
        public Vector2 Scale { get; set; } = Vector2.One;
    }
}
