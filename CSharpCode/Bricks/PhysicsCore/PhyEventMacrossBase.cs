using EngineNS.Bricks.CodeBuilder;
using EngineNS.GamePlay.Scene;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.PhysicsCore
{
    [Macross.TtMacross]
    [TtMacrossNodeCustomCodeGen]
    public partial class TtPhyEventMacrossBase : ISceneNodeMacross<object>
    {
        [Rtti.Meta]
        public virtual void OnContact(TtNode selfNode, TtNode otherNode)
        {
            //MacrossGetter.Get()
        }
        [Rtti.Meta]
        public virtual void OnBeginTrigger(TtNode selfNode, TtNode otherNode)
        {
            //MacrossGetter.Get()
        }
        [Rtti.Meta]
        public virtual void OnEndTrigger(TtNode selfNode, TtNode otherNode)
        {
            //MacrossGetter.Get()
        }

        public void SetPropertyValue_Gen(ulong propertyNameHash, in object value)
        {
            
        }
    }
}
