using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Macross
{
    partial class MacrossMethodTable
    {
        [Rtti.MetaClass]
        class Manual
        {
            [Rtti.MetaData]
            public string FullName
            {
                get;
                set;
            }
            [Rtti.MetaData]
            public string Description
            {
                get;
                set;
            } = "";
        }
        Dictionary<string, Manual> Records = new Dictionary<string, Manual>();
        public void LoadManual(string file)
        {
            var excel = new Bricks.ExcelTable.ExcelImporter();
            if (excel.Init(file))
            {
                var lst = excel.Table2Objects<Manual>();
                Records.Clear();
                foreach (var i in lst)
                {
                    Records[i.FullName] = i;
                }
            }
        }
        public void SaveManual(string file)
        {
            LoadManual(file);
            var newexport = new Bricks.ExcelTable.ExcelExporter();
            newexport.Init("", typeof(Manual));
            List<Manual> lst = new List<Manual>();
            foreach (var i in Methods)
            {
                Manual mnl;
                try
                {
                    var fullName = Rtti.RttiHelper.GetTypeSaveString(i.DeclaringType) + "." + i.Name;
                    if (false == Records.TryGetValue(fullName, out mnl))
                    {
                        mnl = new Manual();
                        mnl.FullName = fullName;
                        lst.Add(mnl);
                    }
                    else
                    {
                        lst.Add(mnl);
                    }
                }
                catch
                {
                    continue;
                }
            }

            foreach (var i in Properties)
            {
                Manual mnl;
                try
                {
                    var fullName = Rtti.RttiHelper.GetTypeSaveString(i.DeclaringType) + "." + i.Name;
                    if (false == Records.TryGetValue(fullName, out mnl))
                    {
                        mnl = new Manual();
                        mnl.FullName = fullName;
                        lst.Add(mnl);
                    }
                    else
                    {
                        lst.Add(mnl);
                    }
                }
                catch
                {
                    continue;
                }
            }
            newexport.Objects2Table<Manual>(lst);
            newexport.Save(file);
        }
    }
}
