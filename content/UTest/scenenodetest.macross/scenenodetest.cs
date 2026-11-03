namespace NS_utest
{
    [EngineNS.Macross.TtMacross]
    [EngineNS.Macross.TtMacrossSign(RName_Name = "utest/scenenodetest.macross", RName_Type = EngineNS.RName.ERNameType.Game)]
    public partial class scenenodetest : EngineNS.Bricks.CodeBuilder.TtSceneNodeMacrossBase,EngineNS.Bricks.CodeBuilder.ISceneNodeMacross<System.Int32>,EngineNS.Bricks.CodeBuilder.ISceneNodeMacross<System.Boolean>
    {
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Category("Macross")]
        [System.ComponentModel.DisplayName("Int_Value")]
        public System.Int32 Int_Value { get; set; } = 0;
        EngineNS.Macross.TtMacrossStackFrame mFrame_SetPropertyValue_Gen_192039980 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("utest/scenenodetest.macross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_SetPropertyValue_Gen_192039980 = new EngineNS.Macross.TtMacrossStackTracer();
        void EngineNS.Bricks.CodeBuilder.ISceneNodeMacross<System.Int32>.SetPropertyValue_Gen(System.UInt64 nameHash,in System.Int32 value)
        {
            #if !disable_macross_5c37e96d_20b6_4a86_915b_16f4fca32edb
            using(var guard_SetPropertyValue_Gen = new EngineNS.Macross.TtMacrossStackGuard(mStack_SetPropertyValue_Gen_192039980,mFrame_SetPropertyValue_Gen_192039980))
            {
                mFrame_SetPropertyValue_Gen_192039980.SetWatchVariable("nameHash", nameHash);
                mFrame_SetPropertyValue_Gen_192039980.SetWatchVariable("value", value);
                switch (nameHash)
                {
                    case 4660510811676728209:
                    {
                        Int_Value = value;
                    }
                    break;
                }
            }
            #endif //!disable_macross_5c37e96d_20b6_4a86_915b_16f4fca32edb
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_SetPropertyValue_Gen_5757565 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("utest/scenenodetest.macross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_SetPropertyValue_Gen_5757565 = new EngineNS.Macross.TtMacrossStackTracer();
        void EngineNS.Bricks.CodeBuilder.ISceneNodeMacross<System.Boolean>.SetPropertyValue_Gen(System.UInt64 nameHash,in System.Boolean value)
        {
            #if !disable_macross_5c37e96d_20b6_4a86_915b_16f4fca32edb
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
            #endif //!disable_macross_5c37e96d_20b6_4a86_915b_16f4fca32edb
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_CollectionMacrossProperties_1657074542 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("utest/scenenodetest.macross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_CollectionMacrossProperties_1657074542 = new EngineNS.Macross.TtMacrossStackTracer();
        public override System.Collections.Generic.List<EngineNS.Macross.PropertyData> CollectionMacrossProperties()
        {
            #if !disable_macross_5c37e96d_20b6_4a86_915b_16f4fca32edb
            using(var guard_CollectionMacrossProperties = new EngineNS.Macross.TtMacrossStackGuard(mStack_CollectionMacrossProperties_1657074542,mFrame_CollectionMacrossProperties_1657074542))
            {
                System.Collections.Generic.List<EngineNS.Macross.PropertyData> returnValue = new System.Collections.Generic.List<EngineNS.Macross.PropertyData>();
                EngineNS.Macross.PropertyData data_Int_Value = new EngineNS.Macross.PropertyData();
                data_Int_Value.Name = "Int_Value";
                data_Int_Value.Type = EngineNS.Rtti.TtTypeDescGetter<System.Int32>.TypeDesc;
                System.UInt64 data_Int_Value_hash = Standart.Hash.xxHash.xxHash64.ComputeHash("Int_Value");
                data_Int_Value.NameHash = data_Int_Value_hash;
                returnValue.Add(data_Int_Value);
                EngineNS.Macross.PropertyData data_TestBool = new EngineNS.Macross.PropertyData();
                data_TestBool.Name = "TestBool";
                data_TestBool.Type = EngineNS.Rtti.TtTypeDescGetter<System.Boolean>.TypeDesc;
                System.UInt64 data_TestBool_hash = Standart.Hash.xxHash.xxHash64.ComputeHash("TestBool");
                data_TestBool.NameHash = data_TestBool_hash;
                returnValue.Add(data_TestBool);
                return returnValue;
            }
            #elif !(!disable_macross_5c37e96d_20b6_4a86_915b_16f4fca32edb)
            System.Collections.Generic.List<EngineNS.Macross.PropertyData> returnValue = new System.Collections.Generic.List<EngineNS.Macross.PropertyData>();
            return returnValue;
            #endif //!disable_macross_5c37e96d_20b6_4a86_915b_16f4fca32edb
        }
        EngineNS.Macross.TtMacrossStackFrame mFrame_GetPropertyExpression_3963659444 = new EngineNS.Macross.TtMacrossStackFrame(EngineNS.RName.GetRName("utest/scenenodetest.macross", EngineNS.RName.ERNameType.Game));
        EngineNS.Macross.TtMacrossStackTracer mStack_GetPropertyExpression_3963659444 = new EngineNS.Macross.TtMacrossStackTracer();
        public override EngineNS.Bricks.CodeBuilder.TtExpressionBase GetPropertyExpression(in EngineNS.Macross.PropertyData data)
        {
            #if !disable_macross_5c37e96d_20b6_4a86_915b_16f4fca32edb
            using(var guard_GetPropertyExpression = new EngineNS.Macross.TtMacrossStackGuard(mStack_GetPropertyExpression_3963659444,mFrame_GetPropertyExpression_3963659444))
            {
                EngineNS.Bricks.CodeBuilder.TtExpressionBase returnValue = null;
                mFrame_GetPropertyExpression_3963659444.SetWatchVariable("data", data);
                switch (data.NameHash)
                {
                    case 4660510811676728209:
                    {
                        returnValue = new EngineNS.Bricks.CodeBuilder.TtPrimitiveExpression(Int_Value);
                    }
                    break;
                    case 6226757829267111881:
                    {
                        returnValue = new EngineNS.Bricks.CodeBuilder.TtPrimitiveExpression(TestBool);
                    }
                    break;
                }
                return returnValue;
            }
            #elif !(!disable_macross_5c37e96d_20b6_4a86_915b_16f4fca32edb)
            EngineNS.Bricks.CodeBuilder.TtExpressionBase returnValue = null;
            return returnValue;
            #endif //!disable_macross_5c37e96d_20b6_4a86_915b_16f4fca32edb
        }
    }
}
