using Microsoft.Office.Interop.Visio;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using System.Xml.Serialization;

namespace FaultTreeXl
{
    public class DrawAsVisioCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {

            if ((parameter is FaultTreeModel ftm))
            {
                const short visSectionObject= (short) VisSectionIndices.visSectionObject;
                const short visSectionCharacter = (short)VisSectionIndices.visSectionCharacter;
                //const short visSectionParagraph = (short)VisSectionIndices.visSectionParagraph;
                const short visCharacterFont = (short)VisCellIndices.visCharacterFont;
                const short visRowPage = (short) VisRowIndices.visRowPage;
                const short visRowPrintProperties = (short)VisRowIndices.visRowPrintProperties;
                const short visPageWidth = (short) VisCellIndices.visPageWidth;
                const short visPageHeight = (short)VisCellIndices.visPageHeight;
                const short visPrintPropertiesPageOrientation = (short)VisCellIndices.visPrintPropertiesPageOrientation;
                const short visFitPage = (short) VisWindowFit.visFitPage;
                //const short visHorzAlign = (short)VisCellIndices.visHorzAlign;
                const short visRowFill = (short)VisRowIndices.visRowFill;
                const short visFillPattern = (short)VisCellIndices.visFillPattern;
                const short visRowGradientProperties = (short)VisRowIndices.visRowGradientProperties;
                const short visFillGradientEnabled = (short)VisCellIndices.visFillGradientEnabled;
                const short visRowLine = (short)VisRowIndices.visRowLine;
                //const short visRowGradientProperties = (short)VisRowIndices.visRowGradientProperties;
                const short visLinePattern = (short)VisCellIndices.visLinePattern;
                const short visLineGradientEnabled = (short)VisCellIndices.visLineGradientEnabled;

                // Create a new visio application with a document and a single page
                Application theApp = new Application() { Visible = true, };
                Document doc = theApp.Documents.Add("");
                Page page = theApp.ActivePage;

                // Format as A4 Landscape
                theApp.ActiveWindow.ViewFit = visFitPage;
                theApp.ActiveWindow.Page.PageSheet.CellsSRC[visSectionObject, visRowPage, visPageWidth].FormulaU = "297 mm";
                theApp.ActiveWindow.Page.PageSheet.CellsSRC[visSectionObject, visRowPage, visPageHeight].FormulaU = "210 mm";
                theApp.ActiveWindow.Page.PageSheet.CellsSRC[visSectionObject, visRowPrintProperties, visPrintPropertiesPageOrientation].FormulaForceU = "2";

                //// Put the proof test assumptions in a box on the Top lefthand corner
                //var infoTextBox = page.DrawRectangle(0, 0, 1, 0.5);
                //infoTextBox.Text = $"Proof Test Effectiveness: {ftm.ProofTestEffectiveness}\nMission Time: {ftm.MissionTime:N0}";
                //infoTextBox.Cells["PinX"].Formula = "6.35 mm";
                //infoTextBox.Cells["PinY"].Formula = "203.65 mm";
                //infoTextBox.Cells["Width"].Formula = "55 mm";
                //infoTextBox.Cells["Height"].Formula = "15 mm";
                //infoTextBox.Cells["LocPinX"].Formula = "Width*0";
                //infoTextBox.Cells["LocPinY"].Formula = "Height*1";
                //infoTextBox.CellsSRC[visSectionParagraph, 0, visHorzAlign].FormulaU = "0"; //VisHorizontalAlignTypes.visHorzAlignLeft

                // Draw the fault tree
                foreach (GraphicItem graphic in ftm.FaultTree)
                {
                    // Every item has a rectangle with information in it
                    graphic.Rectangle = page.DrawRect(graphic, 10, 0, 80, 50);
                    graphic.Rectangle.AddConnectionPoint(height: 0);
                    graphic.Rectangle.AddConnectionPoint(height: 1.0, fromBelow: false);
                    graphic.Rectangle.Text = $"{graphic.Description}";
                    graphic.Rectangle.Characters.CharProps[(short)VisCellIndices.visCharacterSize] = 8;

                    // draw an ellipse for the Nodes
                    if (graphic is Node)
                    {
                        graphic.BodyShape = page.DrawGraphicEllipse(graphic, x: 22.5, y: 55, width: 55, height: 55);
                        graphic.BodyShape.AddConnectionPoint(width: 0.5, height: 0.0, fromBelow: true);
                        graphic.BodyShape.AddConnectionPoint(width: 0.5, height: 1.0, fromBelow: false);
                    }
                    else if (graphic is DiagnosedFaultNode)
                    {
                        graphic.BodyShape = page.DrawGraphicEllipse(graphic, x: 22.5, y: 55, width: 55, height: 55);
                        graphic.BodyShape.AddConnectionPoint(width: 0.5, height: 0.0, fromBelow: true);
                        graphic.BodyShape.AddConnectionPoint(width: 0.5, height: 1.0, fromBelow: false);
                    }
                    // draw an OR sign for the ORs
                    else if (graphic is OR theOR)
                    {
                        Shape arc3 = page.DrawGraphicArc(graphic, 75, 110, 25, 110, 50, 100);
                        Shape arc4 = page.DrawGraphicArc(graphic, 75, 110, 50, 55, 70, 75);
                        Shape arc5 = page.DrawGraphicArc(graphic, 25, 110, 50, 55, 23, 97);
                        graphic.BodyShape = (new List<Shape> { arc3, arc4, arc5 }).GroupShapes();
                        graphic.BodyShape.AddConnectionPoint(width: 0.5, height: 0.2222, fromBelow: true);
                        graphic.BodyShape.AddConnectionPoint(width: 0.5, height: 1.0, fromBelow: false);
                    }
                    // draw an AND sign for the ANDS
                    else if (graphic is AND theAND)
                    {
                        var yDown = 110;// was 110
                        var yDown2 = 75;// was 85
                        var yTop = 55;// was 65
                        Shape arc3 = page.DrawGraphicArc(graphic, xb:75, yb: yDown2, xe:25, ye: yDown2, xControl:50, yControl:yTop);
                        Shape line1 = page.DrawGraphicLine(graphic, 25, yDown2, 25, yDown);
                        Shape line2 = page.DrawGraphicLine(graphic, 25, yDown, 75, yDown);
                        Shape line3 = page.DrawGraphicLine(graphic, 75, yDown, 75, yDown2);
                        graphic.BodyShape = (new List<Shape> { arc3, line1, line2, line3 }).GroupShapes();
                        graphic.BodyShape.AddConnectionPoint(width: 0.5, height: 0.0, fromBelow: true);
                        graphic.BodyShape.AddConnectionPoint(width: 0.5, height: 1.0, fromBelow: false);
                    }
                    // used for debugging purposes { remove for production code }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                        graphic.BodyShape = page.DrawGraphicEllipse(graphic, x:20, y:60, width: 60, height: 60);
                    }

                    // put the relevant text in the body shape
                    Microsoft.Office.Interop.Visio.Shape TextRect= null;
                    if (graphic is Node theNode)
                    {
                        TextRect = page.DrawRect(graphic, 10, 70, width:80, height:30);
                        TextRect.Text = $"{graphic.Name}";
                        TextRect.Characters.CharProps[(short)VisCellIndices.visCharacterSize] = 8;
                        // show beta factor if included
                        if (theNode.Beta > 0.0)
                        {
                            var BetaRect = page.DrawRect(graphic, 65, 55, width: 45, height: 10);
                            BetaRect.Characters.CharProps[(short)VisCellIndices.visCharacterSize] = 6;
                            BetaRect.CellsSRC[visSectionObject, visRowFill, visFillPattern].FormulaU="0";
                            BetaRect.CellsSRC[visSectionObject, visRowGradientProperties, visFillGradientEnabled].FormulaU = "FALSE";
                            BetaRect.CellsSRC[visSectionCharacter, 0, visCharacterFont].FormulaU = "2";
                            BetaRect.CellsSRC[visSectionObject, visRowLine, visLinePattern].FormulaU = "0";
                            BetaRect.CellsSRC[visSectionObject, visRowGradientProperties, visLineGradientEnabled].FormulaU = "FALSE";
                            var beta = (theNode.Beta/100).ToString("P0");
                            BetaRect.Text = $"b={beta}";
                        }
                    }
                    else
                    {
                        TextRect = page.DrawRect(graphic, 10, 70, width: 80, height: 30);
                        var qText = graphic.PFD.ToString("E2");
                        //TextRect.Text = $"{graphic.Name}\nQ={qText}"; not usefull for AWE
                        TextRect.Text = $"{graphic.Name}";
                        TextRect.Characters.CharProps[(short)VisCellIndices.visCharacterSize] = 8;
                    }
                    graphic.BodyShape.Characters.CharProps[(short)VisCellIndices.visCharacterSize] = 8;

                    // connect the body shape to the rectangle above it
                    graphic.Rectangle.ConnectTo(graphic.BodyShape, secondConnector: 1);

                    if (graphic.Parent != null)
                    {
                        // connect to the parent shape above it
                        graphic.Parent.BodyShape.ConnectTo(graphic.Rectangle, secondConnector: 1);
                    }

                }

                // Autosize the drawing
                page.AutoSizeDrawing();

            }

        }

        private string GetFileName()
        {
            string desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            OpenFileDialog fileDialog = new OpenFileDialog()
            {
                InitialDirectory = desktopFolder,
                DefaultExt = ".xml",
                Filter = "XML documents (.xml)|*.xml",
                CheckFileExists = true,

            };
            if (fileDialog.ShowDialog() == true)
            {
                return fileDialog.FileName;
            }
            return null;
        }

    }

}
