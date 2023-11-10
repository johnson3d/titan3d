using EngineNS.UI.Bind;
using EngineNS.UI.Template;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.UI.Controls
{
    public partial class TtUIElement
    {
        static partial void TtUIELement_Template()
        {

        }

        partial void TtUIElementConstructor_Template()
        {

        }

        TtUITemplate mTemplate;
        [BindProperty, Browsable(false)]
        public TtUITemplate Template
        {
            get => mTemplate;
            set
            {
                OnValueChange(value, mTemplate);
                mTemplate = value;
            }
        }
        internal virtual Template.TtUITemplate TemplateInternal => Template;
        internal TtUIElement mTemplateParent = null;
        [Browsable(false)]
        public TtUIElement TemplateParent => mTemplateParent;
        internal int TemplateChildIndex
        {
            get
            {
                uint childIndex = ((uint)mInternalFlags & 0xFFFF);
                if (childIndex == 0xFFFF)
                    return -1;
                else
                    return (int)childIndex;
            }
            set
            {
                if (value < -1 || value >= 0xFFFF)
                    throw new ArgumentOutOfRangeException("TemplateChildIndex is out of range");
                uint childIdx = (value == -1) ? 0xFFFF : (uint)value;
                mInternalFlags = (eInternalFlags)(childIdx | (((uint)mInternalFlags) & 0xFFFF0000));
            }
        }

        protected virtual void OnPreApplyTemplate()
        {

        }
        protected virtual void OnApplyTemplate()
        {

        }
        protected virtual void OnPostApplyTemplate()
        {

        }
        public bool ApplyTemplate()
        {
            if (!UEngine.Instance.EventPoster.IsThread(Thread.Async.EAsyncTarget.Logic))
            {
                throw new InvalidOperationException("ApplyTemplate need be call in logic thread");
            }

            OnPreApplyTemplate();
            var retValue = false;
            var template = TemplateInternal;
            if(template != null)
            {
                var tryCount = 2;
                for(int i=0; i < tryCount; i++)
                {
                    if(!HasTemplateGeneratedSubTree)
                    {
                        retValue = template.ApplyTemplateContent(this);
                        if(retValue)
                        {
                            HasTemplateGeneratedSubTree = true;
                            // trigger action when 
                            InovkeDeferredActions();
                            OnApplyTemplate();
                        }

                        if(template != TemplateInternal)
                        {
                            template = TemplateInternal;
                            continue;
                        }
                    }
                    break;
                }

                if(HasTemplateGeneratedSubTree)
                {
                    for(int i=0; i<mDefferedTriggers.Count; i++)
                    {
                        mDefferedTriggers[i].Seal(this, TemplateInternal);
                    }
                    mDefferedTriggers.Clear();
                }

                // set default value
                for(int i=0; i<template.DefaultValues.Count; i++)
                {
                    template.DefaultValues[i].SetTempalteValue(this);
                }
            }

            OnPostApplyTemplate();

            return retValue;
        }
    }
}
