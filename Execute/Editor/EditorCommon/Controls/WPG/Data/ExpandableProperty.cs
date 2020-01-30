using System.ComponentModel;
using System.Collections.ObjectModel;


namespace WPG.Data
{
    class ExpandableProperty : Property
    {
        private PropertyCollection _propertyCollection;
        private bool _automaticlyExpandObjects;
        private string _filter;
        private bool _noCategory = false;

        bool mIsExpanded = false;
        public bool IsExpanded
        {
            get => mIsExpanded;
            set
            {
                mIsExpanded = value;
                NotifyPropertyChanged("IsExpanded");
            }
        }

        public ExpandableProperty(object instance, EditorCommon.CustomPropertyDescriptor property, bool noCategory, bool alphabetical, bool automaticlyExpandObjects, string filter, Property parentProperty, PropertyGrid hostGrid)
            : base(instance, property, parentProperty, hostGrid)
        {
            _automaticlyExpandObjects = automaticlyExpandObjects;
            _filter = filter;
            _noCategory = noCategory;
            //var insValue = property.GetValue(instance);
            //_propertyCollection = new PropertyCollection(insValue, noCategory, alphabetical, automaticlyExpandObjects, filter);
            foreach(var att in property.Attributes)
            {
                var epAtt = att as EngineNS.Editor.Editor_ExpandedInPropertyGridAttribute;
                if (epAtt != null)
                {
                    IsExpanded = epAtt.IsExpanded;
                    break;
                }
            }
        }

        public ObservableCollection<Item> Items
        {
            get
            {

                if (_propertyCollection == null)
                {
                    //Lazy initialisation prevent from deep search and looping
                    _propertyCollection = new PropertyCollection(_property.GetValue(_instance), _noCategory, true, _automaticlyExpandObjects, _filter, this, _property.ParentIsValueType, _hostGrid);
                }

                return _propertyCollection.Items;
            }
        }

    }
}
