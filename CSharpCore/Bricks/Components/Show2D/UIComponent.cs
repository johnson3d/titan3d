using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EngineNS.GamePlay.Actor;
using EngineNS.GamePlay.Component;
using EngineNS.GamePlay.SceneGraph;
using EngineNS.Graphics;
using EngineNS.Graphics.View;

namespace EngineNS.Bricks.Componets.Show2D
{
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(UIComponentInitializer), "UIComponent", "Show2D", "UIComponent")]
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable)]
    public class UIComponent : GamePlay.Component.GComponent, IPlaceable
    {
        [Rtti.MetaClassAttribute]
        public class UIComponentInitializer : GComponentInitializer
        {
            public UIComponentInitializer()
            {
                
            }
            [Rtti.MetaData]
            [EngineNS.Editor.Editor_RNameMacrossType(typeof(EngineNS.UISystem.UIElement))]
            public RName UIName
            {
                get;
                set;
            }
            [Rtti.MetaData]
            public Vector3 Offset
            {
                get;
                set;
            }
            [Rtti.MetaData]
            public Vector2 Offset2D
            {
                get;
                set;
            }
            public Vector3 AbsOffset;
        }
        public override bool IsVisualComponent
        {
            get => true;
        }
        #region Placeable
        public GPlacementComponent Placement
        {
            get { return null; }
            set { }
        }
        public void OnPlacementChanged(GPlacementComponent placement)
        {
            mInit.AbsOffset = Vector3.TransformCoordinate(mInit.Offset, placement.WorldMatrix);
        }
        //不作用于物理
        public void OnPlacementChangedUninfluencePhysics(GPlacementComponent placement)
        {
            mInit.AbsOffset = Vector3.TransformCoordinate(mInit.Offset, placement.WorldMatrix);
        }
        #endregion
        UISystem.UIElement mUIElement;
        [Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public UISystem.UIElement UIElement => mUIElement;
        [EngineNS.Editor.Editor_RNameMacrossType(typeof(EngineNS.UISystem.UIElement))]
        public RName UIName
        {
            get => mInit.UIName;
            set
            {
                mInit.UIName = value;
                if (value != null)
                {
                    System.Action action = async () =>
                    {
                        mUIElement = await CEngine.Instance.UIManager.GetUICloneAsync(value, this.GetHashCode().ToString());
                    };
                    action();
                }
            }
        }
        public Vector3 Offset
        {
            get => mInit.Offset;
            set
            {
                mInit.Offset = value;
                mInit.AbsOffset = Vector3.TransformCoordinate(mInit.Offset, Host.Placement.WorldMatrix);
            }
        }
        public Vector2 Offset2D
        {
            get => mInit.Offset2D;
            set
            {
                mInit.Offset2D = value;
            }
        }
        private UIComponentInitializer mInit
        {
            get
            {
                return this.Initializer as UIComponentInitializer;
            }
        }
        public async override Task<bool> SetInitializer(CRenderContext rc, GamePlay.IEntity host, IComponentContainer hostContainer, GComponentInitializer v)
        {
            await base.SetInitializer(rc, host, hostContainer, v);
            if (mInit.UIName != null)
            {
                mUIElement = await CEngine.Instance.UIManager.GetUICloneAsync(mInit.UIName, this.GetHashCode().ToString());
            }
            return true;
        }
        public override void CommitVisual(CCommandList cmd, CGfxCamera camera, CheckVisibleParam param)
        {
            base.CommitVisual(cmd, camera, param);

            if (mUIElement == null)
                return;

            Vector3 screenPos;
            var w = camera.CameraData.Trans2ViewPortWithW(ref mInit.AbsOffset, out screenPos);
            if (w < 0)
                return;
            screenPos.X -= mUIElement.DesignRect.Width / 2.0f;
            screenPos.X += mInit.Offset2D.X;
            screenPos.Y += mInit.Offset2D.Y;
            var mat = Matrix.Translate(new Vector3(screenPos.X, screenPos.Y, screenPos.Z));

            var manager = camera.SceneView.UIHost.GetAttachments("UIComponentExecuter") as UICompAttachements;
            if(manager==null)
            {
                manager = new UICompAttachements();
                camera.SceneView.UIHost.AddAttachments("UIComponentExecuter", manager);
            }

            mUIElement.Commit(cmd, ref mat, camera.SceneView.UIHost.DpiScale);
            
            manager.CommitComps.Add(this);
        }
    }

    public class UICompAttachements : EngineNS.UISystem.UIHostAttachments
    {
        public List<UIComponent> CommitComps = new List<UIComponent>();
        public override void BeforeDrawUI(CRenderContext rc, CCommandList cmd, CGfxScreenView view)
        {
            for(int i=0; i < CommitComps.Count; i++)
            {
                CommitComps[i].UIElement.Draw(rc, cmd, view);
            }
        }
        public override void AfterDrawUI(CRenderContext rc, CCommandList cmd, CGfxScreenView view)
        {
            CommitComps.Clear();
        }
    }
}
