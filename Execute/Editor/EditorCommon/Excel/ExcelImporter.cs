//#define FieldExcelTable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.HSSF.UserModel;

namespace EditorCommon.Excel
{
    public class ExcelBinder
    {
        public string Name;
#if !FieldExcelTable
        public System.Reflection.PropertyInfo FieldInfo;
#else
        public System.Reflection.FieldInfo FieldInfo;
#endif
        public int Column;
        public HSSFSheet Sheet;
    }
    public class ExcelImporter : ExcelBase
    {
        public Dictionary<string, ExcelBinder> Binder
        {
            get;
        } = new Dictionary<string, ExcelBinder>();
        public ExcelBinder FindBinder(string name)
        {
            ExcelBinder binder;
            if (Binder.TryGetValue(name, out binder) == false)
                return null;
            return binder;
        }
        public bool Init(string filePath)
        {
            try
            {
                System.IO.FileStream fs = System.IO.File.OpenRead(filePath);
                if (fs == null)
                    return false;

                Workbook = new HSSFWorkbook(fs);
                HSSFSheet sheet = (HSSFSheet)Workbook.GetSheet("MainSheet");

                var row = sheet.GetRow(0);
                for (int j = 0; j < row.LastCellNum; j++)
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
        public List<T> Table2Objects<T>() where T : new()
        {
            try
            {
                List<T> result = new List<T>();
                System.Type type = typeof(T);
#if !FieldExcelTable
                var props = type.GetProperties();
#else
                var props = type.GetFields();
#endif

                foreach (var i in props)
                {
                    var binder = FindBinder(i.Name);
                    if (binder == null)
                        continue;

                    binder.FieldInfo = i;
                }

                HSSFSheet sheet = (HSSFSheet)Workbook.GetSheet("MainSheet");

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row == null)
                    {
                        break;
                    }
                    var obj = new T();
                    foreach (var f in Binder)
                    {
                        if (f.Value.FieldInfo == null)
                            continue;
                        var cell = row.GetCell(f.Value.Column);
                        bool doSet;

#if !FieldExcelTable
                        var value = Cell2Object(cell, f.Value.FieldInfo.PropertyType, out doSet);
#else
                        var value = Cell2Object(cell, f.Value.FieldInfo.FieldType, out doSet);
#endif
                        if (doSet)
                        {
                            try
                            {
                                f.Value.FieldInfo.SetValue(obj, value);
                            }
                            catch (Exception ex)
                            {
                                EngineNS.Profiler.Log.WriteException(ex);
                            }
                        }

                    }
                    result.Add(obj);
                }
                return result;
            }
            catch (Exception ex)
            {
                EngineNS.Profiler.Log.WriteException(ex);
                return null;
            }
        }

        public object Cell2Object(NPOI.SS.UserModel.ICell cell, System.Type type, out bool doSet)
        {
            if (cell.Hyperlink != null)
            {
                var segs = cell.Hyperlink.Address.Split('!');
                if(segs.Length!=2)
                {
                    doSet = false;
                    EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "Excel", $"HyperLink({cell.Hyperlink}) is invalid");
                    return null;
                }
                var tab = segs[0];
                var linkCell = segs[1];
                var sheet = Workbook.GetSheet(tab);
                if (sheet == null)
                {
                    doSet = false;
                    return null;
                }

                int col, row;
                GetCellPosition(linkCell, out col, out row);
                var rowObj = sheet.GetRow(row);
                if (rowObj == null)
                {
                    doSet = false;
                    return null;
                }

                var linker = rowObj.GetCell(col);
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                {//是一个数组
                    var obj = System.Activator.CreateInstance(type) as System.Collections.IList;
                    if(linker!=null)
                    {
                        FillObjectList(obj, type.GenericTypeArguments[0], linker.Row);
                    }
                    doSet = true;
                    return obj;
                }
                else
                {//是一个对象
                    if (linker == null)
                    {
                        doSet = false;
                        return null;
                    }

                    var atts = type.GetCustomAttributes(typeof(EngineNS.IO.Serializer.ExcelSheetAttribute), true);
                    if (atts == null || atts.Length == 0)
                    {
                        doSet = false;
                        return null;
                    }

                    var obj = System.Activator.CreateInstance(type);
                    if (obj != null)
                    {
                        FillObject(obj, linker.Row);
                    }
                    doSet = true;
                    return obj;
                }
            }
            else
            {
                doSet = true;
                if (type == typeof(sbyte))
                {
                    return System.Convert.ToSByte(cell.ToString());
                }
                else if (type == typeof(Int16))
                {
                    return System.Convert.ToInt16(cell.ToString());
                }
                else if (type == typeof(Int32))
                {
                    return System.Convert.ToInt32(cell.ToString());
                }
                else if (type == typeof(Int64))
                {
                    return System.Convert.ToInt64(cell.ToString());
                }
                if (type == typeof(byte))
                {
                    return System.Convert.ToByte(cell.ToString());
                }
                else if (type == typeof(UInt16))
                {
                    return System.Convert.ToUInt16(cell.ToString());
                }
                else if (type == typeof(UInt32))
                {
                    return System.Convert.ToUInt32(cell.ToString());
                }
                else if (type == typeof(UInt64))
                {
                    return System.Convert.ToUInt64(cell.ToString());
                }
                else if (type == typeof(double))
                {
                    return System.Convert.ToDouble(cell.ToString());
                }
                else if (type == typeof(float))
                {
                    return System.Convert.ToSingle(cell.ToString());
                }
                else if (type == typeof(string))
                {
                    return cell.StringCellValue;
                }
                else if (type == typeof(bool))
                {
                    return cell.BooleanCellValue;
                }
                else if (type == typeof(DateTime))
                {
                    return System.Convert.ToDateTime(cell.ToString());
                }
                doSet = false;
            }
            return null;
        }
        public bool GetCellPosition(string cell, out int col, out int row)
        {
            cell = cell.Substring(1);
            col = 0;
            row = System.Convert.ToInt32(cell) - 1;
            return true;
        }

        public void FillObject(object obj, NPOI.SS.UserModel.IRow row)
        {
#if !FieldExcelTable
            var props = obj.GetType().GetProperties();
#else
            var props = obj.GetType().GetFields();
#endif
            var header = row.Sheet.GetRow(0);
            foreach (var i in props)
            {
                var idx = FindText(header, i.Name);
                if (idx == -1)
                    continue;

                var cell = row.GetCell(idx);
                bool doSet;
#if !FieldExcelTable
                var value = Cell2Object(cell, i.PropertyType, out doSet);
#else
                var value = Cell2Object(cell, i.FieldType, out doSet);
#endif
                if (doSet)
                    i.SetValue(obj, value);
            }
        }
        
        public void FillObjectList(System.Collections.IList lst, System.Type elemType, NPOI.SS.UserModel.IRow row)
        {
            for (int i = 1; i < row.LastCellNum; i++)
            {
                var cell = row.GetCell(i);
                if (cell == null)
                    continue;

                object obj = null;
                if (cell.Hyperlink != null)
                {
                    var segs = cell.Hyperlink.Address.Split('!');
                    if (segs.Length != 2)
                    {
                        EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "Excel", $"HyperLink({cell.Hyperlink}) is invalid");
                        continue;
                    }
                    var tab = segs[0];
                    var linkCell = segs[1];
                    var sheet = Workbook.GetSheet(tab);
                    if(sheet==null)
                    {
                        EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "Excel", $"Sheet({tab}) is invalid");
                        continue;
                    }

                    int colIdx, rowIdx;
                    GetCellPosition(linkCell, out colIdx, out rowIdx);
                    var rowObj = sheet.GetRow(rowIdx);
                    if (rowObj == null)
                    {
                        EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "Excel", $"HyperLink({cell.Hyperlink}) target(row) is null");
                        continue;
                    }

                    obj = System.Activator.CreateInstance(elemType);
                    FillObject(obj, rowObj);
                }
                else
                {
                    bool doSet;
                    obj = Cell2Object(cell, elemType, out doSet);
                }
                lst.Add(obj);
            }
        }
        public static void Test()
        {
            var imp = new ExcelImporter();
            var rn = EngineNS.RName.GetRName("GameTable/XlslObject.xls");
            imp.Init(rn.Address);

            var objs = imp.Table2Objects<ExcelHelper.XlslObject>();
            if(objs!=null)
            {

            }
        }
    }
}
