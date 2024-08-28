﻿using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Mesh;
using EngineNS.Profiler;
using EngineNS.UI.Controls;
using EngineNS.UI.Controls.Containers;
using NPOI.OpenXmlFormats.Dml;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UI.Editor
{
    internal class Decorator_Canvas : IUIEditorDecorator
    {
        enum EDecoratorType : sbyte
        {
            None = -1,

            Size_Left_Top = 0,
            Size_Middle_Top = 1,
            Size_Right_Top = 2,
            Size_Left_Middle = 3,
            Size_Right_Middle = 4,
            Size_Left_Bottom = 5,
            Size_Middle_Bottom = 6,
            Size_Right_Bottom = 7,

            Move = 8,

            Anchor_Start,
            Anchor_Center = Anchor_Start,   // p_001
            Anchor_Top,         // p_002
            Anchor_TopRight,    // p_003
            Anchor_Right,
            Anchor_BottomRight,
            Anchor_Bottom,
            Anchor_BottomLeft,
            Anchor_Left,
            Anchor_TopLeft,
            Anchor_MTop,        // p_004
            Anchor_MRight,
            Anchor_MBottom,
            Anchor_MLeft,
            Anchor_STopRight,   // p_005
            Anchor_SBottomRight,
            Anchor_SBottomLeft,
            Anchor_STopLeft,
            Anchor_End,
        }
        EDecoratorType mCurDecoratorType = EDecoratorType.None;
        enum EAnchor_OnePoint : sbyte
        {
            Anchor_OnePointCenter = EDecoratorType.Anchor_Center,
            Anchor_OnePointTop = EDecoratorType.Anchor_Top,
            Anchor_OnePointTopRight = EDecoratorType.Anchor_TopRight,
            Anchor_OnePointRight = EDecoratorType.Anchor_Right,
            Anchor_OnePointBottomRight = EDecoratorType.Anchor_BottomRight,
            Anchor_OnePointBottom = EDecoratorType.Anchor_Bottom,
            Anchor_OnePointBottomLeft = EDecoratorType.Anchor_BottomLeft,
            Anchor_OnePointLeft = EDecoratorType.Anchor_Left,
            Anchor_OnePointTopLeft = EDecoratorType.Anchor_TopLeft,
        }
        enum EAnchor_Vertical : sbyte
        {
            Anchor_VTop = EDecoratorType.Anchor_MTop,        
            Anchor_VBottom = EDecoratorType.Anchor_MBottom,
            Anchor_VTopRight = EDecoratorType.Anchor_TopRight,
            Anchor_VBottomRight = EDecoratorType.Anchor_BottomRight,
            Anchor_VBottomLeft = EDecoratorType.Anchor_BottomLeft,
            Anchor_VTopLeft = EDecoratorType.Anchor_TopLeft,
        }
        enum EAnchor_Horizontal : sbyte
        { 
            Anchor_HRight = EDecoratorType.Anchor_MRight,
            Anchor_HLeft = EDecoratorType.Anchor_MLeft,
            Anchor_HTopRight = EDecoratorType.Anchor_TopRight,
            Anchor_HBottomRight = EDecoratorType.Anchor_BottomRight,
            Anchor_HBottomLeft = EDecoratorType.Anchor_BottomLeft,
            Anchor_HTopLeft = EDecoratorType.Anchor_TopLeft,
        }
        enum EAnchor_Rect : sbyte
        {
            Anchor_RectTopRight = EDecoratorType.Anchor_STopRight, 
            Anchor_RectBottomRight = EDecoratorType.Anchor_SBottomRight,
            Anchor_RectBottomLeft = EDecoratorType.Anchor_SBottomLeft,
            Anchor_RectTopLeft = EDecoratorType.Anchor_STopLeft,
        }


        enum EAnchorType : byte
        {
            OnePoint,
            Vertical,
            Horizontal,
            Rect,
        }
        EAnchorType mAnchorType = EAnchorType.OnePoint;
        DVector3 mDecoratorMouseDownOffset;
        DVector3 mPickPlanePos;
        Vector3 mPickPlaneNormal;

        TtMeshNode[] mOperatorNodes = new TtMeshNode[8];
        TtMeshNode[] mAnchorNodes = new TtMeshNode[EDecoratorType.Anchor_End - EDecoratorType.Anchor_Start];
        List<Vector4> mOriAnchorRects = new List<Vector4>();
        List<RectangleF> mOriDesignRects = new List<RectangleF>();
        List<Vector4> mOriAnchorPoints = new List<Vector4>();

        TtUIEditor mEditor;
        Graphics.Pipeline.Shader.TtMaterialInstance mWhiteColorMat;
        Graphics.Pipeline.Shader.TtMaterialInstance mGreenColorMat;
        List<Graphics.Pipeline.Shader.TtMaterial> mNormalAnchorMats;
        List<Graphics.Pipeline.Shader.TtMaterial> mHighLightAnchorMats;
        bool mInitialized = false;

        public bool IsDirty { get; set; } = false;

        public async Thread.Async.TtTask Initialize(TtUIEditor editor)
        {
            mEditor = editor;

            mWhiteColorMat = await TtEngine.Instance.GfxDevice.MaterialInstanceManager.CreateMaterialInstance(RName.GetRName("ui/uidecorator_white.uminst", RName.ERNameType.Engine));
            mGreenColorMat = await TtEngine.Instance.GfxDevice.MaterialInstanceManager.CreateMaterialInstance(RName.GetRName("ui/uidecorator_green.uminst", RName.ERNameType.Engine));
            mNormalAnchorMats = new List<Graphics.Pipeline.Shader.TtMaterial>();
            mNormalAnchorMats.Add(mWhiteColorMat);
            mHighLightAnchorMats = new List<Graphics.Pipeline.Shader.TtMaterial>();
            mHighLightAnchorMats.Add(mGreenColorMat);
            var meshProvider = UMeshDataProvider.MakeSphere(1.0f, 8, 8, 0xffffffff);
            var meshPrim = meshProvider.ToMesh();
            for (int i = (int)EDecoratorType.Size_Left_Top; i <= (int)EDecoratorType.Size_Right_Bottom; i++)
            {
                var mesh = new TtMesh();
                mesh.Initialize(meshPrim, new Graphics.Pipeline.Shader.TtMaterial[] { mWhiteColorMat },
                    Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                mOperatorNodes[i] = await TtMeshNode.AddMeshNode(editor.PreviewViewport.World, editor.mUINode,
                    new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.UPlacement),
                    mesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
                mOperatorNodes[i].Parent = null;
                mOperatorNodes[i].HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
            }

            for(var i=EDecoratorType.Anchor_Start; i<EDecoratorType.Anchor_End; i++)
            {
                var idx = (int)(i - EDecoratorType.Anchor_Start);
                switch(i)
                {
                    case EDecoratorType.Anchor_Center:
                        {
                            var mesh = new TtMesh();
                            await mesh.Initialize(RName.GetRName("ui/p_001.vms", RName.ERNameType.Engine), 
                                new List<Graphics.Pipeline.Shader.TtMaterial>() { mWhiteColorMat }, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                            mAnchorNodes[idx] = await TtMeshNode.AddMeshNode(editor.PreviewViewport.World, editor.mUINode,
                                new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.UPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
                            mAnchorNodes[idx].Parent = null;
                            mAnchorNodes[idx].HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                        }
                        break;
                    case EDecoratorType.Anchor_Top:
                        {
                            var mesh = new TtMesh();
                            await mesh.Initialize(RName.GetRName("ui/p_002.vms", RName.ERNameType.Engine), 
                                new List<Graphics.Pipeline.Shader.TtMaterial>() { mWhiteColorMat }, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                            mAnchorNodes[idx] = await TtMeshNode.AddMeshNode(editor.PreviewViewport.World, editor.mUINode,
                                new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.UPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
                            mAnchorNodes[idx].Parent = null;
                            mAnchorNodes[idx].HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                        }
                        break;
                    case EDecoratorType.Anchor_TopRight:
                        {
                            var mesh = new TtMesh();
                            await mesh.Initialize(RName.GetRName("ui/p_003.vms", RName.ERNameType.Engine),
                                new List<Graphics.Pipeline.Shader.TtMaterial>() { mWhiteColorMat }, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                            mAnchorNodes[idx] = await TtMeshNode.AddMeshNode(editor.PreviewViewport.World, editor.mUINode,
                                new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.UPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
                            mAnchorNodes[idx].Parent = null;
                            mAnchorNodes[idx].HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                        }
                        break;
                    case EDecoratorType.Anchor_Right:
                        {
                            var mesh = new TtMesh();
                            await mesh.Initialize(RName.GetRName("ui/p_002.vms", RName.ERNameType.Engine), 
                                new List<Graphics.Pipeline.Shader.TtMaterial>() { mWhiteColorMat }, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                            mAnchorNodes[idx] = await TtMeshNode.AddMeshNode(editor.PreviewViewport.World, editor.mUINode,
                                new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.UPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.RotationAxis(Vector3.UnitZ, -MathF.PI * 0.5f));
                            mAnchorNodes[idx].Parent = null;
                            mAnchorNodes[idx].HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                        }
                        break;
                    case EDecoratorType.Anchor_BottomRight:
                        {
                            var mesh = new TtMesh();
                            await mesh.Initialize(RName.GetRName("ui/p_003.vms", RName.ERNameType.Engine), 
                                new List<Graphics.Pipeline.Shader.TtMaterial> { mWhiteColorMat }, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                            mAnchorNodes[idx] = await TtMeshNode.AddMeshNode(editor.PreviewViewport.World, editor.mUINode,
                                new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.UPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.RotationAxis(Vector3.UnitZ, -MathF.PI * 0.5f));
                            mAnchorNodes[idx].Parent = null;
                            mAnchorNodes[idx].HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                        }
                        break;
                    case EDecoratorType.Anchor_Bottom:
                        {
                            var mesh = new TtMesh();
                            await mesh.Initialize(RName.GetRName("ui/p_002.vms", RName.ERNameType.Engine), 
                                new List<Graphics.Pipeline.Shader.TtMaterial> { mWhiteColorMat }, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                            mAnchorNodes[idx] = await TtMeshNode.AddMeshNode(editor.PreviewViewport.World, editor.mUINode,
                                new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.UPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.RotationAxis(Vector3.UnitZ, MathF.PI));
                            mAnchorNodes[idx].Parent = null;
                            mAnchorNodes[idx].HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                        }
                        break;
                    case EDecoratorType.Anchor_BottomLeft:
                        {
                            var mesh = new TtMesh();
                            await mesh.Initialize(RName.GetRName("ui/p_003.vms", RName.ERNameType.Engine),
                                new List<Graphics.Pipeline.Shader.TtMaterial> { mWhiteColorMat }, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                            mAnchorNodes[idx] = await TtMeshNode.AddMeshNode(editor.PreviewViewport.World, editor.mUINode,
                                new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.UPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.RotationAxis(Vector3.UnitZ, MathF.PI));
                            mAnchorNodes[idx].Parent = null;
                            mAnchorNodes[idx].HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                        }
                        break;
                    case EDecoratorType.Anchor_Left:
                        {
                            var mesh = new TtMesh();
                            await mesh.Initialize(RName.GetRName("ui/p_002.vms", RName.ERNameType.Engine),
                                new List<Graphics.Pipeline.Shader.TtMaterial> { mWhiteColorMat }, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                            mAnchorNodes[idx] = await TtMeshNode.AddMeshNode(editor.PreviewViewport.World, editor.mUINode,
                                new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.UPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.RotationAxis(Vector3.UnitZ, MathF.PI * 0.5f));
                            mAnchorNodes[idx].Parent = null;
                            mAnchorNodes[idx].HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                        }
                        break;
                    case EDecoratorType.Anchor_TopLeft:
                        {
                            var mesh = new TtMesh();
                            await mesh.Initialize(RName.GetRName("ui/p_003.vms", RName.ERNameType.Engine), 
                                new List<Graphics.Pipeline.Shader.TtMaterial> { mWhiteColorMat }, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                            mAnchorNodes[idx] = await TtMeshNode.AddMeshNode(editor.PreviewViewport.World, editor.mUINode,
                                new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.UPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.RotationAxis(Vector3.UnitZ, MathF.PI * 0.5f));
                            mAnchorNodes[idx].Parent = null;
                            mAnchorNodes[idx].HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                        }
                        break;
                    case EDecoratorType.Anchor_MTop:
                        {
                            var mesh = new TtMesh();
                            await mesh.Initialize(RName.GetRName("ui/p_004.vms", RName.ERNameType.Engine),
                                new List<Graphics.Pipeline.Shader.TtMaterial> { mWhiteColorMat }, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                            mAnchorNodes[idx] = await TtMeshNode.AddMeshNode(editor.PreviewViewport.World, editor.mUINode,
                                new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.UPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
                            mAnchorNodes[idx].Parent = null;
                            mAnchorNodes[idx].HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                        }
                        break;
                    case EDecoratorType.Anchor_MRight:
                        {
                            var mesh = new TtMesh();
                            await mesh.Initialize(RName.GetRName("ui/p_004.vms", RName.ERNameType.Engine),
                                new List<Graphics.Pipeline.Shader.TtMaterial> { mWhiteColorMat }, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                            mAnchorNodes[idx] = await TtMeshNode.AddMeshNode(editor.PreviewViewport.World, editor.mUINode,
                                new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.UPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.RotationAxis(Vector3.UnitZ, -MathF.PI * 0.5f));
                            mAnchorNodes[idx].Parent = null;
                            mAnchorNodes[idx].HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                        }
                        break;
                    case EDecoratorType.Anchor_MBottom:
                        {
                            var mesh = new TtMesh();
                            await mesh.Initialize(RName.GetRName("ui/p_004.vms", RName.ERNameType.Engine), 
                                new List<Graphics.Pipeline.Shader.TtMaterial> { mWhiteColorMat }, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                            mAnchorNodes[idx] = await TtMeshNode.AddMeshNode(editor.PreviewViewport.World, editor.mUINode,
                                new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.UPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.RotationAxis(Vector3.UnitZ, MathF.PI));
                            mAnchorNodes[idx].Parent = null;
                            mAnchorNodes[idx].HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                        }
                        break;
                    case EDecoratorType.Anchor_MLeft:
                        {
                            var mesh = new TtMesh();
                            await mesh.Initialize(RName.GetRName("ui/p_004.vms", RName.ERNameType.Engine), 
                                new List<Graphics.Pipeline.Shader.TtMaterial> { mWhiteColorMat }, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                            mAnchorNodes[idx] = await TtMeshNode.AddMeshNode(editor.PreviewViewport.World, editor.mUINode,
                                new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.UPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.RotationAxis(Vector3.UnitZ, MathF.PI * 0.5f));
                            mAnchorNodes[idx].Parent = null;
                            mAnchorNodes[idx].HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                        }
                        break;
                    case EDecoratorType.Anchor_STopRight:
                        {
                            var mesh = new TtMesh();
                            await mesh.Initialize(RName.GetRName("ui/p_005.vms", RName.ERNameType.Engine), 
                                new List<Graphics.Pipeline.Shader.TtMaterial> { mWhiteColorMat }, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                            mAnchorNodes[idx] = await TtMeshNode.AddMeshNode(editor.PreviewViewport.World, editor.mUINode,
                                new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.UPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
                            mAnchorNodes[idx].Parent = null;
                            mAnchorNodes[idx].HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                        }
                        break;
                    case EDecoratorType.Anchor_SBottomRight:
                        {
                            var mesh = new TtMesh();
                            await mesh.Initialize(RName.GetRName("ui/p_005.vms", RName.ERNameType.Engine), 
                                new List<Graphics.Pipeline.Shader.TtMaterial> { mWhiteColorMat }, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                            mAnchorNodes[idx] = await TtMeshNode.AddMeshNode(editor.PreviewViewport.World, editor.mUINode,
                                new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.UPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.RotationAxis(Vector3.UnitZ, -MathF.PI * 0.5f));
                            mAnchorNodes[idx].Parent = null;
                            mAnchorNodes[idx].HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                        }
                        break;
                    case EDecoratorType.Anchor_SBottomLeft:
                        {
                            var mesh = new TtMesh();
                            await mesh.Initialize(RName.GetRName("ui/p_005.vms", RName.ERNameType.Engine), 
                                new List<Graphics.Pipeline.Shader.TtMaterial> { mWhiteColorMat }, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                            mAnchorNodes[idx] = await TtMeshNode.AddMeshNode(editor.PreviewViewport.World, editor.mUINode,
                                new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.UPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.RotationAxis(Vector3.UnitZ, MathF.PI));
                            mAnchorNodes[idx].Parent = null;
                            mAnchorNodes[idx].HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                        }
                        break;
                    case EDecoratorType.Anchor_STopLeft:
                        {
                            var mesh = new TtMesh();
                            await mesh.Initialize(RName.GetRName("ui/p_005.vms", RName.ERNameType.Engine), 
                                new List<Graphics.Pipeline.Shader.TtMaterial> { mWhiteColorMat }, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                            mAnchorNodes[idx] = await TtMeshNode.AddMeshNode(editor.PreviewViewport.World, editor.mUINode,
                                new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.UPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.RotationAxis(Vector3.UnitZ, MathF.PI * 0.5f));
                            mAnchorNodes[idx].Parent = null;
                            mAnchorNodes[idx].HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                        }
                        break;
                }
            }
            mInitialized = true;
        }
        public async Thread.Async.TtTask UpdateDecorator()
        {
            for (int i = 0; i < mOperatorNodes.Length; i++)
            {
                if (mOperatorNodes[i] == null)
                    continue;
                if (mOperatorNodes[i].Parent == null)
                    continue;
                var scale = mEditor.PreviewViewport.CameraController.Camera.GetScaleWithFixSizeInScreen(mOperatorNodes[i].Placement.Position, 8);
                mOperatorNodes[i].Placement.Scale = new Vector3(scale);
            }
            for (var i = EDecoratorType.Anchor_Start; i < EDecoratorType.Anchor_End; i++)
            {
                var node = mAnchorNodes[(int)(i - EDecoratorType.Anchor_Start)];
                if (node == null)
                    continue;
                if (node.Parent == null)
                    continue;
                var scale = mEditor.PreviewViewport.CameraController.Camera.GetScaleWithFixSizeInScreen(node.Placement.Position, 8);
                node.Placement.Scale = new Vector3(scale);
            }

            if (IsDirty)
            {
                UpdateOperatorNodes(mEditor.SelectedElements[0]);
                IsDirty = false;
            }
        }

        public void UpdateOperatorNodes(TtUIElement element)
        {
            // 0 --- 1 --- 2
            // |           |
            // 3           4
            // |           |
            // 5 --- 6 --- 7
            var rootRect = element.RootUIHost.DesignRect;
            var elementRect = element.DesignRect;
            var transMat = mEditor.mUIHost.TransformedElements[element.TransformIndex].Matrix;
            mOperatorNodes[(int)EDecoratorType.Size_Left_Top].Parent = mEditor.mUINode;
            mOperatorNodes[(int)EDecoratorType.Size_Left_Top].Placement.Position = mEditor.mUINode.GetWorld().CameraOffset + Vector3.TransformCoordinate(new Vector3(elementRect.Left, rootRect.Height - elementRect.Top, 0.0f), in transMat);
            mOperatorNodes[(int)EDecoratorType.Size_Middle_Top].Parent = mEditor.mUINode;
            mOperatorNodes[(int)EDecoratorType.Size_Middle_Top].Placement.Position = mEditor.mUINode.GetWorld().CameraOffset + Vector3.TransformCoordinate(new Vector3(elementRect.Left + elementRect.Width * 0.5f, rootRect.Height - elementRect.Top, 0.0f), in transMat);
            mOperatorNodes[(int)EDecoratorType.Size_Right_Top].Parent = mEditor.mUINode;
            mOperatorNodes[(int)EDecoratorType.Size_Right_Top].Placement.Position = mEditor.mUINode.GetWorld().CameraOffset + Vector3.TransformCoordinate(new Vector3(elementRect.Right, rootRect.Height - elementRect.Top, 0.0f), in transMat);
            mOperatorNodes[(int)EDecoratorType.Size_Left_Middle].Parent = mEditor.mUINode;
            mOperatorNodes[(int)EDecoratorType.Size_Left_Middle].Placement.Position = mEditor.mUINode.GetWorld().CameraOffset + Vector3.TransformCoordinate(new Vector3(elementRect.Left, rootRect.Height - elementRect.Top - elementRect.Height * 0.5f, 0.0f), in transMat);
            mOperatorNodes[(int)EDecoratorType.Size_Right_Middle].Parent = mEditor.mUINode;
            mOperatorNodes[(int)EDecoratorType.Size_Right_Middle].Placement.Position = mEditor.mUINode.GetWorld().CameraOffset + Vector3.TransformCoordinate(new Vector3(elementRect.Right, rootRect.Height - elementRect.Top - elementRect.Height * 0.5f, 0.0f), in transMat);
            mOperatorNodes[(int)EDecoratorType.Size_Left_Bottom].Parent = mEditor.mUINode;
            mOperatorNodes[(int)EDecoratorType.Size_Left_Bottom].Placement.Position = mEditor.mUINode.GetWorld().CameraOffset + Vector3.TransformCoordinate(new Vector3(elementRect.Left, rootRect.Height - elementRect.Bottom, 0.0f), in transMat);
            mOperatorNodes[(int)EDecoratorType.Size_Middle_Bottom].Parent = mEditor.mUINode;
            mOperatorNodes[(int)EDecoratorType.Size_Middle_Bottom].Placement.Position = mEditor.mUINode.GetWorld().CameraOffset + Vector3.TransformCoordinate(new Vector3(elementRect.Left + elementRect.Width * 0.5f, rootRect.Height - elementRect.Bottom, 0.0f), in transMat);
            mOperatorNodes[(int)EDecoratorType.Size_Right_Bottom].Parent = mEditor.mUINode;
            mOperatorNodes[(int)EDecoratorType.Size_Right_Bottom].Placement.Position = mEditor.mUINode.GetWorld().CameraOffset + Vector3.TransformCoordinate(new Vector3(elementRect.Right, rootRect.Height - elementRect.Bottom, 0.0f), in transMat);

            var anchorMin = TtCanvasControl.GetAnchorMin(element);
            var anchorMax = TtCanvasControl.GetAnchorMax(element);
            if (anchorMin == anchorMax)
                mAnchorType = EAnchorType.OnePoint;
            else if (MathF.Abs(anchorMax.X - anchorMin.X) < MathHelper.Epsilon)
                mAnchorType = EAnchorType.Vertical;
            else if (MathF.Abs(anchorMax.Y - anchorMin.Y) < MathHelper.Epsilon)
                mAnchorType = EAnchorType.Horizontal;
            else
                mAnchorType = EAnchorType.Rect;
            for (var i = EDecoratorType.Anchor_Start; i < EDecoratorType.Anchor_End; i++)
            {
                var node = mAnchorNodes[(int)(i - EDecoratorType.Anchor_Start)];
                node.Parent = null;
            }

            var canvas = VisualTreeHelper.GetParent(element);
            var canvasTransMat = mEditor.mUIHost.TransformedElements[canvas.TransformIndex].Matrix;
            var canvasRect = canvas.DesignRect;
            switch (mAnchorType)
            {
                case EAnchorType.OnePoint:
                    foreach (var val in Enum.GetValues<EAnchor_OnePoint>())
                    {
                        var node = mAnchorNodes[((sbyte)val - (sbyte)EDecoratorType.Anchor_Start)];
                        node.Parent = mEditor.mUINode;
                        node.Placement.Position = mEditor.mUINode.GetWorld().CameraOffset + Vector3.TransformCoordinate(new Vector3(canvasRect.Left + canvasRect.Width * anchorMin.X, rootRect.Height - canvasRect.Top - canvasRect.Height * anchorMin.Y, 0.0f), in canvasTransMat);
                    }
                    break;
                case EAnchorType.Vertical:
                    foreach(var val in Enum.GetValues<EAnchor_Vertical>())
                    {
                        switch(val)
                        {
                            case EAnchor_Vertical.Anchor_VTop:
                            case EAnchor_Vertical.Anchor_VTopRight:
                            case EAnchor_Vertical.Anchor_VTopLeft:
                                {
                                    var node = mAnchorNodes[((sbyte)val - (sbyte)EDecoratorType.Anchor_Start)];
                                    node.Parent = mEditor.mUINode;
                                    node.Placement.Position = mEditor.mUINode.GetWorld().CameraOffset + Vector3.TransformCoordinate(new Vector3(canvasRect.Left + canvasRect.Width * anchorMin.X, rootRect.Height - canvasRect.Top - canvasRect.Height * anchorMin.Y, 0.0f), in canvasTransMat);
                                }
                                break;
                            case EAnchor_Vertical.Anchor_VBottom:
                            case EAnchor_Vertical.Anchor_VBottomRight:
                            case EAnchor_Vertical.Anchor_VBottomLeft:
                                {
                                    var node = mAnchorNodes[((sbyte)val - (sbyte)EDecoratorType.Anchor_Start)];
                                    node.Parent = mEditor.mUINode;
                                    node.Placement.Position = mEditor.mUINode.GetWorld().CameraOffset + Vector3.TransformCoordinate(new Vector3(canvasRect.Left + canvasRect.Width * anchorMax.X, rootRect.Height - canvasRect.Top - canvasRect.Height * anchorMax.Y, 0.0f), in canvasTransMat);
                                }
                                break;
                        }
                    }
                    break;
                case EAnchorType.Horizontal:
                    foreach (var val in Enum.GetValues<EAnchor_Horizontal>())
                    {
                        switch(val)
                        {
                            case EAnchor_Horizontal.Anchor_HLeft:
                            case EAnchor_Horizontal.Anchor_HTopLeft:
                            case EAnchor_Horizontal.Anchor_HBottomLeft:
                                {
                                    var node = mAnchorNodes[((sbyte)val - (sbyte)EDecoratorType.Anchor_Start)];
                                    node.Parent = mEditor.mUINode;
                                    node.Placement.Position = mEditor.mUINode.GetWorld().CameraOffset + Vector3.TransformCoordinate(new Vector3(canvasRect.Left + canvasRect.Width * anchorMin.X, rootRect.Height - canvasRect.Top - canvasRect.Height * anchorMin.Y, 0.0f), in canvasTransMat);
                                }
                                break;
                            case EAnchor_Horizontal.Anchor_HRight:
                            case EAnchor_Horizontal.Anchor_HTopRight:
                            case EAnchor_Horizontal.Anchor_HBottomRight:
                                {
                                    var node = mAnchorNodes[((sbyte)val - (sbyte)EDecoratorType.Anchor_Start)];
                                    node.Parent = mEditor.mUINode;
                                    node.Placement.Position = mEditor.mUINode.GetWorld().CameraOffset + Vector3.TransformCoordinate(new Vector3(canvasRect.Left + canvasRect.Width * anchorMax.X, rootRect.Height - canvasRect.Top - canvasRect.Height * anchorMax.Y, 0.0f), in canvasTransMat);
                                }
                                break;
                        }
                    }
                    break;
                case EAnchorType.Rect:
                    foreach (var val in Enum.GetValues<EAnchor_Rect>())
                    {
                        switch(val)
                        {
                            case EAnchor_Rect.Anchor_RectTopRight:
                                {
                                    var node = mAnchorNodes[((sbyte)val - (sbyte)EDecoratorType.Anchor_Start)];
                                    node.Parent = mEditor.mUINode;
                                    node.Placement.Position = mEditor.mUINode.GetWorld().CameraOffset + Vector3.TransformCoordinate(new Vector3(canvasRect.Left + canvasRect.Width * anchorMax.X, rootRect.Height - canvasRect.Top - canvasRect.Height * anchorMin.Y, 0.0f), in canvasTransMat);
                                }
                                break;
                            case EAnchor_Rect.Anchor_RectBottomRight:
                                {
                                    var node = mAnchorNodes[((sbyte)val - (sbyte)EDecoratorType.Anchor_Start)];
                                    node.Parent = mEditor.mUINode;
                                    node.Placement.Position = mEditor.mUINode.GetWorld().CameraOffset + Vector3.TransformCoordinate(new Vector3(canvasRect.Left + canvasRect.Width * anchorMax.X, rootRect.Height - canvasRect.Top - canvasRect.Height * anchorMax.Y, 0.0f), in canvasTransMat);
                                }
                                break;
                            case EAnchor_Rect.Anchor_RectBottomLeft:
                                {
                                    var node = mAnchorNodes[((sbyte)val - (sbyte)EDecoratorType.Anchor_Start)];
                                    node.Parent = mEditor.mUINode;
                                    node.Placement.Position = mEditor.mUINode.GetWorld().CameraOffset + Vector3.TransformCoordinate(new Vector3(canvasRect.Left + canvasRect.Width * anchorMin.X, rootRect.Height - canvasRect.Top - canvasRect.Height * anchorMax.Y, 0.0f), in canvasTransMat);
                                }
                                break;
                            case EAnchor_Rect.Anchor_RectTopLeft:
                                {
                                    var node = mAnchorNodes[((sbyte)val - (sbyte)EDecoratorType.Anchor_Start)];
                                    node.Parent = mEditor.mUINode;
                                    node.Placement.Position = mEditor.mUINode.GetWorld().CameraOffset + Vector3.TransformCoordinate(new Vector3(canvasRect.Left + canvasRect.Width * anchorMin.X, rootRect.Height - canvasRect.Top - canvasRect.Height * anchorMin.Y, 0.0f), in canvasTransMat);
                                }
                                break;
                        }
                    }
                    break;
            }
        }

        public void ProcessSelectElementDecorator()
        {
            ClearDecorator();
            if (mEditor.SelectedElements.Count == 1)
            {
                UpdateOperatorNodes(mEditor.SelectedElements[0]);
            }
            UpdateOriAnchorDatas();
        }
        public void ClearDecorator()
        {
            for (int i = 0; i < mOperatorNodes.Length; i++)
            {
                mOperatorNodes[i].Parent = null;
            }
            for (var i = EDecoratorType.Anchor_Start; i < EDecoratorType.Anchor_End; i++)
            {
                if (mAnchorNodes[(int)(i - EDecoratorType.Anchor_Start)] == null)
                    continue;
                mAnchorNodes[(int)(i - EDecoratorType.Anchor_Start)].Parent = null;
            }
        }

        void UpdateOriAnchorDatas()
        {
            mOriAnchorRects.Clear();
            mOriAnchorPoints.Clear();
            for (int i = 0; i < mEditor.SelectedElements.Count; i++)
            {
                Vector4 rect;
                rect.X = TtCanvasControl.GetAnchorRectX(mEditor.SelectedElements[i]);
                rect.Y = TtCanvasControl.GetAnchorRectY(mEditor.SelectedElements[i]);
                rect.Z = TtCanvasControl.GetAnchorRectZ(mEditor.SelectedElements[i]);
                rect.W = TtCanvasControl.GetAnchorRectW(mEditor.SelectedElements[i]);
                mOriAnchorRects.Add(rect);
                var min = TtCanvasControl.GetAnchorMin(mEditor.SelectedElements[i]);
                var max = TtCanvasControl.GetAnchorMax(mEditor.SelectedElements[i]);
                mOriAnchorPoints.Add(new Vector4(min.X, min.Y, max.X, max.Y));
                mOriDesignRects.Add(mEditor.SelectedElements[i].DesignRect);
            }
        }

        bool PickPlanePos(int x, int y, in DVector3 planePos, in EngineNS.Vector3 planeNormal, out DVector3 resultPos)
        {
            resultPos = DVector3.Zero;
            EngineNS.Vector3 pickRay = -EngineNS.Vector3.UnitY;
            var pos = mEditor.PreviewViewport.Window2Viewport(new Vector2(x, y));
            var camera = mEditor.PreviewViewport.CameraController.Camera;
            var pickResult = camera.GetPickRay(ref pickRay, pos.X, pos.Y, mEditor.PreviewViewport.ClientSize.X, mEditor.PreviewViewport.ClientSize.Y);
            if (pickResult <= 0)
                return false;
            return Plane.PickPlanePos(pickRay, camera.GetLocalPosition(), camera.GetMatrixStartPosition(), x, y, planePos, planeNormal, out resultPos);
        }

        float CalculateAnchorLeft(in DVector3 offset, float inWidth, in Vector2 min, in Vector2 max, TtUIElement element, int elementIndex)
        {
            var offsetX = offset.X / inWidth;
            var x = MathF.Min(max.X, MathF.Max(0.0f, (float)(mOriAnchorPoints[elementIndex].X + offsetX)));
            //TtCanvasControl.SetAnchorMin(element, new Vector2(x, min.Y));
            var realOffsetX = (x - mOriAnchorPoints[elementIndex].X) * inWidth;
            float width = 0.0f;
            switch(mCurDecoratorType)
            {
                case EDecoratorType.Anchor_Left:
                case EDecoratorType.Anchor_TopLeft:
                case EDecoratorType.Anchor_BottomLeft:
                    {
                        width = mOriAnchorRects[elementIndex].Z;
                        if ((max.X - x) > MathHelper.Epsilon)
                            width = max.X - x - mOriAnchorRects[elementIndex].X - mOriAnchorRects[elementIndex].Z;
                    }
                    break;
                case EDecoratorType.Anchor_MLeft:
                case EDecoratorType.Anchor_STopLeft:
                case EDecoratorType.Anchor_SBottomLeft:
                    {
                        width = mOriAnchorRects[elementIndex].Z;
                        if ((max.X - x) <= MathHelper.Epsilon)
                            width = mOriDesignRects[elementIndex].Width;
                    }
                    break;
            }
            TtCanvasControl.SetAnchorRectX(element, (float)(mOriAnchorRects[elementIndex].X - realOffsetX));
            TtCanvasControl.SetAnchorRectZ(element, width);
            return x;
        }
        float CalculateAnchorRight(in DVector3 offset, float inWidth, in Vector2 min, in Vector2 max, TtUIElement element, int elementIndex)
        {
            var offsetX = offset.X / inWidth;
            var x = MathF.Max(min.X, MathF.Min(1.0f, (float)(mOriAnchorPoints[elementIndex].Z + offsetX)));
            //TtCanvasControl.SetAnchorMax(element, new Vector2(x, max.Y));
            var realOffsetX = (x - mOriAnchorPoints[elementIndex].Z) * inWidth;
            float width = 0.0f;
            switch(mCurDecoratorType)
            {
                case EDecoratorType.Anchor_Right:
                case EDecoratorType.Anchor_TopRight:
                case EDecoratorType.Anchor_BottomRight:
                    {
                        width = mOriAnchorRects[elementIndex].Z;
                        if ((x - min.X) > MathHelper.Epsilon)
                            width = x - min.X - mOriAnchorRects[elementIndex].X - mOriAnchorRects[elementIndex].Z + realOffsetX;
                    }
                    break;
                case EDecoratorType.Anchor_MRight:
                case EDecoratorType.Anchor_STopRight:
                case EDecoratorType.Anchor_SBottomRight:
                    {
                        width = mOriAnchorRects[elementIndex].Z + realOffsetX;
                        if ((x - min.X) <= MathHelper.Epsilon)
                            width = mOriDesignRects[elementIndex].Width;
                    }
                    break;
            }
            TtCanvasControl.SetAnchorRectZ(element, width);
            return x;
        }
        float CalculateAnchorTop(in DVector3 offset, float inHeight, in Vector2 min, in Vector2 max, TtUIElement element, int elementIndex)
        {
            var offsetY = offset.Y / inHeight;
            var y = MathF.Min(max.Y, MathF.Max(0.0f, (float)(mOriAnchorPoints[elementIndex].Y - offsetY)));
            //TtCanvasControl.SetAnchorMin(element, new Vector2(min.X, y));
            var realOffsetY = (mOriAnchorPoints[elementIndex].Y - y) * inHeight;
            float height = 0.0f;
            switch(mCurDecoratorType)
            {
                case EDecoratorType.Anchor_Top:
                case EDecoratorType.Anchor_TopRight:
                case EDecoratorType.Anchor_TopLeft:
                    {
                        height = mOriAnchorRects[elementIndex].W;
                        if ((max.Y - y) > MathHelper.Epsilon)
                            height = max.Y - y - mOriAnchorRects[elementIndex].Y - mOriAnchorRects[elementIndex].W;
                    }
                    break;
                case EDecoratorType.Anchor_MTop:
                case EDecoratorType.Anchor_STopRight:
                case EDecoratorType.Anchor_STopLeft:
                    {
                        height = mOriAnchorRects[elementIndex].W;
                        if ((max.Y - y) <= MathHelper.Epsilon)
                            height = mOriDesignRects[elementIndex].Height;
                    }
                    break;
            }
            TtCanvasControl.SetAnchorRectY(element, (float)(mOriAnchorRects[elementIndex].Y + realOffsetY));
            TtCanvasControl.SetAnchorRectW(element, height);
            return y;
        }
        float CalculateAnchorBottom(in DVector3 offset, float inHeight, in Vector2 min, in Vector2 max, TtUIElement element, int elementIndex)
        {
            var offsetY = offset.Y / inHeight;
            var y = MathF.Max(min.Y, MathF.Min(1.0f, (float)(mOriAnchorPoints[elementIndex].W - offsetY)));
            var realOffsetY = (mOriAnchorPoints[elementIndex].W - y) * inHeight;
            float height = 0.0f;
            switch(mCurDecoratorType)
            {
                case EDecoratorType.Anchor_Bottom:
                case EDecoratorType.Anchor_BottomRight:
                case EDecoratorType.Anchor_BottomLeft:
                    {
                        height = mOriAnchorRects[elementIndex].W;
                        if ((y - min.Y) > MathHelper.Epsilon)
                            height = y - min.Y - mOriAnchorRects[elementIndex].Y - mOriAnchorRects[elementIndex].W - realOffsetY;
                    }
                    break;
                case EDecoratorType.Anchor_MBottom:
                case EDecoratorType.Anchor_SBottomRight:
                case EDecoratorType.Anchor_SBottomLeft:
                    {
                        height = mOriAnchorRects[elementIndex].W - realOffsetY;
                        if ((y - min.Y) <= MathHelper.Epsilon)
                            height = mOriDesignRects[elementIndex].Height;
                    }
                    break;
            }
            TtCanvasControl.SetAnchorRectW(element, height);
            //TtCanvasControl.SetAnchorMax(element, new Vector2(x, y));
            return y;
        }
        TtMeshNode mCurrentPointAtAnchor;
        public void DecoratorEventProcess(in Bricks.Input.Event e)
        {
            if (!mInitialized)
                return;

            switch (e.Type)
            {
                case Bricks.Input.EventType.MOUSEMOTION:
                    {
                        switch (mCurDecoratorType)
                        {
                            case EDecoratorType.None:
                                if (mEditor.HitProxyNode != null)
                                {
                                    var delta = mEditor.PreviewViewport.WindowPos - mEditor.PreviewViewport.ViewportPos;
                                    var mousePt = new Vector2(e.MouseButton.X - delta.X, e.MouseButton.Y - delta.Y);
                                    var proxy = mEditor.HitProxyNode.GetHitproxy((uint)mousePt.X, (uint)mousePt.Y);
                                    if (proxy == mOperatorNodes[(int)EDecoratorType.Size_Left_Top] ||
                                        proxy == mOperatorNodes[(int)EDecoratorType.Size_Right_Bottom])
                                        mEditor.mMouseCursor = ImGuiMouseCursor_.ImGuiMouseCursor_ResizeNWSE;
                                    else if (proxy == mOperatorNodes[(int)EDecoratorType.Size_Middle_Top] ||
                                             proxy == mOperatorNodes[(int)EDecoratorType.Size_Middle_Bottom])
                                        mEditor.mMouseCursor = ImGuiMouseCursor_.ImGuiMouseCursor_ResizeNS;
                                    else if (proxy == mOperatorNodes[(int)EDecoratorType.Size_Right_Top] ||
                                             proxy == mOperatorNodes[(int)EDecoratorType.Size_Left_Bottom])
                                        mEditor.mMouseCursor = ImGuiMouseCursor_.ImGuiMouseCursor_ResizeNESW;
                                    else if (proxy == mOperatorNodes[(int)EDecoratorType.Size_Right_Middle] ||
                                             proxy == mOperatorNodes[(int)EDecoratorType.Size_Left_Middle])
                                        mEditor.mMouseCursor = ImGuiMouseCursor_.ImGuiMouseCursor_ResizeEW;
                                    else if (mEditor.SelectedElements.Contains(mEditor.CurrentPointAtElement))
                                        mEditor.mMouseCursor = ImGuiMouseCursor_.ImGuiMouseCursor_ResizeAll;
                                    else
                                    {
                                        mEditor.mMouseCursor = ImGuiMouseCursor_.ImGuiMouseCursor_Arrow;
                                        bool bFind = false;
                                        for(int i=0; i< mAnchorNodes.Length; i++)
                                        {
                                            if((proxy == mAnchorNodes[i]) && (mAnchorNodes[i] != mCurrentPointAtAnchor))
                                            {
                                                var mesh = mAnchorNodes[i].Mesh;
                                                mesh.UpdateMesh(0, mesh.MaterialMesh.SubMeshes[0].Mesh, mHighLightAnchorMats);
                                                mCurrentPointAtAnchor = mAnchorNodes[i];
                                                bFind = true;
                                                Log.WriteLine<Profiler.TtGraphicsGategory>(ELogTag.Info, "Set current point at anchor " + i);
                                                break;
                                            }
                                        }
                                        if(!bFind)
                                        {
                                            if (mCurrentPointAtAnchor != null)
                                            {
                                                mCurrentPointAtAnchor.Mesh.UpdateMesh(0, mCurrentPointAtAnchor.Mesh.MaterialMesh.SubMeshes[0].Mesh, mNormalAnchorMats);
                                            }
                                            mCurrentPointAtAnchor = null;
                                            Log.WriteLine<Profiler.TtGraphicsGategory>(ELogTag.Info, "clear current point at anchor");
                                        }
                                    }
                                }
                                break;
                            case EDecoratorType.Size_Left_Top:
                                {
                                    if(e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
                                    {
                                        DVector3 pickPos;
                                        PickPlanePos(e.MouseButton.X, e.MouseButton.Y, mPickPlanePos, mPickPlaneNormal, out pickPos);
                                        var pos = pickPos - mDecoratorMouseDownOffset;
                                        mOperatorNodes[(int)mCurDecoratorType].Placement.Position = pos;

                                        var element = mEditor.SelectedElements[0];
                                        var rootRect = element.RootUIHost.DesignRect;
                                        var elementRect = element.DesignRect;
                                        var transInvMat = mEditor.mUIHost.TransformedElements[element.TransformIndex].InvMatrix;
                                        var oriPos = Vector3.TransformCoordinate(new Vector3(pos), transInvMat);

                                        var x = TtCanvasControl.GetAnchorRectX(element);
                                        var width = TtCanvasControl.GetAnchorRectZ(element);
                                        var deltaX = oriPos.X - elementRect.X;
                                        if(width - deltaX < 0)
                                            deltaX = width;
                                        x += deltaX;
                                        width -= deltaX;
                                        TtCanvasControl.SetAnchorRectX(element, x);
                                        TtCanvasControl.SetAnchorRectZ(element, width);

                                        var y = TtCanvasControl.GetAnchorRectY(element);
                                        var height = TtCanvasControl.GetAnchorRectW(element);
                                        var deltaY = (rootRect.Height - oriPos.Y) - elementRect.Y;
                                        if (height - deltaY < 0)
                                            deltaY = height;
                                        y += deltaY;
                                        height -= deltaY;
                                        TtCanvasControl.SetAnchorRectY(element, y);
                                        TtCanvasControl.SetAnchorRectW(element, height);
                                    }
                                }
                                break;
                            case EDecoratorType.Size_Middle_Top:
                                {
                                    if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
                                    {
                                        DVector3 pickPos;
                                        PickPlanePos(e.MouseButton.X, e.MouseButton.Y, mPickPlanePos, mPickPlaneNormal, out pickPos);
                                        var pos = pickPos - mDecoratorMouseDownOffset;
                                        mOperatorNodes[(int)mCurDecoratorType].Placement.Position = pos;

                                        var element = mEditor.SelectedElements[0];
                                        var rootRect = element.RootUIHost.DesignRect;
                                        var elementRect = element.DesignRect;
                                        var transInvMat = mEditor.mUIHost.TransformedElements[element.TransformIndex].InvMatrix;
                                        var oriPos = Vector3.TransformCoordinate(new Vector3(pos), transInvMat);

                                        var y = TtCanvasControl.GetAnchorRectY(element);
                                        var height = TtCanvasControl.GetAnchorRectW(element);
                                        var deltaY = (rootRect.Height - oriPos.Y) - elementRect.Y;
                                        if (height - deltaY < 0)
                                            deltaY = height;
                                        y += deltaY;
                                        height -= deltaY;
                                        TtCanvasControl.SetAnchorRectY(element, y);
                                        TtCanvasControl.SetAnchorRectW(element, height);
                                    }
                                }
                                break;
                            case EDecoratorType.Size_Right_Top:
                                {
                                    if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
                                    {
                                        DVector3 pickPos;
                                        PickPlanePos(e.MouseButton.X, e.MouseButton.Y, mPickPlanePos, mPickPlaneNormal, out pickPos);
                                        var pos = pickPos - mDecoratorMouseDownOffset;
                                        mOperatorNodes[(int)mCurDecoratorType].Placement.Position = pos;

                                        var element = mEditor.SelectedElements[0];
                                        var rootRect = element.RootUIHost.DesignRect;
                                        var elementRect = element.DesignRect;
                                        var transInvMat = mEditor.mUIHost.TransformedElements[element.TransformIndex].InvMatrix;
                                        var oriPos = Vector3.TransformCoordinate(new Vector3(pos), transInvMat);

                                        var width = MathF.Max(oriPos.X - elementRect.X, 0.0f);
                                        TtCanvasControl.SetAnchorRectZ(element, width);

                                        var y = TtCanvasControl.GetAnchorRectY(element);
                                        var height = TtCanvasControl.GetAnchorRectW(element);
                                        var deltaY = (rootRect.Height - oriPos.Y) - elementRect.Y;
                                        if (height - deltaY < 0)
                                            deltaY = height;
                                        y += deltaY;
                                        height -= deltaY;
                                        TtCanvasControl.SetAnchorRectY(element, y);
                                        TtCanvasControl.SetAnchorRectW(element, height);
                                    }
                                }
                                break;
                            case EDecoratorType.Size_Left_Middle:
                                {
                                    if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
                                    {
                                        DVector3 pickPos;
                                        PickPlanePos(e.MouseButton.X, e.MouseButton.Y, mPickPlanePos, mPickPlaneNormal, out pickPos);
                                        var pos = pickPos - mDecoratorMouseDownOffset;
                                        mOperatorNodes[(int)mCurDecoratorType].Placement.Position = pos;

                                        var element = mEditor.SelectedElements[0];
                                        var rootRect = element.RootUIHost.DesignRect;
                                        var elementRect = element.DesignRect;
                                        var transInvMat = mEditor.mUIHost.TransformedElements[element.TransformIndex].InvMatrix;
                                        var oriPos = Vector3.TransformCoordinate(new Vector3(pos), transInvMat);

                                        var x = TtCanvasControl.GetAnchorRectX(element);
                                        var width = TtCanvasControl.GetAnchorRectZ(element);
                                        var deltaX = oriPos.X - elementRect.X;
                                        if (width - deltaX < 0)
                                            deltaX = width;
                                        x += deltaX;
                                        width -= deltaX;
                                        TtCanvasControl.SetAnchorRectX(element, x);
                                        TtCanvasControl.SetAnchorRectZ(element, width);
                                    }
                                }
                                break;
                            case EDecoratorType.Size_Right_Middle:
                                {
                                    if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
                                    {
                                        DVector3 pickPos;
                                        PickPlanePos(e.MouseButton.X, e.MouseButton.Y, mPickPlanePos, mPickPlaneNormal, out pickPos);
                                        var pos = pickPos - mDecoratorMouseDownOffset;
                                        mOperatorNodes[(int)mCurDecoratorType].Placement.Position = pos;

                                        var element = mEditor.SelectedElements[0];
                                        var rootRect = element.RootUIHost.DesignRect;
                                        var elementRect = element.DesignRect;
                                        var transInvMat = mEditor.mUIHost.TransformedElements[element.TransformIndex].InvMatrix;
                                        var oriPos = Vector3.TransformCoordinate(new Vector3(pos), transInvMat);

                                        var width = MathF.Max(oriPos.X - elementRect.X, 0.0f);
                                        TtCanvasControl.SetAnchorRectZ(element, width);
                                    }
                                }
                                break;
                            case EDecoratorType.Size_Left_Bottom:
                                {
                                    if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
                                    {
                                        DVector3 pickPos;
                                        PickPlanePos(e.MouseButton.X, e.MouseButton.Y, mPickPlanePos, mPickPlaneNormal, out pickPos);
                                        var pos = pickPos - mDecoratorMouseDownOffset;
                                        mOperatorNodes[(int)mCurDecoratorType].Placement.Position = pos;

                                        var element = mEditor.SelectedElements[0];
                                        var rootRect = element.RootUIHost.DesignRect;
                                        var elementRect = element.DesignRect;
                                        var transInvMat = mEditor.mUIHost.TransformedElements[element.TransformIndex].InvMatrix;
                                        var oriPos = Vector3.TransformCoordinate(new Vector3(pos), transInvMat);

                                        var x = TtCanvasControl.GetAnchorRectX(element);
                                        var width = TtCanvasControl.GetAnchorRectZ(element);
                                        var deltaX = oriPos.X - elementRect.X;
                                        if (width - deltaX < 0)
                                            deltaX = width;
                                        x += deltaX;
                                        width -= deltaX;
                                        TtCanvasControl.SetAnchorRectX(element, x);
                                        TtCanvasControl.SetAnchorRectZ(element, width);

                                        var height = MathF.Max((rootRect.Height - oriPos.Y) - elementRect.Y, 0.0f);
                                        TtCanvasControl.SetAnchorRectW(element, height);
                                    }
                                }
                                break;
                            case EDecoratorType.Size_Middle_Bottom:
                                {
                                    if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
                                    {
                                        DVector3 pickPos;
                                        PickPlanePos(e.MouseButton.X, e.MouseButton.Y, mPickPlanePos, mPickPlaneNormal, out pickPos);
                                        var pos = pickPos - mDecoratorMouseDownOffset;
                                        mOperatorNodes[(int)mCurDecoratorType].Placement.Position = pos;

                                        var element = mEditor.SelectedElements[0];
                                        var rootRect = element.RootUIHost.DesignRect;
                                        var elementRect = element.DesignRect;
                                        var transInvMat = mEditor.mUIHost.TransformedElements[element.TransformIndex].InvMatrix;
                                        var oriPos = Vector3.TransformCoordinate(new Vector3(pos), transInvMat);

                                        var height = MathF.Max((rootRect.Height - oriPos.Y) - elementRect.Y, 0.0f);
                                        TtCanvasControl.SetAnchorRectW(element, height);
                                    }
                                }
                                break;
                            case EDecoratorType.Size_Right_Bottom:
                                {
                                    if (e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
                                    {
                                        DVector3 pickPos;
                                        PickPlanePos(e.MouseButton.X, e.MouseButton.Y, mPickPlanePos, mPickPlaneNormal, out pickPos);
                                        var pos = pickPos - mDecoratorMouseDownOffset;
                                        mOperatorNodes[(int)mCurDecoratorType].Placement.Position = pos;

                                        var element = mEditor.SelectedElements[0];
                                        var rootRect = element.RootUIHost.DesignRect;
                                        var elementRect = element.DesignRect;
                                        var transInvMat = mEditor.mUIHost.TransformedElements[element.TransformIndex].InvMatrix;
                                        var oriPos = Vector3.TransformCoordinate(new Vector3(pos), transInvMat);

                                        var width = MathF.Max(oriPos.X - elementRect.X, 0.0f);
                                        TtCanvasControl.SetAnchorRectZ(element, width);

                                        var height = MathF.Max((rootRect.Height - oriPos.Y) - elementRect.Y, 0.0f);
                                        TtCanvasControl.SetAnchorRectW(element, height);
                                    }
                                }
                                break;
                            case EDecoratorType.Move:
                                {
                                    if(e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
                                    {
                                        DVector3 pickPos;
                                        PickPlanePos(e.MouseButton.X, e.MouseButton.Y, mPickPlanePos, mPickPlaneNormal, out pickPos);
                                        var pos = pickPos - mDecoratorMouseDownOffset;
                                        var offset = pos - mPickPlanePos;

                                        for(int i=0; i<mEditor.SelectedElements.Count; i++)
                                        {
                                            TtCanvasControl.SetAnchorRectX(mEditor.SelectedElements[i], (float)(mOriAnchorRects[i].X + offset.X));
                                            TtCanvasControl.SetAnchorRectY(mEditor.SelectedElements[i], (float)(mOriAnchorRects[i].Y - offset.Y));
                                        }
                                    }
                                }
                                break;
                            case EDecoratorType.Anchor_Center:
                                {
                                    if(e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
                                    {
                                        DVector3 pickPos;
                                        PickPlanePos(e.MouseButton.X, e.MouseButton.Y, mPickPlanePos, mPickPlaneNormal, out pickPos);
                                        var pos = pickPos - mDecoratorMouseDownOffset;
                                        var offset = pos - mPickPlanePos;

                                        for(int i=0; i<mEditor.SelectedElements.Count; i++)
                                        {
                                            var canvas = VisualTreeHelper.GetParent(mEditor.SelectedElements[i]);
                                            var offsetX = offset.X / canvas.DesignRect.Width;
                                            var offsetY = offset.Y / canvas.DesignRect.Height;
                                            var x = MathF.Min(1.0f, MathF.Max(0.0f, (float)(mOriAnchorPoints[i].X + offsetX)));
                                            var y = MathF.Min(1.0f, MathF.Max(0.0f, (float)(mOriAnchorPoints[i].Y - offsetY)));
                                            TtCanvasControl.SetAnchorMin(mEditor.SelectedElements[i], new Vector2(x, y));
                                            TtCanvasControl.SetAnchorMax(mEditor.SelectedElements[i], new Vector2(x, y));
                                            var realOffsetX = (x - mOriAnchorPoints[i].X) * canvas.DesignRect.Width;
                                            var realOffsetY = (mOriAnchorPoints[i].Y - y) * canvas.DesignRect.Height;
                                            TtCanvasControl.SetAnchorRectX(mEditor.SelectedElements[i], (float)(mOriAnchorRects[i].X - realOffsetX));
                                            TtCanvasControl.SetAnchorRectY(mEditor.SelectedElements[i], (float)(mOriAnchorRects[i].Y + realOffsetY));
                                        }
                                    }
                                }
                                break;
                            case EDecoratorType.Anchor_MTop:
                            case EDecoratorType.Anchor_Top:
                                {
                                    if(e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
                                    {
                                        DVector3 pickPos;
                                        PickPlanePos(e.MouseButton.X, e.MouseButton.Y, mPickPlanePos, mPickPlaneNormal, out pickPos);
                                        var pos = pickPos - mDecoratorMouseDownOffset;
                                        var offset = pos - mPickPlanePos;

                                        for(int i=0; i<mEditor.SelectedElements.Count; i++)
                                        {
                                            var element = mEditor.SelectedElements[i];
                                            var canvas = VisualTreeHelper.GetParent(element);
                                            var min = TtCanvasControl.GetAnchorMin(element);
                                            var max = TtCanvasControl.GetAnchorMax(element);
                                            var y = CalculateAnchorTop(offset, canvas.DesignRect.Height, min, max, element, i);
                                            TtCanvasControl.SetAnchorMin(element, new Vector2(min.X, y));
                                        }
                                    }
                                }
                                break;
                            case EDecoratorType.Anchor_STopRight:
                            case EDecoratorType.Anchor_TopRight:
                                {
                                    if(e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
                                    {
                                        DVector3 pickPos;
                                        PickPlanePos(e.MouseButton.X, e.MouseButton.Y, mPickPlanePos, mPickPlaneNormal, out pickPos);
                                        var pos = pickPos - mDecoratorMouseDownOffset;
                                        var offset = pos - mPickPlanePos;

                                        for(int i=0; i<mEditor.SelectedElements.Count; i++)
                                        {
                                            var element = mEditor.SelectedElements[i];
                                            var canvas = VisualTreeHelper.GetParent(element);
                                            var min = TtCanvasControl.GetAnchorMin(element);
                                            var max = TtCanvasControl.GetAnchorMax(element);
                                            var y = CalculateAnchorTop(offset, canvas.DesignRect.Height, min, max, element, i);
                                            var x = CalculateAnchorRight(offset, canvas.DesignRect.Width, min, max, element, i);
                                            TtCanvasControl.SetAnchorMin(element, new Vector2(min.X, y));
                                            TtCanvasControl.SetAnchorMax(element, new Vector2(x, max.Y));
                                        }
                                    }
                                }
                                break;
                            case EDecoratorType.Anchor_MRight:
                            case EDecoratorType.Anchor_Right:
                                {
                                    if(e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
                                    {
                                        DVector3 pickPos;
                                        PickPlanePos(e.MouseButton.X, e.MouseButton.Y, mPickPlanePos, mPickPlaneNormal, out pickPos);
                                        var pos = pickPos - mDecoratorMouseDownOffset;
                                        var offset = pos - mPickPlanePos;

                                        for(int i=0; i<mEditor.SelectedElements.Count; i++)
                                        {
                                            var element = mEditor.SelectedElements[i];
                                            var canvas = VisualTreeHelper.GetParent(element);
                                            var min = TtCanvasControl.GetAnchorMin(element);
                                            var max = TtCanvasControl.GetAnchorMax(element);
                                            var x = CalculateAnchorRight(offset, canvas.DesignRect.Width, min, max, element, i);
                                            TtCanvasControl.SetAnchorMax(element, new Vector2(x, max.Y));
                                        }
                                    }
                                }
                                break;
                            case EDecoratorType.Anchor_SBottomRight:
                            case EDecoratorType.Anchor_BottomRight:
                                {
                                    if(e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
                                    {
                                        DVector3 pickPos;
                                        PickPlanePos(e.MouseButton.X, e.MouseButton.Y, mPickPlanePos, mPickPlaneNormal, out pickPos);
                                        var pos = pickPos - mDecoratorMouseDownOffset;
                                        var offset = pos - mPickPlanePos;

                                        for(int i=0; i<mEditor.SelectedElements.Count; i++)
                                        {
                                            var element = mEditor.SelectedElements[i];
                                            var canvas = VisualTreeHelper.GetParent(element);
                                            var min = TtCanvasControl.GetAnchorMin(element);
                                            var max = TtCanvasControl.GetAnchorMax(element);
                                            var x = CalculateAnchorRight(offset, canvas.DesignRect.Width, min, max, element, i);
                                            var y = CalculateAnchorBottom(offset, canvas.DesignRect.Height, min, max, element, i);
                                            TtCanvasControl.SetAnchorMax(element, new Vector2(x, y));
                                        }
                                    }
                                }
                                break;
                            case EDecoratorType.Anchor_MBottom:
                            case EDecoratorType.Anchor_Bottom:
                                {
                                    if(e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
                                    {
                                        DVector3 pickPos;
                                        PickPlanePos(e.MouseButton.X, e.MouseButton.Y, mPickPlanePos, mPickPlaneNormal, out pickPos);
                                        var pos = pickPos - mDecoratorMouseDownOffset;
                                        var offset = pos - mPickPlanePos;

                                        for(int i=0; i<mEditor.SelectedElements.Count; i++)
                                        {
                                            var element = mEditor.SelectedElements[i];
                                            var canvas = VisualTreeHelper.GetParent(element);
                                            var min = TtCanvasControl.GetAnchorMin(element);
                                            var max = TtCanvasControl.GetAnchorMax(element);
                                            var y = CalculateAnchorBottom(offset, canvas.DesignRect.Height, min, max, element, i);
                                            TtCanvasControl.SetAnchorMax(element, new Vector2(max.X, y));
                                        }
                                    }
                                }
                                break;
                            case EDecoratorType.Anchor_SBottomLeft:
                            case EDecoratorType.Anchor_BottomLeft:
                                {
                                    if(e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
                                    {
                                        DVector3 pickPos;
                                        PickPlanePos(e.MouseButton.X, e.MouseButton.Y, mPickPlanePos, mPickPlaneNormal, out pickPos);
                                        var pos = pickPos - mDecoratorMouseDownOffset;
                                        var offset = pos - mPickPlanePos;

                                        for(int i=0; i<mEditor.SelectedElements.Count; i++)
                                        {
                                            var element = mEditor.SelectedElements[i];
                                            var canvas = VisualTreeHelper.GetParent(element);
                                            var min = TtCanvasControl.GetAnchorMin(element);
                                            var max = TtCanvasControl.GetAnchorMax(element);
                                            var x = CalculateAnchorLeft(offset, canvas.DesignRect.Width, min, max, element, i);
                                            var y = CalculateAnchorBottom(offset, canvas.DesignRect.Height, min, max, element, i);
                                            TtCanvasControl.SetAnchorMin(element, new Vector2(x, min.Y));
                                            TtCanvasControl.SetAnchorMax(element, new Vector2(max.X, y));
                                        }
                                    }
                                }
                                break;
                            case EDecoratorType.Anchor_MLeft:
                            case EDecoratorType.Anchor_Left:
                                {
                                    if(e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
                                    {
                                        DVector3 pickPos;
                                        PickPlanePos(e.MouseButton.X, e.MouseButton.Y, mPickPlanePos, mPickPlaneNormal, out pickPos);
                                        var pos = pickPos - mDecoratorMouseDownOffset;
                                        var offset = pos - mPickPlanePos;

                                        for(int i=0; i<mEditor.SelectedElements.Count; i++)
                                        {
                                            var element = mEditor.SelectedElements[i];
                                            var canvas = VisualTreeHelper.GetParent(element);
                                            var min = TtCanvasControl.GetAnchorMin(element);
                                            var max = TtCanvasControl.GetAnchorMax(element);
                                            var x = CalculateAnchorLeft(offset, canvas.DesignRect.Width, min, max, element, i);
                                            TtCanvasControl.SetAnchorMin(element, new Vector2(x, min.Y));
                                        }
                                    }
                                }
                                break;
                            case EDecoratorType.Anchor_STopLeft:
                            case EDecoratorType.Anchor_TopLeft:
                                {
                                    if(e.MouseButton.Button == (byte)Bricks.Input.EMouseButton.BUTTON_LEFT)
                                    {
                                        DVector3 pickPos;
                                        PickPlanePos(e.MouseButton.X, e.MouseButton.Y, mPickPlanePos, mPickPlaneNormal, out pickPos);
                                        var pos = pickPos - mDecoratorMouseDownOffset;
                                        var offset = pos - mPickPlanePos;

                                        for(int i=0; i<mEditor.SelectedElements.Count; i++)
                                        {
                                            var element = mEditor.SelectedElements[i];
                                            var canvas = VisualTreeHelper.GetParent(element);
                                            var min = TtCanvasControl.GetAnchorMin(element);
                                            var max = TtCanvasControl.GetAnchorMax(element);
                                            var x = CalculateAnchorLeft(offset, canvas.DesignRect.Width, min, max, element, i);
                                            var y = CalculateAnchorTop(offset, canvas.DesignRect.Height, min, max, element, i);
                                            TtCanvasControl.SetAnchorMin(element, new Vector2(x, y));
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    break;
                case Bricks.Input.EventType.MOUSEBUTTONDOWN:
                    {
                        if (mEditor.HitProxyNode != null)
                        {
                            mCurDecoratorType = EDecoratorType.None;

                            var delta = mEditor.PreviewViewport.WindowPos - mEditor.PreviewViewport.ViewportPos;
                            var mousePt = new Vector2(e.MouseButton.X - delta.X, e.MouseButton.Y - delta.Y);
                            var proxy = mEditor.HitProxyNode.GetHitproxy((uint)mousePt.X, (uint)mousePt.Y);

                            for(int i=0; i < mAnchorNodes.Length; i++)
                            {
                                if (mAnchorNodes[i] == proxy)
                                    mCurDecoratorType = (EDecoratorType)(i + (int)EDecoratorType.Anchor_Start);
                            }
                            for (int i = 0; i < mOperatorNodes.Length; i++)
                            {
                                if (mOperatorNodes[i] == proxy)
                                    mCurDecoratorType = (EDecoratorType)i;
                            }

                            if (mCurDecoratorType != EDecoratorType.None)
                            {
                                DVector3 pickPos;
                                if (mCurDecoratorType >= EDecoratorType.Size_Left_Top && mCurDecoratorType <= EDecoratorType.Size_Right_Bottom)
                                    mPickPlanePos = mOperatorNodes[(int)(mCurDecoratorType)].Placement.Position;
                                else if (mCurDecoratorType >= EDecoratorType.Anchor_Start && mCurDecoratorType < EDecoratorType.Anchor_End)
                                    mPickPlanePos = mAnchorNodes[(int)(mCurDecoratorType - EDecoratorType.Anchor_Start)].Placement.Position;
                                else
                                    throw new InvalidOperationException($"No operation with type:{mCurDecoratorType}");

                                mPickPlaneNormal = Vector3.TransformCoordinate(Vector3.UnitZ, mEditor.SelectedRect.GetUIHost(0).TransformedElements[0].Matrix);
                                PickPlanePos(e.MouseButton.X, e.MouseButton.Y, mPickPlanePos, mPickPlaneNormal, out pickPos);
                                mDecoratorMouseDownOffset = pickPos - mPickPlanePos;

                                mEditor.SetCurrentPointAtElement(null);
                            }
                            else if(mEditor.CurrentPointAtElement != null && mEditor.SelectedElements.Contains(mEditor.CurrentPointAtElement))
                            {
                                mCurDecoratorType = EDecoratorType.Move;
                                mPickPlanePos = DVector3.TransformCoordinate(
                                    mEditor.CurrentPointAtElement.RootUIHost.SceneNode.Location,
                                    mEditor.CurrentPointAtElement.RootUIHost.SceneNode.GetWorld().CameraOffset, 
                                    mEditor.CurrentPointAtElement.RootUIHost.TransformedElements[0].Matrix);
                                mPickPlaneNormal = Vector3.TransformCoordinate(Vector3.UnitZ, mEditor.CurrentPointAtElement.RootUIHost.TransformedElements[0].Matrix);
                                DVector3 pickPos;
                                PickPlanePos(e.MouseButton.X, e.MouseButton.Y, mPickPlanePos, mPickPlaneNormal, out pickPos);
                                mDecoratorMouseDownOffset = pickPos - mPickPlanePos;

                                mEditor.SetCurrentPointAtElement(null);
                            }
                            UpdateOriAnchorDatas();
                        }
                    }
                    break;
                case Bricks.Input.EventType.MOUSEBUTTONUP:
                    mCurDecoratorType = EDecoratorType.None;
                    break;
            }
        }

        public bool IsInDecoratorOperation()
        {
            return mCurDecoratorType != EDecoratorType.None;
        }
    }
}
