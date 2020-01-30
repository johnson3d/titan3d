using EngineNS.Bricks.Particle;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ParticleEditor
{
    /// <summary>
    /// ParticleEffect.xaml 的交互逻辑
    /// </summary>
    public partial class ParticleEffect : UserControl
    {
        public class ControlData : INotifyPropertyChanged
        {

            #region INotifyPropertyChangedMembers
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName)
            {
                EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
            }
            #endregion
            public ParticleEffect Effect;
            float mSpeed = 1.0f;
            [DisplayName("Play Speed")]
            public float Speed
            {
                set
                {
                    mSpeed = value;
                    Effect.SetParticleSpeed(mSpeed);

                    OnPropertyChanged("Speed");
                }
                get
                {
                    return mSpeed;
                }
            }

            float mPlayTime = 0.0f;
            [DisplayName("Play Time")]
            public float PlayTime
            {
                set
                {
                    mPlayTime = value;
                    OnPropertyChanged("PlayTime");
                }
                get => mPlayTime;
            }

            bool mIsShowShape = false;
            [DisplayName("Show Shape")]
            public bool IsShowShape
            {
                get => mIsShowShape;
                set
                {
                    mIsShowShape = value;
                    Effect.SetParticleShapeShow(value);
                }
            }

            bool mIsShowSelect = false;
            [DisplayName("Show Only Select")]
            public bool IsShowSelect
            {

                get => mIsShowSelect;
                set
                {
                    mIsShowSelect = value;
                    Effect.SetParticleComponentShow(value);
                }
            }

            bool mIsShowWireFrame = false;
            [DisplayName("Show WireFrame")]
            public bool IsShowWireFrame
            {

                get => mIsShowWireFrame;
                set
                {
                    mIsShowWireFrame = value;
                    Effect.ShowWireFrame(value);
                }
            }

            EngineNS.RName mPrefabResource;
            [EngineNS.Editor.Editor_RNameType(EngineNS.Editor.Editor_RNameTypeAttribute.Mesh)]
            public EngineNS.RName PrefabResource
            {
                get
                {
                    return mPrefabResource;
                }
                set
                {
                    mPrefabResource = value;
                    Effect.SetPrefab(value);
                }
            }

            EngineNS.RName mAnimaResource;
            [EngineNS.Editor.Editor_RNameType(EngineNS.Editor.Editor_RNameTypeAttribute.AnimationClip)]
            public EngineNS.RName AnimaResource
            {
                get
                {
                    return mAnimaResource;
                }
                set
                {
                    mAnimaResource = value;
                    Effect.SetAnimaResource(value);
                    //if (Effect != null && Effect.Host != null && Effect.Host.PreviewClip != null)
                    //{
                    //    Effect.Host.PreviewClip.IsLoop = mShowAnimaLoop;
                    //}
                }
            }

            bool mShowAnimaLoop = true;
            public bool ShowAnimaLoop
            {
                get 
                {
                    return mShowAnimaLoop;
                }

                set
                {
                    mShowAnimaLoop = value;
                    if (Effect != null && Effect.Host != null && Effect.Host.PreviewClip != null)
                    {
                        Effect.Host.PreviewClip.IsLoop = value;
                    }


                }
            }
        }
        public ParticleMacrossLinkControl Host;
        public ControlData Data;
        public ParticleEffect()
        {
            InitializeComponent();
            Data = new ControlData();
            Data.Effect = this;
            PG.Instance = Data;
        }

        public void SetParticleSpeed(float speed)
        {
            if (Host.ParticleComponent == null)
                return;

            oldspeed = Host.ParticleComponent.ParticleModifier.Speed;
            Host.ParticleComponent.ParticleModifier.Speed = speed;

            for (int i = 0; i < Host.ParticleComponent.Components.Count; ++i)
            {
                var current = Host.ParticleComponent.Components[i];

                var component = current as GParticleSubSystemComponent;
                if (component != null)
                {
                    SetParticleSpeed(component, speed);
                }
            }
        }

        public void SetParticleSpeed(GParticleSubSystemComponent ParticleSubSystemComponent, float speed)
        {
            ParticleSubSystemComponent.ParticleModifier.Speed = speed;

            var Enumerator = ParticleSubSystemComponent.Components.GetEnumerator();
            for (int i = 0; i < ParticleSubSystemComponent.Components.Count; ++i)
            {
                var component = ParticleSubSystemComponent.Components[i] as GParticleSubSystemComponent;
                if (component != null)
                {
                    SetParticleSpeed(component, speed);
                }
            }
        }

        public void SetParticleComponentShow(bool showselect)
        {
            if (Host.ParticleComponent == null)
                return;

            for (int i = 0; i < Host.ParticleComponent.Components.Count; ++i)
            {
                var component = Host.ParticleComponent.Components[i] as GParticleSubSystemComponent;
                if (component != null)
                {
                    SetParticleComponentShow(component, showselect);
                }
            }

            if (showselect)
            {
                if (Host.SelectParticleComponent != null)
                {
                    Host.SelectParticleComponent.Visible = true;
                }
            }
        }

        public void SetParticleShapeShow(bool showselect)
        {
            if (Host.TempMeshComponent == null)
                return;

            Host.TempMeshComponent.Visible = showselect;
        }

        public void SetParticleComponentShow(GParticleSubSystemComponent ParticleSubSystemComponent, bool showselect)
        {
            if (showselect == false)
            {
                ParticleSubSystemComponent.Visible = ParticleSubSystemComponent.IsCheckForVisible;
            }
            else
            {
                ParticleSubSystemComponent.Visible = false;
            }

            for (int i = 0; i < ParticleSubSystemComponent.Components.Count; ++i)
            {
                var component = ParticleSubSystemComponent.Components[i] as GParticleSubSystemComponent;
                if (component != null)
                {
                    SetParticleComponentShow(component, showselect);
                }
            }
        }

        public void ShowWireFrame(bool show)
        {
            if (Host.SelectParticleComponent == null)
                return;

            Host.SetPassUserFlags(Host.SelectParticleComponent, show ? (uint)1 : (uint)0);
        }

        public void SetPrefab(EngineNS.RName resource)
        {
            var test = Host.SetPrefab(resource);
        }

        public void SetAnimaResource(EngineNS.RName resource)
        {
            Host.ChangePreviewAnimation(resource);
        }

        float oldspeed = 1;
        bool isplay = true;
        bool istop = false;

        public void ControlPlayParticle()
        {
            SetParticleSpeed(isplay ? 0 : oldspeed);
            UIPLay.Content = isplay ? "Pause" : "Play";
            isplay = !isplay;
            if (Host.PreviewClip != null)
            {
                Host.PreviewClip.Pause = !isplay;
                if (isplay)
                {
                    Host.PreviewClip.Seek(0);
                }
            }
        }

        public void PlayAndPause()
        {
            if (istop)
            {
                var noUse = Restart();
                if (Host.PreviewClip != null)
                {
                    Host.PreviewClip.CurrentTimeInMilliSecond = 0;
                    Host.PreviewClip.Pause =  true;
                }
            }
            else
            {
                ControlPlayParticle();
            }
        }
        private void Button_Click_Play(object sender, RoutedEventArgs e)
        {
            PlayAndPause();
        }

        public async Task Restart()
        {
            await Host.Restart();
            isplay = true;
            UIPLay.Content = "Play";
            oldspeed = 1.0f;
            istop = false;

            if (Host.PreviewClip != null)
            {
                Host.PreviewClip.Pause = false;
                Host.PreviewClip.Seek(0);
            }
        }

        private void Button_Click_Restart(object sender, RoutedEventArgs e)
        {
            var noUse = Restart();
        }


        public void Stop()
        {
            istop = true;
            isplay = false;
            oldspeed = 1.0f;
            UIPLay.Content = "Play";
            Host.Stop();
        }
        private void Button_Click_Stop(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        public void ShowSelectEvent(bool isshow)
        {
        }
    }
}
