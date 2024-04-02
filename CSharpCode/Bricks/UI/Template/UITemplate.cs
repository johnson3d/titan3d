using EngineNS.Rtti;
using EngineNS.UI.Bind;
using EngineNS.UI.Controls;
using EngineNS.UI.Controls.Containers;
using EngineNS.UI.Trigger;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace EngineNS.UI.Template
{
    public class TtUIElementFactory
    {
        UTypeDesc mType;
        public UTypeDesc Type
        {
            get => mType;
            set
            {
                mType = value;
            }
        }
        string mText = null;
        public string Text
        {
            get => mText;
            set
            {
                mText = value;
            }
        }
        int mChildIndex = -1;
        TtUIElementFactory mParent;
        public TtUIElementFactory Parent => mParent;
        TtUIElementFactory mFirstChild;
        public TtUIElementFactory FirstChild => mFirstChild;
        TtUIElementFactory mLastChild;
        public TtUIElementFactory LastChild => mLastChild;
        TtUIElementFactory mNextSibling;
        public TtUIElementFactory NextSibling => mNextSibling;
        bool mSealed = false;
        public bool Sealed => mSealed;
        TtUITemplate mOwnerTemplate;
        public TtUITemplate OwnerTemplate => mOwnerTemplate;
        Dictionary<Bind.TtBindableProperty, Bind.TtBindablePropertyValueBase> mTemplateProperties = new Dictionary<Bind.TtBindableProperty, Bind.TtBindablePropertyValueBase>();

        public TtUIElementFactory()
            : this(null, null)
        {
        }
        public TtUIElementFactory(UTypeDesc type)
            : this(type, null)
        {
        }
        public TtUIElementFactory(string text)
            : this(null, text)
        {
        }
        public TtUIElementFactory(UTypeDesc type, string text)
        {
            Type = type;
            Text = text;
        }

        internal void Seal(TtUITemplate ownerTemplate)
        {
            if (mSealed)
                return;

            mOwnerTemplate = ownerTemplate;
            if (mType == null && mText == null)
            {
                throw new InvalidOperationException("Factory template type is null");
            }

            if (typeof(TtContentsPresenter).IsAssignableFrom(Type.SystemType))
            {
                // todo: set content template bind property
            }

            mChildIndex = ownerTemplate.CreateChildIndex();

            var child = mFirstChild;
            while (child != null)
            {
                if (mOwnerTemplate != null)
                    child.Seal(mOwnerTemplate);

                child = child.mNextSibling;
            }
            mSealed = true;
        }
        internal TtUIElement InstantiateTree(TtUIElement container, TtUIElement parent)
        {
            if(!Sealed)
                throw new InvalidOperationException("Can't instantiate before sealed!");

            dynamic treeNode = null;
            if(mText != null)
            {
            }
            else
            {
                var logicParent = container as TtContainer;
                treeNode = UTypeDescManager.CreateInstance(mType, logicParent);
                treeNode.mTemplateParent = container;
                treeNode.TemplateChildIndex = mChildIndex;
                var parentContainer = parent as TtContainer;
                if (parentContainer == null)
                    throw new InvalidCastException("parent is not container");

                parentContainer.Children.mChildren.Add(treeNode);
                treeNode.RootUIHost = parent.RootUIHost;
                treeNode.mVisualParent = parentContainer;
                // instance values
                foreach(var valData in mTemplateProperties)
                {
                    if(valData.Value is TtTemplateBindingValue)
                    {
                        var bindValue = valData.Value as TtTemplateBindingValue;
                        bindValue.BindingTemplate((IBindableObject)treeNode, container);
                    }
                    else if(valData.Value is TtTemplateSimpleValue)
                    {
                        var bindValue = valData.Value as TtTemplateSimpleValue;
                        treeNode.SetTemplateValue(bindValue);
                    }
                    //valData.Value.SetValue(treeNode, valData.Key, container);
                }
                if(mType.IsEqual(typeof(TtContentsPresenter)))
                {
                    var cpNode = treeNode as TtContentsPresenter;
                    if(string.IsNullOrEmpty(cpNode.ContentSource))
                    {
                        parentContainer.ChildIsContentsPresenter = true;
                        logicParent.mLogicContentsPresenter = cpNode;
                    }
                    else
                    {
                        var contentParent = logicParent.GetContentsPresenterContainer(cpNode.ContentSourceHash);
                        contentParent.mLogicContentsPresenter = cpNode;
                    }
                }
                var child = mFirstChild;
                while(child != null)
                {
                    child.InstantiateTree(container, treeNode);
                    child = child.NextSibling;
                }
            }

            return treeNode;
        }
        public void AppendChild(TtUIElementFactory child)
        {
            if (mSealed)
                throw new InvalidOperationException("Can't change after sealed");
            if (child == null)
                throw new ArgumentNullException("child");
            if (child.Parent != null)
                throw new ArgumentException("child already has parent");
            if (mText != null)
                throw new InvalidOperationException("Can't add child with text set");
            if(mFirstChild == null)
            {
                mFirstChild = child;
                mLastChild = child;
            }
            else
            {
                mLastChild.mNextSibling = child;
                mLastChild = child;
            }
            child.mParent = this;
        }
        public void SetValue(TtBindableProperty prop, TtBindablePropertyValueBase val)
        {
            mTemplateProperties[prop] = val;
        }
        public void SetValue<T>(TtBindableProperty prop, T val)
        {
            var simpleVal = new TtTemplateSimpleValue();
            simpleVal.Value = val;
            simpleVal.Property = prop;
            simpleVal.PropertyNameHash = Standart.Hash.xxHash.xxHash64.ComputeHash(prop.Name);
            mTemplateProperties[prop] = simpleVal;
        }
        public void SetTemplateBindingValue<TTag, TSrc>(TtBindableProperty prop, string propertyName, string templatePropertyName, EBindingMode mode = EBindingMode.Default)
        {
            var binding = new TtTemplateBindingValue<TTag, TSrc>(propertyName, templatePropertyName, mode);
            mTemplateProperties[prop] = binding;
        }
    }

    public abstract class TtUITemplate
    {
        List<TtTemplateSimpleValue> mDefaultValues = new List<TtTemplateSimpleValue>();
        public List<TtTemplateSimpleValue> DefaultValues => mDefaultValues;

        TtUIElementFactory mTemplateRoot;
        public TtUIElementFactory TemplateRoot
        {
            get => mTemplateRoot;
            set
            {
                mTemplateRoot = value;
            }
        }

        int mLastChildIndex = 1;
        internal int LastChildIndex => mLastChildIndex;
        internal int CreateChildIndex()
        {
            if (LastChildIndex >= (int)TtUIElement.eInternalFlags.TemplateIndexDefault)
                throw new InvalidOperationException("Template has too many children!");

            var retVal = mLastChildIndex;
            Interlocked.Increment(ref mLastChildIndex);
            return retVal;
        }

        protected abstract void CheckTemplateParentValid(TtUIElement templateParent);

        bool mSealed = false;
        public void Seal()
        {
            if (mSealed)
                return;

            ProcessTemplateBeforeSeal();
            mTemplateRoot?.Seal(this);
            mSealed = true;
        }

        protected virtual void ProcessTemplateBeforeSeal()
        {

        }

        internal bool ApplyTemplateContent(TtUIElement container)
        {
            CheckTemplateParentValid(container);

            if(mTemplateRoot != null)
            {
                if (CheckForCircularReferencesInTemplateTree(container))
                    return false;

#if DEBUG_UI
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "UI", $"ApplyTemplateContent with {container.Name}({container.GetType().FullName})");
#endif
                mTemplateRoot.InstantiateTree(container, container);

                for(int i=0; i<mTriggers.Count; i++)
                {
                    mTriggers[i].Seal(container, this);
                }
            }

            return true;
        }

        bool CheckForCircularReferencesInTemplateTree(TtUIElement container)
        {
            var curNode = container;
            TtUIElement nextParent = null;
            while (curNode != null)
            {
                nextParent = curNode.TemplateParent;

                if(curNode != container && nextParent != null)
                {
                    if(curNode.TemplateInternal == this)
                    {
                        if(curNode.GetType() == container.GetType())
                        {
                            return true;
                        }
                    }
                }

                curNode = (curNode is TtContentsPresenter)? null : nextParent;
            }
            return false;
        }

        #region Trigger

        List<TtTriggerBase> mTriggers = new List<TtTriggerBase>();
        public void AddTrigger(TtTriggerBase trigger)
        {
            mTriggers.Add(trigger);
        }

        #endregion
    }
}
