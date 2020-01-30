using EngineNS.GamePlay.Component;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Camera.ControlStratety
{
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Declareable| Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable| Editor.Editor_MacrossClassAttribute.enMacrossType.MacrossGetter| Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    [Editor.Editor_MacrossClassIconAttribute("icon/CameraControlStrategy_x64.txpic", RName.enRNameType.Editor)]
    public class GCameraControlStrategy: McComponent
    {
        public void StartExecution(GCamera camera)
        {
            OnStartExecution(camera);
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnStartExecution(GCamera camera)
        {
        }
        public void Perform(GCamera camera)
        {
            OnPerform(camera);
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnPerform(GCamera camera)
        {

        }
        public void StopExecution(GCamera camera)
        {
            OnStopExecution(camera);
        }
        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void OnStopExecution(GCamera camera)
        {
        }
    }
}
