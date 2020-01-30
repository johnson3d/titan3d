using EngineNS.GamePlay.Component;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.RecastRuntime
{
    public partial class NavMeshBoundVolumeComponent
    {
        partial void UpdateMatrixEditor()
        {
            if (CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Editor)
            {
                var editor = CEngine.Instance.GameEditorInstance as EngineNS.Editor.CEditorInstance;
                if (editor != null)
                {
                    var showMesh = Host.FindComponentBySpecialName("EditorShow") as GMeshComponent;
                    if (showMesh != null)
                    {
                        showMesh.Placement.Scale = new Vector3(1 / Host.Placement.Scale.X, 1 / Host.Placement.Scale.Y, 1 / Host.Placement.Scale.Z);
                    }
                }
            }
        }

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
                        showMesh.Placement.Scale = Vector3.UnitXYZ;
                        showMesh.Placement.FaceToCamera(editor.GetMainViewCamera());
                    }
                }
            }
        }
    }
}