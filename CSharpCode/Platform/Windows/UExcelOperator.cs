using System;
using System.Collections.Generic;
using System.Text;
using NPOI.HSSF.UserModel;

namespace EngineNS.Bricks.DataSet
{
    public partial class TtDataSet
    {
        internal HSSFWorkbook mWorkbook;
        partial void LoadDataSet_Exel(ref bool isOk, RName name, Type objType)
        {
            System.IO.FileStream fs = System.IO.File.OpenRead(name.Address);
            if (fs == null)
            {
                isOk = false;
            }

            var workbook = new NPOI.HSSF.UserModel.HSSFWorkbook(fs);
            isOk = LoadDataSetFromExcel(workbook, objType);
        }
        private bool LoadDataSetFromExcel(HSSFWorkbook workbook, Type type)
        {
            var sheetTypes = new List<Type>();
            BinderManager.CollectSheetTypes(type, sheetTypes);
            if (sheetTypes.Count == 0)
                return false;

            mWorkbook = workbook;
            foreach (var i in sheetTypes)
            {
                var tmp = new TtTable();
                var binder = BinderManager.GetBinder(i);
                tmp.LoadTableFromExcel(this, binder.SheetName, i);
                Tables.Add(binder.SheetName, tmp);
            }

            var mainBinder = BinderManager.GetBinder(type);
            MainTable = GetTable(mainBinder.SheetName);

            CheckSheetLinks();
            return true;
        }
        internal void SaveDataSetToExcel(string filepath)
        {
            HSSFWorkbook workbook = new HSSFWorkbook();
            foreach (var i in Tables)
            {
                var sheet = GetSheetSure(workbook, i.Value.Binder.SheetName);
                i.Value.SaveTableToExcel(this, sheet);
            }

            var file = new System.IO.FileStream(filepath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
            workbook.Write(file);
            file.Close();
            workbook.Close();
        }
        protected NPOI.HSSF.UserModel.HSSFSheet GetSheetSure(HSSFWorkbook workbook, string name)
        {
            for (int i = 0; i < workbook.NumberOfSheets; i++)
            {
                var sheet = workbook.GetSheetAt(i);
                if (sheet.SheetName == name)
                    return (NPOI.HSSF.UserModel.HSSFSheet)sheet;
            }
            return (NPOI.HSSF.UserModel.HSSFSheet)workbook.CreateSheet(name);
        }
    }
    public partial class TtTable
    {
        HSSFSheet mSheet;
        internal bool LoadTableFromExcel(TtDataSet dataSet, string sheetName, Type objType)
        {
            HSSFWorkbook workbook = dataSet.mWorkbook;
            Binder = dataSet.BinderManager.GetBinder(objType);
            if (Binder == null)
                return false;
            var sheet = (HSSFSheet)workbook.GetSheet(sheetName);
            for (int i = 0; i <= sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                if (row == null)
                {
                    break;
                }
                var obj = Rtti.TtTypeDescManager.CreateInstance(objType) as IDataProvider;
                if (obj == null)
                    return false;

                obj.RowInSheet = i;

                foreach (var j in Binder.Fields)
                {
                    if (j.SheetName != null)
                        continue;

                    var cell = row.GetCell(j.ColumnIndex);
                    if (cell != null)
                    {
                        j.PropInfo.SetValue(obj, CellParse(cell, j.PropInfo.PropertyType, j.Conveter));
                    }
                }

                DataProviders.Add(obj);
            }

            mSheet = sheet;
            return true;
        }
        internal void SaveTableToExcel(TtDataSet dataSet, HSSFSheet sheet)
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
        protected NPOI.SS.UserModel.IRow GetRowSure(NPOI.SS.UserModel.ISheet sheet, int index)
        {
            var row = sheet.GetRow(index);
            if (row == null)
            {
                row = sheet.CreateRow(index);
            }
            return row;
        }
        protected NPOI.SS.UserModel.ICell GetCellSure(NPOI.SS.UserModel.IRow row, int index)
        {
            var cell = row.GetCell(index);
            if (cell == null)
            {
                cell = row.CreateCell(index);
            }
            return cell;
        }
        private object CellParse(NPOI.SS.UserModel.ICell cell, Type type, UDataConverter converter)
        {
            if (type.IsGenericType && type.GetInterface("IList") != null)
            {
                var elemType = type.GetGenericArguments()[0];
                var lst = Rtti.TtTypeDescManager.CreateInstance(type) as System.Collections.IList;
                if (elemType == typeof(string))
                {

                }
                else
                {
                    var text = cell.ToString();
                    if (string.IsNullOrEmpty(text) == false)
                    {
                        var segs = cell.ToString().Split(',');
                        foreach (var j in segs)
                        {
                            lst.Add(Support.TConvert.ToObject(elemType, j));
                        }
                    }
                }
                return lst;
            }
            else
            {
                return Support.TConvert.ToObject(type, cell.ToString());
            }
        }
        private void SetCellValue(object value, NPOI.SS.UserModel.ICell cell, UDataConverter converter)
        {
            if (value == null)
            {
                cell.SetCellValue("");
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
                        var data = i as IDataProvider;
                        if (data == null)
                            continue;
                        text += $"{data.RowInSheet},";
                    }
                    if (text.EndsWith(","))
                        text = text.Substring(0, text.Length - 1);
                    cell.SetCellValue(text);
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
                        cell.SetCellValue(text);
                    }
                }
            }
            else if (type == typeof(bool))
            {
                cell.SetCellValue((bool)value);
            }
            else if (type == typeof(SByte))
            {
                cell.SetCellValue((SByte)value);
            }
            else if (type == typeof(Int16))
            {
                cell.SetCellValue((Int16)value);
            }
            else if (type == typeof(Int32))
            {
                cell.SetCellValue((Int32)value);
            }
            else if (type == typeof(byte))
            {
                cell.SetCellValue((byte)value);
            }
            else if (type == typeof(UInt16))
            {
                cell.SetCellValue((UInt16)value);
            }
            else if (type == typeof(UInt32))
            {
                cell.SetCellValue((UInt32)value);
            }
            else if (type == typeof(string))
            {
                cell.SetCellValue((string)value);
            }
            else if (type == typeof(float))
            {
                cell.SetCellValue((float)value);
            }
            else if (type == typeof(double))
            {
                cell.SetCellValue((double)value);
            }
            else if (type == typeof(DateTime))
            {
                cell.SetCellValue((DateTime)value);
            }
            else if (type.GetInterface("IDataProvider") != null)
            {
                var subObj = value as IDataProvider;
                cell.SetCellValue(subObj.RowInSheet.ToString());
            }
            else
            {
                cell.SetCellValue(value.ToString());
            }
        }
        partial void GetCellText(int row, int col, ref string outText)
        {
            if (mSheet == null)
                return;
            var rowObj = mSheet.GetRow(row);
            if (rowObj == null)
                return;
            var cell = rowObj.GetCell(col);
            if (cell == null)
                return;
            outText = cell.ToString();
        }
    }
}
