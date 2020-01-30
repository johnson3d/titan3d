using EngineNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EditorCommon.ViewPort
{
    public partial class ViewPortControl
    {
        readonly string mGridlineMaterial_Common = "editor/mi_gridline1.instmtl";
        EngineNS.Graphics.CGfxMaterialInstance mGridlineMaterial = null;
        EngineNS.GamePlay.Actor.GActor mPlaneActor = null;
        float mEditor3DGridFade = 0.5f;
        float mEditor2DGridFade = 0.5f;
        float SnapGridSize = 10.0f; //1,10,50... GEditor->GetGridSize();
        bool mShowGridLine = true;
        public bool ShowGridLine
        {
            get => mShowGridLine;
            set
            {
                mShowGridLine = value;
                OnPropertyChanged("ShowGridLine");
            }
        }

        bool mGridlineInitialized = false;
        public async Task InitializeGridline()
        {
            if (mGridlineInitialized)
                return;

            var rc = EngineNS.CEngine.Instance.RenderContext;
            mGridlineMaterial = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(rc, EngineNS.CEngine.Instance.FileManager.GetRName(mGridlineMaterial_Common));

            mGridlineInitialized = true;
            return;
        }

        EngineNS.Vector3 mPreCameraPos = EngineNS.Vector3.Zero;
        public void DrawGridline()
        {
            if (mGridlineInitialized == false)
                return;
            if (World == null)
                return;
            if (mShowGridLine == false)
            {
                if (mPlaneActor != null)
                    World.RemoveEditorActor(mPlaneActor.ActorId);
                return;
            }
            if (mPreCameraPos == Camera.CameraData.Position)
                return;

            var rc = EngineNS.CEngine.Instance.RenderContext;

            bool bIsPerspective = true;
            float SnapAlphaMultiplier = 1.0f;

            // to get a light grid in a black level but use a high opacity value to be able to see it in a bright level
            float Darken = 0.11f;

            var ShaderIdx_SnapTile = mGridlineMaterial.FindVarIndex("SnapTile");
            var ShaderIdx_GridColor = mGridlineMaterial.FindVarIndex("GridColor");

            if (bIsPerspective)
            {
                var gridColor = new EngineNS.Vector4(0.6f * Darken, 0.6f * Darken, 0.6f * Darken, mEditor3DGridFade);
                mGridlineMaterial.SetVarValue(ShaderIdx_GridColor, 0, ref gridColor);
            }
            else
            {
                var gridColor = new EngineNS.Vector4(0.6f * Darken, 0.6f * Darken, 0.6f * Darken, mEditor2DGridFade);
                mGridlineMaterial.SetVarValue(ShaderIdx_GridColor, 0, ref gridColor);
            }

            // true:1m, false:1dm ios smallest grid size
            bool bLarger1mGrid = true;
            float WorldToUVScale = 0.001f;

            if (bLarger1mGrid)
            {
                WorldToUVScale *= 0.1f;
            }
            float SnapTile = (1.0f / WorldToUVScale) / System.Math.Max(1.0f, SnapGridSize);
            mGridlineMaterial.SetVarValue(ShaderIdx_SnapTile, 0, ref SnapTile);

            EngineNS.Matrix ObjectToWorld = EngineNS.Matrix.Identity;

            mPreCameraPos = Camera.CameraData.Position;

            var UVCameraPos = new EngineNS.Vector2(mPreCameraPos.X, mPreCameraPos.Z);

            ObjectToWorld.SetTrans(new EngineNS.Vector3(mPreCameraPos.X, 0, mPreCameraPos.Z));

            // good enough to avoid the AMD artifacts, horizon still appears to be a line
            float Radii = 100000;

            if (bIsPerspective)
            {
                // the higher we get the larger we make the geometry to give the illusion of an infinite grid while maintains the precision nearby
                Radii *= System.Math.Max(1.0f, System.Math.Abs(mPreCameraPos.Y) / 1000.0f);
            }

            EngineNS.Vector2 UVMid = UVCameraPos * WorldToUVScale;
            float UVRadi = Radii * WorldToUVScale;

            EngineNS.Vector2 UVMin = UVMid + new EngineNS.Vector2(-UVRadi, -UVRadi);
            EngineNS.Vector2 UVMax = UVMid + new EngineNS.Vector2(UVRadi, UVRadi);

            var planeMesh = EngineNS.CEngine.Instance.MeshPrimitivesManager.CreateMeshPrimitives(rc, 1);
            EngineNS.Graphics.Mesh.CGfxMeshCooker.MakePlane10x10(rc, planeMesh, UVMin, UVMax);
            planeMesh.ResourceState.KeepValid = true;
            var plane10x10 = EngineNS.CEngine.Instance.MeshManager.CreateMesh(rc, planeMesh);
            var task = plane10x10.SetMaterialInstanceAsync(rc, 0, mGridlineMaterial, EngineNS.CEngine.Instance.PrebuildPassData.DefaultShadingEnvs);
            if (task.IsCompleted)
            {
                if (mPlaneActor != null)
                {
                    World.RemoveEditorActor(mPlaneActor.ActorId);
                    var saved = mPlaneActor;
                    mPlaneActor = null;
                    EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
                    {
                        saved.Cleanup();
                        return null;
                    }, EngineNS.Thread.Async.EAsyncTarget.Main);
                }
                mPlaneActor = EngineNS.GamePlay.Actor.GActor.NewMeshActorDirect(plane10x10);
                mPlaneActor.Placement.Location = new Vector3(mPreCameraPos.X, 0, mPreCameraPos.Z);
                mPlaneActor.Placement.Scale = new Vector3(Radii);
                World.AddEditorActor(mPlaneActor);
            }

            return;
        }

        private void ComboBox_SnapTile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox_SnapTile.SelectedIndex < 0)
                return;
            var text = ComboBox_SnapTile.SelectedItem as TextBlock;
            if (text != null)
            {
                SnapGridSize = System.Convert.ToInt32(text.Text.ToString());
            }
        }

    }
}
