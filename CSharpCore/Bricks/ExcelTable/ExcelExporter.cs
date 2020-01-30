using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NPOI.HSSF.UserModel;

namespace EngineNS.Bricks.ExcelTable
{
    public class ExcelExporter : ExcelBase
    {
        public void Save(string filepath)
        {
            var file = new System.IO.FileStream(filepath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
            Workbook.Write(file);
            file.Close();
            Workbook.Close();
        }
        public Dictionary<string, ExcelBinder> Binder
        {
            get;
        } = new Dictionary<string, ExcelBinder>();
        public bool Init(string filePath, System.Type type)
        {
            try
            {
                try
                {
                    if (CEngine.Instance.FileManager.FileExists(filePath))
                    {
                        System.IO.FileStream fs = System.IO.File.OpenRead(filePath);
                        if (fs != null)
                        {
                            Workbook = new HSSFWorkbook(fs);
                        }
                    }
                }
                catch
                {
                }
                HSSFSheet sheet = GetSheetSure("MainSheet");
                var row = InitSheet(sheet, type);

                for (int j = 1; j < row.LastCellNum; j++)
                {
                    var cell = row.GetCell(j);

                    var bd = new ExcelBinder();
                    bd.Sheet = sheet;
                    bd.Name = cell.StringCellValue;
                    bd.Column = j;
                    Binder.Add(bd.Name, bd);
                }
            }
            catch (Exception ex)
            {
                EngineNS.Profiler.Log.WriteException(ex);
                return false;
            }
            return true;
        }
        public bool Objects2Table<T>(List<T> lst, Type type = null)
        {
            var sheet = GetSheetSure("MainSheet");
            
            var header = sheet.GetRow(0);
            for (int i = 0; i < lst.Count; i++)
            {
                var obj = lst[i];
                var row = GetRowSure(sheet, 1 + i);

                FillObject2Row(obj, row, header, type == null ? typeof(T) : type);
            }
            return true;
        }

        private void FillObject2Row(object obj, NPOI.SS.UserModel.IRow row, NPOI.SS.UserModel.IRow header, System.Type type)
        {
            var props = type.GetProperties();
            foreach (var j in props)
            {
                var mtAttr = j.GetCustomAttributes(typeof(Rtti.MetaDataAttribute), true);
                if (mtAttr == null || mtAttr.Length == 0)
                    continue;
                var value = j.GetValue(obj, null);
                var cell = GetCellSure(row, header, j.Name, j.PropertyType);

                if (cell==null)
                {
                    EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "Excel", $"Column {j.Name} is not found");
                    continue;
                }

                if (value == null)
                {
                    EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "Excel Value", $"Name {j.Name} is null");
                    continue;
                }
                Object2Cell(j, value, cell);
            }
        }
        private void FillObjectList2Row(System.Collections.IList lst, NPOI.SS.UserModel.IRow row, System.Type type)
        {
            var jmp = GetCellSure(row, 0, typeof(string));
            for (int i = 0; i < lst.Count; i++)
            {
                var obj = lst[i];
                Object2Cell(null, obj, GetCellSure(row, i+1, type));
            }
        }
        private int AddColumn(string name, NPOI.SS.UserModel.IRow header)
        {
            for (int i = 1; i < header.LastCellNum; i++)
            {
                if (header.GetCell(i).StringCellValue == name)
                    return i;
            }
            int index = 1;
            if(header.LastCellNum>0)
                index = header.LastCellNum;
            var cell = header.CreateCell(index, NPOI.SS.UserModel.CellType.String);
            cell.SetCellValue(name);
            return index;
        }
        public void Object2Cell(System.Reflection.PropertyInfo prop, object obj, NPOI.SS.UserModel.ICell cell)
        {
            //如果是一个公式计算结果的Cell，不能往里面生写
            if (cell.CellType == NPOI.SS.UserModel.CellType.Formula)
                return;
            var type = obj.GetType();
            if (type == typeof(sbyte))
            {
                var value = (sbyte)obj;
                cell.SetCellValue(value);
                return;
            }
            else if (type == typeof(Int16))
            {
                var value = (Int16)obj;
                cell.SetCellValue(value);
                return;
            }
            else if (type == typeof(Int32))
            {
                var value = (Int32)obj;
                cell.SetCellValue(value);
                return;
            }
            else if (type == typeof(Int64))
            {
                var value = (Int64)obj;
                cell.SetCellValue(value);
                return;
            }
            if (type == typeof(byte))
            {
                var value = (byte)obj;
                cell.SetCellValue(value);
                return;
            }
            else if (type == typeof(UInt16))
            {
                var value = (UInt16)obj;
                cell.SetCellValue(value);
                return;
            }
            else if (type == typeof(UInt32))
            {
                var value = (UInt32)obj;
                cell.SetCellValue(value);
                return;
            }
            else if (type == typeof(UInt64))
            {
                var value = (UInt64)obj;
                cell.SetCellValue(value);
                return;
            }
            else if (type == typeof(double))
            {
                var value = (double)obj;
                cell.SetCellValue(value);
                return;
            }
            else if (type == typeof(float))
            {
                var value = (float)obj;
                cell.SetCellValue(value);
                return;
            }
            else if (type == typeof(string))
            {
                var value = (string)obj;
                cell.SetCellValue(value);
                return;
            }
            else if (type == typeof(bool))
            {
                var value = (bool)obj;
                cell.SetCellValue(value);
                return;
            }
            else if (type == typeof(DateTime))
            {
                var value = (DateTime)obj;
                cell.SetCellValue(value);
                return;
            }
            else if (type == typeof(Vector2))
            {
                var value = (Vector2)obj;
                cell.SetCellValue(value.ToString());
                return;
            }
            else if (type == typeof(Vector3))
            {
                var value = (Vector3)obj;
                cell.SetCellValue(value.ToString());
                return;
            }
            else if (type == typeof(Vector4))
            {
                var value = (Vector4)obj;
                cell.SetCellValue(value.ToString());
                return;
            }
            else if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var argType = type.GenericTypeArguments[0];
                //var atts = argType.GetCustomAttributes(typeof(EngineNS.IO.Serializer.ExcelSheetAttribute), true);
                //if (atts == null || atts.Length == 0)
                //    return;
                //var sheetAttr = atts[0] as EngineNS.IO.Serializer.ExcelSheetAttribute;

                string sheetName = "LST_" + prop.Name;
                int index = -1;
                var link = cell.Hyperlink as HSSFHyperlink;
                if (link != null && link.Address.Contains($"{sheetName}!A"))
                {
                    var suffix = link.Address.Substring($"{sheetName}!A".Length);
                    index = System.Convert.ToInt32(suffix);
                    if (index > 0)
                        index--;
                }
                var sheet = this.GetSheetSure($"{sheetName}");
                //var header = InitSheet(sheet, type);
                var row = GetRowSure(sheet, index);
                FillObjectList2Row(obj as System.Collections.IList, row, argType);

                if (link == null)
                {
                    link = new HSSFHyperlink(NPOI.SS.UserModel.HyperlinkType.Document);
                }
                link.Address = $"{sheetName}!A{row.RowNum + 1}";
                cell.SetCellValue(link.Address);
                cell.Hyperlink = link;

                var target = row.GetCell(0);
                if (target != null)
                {
                    var linkBack = new HSSFHyperlink(NPOI.SS.UserModel.HyperlinkType.Document);
                    linkBack.Address = $"{cell.Sheet.SheetName}!{GetColumnString(cell.ColumnIndex)}{cell.RowIndex + 1}";
                    target.Hyperlink = linkBack;
                    target.SetCellValue(linkBack.Address);
                }
            }
            else
            {
                var atts = type.GetCustomAttributes(typeof(EngineNS.IO.Serializer.ExcelSheetAttribute), true);
                if (atts == null || atts.Length == 0)
                    return;

                var sheetAttr = atts[0] as EngineNS.IO.Serializer.ExcelSheetAttribute;
                var sheet = this.GetSheetSure(sheetAttr.SheetName);
                var header = InitSheet(sheet, type);

                string sheetName = sheetAttr.SheetName;
                int index = -1;
                var link = cell.Hyperlink as HSSFHyperlink;
                if (link != null && link.Address.Contains($"{sheetName}!A"))
                {
                    var suffix = link.Address.Substring($"{sheetName}!A".Length);
                    index = System.Convert.ToInt32(suffix);
                    if (index > 0)
                        index--;
                }
                var row = GetRowSure(sheet, index);
                FillObject2Row(obj, row, header, type);

                if (link == null)
                {
                    link = new HSSFHyperlink(NPOI.SS.UserModel.HyperlinkType.Document);
                }
                link.Address = $"{sheetName}!A{row.RowNum + 1}";
                cell.SetCellValue(link.Address);
                cell.Hyperlink = link;

                var target = GetCellSure(row, 0, typeof(string));
                if(target!=null)
                {
                    var linkBack = new HSSFHyperlink(NPOI.SS.UserModel.HyperlinkType.Document);
                    linkBack.Address = $"{cell.Sheet.SheetName}!{GetColumnString(cell.ColumnIndex)}{cell.RowIndex+1}";
                    target.Hyperlink = linkBack;
                    target.SetCellValue(linkBack.Address);
                }
            }
        }
        private NPOI.SS.UserModel.IRow InitSheet(NPOI.SS.UserModel.ISheet sheet, System.Type type)
        {
            var row = GetRowSure(sheet, 0);
            var props = type.GetProperties();

            var jmp = GetCellSure(row, 0, typeof(string));
            if (jmp.RichStringCellValue!=null && jmp.StringCellValue == "@Jump")
                return row;

            jmp.SetCellValue("@Jump");
            foreach (var i in props)
            {
                var mtAttr = i.GetCustomAttributes(typeof(Rtti.MetaDataAttribute), true);
                if (mtAttr == null || mtAttr.Length == 0)
                    continue;
                AddColumn(i.Name, row);
            }
            return row;
        }
    }

    public class ExcelHelper : EngineNS.BrickDescriptor
    {
        public override async System.Threading.Tasks.Task DoTest()
        {
            await base.DoTest();
        }
        #region Test
        [EngineNS.IO.Serializer.ExcelSheetAttribute("XlslSubObject")]
        public static void Test()
        {
            ExcelImporter.Test();

            var exp = new ExcelExporter();
            var rn = EngineNS.RName.GetRName("GameTable/XlslObject.xls");
            exp.Init(rn.Address, typeof(DataProvider.XlslObject));

            var lst = new List<DataProvider.XlslObject>();
            var obj = new DataProvider.XlslObject();
            for (int i = 0; i < 10; i++)
            {
                obj = new DataProvider.XlslObject();
                obj.D = $"D_{i}";
                obj.F.Add(new DataProvider.XlslSubObject());
                obj.F.Add(new DataProvider.XlslSubObject());
                var sobj = new DataProvider.XlslSubObject();
                sobj.A = i;
                obj.F.Add(sobj);
                obj.G.Add($"abc{i}");
                obj.G.Add("efg");
                obj.G.Add("123");
                lst.Add(obj);
            }
            //lst.Add(obj);
            //obj = new XlslObject();
            //obj.A = 10;
            //lst.Add(obj);
            //obj = new XlslObject();
            //obj.D = "AXEXTEXT";
            //obj.F.Add(new XlslSubObject());
            //obj.F.Add(new XlslSubObject());
            //obj.F.Add(new XlslSubObject());
            //obj.G.Add("abc");
            //obj.G.Add("efg");
            //obj.G.Add("123");
            //lst.Add(obj);
            exp.Objects2Table(lst);
            exp.Save(rn.Address);
        }
        #endregion
    }
}
