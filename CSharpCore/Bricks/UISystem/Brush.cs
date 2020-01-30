using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using EngineNS.IO.Serializer;
using EngineNS.Rtti;

namespace EngineNS.UISystem
{
    [Rtti.MetaClass]
   
    [EngineNS.Editor.Editor_MacrossClass(ECSType.Client, EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    public class Brush : EngineNS.IO.Serializer.Serializer, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        RName mImageName = EngineNS.CEngineDesc.DefaultUIUVAnim;
        [Rtti.MetaData]
        [DisplayName("图元名称")]
        [EngineNS.Editor.Editor_PackDataAttribute]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.Editor_RNameTypeAttribute(EngineNS.Editor.Editor_RNameTypeAttribute.UVAnim)]
        public RName ImageName
        {
            get => mImageName;
            set
            {
                mImageName = value;
                var noUse = UpdateUVAnim();
                ForceUpdateDraw = true;
                OnPropertyChanged("ImageName");
            }
        }
        string mTextureShaderName = "texture";
        [Rtti.MetaData]
        [Category("Shader")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [DisplayName("贴图Shader参数名称")]
        public string TextureShaderName
        {
            get => mTextureShaderName;
            set
            {
                mTextureShaderName = value;
                if(mImage2D != null && mUVAnim != null)
                    mImage2D.SetTexture(EngineNS.CEngine.Instance.RenderContext, TextureShaderName, mUVAnim.TextureRName);
                OnPropertyChanged("TextureShaderName");
            }
        }
        //float mOpacity = 1.0f;
        //[Rtti.MetaData]
        //[EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        //[DisplayName("透明度")]
        //public float Opacity
        //{
        //    get => mOpacity;
        //    set
        //    {
        //        mOpacity = value;
        //        if (mImage2D != null)
        //            mImage2D.RenderOpacity = mOpacity;
        //        OnPropertyChanged("Opacity");
        //    }
        //}
        Color4 mColor = new Color4(1, 1, 1, 1);
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.Editor_UseCustomEditor]
        [DisplayName("颜色")]
        public Color4 Color
        {
            get => mColor;
            set
            {
                mColor = value;
                if (mImage2D != null)
                    mImage2D.RenderColor = mColor;
                OnPropertyChanged("Color");
            }
        }

        float mTileU = 1.0f;
        [Category("Tile")]
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float TileU
        {
            get => mTileU;
            set
            {
                mTileU = value;
                ForceUpdateDraw = true;
                OnPropertyChanged("TileU");
            }
        }
        float mTileV = 1.0f;
        [Category("Tile")]
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float TileV
        {
            get => mTileV;
            set
            {
                mTileV = value;
                ForceUpdateDraw = true;
                OnPropertyChanged("TileV");
            }
        }
        public enum enTileMode
        {
            None,
            Horizontal,
            Vertical,
            Both,
            Custom,
        }
        enTileMode mTileMode = enTileMode.None;
        [Category("Tile")]
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public enTileMode TileMode
        {
            get => mTileMode;
            set
            {
                mTileMode = value;
                ForceUpdateDraw = true;
                OnPropertyChanged("TileMode");
            }
        }

        Vector2 mImageSize = Vector2.Zero;
        [Rtti.MetaData]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector2 ImageSize
        {
            get => mImageSize;
            set
            {
                mImageSize = value;
                mHostElement?.UpdateLayout();
                OnPropertyChanged("ImageSize");
            }
        }

        Graphics.Mesh.CGfxImage2D mImage2D;
        [Browsable(false)]
        public Graphics.Mesh.CGfxImage2D Image2D
        {
            get => mImage2D;
        }
        UVAnim mUVAnim;
        [Browsable(false)]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.PropReadOnly)]
        public UVAnim UVAnim
        {
            get => mUVAnim;
        }

        UIElement mHostElement;
        bool mInitialized = false;
        public async Task Initialize(CRenderContext rc, UIElement hostElement)
        {
            mHostElement = hostElement;

            mImage2D = await EngineNS.Graphics.Mesh.CGfxImage2D.CreateImage2D(rc, CEngineDesc.DefaultUIMaterialInstance, 0, 0, 0, 1, 1);
            mImage2D.RenderMatrix = Matrix.Scaling(100.0f, 100.0f, 1.0f);

            await UpdateUVAnim();
            if (ImageSize == Vector2.Zero)
                ImageSize = GetImagePixelSize();
            //mImage2D.RenderOpacity = Opacity;
            mImage2D.RenderColor = mColor;
            mInitialized = true;
        }
        async Task UpdateUVAnim()
        {
            if (mImageName == null || mImageName == EngineNS.RName.EmptyName)
                mUVAnim = null;
            else
            {
                var rc = EngineNS.CEngine.Instance.RenderContext;
                if (rc == null)
                    return;
                mUVAnim = await EngineNS.CEngine.Instance.UVAnimManager.GetUVAnimCloneAsync(rc, ImageName);
                if(mImage2D != null && mUVAnim != null)
                {
                    if(mUVAnim.MaterialInstanceRName != null && mUVAnim.MaterialInstanceRName != RName.EmptyName)
                        await mImage2D.SetMaterialInstance(rc, mUVAnim.MaterialInstanceRName);
                    if(mUVAnim.TextureRName != null && mUVAnim.TextureRName != RName.EmptyName)
                        mImage2D.SetTexture(rc, TextureShaderName, mUVAnim.TextureRName);
                }
            }

            if (mInitialized)
                ImageSize = GetImagePixelSize();
        }
        Vector2 GetImagePixelSize()
        {
            if (mUVAnim == null)
                return Vector2.Zero;
            if (mUVAnim.Frames.Count == 0)
                return Vector2.Zero;

            var frame = mUVAnim.Frames[0];
            var retSize = new Vector2((mUVAnim.PixelWidth * frame.SizeU), (mUVAnim.PixelHeight * frame.SizeV));
            return retSize;
        }
        public bool ForceUpdateDraw = true;      // 强制刷新绘制
        public void CommitUVAnim(CCommandList cmd, ref RectangleF designRect, ref RectangleF clipRect, ref Matrix parentTransformMatrix, float dpiScale)
        {
            if (mUVAnim == null || mImage2D == null)
                return;

            mUVAnim.CheckAndAutoReferenceFromTemplateUVAnim();
            bool frameChanged;
            var frame = mUVAnim.GetUVFrame(Support.Time.GetTickCount(), out frameChanged);
            if (frameChanged || ForceUpdateDraw)
            {
                using (var posData = Support.NativeListProxy<Vector3>.CreateNativeList())
                using (var uvData = Support.NativeListProxy<Vector2>.CreateNativeList())
                {
                    switch (this.TileMode)
                    {
                        case Brush.enTileMode.None:
                            {
                                frame.UTile = 1.0f;
                                frame.VTile = 1.0f;
                            }
                            break;
                        case Brush.enTileMode.Horizontal:
                            {
                                if (mUVAnim.PixelWidth == UInt32.MaxValue)
                                    frame.UTile = 1.0f;
                                else
                                    frame.UTile = (float)designRect.Width / (mUVAnim.PixelWidth * frame.SizeU);
                                frame.VTile = 1.0f;
                            }
                            break;
                        case Brush.enTileMode.Vertical:
                            {
                                if (mUVAnim.PixelHeight == UInt32.MaxValue)
                                    frame.VTile = 1.0f;
                                else
                                    frame.VTile = (float)designRect.Height / (mUVAnim.PixelHeight * frame.SizeV);
                                frame.VTile = 1.0f;
                            }
                            break;
                        case Brush.enTileMode.Both:
                            {
                                if (mUVAnim.PixelWidth == UInt32.MaxValue)
                                    frame.UTile = 1.0f;
                                else
                                    frame.UTile = (float)designRect.Width / (mUVAnim.PixelWidth * frame.SizeU);

                                if (mUVAnim.PixelHeight == UInt32.MaxValue)
                                    frame.VTile = 1.0f;
                                else
                                    frame.VTile = (float)designRect.Height / (mUVAnim.PixelHeight * frame.SizeV);
                            }
                            break;
                        case Brush.enTileMode.Custom:
                            {
                                frame.UTile = this.TileU;
                                frame.VTile = this.TileV;
                            }
                            break;
                    }
                    frame.UpdateVertexes(posData, ref designRect, ref clipRect);
                    frame.UpdateUVs(uvData, ref designRect, ref clipRect);

                    mImage2D.SetUV(uvData, cmd);
                    mImage2D.SetVertexBuffer(posData, cmd);
                }

                ForceUpdateDraw = false;
            }
            var tempRect = new RectangleF(clipRect.Left * dpiScale, clipRect.Top * dpiScale, clipRect.Width * dpiScale, clipRect.Height * dpiScale);
            // System.Diagnostics.Debug.WriteLine($"{tempRect.X},{tempRect.Y},{tempRect.Width},{tempRect.Height}");
            mImage2D.RenderMatrix = Matrix.Scaling(tempRect.Width, tempRect.Height, 1) *
                                    Matrix.Translate(tempRect.X, tempRect.Y, 0.0f) *
                                    parentTransformMatrix;
        }
        public void CommitUVAnim(CCommandList cmd, UIElement hostElement, ref Matrix parentTransformMatrix, float dpiScale)
        {
            var designRect = hostElement.DesignRect;
            var clipRect = hostElement.DesignClipRect;
            CommitUVAnim(cmd, ref designRect, ref clipRect, ref parentTransformMatrix, dpiScale);
        }
        public void Draw(CRenderContext rc, CCommandList cmd, Graphics.View.CGfxScreenView view)
        {
            if (mUVAnim == null || Image2D == null)
                return;

            var mtlmesh = this.Image2D.Mesh.MtlMeshArray[0];
            var pass = this.Image2D.GetPass();
            
            pass.ViewPort = view.Viewport;
            if (pass.RenderPipeline == null)
            {
                var rplDesc = new CRenderPipelineDesc();
                pass.RenderPipeline = rc.CreateRenderPipeline(rplDesc);
            }
            pass.RenderPipeline.RasterizerState = mtlmesh.MtlInst.CustomRasterizerState;
            pass.RenderPipeline.DepthStencilState = mtlmesh.MtlInst.CustomDepthStencilState;
            pass.RenderPipeline.BlendState = mtlmesh.MtlInst.CustomBlendState;
            //pass.ShaderSamplerBinder = mtlmesh.GetSamplerBinder(rc, pass.Effect.ShaderProgram);
            pass.BindCBuffer(pass.Effect.ShaderProgram, pass.Effect.CacheData.CBID_View, view.ScreenViewCB);
            //if (pass.Effect.PerFrameId != UInt32.MaxValue)
            //    pass.BindCBuffer(pass.Effect.ShaderProgram, pass.Effect.PerFrameId, CEngine.Instance.PerFrameCBuffer);
            pass.ShadingEnv.BindResources(this.Image2D.Mesh, pass);
            cmd.PushPass(pass);
        }

        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetMaterialVarFloatValue(uint matIdx, string name, float val)
        {
            var idx = this.Image2D.Mesh.McFindVar(matIdx, name);
            this.Image2D.Mesh.McSetVarFloat(matIdx, idx, val, 0);
        }
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetMaterialVarVector2Value(uint matIdx, string name, ref Vector2 val)
        {
            var idx = this.Image2D.Mesh.McFindVar(matIdx, name);
            this.Image2D.Mesh.McSetVarVector2(matIdx, idx, val, 0);
        }
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetMaterialVarVector3Value(uint matIdx, string name, ref Vector3 val)
        {
            var idx = this.Image2D.Mesh.McFindVar(matIdx, name);
            this.Image2D.Mesh.McSetVarVector3(matIdx, idx, val, 0);
        }
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void SetMaterialVarColor4Value(uint matIdx, string name, ref Color4 val)
        {
            var idx = this.Image2D.Mesh.McFindVar(matIdx, name);
            this.Image2D.Mesh.McSetVarColor4(matIdx, idx, val, 0);
        }
    }
}
