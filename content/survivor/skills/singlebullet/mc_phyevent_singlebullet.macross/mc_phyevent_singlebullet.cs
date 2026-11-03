namespace NS_survivor.skills.singlebullet
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "survivor/skills/singlebullet/mc_phyevent_singlebullet.macross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class mc_phyevent_singlebullet : EngineNS.Bricks.PhysicsCore.TtPhyEventMacrossBase,EngineNS.Bricks.CodeBuilder.ISceneNodeMacross<System.Boolean>
    {
        public static EngineNS.Macross.TtMacrossBreak breaker_FindFirstChild_1656828671 = new EngineNS.Macross.TtMacrossBreak("breaker_FindFirstChild_1656828671");
        public static EngineNS.Macross.TtMacrossBreak breaker_Attack_2538664108 = new EngineNS.Macross.TtMacrossBreak("breaker_Attack_2538664108");
        public static EngineNS.Macross.TtMacrossBreak breaker_if_141321131 = new EngineNS.Macross.TtMacrossBreak("breaker_if_141321131");
        EngineNS.Macross.TtMacrossStackFrame mFrame_OnBeginTrigger_3141953707 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/skills/singlebullet/mc_phyevent_singlebullet.macross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_OnBeginTrigger_3141953707 = new EngineNS.Macross.TtMacrossStackTracer();
        [EngineNS.Rtti.MetaAttribute]
        public override void OnBeginTrigger(EngineNS.GamePlay.Scene.TtNode selfNode,EngineNS.GamePlay.Scene.TtNode otherNode)
        {
            #if !disable_macross_392fa105_7155_429d_8767_a951f1135d70
            using(var guard_OnBeginTrigger = new EngineNS.Macross.TtMacrossStackGuard(mStack_OnBeginTrigger_3141953707,mFrame_OnBeginTrigger_3141953707))
            {
                mFrame_OnBeginTrigger_3141953707.SetWatchVariable("selfNode", selfNode);
                mFrame_OnBeginTrigger_3141953707.SetWatchVariable("otherNode", otherNode);
                Survivor.TtWeaponProxyNode tmp_r_FindFirstChild_1656828671 = default(Survivor.TtWeaponProxyNode);
                mFrame_OnBeginTrigger_3141953707.SetWatchVariable("Condition0_141321131", (selfNode.Parent != null));
                breaker_if_141321131.TryBreak(mStack_OnBeginTrigger_3141953707, this);
                if ((selfNode.Parent != null))
                {
                    mFrame_OnBeginTrigger_3141953707.SetWatchVariable("v_name_FindFirstChild_1656828671", "");
                    mFrame_OnBeginTrigger_3141953707.SetWatchVariable("v_type_FindFirstChild_1656828671", typeof(Survivor.TtWeaponProxyNode));
                    mFrame_OnBeginTrigger_3141953707.SetWatchVariable("v_bRecursive_FindFirstChild_1656828671", true);
                    breaker_FindFirstChild_1656828671.TryBreak(mStack_OnBeginTrigger_3141953707, this);
                    tmp_r_FindFirstChild_1656828671 = (Survivor.TtWeaponProxyNode)selfNode.Parent.FindFirstChild("",typeof(Survivor.TtWeaponProxyNode),true);
                    mFrame_OnBeginTrigger_3141953707.SetWatchVariable("tmp_r_FindFirstChild_1656828671", tmp_r_FindFirstChild_1656828671);
                    if ((tmp_r_FindFirstChild_1656828671 != null))
                    {
                        mFrame_OnBeginTrigger_3141953707.SetWatchVariable("v_targetNode_Attack_2538664108", otherNode.Parent);
                        mFrame_OnBeginTrigger_3141953707.SetWatchVariable("v_bulletNode_Attack_2538664108", selfNode.Parent);
                        breaker_Attack_2538664108.TryBreak(mStack_OnBeginTrigger_3141953707, this);
                        tmp_r_FindFirstChild_1656828671.WeaponNode.Attack(otherNode.Parent,selfNode.Parent);
                    }
                }
                else
                {
                }
            }
            #endif //!disable_macross_392fa105_7155_429d_8767_a951f1135d70
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_SetPropertyValue_Gen_5757565 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/skills/singlebullet/mc_phyevent_singlebullet.macross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_SetPropertyValue_Gen_5757565 = new EngineNS.Macross.TtMacrossStackTracer();
        void EngineNS.Bricks.CodeBuilder.ISceneNodeMacross<System.Boolean>.SetPropertyValue_Gen(System.UInt64 nameHash,in System.Boolean value)
        {
            #if !disable_macross_392fa105_7155_429d_8767_a951f1135d70
            using(var guard_SetPropertyValue_Gen = new EngineNS.Macross.TtMacrossStackGuard(mStack_SetPropertyValue_Gen_5757565,mFrame_SetPropertyValue_Gen_5757565))
            {
                mFrame_SetPropertyValue_Gen_5757565.SetWatchVariable("nameHash", nameHash);
                mFrame_SetPropertyValue_Gen_5757565.SetWatchVariable("value", value);
                switch (nameHash)
                {
                    case 6226757829267111881:
                    {
                        TestBool = value;
                    }
                    break;
                }
            }
            #endif //!disable_macross_392fa105_7155_429d_8767_a951f1135d70
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_CollectionMacrossProperties_1657074542 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/skills/singlebullet/mc_phyevent_singlebullet.macross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_CollectionMacrossProperties_1657074542 = new EngineNS.Macross.TtMacrossStackTracer();
        public override System.Collections.Generic.List<EngineNS.Macross.PropertyData> CollectionMacrossProperties()
        {
            #if !disable_macross_392fa105_7155_429d_8767_a951f1135d70
            using(var guard_CollectionMacrossProperties = new EngineNS.Macross.TtMacrossStackGuard(mStack_CollectionMacrossProperties_1657074542,mFrame_CollectionMacrossProperties_1657074542))
            {
                System.Collections.Generic.List<EngineNS.Macross.PropertyData> returnValue = new System.Collections.Generic.List<EngineNS.Macross.PropertyData>();
                EngineNS.Macross.PropertyData data_TestBool = new EngineNS.Macross.PropertyData();
                data_TestBool.Name = "TestBool";
                data_TestBool.Type = EngineNS.Rtti.TtTypeDescGetter<System.Boolean>.TypeDesc;
                System.UInt64 data_TestBool_hash = Standart.Hash.xxHash.xxHash64.ComputeHash("TestBool");
                data_TestBool.NameHash = data_TestBool_hash;
                returnValue.Add(data_TestBool);
                return returnValue;
            }
            #elif !(!disable_macross_392fa105_7155_429d_8767_a951f1135d70)
            System.Collections.Generic.List<EngineNS.Macross.PropertyData> returnValue = new System.Collections.Generic.List<EngineNS.Macross.PropertyData>();
            return returnValue;
            #endif //!disable_macross_392fa105_7155_429d_8767_a951f1135d70
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_GetPropertyExpression_3963659444 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("survivor/skills/singlebullet/mc_phyevent_singlebullet.macross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_GetPropertyExpression_3963659444 = new EngineNS.Macross.TtMacrossStackTracer();
        public override EngineNS.Bricks.CodeBuilder.TtExpressionBase GetPropertyExpression(in EngineNS.Macross.PropertyData data)
        {
            #if !disable_macross_392fa105_7155_429d_8767_a951f1135d70
            using(var guard_GetPropertyExpression = new EngineNS.Macross.TtMacrossStackGuard(mStack_GetPropertyExpression_3963659444,mFrame_GetPropertyExpression_3963659444))
            {
                EngineNS.Bricks.CodeBuilder.TtExpressionBase returnValue = null;
                mFrame_GetPropertyExpression_3963659444.SetWatchVariable("data", data);
                switch (data.NameHash)
                {
                    case 6226757829267111881:
                    {
                        returnValue = new EngineNS.Bricks.CodeBuilder.TtPrimitiveExpression(TestBool);
                    }
                    break;
                }
                return returnValue;
            }
            #elif !(!disable_macross_392fa105_7155_429d_8767_a951f1135d70)
            EngineNS.Bricks.CodeBuilder.TtExpressionBase returnValue = null;
            return returnValue;
            #endif //!disable_macross_392fa105_7155_429d_8767_a951f1135d70
        }
    }
}
