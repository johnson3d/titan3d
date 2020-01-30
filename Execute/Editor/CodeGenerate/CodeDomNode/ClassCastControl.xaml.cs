using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;

namespace CodeDomNode
{
    /// <summary>
    /// Interaction logic for ClassCastControl.xaml
    /// </summary>
    public partial class ClassCastControl : CodeGenerateSystem.Base.IDebugableNode
    {
        #region IDebugableNode

        bool mBreaked = false;
        public bool Breaked
        {
            get { return mBreaked; }
            set
            {
                if (mBreaked == value)
                    return;
                mBreaked = value;
                EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
                {
                    BreakedPinShow = mBreaked;
                    ChangeParentLogicLinkLine(mBreaked);
                    return true;
                }, EngineNS.Thread.Async.EAsyncTarget.Editor);
            }
        }

        public void ChangeParentLogicLinkLine(bool change)
        {
            ChangeParentLogicLinkLine(change, ClassLinkHandle_In);
        }
        public override void Tick(long elapsedMillisecond)
        {
            TickDebugLine(elapsedMillisecond, ClassLinkHandle_In);
        }
        public bool CanBreak()
        {
            return true;
        }

        #endregion


        partial void InitConstruction()
        {
            InitializeComponent();

            mCtrlMethodIn = ClassLinkHandle_In;
            mCtrlMethodOut = ClassLinkHandle_Out;
            mTargetPin = TargetPin;
            mCastFailedPin = CastFailedPin;
            mCastResultPin = CastResultPin;
        }

        protected override void CollectionErrorMsg()
        {
            if(!mTargetPin.HasLink)
            {
                HasError = true;
                ErrorDescription = "Target未连接";
            }
        }
    }
}
