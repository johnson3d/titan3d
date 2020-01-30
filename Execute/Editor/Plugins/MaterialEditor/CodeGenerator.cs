using System.Collections.Generic;

namespace MaterialEditor
{
    public class CodeGenerator
    {
        public static bool GenerateCode(CodeGenerateSystem.Base.NodesContainer container, Controls.MaterialControl setValueCtrl, out System.IO.TextWriter codeFile, out System.IO.TextWriter varFile)
        {
            codeFile = new System.IO.StringWriter();
            varFile = new System.IO.StringWriter();

            // #include...
            //string includeString = "";
            //foreach (var node in container.CtrlNodeList)
            //{
            //    if (node is Controls.Operation.Function)
            //    {
            //        var funcCtrl = node as Controls.Operation.Function;
            //        if (!includeString.Contains(funcCtrl.Include))
            //            includeString += "#include \"" + funcCtrl.Include + "\"\r\n";
            //        //tw.WriteLine("#include \"" + funcCtrl.Include + "\"");
            //    }
            //}

            //codeFile.WriteLine(includeString);
            codeFile.WriteLine("");

            // value
            Dictionary<EngineNS.EShaderVarType, List<Controls.BaseNodeControl_ShaderVar>> varNodeDic = new Dictionary<EngineNS.EShaderVarType, List<Controls.BaseNodeControl_ShaderVar>>();
            foreach (var node in container.CtrlNodeList)
            {
                if(node is Controls.BaseNodeControl_ShaderVar)
                {
                    var ctrl = node as Controls.BaseNodeControl_ShaderVar;
                    if (ctrl.IsInConstantBuffer)
                    {
                        EngineNS.EShaderVarType varType = EngineNS.EShaderVarType.SVT_Unknown;
                        var valueTypeStr = ctrl.GCode_GetTypeString(null, null);
                        switch(valueTypeStr)
                        {
                            case "int":
                                varType = EngineNS.EShaderVarType.SVT_Int1;
                                break;
                            case "int2":
                                varType = EngineNS.EShaderVarType.SVT_Int2;
                                break;
                            case "int3":
                                varType = EngineNS.EShaderVarType.SVT_Int3;
                                break;
                            case "int4":
                                varType = EngineNS.EShaderVarType.SVT_Int4;
                                break;
                            case "float":
                            case "float1":
                                varType = EngineNS.EShaderVarType.SVT_Float1;
                                break;
                            case "float2":
                                varType = EngineNS.EShaderVarType.SVT_Float2;
                                break;
                            case "float3":
                                varType = EngineNS.EShaderVarType.SVT_Float3;
                                break;
                            case "float4":
                                varType = EngineNS.EShaderVarType.SVT_Float4;
                                break;
                        }
                        List<Controls.BaseNodeControl_ShaderVar> varList;
                        if(!varNodeDic.TryGetValue(varType, out varList))
                        {
                            varList = new List<Controls.BaseNodeControl_ShaderVar>();
                            varNodeDic[varType] = varList;
                        }
                        varList.Add(ctrl);
                        //varFile.Write(ctrl.GetValueDefine());
                    }
                    else
                        codeFile.Write(ctrl.GetValueDefine());
                }
            }
            codeFile.WriteLine("");

            // 字节对齐
            List<Controls.BaseNodeControl_ShaderVar> tempList;
            if(varNodeDic.TryGetValue(EngineNS.EShaderVarType.SVT_Float4, out tempList))
            {
                foreach (var ctrl in tempList)
                    varFile.Write(ctrl.GetValueDefine());
            }
            if(varNodeDic.TryGetValue(EngineNS.EShaderVarType.SVT_Int4, out tempList))
            {
                foreach (var ctrl in tempList)
                    varFile.Write(ctrl.GetValueDefine());
            }
            List<Controls.BaseNodeControl_ShaderVar> float1List;
            varNodeDic.TryGetValue(EngineNS.EShaderVarType.SVT_Float1, out float1List);
            if (float1List == null)
                float1List = new List<Controls.BaseNodeControl_ShaderVar>();
            List<Controls.BaseNodeControl_ShaderVar> int1List;
            varNodeDic.TryGetValue(EngineNS.EShaderVarType.SVT_Int1, out int1List);
            if (int1List == null)
                int1List = new List<Controls.BaseNodeControl_ShaderVar>();
            if (varNodeDic.TryGetValue(EngineNS.EShaderVarType.SVT_Float3, out tempList))
            {
                foreach(var ctrl in tempList)
                {
                    varFile.Write(ctrl.GetValueDefine());
                    if(float1List.Count > 0)
                    {
                        var floatVar = float1List[0];
                        varFile.Write(floatVar.GetValueDefine());
                        float1List.RemoveAt(0);
                    }
                    else if(int1List.Count > 0)
                    {
                        var intVar = int1List[0];
                        varFile.Write(intVar.GetValueDefine());
                        int1List.RemoveAt(0);
                    }
                    else
                    {
                        // 补位
                        var varName = ctrl.GCode_GetValueName(null, null);
                        varFile.Write("float " + varName + "_fill;\r\n");
                    }
                }
            }
            if(varNodeDic.TryGetValue(EngineNS.EShaderVarType.SVT_Int3, out tempList))
            {
                foreach(var ctrl in tempList)
                {
                    varFile.Write(ctrl.GetValueDefine());
                    if (float1List.Count > 0)
                    {
                        var floatVar = float1List[0];
                        varFile.Write(floatVar.GetValueDefine());
                        float1List.RemoveAt(0);
                    }
                    else if (int1List.Count > 0)
                    {
                        var intVar = int1List[0];
                        varFile.Write(intVar.GetValueDefine());
                        int1List.RemoveAt(0);
                    }
                    else
                    {
                        // 补位
                        var varName = ctrl.GCode_GetValueName(null, null);
                        varFile.Write("int1 " + varName + "_fill;\r\n");
                    }
                }
            }
            if(varNodeDic.TryGetValue(EngineNS.EShaderVarType.SVT_Float2, out tempList))
            {
                foreach(var ctrl in tempList)
                {
                    varFile.Write(ctrl.GetValueDefine());
                }
                if ((tempList.Count % 2) != 0)
                {
                    int float2fillCount = 2;
                    while(float2fillCount > 0)
                    {
                        if (float1List.Count > 0)
                        {
                            var floatVar = float1List[0];
                            varFile.Write(floatVar.GetValueDefine());
                            float1List.RemoveAt(0);
                            float2fillCount--;
                        }
                        else
                            break;
                    }
                    while(float2fillCount > 0)
                    {
                        if (int1List.Count > 0)
                        {
                            var intVar = int1List[0];
                            varFile.Write(intVar.GetValueDefine());
                            int1List.RemoveAt(0);
                            float2fillCount--;
                        }
                        else
                            break;
                    }
                    if(float2fillCount > 0)
                        varFile.Write($"float{float2fillCount} SV_{EngineNS.Editor.Assist.GetValuedGUIDString(System.Guid.NewGuid())}_Fill;\r\n");
                }
            }
            if (varNodeDic.TryGetValue(EngineNS.EShaderVarType.SVT_Int2, out tempList))
            {
                foreach (var ctrl in tempList)
                {
                    varFile.Write(ctrl.GetValueDefine());
                }
                if ((tempList.Count % 2) != 0)
                {
                    int int2fillCount = 2;
                    while (int2fillCount > 0)
                    {
                        if (float1List.Count > 0)
                        {
                            var floatVar = float1List[0];
                            varFile.Write(floatVar.GetValueDefine());
                            float1List.RemoveAt(0);
                            int2fillCount--;
                        }
                        else
                            break;
                    }
                    while (int2fillCount > 0)
                    {
                        if (int1List.Count > 0)
                        {
                            var intVar = int1List[0];
                            varFile.Write(intVar.GetValueDefine());
                            int1List.RemoveAt(0);
                            int2fillCount--;
                        }
                        else
                            break;
                    }
                    if (int2fillCount > 0)
                        varFile.Write($"int{int2fillCount} SV_{EngineNS.Editor.Assist.GetValuedGUIDString(System.Guid.NewGuid())}_Fill;\r\n");
                }
            }
            var fillCount = (float1List.Count + int1List.Count) % 4;
            var idxCount = float1List.Count + int1List.Count - fillCount;
            int idx = 0;
            if(idx < idxCount)
            {
                for (int i = float1List.Count - 1; i >= 0; i--)
                {
                    var ctrl = float1List[i];
                    varFile.Write(ctrl.GetValueDefine());
                    float1List.RemoveAt(i);
                    idx++;
                    if (idx >= idxCount)
                    {
                        break;
                    }
                }
            }
            if (idx < idxCount)
            {
                for (int i = int1List.Count - 1; i >= 0; i--)
                {
                    var ctrl = int1List[i];
                    varFile.Write(ctrl.GetValueDefine());
                    int1List.RemoveAt(i);
                    idx++;
                    if (idx >= idxCount)
                        break;
                }
            }
            if (fillCount > 0)
                varFile.Write($"float{4 - fillCount} SV_{EngineNS.Editor.Assist.GetValuedGUIDString(System.Guid.NewGuid())}_FinalFill;\r\n");
            foreach(var ctrl in float1List)
                varFile.Write(ctrl.GetValueDefine());
            foreach (var ctrl in int1List)
                varFile.Write(ctrl.GetValueDefine());

            var idStr = EngineNS.Editor.Assist.GetValuedGUIDString(setValueCtrl.Id);
            // VS
            codeFile.WriteLine($"void DoVSMaterial_{idStr}(in PS_INPUT input, inout MTL_OUTPUT mtl)");
            codeFile.WriteLine("{");
            string strSegment = "";
            string strDefinitionSegment = "";
            setValueCtrl.GCode_GenerateCode_VS(ref strDefinitionSegment, ref strSegment, 1, null);
            codeFile.WriteLine(strDefinitionSegment + "\r\n");
            codeFile.WriteLine(strSegment);
            codeFile.WriteLine("}");

            // PS
            codeFile.WriteLine($"void DoPSMaterial_{idStr}(in PS_INPUT input, inout MTL_OUTPUT mtl)");
            codeFile.WriteLine("{");
            strSegment = "";
            strDefinitionSegment = "";
            setValueCtrl.GCode_GenerateCode_PS(ref strDefinitionSegment, ref strSegment, 1, null);
            codeFile.WriteLine(strDefinitionSegment + "\r\n");
            codeFile.WriteLine(strSegment);
            codeFile.WriteLine("}");

            codeFile.WriteLine($"#define DO_VS_MATERIAL DoVSMaterial_{idStr}");
            codeFile.WriteLine($"#define DO_PS_MATERIAL DoPSMaterial_{idStr}");

            return true;
        }
    }
}
