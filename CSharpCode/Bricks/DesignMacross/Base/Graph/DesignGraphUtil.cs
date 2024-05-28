namespace EngineNS.DesignMacross.Base.Graph
{
    public class TtDesignGraphUtil
    {
        public static Vector2 CalculateAbsLocation(IGraphElement element)
        {
            if (element.Parent != null)
            {
                return element.Location + CalculateAbsLocation(element.Parent);
            }
            return element.Location;
        }
    }
}
