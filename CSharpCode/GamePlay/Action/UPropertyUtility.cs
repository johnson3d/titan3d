using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Action
{
    public interface IActionRecordable
    {
        UActionRecorder ActionRecorder { get; set; }
    }
}

namespace EngineNS.Graphics.Pipeline.Shader
{
    public partial class UMaterial : GamePlay.Action.IActionRecordable
    {
        public GamePlay.Action.UActionRecorder ActionRecorder { get; set; }
    }
}