using ResourceLibrary;
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
    /// CreateParticleShape.xaml 的交互逻辑
    /// </summary>
    public partial class CreateParticleShape : WindowBase, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        #region binddata
        public class BindData : EditorCommon.TreeListView.TreeItemViewModel, EditorCommon.DragDrop.IDragAbleObject
        {
            public string Description
            {
                get;
                set;
            } = "";

            public string ShapeName
            {
                get;
                set;
            }

            public Type ParticleShapeType
            {
                get;
                set;
            }

            public BindData()
            {
            }

            public System.Windows.FrameworkElement GetDragVisual()
            {
                return null;
            }

            public override bool EnableDrop => true;
        }
        #endregion
        public CreateParticleShape()
        {
            InitializeComponent();
        }

        EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel> TreeViewItemsNodes = new EditorCommon.TreeListView.ObservableCollectionAdv<EditorCommon.TreeListView.ITreeModel>();

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            UIList.TreeListItemsSource = TreeViewItemsNodes;
            AddParticleShapeNames();
        }

        public void AddParticleShapeNames()
        {
            TreeViewItemsNodes.Clear();
            TreeViewItemsNodes.Add(new BindData()
            {
                ShapeName = "Particle Emiter Box",
                ParticleShapeType = typeof(EngineNS.Bricks.Particle.EmitShape.CGfxParticleEmitterShapeBox)
            });

            TreeViewItemsNodes.Add(new BindData()
            {
                ShapeName = "Particle Emiter Cone",
                ParticleShapeType = typeof(EngineNS.Bricks.Particle.EmitShape.CGfxParticleEmitterShapeCone)
            });

            TreeViewItemsNodes.Add(new BindData()
            {
                ShapeName = "Particle Emiter Mesh",
                ParticleShapeType = typeof(EngineNS.Bricks.Particle.EmitShape.CGfxParticleEmitterShapeMesh)
            });

            TreeViewItemsNodes.Add(new BindData()
            {
                ShapeName = "Particle Emiter Sphere",
                ParticleShapeType = typeof(EngineNS.Bricks.Particle.EmitShape.CGfxParticleEmitterShapeSphere)
            });
        }

        public BindData CurrentParticleShape;
        private void Button_Select(object sender, System.Windows.RoutedEventArgs e)
        {
            if (UIList.SelectedItem == null)
            {
                this.Close();
                return;
            }


            EditorCommon.TreeListView.TreeNode treenode = UIList.SelectedItem as EditorCommon.TreeListView.TreeNode;
            BindData data = treenode.Tag as BindData;
            if (data == null)
            {
                return;
            }
            CurrentParticleShape = data;
            this.Close();
        }

        private void Button_Cancel(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Close();
        }

        private void Windowbase_Closed(object sender, EventArgs e)
        {

        }
    }
}
