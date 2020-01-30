using EngineNS.GamePlay.Component;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.Camera.Modifier
{
    public enum ModifierStatus
    {
        Success,
        Failure,
        Running,
        Invalid,
    }
    [Rtti.MetaClass]
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Declareable | Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable | Editor.Editor_MacrossClassAttribute.enMacrossType.MacrossGetter | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    [Editor.Editor_MacrossClassIconAttribute("icon/CameraModifier_x64.txpic", RName.enRNameType.Editor)]
    public class GCameraModifier : McComponent
    {
        [Browsable(false)]
        public ModifierStatus Status { get; set; } = ModifierStatus.Invalid;
        [Rtti.MetaData]
        public float Duration { get; set; } = 1.0f;
        protected float mElapseTime = 0.0f;
        public ModifierStatus Perform(GCamera camera)
        {
            mElapseTime += CEngine.Instance.EngineElapseTimeSecond;
            if (mElapseTime > Duration)
            {
                mElapseTime = Duration;
                Status = ModifierStatus.Success;
            }
            else
            {
                Status = ModifierStatus.Running;
            }
            OnPerform(camera);
            return Status;
        }
        public virtual void StartExecution(GCamera camera)
        {
            OnStartExecution(camera);
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnStartExecution(GCamera camera)
        {
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnPerform(GCamera camera)
        {
        }
        public virtual void StopExecution(GCamera camera)
        {
            OnStopExecution(camera);
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnStopExecution(GCamera camera)
        {
        }
    }
}
