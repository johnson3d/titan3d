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

namespace ExcelViewEditor
{
    /// <summary>
    /// PropertyPanel.xaml 的交互逻辑
    /// </summary>
    public partial class PropertyPanel : UserControl
    {
        #region PropertyProxy
        public class Element
        {
            public string ShowName;
            public PropertyProxy VarObject;
        }
        public class VarPropertyUIProvider : EngineNS.Editor.Editor_PropertyGridUIProvider
        {
            public override string GetName(object arg)
            {
                var elem = arg as Element;
                return elem.ShowName;
            }
            public override Type GetUIType(object arg)
            {
                var elem = arg as Element;
                if (elem.VarObject == null || elem.VarObject.Value == null)
                    return typeof(object);

                Type type = elem.VarObject.Value.GetType();
                return elem.VarObject.Value.GetType();
            }
            public override object GetValue(object arg)
            {
                var elem = arg as Element;
                return elem.VarObject.Value;
            }
            public override void SetValue(object arg, object val)
            {
                var elem = arg as Element;

                elem.VarObject.Value = val;
            }
        }

        public class PropertyProxy
        {
            [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            [EngineNS.Editor.Editor_PropertyGridUIShowProviderAttribute(typeof(VarPropertyUIProvider))]
            public Object Name
            {
                get
                {
                    var elem = new Element();
                    elem.VarObject = this;
                    elem.ShowName = "实例";
                    return elem;
                }
            }
            //[EngineNS.Rtti.MetaData]
            //[Browsable(false)]
            public Object Value
            {
                get;
                set;
            }

            public PropertyProxy(Object obj)
            {
                Value = obj;
            }
        }
        #endregion

        public PropertyPanel(Object obj)
        {
            InitializeComponent();

            SetValue(obj);
        }

        public PropertyPanel()
        {
            InitializeComponent();
        }

        //WeakReference<Object> Host;

        public int Index
        {
            set
            {
                PG.Headline = value.ToString();
            }
        }
        PropertyProxy Host;
        public void SetValue(Object obj)
        {
            Host = new PropertyProxy(obj);
            PG.Instance = Host;
        }

        #region event
        public delegate void DeleteObjectDelegate(Object obj);
        public event DeleteObjectDelegate DeleteObject;
        private void UIDelete_Click(object sender, RoutedEventArgs e)
        {
            if (Host == null)
                return;

            if (Host.Value == null)
                return;

            DeleteObject?.Invoke(Host.Value);

            Host.Value = null;
            PG.Instance = null;

        }
        #endregion
    }
}
