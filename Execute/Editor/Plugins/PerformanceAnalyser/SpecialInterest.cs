using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PerformanceAnalyser
{
    public class DataTemplateSelector : System.Windows.Controls.DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;
            if (element == null)
                return null;

            if (item is PerformanceItem_Parent)
                return element.TryFindResource("PerfParentTemplate") as DataTemplate;
            else if (item is PerformanceItem_PerfCount)
                return element.TryFindResource("PerfCountTemplate") as DataTemplate;
            else if (item is PerformanceItem_Data)
                return element.TryFindResource("PerfDataTemplate") as DataTemplate;

            return null;
        }
    }

    public class SpecialInterest : INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        string mName = "";
        public string Name
        {
            get { return mName; }
            set
            {
                mName = value;
                OnPropertyChanged("Name");
            }
        }

        PerformanceItem mData;
        public PerformanceItem Data
        {
            get { return mData; }
            set
            {
                mData = value;
                OnPropertyChanged("Data");
            }
        }
    }
}
