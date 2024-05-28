namespace EngineNS.DesignMacross.Base.Graph
{
    public class TtGraphViewport
    {
        //视口 screen space
        public Vector2 Location { get; set; } = Vector2.Zero;
        public SizeF Size { get; set; } = new SizeF(100, 100);
        public bool IsInViewport(Vector2 screenPos)
        {
            Rect veiwPortRect = new Rect(Location, Size);
            return veiwPortRect.Contains(screenPos);
        }

        public Vector2 ViewportTransform(Vector2 cameraPos, Vector2 pos)
        {
            return pos - cameraPos + Location;
        }
        public Vector2 ViewportInverseTransform(Vector2 cameraPos, Vector2 pos)
        {
            return pos + cameraPos - Location;
        }
    }
}
