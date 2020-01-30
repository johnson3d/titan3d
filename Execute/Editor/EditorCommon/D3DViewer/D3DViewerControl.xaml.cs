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

namespace EditorCommon.D3DViewer
{
    /// <summary>
    /// D3DViewerControl.xaml 的交互逻辑
    /// </summary>
    public partial class D3DViewerControl : UserControl
    {
        public D3DViewerControl()
        {
            InitializeComponent();
        }

        public EngineNS.CSwapChain SwapChain;
        public EngineNS.Graphics.CGfxViewPort EditorViewPort;
        public EngineNS.CCommandList CommitCommandList;

        bool mInitialized = false;
        bool InitD3DEnviroment()
        {
            if (mInitialized)
                return true;
            mInitialized = true;
            var rc = EngineNS.CEngine.Instance.RenderContext;
            EngineNS.CSwapChainDesc desc;
            desc.Format = EngineNS.EPixelFormat.PXF_R8G8B8A8_UNORM;
            desc.Width = (UInt32)DrawPanel.Width;
            desc.Height = (UInt32)DrawPanel.Height;
            desc.WindowHandle = DrawPanel.Handle;
            SwapChain = rc.CreateSwapChain(desc);

            var evpDesc = new EngineNS.Graphics.CGfxViewPortDesc();
            evpDesc.IsDefault = true;
            evpDesc.Width = desc.Width;
            evpDesc.Height = desc.Height;
            evpDesc.DepthStencil.Format = EngineNS.EPixelFormat.PXF_D24_UNORM_S8_UINT;
            evpDesc.DepthStencil.Width = desc.Width;
            evpDesc.DepthStencil.Height = desc.Height;
            var rtDesc = new EngineNS.CRenderTargetViewDesc();
            rtDesc.CreateSRV = 0;
            evpDesc.RenderTargets.Add(rtDesc);
            EditorViewPort = new EngineNS.Graphics.CGfxViewPort();
            EditorViewPort.Init(rc, SwapChain, evpDesc);

            return true;
        }
        private void Changed_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (EngineNS.CEngine.Instance == null)
                return;
            var rc = EngineNS.CEngine.Instance.RenderContext;
            SwapChain.OnResize((UInt32)DrawPanel.Width, (UInt32)DrawPanel.Height);

            this.EditorViewPort.OnResize(rc, SwapChain, (UInt32)DrawPanel.Width, (UInt32)DrawPanel.Height);

            var PerViewPortCBuffer = EditorViewPort.ViewPortCBuffer;
            {
                var g_Projection = EngineNS.Matrix.PerspectiveFovLH(EngineNS.MathHelper.V_PI / 4, (float)DrawPanel.Width / (float)DrawPanel.Height, 0.01f, 100.0f);
                g_Projection = EngineNS.Matrix.Transpose(ref g_Projection);
                var varIndex = PerViewPortCBuffer.FindVar("Projection");
                PerViewPortCBuffer.SetValue(varIndex, g_Projection, 0);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitD3DEnviroment();
        }
    }
}
