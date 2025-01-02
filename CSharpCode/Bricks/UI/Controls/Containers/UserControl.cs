using EngineNS.IO;
using EngineNS.Thread.Async;
using EngineNS.UI.Bind;
using EngineNS.UI.Canvas;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.UI.Controls.Containers
{
    public partial class TtUserControl : TtContainer
    {
        TtUIElement mChildElement = null;
        RName mChildRName;
        [Rtti.Meta]
        public RName ChildRName 
        {
            get => mChildRName;
            set
            {
                mChildRName = value;
                var task = OnSetChildRName();
                TtEngine.Instance.TaskCollector.AddWaitTask(task);
            }
        }
        // check source is dirty
        public int IOSerialdId = -1;

        public TtUserControl()
        {
            mSizeToContent = ESizeToContent.WidthAndHeight;
        }

        public override string GetEditorShowName()
        {
            return "[" + ChildRName.PureName + "]" + Name;
        }

        async TtTask OnSetChildRName()
        {
            var tempElement = await TtEngine.Instance.UIManager.AsyncLoad(ChildRName);
            if(mChildElement != null)
            {
                Children.Remove(mChildElement);
            }
            mChildElement = tempElement;
            Children.Add(mChildElement);
        }
    }
}