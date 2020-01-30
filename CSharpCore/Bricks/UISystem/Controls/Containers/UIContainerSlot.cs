using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.UISystem.Controls.Containers
{
    [Rtti.MetaClass]
   
    public abstract class UIContainerSlot : EngineNS.IO.Serializer.Serializer, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion
       
        public EngineNS.UISystem.UIElement Parent;
       
        public EngineNS.UISystem.UIElement Content;

        public abstract EngineNS.SizeF Measure(ref EngineNS.SizeF availableSize);
        public abstract void Arrange(ref EngineNS.RectangleF arrangeSize);
        public abstract void ProcessSetContentDesignRect(ref EngineNS.RectangleF tagRect);
        public abstract bool NeedUpdateLayoutWhenChildDesiredSizeChanged(UIElement child);
        public virtual Type GetSlotOperatorType() { return null; }
    }

    public interface ISlotOperator
    {
        Task Init(EngineNS.CRenderContext rc);
        void UpdateShow(EngineNS.UISystem.Editor.SelectedData operationUIData, ref EngineNS.RectangleF windowDesignSize, ref EngineNS.RectangleF windowDrawSize);
        void ProcessSelect(EngineNS.UISystem.Editor.SelectedData operationUIData, ref EngineNS.PointF mouseInViewport);
        bool ProcessMousePointAt(ref EngineNS.PointF mouseInViewport, Action<EngineNS.UISystem.Editor.enCursors> setCursorAction);
        bool IsSelectedOperator();
        void Operation(EngineNS.UISystem.Editor.SelectedData operationUIData, ref EngineNS.Vector2 mouseDelta, float scaleDelta);
        void Commit(EngineNS.CCommandList cmd);
        void Draw(EngineNS.CRenderContext rc, EngineNS.CCommandList cmd, EngineNS.Graphics.View.CGfxScreenView view);
    }
}
