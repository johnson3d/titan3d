using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WPG
{
    public class Program
    {
        static Dictionary<string, DataTemplate> mRegisterDataTemplates = new Dictionary<string, DataTemplate>();

        public static bool RegisterDataTemplate(string name, DataTemplate template)
        {
            if (mRegisterDataTemplates.ContainsKey(name))
                return false;

            mRegisterDataTemplates[name] = template;
            return true;
        }

        public static void UnRegisterDataTemplate(string name)
        {
            mRegisterDataTemplates.Remove(name);
        }

        public static DataTemplate GetRegisterDataTemplate(string name)
        {
            DataTemplate retTemplate;
            if (mRegisterDataTemplates.TryGetValue(name, out retTemplate))
                return retTemplate;

            return null;
        }
    }
}
