using EngineNS.IO;
using EngineNS.Macross;
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
    public partial class TtUserControl : TtContainer, EGui.Controls.PropertyGrid.IPropertyCustomization
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

        TtMacrossGetter<TtUIMacrossBase> mMacrossGetter;
        public override TtMacrossGetter<TtUIMacrossBase> MacrossGetter 
        {
            get
            {
                if(mMacrossGetter == null)
                {
                    mMacrossGetter = TtMacrossGetter<TtUIMacrossBase>.NewInstance();
                    mMacrossGetter.Name = ChildRName;
                }
                return mMacrossGetter;
            }
            set
            {
                mMacrossGetter = value;
            }
        }

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

        public override TtUIElement GetPointAtElement(ref PointAtProcessData data)
        {
            var retVal = base.GetPointAtElement(ref data);
            if (retVal == this)
                return null;
            return retVal;
        }

        public override void GetProperties(ref EGui.Controls.PropertyGrid.CustomPropertyDescriptorCollection collection, bool parentIsValueType)
        {
            if (MacrossGetter == null)
                return;
            var mc = MacrossGetter.Get();
            if (mc != null)
            {
                mc.GetProperties(ref collection, parentIsValueType);
            }
        }
        public override object GetPropertyValue(string propertyName)
        {
            if (MacrossGetter == null)
                return null;
            var mc = MacrossGetter.Get();
            if (mc != null)
            {
                return mc.GetPropertyValue(propertyName);
            }
            return null;
        }
        public override void SetPropertyValue(string propertyName, object value)
        {
            if (MacrossGetter == null)
                return;
            var mc = MacrossGetter.Get();
            if (mc != null)
            {
                mc.SetPropertyValue(propertyName, value);
            }
        }
        public override bool QueryElements<T>(Delegate_QueryProcess<T> queryAction, ref QueryProcessData data, ref T queryData)
        {
            if(data.IgnoreUserControl)
                return false;
            return base.QueryElements(queryAction, ref data, ref queryData);
        }
    }
}