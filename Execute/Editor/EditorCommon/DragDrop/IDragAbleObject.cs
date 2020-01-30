namespace EditorCommon.DragDrop
{
    public interface IDragAbleObject
    {
        System.Windows.FrameworkElement GetDragVisual();
    }

    public interface IDragToViewport
    {
        System.Threading.Tasks.Task OnDragEnterViewport(ViewPort.ViewPortControl viewport, System.Windows.Forms.DragEventArgs e);
        System.Threading.Tasks.Task OnDragLeaveViewport(ViewPort.ViewPortControl viewport, System.EventArgs e);
        System.Threading.Tasks.Task OnDragOverViewport(ViewPort.ViewPortControl viewport, System.Windows.Forms.DragEventArgs e);
        System.Threading.Tasks.Task OnDragDropViewport(ViewPort.ViewPortControl viewport, System.Windows.Forms.DragEventArgs e);
    }
}
