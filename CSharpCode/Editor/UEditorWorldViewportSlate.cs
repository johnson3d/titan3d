using System;
using System.Collections.Generic;
using SDL2;

namespace EngineNS.Editor
{
    public class UEditorWorldViewportSlate : EGui.Slate.UWorldViewportSlate
    {
        public UEditorWorldViewportSlate(bool regRoot)
            : base(regRoot)
        {
            CameraController = new Controller.EditorCameraController();
        }
    }
}
