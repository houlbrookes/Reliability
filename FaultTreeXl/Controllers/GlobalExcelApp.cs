using System;
using System.Linq;
using Microsoft.Office.Interop.Excel;

namespace FaultTreeXl
{
    public static class GlobalExcelApp
    {
        private static Application _excelApp = null;
        public static Application ExcelApp
        {
            get
            {
                if (_excelApp == null)
                {
                    try
                    {
                        _excelApp = (Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
                    }
                    catch (System.Runtime.InteropServices.COMException)
                    {
                        // Do nothing, this means there are no running Word Applications 
                        // go on to create a new applicaton
                        // 
                        _excelApp = null;
                    }
                    if (_excelApp == null)
                    {
                        _excelApp = new Application();
                    }
                    _excelApp.Visible = true;
                }
                return _excelApp;
            }
        }

        private static Workbook _workbook = null;
        public static Workbook TheWorkbook
        {
            get
            {
                if (_workbook == null)
                {
                    if (ExcelApp.Workbooks.Count > 0)
                    {
                        _workbook = ExcelApp.ActiveWorkbook;
                    }
                    if (_workbook == null)
                    {
                        _workbook = ExcelApp.Workbooks.Add();
                    }
                }

                //_document.PageSetup.Orientation = WdOrientation.wdOrientLandscape;

                return _workbook;
            }
        }

        public static void NewWorksheet()
        {
            Worksheet ws = TheWorkbook.Worksheets.Add();
            ws.Activate();
        }

        public static Worksheet TheActiveWorksheet
        {
            get => TheWorkbook.ActiveSheet;
        }

        public static XLRange AssignValue(this XLRange thisRange, object theValue)
        {
            thisRange.Range.Value = theValue;
            return thisRange;
        }

        public static XLRange Bold(this XLRange thisRange)
        {
            thisRange.Range.Font.Bold = true;
            return thisRange;
        }

        public static XLRange AlignCentre(this XLRange thisRange)
        {
            thisRange.Range.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            return thisRange;
        }

        public static XLRange AlignLeft(this XLRange thisRange)
        {
            thisRange.Range.HorizontalAlignment = XlHAlign.xlHAlignLeft;
            return thisRange;
        }
        public static XLRange AlignRight(this XLRange thisRange)
        {
            thisRange.Range.HorizontalAlignment = XlHAlign.xlHAlignRight;
            return thisRange;
        }

        public static XLRange Subscript(this XLRange thisRange, int from, int to)
        {
            thisRange.Range.Characters[from, to].Font.Subscript = true;
            return thisRange;
        }

        public static XLRange SetNumberFormat(this XLRange thisRange, string theNumberFormat)
        {
            thisRange.Range.NumberFormat = theNumberFormat;
            return thisRange;
        }

        public static XLRange SetScientificFormat(this XLRange thisRange)
        {
            thisRange.SetNumberFormat("0.00E+00");
            return thisRange;
        }
        public static XLRange SetThousandsFormat(this XLRange thisRange)
        {
            thisRange.SetNumberFormat(@"#,###,##0");
            return thisRange;
        }
        public static XLRange SetPercentFormat(this XLRange thisRange)
        {
            thisRange.SetNumberFormat(@"0%");
            return thisRange;
        }

        public static XLRange MergeCells(this XLRange thisRange)
        {
            thisRange.Range.MergeCells = true;
            return thisRange;
        }

        public static XLRange NameRange(this XLRange thisRange, string rangeName)
        {
            thisRange.Range.Name = rangeName;
            return thisRange;
        }

        public static string addr(this (int, int) thisAddress )
        {
            return GetAddress(thisAddress.Item1, thisAddress.Item2);
        }

        public static string GetAddress(int row, int col) => TheActiveWorksheet.Cells[row, col].Address(false, false);

        public static XLRange GetRange(int row1, int col1, int row2, int col2)
        {
            return new XLRange(TheActiveWorksheet.Range[(row1, col1).addr(), (row2, col2).addr()]);
        }

        public static Range UsedRange => TheActiveWorksheet.UsedRange;

        public static XLRange Cell(int row, int col)
        {
            var theRange = new XLRange(TheActiveWorksheet.Cells[row, col]);
            return theRange;
        }

        public static XLRange CreateRange(int row1, int col1, int row2, int col2)
        {
            var fromAddress = $"{GetAddress(row1, col1)}";
            var toAddress = $"{GetAddress(row2, col2)}";
            var theRange = new XLRange(TheActiveWorksheet.Range[fromAddress, toAddress]);
            return theRange;
        }

        public static void SetWorksheetName(string theWorksheetName)
        {
            TheActiveWorksheet.Name = theWorksheetName;
        }
    }

    public class XLRange
    {
        Range xlRange = null;

        public Range Range { get => xlRange; }
        public XLRange(Range theXlRange)
        {
            xlRange = theXlRange;
        }
    }
}
