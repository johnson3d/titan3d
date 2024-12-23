using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Design.Statement;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using EngineNS.Macross;
using EngineNS.Thread.Async;
using Microsoft.Build.Framework;
using NPOI.SS.Formula.Functions;
using Standart.Hash.xxHash;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder
{
    public class TtMacrossNodeCustomCodeGenAttribute : TtMacrossCustomCodeGenAttribute
    {
        public override void GenCustomCode(TtClassDeclaration classDec, TtCodeGeneratorBase codeGen)
        {
            GenericSetPropertValueMethod(classDec, codeGen);
            GenCollectionMacrossPropertiesMethod(classDec, codeGen);
            GenGetPropertyExpressionMethod(classDec, codeGen);
        }

        void GenericSetPropertValueMethod(TtClassDeclaration classDec, TtCodeGeneratorBase codeGen)
        {
            Dictionary<TtTypeReference, List<TtVariableDeclaration>> proDic = new Dictionary<TtTypeReference, List<TtVariableDeclaration>>();
            // Generic properties set functions
            for (int i = 0; i < classDec.Properties.Count; i++)
            {
                var pro = classDec.Properties[i];
                List<TtVariableDeclaration> val;
                if (proDic.TryGetValue(pro.VariableType, out val))
                {
                    val.Add(pro);
                }
                else
                {
                    val = new List<TtVariableDeclaration>();
                    val.Add(pro);
                    proDic.Add(pro.VariableType, val);
                }
            }
            classDec.TourSuperClassMeta((clsDec, clsMeta) =>
            {
                for(int i=0; i<clsMeta.Properties.Count; i++)
                {
                    var pro = clsMeta.Properties[i];
                    var proVarDec = new TtVariableDeclaration()
                    {
                        VariableType = new TtTypeReference(pro.FieldType),
                        VariableName = pro.PropertyName,
                    };
                    List<TtVariableDeclaration> val;
                    if(proDic.TryGetValue(proVarDec.VariableType, out val))
                    {
                        val.Add(proVarDec);
                    }
                    else
                    {
                        val = new List<TtVariableDeclaration>();
                        val.Add(proVarDec);
                        proDic.Add(proVarDec.VariableType, val);
                    }
                }
            });
            
            foreach (var v in proDic)
            {
                var superClsName = "EngineNS.Bricks.CodeBuilder.ISceneNodeMacross<";
                superClsName += codeGen.GetTypeString(v.Key);
                superClsName += ">";
                if (!classDec.SupperClassNames.Contains(superClsName))
                    classDec.SupperClassNames.Add(superClsName);

                var functionDec = new TtMethodDeclaration();
                functionDec.VisitMode = EVisisMode.None;
                var funcHos = new TtVariableReferenceExpression()
                {
                    VariableName = "EngineNS.Bricks.CodeBuilder.ISceneNodeMacross",
                };
                funcHos.GenericTypes.Add(v.Key);
                functionDec.Host = funcHos;
                functionDec.MethodName = "SetPropertyValue_Gen";
                functionDec.Arguments.Add(new TtMethodArgumentDeclaration()
                {
                    VariableName = "nameHash",
                    VariableType = new TtTypeReference(Rtti.TtTypeDescGetter<ulong>.TypeDesc),
                });
                functionDec.Arguments.Add(new TtMethodArgumentDeclaration()
                {
                    VariableName = "value",
                    VariableType = v.Key,
                    OperationType = EMethodArgumentAttribute.In,
                });
                if (classDec.Methods.Contains(functionDec))
                    classDec.Methods.Remove(functionDec);
                classDec.Methods.Add(functionDec);

                var swithDec = new TtSwitchStatement()
                {
                    Condition = new TtVariableReferenceExpression("nameHash")
                };
                functionDec.MethodBody.Sequence.Add(swithDec);

                foreach (var pro in v.Value)
                {
                    var proNameHash = Standart.Hash.xxHash.xxHash64.ComputeHash(pro.VariableName);
                    var caseExp = new TtPrimitiveExpression(proNameHash);
                    var caseBody = new TtAssignOperatorStatement()
                    {
                        From = new TtVariableReferenceExpression("value"),
                        To = new TtVariableReferenceExpression(pro.VariableName),
                    };
                    swithDec.Statements.Add(caseExp, caseBody);
                }
            }
        }
        void GenPropertyData(string propertyName, TtTypeReference propertyType, TtExecuteSequenceStatement functionBody, TtClassDeclaration classDec, TtCodeGeneratorBase codeGen)
        {
            var dataVarName = "data_" + propertyName;
            var propertyDataVarDec = new TtVariableDeclaration();
            propertyDataVarDec.VariableName = dataVarName;
            propertyDataVarDec.VariableType = new TtTypeReference(typeof(PropertyData));
            propertyDataVarDec.InitValue = new TtCreateObjectExpression(codeGen.GetTypeString(propertyDataVarDec.VariableType));
            functionBody.Sequence.Add(propertyDataVarDec);

            var nameAssign = new TtAssignOperatorStatement()
            {
                From = new TtPrimitiveExpression(propertyName),
                To = new TtVariableReferenceExpression()
                {
                    VariableName = "Name",
                    Host = new TtVariableReferenceExpression(dataVarName)
                }
            };
            functionBody.Sequence.Add(nameAssign);

            var typeAssign = new TtAssignOperatorStatement()
            {
                From = new TtTypeDescGetterExpression(propertyType),
                To = new TtVariableReferenceExpression()
                {
                    VariableName = "Type",
                    Host = new TtVariableReferenceExpression(dataVarName)
                }
            };
            functionBody.Sequence.Add(typeAssign);
            var hashCodeVarName = dataVarName + "_hash";
            var computeHashInvoke = new TtMethodInvokeStatement()
            {
                DeclarationReturnValue = true,
                ReturnValue = new TtVariableDeclaration()
                {
                    VariableName = hashCodeVarName,
                    VariableType = new TtTypeReference(typeof(ulong)),
                },
                MethodName = "ComputeHash",
                Host = new TtVariableReferenceExpression("xxHash64",
                    new TtVariableReferenceExpression("xxHash",
                    new TtVariableReferenceExpression("Hash",
                    new TtVariableReferenceExpression("Standart")))),
            };
            computeHashInvoke.Arguments.Add(new TtMethodInvokeArgumentExpression(new TtPrimitiveExpression(propertyName)));
            functionBody.Sequence.Add(computeHashInvoke);
            var nameHashAssign = new TtAssignOperatorStatement()
            {
                From = new TtVariableReferenceExpression(hashCodeVarName),
                To = new TtVariableReferenceExpression("NameHash", new TtVariableReferenceExpression(dataVarName)),
            };
            functionBody.Sequence.Add(nameHashAssign);

            var arrayAddMethod = new TtMethodInvokeStatement()
            {
                MethodName = "Add",
                Host = new TtVariableReferenceExpression("returnValue")
            };
            arrayAddMethod.Arguments.Add(new TtMethodInvokeArgumentExpression(new TtVariableReferenceExpression(dataVarName)));
            functionBody.Sequence.Add(arrayAddMethod);
        }
        void GenCollectionMacrossPropertiesMethod(TtClassDeclaration classDec, TtCodeGeneratorBase codeGen)
        {
            var functionDec = new TtMethodDeclaration();
            functionDec.VisitMode = EVisisMode.Public;
            functionDec.IsOverride = true;
            functionDec.MethodName = "CollectionMacrossProperties";
            functionDec.ReturnValue = new TtVariableDeclaration()
            {
                VariableType = new TtTypeReference(typeof(List<PropertyData>)),
                VariableName = "returnValue",
            };
            functionDec.ReturnValue.InitValue = new TtCreateObjectExpression(codeGen.GetTypeString(functionDec.ReturnValue.VariableType));
            if (classDec.Methods.Contains(functionDec))
                classDec.Methods.Remove(functionDec);
            classDec.Methods.Add(functionDec);

            var functionBody = new TtExecuteSequenceStatement();
            functionDec.MethodBody = functionBody;

            for (int i = 0; i < classDec.Properties.Count; i++)
            {
                GenPropertyData(classDec.Properties[i].VariableName, classDec.Properties[i].VariableType, functionBody, classDec, codeGen);
            }
            classDec.TourSuperClassMeta((clsDec, clsMeta) =>
            {
                for(int i=0; i<clsMeta.Properties.Count; i++)
                {
                    var prop = clsMeta.Properties[i];
                    GenPropertyData(prop.PropertyName, new TtTypeReference(prop.FieldType), functionBody, classDec, codeGen);
                }
            });
        }

        TtExpressionBase GenPropertyExpression(
            string proName, 
            Rtti.TtTypeDesc type, 
            TtClassDeclaration classDec, 
            TtCodeGeneratorBase codeGen,
            TtExecuteSequenceStatement statements)
        {
            if (TtPrimitiveExpression.IsValidType(type))
            {
                return new TtCreateObjectExpression(codeGen.GetTypeString(Rtti.TtTypeDescGetter<TtPrimitiveExpression>.TypeDesc), new TtVariableReferenceExpression(proName));
            }
            // 还没实现
            return null;
        }
        void GenGetPropertyExpressionMethod(TtClassDeclaration classDec, TtCodeGeneratorBase codeGen)
        {
            var retValName = "returnValue";
            var funcDec = new TtMethodDeclaration();
            funcDec.VisitMode = EVisisMode.Public;
            funcDec.IsOverride = true;
            funcDec.MethodName = "GetPropertyExpression";
            funcDec.ReturnValue = new TtVariableDeclaration()
            {
                VariableName = retValName,
                VariableType = new TtTypeReference(typeof(TtExpressionBase))
            };
            funcDec.ReturnValue.InitValue = new TtNullValueExpression();
            funcDec.Arguments.Add(new TtMethodArgumentDeclaration()
            {
                VariableName = "data",
                OperationType = EMethodArgumentAttribute.In,
                VariableType = new TtTypeReference(typeof(PropertyData))
            });
            if (classDec.Methods.Contains(funcDec))
                classDec.Methods.Remove(funcDec);
            classDec.Methods.Add(funcDec);

            var funcBody = new TtExecuteSequenceStatement();
            funcDec.MethodBody = funcBody;

            var switchSt = new TtSwitchStatement()
            {
                Condition = new TtVariableReferenceExpression("NameHash", new TtVariableReferenceExpression("data"))
            };
            funcBody.Sequence.Add(switchSt);
            for(int i=0; i<classDec.Properties.Count; i++)
            {
                var pro = classDec.Properties[i];
                var proNameHash = Standart.Hash.xxHash.xxHash64.ComputeHash(pro.VariableName);
                var caseHead = new TtPrimitiveExpression(proNameHash);
                var caseBody = new TtExecuteSequenceStatement();
                switchSt.Statements.Add(caseHead, caseBody);
                var assign = new TtAssignOperatorStatement()
                {
                    To = new TtVariableReferenceExpression(retValName),
                    From = GenPropertyExpression(pro.VariableName, pro.VariableType.TypeDesc, classDec, codeGen, caseBody)
                };
                caseBody.Sequence.Add(assign);                
            }
            classDec.TourSuperClassMeta((clsDec, clsMeta) =>
            {
                for (int i = 0; i < clsMeta.Properties.Count; i++)
                {
                    var pro = clsMeta.Properties[i];
                    var proNameHash = Standart.Hash.xxHash.xxHash64.ComputeHash(pro.PropertyName);
                    var caseHead = new TtPrimitiveExpression(proNameHash);
                    var caseBody = new TtExecuteSequenceStatement();
                    switchSt.Statements.Add(caseHead, caseBody);
                    var assign = new TtAssignOperatorStatement()
                    {
                        To = new TtVariableReferenceExpression(retValName),
                        From = GenPropertyExpression(pro.PropertyName, pro.FieldType, classDec, codeGen, caseBody)
                    };
                    caseBody.Sequence.Add(assign);
                }
            });
        }
    }

    public interface ISceneNodeMacross<T>
    {
        void SetPropertyValue_Gen(ulong propertyNameHash, in T value);
    }

    [Macross.TtMacross]
    [TtMacrossNodeCustomCodeGen]
    public partial class TtSceneNodeMacrossBase : ISceneNodeMacross<object>
    {
        public virtual void InitPros()
        {

        }

        void ISceneNodeMacross<object>.SetPropertyValue_Gen(ulong propertyNameHash, in object value)
        {

        }

        public virtual List<PropertyData> CollectionMacrossProperties()
        {
            return new List<PropertyData>();
        }

        public virtual TtExpressionBase GetPropertyExpression(in PropertyData data)
        {
            return null;
        }

        [Rtti.Meta]
        public bool TestBool { get; set; } = true;

        [Rtti.Meta]
        public virtual async System.Threading.Tasks.Task<bool> OnNodeInited(TtNode host)
        {
            return true;
        }
        [Rtti.Meta]
        public virtual void Tick(TtNode host)
        {

        }
        [Rtti.Meta]
        public virtual void DestroyNode(TtNode host)
        {
            
        }
    }

    [Bricks.CodeBuilder.ContextMenu("MacrossNode", "MacrossNode", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtMacrossSceneNode.TtMacrossSceneNodeData), DefaultNamePrefix = "Macross")]
    public class TtMacrossSceneNode : TtSceneActorNode, ISceneNodeMacrossInterface
    {
        public class TtMacrossSceneNodeData : TtNodeData
        {
            RName mMacrossName;
            [Rtti.Meta]
            [RName.PGMacrossRName<TtSceneNodeMacrossBase>(FilterExts = TtMacross.AssetExt)]
            public RName MacrossName 
            {
                get => mMacrossName;
                set
                {
                    mMacrossName = value;
                    var newRName = value;
                    MacrossGetter.Name = newRName;
                    //if (MacrossGetter.Name != newRName)
                    //{
                    //    MacrossGetter.Reset(TtEngine.Instance.MacrossModule);
                    //    MacrossGetter.Name = newRName;
                    //}
                }
            }

            TtMacrossGetter<TtSceneNodeMacrossBase> mMacrossGetter;
            public TtMacrossGetter<TtSceneNodeMacrossBase> MacrossGetter
            {
                get
                {
                    if (mMacrossGetter == null)
                        mMacrossGetter = TtMacrossGetter<TtSceneNodeMacrossBase>.NewInstance();
                    return mMacrossGetter;
                }
            }

            Guid mNodeId = Guid.NewGuid();
            [Rtti.Meta]
            public Guid NodeId 
            { 
                get => mNodeId; 
                set => mNodeId = value; 
            }
        }

        [RName.PGMacrossRName<TtSceneNodeMacrossBase>(FilterExts = TtMacross.AssetExt)]
        [Category("Option")]
        [Rtti.Meta]
        public RName MacrossName
        {
            get
            {
                if(NodeData is TtMacrossSceneNodeData data)
                {
                    return data.MacrossName;
                }
                return null;
            }
            set
            {
                if(NodeData is TtMacrossSceneNodeData data)
                {
                    data.MacrossName = value;
                }
            }
        }

        public TtMacrossGetter<TtSceneNodeMacrossBase> MacrossGetter 
        {
            get
            {
                if(NodeData is TtMacrossSceneNodeData data)
                    return data.MacrossGetter;
                return null;
            }
        }

        public object GetMacrossObject()
        {
            if (MacrossGetter == null)
                return null;
            return MacrossGetter.Get();
        }

        public override Guid NodeId 
        { 
            get
            {
                if (NodeData is TtMacrossSceneNodeData data)
                    return data.NodeId;
                return Guid.Empty;
            }
            set
            {
                if (NodeData is TtMacrossSceneNodeData data)
                    data.NodeId = value;
            }
        }

        public List<PropertyData> CollectionMacrossProperties()
        {
            var inner = MacrossGetter.Get();
            if (inner == null)
                return new List<PropertyData>();
            return inner.CollectionMacrossProperties();
        }

        public TtExpressionBase GetPropertyExpression(in PropertyData propData)
        {
            var inner = MacrossGetter.Get();
            if (inner == null)
                return null;
            return inner.GetPropertyExpression(in propData);
        }
        public void SetPropertyValue<T>(UInt64 nameHash, in T value)
        {
            var inner = MacrossGetter.Get();
            if (inner == null)
                return;
            ((ISceneNodeMacross<T>)inner).SetPropertyValue_Gen(nameHash, in value);
        }

        public override async Thread.Async.TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var ret = await base.InitializeNode(world, data, bvType, placementType);

            MacrossGetter?.Get()?.OnNodeInited(this);
            return ret;
        }
        public override bool OnTickLogic(TtWorld world, TtRenderPolicy policy)
        {
            base.OnTickLogic(world, policy);
            MacrossGetter?.Get()?.Tick(this);
            return true;
        }
        public override void Dispose()
        {
            MacrossGetter?.Get()?.DestroyNode(this);
            base.Dispose();
        }
    }
}
#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.Bricks.CodeBuilder
{
	partial class TtSceneNodeMacrossBase
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_OnNodeInited_2673848221 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.TtSceneNodeMacrossBase->System.Threading.Tasks.Task<bool> OnNodeInited(TtNode host)");
		public async System.Threading.Tasks.Task<bool> macross_OnNodeInited (string nodeName, TtNode host) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":host", host);
				}
			}
			var _return_value = await OnNodeInited(host);
			macross_break_OnNodeInited_2673848221.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Tick_2673848221 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.TtSceneNodeMacrossBase->void Tick(TtNode host)");
		public unsafe void macross_Tick (string nodeName, TtNode host) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":host", host);
				}
			}
			Tick(host);
			macross_break_Tick_2673848221.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_DestroyNode_2673848221 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.CodeBuilder.TtSceneNodeMacrossBase->void DestroyNode(TtNode host)");
		public unsafe void macross_DestroyNode (string nodeName, TtNode host) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":host", host);
				}
			}
			DestroyNode(host);
			macross_break_DestroyNode_2673848221.TryBreak();
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross