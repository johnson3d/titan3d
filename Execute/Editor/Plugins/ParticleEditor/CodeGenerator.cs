using CodeDomNode.Particle;
using Macross;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParticleEditor
{
    public class CodeGeneratorHelper
    {
        public string EditorVisibleName;

        public CodeDomNode.Particle.ParticleSystemControl ParticleSystemNode = new CodeDomNode.Particle.ParticleSystemControl();

        public CodeDomNode.Particle.ParticleTextureCutControl TextureCutNode;
        public CodeDomNode.Particle.ParticlePointGravityControl PointGravityNode;
        public CodeDomNode.Particle.ParticleTrailControl ParticleTrailNode;
        public List<CodeDomNode.Particle.ParticleTriggerControl> ParticleTriggerNode = new List<ParticleTriggerControl>();
        public CodeDomNode.Particle.ParticleMaterialInstanceControl MaterialInstanceNode;

        public List<CodeDomNode.Particle.IParticleShape> ParticleShapeNodes = new List<CodeDomNode.Particle.IParticleShape>();
        public Dictionary<CodeDomNode.Particle.IParticleShape, CodeDomNode.Particle.ParticleStateControl> ParticleStateNodes = new Dictionary<CodeDomNode.Particle.IParticleShape, CodeDomNode.Particle.ParticleStateControl>();
        public Dictionary<CodeDomNode.Particle.IParticleShape, CodeDomNode.Particle.ParticleColorControl> ParticleColorNodes = new Dictionary<CodeDomNode.Particle.IParticleShape, CodeDomNode.Particle.ParticleColorControl>();
        public Dictionary<CodeDomNode.Particle.IParticleShape, CodeDomNode.Particle.ParticleScaleControl> ParticleScaleNodes = new Dictionary<CodeDomNode.Particle.IParticleShape, CodeDomNode.Particle.ParticleScaleControl>();

        public Dictionary<CodeDomNode.Particle.IParticleShape, CodeDomNode.Particle.ParticleVelocityByCenterControl> VelocityByCenterNodes = new Dictionary<CodeDomNode.Particle.IParticleShape, CodeDomNode.Particle.ParticleVelocityByCenterControl>();
        public Dictionary<CodeDomNode.Particle.IParticleShape, CodeDomNode.Particle.ParticleVelocityByTangentControl> VelocityByTangentNodes = new Dictionary<CodeDomNode.Particle.IParticleShape, CodeDomNode.Particle.ParticleVelocityByTangentControl>();

        public Dictionary<CodeDomNode.Particle.IParticleShape, List<CodeDomNode.Particle.ParticleVelocityControl>> ParticleVelocityNodes = new Dictionary<CodeDomNode.Particle.IParticleShape, List<CodeDomNode.Particle.ParticleVelocityControl>>();

        public Dictionary<CodeDomNode.Particle.IParticleShape, List<CodeDomNode.Particle.ParticleRotationControl>> ParticleRotationNodes = new Dictionary<CodeDomNode.Particle.IParticleShape, List<CodeDomNode.Particle.ParticleRotationControl>>();

        public Dictionary<CodeDomNode.Particle.IParticleShape, CodeDomNode.Particle.ParticleTransformControl> TransformNodes = new Dictionary<CodeDomNode.Particle.IParticleShape, CodeDomNode.Particle.ParticleTransformControl>();

        public List<CodeDomNode.Particle.ParticleComposeControl> ParticleComposes = new List<CodeDomNode.Particle.ParticleComposeControl>();
        public Dictionary<CodeDomNode.Particle.IParticleShape, int> ParticleShapeIndex = new Dictionary<CodeDomNode.Particle.IParticleShape, int>();

        public Dictionary<CodeDomNode.Particle.IParticleShape, CodeDomNode.Particle.ParticleAcceleratedControl> AcceleratedNodes = new Dictionary<CodeDomNode.Particle.IParticleShape, CodeDomNode.Particle.ParticleAcceleratedControl>();
        public Dictionary<CodeDomNode.Particle.IParticleShape, CodeDomNode.Particle.ParticleRandomDirectionControl> RandomDirectionNodes = new Dictionary<CodeDomNode.Particle.IParticleShape, CodeDomNode.Particle.ParticleRandomDirectionControl>();

    }

    public class CurrentValue
    {
        public CodeDomNode.Particle.ParticleColorControl CurrentParticleColorNode;
        public CodeDomNode.Particle.ParticleTrailControl CurrentParticleTrailNode;
        public CodeDomNode.Particle.ParticleTextureCutControl CurrentTextureCutNode;
        public CodeDomNode.Particle.ParticleScaleControl CurrentParticleScaleNode;
        public CodeDomNode.Particle.ParticleRandomDirectionControl CurrentRandomDirectionNode;
        public CodeDomNode.Particle.ParticleVelocityControl CurrentParticleVelocityNode;
        public CodeDomNode.Particle.ParticleRotationControl CurrentParticleRotationNode;
        public CodeDomNode.Particle.ParticleTransformControl CurrentParticleTransformNode;
        public CodeDomNode.Particle.ParticleVelocityByTangentControl CurrentParticleVelocityByTangentNode;
        public CodeDomNode.Particle.ParticleVelocityByCenterControl CurrentParticleVelocityByCenterNode;
        public CodeDomNode.Particle.ParticleAcceleratedControl CurrentAcceleratedNode;

        public CodeDomNode.Particle.ParticleMaterialInstanceControl CurrentMaterialInstanceNode;
    }

    class CodeGenerator : Macross.CodeGenerator
    {
        public EngineNS.Bricks.Particle.GParticleComponent ParticleComponent
        {
            get;
            set;
        }
        public ParticleSceneSetter ParticleSetter;

        public void CopySystemParamCode(CodeStatementCollection collection, string sysname)
        {
            if (ParticleSetter == null || ParticleSetter.ParticleSystem == null)
                return;

            var Properties = ParticleSetter.GetType().GetProperties();
            var type = typeof(EngineNS.Bricks.Particle.CGfxParticleSystem);
            foreach (var pro in Properties)
            {
                if (type.GetProperty(pro.Name) != null && pro.Name.Equals("IsShowBox") == false)
                {
                    collection.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(sysname + "." + pro.Name), new CodePrimitiveExpression(pro.GetValue(ParticleSetter))));
                }
               
            }
        }
        public override async Task GenerateMethods(IMacrossOperationContainer linkCtrl, CodeTypeDeclaration macrossClass, CodeGenerateSystem.Base.GenerateCodeContext_Class codeClassContext)
        {
            // 生成各函数代码
            Category graphCategory;
            //foreach (var categoryName in GenerateMethodsCategoryNames)
            //{
            //    if (linkCtrl.MacrossOpPanel.CategoryDic.TryGetValue(categoryName, out graphCategory))
            //    {
            //        for (int i = 0; i < graphCategory.Items.Count; i++)
            //        {
            //            var graph = graphCategory.Items[i];
            //            var nodesContainer = await linkCtrl.GetNodesContainer(graph, true);
            //            var test = nodesContainer.Name;
            //            await nodesContainer.GenerateCode(macrossClass, codeClassContext);
            //        }
            //    }
            //}

            if (CurrentClassNode != null && CurrentClassNode.LinkedNodesContainer != null)
            {

                //CurrentClassNode.LinkedNodesContainer..GenerateCode(macrossClass, codeClassContext);

                foreach (var ctrl in CurrentClassNode.LinkedNodesContainer.CtrlNodeList)
                {
                    ctrl.ReInitForGenericCode();
                }
                foreach (var ctrl in CurrentClassNode.LinkedNodesContainer.CtrlNodeList)
                {
                    string sss = ctrl.GetNodeDescriptionString();
                    string m_nodeName = ctrl.m_nodeName;
                    string NodeName = ctrl.NodeName;
                    Type[] types = ctrl.GetType().GetInterfaces();
                    if ((ctrl is CodeDomNode.MethodOverride) ||
                        (ctrl is CodeDomNode.MethodCustom))
                    {
                        await ctrl.GCode_CodeDom_GenerateCode(macrossClass, null, codeClassContext);
                    }
                }
            }
            else
            {
                //变量
            }
            if (linkCtrl.MacrossOpPanel.CategoryDic.TryGetValue(MacrossPanel.PropertyCategoryName, out graphCategory))
            {
                for (int i = 0; i < graphCategory.Items.Count; i++)
                {
                    var graph = graphCategory.Items[i];
                    var pro = graph.PropertyShowItem as Macross.CategoryItemProperty_Property;

                    var getNodesContainer = await linkCtrl.GetNodesContainer(pro.GetMethodNodesKey, true);
                    await getNodesContainer.GenerateCode(macrossClass, codeClassContext);

                    var setNodesContainer = await linkCtrl.GetNodesContainer(pro.SetMethodNodesKey, true);
                    await setNodesContainer.GenerateCode(macrossClass, codeClassContext);
                }
            }

            await GenerateInputActions(macrossClass, linkCtrl);
        }

        List<CodeGeneratorHelper> codeGeneratorHelpers;
        CurrentValue CurrentValue = new CurrentValue();
        public async Task GenerateParticleTemplateCode(Macross.ResourceInfos.MacrossResourceInfo info, IMacrossOperationContainer linkCtrl, CodeTypeDeclaration macrossClass, CodeNamespace nameSpace)
        {
            if (codeGeneratorHelpers == null)
                return;

            //Function CreateParticleSystems
            var CreateParticleSystems = new CodeMemberMethod();
            CreateParticleSystems.Name = "CreateParticleSystems";
            CreateParticleSystems.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            macrossClass.Members.Add(CreateParticleSystems);

            var param = new CodeParameterDeclarationExpression(typeof(EngineNS.Bricks.Particle.CGfxParticleSystem), "sys");
            CreateParticleSystems.Parameters.Add(param);

            CopySystemParamCode(CreateParticleSystems.Statements, "sys");
            CreateParticleSystems.Statements.Add(new CodeVariableDeclarationStatement(
            // Type of the variable to declare.
            typeof(List<EngineNS.Bricks.Particle.CGfxParticleSystem>),
            // Name of the variable to declare.
            "particlesystems",
            // Optional initExpression parameter initializes the variable.
            new CodeObjectCreateExpression("System.Collections.Generic.List<EngineNS.Bricks.Particle.CGfxParticleSystem>", new CodeExpression[] { })));

            for (int i = 0; i < codeGeneratorHelpers.Count; i++)
            {
                var codeGeneratorHelper = codeGeneratorHelpers[i];
                var createparticlesystemnode = codeGeneratorHelper.ParticleSystemNode.GetCreateObject();
                await createparticlesystemnode.GCode_CodeDom_GenerateCode(CreateParticleSystems, createparticlesystemnode.GCode_GetValueName(null, null));

                var systecomname = codeGeneratorHelper.ParticleSystemNode.Id.ToString().Replace("-", "_");
                CreateParticleSystems.Statements.Add(new CodeAssignStatement(
              // Name of the variable to declare.
              new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".Name"),
              new CodePrimitiveExpression(systecomname)));

                CreateParticleSystems.Statements.Add(new CodeAssignStatement(
             // Name of the variable to declare.
             new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".EditorVisibleName"),
             new CodePrimitiveExpression(codeGeneratorHelper.EditorVisibleName)));

                //处理矩阵信息
                {
                    if (ParticleComponent != null)
                    {
                        EngineNS.GamePlay.Component.GComponent component = ParticleComponent.FindComponentBySpecialName(systecomname);
                        if (component != null)
                        {
                            var ParticleSubSystemComponent = component as EngineNS.Bricks.Particle.GParticleSubSystemComponent;
                            if (ParticleSubSystemComponent != null && ParticleComponent.Placement != null)
                            {
                                EngineNS.Vector3 pos;
                                EngineNS.Vector3 scale;
                                EngineNS.Quaternion rot;
                                var Placement = ParticleSubSystemComponent.Placement as EngineNS.Bricks.Particle.GParticlePlacementComponent;
                                bool IsIgnore = Placement.IsIgnore;
                                Placement.IsIgnore = false;
                                Placement.WorldMatrix.Decompose(out scale, out rot, out pos);
                                Placement.IsIgnore = IsIgnore;
                                if (float.IsNaN(rot.X) || float.IsNaN(rot.Y) || float.IsNaN(rot.Z) || float.IsNaN(rot.W))
                                {
                                    rot = EngineNS.Quaternion.Identity;
                                }

                                string callstaticfunction = "EngineNS.Matrix.Transformation(new EngineNS.Vector3(" + scale.X + "f," + scale.Y + "f," + scale.Z + "f) ," +
                                    " new EngineNS.Quaternion(" + rot.X + "f," + rot.Y + "f," + rot.Z + "f," + rot.W + "f), new EngineNS.Vector3(" + pos.X + "f," + pos.Y + "f," + pos.Z + "f))";
                                CodeSnippetExpression expression = new CodeSnippetExpression(callstaticfunction);
                                CreateParticleSystems.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".Matrix"), expression));

                            }
                        }
                    }
                }
                CreateParticleSystems.Statements.Add(new CodeAssignStatement(
              // Name of the variable to declare.
              new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".MacrossNode"),
               // Optional initExpression parameter initializes the variable.
               new CodeObjectCreateExpression(codeGeneratorHelper.ParticleSystemNode.GetClassName(), new CodeExpression[] { })));

                CreateParticleSystems.Statements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression("particlesystems"), "Add", new CodeExpression[] { new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null)) }));
                //处理shapes
                {
                    for (int j = 0; j < codeGeneratorHelper.ParticleShapeNodes.Count; j++)
                    {

                        var ParticleShapeNode = codeGeneratorHelper.ParticleShapeNodes[j] as CodeGenerateSystem.Base.BaseNodeControl;
                        var iparticleshape = codeGeneratorHelper.ParticleShapeNodes[j];

                        var createobjnode = iparticleshape.GetCreateObject();
                        await createobjnode.GCode_CodeDom_GenerateCode(CreateParticleSystems, createobjnode.GCode_GetValueName(null, null));

                        //CreateParticleSystems.Statements.Add(new CodeVariableDeclarationStatement(
                        // // Type of the variable to declare.
                        // typeof(EngineNS.Bricks.Particle.CGfxParticleSystem),
                        // // Name of the variable to declare.
                        // createobjnode.GCode_GetValueName(null, null) + ".MacrossNode",
                        // // Optional initExpression parameter initializes the variable.
                        // new CodeObjectCreateExpression(ParticleShapeNode.NodeName + "_" + ParticleShapeNode.Id.ToString().Replace("-", "_"), new CodeExpression[] { })));

                        CreateParticleSystems.Statements.Add(new CodeAssignStatement(
                         // Name of the variable to declare.
                         new CodeVariableReferenceExpression(createobjnode.GCode_GetValueName(null, null) + ".Name"),
                         new CodePrimitiveExpression(ParticleShapeNode.Id.ToString().Replace("-", "_"))));

                        CreateParticleSystems.Statements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".TempParticleSubStateNodes"), "Add", new CodeExpression[] { new CodeObjectCreateExpression(iparticleshape.GetClassName(), new CodeExpression[] { }) }));

                        CreateParticleSystems.Statements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".TempSubStates"), "Add", new CodeExpression[] { new CodeVariableReferenceExpression(createobjnode.GCode_GetValueName(null, null)) }));

                        codeGeneratorHelper.ParticleShapeIndex.Add(codeGeneratorHelper.ParticleShapeNodes[j], j);

                        //处理 particle color
                        {
                            CodeDomNode.Particle.ParticleColorControl particlecolornode;
                            if (codeGeneratorHelper.ParticleColorNodes.TryGetValue(iparticleshape, out particlecolornode))
                            {
                                CreateParticleSystems.Statements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".TempParticleColorNodes"), "Add", new CodeExpression[] { new CodePrimitiveExpression(j), new CodeObjectCreateExpression(particlecolornode.GetClassName()) }));
                            }
                            
                        }

                        //处理 particle scale
                        {
                            CodeDomNode.Particle.ParticleScaleControl node;
                            if (codeGeneratorHelper.ParticleScaleNodes.TryGetValue(iparticleshape, out node))
                            {
                                CreateParticleSystems.Statements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".TempParticleScaleNodes"), "Add", new CodeExpression[] { new CodePrimitiveExpression(j), new CodeObjectCreateExpression(node.GetClassName()) }));
                            }

                        }

                        //处理 加速结点
                        {
                            CodeDomNode.Particle.ParticleAcceleratedControl node;
                            if (codeGeneratorHelper.AcceleratedNodes.TryGetValue(iparticleshape, out node))
                            {
                                CreateParticleSystems.Statements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".TempAcceleratedNodes"), "Add", new CodeExpression[] { new CodePrimitiveExpression(j), new CodeObjectCreateExpression(node.GetClassName()) }));
                            }

                        }

                        //处理 随机方向结点
                        {
                            CodeDomNode.Particle.ParticleRandomDirectionControl node;
                            if (codeGeneratorHelper.RandomDirectionNodes.TryGetValue(iparticleshape, out node))
                            {
                                var cname = node.GetClassName();
                                var variablename = "RandomDirection_" + cname;
                                CreateParticleSystems.Statements.Add(new CodeVariableDeclarationStatement(typeof(EngineNS.Bricks.Particle.RandomDirectionNode), variablename));
                                CreateParticleSystems.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(variablename), new CodeObjectCreateExpression(cname)));

                                var createnode = node.GetCreateObject();
                                await createnode.GCode_CodeDom_GenerateCode(CreateParticleSystems, createnode.GCode_GetValueName(null, null));

                                CreateParticleSystems.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(variablename + "._Data"), new CodeVariableReferenceExpression(createnode.GCode_GetValueName(null, null))));
                                //CreateParticleSystems.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".TempRandomDirectionNodes"), new CodeVariableReferenceExpression(variablename)));

                                CreateParticleSystems.Statements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".TempRandomDirectionNodes"), "Add", new CodeExpression[] { new CodePrimitiveExpression(j), new CodeVariableReferenceExpression(variablename) }));
                            }

                        }

                        //处理 particle ByCenter
                        {
                            CodeDomNode.Particle.ParticleVelocityByCenterControl node;
                            if (codeGeneratorHelper.VelocityByCenterNodes.TryGetValue(iparticleshape, out node))
                            {
                                CreateParticleSystems.Statements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".TempVelocityByCenterNodes"), "Add", new CodeExpression[] { new CodePrimitiveExpression(j), new CodeObjectCreateExpression(node.GetClassName()) }));
                            }

                        }

                        //处理 particle ByTangent

                        {
                            CodeDomNode.Particle.ParticleVelocityByTangentControl node;
                            if (codeGeneratorHelper.VelocityByTangentNodes.TryGetValue(iparticleshape, out node))
                            {
                                CreateParticleSystems.Statements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".TempVelocityByTangentNodes"), "Add", new CodeExpression[] { new CodePrimitiveExpression(j), new CodeObjectCreateExpression(node.GetClassName()) }));
                            }

                        }

                        //处理 particle velocity
                        {
                            List<CodeDomNode.Particle.ParticleVelocityControl> nodes;
                            {
                                if (codeGeneratorHelper.ParticleVelocityNodes.TryGetValue(iparticleshape, out nodes))
                                {
                                    string vname = iparticleshape.GetClassName()+ "_VelocityNodes";
                                    CreateParticleSystems.Statements.Add(new CodeVariableDeclarationStatement(
                                         // Type of the variable to declare.
                                         typeof(List<EngineNS.Bricks.Particle.ParticleVelocityNode>),
                                         // Name of the variable to declare.
                                         vname,
                                        // Optional initExpression parameter initializes the variable.
                                        new CodeObjectCreateExpression("System.Collections.Generic.List<EngineNS.Bricks.Particle.ParticleVelocityNode>")));
                                    for (int vi = 0; vi < nodes.Count; vi++)
                                    {
                                        var node = nodes[vi];
                                        CreateParticleSystems.Statements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression(vname), "Add", new CodeExpression[] {new CodeObjectCreateExpression(node.GetClassName()) }));


                                    }

                                    CreateParticleSystems.Statements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".TempParticleVelocityNodes"), "Add", new CodeExpression[] { new CodePrimitiveExpression(j), new CodeVariableReferenceExpression(vname) }));
                                }
                            }
                        }

                        //处理 particle rotation
                        {
                            List<CodeDomNode.Particle.ParticleRotationControl> nodes;
                            {
                                if (codeGeneratorHelper.ParticleRotationNodes.TryGetValue(iparticleshape, out nodes))
                                {
                                    string vname = iparticleshape.GetClassName() + "_RotationNodes";
                                    CreateParticleSystems.Statements.Add(new CodeVariableDeclarationStatement(
                                         // Type of the variable to declare.
                                         typeof(List<EngineNS.Bricks.Particle.RotationNode>),
                                         // Name of the variable to declare.
                                         vname,
                                        // Optional sinitExpression parameter initializes the variable.
                                        new CodeObjectCreateExpression("System.Collections.Generic.List<EngineNS.Bricks.Particle.RotationNode>")));
                                    for (int vi = 0; vi < nodes.Count; vi++)
                                    {
                                        var node = nodes[vi];
                                        CreateParticleSystems.Statements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression(vname), "Add", new CodeExpression[] { new CodeObjectCreateExpression(node.GetClassName()) }));


                                    }

                                    CreateParticleSystems.Statements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".TempParticleRotationNodes"), "Add", new CodeExpression[] { new CodePrimitiveExpression(j), new CodeVariableReferenceExpression(vname) }));
                                }
                            }
                        }

                        //处理 particle transform
                        {
                            CodeDomNode.Particle.ParticleTransformControl node;
                            {
                                if (codeGeneratorHelper.TransformNodes.TryGetValue(iparticleshape, out node))
                                {
                                    CreateParticleSystems.Statements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".TempParticleTransformNodes"), "Add", new CodeExpression[] { new CodePrimitiveExpression(j), new CodeObjectCreateExpression(node.GetClassName()) }));
                                }
                            }
                        }

                        //处理 ParticleState
                        {
                            CodeDomNode.Particle.ParticleStateControl particlestatenode;
                            if (codeGeneratorHelper.ParticleStateNodes.TryGetValue(iparticleshape, out particlestatenode))
                            {
                                var statename = "ParticleStateNode_" + particlestatenode.GetClassName();
                                CreateParticleSystems.Statements.Add(new CodeVariableDeclarationStatement(
                               // Type of the variable to declare.
                               typeof(EngineNS.Bricks.Particle.ParticleStateNode),
                               // Name of the variable to declare.
                               statename,
                              // Optional initExpression parameter initializes the variable.
                              new CodeObjectCreateExpression(particlestatenode.GetClassName())));

                                var createobjnode_particlestate = particlestatenode.GetCreateObject();
                                await createobjnode_particlestate.GCode_CodeDom_GenerateCode(CreateParticleSystems, createobjnode_particlestate.GCode_GetValueName(null, null));

                                CreateParticleSystems.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(statename + ".Data"), new CodeVariableReferenceExpression(createobjnode_particlestate.GCode_GetValueName(null, null))));



                                CreateParticleSystems.Statements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".TempParticleStateNodes"), "Add", new CodeExpression[] { new CodePrimitiveExpression(j), new CodeVariableReferenceExpression(statename) }));
                            }

                        }
                    }
                }

                //处理particle trigger
                {
                    for (int j = 0; j < codeGeneratorHelper.ParticleTriggerNode.Count; j++)
                    {

                        var ParticleTriggerNode = codeGeneratorHelper.ParticleTriggerNode[j] as IParticleNode;

                        var createobjnode = ParticleTriggerNode.GetCreateObject();
                        await createobjnode.GCode_CodeDom_GenerateCode(CreateParticleSystems, createobjnode.GCode_GetValueName(null, null));

                        var cname = ParticleTriggerNode.GetClassName();
                        var varname = "Trigger_" + cname;
                        CreateParticleSystems.Statements.Add(new CodeVariableDeclarationStatement(
                       // Type of the variable to declare.
                       typeof(EngineNS.Bricks.Particle.TriggerNode),
                       // Name of the variable to declare.
                       varname,
                      // Optional initExpression parameter initializes the variable.
                      new CodeObjectCreateExpression(cname)));

                        CreateParticleSystems.Statements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".TriggerNodes"), "Add", new CodeExpression[] { new CodeVariableReferenceExpression(varname) }));
                       

                        CreateParticleSystems.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(varname + "._Data"), new CodeVariableReferenceExpression(createobjnode.GCode_GetValueName(null, null))));
                        
                    }
                }

                //处理particle trail..
                {
                    if (codeGeneratorHelper.ParticleTrailNode != null)
                    {
                        var cname = codeGeneratorHelper.ParticleTrailNode.GetClassName();
                        CreateParticleSystems.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".TrailNode"), new CodeObjectCreateExpression(cname)));

                        var createparticletrailode = codeGeneratorHelper.ParticleTrailNode.GetCreateObject();
                        await createparticletrailode.GCode_CodeDom_GenerateCode(CreateParticleSystems, createparticletrailode.GCode_GetValueName(null, null));
                        
                        CreateParticleSystems.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".TrailNode.TrailDataControlData"), new CodeVariableReferenceExpression(createparticletrailode.GCode_GetValueName(null, null))));

                        //CreateParticleSystems.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".TrailNode"), new CodeVariableReferenceExpression(cname)));
                    }
                }

                //处理particle texture..
                {
                    if (codeGeneratorHelper.TextureCutNode != null)
                    {
                        var cname = codeGeneratorHelper.TextureCutNode.GetClassName();
                        CreateParticleSystems.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".TextureCutNode"), new CodeObjectCreateExpression(cname)));

                        var createnode = codeGeneratorHelper.TextureCutNode.GetCreateObject();
                        await createnode.GCode_CodeDom_GenerateCode(CreateParticleSystems, createnode.GCode_GetValueName(null, null));

                        CreateParticleSystems.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".TextureCutNode._Data"), new CodeVariableReferenceExpression(createnode.GCode_GetValueName(null, null))));

                        //CreateParticleSystems.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".TrailNode"), new CodeVariableReferenceExpression(cname)));
                    }
                }

                //处理particle point gravity..
                {
                    if (codeGeneratorHelper.PointGravityNode != null)
                    {
                        var cname = codeGeneratorHelper.PointGravityNode.GetClassName();
                        CreateParticleSystems.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".PointGravityNode"), new CodeObjectCreateExpression(cname)));

                        var createnode = codeGeneratorHelper.PointGravityNode.GetCreateObject();
                        await createnode.GCode_CodeDom_GenerateCode(CreateParticleSystems, createnode.GCode_GetValueName(null, null));

                        CreateParticleSystems.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".PointGravityNode._Data"), new CodeVariableReferenceExpression(createnode.GCode_GetValueName(null, null))));

                        //CreateParticleSystems.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".TrailNode"), new CodeVariableReferenceExpression(cname)));
                    }
                }

                //处理particle material..
                {
                    if (codeGeneratorHelper.MaterialInstanceNode != null)
                    {
                        var cname = codeGeneratorHelper.MaterialInstanceNode.GetClassName();
                        CreateParticleSystems.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".MaterialInstanceNode"), new CodeObjectCreateExpression(cname)));
                    }
                }

                //处理particle compose..
                {
                    if (codeGeneratorHelper.ParticleComposes.Count != 0)
                    {
                        for (int ci = 0; ci < codeGeneratorHelper.ParticleComposes.Count; ci++)
                        {
                            var cname = codeGeneratorHelper.ParticleComposes[ci].GetClassName();
                            var composename = "ParticleComposeNode_" + cname;
                            CreateParticleSystems.Statements.Add(new CodeVariableDeclarationStatement(
                           // Type of the variable to declare.
                           typeof(EngineNS.Bricks.Particle.ParticleComposeNode),
                           // Name of the variable to declare.
                           composename,
                          // Optional initExpression parameter initializes the variable.
                          new CodeObjectCreateExpression(cname)));

                            var createparticlecomposenode = codeGeneratorHelper.ParticleComposes[ci].GetCreateObject();
                            await createparticlecomposenode.GCode_CodeDom_GenerateCode(CreateParticleSystems, createparticlecomposenode.GCode_GetValueName(null, null));

                            CreateParticleSystems.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(composename + "._Data"), new CodeVariableReferenceExpression(createparticlecomposenode.GCode_GetValueName(null, null))));

                            CreateParticleSystems.Statements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression(createparticlesystemnode.GCode_GetValueName(null, null) + ".ComposeNodes"), "Add", new CodeExpression[] { new CodeVariableReferenceExpression(composename) }));

                            await GenerateParticleComposeCode(composename, codeGeneratorHelper.ParticleComposes[ci], CreateParticleSystems, createparticlesystemnode, codeGeneratorHelper.ParticleShapeIndex);
                        }
                    }
                }
            }
           
            //赋值操作
            CreateParticleSystems.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("sys.SubParticleSystems"), new CodeVariableReferenceExpression("particlesystems")));
        }

        //Composenodes, 
        public async Task GenerateParticleComposeCode(string objname, CodeDomNode.Particle.ParticleComposeControl composecontrol, CodeMemberMethod CreateParticleSystems, CodeDomNode.CreateObject createparticlesystemnode, Dictionary<CodeDomNode.Particle.IParticleShape, int> ParticleShapeIndex)
        {
            //Left
            {
                if (composecontrol.ParticleComposeLeft != null)
                {
                    var subcompose = composecontrol.ParticleComposeLeft;

                    var cname = subcompose.GetClassName();
                    var composename = "ParticleComposeNode_" + cname;
                    CreateParticleSystems.Statements.Add(new CodeVariableDeclarationStatement(
                   // Type of the variable to declare.
                   typeof(EngineNS.Bricks.Particle.ParticleComposeNode),
                   // Name of the variable to declare.
                   composename,
                  // Optional initExpression parameter initializes the variable.
                  new CodeObjectCreateExpression(cname)));

                    var createparticlecomposenode = subcompose.GetCreateObject();
                    await createparticlecomposenode.GCode_CodeDom_GenerateCode(CreateParticleSystems, createparticlecomposenode.GCode_GetValueName(null, null));

                    CreateParticleSystems.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(composename + "._Data"), new CodeVariableReferenceExpression(createparticlecomposenode.GCode_GetValueName(null, null))));

                    CreateParticleSystems.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(objname + ".ComposeNodeLeft"), new CodeVariableReferenceExpression(composename)));

                    await GenerateParticleComposeCode(composename, subcompose, CreateParticleSystems, createparticlesystemnode, ParticleShapeIndex);
                }
                else
                {
                    var ps = composecontrol.ParticleShapeLeft;
                    int index;
                    if (ParticleShapeIndex.TryGetValue(ps, out index))
                    {
                        CreateParticleSystems.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(objname + ".LeftSubStateIndex"), new CodePrimitiveExpression(index)));
                    }
                    
                }
            }

            {
                if (composecontrol.ParticleComposeRight != null)
                {
                    var subcompose = composecontrol.ParticleComposeRight;

                    var cname = subcompose.GetClassName();
                    var composename = "ParticleComposeNode_" + cname;
                    CreateParticleSystems.Statements.Add(new CodeVariableDeclarationStatement(
                   // Type of the variable to declare.
                   typeof(EngineNS.Bricks.Particle.ParticleComposeNode),
                   // Name of the variable to declare.
                   composename,
                  // Optional initExpression parameter initializes the variable.
                  new CodeObjectCreateExpression(cname)));

                    var createparticlecomposenode = subcompose.GetCreateObject();
                    await createparticlecomposenode.GCode_CodeDom_GenerateCode(CreateParticleSystems, createparticlecomposenode.GCode_GetValueName(null, null));

                    CreateParticleSystems.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(composename + "._Data"), new CodeVariableReferenceExpression(createparticlecomposenode.GCode_GetValueName(null, null))));

                    CreateParticleSystems.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(objname + ".ComposeNodeRight"), new CodeVariableReferenceExpression(composename)));

                    await GenerateParticleComposeCode(composename, subcompose, CreateParticleSystems, createparticlesystemnode, ParticleShapeIndex);
                }
                else
                {
                    var ps = composecontrol.ParticleShapeRight;
                    int index;
                    if (ParticleShapeIndex.TryGetValue(ps, out index))
                    {
                        CreateParticleSystems.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(objname + ".RightSubStateIndex"), new CodePrimitiveExpression(index)));
                    }

                }
            }
        }

        public async Task GenerateParticleColorCode(Macross.ResourceInfos.MacrossResourceInfo info, IMacrossOperationContainer linkCtrl, CodeTypeDeclaration macrossClass, CodeNamespace nameSpace)
        {
            if (codeGeneratorHelpers == null || CurrentValue.CurrentParticleColorNode == null)
                return;

            await CurrentValue.CurrentParticleColorNode.GCode_CodeDom_GenerateCode(macrossClass, null, null, null);
        }

        public async Task GenerateParticleScaleCode(Macross.ResourceInfos.MacrossResourceInfo info, IMacrossOperationContainer linkCtrl, CodeTypeDeclaration macrossClass, CodeNamespace nameSpace)
        {
            if (codeGeneratorHelpers == null || CurrentValue.CurrentParticleScaleNode == null)
                return;

            await CurrentValue.CurrentParticleScaleNode.GCode_CodeDom_GenerateCode(macrossClass, null, null, null);
        }
        
        public async Task GenerateParticleAcceleratedCode(Macross.ResourceInfos.MacrossResourceInfo info, IMacrossOperationContainer linkCtrl, CodeTypeDeclaration macrossClass, CodeNamespace nameSpace)
        {
            if (codeGeneratorHelpers == null || CurrentValue.CurrentAcceleratedNode == null)
                return;

            await CurrentValue.CurrentAcceleratedNode.GCode_CodeDom_GenerateCode(macrossClass, null, null, null);
        }

        public async Task GenerateVelocityByTangentCode(Macross.ResourceInfos.MacrossResourceInfo info, IMacrossOperationContainer linkCtrl, CodeTypeDeclaration macrossClass, CodeNamespace nameSpace)
        {
            if (codeGeneratorHelpers == null || CurrentValue.CurrentParticleVelocityByTangentNode == null)
                return;

            await CurrentValue.CurrentParticleVelocityByTangentNode.GCode_CodeDom_GenerateCode(macrossClass, null, null, null);
        }

        public async Task GenerateVelocityByCenterCode(Macross.ResourceInfos.MacrossResourceInfo info, IMacrossOperationContainer linkCtrl, CodeTypeDeclaration macrossClass, CodeNamespace nameSpace)
        {
            if (codeGeneratorHelpers == null || CurrentValue.CurrentParticleVelocityByCenterNode == null)
                return;

            await CurrentValue.CurrentParticleVelocityByCenterNode.GCode_CodeDom_GenerateCode(macrossClass, null, null, null);
        }

        public async Task GenerateParticleVelocityCode(Macross.ResourceInfos.MacrossResourceInfo info, IMacrossOperationContainer linkCtrl, CodeTypeDeclaration macrossClass, CodeNamespace nameSpace)
        {
            if (codeGeneratorHelpers == null || CurrentValue.CurrentParticleVelocityNode == null)
                return;

            await CurrentValue.CurrentParticleVelocityNode.GCode_CodeDom_GenerateCode(macrossClass, null, null, null);
        }

        public async Task GenerateParticleRotationCode(Macross.ResourceInfos.MacrossResourceInfo info, IMacrossOperationContainer linkCtrl, CodeTypeDeclaration macrossClass, CodeNamespace nameSpace)
        {
            if (codeGeneratorHelpers == null || CurrentValue.CurrentParticleRotationNode == null)
                return;

            await CurrentValue.CurrentParticleRotationNode.GCode_CodeDom_GenerateCode(macrossClass, null, null, null);
        }
        public async Task GenerateParticleTransformCode(Macross.ResourceInfos.MacrossResourceInfo info, IMacrossOperationContainer linkCtrl, CodeTypeDeclaration macrossClass, CodeNamespace nameSpace)
        {
            if (codeGeneratorHelpers == null || CurrentValue.CurrentParticleTransformNode == null)
                return;

            await CurrentValue.CurrentParticleTransformNode.GCode_CodeDom_GenerateCode(macrossClass, null, null, null);
        }

        public async Task GenerateParticleMaterialCode(Macross.ResourceInfos.MacrossResourceInfo info, IMacrossOperationContainer linkCtrl, CodeTypeDeclaration macrossClass, CodeNamespace nameSpace)
        {
            if (codeGeneratorHelpers == null || CurrentValue.CurrentMaterialInstanceNode == null)
                return;

            await CurrentValue.CurrentMaterialInstanceNode.GCode_CodeDom_GenerateCode(macrossClass, null, null, null);
        }


        CodeGenerateSystem.Base.BaseNodeControl CurrentClassNode;
        public override async Task<string> GenerateCode(Macross.ResourceInfos.MacrossResourceInfo info, IMacrossOperationContainer linkCtrl, Func<Macross.ResourceInfos.MacrossResourceInfo, IMacrossOperationContainer, CodeTypeDeclaration, CodeNamespace, Task> generateAction)
        {
            //ParticleSystemNodes.Clear();
            //ParticleShapeNodes.Clear();
            codeGeneratorHelpers = new List<CodeGeneratorHelper>();
            var className = Program.GetClassName(info, linkCtrl.CSType);
            var macrosslinkcontrol = linkCtrl as ParticleMacrossLinkControl;
            //macrosslinkcontrol.CopyLast();
            try
            {
                CodeDomNode.BreakPoint.ClearDebugValueFieldDic();
                //var codeNameSpace = "Macross.Generated";
                var codeNameSpace = Program.GetClassNamespace(info, linkCtrl.CSType);

                var nameSpace = new CodeNamespace(codeNameSpace);

                var nameSpaceContext = new CodeGenerateSystem.Base.GenerateCodeContext_Namespace(codeNameSpace, nameSpace);
                nameSpaceContext.NameSpaceID = info.Id;
                nameSpaceContext.Sign = className;
               
                Category category;
                string codestr = "";
                if (linkCtrl.MacrossOpPanel.CategoryDic.TryGetValue(ParticlePanel.ParticleCategoryName, out category))
                {
                    for (int i = 0; i < category.Items.Count; i++)
                    {
                        var graph = category.Items[i];
                        var nodesContainer = await linkCtrl.GetNodesContainer(graph, true);
                        foreach (var ctrl in nodesContainer.NodesControl.CtrlNodeList)
                        {
                            ctrl.ReInitForGenericCode();
                        }

                        var codeGeneratorHelper = new CodeGeneratorHelper();
                        foreach (var ctrl in nodesContainer.NodesControl.CtrlNodeList)
                        {   
                            if (macrosslinkcontrol.IsParticleNode(ctrl))
                            {
                                CurrentClassNode = ctrl;
                                var csparam = ctrl.CSParam as CodeDomNode.Particle.StructNodeControlConstructionParams;
                                macrosslinkcontrol.CopyCategorys(csparam.CategoryDic, macrosslinkcontrol.MacrossOpPanel.CategoryDic);
                                //TODO] 

                                //{
                                //    var ParticleNode = ctrl as CodeDomNode.Particle.IParticleNode;
                                //    var createobj = ParticleNode.GetCreateObject();
                                //    CodeGenerateSystem.Base.GenerateCodeContext_Method context = new CodeGenerateSystem.Base.GenerateCodeContext_Method();
                                //    createobj.GCode_CodeDom_GenerateCode(null, null, null, CodeGenerateSystem.Base.GenerateCodeContext_Method context);
                                //}

                                if ((ctrl as CodeDomNode.Particle.ParticleSystemControl) != null)
                                {
                                    codeGeneratorHelper.ParticleSystemNode = ctrl as CodeDomNode.Particle.ParticleSystemControl;
                                    codestr = await GenerateCode(info, linkCtrl, null, nameSpaceContext, nameSpace, codeGeneratorHelper.ParticleSystemNode.GetClassName(), csparam.BaseClassName);
                                    codeGeneratorHelper.EditorVisibleName = graph.Name;


                                }
                                else if ((ctrl as CodeDomNode.Particle.IParticleShape) != null)
                                {
                                    var node = ctrl as CodeDomNode.Particle.IParticleShape;
                                    if (ParticleNodesControl.FindParticleSystemNode(ctrl as IParticleNode) != null)
                                    {
                                        codestr = await GenerateCode(info, linkCtrl, null, nameSpaceContext, nameSpace, node.GetClassName(), csparam.BaseClassName);
                                        codeGeneratorHelper.ParticleShapeNodes.Add(node);
                                    }
                                    
                                }
                                else if ((ctrl as CodeDomNode.Particle.ParticleStateControl) != null)
                                {
                                    if (ParticleNodesControl.FindParticleSystemNode(ctrl as IParticleNode) != null)
                                    {
                                        var particlestatenode = ctrl as CodeDomNode.Particle.ParticleStateControl;
                                        var particleshapenode = particlestatenode.FindParticleShapeNode();
                                        if (particleshapenode != null)
                                        {
                                            codestr = await GenerateCode(info, linkCtrl, null, nameSpaceContext, nameSpace, particlestatenode.GetClassName(), csparam.BaseClassName);
                                            codeGeneratorHelper.ParticleStateNodes.Add(particleshapenode, ctrl as CodeDomNode.Particle.ParticleStateControl);
                                        }
                                    }
                                }
                                else if ((ctrl as CodeDomNode.Particle.ParticleColorControl) != null)
                                {
                                    if (ParticleNodesControl.FindParticleSystemNode(ctrl as IParticleNode) != null)
                                    {
                                        var particlecolornode = ctrl as CodeDomNode.Particle.ParticleColorControl;
                                        var particleshapenode = particlecolornode.FindParticleShapeNode();
                                        if (particleshapenode != null)
                                        {
                                            CurrentValue.CurrentParticleColorNode = particlecolornode;
                                            codeGeneratorHelper.ParticleColorNodes.Add(particleshapenode, particlecolornode);
                                            codestr = await GenerateCode(info, linkCtrl, GenerateParticleColorCode, nameSpaceContext, nameSpace, particlecolornode.GetClassName(), csparam.BaseClassName);
                                            CurrentValue.CurrentParticleColorNode = null;
                                        }
                                    }
                                }
                                else if ((ctrl as CodeDomNode.Particle.ParticleScaleControl) != null)
                                {
                                    if (ParticleNodesControl.FindParticleSystemNode(ctrl as IParticleNode) != null)
                                    {
                                        var particlescalenode = ctrl as CodeDomNode.Particle.ParticleScaleControl;
                                        var particleshapenode = particlescalenode.FindParticleShapeNode();
                                        if (particleshapenode != null)
                                        {
                                            CurrentValue.CurrentParticleScaleNode = particlescalenode;
                                            codeGeneratorHelper.ParticleScaleNodes.Add(particleshapenode, particlescalenode);
                                            codestr = await GenerateCode(info, linkCtrl, GenerateParticleScaleCode, nameSpaceContext, nameSpace, particlescalenode.GetClassName(), csparam.BaseClassName);
                                            CurrentValue.CurrentParticleScaleNode = null;
                                        }
                                    }
                                }
                                else if ((ctrl as CodeDomNode.Particle.ParticleRandomDirectionControl) != null)
                                {
                                    if (ParticleNodesControl.FindParticleSystemNode(ctrl as IParticleNode) != null)
                                    {
                                        var node = ctrl as CodeDomNode.Particle.ParticleRandomDirectionControl;
                                        var particleshapenode = node.FindParticleShapeNode();
                                        if (particleshapenode != null)
                                        {
                                            CurrentValue.CurrentRandomDirectionNode = node;
                                            codeGeneratorHelper.RandomDirectionNodes.Add(particleshapenode, node);
                                            codestr = await GenerateCode(info, linkCtrl, null, nameSpaceContext, nameSpace, node.GetClassName(), csparam.BaseClassName);
                                            CurrentValue.CurrentRandomDirectionNode = null;
                                        }
                                    }
                                }
                                else if ((ctrl as CodeDomNode.Particle.ParticleAcceleratedControl) != null)
                                {
                                    if (ParticleNodesControl.FindParticleSystemNode(ctrl as IParticleNode) != null)
                                    {
                                        var node = ctrl as CodeDomNode.Particle.ParticleAcceleratedControl;
                                        var particleshapenode = node.FindParticleShapeNode();
                                        if (particleshapenode != null)
                                        {
                                            CurrentValue.CurrentAcceleratedNode = node;
                                            codeGeneratorHelper.AcceleratedNodes.Add(particleshapenode, node);
                                            codestr = await GenerateCode(info, linkCtrl, GenerateParticleAcceleratedCode, nameSpaceContext, nameSpace, node.GetClassName(), csparam.BaseClassName);
                                            CurrentValue.CurrentAcceleratedNode = null;
                                        }
                                    }
                                }
                                else if ((ctrl as CodeDomNode.Particle.ParticleVelocityByCenterControl) != null)
                                {
                                    if (ParticleNodesControl.FindParticleSystemNode(ctrl as IParticleNode) != null)
                                    {
                                        var node = ctrl as CodeDomNode.Particle.ParticleVelocityByCenterControl;
                                        var particleshapenode = node.FindParticleShapeNode();
                                        if (particleshapenode != null)
                                        {
                                            CurrentValue.CurrentParticleVelocityByCenterNode = node;
                                            codeGeneratorHelper.VelocityByCenterNodes.Add(particleshapenode, node);
                                            codestr = await GenerateCode(info, linkCtrl, GenerateVelocityByCenterCode, nameSpaceContext, nameSpace, node.GetClassName(), csparam.BaseClassName);
                                            CurrentValue.CurrentParticleVelocityByCenterNode = null;
                                        }
                                    }
                                }
                                else if ((ctrl as CodeDomNode.Particle.ParticleVelocityByTangentControl) != null)
                                {
                                    if (ParticleNodesControl.FindParticleSystemNode(ctrl as IParticleNode) != null)
                                    {
                                        var node = ctrl as CodeDomNode.Particle.ParticleVelocityByTangentControl;
                                        var particleshapenode = node.FindParticleShapeNode();
                                        if (particleshapenode != null)
                                        {
                                            CurrentValue.CurrentParticleVelocityByTangentNode = node;
                                            codeGeneratorHelper.VelocityByTangentNodes.Add(particleshapenode, node);
                                            codestr = await GenerateCode(info, linkCtrl, GenerateVelocityByTangentCode, nameSpaceContext, nameSpace, node.GetClassName(), csparam.BaseClassName);
                                            CurrentValue.CurrentParticleVelocityByTangentNode = null;
                                        }
                                    }
                                }
                                else if ((ctrl as CodeDomNode.Particle.ParticleVelocityControl) != null)
                                {
                                    if (ParticleNodesControl.FindParticleSystemNode(ctrl as IParticleNode) != null)
                                    {
                                        var particlevelocitynode = ctrl as CodeDomNode.Particle.ParticleVelocityControl;
                                        var particleshapenode = particlevelocitynode.FindParticleShapeNode();
                                        if (particleshapenode != null)
                                        {
                                            List<CodeDomNode.Particle.ParticleVelocityControl> nodes;

                                            if (codeGeneratorHelper.ParticleVelocityNodes.TryGetValue(particleshapenode, out nodes) == false)
                                            {
                                                nodes = new List<CodeDomNode.Particle.ParticleVelocityControl>();
                                                codeGeneratorHelper.ParticleVelocityNodes.Add(particleshapenode, nodes);
                                            }

                                            CurrentValue.CurrentParticleVelocityNode = particlevelocitynode;
                                            nodes.Add(particlevelocitynode);
                                            codestr = await GenerateCode(info, linkCtrl, GenerateParticleVelocityCode, nameSpaceContext, nameSpace, particlevelocitynode.GetClassName(), csparam.BaseClassName);
                                            CurrentValue.CurrentParticleVelocityNode = null;
                                        }
                                    }
                                }
                                else if ((ctrl as CodeDomNode.Particle.ParticleRotationControl) != null)
                                {
                                    if (ParticleNodesControl.FindParticleSystemNode(ctrl as IParticleNode) != null)
                                    {
                                        var node = ctrl as CodeDomNode.Particle.ParticleRotationControl;
                                        var particleshapenode = node.FindParticleShapeNode();
                                        if (particleshapenode != null)
                                        {
                                            List<CodeDomNode.Particle.ParticleRotationControl> nodes;

                                            if (codeGeneratorHelper.ParticleRotationNodes.TryGetValue(particleshapenode, out nodes) == false)
                                            {
                                                nodes = new List<CodeDomNode.Particle.ParticleRotationControl>();
                                                codeGeneratorHelper.ParticleRotationNodes.Add(particleshapenode, nodes);
                                            }

                                            CurrentValue.CurrentParticleRotationNode = node;
                                            nodes.Add(node);
                                            codestr = await GenerateCode(info, linkCtrl, GenerateParticleRotationCode, nameSpaceContext, nameSpace, node.GetClassName(), csparam.BaseClassName);
                                            CurrentValue.CurrentParticleRotationNode = null;
                                        }
                                    }
                                }
                                else if ((ctrl as CodeDomNode.Particle.ParticleTransformControl) != null)
                                {
                                    if (ParticleNodesControl.FindParticleSystemNode(ctrl as IParticleNode) != null)
                                    {
                                        var node = ctrl as CodeDomNode.Particle.ParticleTransformControl;
                                        var particleshapenode = node.FindParticleShapeNode();
                                        if (particleshapenode != null)
                                        {
                                            codeGeneratorHelper.TransformNodes.Add(particleshapenode, node);

                                            CurrentValue.CurrentParticleTransformNode = node;
                                            codestr = await GenerateCode(info, linkCtrl, GenerateParticleTransformCode, nameSpaceContext, nameSpace, node.GetClassName(), csparam.BaseClassName);
                                            CurrentValue.CurrentParticleTransformNode = null;
                                        }
                                    }
                                }
                                else if ((ctrl as CodeDomNode.Particle.ParticleTrailControl) != null)
                                {
                                    if (ParticleNodesControl.FindParticleSystemNode(ctrl as IParticleNode) != null)
                                    {
                                        var particletrailnode = ctrl as CodeDomNode.Particle.ParticleTrailControl;
                                        codeGeneratorHelper.ParticleTrailNode = particletrailnode;
                                        codestr = await GenerateCode(info, linkCtrl, null, nameSpaceContext, nameSpace, particletrailnode.GetClassName(), csparam.BaseClassName);
                                    }
                                }
                                else if ((ctrl as CodeDomNode.Particle.ParticleTriggerControl) != null)
                                {
                                    if (ParticleNodesControl.FindParticleSystemNode(ctrl as IParticleNode) != null)
                                    {
                                        var particletriggernode = ctrl as CodeDomNode.Particle.ParticleTriggerControl;
                                        codeGeneratorHelper.ParticleTriggerNode.Add(particletriggernode);
                                        codestr = await GenerateCode(info, linkCtrl, null, nameSpaceContext, nameSpace, particletriggernode.GetClassName(), csparam.BaseClassName);
                                    }
                                }
                                else if ((ctrl as CodeDomNode.Particle.ParticleTextureCutControl) != null)
                                {
                                    if (ParticleNodesControl.FindParticleSystemNode(ctrl as IParticleNode) != null)
                                    {
                                        var node = ctrl as CodeDomNode.Particle.ParticleTextureCutControl;
                                        codeGeneratorHelper.TextureCutNode = node;
                                        codestr = await GenerateCode(info, linkCtrl, null, nameSpaceContext, nameSpace, node.GetClassName(), csparam.BaseClassName);
                                    }
                                }
                                else if ((ctrl as CodeDomNode.Particle.ParticleMaterialInstanceControl) != null)
                                {
                                    if (ParticleNodesControl.FindParticleSystemNode(ctrl as IParticleNode) != null)
                                    {
                                        var node = ctrl as CodeDomNode.Particle.ParticleMaterialInstanceControl;
                                        CurrentValue.CurrentMaterialInstanceNode = node;
                                        codeGeneratorHelper.MaterialInstanceNode = node;
                                        codestr = await GenerateCode(info, linkCtrl, GenerateParticleMaterialCode, nameSpaceContext, nameSpace, node.GetClassName(), csparam.BaseClassName);
                                        CurrentValue.CurrentMaterialInstanceNode = null;
                                    }
                                }
                                else if ((ctrl as CodeDomNode.Particle.ParticleComposeControl) != null)
                                {
                                    if (ParticleNodesControl.FindParticleSystemNode(ctrl as IParticleNode) != null)
                                    {
                                        var particlecomposenode = ctrl as CodeDomNode.Particle.ParticleComposeControl;

                                        particlecomposenode.DealShape();
                                        if (particlecomposenode.CanUse())
                                        {
                                            codestr = await GenerateCode(info, linkCtrl, null, nameSpaceContext, nameSpace, particlecomposenode.GetClassName(), csparam.BaseClassName);
                                            if (particlecomposenode.IsLastNode())
                                            {
                                                codeGeneratorHelper.ParticleComposes.Add(particlecomposenode);
                                            }
                                        }
                                    }
                                }

                                else if ((ctrl as CodeDomNode.Particle.ParticlePointGravityControl) != null)
                                {
                                    if (ParticleNodesControl.FindParticleSystemNode(ctrl as IParticleNode) != null)
                                    {
                                        var node = ctrl as CodeDomNode.Particle.ParticlePointGravityControl;
                                        codeGeneratorHelper.PointGravityNode = node;
                                        codestr = await GenerateCode(info, linkCtrl, null, nameSpaceContext, nameSpace, node.GetClassName(), csparam.BaseClassName);
                                    }
                                }

                                CurrentClassNode = null;
                            }
                        }
                        codeGeneratorHelpers.Add(codeGeneratorHelper);

                    }

                }

                if (info.BaseType == null)
                {
                    codestr = await GenerateCode(info, linkCtrl, generateAction, nameSpaceContext, nameSpace, className, typeof(EngineNS.Bricks.Particle.McParticleEffector).FullName);
                }
                else
                {
                    codestr = await GenerateCode(info, linkCtrl, generateAction, nameSpaceContext, nameSpace, className, info.BaseType.FullName);
                }
                //return codestr;
                
                codeGeneratorHelpers = null;
                return codestr;

            }
            catch (System.Exception e)
            {
                EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, "Macross", e.ToString());
                System.IO.TextWriter retTw = new System.IO.StringWriter();
                retTw.WriteLine("// Engine!\n");
                retTw.WriteLine($"// 名称:{className}");
                retTw.WriteLine($"#error Macross生成代码异常! Id={info.Id}, Name={className}\r\n{e.ToString()}");
                return retTw.ToString();
            }
        }
    }
}
