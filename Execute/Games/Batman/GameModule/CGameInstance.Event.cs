using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using EngineNS;
using EngineNS.Graphics;
using EngineNS.Input;

namespace Game
{
    partial class CGameInstance
    {
        private EngineNS.Vector2 mPrevPos;
        private bool IsDown = false;
        private EngineNS.Vector2 mPrevScale;
        private int PrevPointerNumber;

        private EngineNS.Vector2 mPrevMove;
        //public  void UselessOnActionEvent(ref EngineNS.Input.ActionEvent e)
        //{
        //    try
        //    {
        //        OnActionEventImpl(ref e);
        //    }
        //    catch(Exception ex)
        //    {
        //        EngineNS.Profiler.Log.WriteException(ex);
        //    }
        //}
        //private void OnActionEventImpl(ref EngineNS.Input.ActionEvent e)
        //{
        //    if (e.PointerNumber == 1)
        //    {
        //        switch (e.MotionType)
        //        {
        //            case EMotionType.Down:
        //                IsDown = true;
        //                break;
        //            case EMotionType.Up:
        //                IsDown = false;
        //                break;
        //            case EMotionType.Move:
        //                if (PrevPointerNumber == e.PointerNumber)
        //                {
        //                    OnDrag(e.X - mPrevPos.X, e.Y - mPrevPos.Y);
        //                }
        //                break;
        //            case EMotionType.Hover:
        //                break;
        //        }
        //        mPrevPos.X = e.X;
        //        mPrevPos.Y = e.Y;
        //    }
        //    else if (e.PointerNumber == 2)
        //    {
        //        EngineNS.Vector2 scale;
        //        scale.X = System.Math.Abs(e.GetX(1) - e.GetX(0));
        //        scale.Y = System.Math.Abs(e.GetY(1) - e.GetY(0));
        //        switch (e.MotionType)
        //        {
        //            case EMotionType.Down:
        //                mPrevScale.X = 0;
        //                mPrevScale.Y = 0;
        //                break;
        //            case EMotionType.Up:
        //                break;
        //            case EMotionType.Move:
        //                {
        //                    if (PrevPointerNumber == e.PointerNumber)
        //                    {
        //                        Scale2Fingers(scale.X - mPrevScale.X, scale.Y - mPrevScale.Y);
        //                    }
        //                }
        //                break;
        //            case EMotionType.Hover:
        //                break;
        //        }
        //        mPrevScale = scale;
        //    }
        //    else if (e.PointerNumber == 3)
        //    {
        //        EngineNS.Vector2 movePos;
        //        movePos.X = (e.GetX(2) + e.GetX(1) + e.GetX(0)) / 3;
        //        movePos.Y = (e.GetY(2) + e.GetY(1) + e.GetY(0)) / 3;
        //        switch (e.MotionType)
        //        {
        //            case EMotionType.Down:
        //                mPrevMove.X = movePos.X;
        //                mPrevMove.Y = movePos.Y;
        //                break;
        //            case EMotionType.Up:
        //                break;
        //            case EMotionType.Move:
        //                {
        //                    if (PrevPointerNumber == e.PointerNumber)
        //                    {
        //                        Move3Fingers(movePos.X - mPrevMove.X, movePos.Y - mPrevMove.Y);
        //                    }
        //                }
        //                break;
        //            case EMotionType.Hover:
        //                break;
        //        }
        //        mPrevMove = movePos;
        //    }
        //    else
        //    {
        //        switch (e.MotionType)
        //        {
        //            case EMotionType.MouseWheel:
        //                if (CameraController!=null)
        //                {
        //                    CameraController.Move(EngineNS.GamePlay.Camera.eCameraAxis.Forward, e.GetX(0) * 0.01f);
        //                }
        //                break;
        //        }
        //    }
        //    PrevPointerNumber = e.PointerNumber;
        //}
        EngineNS.GamePlay.Camera.CameraController mCameraController;
        EngineNS.GamePlay.Camera.CameraController CameraController
        {
            get
            {
                if (mCameraController == null)
                {
                    var actor = mWorld.FindActorBySpecialName("TitanActor");
                    if (actor == null)
                        return null;
                    mCameraController = actor.GetComponent<EngineNS.GamePlay.Camera.ThirdPersonCameraComponent>();
                    if(mCameraController == null)
                        mCameraController = actor.GetComponent<EngineNS.GamePlay.Camera.CameraComponent>();
                    mCameraController.Camera = GameCamera;
                }
                return mCameraController;
            }
        }
        private void OnDrag(float offsetx, float offsety)
        {
            if (CameraController == null)
                return;

            CameraController.Rotate(EngineNS.GamePlay.Camera.eCameraAxis.Up, offsetx * 0.01f);
            CameraController.Rotate(EngineNS.GamePlay.Camera.eCameraAxis.Right, offsety * 0.01f);
            var moveMent = ControlActor.GetComponent(typeof(EngineNS.GamePlay.Component.GMovementComponent)) as EngineNS.GamePlay.Component.GMovementComponent;
            var dir = this.GameCamera.CameraData.Direction;
            dir.Y = 0;
            //moveMent.Forward = dir;
        }
        private void Scale2Fingers(float offsetx, float offsety)
        {
            if (CameraController == null)
                return;
            if (offsetx == 0)
                return;
            CameraController.Move(EngineNS.GamePlay.Camera.eCameraAxis.Forward, offsetx * 0.01f);
        }
        private void Move3Fingers(float offsetx, float offsety)
        {
            if (System.Math.Abs(offsetx) > System.Math.Abs(offsety))
            {
                CameraController.Move(EngineNS.GamePlay.Camera.eCameraAxis.Right, offsetx * 0.01f);
            }
            else
            {
                CameraController.Move(EngineNS.GamePlay.Camera.eCameraAxis.Up, offsety * 0.01f);
            }
        }


        public void TestComputeShader()
        {
            const string CSVersion = "cs_5_0";
            var rc = EngineNS.CEngine.Instance.RenderContext;
            var macros = new CShaderDefinitions();
            macros.SetDefine("USE_STRUCTURED_BUFFERS", "");
            var shaderFile = RName.GetRName("Shaders/Compute/test1.compute").Address;
            var shaderRName_extractID = RName.GetRName("Shaders/Compute/test1.compute");
            var shaderRName_calcRate = RName.GetRName("Shaders/Compute/calcrate.compute");
            //var shaderRName_clearID = RName.GetRName("Shaders/Compute/clearid.compute");

            var csMain0 = rc.CreateShaderDesc(shaderRName_extractID, "CSMain", EShaderType.EST_ComputeShader, macros, CIPlatform.Instance.PlatformType);
            var csMain1 = rc.CreateShaderDesc(shaderRName_calcRate, "CSMain1", EShaderType.EST_ComputeShader, macros, CIPlatform.Instance.PlatformType);
            //var csMain2 = rc.CreateShaderDesc(shaderRName_clearID, "CSMain_Clear", EShaderType.EST_ComputeShader, macros, CIPlatform.Instance.PlatformType);
            var csExtractID = rc.CreateComputeShader(csMain0);
            var csCalcRate = rc.CreateComputeShader(csMain1);
            //var csClearID = rc.CreateComputeShader(csMain2);

            uint numElem = 8;
            var bfDesc = new CGpuBufferDesc();
            bfDesc.SetMode(false,true);
            bfDesc.ByteWidth = 4 * numElem;
            bfDesc.StructureByteStride = 4;
            var buffer = rc.CreateGpuBuffer(bfDesc, IntPtr.Zero);
            var uavDesc = new CUnorderedAccessViewDesc();
            uavDesc.ToDefault();
            uavDesc.Buffer.NumElements = numElem;
            var uav = rc.CreateUnorderedAccessView(buffer, uavDesc);

            var buffer1 = rc.CreateGpuBuffer(bfDesc, IntPtr.Zero);
            var uav1 = rc.CreateUnorderedAccessView(buffer1, uavDesc);

            var cmd = rc.ImmCommandList;
            cmd.SetComputeShader(csExtractID);

            UInt32[] pUAVInitialCounts = new UInt32[1] { 1, };

            var dsv = this.RenderPolicy.BaseSceneView.FrameBuffer.GetSRV_DepthStencil();
            var viewWidth = this.RenderPolicy.BaseSceneView.Viewport.Width;
            var viewHeight = this.RenderPolicy.BaseSceneView.Viewport.Height;
            System.Diagnostics.Trace.WriteLine("[TestComputeShader] Framebuffer width :" + viewWidth.ToString() + ", height: " + viewHeight.ToString());
            var camVP = this.RenderPolicy.Camera.CameraData.ViewProjection;
            //var srcTex = CEngine.Instance.TextureManager.GetShaderRView(rc, RName.GetRName("Texture/testActorID.txpic"), true);
            //if (srcTex != null)
            //{
            //    srcTex.PreUse(true);
            //    cmd.CSSetShaderResource(0, srcTex);
            //}
            cmd.CSSetShaderResource(0, dsv);
            cmd.CSSetUnorderedAccessView(0, uav, pUAVInitialCounts);
            //uint texSizeW = 1092;
            //uint texSizeH = 516;
            cmd.CSDispatch((uint)viewWidth / 4, (uint)viewHeight / 4, 1);
            System.Diagnostics.Trace.WriteLine("[TestComputeShader] Dispatch width :" + viewWidth.ToString() + ", height: " + viewHeight.ToString());
            //uint texSize = 512;
            //cmd.CSDispatch(texSize/8, texSize/8, 1);

            var blob = new EngineNS.Support.CBlobObject();
            buffer.GetBufferData(rc, blob);
            var idArray = blob.ToUInts();

            // fill uav1, clear uav0
            cmd.SetComputeShader(csCalcRate);
            UInt32[] pUAVInitialCounts1 = new UInt32[1] { 1, };
            var tempBufferData = new uint[8];
            tempBufferData[0] = 9543;
            tempBufferData[1] = 3756;
            tempBufferData[2] = 2716;
            tempBufferData[3] = 297;
            tempBufferData[4] = 961;
            tempBufferData[5] = 45046;
            tempBufferData[6] = 0;
            tempBufferData[7] = 5686;
            unsafe
            {
                fixed (uint* pData = &tempBufferData[0])
                {
                    var buffer2 = rc.CreateGpuBuffer(bfDesc, (IntPtr)pData);
                    var uav2 = rc.CreateUnorderedAccessView(buffer2, uavDesc);
                    cmd.CSSetUnorderedAccessView(0, uav2, pUAVInitialCounts);
                }
            }
            //cmd.CSSetUnorderedAccessView(0, uav, pUAVInitialCounts);
            cmd.CSSetUnorderedAccessView(1, uav1, pUAVInitialCounts);
            var cbIndex = csMain1.FindCBufferDesc("CaptureEnv");
            if (cbIndex != uint.MaxValue)
            {
                var cbuffer = rc.CreateConstantBuffer(csMain1, cbIndex);
                var varIdx = cbuffer.FindVar("rateWeight");
                cbuffer.SetValue(varIdx, new EngineNS.Quaternion(100, 1000, 10000, 100000), 0);
                cbuffer.FlushContent(rc.ImmCommandList);
                var cbDesc = new EngineNS.CConstantBufferDesc();
                if (csMain1.GetCBufferDesc(cbIndex, ref cbDesc))
                {
                    if (cbDesc.Type == EngineNS.ECBufferRhiType.SIT_CBUFFER)
                    {
                        cmd.CSSetConstantBuffer(cbDesc.CSBindPoint, cbuffer);
                    }
                }
            }

            cmd.CSDispatch(8, 1, 1);

            // gbuffer1
            buffer.GetBufferData(rc, blob);
            idArray = blob.ToUInts();
            var blob1 = new EngineNS.Support.CBlobObject();
            buffer1.GetBufferData(rc, blob1);
            var idArray1 = blob1.ToBytes();
            var idUintArray1 = blob1.ToUInts();

        }

        bool IsDown_W = false;
        bool IsDown_S = false;
        bool IsDown_A = false;
        bool IsDown_D = false;
        //public void UselessOnKeyEvent(ref EngineNS.Input.KeyEvent e)
        //{
        //    if (ControlActor == null)
        //        return;
        //    var moveMent = ControlActor.GetComponent(typeof(EngineNS.GamePlay.Component.GMovementComponent)) as EngineNS.GamePlay.Component.GMovementComponent;
        //    var dir = this.GameCamera.RenderData.Direction;
        //    dir.Y = 0;
        //    moveMent.Forward = dir;
        //    //输入的放大倍数，现在是常数输入，以后可能是一个根据曲线变化的值
        //    float ScaleFactor = 5.0f;
        //    switch (e.KeyCode)
        //    {
        //        case KeyEvent.Keys.W:
        //            {
        //                if (e.Event == KeyEvent.EventType.Down)
        //                {
        //                    if (IsDown_W == true)
        //                        return;
        //                    IsDown_W = true;
                            
        //                    moveMent.VerticalInput = 1* ScaleFactor;
        //                    CEngine.Instance.TickManager.AddTickUntil(() =>
        //                    {
        //                        if (ControlActor == null)
        //                            return false;

        //                        if (CIPlatform.Instance.IsKeyDown(KeyEvent.Keys.W) == false)
        //                        {
        //                            IsDown_W = false;
        //                            moveMent.VerticalInput = 0;
        //                            return false;
        //                        }

        //                        //var dir = this.GameCamera.Direction;
        //                        ////dir.Y = 0;
        //                        //float dist = CEngine.Instance.EngineElapseTimeSecond * ScaleFactor;
        //                        ////ControlActor.Placement?.TryMove(ref dir, dist, CEngine.Instance.EngineElapseTimeSecond);
        //                        //var moveMent = ControlActor.Placement as EngineNS.GamePlay.Component.GMovementComponent;
        //                        //moveMent.Direction = dir;
        //                        //moveMent.YawInput = dist;
        //                        return IsDown_W;
        //                    });
        //                }
        //                else
        //                    IsDown_W = false;
                        
        //            }
        //            break;
        //        case KeyEvent.Keys.S:
        //            {
        //                if (e.Event == KeyEvent.EventType.Down)
        //                {
        //                    if (IsDown_S == true)
        //                        return;
        //                    IsDown_S = true;
        //                    moveMent.VerticalInput = -1 * ScaleFactor;
        //                    CEngine.Instance.TickManager.AddTickUntil(() =>
        //                    {
        //                        if (ControlActor == null)
        //                            return false;

        //                        if (CIPlatform.Instance.IsKeyDown(KeyEvent.Keys.S) == false)
        //                        {
        //                            IsDown_S = false;
        //                            moveMent.VerticalInput = 0;
        //                            return false;
        //                        }

        //                        //var dir = -this.GameCamera.Direction;
        //                        //dir.Y = 0;
        //                        //float dist = CEngine.Instance.EngineElapseTimeSecond * ScaleFactor;
        //                        ////ControlActor.Placement?.TryMove(ref dir, dist, CEngine.Instance.EngineElapseTimeSecond);
        //                        //var moveMent = ControlActor.Placement as EngineNS.GamePlay.Component.GMovementComponent;
        //                        //moveMent.Direction = dir;
        //                        //moveMent.YawInput = dist;
        //                        return IsDown_S;
        //                    });
        //                }
        //                else
        //                    IsDown_S = false;
        //            }
        //            break;
        //        case KeyEvent.Keys.A:
        //            {
        //                if (e.Event == KeyEvent.EventType.Down)
        //                {
        //                    if (IsDown_A == true)
        //                        return;
        //                    IsDown_A = true;
        //                    moveMent.HorizontalInput = -1 * ScaleFactor;
        //                    CEngine.Instance.TickManager.AddTickUntil(() =>
        //                    {
        //                        if (ControlActor == null)
        //                            return false;

        //                        if (CIPlatform.Instance.IsKeyDown(KeyEvent.Keys.A) == false)
        //                        {
        //                            IsDown_A = false;
        //                            moveMent.HorizontalInput = 0;
        //                            return false;
        //                        }

        //                        //var dir = -this.GameCamera.Right;
        //                        //dir.Y = 0;
        //                        //float dist = CEngine.Instance.EngineElapseTimeSecond * ScaleFactor;
        //                        ////ControlActor.Placement?.TryMove(ref dir, dist, CEngine.Instance.EngineElapseTimeSecond);
        //                        //var moveMent = ControlActor.Placement as EngineNS.GamePlay.Component.GMovementComponent;
        //                        //moveMent.Direction = dir;
        //                        //moveMent.YawInput = dist;
        //                        return IsDown_A;
        //                    });
        //                }
        //                else
        //                    IsDown_A = false;
        //            }
        //            break;
        //        case KeyEvent.Keys.D:
        //            {
        //                if (e.Event == KeyEvent.EventType.Down)
        //                {
        //                    if (IsDown_D == true)
        //                        return;
        //                    IsDown_D = true;
        //                    moveMent.HorizontalInput = 1 * ScaleFactor;
        //                    CEngine.Instance.TickManager.AddTickUntil(() =>
        //                    {
        //                        if (ControlActor == null)
        //                            return false;

        //                        if (CIPlatform.Instance.IsKeyDown(KeyEvent.Keys.D) == false)
        //                        {
        //                            IsDown_D = false;
        //                            moveMent.HorizontalInput = 0;
        //                            return false;
        //                        }

        //                        //var dir = this.GameCamera.Right;
        //                        //dir.Y = 0;
        //                        //float dist = CEngine.Instance.EngineElapseTimeSecond * ScaleFactor;
        //                        //ControlActor.Placement?.TryMove(ref dir, dist, CEngine.Instance.EngineElapseTimeSecond);
        //                        return IsDown_D;
        //                    });
        //                }
        //                else
        //                    IsDown_D = false;
        //            }
        //            break;
        //    }
        //}
    }
}
