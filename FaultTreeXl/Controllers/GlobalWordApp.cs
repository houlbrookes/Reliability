using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Word;

namespace FaultTreeXl
{

    public static class GlobalWordApp
    {
        private static Application _wordApp = null;
        public static Application WordApp
        {
            get
            {
                if (_wordApp == null)
                {
                    try
                    {
                        _wordApp = (Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Word.Application");
                    }
                    catch (System.Runtime.InteropServices.COMException)
                    {
                        // Do nothing, this means there are no running Word Applications 
                        // go on to create a new applicaton
                        // 
                        _wordApp = null;
                    }
                    if (_wordApp == null)
                    {
                        _wordApp = new Application();
                    }
                    _wordApp.Visible = true;
                }
                return _wordApp;
            }
        }

        private static Document _document = null;
        public static Document Document
        {
            get
            {
                if (_document == null)
                {
                    if (WordApp.Documents.Count > 0)
                    {
                        _document = WordApp.ActiveDocument;
                    }
                    if (_document == null)
                    {
                        _document = WordApp.Documents.Add();
                    }
                }

                //_document.PageSetup.Orientation = WdOrientation.wdOrientLandscape;

                return _document;
            }
        }
    
        /// <summary>
        /// Add a string to the end of the document
        /// </summary>
        /// <param name="text"></param>
        public static void AddText(string text)
        {
            try
            {
                Document.Paragraphs.Add().Range.Text = text;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        public static void AddSection(string param)
        {
            // Add a new line
            try
            {
                Document.Paragraphs.Add();
                Document.AddEquation(param);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        public static void AddSection((string text, string equation) param)
        {
            NewLine();
            // Decided to take out the comments as they were not useful and needed
            // to be removed from the document
            //AddText(param.text);
            //NewLine();
            Document.AddEquation(param.equation);
        }

        public static void NewLine()
        {
            try
            {
                Document.Paragraphs.Add();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Adds an string representation
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="equation"></param>
        public static void AddEquation(this Document doc, string equation)
        {
            try
            {
                var para = doc.Paragraphs.Add();
                var paraRange = para.Range;
                paraRange.Text = equation;
                
                OMaths math = doc.OMaths;
                Range mathRange = math.Add(paraRange);
                //OMath equ = mathRange.OMaths[1];
                //equ.BuildUp();
                //if (equation.Contains("="))
                //{
                //    int firstEquals = equ.Range.Text.IndexOf("=");
                //    equ.AlignPoint = firstEquals;
                //}
                math.BuildUp();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }
        /// <summary>
        /// Format a double as Scientific Notation
        /// </summary>
        /// <param name="dbl">Number to Forma</param>
        /// <returns>Formatted Text</returns>
        public static string FormatDouble(this double dbl)
        {
            var lm = dbl.ToString("E2").Split('E');
            var mantissa = lm[0];
            var expon = int.Parse(lm[1]);
            return $"{mantissa}×10^{expon}";
        }
        /// <summary>
        /// Format a Decimal as Scientific Notation
        /// </summary>
        /// <param name="dbl">Number to format</param>
        /// <returns>Formatted String</returns>
        public static string FormatDouble(this decimal dbl)
        {
            var lm = dbl.ToString("E2").Split('E');
            var mantissa = lm[0];
            var expon = int.Parse(lm[1]);
            return $"{mantissa}×10^{expon}";
        }


    }
}
