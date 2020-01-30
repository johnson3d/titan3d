using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace EditorCommon.Controls.ObjectsPlant
{
    /// <summary>
    /// Interaction logic for ObjectsPlant.xaml
    /// </summary>
    public partial class ObjectsPlant : UserControl
    {
        public ObjectsPlant()
        {
            InitializeComponent();
        }

        Dictionary<string, List<PlantItem>> mPlantItemTypes = new Dictionary<string, List<PlantItem>>();

        void InitPlatnAbleItems()
        {
            StackPanel_Categorys.Children.Clear();
            StackPanel_PlantItems.Children.Clear();
            mPlantItemTypes.Clear();

            RadioButton firstRadioBtn = null;
            var types = EngineNS.Rtti.RttiHelper.GetTypes();
            foreach(var type in types)
            {
                var atts = type.GetCustomAttributes(typeof(EngineNS.Editor.Editor_PlantAbleActor), false);
                if (atts == null || atts.Length == 0)
                    continue;

                var att = atts[0] as EngineNS.Editor.Editor_PlantAbleActor;

                List<PlantItem> lists;
                bool needcreate = !mPlantItemTypes.TryGetValue(att.Category, out lists);
                if (needcreate)
                {
                    lists = new List<PlantItem>();
                    mPlantItemTypes[att.Category] = lists;
                }

                var data = new PlantItem()
                {
                    ItemType = type,
                    ItemName = att.Name,
                };
                lists.Add(data);

                if (needcreate)
                {
                    var radioBtn = new RadioButton()
                    {
                        Content = att.Category,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        HorizontalContentAlignment = HorizontalAlignment.Left,
                        Padding = new Thickness(8, 8, 16, 8),
                        GroupName = "CategoryGroup",
                        Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "RadioToggleButtonStyle2")) as Style,
                    };
                    if (firstRadioBtn == null)
                        firstRadioBtn = radioBtn;

                    radioBtn.Checked += (object sender, RoutedEventArgs e) =>
                    {
                        StackPanel_PlantItems.Children.Clear();
                        foreach (var item in lists)
                        {
                            StackPanel_PlantItems.Children.Add(item);
                        }
                    };
                    StackPanel_Categorys.Children.Add(radioBtn);
                }
            }

            firstRadioBtn.IsChecked = true;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitPlatnAbleItems();
        }
    }
}
