using EngineNS.UI.Bind;
using EngineNS.UI.Controls;
using EngineNS.UI.Controls.Containers;
using EngineNS.UI.Template;
using NPOI.HSSF.Record.AutoFilter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI.Trigger
{
    public class TtTriggerCollection
    {
        Dictionary<UInt16, List<TtTriggerBase>> mTriggers = new Dictionary<ushort, List<TtTriggerBase>>();

        public bool HasTrigger(TtBindableProperty property)
        {
            return mTriggers.ContainsKey(property.GlobalIndex);
        }
        public void InvokeTriggers<T>(IBindableObject obj, TtBindableProperty property, in T oldValue, in T newValue)
        {
            List<TtTriggerBase> triggers;
            if(mTriggers.TryGetValue(property.GlobalIndex, out triggers))
            {
                for(int i=0; i<triggers.Count; i++)
                {
                    triggers[i].InvokeAction(obj, property, oldValue, newValue);
                }
            }
        }
        public void Add(UInt16 index, TtTriggerBase trigger)
        {
            List<TtTriggerBase> triggers;
            if(!mTriggers.TryGetValue(index, out triggers))
            {
                triggers = new List<TtTriggerBase>();
                mTriggers[index] = triggers;
            }
            triggers.Add(trigger);
        }
    }

    public abstract class TtTriggerConditionBase
    {
        string mSourceName;
        public string SourceName => mSourceName;
        TtBindableProperty mProperty;
        public TtBindableProperty Property => mProperty;
        UInt64 mPropertyNameHash;
        public UInt64 PropertyNameHash => mPropertyNameHash;

        public TtTriggerConditionBase(TtBindableProperty property, string sourceName = null)
        {
            mSourceName = sourceName;
            mProperty = property;
            mPropertyNameHash = Standart.Hash.xxHash.xxHash64.ComputeHash(mProperty.Name);
        }

        public abstract bool IsMatch(IBindableObject obj);
        public abstract bool IsMatch<T>(in T value);
    }
    public partial class TtTriggerConditionLogical<T> : TtTriggerConditionBase
    {
        public enum ELogicalOperation : byte
        {
            Equal,
            NotEqual,
        }
        ELogicalOperation mOp;
        ValueStoreBase mValue;

        public TtTriggerConditionLogical(TtBindableProperty property, ELogicalOperation op, T value, string sourceName = null)
            : base(property, sourceName)
        {
            mOp = op;
            mValue = new ValueStore<T>(value);
        }

        public override bool IsMatch(IBindableObject obj)
        {
            switch(mOp)
            {
                case ELogicalOperation.Equal:
                    return EqualityComparer<T>.Default.Equals(mValue.GetValue<T>(), obj.GetValue<T>(Property));
                case ELogicalOperation.NotEqual:
                    return !EqualityComparer<T>.Default.Equals(mValue.GetValue<T>(), obj.GetValue<T>(Property));
            }
            return false;
        }
        public override bool IsMatch<TValue>(in TValue value)
        {
            bool result = mValue.IsValueEqual(value);
            switch(mOp)
            {
                case ELogicalOperation.Equal:
                    return result;
                case ELogicalOperation.NotEqual:
                    return !result;
            }
            return false;
        }
    }
    public abstract class TtTriggerValue : TtBindablePropertyValueBase
    {
        public abstract void RestoreValue();
        public abstract void Seal(IBindableObject host, Template.TtUITemplate template);
    }
    public partial class TtTriggerSimpleValue<TProp> : TtTriggerValue
    {
        ValueStoreBase mValue;
        ValueStoreBase mOldValue;
        public TProp Value => (mValue == null) ? default :  mValue.GetValue<TProp>();
        TtBindableProperty mProperty;
        public TtBindableProperty Property => mProperty;
        UInt64 mPropertyNameHash;
        public UInt64 PropertyNameHash => mPropertyNameHash;
        string mTarget;
        public string Target => mTarget;
        dynamic mElement;

        public TtTriggerSimpleValue(TtBindableProperty property, TProp value, string target)
        {
            mProperty = property;
            mPropertyNameHash = Standart.Hash.xxHash.xxHash64.ComputeHash(mProperty.Name);
            mValue = new ValueStore<TProp>(value);
            mOldValue = new ValueStore<TProp>(value);
            mTarget = target;
        }

        public override void Seal(IBindableObject host, TtUITemplate template)
        {
            var container = host as TtContainer;
            if (container == null)
                return;
            mElement = VisualTreeHelper.GetChild(container, mTarget);
        }

        public override T GetValue<T>(TtBindableProperty bp)
        {
            return mValue.GetValue<T>();;
        }

        public override void SetValue<T>(IBindableObject obj, TtBindableProperty bp, in T value)
        {
            SetValue(mElement);
        }
        void SetValue(IBindableObject obj)
        {
            if (obj == null)
                return;
            var objType = obj.GetType();
            if (mProperty.IsAttachedProperty)
            {
                mOldValue.SetValue(obj.GetValue<TProp>(mProperty));
                obj.SetValue(mValue.GetValue<TProp>(), mProperty);
            }
            else if (mProperty.HostType.IsEqual(objType) || mProperty.HostType.IsParentClass(objType))
            {
                var prop = obj.GetType().GetProperty(mProperty.Name);
                mOldValue.SetValue((TProp)prop.GetValue(obj));
                prop.SetValue(obj, mValue.GetValue<TProp>());
            }
            else
            {
                mOldValue.SetValue(obj.GetValue<TProp>(mProperty));
                obj.SetValue(mValue.GetValue<TProp>(), mProperty);
            }
        }
        public override void RestoreValue()
        {
            RestoreValue(mElement);
        }
        void RestoreValue(IBindableObject obj)
        {
            if (obj == null)
                return;
            var objType = obj.GetType();
            if(mProperty.IsAttachedProperty)
            {
                obj.SetValue(mOldValue.GetValue<TProp>(), mProperty);
            }
            else if(mProperty.HostType.IsEqual(objType) || mProperty.HostType.IsParentClass(objType))
            {
                var prop = obj.GetType().GetProperty(mProperty.Name);
                prop.SetValue(obj, mOldValue.GetValue<TProp>());
            }
            else
            {
                obj.SetValue(mOldValue.GetValue<TProp>(), mProperty);
            }
        }
    }

    public abstract class TtTriggerBase
    {
        protected bool mIsSealed = false;
        //protected List<TtTriggerActionBase> mEnterActions;
        //public bool HasEnterActions
        //{
        //    get => (mEnterActions != null) && (mEnterActions.Count > 0);
        //}
        //protected List<TtTriggerActionBase> mExitActions;
        //public bool HasExitActions
        //{
        //    get => (mExitActions != null) && (mExitActions.Count > 0);
        //}

        public abstract void Seal(IBindableObject host, Template.TtUITemplate template);
        public abstract bool CheckCurrentState(IBindableObject obj);
        //public abstract bool CanInvokeActionsNow(IBindableObject element);
        public abstract void InvokeAction<T>(IBindableObject element, TtBindableProperty property, in T oldValue, in T newValue);
    }
    public partial class TtUIPropertyTrigger : TtTriggerBase
    {
        public bool CanRestoreValue = false;
        List<TtTriggerConditionBase> mConditions = new List<TtTriggerConditionBase>();
        List<TtTriggerValue> mSetValues = new List<TtTriggerValue>();

        public void AddCondition(TtTriggerConditionBase condition)
        {
            mConditions.Add(condition);
        }
        public void AddCondition<T>(TtBindableProperty property, T value, TtTriggerConditionLogical<T>.ELogicalOperation op = TtTriggerConditionLogical<T>.ELogicalOperation.Equal, string sourceName = null)
        {
            var condition = new TtTriggerConditionLogical<T>(property, op, value, sourceName);
            mConditions.Add(condition);
        }

        public void AddTriggerValue(TtTriggerValue value)
        {
            mSetValues.Add(value);
        }
        public void AddTriggerValue<T>(TtBindableProperty property, T value, string target = null)
        {
            var setter = new TtTriggerSimpleValue<T>(property, value, target);
            mSetValues.Add(setter);
        }

        public override void Seal(IBindableObject host, TtUITemplate template)
        {
            if (mIsSealed)
                return;

            var uiElement = host as TtUIElement;
            if (uiElement == null)
                return;

            // todo: resource value fix
            for(int i=0; i<mConditions.Count; i++)
            {
                var condition = mConditions[i];
                if(string.IsNullOrEmpty(condition.SourceName))
                {
                    uiElement.Triggers.Add(condition.Property.GlobalIndex, this);
                }
                else
                {
                    // todo: property with source name
                }
            }

            // action set
            for(int i=0; i<mSetValues.Count; i++)
            {
                mSetValues[i].Seal(host, template);
            }

            mIsSealed = true;
        }
        public override bool CheckCurrentState(IBindableObject obj)
        {
            if (obj == null)
                return false;
            bool retVal = (mConditions.Count > 0);
            for(int i=0; retVal && i<mConditions.Count; i++)
            {
                retVal = mConditions[i].IsMatch((dynamic)obj);
            }
            return retVal;
        }

        //public override bool CanInvokeActionsNow(IBindableObject element)
        //{
        //    var uiElem = element as TtUIElement;
        //    if(uiElem != null)
        //    {
        //        if (uiElem.TemplateInternal != null)
        //        {
        //            if (!uiElem.HasTemplateGeneratedSubTree)
        //                return false;
        //        }
        //        return true;
        //    }
        //    return false;
        //}
        public override void InvokeAction<T>(IBindableObject element, TtBindableProperty property, in T oldValue, in T newValue)
        {
            dynamic checkElement = element;

            bool oldState = false;
            bool newState = false;

            for(int i=0; i<mConditions.Count; i++)
            {
                // todo: condition property is from current element child

                if (mConditions[i].Property == property && checkElement == element)
                {
                    oldState = mConditions[i].IsMatch(oldValue);
                    newState = mConditions[i].IsMatch(newValue);
                    if (oldState == newState)
                        return;
                }
                else
                {
                    if(!mConditions[i].IsMatch(checkElement))
                    {
                        oldState = false;
                        newState = false;
                        return;
                    }
                }
            }

            //List<TtTriggerActionBase> actions = null;
            if (!oldState && newState)
            {
                //actions = mEnterActions;

                for(int i=0; i<mSetValues.Count; i++)
                {
                    mSetValues[i].SetValue<T>(element, property, newValue);
                }
            }
            else if(CanRestoreValue && oldState && !newState)
            {
                //actions = mExitActions;
                for(int i=0; i<mSetValues.Count; i++)
                {
                    mSetValues[i].RestoreValue();
                }
            }

            //if (actions != null)
            //    InvokeActions(actions, checkElement);
        }
        //void InvokeActions(List<TtTriggerActionBase> actions, IBindableObject element)
        //{
        //    var uiElement = element as TtUIElement;
        //    if (uiElement == null)
        //        return;
        //    if (CanInvokeActionsNow(uiElement))
        //    {
        //        for(int i=0; i<actions.Count; i++)
        //        {
        //            actions[i].Invoke(element);
        //        }
        //    }
        //    else
        //    {
        //        uiElement.DeferActions(actions);
        //    }
        //}
    }
}
