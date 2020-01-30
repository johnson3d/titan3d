using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CodeGenerateSystem.Controls
{
    public class MacrossNodesContainsExtendData
    {
        public string ClassName;
        public string Namespace = "Macross.Generated";
    }

    public sealed partial class BreakPoint : UserControl
    {
        public bool IsBreak
        {
            get { return (bool)GetValue(HotKeyValueProperty); }
            set { SetValue(HotKeyValueProperty, value); }
        }

        public static readonly DependencyProperty HotKeyValueProperty =
            DependencyProperty.Register("IsBreak", typeof(bool), typeof(BreakPoint),
                                    new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsBreakChanged))
                                    );
        public static void OnIsBreakChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var bp = d as BreakPoint;
            bool isBreak = (bool)e.NewValue;
            if(isBreak)
            {
                bp.PART_BREAKPT.Source = bp.TryFindResource("Breakpoint_Valid") as ImageSource;
            }
            else
            {
                bp.PART_BREAKPT.Source = bp.TryFindResource("Breakpoint_Disabled") as ImageSource;
            }

            if (bp.mHostNode != null)
            {
                var extData = bp.mHostNode.HostNodesContainer.ExtendData as MacrossNodesContainsExtendData;
                if (extData == null)
                    throw new InvalidOperationException();
                EngineNS.Editor.Runner.RunnerManager.MacrossBreakContext context = new EngineNS.Editor.Runner.RunnerManager.MacrossBreakContext()
                {
                    DebuggerID = bp.mHostNode.HostNodesContainer.GUID,
                    BreakID = bp.mHostNode.Id,
                    Enable = isBreak,
                    ClassName = extData.ClassName,
                    Namespace = extData.Namespace,
                };

                switch (bp.mHostNode.HostNodesContainer.CSType)
                {
                    case EngineNS.ECSType.Client:
                        {
                            EngineNS.Editor.Runner.RunnerManager.Instance.SetBreakEnable(context);
                        }
                        break;
                        //case EngineNS.ECSType.Server:
                        //    EngineNS.Editor.Runner.RunnerManager.Instance.SetBreakEnable_Remote(CCore.Client.Instance.GateSvrConnect, CCore.Client.Instance.ChiefRole.ActorId, eventType, context);
                        //    break;
                    case EngineNS.ECSType.Common:
                        {
                            EngineNS.Editor.Runner.RunnerManager.Instance.SetBreakEnable(context);
                            //        EngineNS.Editor.Runner.RunnerManager.Instance.SetBreakEnable_Remote(CCore.Client.Instance.GateSvrConnect, CCore.Client.Instance.ChiefRole.ActorId, eventType, context);
                        }
                        break;
                }

            }
        }

        public BreakPoint()
        {
            InitializeComponent();
        }

        CodeGenerateSystem.Base.BaseNodeControl mHostNode = null;
        public void Initialize(CodeGenerateSystem.Base.BaseNodeControl hostNode)
        {
            mHostNode = hostNode;
        }

        private void UserControl_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsBreak = !IsBreak;
            e.Handled = true;
        }

        private void UserControl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
