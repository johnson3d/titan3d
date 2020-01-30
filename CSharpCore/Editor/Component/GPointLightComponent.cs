using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Component
{
    partial class GPointLightComponent
    {
        partial void TickEditor()
        {
            if (CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Editor)
            {
                var editor = CEngine.Instance.GameEditorInstance as EngineNS.Editor.CEditorInstance;
                if (editor != null)
                {
                    var showMesh = Host.FindComponentBySpecialName("EditorShow") as GMeshComponent;
                    if (showMesh != null)
                    {
                        showMesh.Placement.FaceToCamera(editor.GetMainViewCamera());
                    }
                }
            }
        }
    }
}
