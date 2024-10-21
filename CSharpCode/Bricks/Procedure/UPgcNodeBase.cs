using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure
{
    public abstract partial class UPgcNodeBase : TtNodeBase, EGui.Controls.PropertyGrid.IPropertyCustomization
    {
        public int RootDistance;
        protected int mPreviewResultIndex = -1;
        public float ScaleForPreview { get; set; } = 1.0f;
        protected NxRHI.TtTexture PreviewTexture;
        protected NxRHI.TtSrView PreviewSRV;
        public class UCompileButton
        {
            internal UPgcNodeBase HostNode;
            public class UValueEditorAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
            {
                public unsafe override bool OnDraw(in EditorInfo info, out object newValue)
                {
                    newValue = info.Value;
                    var nodeDef = newValue as UCompileButton;
                    
                    if (ImGuiAPI.Button("Compile"))
                    {
                        var graph = nodeDef.HostNode.ParentGraph as UPgcGraph;

                        graph.Compile(nodeDef.HostNode);
                    }
                    if (ImGuiAPI.Button("PreviewMesh"))
                    {
                        var task = nodeDef.HostNode.DoPreviewMesh();
                    }
                    return false;
                }
            }
        }
        public virtual async System.Threading.Tasks.Task DoPreviewMesh()
        {
            if (this.PreviewResultIndex < 0)
                return;

            var graph = this.ParentGraph as UPgcGraph;
            graph.GraphEditor.PreviewRoot.ClearChildren();

            var vms = Graphics.Mesh.TtMeshDataProvider.MakeRect2D(0, 0, 100, 100, 0).ToMesh();
            var mesh = new Graphics.Mesh.TtMesh();

            var materials1 = new Graphics.Pipeline.Shader.TtMaterialInstance[1];
            materials1[0] = Graphics.Pipeline.Shader.TtMaterialInstance.CreateMaterialInstance(
                await TtEngine.Instance.GfxDevice.MaterialManager.GetMaterial(RName.GetRName("material/SysDft.material", RName.ERNameType.Engine))
                );
            var state = materials1[0].Rasterizer;
            state.CullMode = NxRHI.ECullMode.CMD_NONE;
            materials1[0].Rasterizer = state;
            var srv = materials1[0].FindSRV("DiffuseTex");
            if (srv != null)
            {
                srv.SrvObject = this.PreviewSRV;
            }
            mesh.Initialize(vms, materials1, Rtti.TtTypeDesc.TypeOf(typeof(Graphics.Mesh.UMdfStaticMesh)));
            var nodeData = new GamePlay.Scene.TtMeshNode.TtMeshNodeData();
            nodeData.Name = "TexturePreivew";
            var prevMesh = await GamePlay.Scene.TtMeshNode.AddMeshNode(graph.GraphEditor.PreviewViewport.World, graph.GraphEditor.PreviewRoot, 
                    new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.TtPlacement), mesh,
                    DVector3.Zero, Vector3.One, Quaternion.Identity);
            prevMesh.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.None;
            prevMesh.IsCastShadow = true;
            prevMesh.IsAcceptShadow = true;

            var aabb = mesh.MaterialMesh.AABB;
            float radius = aabb.GetMaxSide();
            DBoundingSphere sphere;
            sphere.Center = aabb.GetCenter().AsDVector();
            sphere.Radius = radius;
            graph.GraphEditor.PreviewViewport.RenderPolicy.DefaultCamera.AutoZoom(in sphere);
        }
        [UCompileButton.UValueEditor]
        public UCompileButton CompileButton
        {
            get;
        } = new UCompileButton();
        public UPgcNodeBase()
        {
            CompileButton.HostNode = this;
            this.TitleColor = Color4b.FromRgb(255, 0, 255).ToArgb();
        }
        ~UPgcNodeBase()
        {
            if (PreviewSRV != null)
            {
                PreviewSRV?.FreeTextureHandle();
                PreviewSRV = null;
            }
        }
        public void AddInput(PinIn pin, string name, UBufferCreator desc, string linkType = "Value")
        {
            pin.Name = name;
            pin.LinkDesc = UPgcEditorStyles.Instance.NewInOutPinDesc(linkType);

            pin.Tag = desc;
            AddPinIn(pin);
        }
        public void AddOutput(PinOut pin, string name, UBufferCreator desc, string linkType = "Value")
        {
            pin.Name = name;
            pin.LinkDesc = UPgcEditorStyles.Instance.NewInOutPinDesc(linkType);

            pin.Tag = desc;
            AddPinOut(pin);
        }
        public virtual void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {

        }
        #region GUI
        public override void OnMouseStayPin(NodePin stayPin, TtNodeGraph graph)
        {
            var creator = stayPin.Tag as UBufferCreator;
            if (creator != null)
            {
                EGui.Controls.CtrlUtility.DrawHelper($"{creator.ElementType.Name}");
            }
        }
        [Rtti.Meta]
        public virtual int PreviewResultIndex
        {
            get => mPreviewResultIndex;
            set
            {
                if (mPreviewResultIndex == value)
                    return;
                mPreviewResultIndex = value;
                if (value >= 0)
                {
                    PrevSize = new Vector2(100, 100);
                }
                else
                {
                    PrevSize = Vector2.Zero;
                }
                OnPositionChanged();
            }
        }
        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            if (mPreviewResultIndex < 0)
                return;

            if (PreviewSRV == null)
                return;

            var uv0 = new Vector2(0, 0);
            var uv1 = new Vector2(1, 1);
            unsafe
            {
                cmdlist.AddImage((ulong)PreviewSRV.GetTextureHandle(), in prevStart, in prevEnd, in uv0, in uv1, 0xFFFFFFFF);
            }
        }
        public override void OnLButtonClicked(NodePin clickedPin)
        {
            //var graph = this.ParentGraph as UPgcGraph;

            //if (graph != null && graph.GraphEditor != null)
            //{
            //    graph.GraphEditor.NodePropGrid.Target = this;
            //}
        }
        #endregion

        #region Link
        public override void OnLinkedFrom(PinIn iPin, TtNodeBase OutNode, PinOut oPin)
        {
            if (iPin.LinkDesc.CanLinks.Contains("Value"))
            {
                ParentGraph.RemoveLinkedInExcept(iPin, OutNode, oPin.Name);
            }
        }
        public override bool CanLinkFrom(PinIn iPin, TtNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;
            var input = iPin.Tag as UBufferCreator;
            var output = oPin.Tag as UBufferCreator;

            if (IsMatchLinkedPin(input, output) == false)
            {
                return false;
            }
            return true;
        }
        public virtual bool IsMatchLinkedPin(UBufferCreator input, UBufferCreator output)
        {
            if (input == output)
                return true;
            if (input == null || output == null)
                return false;
            if (input.ElementType != output.ElementType)
            {
                return false;
            }
            if (input.XSize > output.XSize)
            {
                return false;
            }
            if (input.YSize > output.YSize)
            {
                return false;
            }
            if (input.ZSize > output.ZSize)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region Cache
        public virtual Hash160 GetOutBufferHash(PinOut pin)
        {
            return Hash160.Emtpy;
        }
        public void SaveOutBufferToCache(UPgcGraph graph, PinOut pin)
        {
            if (graph.IsTryCacheBuffer == false)
                return;
            var buffer = graph.BufferCache.FindBuffer(pin);
            if (buffer == null)
                return;
            var hash = GetOutBufferHash(pin);
            if (hash == Hash160.Emtpy)
                return;
            var rootDir = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Cache) + "pgcbuffer/";
            var dir = IO.TtFileManager.CombinePath(rootDir, graph.AssetName.Name);
            IO.TtFileManager.SureDirectory(dir);
            buffer.SaveToCache($"{dir}/{this.Name}_{NodeId}_{pin.Name}.bfpgc", in hash);
        }
        public void SaveOutBufferToCache(UPgcGraph graph, PinOut pin, in Hash160 hash)
        {
            if (graph.IsTryCacheBuffer == false)
                return;
            var buffer = graph.BufferCache.FindBuffer(pin);
            if (buffer == null)
                return;
            var rootDir = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Cache) + "pgcbuffer/";
            var dir = IO.TtFileManager.CombinePath(rootDir, graph.AssetName.Name);
            IO.TtFileManager.SureDirectory(dir);
            buffer.SaveToCache($"{dir}/{this.Name}_{NodeId}_{pin.Name}.bfpgc", in hash);
        }
        public bool TryLoadOutBufferFromCache(UPgcGraph graph, PinOut pin)
        {
            if (graph.IsTryCacheBuffer == false)
                return false;
            var buffer = graph.BufferCache.FindBuffer(pin);
            if (buffer == null)
                return false;
            var hash = GetOutBufferHash(pin);
            var rootDir = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Cache) + "pgcbuffer/";
            var dir = IO.TtFileManager.CombinePath(rootDir, graph.AssetName.Name);

            return buffer.LoadFromCache($"{dir}/{this.Name}_{NodeId}_{pin.Name}.bfpgc", in hash);
        }
        public bool TryLoadOutBufferFromCache(UPgcGraph graph, PinOut pin, in Hash160 hash)
        {
            if (graph.IsTryCacheBuffer == false)
                return false;
            var buffer = graph.BufferCache.FindBuffer(pin);
            if (buffer == null)
                return false;
            var rootDir = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Cache) + "pgcbuffer/";
            var dir = IO.TtFileManager.CombinePath(rootDir, graph.AssetName.Name);

            return buffer.LoadFromCache($"{dir}/{this.Name}_{NodeId}_{pin.Name}.bfpgc", in hash);
        }
        #endregion

        public UBufferCreator GetInputBufferCreator(PinIn pin)
        {
            return pin.Tag as UBufferCreator;
        }
        public abstract UBufferCreator GetOutBufferCreator(PinOut pin);

        #region procedure
        public virtual UBufferComponent GetResultBuffer(int index)
        {
            if (index < 0 || index >= Outputs.Count)
                return null;
            var graph = ParentGraph as UPgcGraph;
            return graph.BufferCache.FindBuffer(Outputs[index]);
        }
        public virtual bool InitProcedure(UPgcGraph graph)
        {
            return true;
        }
        public bool DoProcedure(UPgcGraph graph)
        {
            var ret = OnProcedure(graph);
            PreviewSRVProcedure(graph);
            OnAfterProcedure(graph);
            return ret;
        }
        protected virtual void PreviewSRVProcedure(UPgcGraph graph)
        {
            if (graph.GraphEditor != null)
            {
                if (mPreviewResultIndex >= 0)
                {
                    if (PreviewSRV != null)
                    {
                        PreviewSRV?.FreeTextureHandle();
                        PreviewSRV = null;
                    }

                    var previewBuffer = GetResultBuffer(mPreviewResultIndex);
                    if (previewBuffer != null)
                    {
                        if (previewBuffer.BufferCreator.ElementType == Rtti.TtTypeDescGetter<float>.TypeDesc)
                        {
                            float minV = float.MaxValue;
                            float maxV = float.MinValue;
                            previewBuffer.GetRangeUnsafe<float, FFloatOperator>(out minV, out maxV);
                            PreviewSRV = previewBuffer.CreateAsHeightMapTexture2D(out PreviewTexture, minV, maxV, EPixelFormat.PXF_R16_FLOAT, ScaleForPreview, true);
                        }
                        else if (previewBuffer.BufferCreator.ElementType == Rtti.TtTypeDescGetter<sbyte>.TypeDesc)
                        {
                            sbyte minV = sbyte.MaxValue;
                            sbyte maxV = sbyte.MinValue;
                            previewBuffer.GetRangeUnsafe<sbyte, FSByteOperator>(out minV, out maxV);
                            PreviewSRV = previewBuffer.CreateAsHeightMapTexture2D(out PreviewTexture, (float)minV, (float)maxV, EPixelFormat.PXF_R16_FLOAT, ScaleForPreview, true);
                        }
                        else if (previewBuffer.BufferCreator.ElementType == Rtti.TtTypeDescGetter<Vector2>.TypeDesc)
                        {
                            Vector2 minV = Vector2.MaxValue;
                            Vector2 maxV = Vector2.MinValue;
                            previewBuffer.GetRangeUnsafe<Vector2, FFloat2Operator>(out minV, out maxV);
                            PreviewSRV = previewBuffer.CreateVector2Texture2D(minV, maxV);
                        }
                        else if (previewBuffer.BufferCreator.ElementType == Rtti.TtTypeDescGetter<Vector3>.TypeDesc)
                        {
                            Vector3 minV = Vector3.MaxValue;
                            Vector3 maxV = Vector3.MinValue;
                            previewBuffer.GetRangeUnsafe<Vector3, FFloat3Operator>(out minV, out maxV);
                            //float fMax = maxV.GetMaxValue();
                            //float fMin = minV.GetMinValue();
                            PreviewSRV = previewBuffer.CreateVector3Texture2D(minV, maxV);
                        }
                    }
                }
            }
        }
        public virtual bool OnProcedure(UPgcGraph graph)
        {
            return true;
        }
        public virtual void OnAfterProcedure(UPgcGraph graph)
        {

        }
        [Rtti.Meta]
        public void DispatchBuffer(UPgcGraph graph, UBufferComponent result, object tag, bool bMultThread = false)
        {
            if (result == null)
                return;
            if (bMultThread == false)
            {
                for (int i = 0; i < result.Depth; i++)
                {
                    for (int j = 0; j < result.Height; j++)
                    {
                        for (int k = 0; k < result.Width; k++)
                        {
                            OnPerPixel(graph, this, result, k, j, i, tag);
                        }
                    }
                }
            }
            else
            {
                //TtEngine.Instance.EventPoster.ParrallelFor(result.Depth * result.Height * result.Width, (index, arg0, arg1) =>
                //{
                //    var pThis = arg1 as UBufferConponent;
                //    pThis.OnPerPixel(graph, this, result, x, y, z, tag);
                //}, this, null);
                var smp = Thread.TtSemaphore.CreateSemaphore(result.Depth * result.Height * result.Width, new AutoResetEvent(false));
                if (result.Depth == 1 && result.Height == 1)
                {
                    for (int i = 0; i < result.Depth; i++)
                    {
                        for (int j = 0; j < result.Height; j++)
                        {
                            for (int k = 0; k < result.Width; k++)
                            {
                                int x = k;
                                int y = j;
                                int z = i;
                                TtEngine.Instance.EventPoster.RunOn((state) =>
                                {
                                    OnPerPixel(graph, this, result, x, y, z, tag);
                                    smp.Release();
                                    return true;
                                }, Thread.Async.EAsyncTarget.TPools);
                            }
                        }
                    }
                }
                else if (result.Depth == 1)
                {
                    for (int i = 0; i < result.Depth; i++)
                    {
                        for (int j = 0; j < result.Height; j++)
                        {
                            int y = j;
                            int z = i;
                            TtEngine.Instance.EventPoster.RunOn((state) =>
                            {
                                for (int k = 0; k < result.Width; k++)
                                {
                                    OnPerPixel(graph, this, result, k, y, z, tag);
                                    smp.Release();
                                }
                                return true;
                            }, Thread.Async.EAsyncTarget.TPools);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < result.Depth; i++)
                    {
                        int z = i;
                        TtEngine.Instance.EventPoster.RunOn((state) =>
                        {
                            for (int j = 0; j < result.Height; j++)
                            {
                                for (int k = 0; k < result.Width; k++)
                                {
                                    OnPerPixel(graph, this, result, k, j, z, tag);
                                    smp.Release();
                                }
                            }
                            return true;
                        }, Thread.Async.EAsyncTarget.TPools);
                    }
                }
                smp.Wait(int.MaxValue);
                smp.FreeSemaphore();
            }
        }
        public virtual void OnPerPixel(UPgcGraph graph, UPgcNodeBase node, UBufferComponent resuilt, int x, int y, int z, object tag)
        {

        }
        #endregion

        #region Macross
        [Rtti.Meta()]
        public UBufferComponent FindBuffer(string name)
        {
            var graph = this.ParentGraph as UPgcGraph;
            var pin = this.FindPinIn(name) as NodePin;
            if (pin == null)
            {
                pin = this.FindPinOut(name);
                if (pin == null)
                {
                    return null;
                }
            }
            return graph.BufferCache.FindBuffer(pin);
        }
        public UPgcNodeBase GetInputNode(UPgcGraph graph, int index, System.Type retType = null)
        {
            var linker = graph.FindInLinkerSingle(Inputs[index]);
            if (linker == null)
                return null;
            return linker.OutNode as UPgcNodeBase;
        }
        [Rtti.Meta]
        public UPgcNodeBase GetInputNodeByName(UPgcGraph graph, string pinName,
            [Rtti.MetaParameter(FilterType = typeof(UPgcNodeBase),
            ConvertOutArguments = Rtti.MetaParameterAttribute.EArgumentFilter.R)]
            System.Type retType = null)
        {
            var pin = this.FindPinIn(pinName);
            if (pin == null)
                return null;
            var linker = graph.FindInLinkerSingle(pin);
            if (linker == null)
                return null;
            return linker.OutNode as UPgcNodeBase;
        }
        [Rtti.Meta]
        public UPgcNodeBase GetInputNode(UPgcGraph graph, PinIn pin)
        {
            var linker = graph.FindInLinkerSingle(pin);
            if (linker == null)
                return null;
            return linker.OutNode as UPgcNodeBase;
        }
        #endregion

        #region PG
        [Browsable(false)]
        public bool IsPropertyVisibleDirty { get; set; } = false;
        public virtual void GetProperties(ref EGui.Controls.PropertyGrid.CustomPropertyDescriptorCollection collection, bool parentIsValueType)
        {
            var thisType = Rtti.TtTypeDesc.TypeOf(this.GetType());
            var pros = System.ComponentModel.TypeDescriptor.GetProperties(this);

            //collection.InitValue(this,  pros, parentIsValueType);
            foreach (PropertyDescriptor prop in pros)
            {
                //var p = this.GetType().GetProperty(prop.Name);
                //if (p == null)
                //    continue;
                if (prop.Name != "Name" && prop.Name != "NodeType" &&
                    prop.ComponentType != typeof(UPgcNodeBase) && !prop.ComponentType.IsSubclassOf(typeof(UPgcNodeBase)))
                {
                    continue;
                }
                var proDesc = EGui.Controls.PropertyGrid.PropertyCollection.PropertyDescPool.QueryObjectSync();
                proDesc.InitValue(this, thisType, prop, parentIsValueType);
                if (!proDesc.IsBrowsable)
                {
                    proDesc.ReleaseObject();
                    continue;
                }
                collection.Add(proDesc);
            }
        }

        public virtual object GetPropertyValue(string propertyName)
        {
            var proInfo = this.GetType().GetProperty(propertyName);
            if (proInfo != null)
                return proInfo.GetValue(this);
            var fieldInfo = this.GetType().GetField(propertyName);
            if (fieldInfo != null)
                return fieldInfo.GetValue(this);
            return null;
        }

        public virtual void SetPropertyValue(string propertyName, object value)
        {
            var proInfo = this.GetType().GetProperty(propertyName);
            if (proInfo != null && proInfo.CanWrite)
                proInfo.SetValue(this, value);
            var fieldInfo = this.GetType().GetField(propertyName);
            if (fieldInfo != null)
                fieldInfo.SetValue(this, value);
        }
        #endregion
    }
}
