using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YCClassEditor
{
    public class YCPropertyCollection
    {
        private ObservableCollection<YCProperty> lstData = new ObservableCollection<YCProperty>();

        public ObservableCollection<YCProperty> LstData
        {
            get { return lstData; }
            set { lstData = value; }
        }
        public void AddCustomClass(string MyClassName)
        {
            foreach (var p in LstData)
            {
                if (!p.types.Contains(MyClassName))
                    p.types.Add(MyClassName);
            }

        }
        public void RemoveCustomClass(string MyClassName)
        {
            foreach (var p in LstData)
            {
                if (p.types.Contains(MyClassName))
                    p.types.Remove(MyClassName);
            }

        }
    }
    public class YCProperty : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int id;
        private string _name;
        public string name
        {
            get { return _name; }
            set
            {
                _name = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("name"));
                }
            }
        }
        //to decide way of displayng,not chosen
        public string type { get; set; }
        //chosen
        private List<string> _types = new List<string> { "int", "string", "float", "double", "bool", "enum" };
        public List<string> types
        {
            get { return _types; }
            set
            {
                _types = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("types"));
                }
            }
        }
        private string _defaultvalue = "";
        public string defaultvalue
        {
            get { return _defaultvalue; }
            set
            {
                _defaultvalue = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("defaultvalue"));
                }
            }
        }
        #region ifEnum
        public string enumtype { get; set; }
        #endregion
        #region ifList
        public bool bList { get; set; }

        //default value for list and single
        
        /*可用于实例化list，后续有需求可填充，暂时不用
        private int _listsize = 0;
        public int listsize
        {
            get { return _listsize; }
            set
            {
                _listsize = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("listsize"));
                }
            }
        }
        private ObservableCollection<ListItem> _defaultvalueList = new ObservableCollection<ListItem>();
        public ObservableCollection<ListItem> defaultvalueList
        {
            get { return _defaultvalueList; }
            set
            {
                _defaultvalueList = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("defaultvalueList"));
                }
            }
        }
        */
        #endregion
        #region ifRange
        //if range is set
        public bool bRange { get; set; }
        private string _min = "";
        public string min
        {
            get { return _min; }
            set
            {
                _min = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("min"));
                }
            }
        }
        private string _max = "";
        public string max
        {
            get { return _max; }
            set
            {
                _max = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("max"));
                }
            }
        }
        #endregion
        #region ifStep
        public bool bStep { get; set; }
        private string _step = "";
        public string step
        {
            get { return _step; }
            set
            {
                _step = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("step"));
                }
            }
        }
        #endregion
        #region ifAttribute
        public bool hasAttribute { get; set; }
        private string _attribute = "";
        public string attribute
        {
            get { return _attribute; }
            set
            {
                _attribute = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("attribute"));
                }
            }
        }

        private string _attributeInput;
        public string attributeInput
        {
            get { return _attributeInput; }
            set
            {
                _attributeInput = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("attributeInput"));
                }
            }
        }
        #endregion
        //避免系统生成默认构造函数，否则datagrid自动添加NewItemPlaceHolder
        public YCProperty(string Name = "")
        {
            _name = Name;
        }
    }
    //用来使combobox绑定type
    class ComboxBindType
    {
        //构造函数
        public ComboxBindType(string _cmbText)
        {
            this.cmbText = _cmbText;
        }

        //用于显示值
        private string cmbText;
        public string CmbText
        {
            get { return cmbText; }
            set { cmbText = value; }
        }
    }
    /*实例化List每一项，后续有需求可用，暂时不用
    public class ListItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public int id;//和它所属的MyProperty id 一致
        private string _value;
        public string value
        {
            get { return _value; }
            set
            {
                _value = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("value"));
                }
            }
        }
        //避免系统生成默认构造函数，否则datagrid自动添加NewItemPlaceHolder
        public ListItem(int _id)
        {
            id = _id;
        }

    }*/


}
