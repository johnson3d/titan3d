using EngineNS.EGui.Controls.PropertyGrid;
using EngineNS.IO;
using EngineNS.Macross;
using EngineNS.Thread.Async;
using EngineNS.UI.Bind;
using EngineNS.UI.Canvas;
using NPOI.OpenXmlFormats.Dml;
using NPOI.XSSF.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.UI.Controls.Containers
{
    [TtCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public partial class TtUserControl : TtContainer, IPropertyCustomization
    {
        TtUIElement mChildElement = null;
        RName mChildRName;
        [Rtti.Meta("")]
        [Browsable(false)]
        public RName ChildRName 
        {
            get => mChildRName;
            set
            {
                mChildRName = value;

                var childCount = VisualTreeHelper.GetChildrenCount(this);
                for (int i = 0; i < childCount; i++)
                {
                    var child = VisualTreeHelper.GetChild(this, i);
                    if (child.AssetName == mChildRName)
                    {
                        mChildElement = child;
                        break;
                    }
                }
                IsPropertyVisibleDirty = true;
            }
        }
        // check source is dirty
        public int IOSerialdId = -1;

        TtMacrossGetter<TtUIMacrossBase> mMacrossGetter;
        public override TtMacrossGetter<TtUIMacrossBase> MacrossGetter 
        {
            get
            {
                if (Children.Count > 0 && Children[Children.Count - 1] != null)
                {
                    return Children[Children.Count - 1].MacrossGetter;
                }
                if (mMacrossGetter == null)
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
        public async TtTask CreateChildElement(RName childRName)
        {
            mChildRName = childRName;
            if(mChildElement != null)
            {
                Children.Remove(mChildElement);
                mChildElement = null;
            }
            mChildElement = await TtEngine.Instance.UIManager.AsyncLoad(mChildRName);
            Children.Add(mChildElement);
            UpdateLayout();
            MeshDirty = true;
            IsPropertyVisibleDirty = true;
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

            GetAttachedProperties(ref collection, parentIsValueType);
            GetEvents(ref collection, parentIsValueType, Rtti.TtTypeDesc.TypeOf<TtUserControl>());

            //if (Children.Count > 0)
            //{
            //    var tempProperties = PropertyCollection.PropertyDescCollectionPool.QueryObjectSync();

            //    Children[0].GetSelfProperties(ref tempProperties, parentIsValueType, Rtti.TtTypeDesc.TypeOf(Children[0].GetType()));
            //    collection.Add(tempProperties);

            //    tempProperties.Cleanup();
            //    PropertyCollection.PropertyDescCollectionPool.ReleaseObject(tempProperties);
            //}
            {
                var tempProperties = PropertyCollection.PropertyDescCollectionPool.QueryObjectSync();

                GetSelfProperties(ref tempProperties, parentIsValueType, Rtti.TtTypeDesc.TypeOf<TtUserControl>());
                collection.Add(tempProperties);

                tempProperties.Cleanup();
                PropertyCollection.PropertyDescCollectionPool.ReleaseObject(tempProperties);
            }
        }
        public override object GetPropertyValue(string propertyName)
        {
            if (MacrossGetter == null)
                return null;
            object retVal = null;
            var mc = MacrossGetter.Get();
            if (mc != null)
            {
                retVal = mc.GetPropertyValue(propertyName);
                if(retVal != PropertyNotFindValueClass.PropertyNotFindValue)
                    return retVal;
            }

            return _GetPropertyValue(propertyName);
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

            _SetPropertyValue(propertyName, value);
        }
        public override bool QueryElements<T>(Delegate_QueryProcess<T> queryAction, ref QueryProcessData data, ref T queryData)
        {
            if(data.IgnoreUserControl)
                return false;
            return base.QueryElements(queryAction, ref data, ref queryData);
        }
    }
}