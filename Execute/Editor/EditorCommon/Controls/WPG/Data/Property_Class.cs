using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPG.Data
{
    public class Property_Class : Property
    {
        private PropertyCollection _propertyCollection;
        private bool _automaticlyExpandObjects;
        private string _filter = "";
        public string Filter
        {
            get => _filter;
            set
            {
                _filter = value;
                NotifyPropertyChanged("Filter");
            }
        }
        private bool _noCategory = false;

        public Property_Class(object instance, EditorCommon.CustomPropertyDescriptor property, bool noCategory, bool alphabetical, bool automaticlyExpandObjects, string filter, Property parentProperty, PropertyGrid hostGrid)
                : base(instance, property, parentProperty, hostGrid)
        {
            _automaticlyExpandObjects = automaticlyExpandObjects;
            _filter = filter;
            _noCategory = noCategory;
        }

        //public ObservableCollection<Item> Items
        //{
        //    get
        //    {

        //        if (_propertyCollection == null)
        //        {
        //            //Lazy initialisation prevent from deep search and looping
        //            _propertyCollection = new PropertyCollection(_property.GetValue(_instance), _noCategory, true, _automaticlyExpandObjects, _filter, this, _property.ParentIsValueType, _hostGrid);
        //        }

        //        return _propertyCollection.Items;
        //    }
        //}

        //public override void SetValue(object ins, object value)
        //{
        //    base.SetValue(ins, value);
        //    var col = new PropertyCollection(_property.GetValue(_instance), _noCategory, true, _automaticlyExpandObjects, _filter, this, _property.ParentIsValueType, _hostGrid);
        //    Items.Clear();
        //    foreach (var item in col.Items)
        //        Items.Add(item);
        //    _propertyCollection = col;
        //    //NotifyPropertyChanged("Items");
        //}
    }
}
