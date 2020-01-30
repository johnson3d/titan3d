using EngineNS;
using EngineNS.Bricks.Animation.AnimNode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorCommon.Controls.Animation
{
    [EngineNS.Rtti.MetaClass]
    public class NotifyTrack : EngineNS.IO.Serializer.Serializer
    {
        [EngineNS.Rtti.MetaData]
        public int TrackID
        {
            get;set;
        }
        [EngineNS.Rtti.MetaData]
        public Guid NotifyID
        {
            get;set;
        }
        [EngineNS.Rtti.MetaData]
        public string NotifyName
        {
            get; set;
        }
    }
}
