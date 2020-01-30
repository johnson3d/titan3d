using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.HSSF.UserModel;

namespace EngineNS.Bricks.ExcelTable
{
    public class ExcelBase
    {
        public HSSFWorkbook Workbook
        {
            get;
            set;
        } = new HSSFWorkbook();
        protected NPOI.HSSF.UserModel.HSSFSheet GetSheetSure(string name)
        {
            for (int i = 0; i < Workbook.NumberOfSheets; i++)
            {
                var sheet = Workbook.GetSheetAt(i);
                if (sheet.SheetName == name)
                    return (NPOI.HSSF.UserModel.HSSFSheet)sheet;
            }
            return (NPOI.HSSF.UserModel.HSSFSheet)Workbook.CreateSheet(name);
        }
        protected NPOI.SS.UserModel.IRow GetRowSure(NPOI.SS.UserModel.ISheet sheet, int index)
        {
            if (index == -1)
            {
                index = 1;
                if (sheet.LastRowNum >= 0)
                    index = sheet.LastRowNum + 1;
            }
            var row = sheet.GetRow(index);
            if (row == null)
            {
                row = sheet.CreateRow(index);
            }
            return row;
        }
        protected NPOI.SS.UserModel.ICell GetCellSure(NPOI.SS.UserModel.IRow row, NPOI.SS.UserModel.IRow header, string name, System.Type type)
        {
            int index = -1;
            for (int i = 1; i < header.LastCellNum; i++)
            {
                var headCell = header.GetCell(i);
                //if (headCell == null)
                //    continue;
                if (headCell.StringCellValue == name)
                {
                    index = i;
                    break;
                }
            }
            if (index == -1)
                return null;
            var cell = row.GetCell(index);
            if (cell == null)
            {
                cell = row.CreateCell(index, Type2CellType(type));
            }
            return cell;
        }
        protected NPOI.SS.UserModel.ICell GetCellSure(NPOI.SS.UserModel.IRow row, int index, System.Type type)
        {
            var cell = row.GetCell(index);
            if (cell == null)
            {
                cell = row.CreateCell(index, Type2CellType(type));
            }
            return cell;
        }
        protected int FindText(NPOI.SS.UserModel.IRow row, string text)
        {
            for (int i = 0; i < row.LastCellNum; i++)
            {
                if (row.GetCell(i).StringCellValue == text)
                    return i;
            }
            return -1;
        }
        protected string GetColumnString(int column)
        {
            byte range = (byte)(column / 26);
            if (range > 26)
                return "Error";

            byte index = (byte)(column % 26);
            var baseA = System.Text.Encoding.ASCII.GetBytes("A");
            var lastByte = new byte[1];
            lastByte[0] = (byte)(index + baseA[0]);

            string first = "";
            if (range > 0)
            {
                var firstByte = new byte[1];
                firstByte[0] = (byte)(baseA[0] + range);
                first = System.Text.Encoding.ASCII.GetString(firstByte);
            }
            
            var last = System.Text.Encoding.ASCII.GetString(lastByte);
            return first + last;
        }
        protected NPOI.SS.UserModel.CellType Type2CellType(System.Type type)
        {
            if (type == typeof(sbyte))
            {
                return NPOI.SS.UserModel.CellType.Numeric;
            }
            else if (type == typeof(Int16))
            {
                return NPOI.SS.UserModel.CellType.Numeric;
            }
            else if (type == typeof(Int32))
            {
                return NPOI.SS.UserModel.CellType.Numeric;
            }
            else if (type == typeof(Int64))
            {
                return NPOI.SS.UserModel.CellType.Numeric;
            }
            if (type == typeof(byte))
            {
                return NPOI.SS.UserModel.CellType.Numeric;
            }
            else if (type == typeof(UInt16))
            {
                return NPOI.SS.UserModel.CellType.Numeric;
            }
            else if (type == typeof(UInt32))
            {
                return NPOI.SS.UserModel.CellType.Numeric;
            }
            else if (type == typeof(UInt64))
            {
                return NPOI.SS.UserModel.CellType.Numeric;
            }
            else if (type == typeof(double))
            {
                return NPOI.SS.UserModel.CellType.Numeric;
            }
            else if (type == typeof(float))
            {
                return NPOI.SS.UserModel.CellType.Numeric;
            }
            else if (type == typeof(string))
            {
                return NPOI.SS.UserModel.CellType.String;
            }
            else if (type == typeof(bool))
            {
                return NPOI.SS.UserModel.CellType.Boolean;
            }
            else if (type == typeof(DateTime))
            {
                return NPOI.SS.UserModel.CellType.String;
            }
            return NPOI.SS.UserModel.CellType.Blank;
        }
    }
}
