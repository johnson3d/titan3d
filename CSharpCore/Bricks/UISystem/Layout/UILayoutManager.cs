using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.UISystem.Layout
{
    internal class UILayoutManager
    {
        // 在极端情况下（内存溢出等），树的强制刷新包含该元素
        private UIElement mForceLayoutElement;
        internal void SetForceLayout(UIElement ui)
        {
            mForceLayoutElement = ui;
        }

        internal static int s_LayoutRecursionLimit = 4096;
        private int mArrangesOnStack = 0;
        private int mMeasuresOnStack = 0;
        private bool mIsUpdating = false;
        private bool mIsInUpdateLayout = false;
        private bool mGotException; // 当UpdateLayout有异常退出时为true
        private bool mLayoutRequestPosted = false;

        private InternalMeasureQueue mMeasureQueue;
        internal LayoutQueue MeasureQueue
        {
            get
            {
                if (mMeasureQueue == null)
                    mMeasureQueue = new InternalMeasureQueue();
                return mMeasureQueue;
            }
        }
        private InternalArrangeQueue mArrangeQueue;
        internal LayoutQueue ArrangeQueue
        {
            get
            {
                if (mArrangeQueue == null)
                    mArrangeQueue = new InternalArrangeQueue();
                return mArrangeQueue;
            }
        }
        private bool HasDirtiness
        {
            get => (!MeasureQueue.IsEmpty) || (!ArrangeQueue.IsEmpty);
        }
        // 一次layout最多执行次数，保证每次tick不会执行布局次数太多
        internal static readonly int LayoutUpdateDeep = 153;

        // 需要更新布局
        internal void NeedsRecalculate()
        {
            if (!mLayoutRequestPosted && !mIsUpdating)
                mLayoutRequestPosted = true;
        }

        internal void UpdateLayout()
        {
            if (mIsInUpdateLayout || mMeasuresOnStack > 0 || mArrangesOnStack > 0)
                return;

            int cnt = 0;
            bool gotException = true;
            UIElement currentElement = null;
            try
            {
                InvalidateTreeIfRecovering();
                while(HasDirtiness)
                {
                    if(++cnt > LayoutUpdateDeep)
                    {
                        currentElement = null;
                        gotException = false;
                        return;
                    }

                    mIsUpdating = true;
                    mIsInUpdateLayout = true;

                    // Measure
                    int loopCounter = 0;
                    var loopStartTime = new DateTime(0);
                    while(true)
                    {
                        if(++loopCounter > LayoutUpdateDeep)
                        {
                            loopCounter = 0;
                            if (loopStartTime.Ticks == 0)
                                loopStartTime = DateTime.UtcNow;
                            else
                            {
                                var loopDuration = (DateTime.UtcNow - loopStartTime);
                                if(loopDuration.Milliseconds > LayoutUpdateDeep * 2)
                                {
                                    currentElement = null;
                                    gotException = false;
                                    return;
                                }
                            }
                        }

                        currentElement = MeasureQueue.GetTopMost();
                        if (currentElement == null)
                            break;
                        var measureSize = currentElement.PreviousAvailableSize;
                        if(currentElement.NeverMeasured)
                        {
                            if(currentElement.Parent == null)
                            {
                                measureSize.Width = float.PositiveInfinity;
                                measureSize.Height = float.PositiveInfinity;
                            }
                            else
                            {
                                measureSize.Width = currentElement.Parent.DesignRect.Width;
                                measureSize.Height = currentElement.Parent.DesignRect.Height;
                            }
                        }

                        currentElement.Measure(ref measureSize);
                    }

                    // Arrange
                    loopCounter = 0;
                    loopStartTime = new DateTime(0);
                    while(MeasureQueue.IsEmpty)
                    {
                        if(++loopCounter > LayoutUpdateDeep)
                        {
                            loopCounter = 0;
                            if (loopStartTime.Ticks == 0)
                                loopStartTime = DateTime.UtcNow;
                            else
                            {
                                var loopDuration = (DateTime.UtcNow - loopStartTime);
                                if(loopDuration.Milliseconds > LayoutUpdateDeep * 2)
                                {
                                    currentElement = null;
                                    gotException = false;
                                    return;
                                }
                            }
                        }

                        currentElement = ArrangeQueue.GetTopMost();
                        if (currentElement == null)
                            break;

                        var finalRect = GetProperArrangeRect(currentElement);
                        currentElement.Arrange(ref finalRect);
                    }
                }

                currentElement = null;
                gotException = false;
            }
            finally
            {
                mIsUpdating = false;
                mLayoutRequestPosted = false;
                mIsInUpdateLayout = false;

                if(gotException)
                {
                    mGotException = true;
                    mForceLayoutElement = currentElement;
                }
            }
        }
        EngineNS.RectangleF GetProperArrangeRect(UIElement uiElement)
        {
            var arrangeRect = uiElement.PreviousArrangeRect;
            if(uiElement.Parent == null)
            {
                arrangeRect.X = arrangeRect.Y = 0;
                if (float.IsPositiveInfinity(uiElement.PreviousAvailableSize.Width))
                    arrangeRect.Width = uiElement.DesiredSize.Width;
                if (float.IsPositiveInfinity(uiElement.PreviousAvailableSize.Height))
                    arrangeRect.Height = uiElement.DesiredSize.Height;
            }
            else if(uiElement.NeverArranged)
            {
                var dr = uiElement.Parent.DesignRect;
                arrangeRect.X = dr.Left;
                arrangeRect.Y = dr.Top;
                arrangeRect.Width = dr.Width;
                arrangeRect.Height = dr.Height;
            }
            return arrangeRect;
        }
        void InvalidateTreeIfRecovering()
        {
            if((mForceLayoutElement != null) || mGotException)
            {
                if(mForceLayoutElement != null)
                {
                    MarkTreeDirty(mForceLayoutElement);
                }
                mForceLayoutElement = null;
                mGotException = false;
            }
        }
        void MarkTreeDirty(UIElement ui)
        {
            while(true)
            {
                var p = ui.Parent;
                if (p == null)
                    break;
                ui = p;
            }

            MarkTreeDirtyHelper(ui);
            MeasureQueue.Add(ui);
            ArrangeQueue.Add(ui);
        }
        void MarkTreeDirtyHelper(UIElement ui)
        {
            ui?.MarkTreeDirty();
        }
        internal void EnterMeasure()
        {
            mMeasuresOnStack++;
            if(mMeasuresOnStack > s_LayoutRecursionLimit)
                throw new InvalidOperationException("EnterMeasure _measuresOnStack > s_LayoutRecursionLimit");
        }
        internal void ExitMeasure()
        {
            mMeasuresOnStack--;
        }
        internal void EnterArrange()
        {
            mArrangesOnStack++;
            if(mArrangesOnStack > s_LayoutRecursionLimit)
                throw new InvalidOperationException("EnterArrange _arrangesOnStack > s_LayoutRecursionLimit");
        }
        internal void ExitArrange()
        {
            mArrangesOnStack--;
        }

        internal UIElement LastExceptionElement;
    }

    public class UILayoutManagerProcesser :CEngineAutoMemberProcessor
    {
        public override async Task<object> CreateObject()
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return new UILayoutManager();
        }
        public override void Tick(object obj)
        {
            var manager = obj as UILayoutManager;
            manager.UpdateLayout();
        }
        public override void Cleanup(object obj)
        {

        }
    }
}

namespace EngineNS
{
    public partial class CEngine
    {
        [CEngineAutoMemberAttribute(typeof(EngineNS.UISystem.Layout.UILayoutManagerProcesser))]
        internal EngineNS.UISystem.Layout.UILayoutManager UILayoutManager
        {
            get;
            set;
        }
    }
}
