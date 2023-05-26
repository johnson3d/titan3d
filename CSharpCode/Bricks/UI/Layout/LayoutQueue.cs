using EngineNS.UI.Controls;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI.Layout
{
    internal abstract class TtLayoutQueue
    {
        internal class Request
        {
            internal TtUIElement Target;
            internal Request Next;
            internal Request Prev;
        }

        // 预创建链表大小
        private const int mRequestParentPoolMaxCount = 153;
        // 预创建
        private const int mPocketReserve = 8;

        private Request mRequestHead;
        private Request mCurrentRequest;
        private int mRequestSize;

        internal abstract Request GetRequest(TtUIElement ui);
        internal abstract void SetRequest(TtUIElement ui, Request r);
        internal abstract bool CanRelyOnParentRecalc(TtUIElement parent);
        internal abstract void Invalidate(TtUIElement ui);

        internal TtLayoutQueue()
        {
            Request r;
            for (int i = 0; i < mRequestParentPoolMaxCount; i++)
            {
                r = new Request();
                r.Next = mCurrentRequest;
                mCurrentRequest = r;
            }
            mRequestSize = mRequestParentPoolMaxCount;
        }
        private Request GetNewRequest(TtUIElement ui)
        {
            Request r;
            if (mCurrentRequest != null)
            {
                r = mCurrentRequest;
                mCurrentRequest = r.Next;
                mRequestSize--;
                r.Next = r.Prev = null;
            }
            else
            {
                try
                {
                    r = new Request();
                }
                catch (System.OutOfMemoryException ex)
                {
                    UEngine.Instance.UILayoutManager.SetForceLayout(ui);
                    throw ex;
                }
            }

            r.Target = ui;
            return r;
        }
        private void AddRequest(TtUIElement ui)
        {
            var r = GetNewRequest(ui);
            if (r != null)
            {
                r.Next = mRequestHead;
                if (mRequestHead != null)
                    mRequestHead.Prev = r;
                mRequestHead = r;

                SetRequest(ui, r);
            }
        }
        internal void Add(TtUIElement ui)
        {
            System.Diagnostics.Debug.Assert(UEngine.Instance.ThreadLogic.IsThisThread());

            if (GetRequest(ui) != null)
                return;

            RemoveOrphans(ui);
            var parent = ui.GetUIParentWithinLayoutIsland();
            if (parent != null && CanRelyOnParentRecalc(parent)) return;

            if (mRequestSize > mPocketReserve)
            {
                AddRequest(ui);
            }
            else
            {
                while (ui != null)
                {
                    var p = ui.GetUIParentWithinLayoutIsland();
                    Invalidate(ui);
                    if (p != null && p.Visibility != Visibility.Collapsed)
                    {
                        Remove(ui);
                    }
                    else
                    {
                        //if (p == null)
                        //{
                        if (GetRequest(ui) == null)// && ui.Visibility != Visibility.Collapsed)
                        {
                            RemoveOrphans(ui);
                            AddRequest(ui);
                        }
                        //}
                        //else
                        //{
                        //    break;
                        //}
                    }
                    ui = p;
                }
            }

            UEngine.Instance.UILayoutManager.NeedsRecalculate();
        }
        internal void Remove(TtUIElement ui)
        {
            System.Diagnostics.Debug.Assert(UEngine.Instance.ThreadLogic.IsThisThread());

            Request r = GetRequest(ui);
            if (r == null)
                return;
            RemoveRequest(r);
            SetRequest(ui, null);
        }
        internal void RemoveOrphans(TtUIElement parent)
        {
            System.Diagnostics.Debug.Assert(UEngine.Instance.ThreadLogic.IsThisThread());

            Request r = mRequestHead;
            while (r != null)
            {
                var child = r.Target;
                Request next = r.Next;
                ulong parentTreeLevel = parent.TreeLevel;

                if ((child.TreeLevel == parentTreeLevel + 1)
                    && (child.GetUIParentWithinLayoutIsland() == parent))
                {
                    RemoveRequest(GetRequest(child));
                    SetRequest(child, null);
                }

                r = next;
            }
        }

        internal bool IsEmpty
        {
            get { return (mRequestHead == null); }
        }

        internal TtUIElement GetTopMost()
        {
            System.Diagnostics.Debug.Assert(UEngine.Instance.ThreadLogic.IsThisThread());

            TtUIElement found = null;
            var treeLevel = UInt32.MaxValue;

            for (Request r = mRequestHead; r != null; r = r.Next)
            {
                TtUIElement t = r.Target;
                //if (t.Visibility == Visibility.Collapsed)
                //    continue;

                var l = t.TreeLevel;

                if (l < treeLevel)
                {
                    treeLevel = l;
                    found = r.Target;
                }
            }

            return found;
        }

        private void RemoveRequest(Request req)
        {
            if (req.Prev == null)
                mRequestHead = req.Next;
            else
                req.Prev.Next = req.Next;

            if (req.Next != null)
                req.Next.Prev = req.Prev;

            ReuseRequest(req);
        }

        private void ReuseRequest(Request req)
        {
            req.Target = null;
            if (mRequestSize < mRequestParentPoolMaxCount)
            {
                req.Next = mCurrentRequest;
                mCurrentRequest = req;
                mRequestSize++;
            }
        }
    }

    internal class TtInternalMeasureQueue : TtLayoutQueue
    {
        internal override void SetRequest(TtUIElement ui, Request r)
        {
            ui.MeasureRequest = r;
        }
        internal override Request GetRequest(TtUIElement ui)
        {
            return ui.MeasureRequest;
        }
        internal override bool CanRelyOnParentRecalc(TtUIElement parent)
        {
            return !parent.IsMeasureValid && !parent.MeasureInProgress;
        }
        internal override void Invalidate(TtUIElement ui)
        {
            ui.InvalidateMeasureInternal();
        }
    }

    internal class TtInternalArrangeQueue : TtLayoutQueue
    {
        internal override void SetRequest(TtUIElement ui, Request r)
        {
            ui.ArrangeRequest = r;
        }
        internal override Request GetRequest(TtUIElement ui)
        {
            return ui.ArrangeRequest;
        }
        internal override bool CanRelyOnParentRecalc(TtUIElement parent)
        {
            return !parent.IsArrangeValid && !parent.ArrangeInProgress;
        }
        internal override void Invalidate(TtUIElement ui)
        {
            ui.InvalidateArrangeInternal();
        }
    }
}
