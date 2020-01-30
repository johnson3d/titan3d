using System;
using System.Collections.Generic;
using EngineNS;

namespace EngineNS.Editor
{
    public class SnapshotCreator
    {   
        bool m_bRenderingSnapshots = false;
        public bool RenderingSnapshots
        {
            get { return m_bRenderingSnapshots; }
        }
        
        EngineNS.GamePlay.GWorld mWorld;
        public EngineNS.GamePlay.GWorld World
        {
            get { return mWorld; }
        }
        
        public UInt32 mWidth = 256;
        public UInt32 mHeight = 256;

        public EngineNS.Graphics.CGfxViewPort EditorViewPort;
        public EngineNS.Graphics.CGfxRenderPolicy RPolicy;

        public bool InitEnviroment()
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;
            
            var evpDesc = new EngineNS.Graphics.CGfxViewPortDesc();
            evpDesc.IsDefault = false;
            evpDesc.Width = mWidth;
            evpDesc.Height = mHeight;
            evpDesc.DepthStencil.Format = EngineNS.EPixelFormat.PXF_D24_UNORM_S8_UINT;
            evpDesc.DepthStencil.Width = mWidth;
            evpDesc.DepthStencil.Height = mHeight;
            var rtDesc = new EngineNS.CRenderTargetViewDesc();
            rtDesc.CreateSRV = 1;
            rtDesc.Width = mWidth;
            rtDesc.Height = mHeight;
            rtDesc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
            evpDesc.RenderTargets.Add(rtDesc);
            EditorViewPort = new EngineNS.Graphics.CGfxViewPort();
            EditorViewPort.Init(rc, null, evpDesc);

            RPolicy = new EngineNS.Graphics.CGfxRPolicy_Default();

            InitWorld();
            return true;
        }
        protected virtual bool InitWorld()
        {
            mWorld = new EngineNS.GamePlay.GWorld();

            if (false == mWorld.Init())
                return false;

            var scene = new EngineNS.GamePlay.SceneGraph.GSceneGraph();

            mWorld.Scenes.Add(RName.GetRName("SnapshorCreator"), scene);
            return true;
        }
        public void FinalCleanup()
        {

        }

        public Int64 ElapsedTime
        {
            get { return 50; }
        }
        public static void MaxZoomMeshShow(float x, float y, float width, float height, Vector3 vMax, Vector3 vMin, EngineNS.Graphics.CGfxCamera Camera, EngineNS.GamePlay.GWorld world, double delta = 2.0)
        {
            List<Vector3> VecList = new List<Vector3>();

            if (vMax.X > 10000)
                vMax.X = 10000;
            if (vMax.Y > 10000)
                vMax.Y = 10000;
            if (vMax.Z > 10000)
                vMax.Z = 10000;
            if (vMin.X < -10000)
                vMin.X = -10000;
            if (vMin.Y < -10000)
                vMin.Y = -10000;
            if (vMin.Z < -10000)
                vMin.Z = -10000;

            VecList.Add(vMin);
            VecList.Add(new Vector3(vMax.X, vMin.Y, vMin.Z));
            VecList.Add(new Vector3(vMax.X, vMax.Y, vMin.Z));
            VecList.Add(new Vector3(vMin.X, vMax.Y, vMin.Z));
            VecList.Add(new Vector3(vMin.X, vMax.Y, vMax.Z));
            VecList.Add(new Vector3(vMin.X, vMin.Y, vMax.Z));
            VecList.Add(new Vector3(vMax.X, vMin.Y, vMax.Z));
            VecList.Add(vMax);

            var vObjCenter = (vMax - vMin) * 0.5f + vMin;
            float fMaxXLength = 0;
            float fMaxYLength = 0;
            foreach (var vec in VecList)
            {
                fMaxXLength = System.Math.Max(System.Math.Abs(Vector3.Dot(vec - vObjCenter, Camera.Right)), fMaxXLength);
                fMaxYLength = System.Math.Max(System.Math.Abs(Vector3.Dot(vec - vObjCenter, Camera.Right)), fMaxYLength);
            }

            Vector3 eyePos = Vector3.Zero;
            float length = 0;
            if (Camera.IsPerspective)
            {
                float lengthX = (float)(fMaxXLength / System.Math.Tan(Camera.Fov * 0.5));
                float lengthY = (float)(fMaxYLength / System.Math.Tan(Camera.Fov * (width / height) * 0.5));

                length = (float)(System.Math.Max(lengthX, lengthY) * delta);
                eyePos = Camera.Direction * -(float)length + vObjCenter;
            }
            else
            {
                var lengthX = (float)(fMaxXLength * 2 * delta);
                var lengthY = lengthX * height / width;
                Camera.MakeOrtho(lengthX, lengthY, width, height);

                var deltaSize = vMax - vMin;
                length = System.Math.Max(System.Math.Max(deltaSize.X, deltaSize.Y), deltaSize.Z) + 50.0f;
                eyePos = Camera.Direction * -length + vObjCenter;
            }

            var dir = Camera.Direction;
            Camera.LookPosAndDir(eyePos, dir);
        }
        public void CalculateCamera(EngineNS.Graphics.CGfxCamera Camera, double delta)
        {
            Vector3 vMax = Vector3.UnitXYZ * float.MinValue;
            Vector3 vMin = Vector3.UnitXYZ * float.MaxValue;
            foreach (var actor in mWorld.Actors)
            {
                Vector3 tempMax = Vector3.Zero, tempMin = Vector3.Zero;
                BoundingBox aabb = new BoundingBox();
                actor.Value.GetAABB(ref aabb);
                tempMin = aabb.Minimum;
                tempMax = aabb.Maximum;
                vMax.X = System.Math.Max(vMax.X, tempMax.X);
                vMax.Y = System.Math.Max(vMax.Y, tempMax.Y);
                vMax.Z = System.Math.Max(vMax.Z, tempMax.Z);
                vMin.X = System.Math.Min(vMin.X, tempMin.X);
                vMin.Y = System.Math.Min(vMin.Y, tempMin.Y);
                vMin.Z = System.Math.Min(vMin.Z, tempMin.Z);
            }

            MaxZoomMeshShow(0, 0, mWidth, mHeight, vMax, vMin, Camera, mWorld, delta);
        }
        public async System.Threading.Tasks.Task SaveToFile(string strFileName)
        {
            var rc = CEngine.Instance.RenderContext;
            EngineNS.Graphics.CGfxCamera Camera = new EngineNS.Graphics.CGfxCamera();
            Vector3 Eye = new Vector3();
            Eye.SetValue(0.0f, 3.0f, -6.0f);
            Vector3 At = new Vector3();
            At.SetValue(0.0f, 1.0f, 0.0f);
            Vector3 Up = new Vector3();
            Up.SetValue(0.0f, 1.0f, 0.0f);
            Camera.LookAtLH(Eye, At, Up);
            Camera.SetViewPort(rc, EditorViewPort);
            Camera.PerspectiveFovLH(MathHelper.V_PI / 4, 
                (float)EditorViewPort.ViewPort.Width, 
                (float)EditorViewPort.ViewPort.Height, 0.01f, 100.0f);

            CalculateCamera(Camera, 2.0);

            CMRTClearColor[] clrColors = new CMRTClearColor[]
                {
                    new CMRTClearColor(0, 0xFF000000),
                };
            Camera.DisplayView.ClearMRT(clrColors, true, 1.0F, true, 0);

            Camera.DisplayView.ClearPasses(EngineNS.Graphics.EDrawChannel.DCN_Base);

            this.World?.Tick();
            this.World?.CheckVisible(rc, Camera);

            RPolicy.TickLogic(Camera.DisplayView);
            RPolicy.TickSync();
            RPolicy.TickRender(null);

            Camera.DisplayView.RenderTargets[0].Texture2D.Save2File(rc, strFileName, EIMAGE_FILE_FORMAT.PNG);
        }
    }
}
