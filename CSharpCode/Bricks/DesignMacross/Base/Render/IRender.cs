namespace EngineNS.DesignMacross.Base.Render
{
    public interface IRenderableElement
    {

    }

    public interface IElementRender<T> where T : struct
    {
        public void Draw(IRenderableElement renderableElement, ref T context);
    }
}
