using EngineNS.Bricks.Animation.Notify;
using System;
using System.Collections.Generic;
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

namespace EditorCommon.Controls.Animation
{
    public enum BuiltInNotifyType
    {
        Default,
        Effect,
        Sound,
    }
    public delegate void AnimNotifyEventHandler<T>(object notify, object sender, T eventArgs);
    /// <summary>
    /// Interaction logic for AnimNotifyNodeControl.xaml
    /// </summary>
    public partial class AnimNotifyNodeControl : UserControl
    {
        public event AnimNotifyEventHandler<MouseButtonEventArgs> AnimNotifyMouseLeftButtonDown;
        public event AnimNotifyEventHandler<MouseButtonEventArgs> AnimNotifyMouseLeftButtonUp;
        public event AnimNotifyEventHandler<MouseButtonEventArgs> AnimNotifyMouseRightButtonDown;
        public event AnimNotifyEventHandler<MouseEventArgs> AnimNotifyMouseMove;
        public event AnimNotifyEventHandler<MouseWheelEventArgs> AnimNotifyMouseWheel;

        public string NodeName
        {
            get => NotifyName.Text;
            set { NotifyName.Text = value; CGfxNotify.NotifyName = value; }
        }
        public double LocationFrame
        {
            get;
            set;
        }
        public double Pos
        {
            get => Margin.Left;
            set { var margin = Margin; margin.Left = value; Margin = margin; }
        }
        public double NodeHeight
        {
            get => ShowedNode.Height;
            set { ShowedNode.Height = value; }
        }
        public double NodeWidth
        {
            get => ShowedNode.ActualWidth;
            //set { ShowedNode.ActualWidth = value; }
        }
        public bool IsLButtonDown = false;
        public int TrackNum
        {
            get;set;
        }
        protected EngineNS.Macross.MacrossGetter<CGfxNotify> mNotifyGetter;
        public EngineNS.Macross.MacrossGetter<CGfxNotify> NotifyGetter
        {
            get { return mNotifyGetter; }
        }
        EngineNS.RName mNotifyRName = EngineNS.RName.EmptyName;
        public EngineNS.RName NotifyRName
        {
            get => mNotifyRName;
            set
            {
                mNotifyRName = value;
                mNotifyGetter = EngineNS.CEngine.Instance.MacrossDataManager.NewObjectGetter<CGfxNotify>(value);
            }
        }
        CGfxNotify mCGfxNotify = new CGfxNotify();
        public CGfxNotify CGfxNotify
        {
            get
            {
                if (mNotifyGetter != null)
                {
                    mCGfxNotify = mNotifyGetter.Get(false);
                }
                return mCGfxNotify;
            }
            set
            {
                mCGfxNotify = value;
                mNotifyGetter = new EngineNS.Macross.MacrossGetter<CGfxNotify>(mNotifyRName, value);
            }
        }
        BuiltInNotifyType BuiltInType { get; set; } = BuiltInNotifyType.Default;
        public AnimNotifyNodeControl(BuiltInNotifyType notifyType)
        {
            InitializeComponent();
            BuiltInType = notifyType;
            switch (BuiltInType)
            {
                case BuiltInNotifyType.Default:
                    {
                        mCGfxNotify = new CGfxNotify();
                    }
                    break;
                case BuiltInNotifyType.Effect:
                    {
                        mCGfxNotify = new EffectNotify();
                    }
                    break;
                case BuiltInNotifyType.Sound:
                    {
                        mCGfxNotify = new CGfxNotify();
                    }
                    break;
            }
        }

        private void ShowedNode_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AnimNotifyMouseLeftButtonDown?.Invoke(this, sender, e);
        }

        private void ShowedNode_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            AnimNotifyMouseLeftButtonUp?.Invoke(this, sender, e);
        }

        private void ShowedNode_MouseMove(object sender, MouseEventArgs e)
        {
            AnimNotifyMouseMove?.Invoke(this, sender, e);
        }

        private void ShowedNode_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            AnimNotifyMouseRightButtonDown?.Invoke(this, sender, e);
        }

        private void ShowedNode_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            AnimNotifyMouseWheel?.Invoke(this, sender, e);
        }
        private void NotifyName_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            AnimNotifyMouseLeftButtonDown?.Invoke(this, sender, e);
        }

        private void NotifyName_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            AnimNotifyMouseLeftButtonUp?.Invoke(this, sender, e);
        }

        private void NotifyName_MouseMove(object sender, MouseEventArgs e)
        {
            AnimNotifyMouseMove?.Invoke(this, sender, e);
        }

        private void NotifyName_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            AnimNotifyMouseRightButtonDown?.Invoke(this, sender, e);
        }


        private void NotifyName_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            AnimNotifyMouseWheel?.Invoke(this, sender, e);
        }
    }
}
