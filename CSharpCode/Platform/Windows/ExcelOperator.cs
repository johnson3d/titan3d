using System;
using System.Collections.Generic;
using System.Text;
using ClosedXML.Excel;

namespace EngineNS.Bricks.DataSet
{
    public partial class TtDataSet
    {
        // ClosedXML 的行号/列号是 1-based；本类对外（RowInSheet、ColumnIndex、HeadRow）沿用 0-based 索引，
        // 所有进出 ClosedXML 的位置统一做 ±1 转换。
        internal XLWorkbook mWorkbook;

        internal static IXLWorksheet FindSheet(XLWorkbook workbook, string name)
        {
            if (workbook == null || string.IsNullOrEmpty(name))
                return null;
            foreach (var ws in workbook.Worksheets)
            {
                if (ws.Name == name)
                    return ws;
            }
            return null;
        }

        partial void LoadDataSet_Exel(ref bool isOk, string name, Type objType)
        {
            System.IO.FileStream fs = System.IO.File.OpenRead(name);
            if (fs == null)
            {
                isOk = false;
            }

            var workbook = new XLWorkbook(fs);
            isOk = LoadDataSetFromExcel(workbook, objType);
        }
        private void UpdateBinderManagerByExcel(XLWorkbook workbook)
        {
            mWorkbook = workbook;
            foreach (var i in BinderManager.Binders)
            {
                if (i.Value.HeadRow >= 0)
                {
                    var sheet = FindSheet(workbook, i.Value.SheetName);
                    if (sheet == null)
                    {
                        Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"DataSet: don't have sheet {i.Value.SheetName}");
                        continue;
                    }
                    var headRow = sheet.Row(i.Value.HeadRow + 1);
                    foreach (var j in i.Value.Fields)
                    {
                        if (j.ColumnIndex == -1)
                        {   
                            j.ColumnIndex = FindColumeIndex(headRow, j.HeadName);
                            if (j.ColumnIndex == -1)
                            {
                                Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"DataSet:Sheet{i.Value.SheetName} don't have {j.HeadName}");
                            }
                        }
                    }
                }
            }
        }
        private int FindColumeIndex(IXLRow row, string name)
        {
            var lastCellNum = row.LastCellUsed()?.Address.ColumnNumber ?? 0;
            for (int i = 1; i <= lastCellNum; i++)
            {
                var cell = row.Cell(i);
                if (cell == null || cell.IsEmpty())
                {
                    continue;
                }
                bool hasError = false;
                var text = TtTable.CellParse(cell, typeof(string), null, out hasError) as string;
                if (text == name)
                    return i - 1;
            }
            return -1;
        }
        private bool LoadDataSetFromExcel(XLWorkbook workbook, Type type)
        {
            var sheetTypes = new List<Type>();
            BinderManager.CollectSheetTypes(type, sheetTypes);
            if (sheetTypes.Count == 0)
                return false;

            UpdateBinderManagerByExcel(workbook);
            
            foreach (var i in sheetTypes)
            {
                var tmp = new TtTable();
                var binder = BinderManager.GetBinder(i);
                tmp.LoadTableFromExcel(this, binder, i);
                Tables.Add(binder.SheetName, tmp);
            }

            var mainBinder = BinderManager.GetBinder(type);
            MainTable = GetTable(mainBinder.SheetName);

            CheckSheetLinks();
            return true;
        }
        partial void SaveToExcel(ref bool isOk, string filepath)
        {
            isOk = false;
            try
            {
                var workbook = new XLWorkbook();
                foreach (var i in Tables)
                {
                    var sheet = GetSheetSure(workbook, i.Value.Binder.SheetName);
                    i.Value.SaveTableToExcel(this, sheet);
                }

                // 用 Create 而不是 OpenOrCreate：旧文件比新内容长时，残留尾部字节会破坏 xlsx(zip) 结构。
                using (var file = new System.IO.FileStream(filepath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    workbook.SaveAs(file);
                }
                workbook.Dispose();
                isOk = true;
            }
            catch (Exception e)
            {
                Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Error, $"DataSet SaveToExcel Error:{e.Message}");
            }
        }
        protected IXLWorksheet GetSheetSure(XLWorkbook workbook, string name)
        {
            var sheet = FindSheet(workbook, name);
            if (sheet != null)
                return sheet;
            return workbook.AddWorksheet(name);
        }
    }
    public partial class TtTable
    {
        IXLWorksheet mSheet;
        internal bool LoadTableFromExcel(TtDataSet dataSet, TtDataProviderBinder binder, Type objType)
        {
            XLWorkbook workbook = dataSet.mWorkbook;
            Binder = dataSet.BinderManager.GetBinder(objType);
            if (Binder == null)
                return false;
            var sheet = TtDataSet.FindSheet(workbook, binder.SheetName);
            if (sheet == null)
            {
                Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"DataSet: don't have sheet {binder.SheetName}");
                return false;
            }
            var lastRowNum = (sheet.LastRowUsed()?.RowNumber() ?? 0) - 1;
            for (int i = Binder.DataStartRow; i <= lastRowNum; i++)
            {
                if (i == binder.HeadRow)
                    continue;
                var row = sheet.Row(i + 1);
                if (row.IsEmpty())
                {
                    break;
                }
                var obj = Rtti.TtTypeDescManager.CreateInstance(objType) as TtDataProvider;
                if (obj == null)
                    return false;

                obj.RowInSheet = i;

                foreach (var j in Binder.Fields)
                {
                    if (j.SheetName != null || j.ColumnIndex == -1)
                        continue;

                    var cell = row.Cell(j.ColumnIndex + 1);
                    if (cell != null && cell.IsEmpty() == false)
                    {
                        bool hasError = false;
                        var value = CellParse(cell, j.PropInfo.PropertyType, j.Conveter, out hasError);
                        if (hasError)
                        {
                            Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"DataSet Sheet({binder.SheetName})[Row={i}, Col={j.ColumnIndex}] format({j.PropInfo.PropertyType.FullName}) error");
                        }
                        else
                        {
                            j.PropInfo.SetValue(obj, value);
                        }
                    }
                }

                DataProviders.Add(obj);
            }

            mSheet = sheet;
            return true;
        }
        internal void SaveTableToExcel(TtDataSet dataSet, IXLWorksheet sheet)
        {
            foreach (var i in DataProviders)
            {
                var row = GetRowSure(sheet, i.RowInSheet);
                foreach (var j in Binder.Fields)
                {
                    var cell = GetCellSure(row, j.ColumnIndex);
                    if (cell == null)
                        continue;
                    SetCellValue(j.PropInfo.GetValue(i), cell, j.Conveter);
                }   
            }
        }
        protected IXLRow GetRowSure(IXLWorksheet sheet, int index)
        {
            return sheet.Row(index + 1);
        }
        protected IXLCell GetCellSure(IXLRow row, int index)
        {
            return row.Cell(index + 1);
        }
        public static object CellParse(IXLCell cell, Type type, TtDataConverter converter, out bool hasError)
        {
            if (type.IsGenericType && type.GetInterface("IList") != null)
            {
                hasError = false;
                var elemType = type.GetGenericArguments()[0];
                var lst = Rtti.TtTypeDescManager.CreateInstance(type) as System.Collections.IList;
                if (elemType == typeof(string))
                {

                }
                else
                {
                    var text = cell.GetFormattedString();
                    if (string.IsNullOrEmpty(text) == false)
                    {
                        var segs = text.Split(',');
                        foreach (var j in segs)
                        {
                            lst.Add(Support.TConvert.ToObject(elemType, j, out hasError));
                        }
                    }
                }
                return lst;
            }
            else
            {
                var result = Support.TConvert.ToObject(type, cell.GetFormattedString(), out hasError);
                if (hasError)
                {
                    Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"XLSX Cell({type.FullName}) convert failed: {cell.GetFormattedString()}");
                }
                return result;
            }
        }
        private void SetCellValue(object value, IXLCell cell, TtDataConverter converter)
        {
            if (value == null)
            {
                cell.Value = "";
                return;
            }
            var type = value.GetType();
            if (type.IsGenericType && type.GetInterface("IList") != null)
            {
                var lst = value as System.Collections.IList;
                var elemType = type.GetGenericArguments()[0];
                if (elemType.GetInterface("IDataProvider") != null)
                {
                    var text = "";
                    foreach(var i in lst)
                    {
                        var data = i as TtDataProvider;
                        if (data == null)
                            continue;
                        text += $"{data.RowInSheet},";
                    }
                    if (text.EndsWith(","))
                        text = text.Substring(0, text.Length - 1);
                    cell.Value = text;
                }
                else
                {
                    if (elemType == typeof(string))
                    {

                    }
                    else
                    {
                        var text = "";
                        foreach (var i in lst)
                        {
                            text += $"{i},";
                        }
                        if (text.EndsWith(","))
                            text = text.Substring(0, text.Length - 1);
                        cell.Value = text;
                    }
                }
            }
            else if (type == typeof(bool))
            {
                cell.Value = (bool)value;
            }
            else if (type == typeof(SByte) || type == typeof(Int16) || type == typeof(Int32) ||
                     type == typeof(byte) || type == typeof(UInt16) || type == typeof(UInt32) ||
                     type == typeof(float) || type == typeof(double))
            {
                // ClosedXML 的单元格数值本质是 double，整数/单精度分支统一转 double 写入。
                cell.Value = Convert.ToDouble(value);
            }
            else if (type == typeof(string))
            {
                cell.Value = (string)value;
            }
            else if (type == typeof(DateTime))
            {
                cell.Value = (DateTime)value;
            }
            else if (type.GetInterface("IDataProvider") != null)
            {
                var subObj = value as TtDataProvider;
                cell.Value = subObj.RowInSheet.ToString();
            }
            else
            {
                cell.Value = value.ToString();
            }
        }
        partial void GetCellText(int row, int col, ref string outText)
        {
            if (mSheet == null)
                return;
            var rowObj = mSheet.Row(row + 1);
            if (rowObj == null || rowObj.IsEmpty())
                return;
            var cell = rowObj.Cell(col + 1);
            if (cell == null || cell.IsEmpty())
                return;
            outText = cell.GetFormattedString();
        }
    }
}
