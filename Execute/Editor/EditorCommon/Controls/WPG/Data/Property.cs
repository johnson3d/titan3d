using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace WPG.Data
{
    public class Property_HideName : Property
    {
        public Property_HideName(object instance, EditorCommon.CustomPropertyDescriptor property, Property parentProperty, PropertyGrid hostGrid)
            : base(instance, property, parentProperty, hostGrid)
        {

        }
    }

    public class Property : Item, IDisposable, INotifyPropertyChanged, EngineNS.Editor.ICustomPropertyDescriptor
    {
        public new event PropertyChangedEventHandler PropertyChanged;

        private new void NotifyPropertyChanged(String info)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, info, PropertyChanged);
            //if (PropertyChanged != null)
            //{
            //    PropertyChanged(this, new PropertyChangedEventArgs(info));
            //}
        }


		#region Fields

		protected object _instance;
        protected EditorCommon.CustomPropertyDescriptor _property;
        protected Property _parentProperty = null;
        protected PropertyGrid _hostGrid;

		#endregion

		#region Initialization

        public Property(object instance, EditorCommon.CustomPropertyDescriptor property, Property parentProperty, PropertyGrid hostGrid)
        {
            if (instance is ICustomTypeDescriptor)
            {
                this._instance = property.GetPropertyOwner((ICustomTypeDescriptor)instance);
            }
            else
            {
                this._instance = instance;
            }

            this._property = property;
            this._property.AddValueChanged(_instance, instance_PropertyChanged);
            _parentProperty = parentProperty;
            _hostGrid = hostGrid;

            NotifyPropertyChanged("PropertyType");
        }

        #endregion

        #region Properties

        object mOldValue;
        /// <value>
        /// Initializes the reflected instance property
        /// </value>
        /// <exception cref="NotSupportedException">
        /// The conversion cannot be performed
        /// </exception>
        public object Value
		{
			get { return GetValue(_instance); }
			set
            {
                //if (mOldValue == null)
                    mOldValue = Value;
                var convertedValue = GetRightTypeValue(value);
                if (object.Equals(mOldValue, convertedValue))
                    return;
                //mOldValue = Value;
                SetValue(_instance, convertedValue);
                if (_property.IsNotifyValueChange)
                {
                    var arg = new EngineNS.Editor.NotifyMemberValueChangedArg()
                    {
                        OldValue = mOldValue,
                        NewValue = convertedValue,
                        ValueHostObject = _instance,
                        Property = this,
                    };
                    NotifyPropertyValueChanged("", arg);
                }
                NotifyPropertyChanged("Value");
            }
        }

        object GetRightTypeValue(object value)
        {
            Type propertyType = _property.GetPropertyType(_instance);
            if (!(propertyType == typeof(object) ||
                value == null && propertyType.IsClass ||
                value != null && propertyType.IsAssignableFrom(value.GetType())))
            {
                TypeConverter converter = TypeDescriptor.GetConverter(propertyType);
                return converter.ConvertFrom(value);
            }
            return value;
        }

        void SetPropertyValue(ref object ins, ref object value)
        {
            try
            {
                //_instance = ins;
                object currentValue = _property.GetValue(ins);
                if (value != null && value.Equals(currentValue))
                {
                    return;
                }
                if (object.Equals(value, currentValue))
                    return;
                _property.SetValue(ref ins, ref value);

                if(_property.ParentIsValueType)
                {
                    if(_parentProperty != null)
                        _parentProperty.SetPropertyValue(ref _parentProperty._instance, ref ins);
                    else
                    {
                        // 最开始赋给Instance的就是值类型，无论如何改都改不回去了
                        //_hostGrid.Instance = ins;
                    }
                }
            }
            catch (Exception)
            { }
        }

        protected virtual void NotifyPropertyValueChanged(string path, EngineNS.Editor.NotifyMemberValueChangedArg arg)
        {
            string tempPath;
            if (string.IsNullOrEmpty(path))
                tempPath = _property.Name;
            else
            {
                tempPath = _property.Name + "." + path;
            }
            var receiver = _instance as EngineNS.Editor.INotifyMemberValueChangedReceiver;
            receiver?.OnMemberValueChanged(tempPath, arg);
            _parentProperty?.NotifyPropertyValueChanged(tempPath, arg);
        }

        public EditorCommon.CustomPropertyDescriptor PGProperty
        {
            get { return _property; }
        }

        public object Instance
        {
            get { return _instance; }
        }

        public AttributeCollection ValueAttributes
        {
            get { return _property.Attributes; }
        }

        public string Name
        {
            get { return _property.GetDisplayName(_instance) ?? _property.Name; }
        }

        public string Description
        {
            get { return _property.Description; }
        }

		public bool IsWriteable
		{
			get { return !IsReadOnly; }
		}

		public bool IsReadOnly
		{
			get { return _property.GetIsReadOnly(_instance); }
		}

		public Type PropertyType
		{
			get { return _property.GetPropertyType(_instance); }
		}

		public string Category
		{
			get { return _property.Category; }
		}
		
		#endregion

		#region Event Handlers

		void instance_PropertyChanged(object sender, EventArgs e)
		{           
			NotifyPropertyChanged("Value");
		}
		
		#endregion		

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			if (Disposed)
			{
				return;
			}
			if (disposing)
			{
				_property.RemoveValueChanged(_instance, instance_PropertyChanged);
			}
			base.Dispose(disposing);
		}

        public object GetValue(object ins)
        {
            return _property.GetValue(ins);
        }

        public virtual void SetValue(object ins, object value)
        {
            if (_hostGrid.EnableUndoRedo)
            {
                var oldValue = mOldValue;
                var newValue = value;
                var redoAction = new Action<object>((obj) =>
                {
                    SetPropertyValue(ref ins, ref newValue);
                    NotifyPropertyChanged("Value");
                    _hostGrid._NotifyPropertyChanged(this, oldValue, newValue);
                });
                redoAction?.Invoke(value);
                EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(_hostGrid.UndoRedoKey, null, redoAction, null, 
                                                    (obj)=>
                                                    {
                                                        SetPropertyValue(ref ins, ref oldValue);
                                                        NotifyPropertyChanged("Value");
                                                        _hostGrid._NotifyPropertyChanged(this, newValue, oldValue);
                                                    }, $"Change {_property.GetDisplayName(_instance)??_property.Name} Value");
            }
            else
            {
                SetPropertyValue(ref ins, ref value);
                _hostGrid._NotifyPropertyChanged(this, mOldValue, value);
            }
        }

        #endregion

        #region Comparer for Sorting

        private class ByCategoryThenByNameComparer : IComparer<Property>
        {

            public int Compare(Property x, Property y)
            {
                if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return 0;
                if (ReferenceEquals(x, y)) return 0;
                int val = 0;
                if(x.Category != null && y.Category != null)
                    val = x.Category.CompareTo(y.Category);
                if (val == 0) return x.Name.CompareTo(y.Name);
                return val;
            }
        }

        private class ByCategoryComparer : IComparer<Property>
        {
            public int Compare(Property x, Property y)
            {
                if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return 0;
                if (ReferenceEquals(x, y)) return 0;
                int val = x.Category.CompareTo(y.Category);
                return val;
            }
        }

        private class ByNameComparer : IComparer<Property>
        {

            public int Compare(Property x, Property y)
            {
                if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return 0;
                if (ReferenceEquals(x, y)) return 0;
                return x.Name.CompareTo(y.Name);
            }
        }

        private class BySortIndex : IComparer<Property>
        {
            public int Compare(Property x, Property y)
            {
                if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return 0;
                if (ReferenceEquals(x, y)) return 0;
                EngineNS.Editor.Editor_PropertyGridSortIndex att_X = null, att_Y = null;
                foreach (var valueAtt in x.ValueAttributes)
                {
                    var att = valueAtt as EngineNS.Editor.Editor_PropertyGridSortIndex;
                    if(att != null)
                    {
                        att_X = att;
                        break;
                    }
                }
                foreach(var valueAtt in y.ValueAttributes)
                {
                    var att = valueAtt as EngineNS.Editor.Editor_PropertyGridSortIndex;
                    if(att != null)
                    {
                        att_Y = att;
                        break;
                    }
                }

                if (att_X == null && att_Y == null)
                    return 0;
                else if (att_X == null)
                    return 1;
                else if (att_Y == null)
                    return -1;
                else if (att_X.Index < att_Y.Index)
                    return -1;
                else if (att_X.Index == att_Y.Index)
                    return 0;
                else
                    return 1;
            }
        }

        public readonly static IComparer<Property> CompareByCategoryThenByName = new ByCategoryThenByNameComparer();
        public readonly static IComparer<Property> CompareByCategory = new ByCategoryComparer();
        public readonly static IComparer<Property> CompareByName = new ByNameComparer();
        public readonly static IComparer<Property> CompareBySortIndex = new BySortIndex();


        #endregion
    }
}
