using System;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;
using EngineNS.Graphics.Pipeline.Shader;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode.Var
{
    /// <summary>
    /// Material editor node that outputs a SubsurfaceProfile index as a uniform float.
    /// User selects a profile RName in the property panel; at runtime, C# sets the
    /// corresponding integer index into the material's uniform variable.
    /// Connect this node's output to the MaterialOutput "Mask" pin.
    /// </summary>
    [ContextMenu("subsurface,sss,profile", "Subsurface\\SubsurfaceProfileId", TtMaterialGraph.MaterialEditorKeyword)]
    public class TtSubsurfaceProfileIdNode : VarNode
    {
        public const string UniformVarName = "SubsurfaceProfileIndex";

        [Browsable(false)]
        public PinOut OutIndex { get; set; } = new PinOut();

        private RName mProfileAsset;

        /// <summary>
        /// The SubsurfaceProfile asset to reference. The runtime will resolve this
        /// to the profile's integer index in TtSubsurfaceProfileManager.
        /// </summary>
        [Rtti.Meta("")]
        [Category("Option")]
        [RName.PGRName(FilterExts = Graphics.Pipeline.Deferred.TtSubsurfaceProfileData.AssetExt)]
        public RName ProfileAsset
        {
            get => mProfileAsset;
            set
            {
                mProfileAsset = value;
                UpdatePreviewIndex();
            }
        }

        /// <summary>
        /// Preview-only: shows the resolved index in the editor. Not serialized.
        /// </summary>
        [Category("Info")]
        [ReadOnly(true)]
        public int PreviewIndex { get; private set; } = 0;

        public TtSubsurfaceProfileIdNode()
        {
            VarType = Rtti.TtTypeDescGetter<float>.TypeDesc;
            Name = UniformVarName;
            IsUniform = true;
            IsHalfPrecision = false;

            OutIndex.Name = "Index";
            OutIndex.LinkDesc = TtMaterialEditorStyles.Instance.NewInOutPinDesc();
            OutIndex.MultiLinks = true;
            this.AddPinOut(OutIndex);
        }

        public override Rtti.TtTypeDesc GetOutPinType(PinOut pin)
        {
            return Rtti.TtTypeDescGetter<float>.TypeDesc;
        }

        public override bool CanLinkFrom(PinIn iPin, TtNodeBase OutNode, PinOut oPin)
        {
            return false; // No inputs allowed; this is a pure output node
        }

        protected override string GetDefaultValue()
        {
            return PreviewIndex.ToString();
        }

        protected override void OnAsUniform(bool isUniform)
        {
            // Always uniform, override to prevent user toggling
            TitleColor = 0xFF2040af;
        }

        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            var material = data.UserData as TtMaterial;
            if (material != null && material.FindVar(UniformVarName) == null)
            {
                var uniformPair = new TtMaterial.NameValuePair();
                uniformPair.VarType = "float";
                uniformPair.Name = UniformVarName;
                uniformPair.Value = PreviewIndex.ToString();
                material.UsedUniformVars.Add(uniformPair);
            }
            if (material != null)
            {
                material.SubsurfaceProfileAsset = mProfileAsset;
            }

            if (!data.MethodDec.HasLocalVariable(UniformVarName))
            {
                var variableDeclaration = new TtVariableDeclaration()
                {
                    VariableType = new TtTypeReference(Type2HLSLType(VarType, IsHalfPrecision)),
                    VariableName = UniformVarName,
                    InitValue = new TtPrimitiveExpression((float)PreviewIndex),
                };
                OnAddLocalVar(variableDeclaration, ref data);
            }
        }

        public override TtExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if (pin == OutIndex)
                return new TtVariableReferenceExpression(UniformVarName);
            return null;
        }

        private void UpdatePreviewIndex()
        {
            if (mProfileAsset == null)
            {
                PreviewIndex = 0;
                return;
            }

            var profileManager = TtEngine.Instance?.GfxDevice?.SubsurfaceProfileManager;
            if (profileManager != null)
            {
                PreviewIndex = profileManager.GetIndexByRName(mProfileAsset);
            }
        }
        public override void UpdateAMetaReferences(IO.IAssetMeta ameta, Bricks.CodeBuilder.ShaderNode.TtMaterialGraph MaterialGraph)
        {
            ameta.AddReferenceAsset(ProfileAsset);
        }
    }
}
