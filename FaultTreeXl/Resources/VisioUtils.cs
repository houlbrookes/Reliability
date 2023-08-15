using Microsoft.Office.Interop.Visio;
using System.Collections.Generic;
using System.Linq;

namespace FaultTreeXl
{
    static public class VisioUtils
    {
        /// <summary>
        /// Connect two shapes with a connector
        /// </summary>
        /// <param name="firstShape">BeginX shape</param>
        /// <param name="secondShape">EndX shape</param>
        /// <returns>the connector shape</returns>
        static public Shape ConnectTo(this Shape firstShape, Shape secondShape, short firstConnector=0, short secondConnector=0)
        {
            const short visConPts = (short)VisSectionIndices.visSectionConnectionPts;

            Application theApp = firstShape.Application;
            Shape connector = firstShape.ContainingPage.Drop(theApp.ConnectorToolDataObject, 0, 0);
            var connectionPoints = firstShape.Section[visConPts];
            if (connectionPoints.Count >firstConnector)
            {
                connector.CellsU["BeginX"].GlueTo(firstShape.CellsSRC[(short)VisSectionIndices.visSectionConnectionPts, firstConnector, 0]);
            }
            else
            {
                connector.CellsU["BeginX"].GlueTo(firstShape.CellsU["PinX"]);
            }

            connectionPoints = secondShape.Section[visConPts];
            if (connectionPoints.Count > secondConnector)
            {
                connector.CellsU["EndX"].GlueTo(secondShape.CellsSRC[(short)VisSectionIndices.visSectionConnectionPts, secondConnector, 0]);
            } else
            {
                connector.CellsU["EndX"].GlueTo(secondShape.CellsU["PinX"]);
            }
            return connector;
        }

        static public double ScaledX(this SFFDisplay thisItem, double offset) => (thisItem.X / 1.3 + offset) / 100;
        static public double ScaledY(this SFFDisplay thisItem, double offset) => 8 - (thisItem.Y + offset) / 100;

        static public double ScaledX(this GraphicItem thisItem, double offset) => (thisItem.X/1.3 + offset) / 100;
        static public double ScaledY(this GraphicItem thisItem, double offset) => 8 - (thisItem.Y + offset) / 100;

        static public Shape DrawRect(this Page page, SFFDisplay theGraphic, double x, double y, double width, double height) =>
            page.DrawRectangle(theGraphic.ScaledX(x), theGraphic.ScaledY(y),
                               theGraphic.ScaledX(x + width), theGraphic.ScaledY(y + height));


        static public Shape DrawRect(this Page page, GraphicItem theGraphic, double x, double y, double width, double height) =>
            page.DrawRectangle(theGraphic.ScaledX(x), theGraphic.ScaledY(y),
                               theGraphic.ScaledX(x + width), theGraphic.ScaledY(y + height));

        static public Shape DrawGraphicEllipse(this Page page, SFFDisplay theGraphic, double x, double y, double width, double height)
            => page.DrawOval(theGraphic.ScaledX(x), theGraphic.ScaledY(y), theGraphic.ScaledX(x + width), theGraphic.ScaledY(y + width));

        static public Shape DrawGraphicEllipse(this Page page, GraphicItem theGraphic, double x, double y, double width, double height)
            => page.DrawOval(theGraphic.ScaledX(x), theGraphic.ScaledY(y), theGraphic.ScaledX(x + width), theGraphic.ScaledY(y + width));


        static public Shape DrawGraphicArc(this Page thisPage, SFFDisplay theGraphic, double xb, double yb, double xe, double ye, double xControl, double yControl)
        =>
            thisPage.DrawArcByThreePoints(
                xBegin: theGraphic.ScaledX(xb), yBegin: theGraphic.ScaledY(yb),
                xEnd: theGraphic.ScaledX(xe), yEnd: theGraphic.ScaledY(ye),
                xControl: theGraphic.ScaledX(xControl),
                yControl: theGraphic.ScaledY(yControl));

        static public Shape DrawGraphicArc(this Page thisPage, GraphicItem theGraphic, double xb, double yb, double xe, double ye, double xControl, double yControl)
        =>
            thisPage.DrawArcByThreePoints(
                xBegin: theGraphic.ScaledX(xb), yBegin: theGraphic.ScaledY(yb),
                xEnd: theGraphic.ScaledX(xe), yEnd: theGraphic.ScaledY(ye),
                xControl: theGraphic.ScaledX(xControl),
                yControl: theGraphic.ScaledY(yControl));

        static public Shape DrawGraphicLine(this Page thisPage, GraphicItem theGraphic, double xb, double yb, double xe, double ye)
        =>
            thisPage.DrawLine(
                xBegin: theGraphic.ScaledX(xb), yBegin: theGraphic.ScaledY(yb),
                xEnd: theGraphic.ScaledX(xe), yEnd: theGraphic.ScaledY(ye));

        static public Shape DrawGraphicLine(this Page thisPage, SFFDisplay theGraphic, double xb, double yb, double xe, double ye)
        =>
            thisPage.DrawLine(
                xBegin: theGraphic.ScaledX(xb), yBegin: theGraphic.ScaledY(yb),
                xEnd: theGraphic.ScaledX(xe), yEnd: theGraphic.ScaledY(ye));

        static public Shape DuplicateAndMove(this Shape theShape, GraphicItem theGraphic, double beginX, double beginY, double endX, double endY)
        {
            Application theApp = theShape.Application;

            Shape duplicatedShape = theShape.Duplicate();
            duplicatedShape.CellsU["BeginX"].ResultIU = theGraphic.ScaledX(beginX);
            duplicatedShape.CellsU["BeginY"].ResultIU = theGraphic.ScaledY(beginY);
            duplicatedShape.CellsU["EndX"].ResultIU = theGraphic.ScaledX(endX);
            duplicatedShape.CellsU["EndY"].ResultIU = theGraphic.ScaledY(endY);

            return duplicatedShape;
        }

        static public void AddConnectionPoint(this Shape theShape, double width = 0.5, double height = 0.2222, bool fromBelow=true)
        {
            const short visConPts = (short)VisSectionIndices.visSectionConnectionPts;
            const short visLstRw = (short)VisRowIndices.visRowLast;
            const short visTgCnPt = (short)VisRowTags.visTagCnnctPt;
            const short visRwCnPts = (short)VisRowIndices.visRowConnectionPts; // ***
            const short visCellX = (short)VisCellIndices.visX;
            const short visCellY = (short)VisCellIndices.visY;
            const short visCellDirX = (short)VisCellIndices.visCnnctDirX;
            const short visCellDirY = (short)VisCellIndices.visCnnctDirY;

            var intRowIndex1 = theShape.AddRow(visConPts, visLstRw, visTgCnPt);
            var vsoRow1 = theShape.Section[visConPts];

            var res = theShape.CellsSRCExists[visConPts, visRwCnPts, visCellX, (short)VisExistsFlags.visExistsLocally];
            var vsX = theShape.CellsSRC[visConPts, intRowIndex1, visCellX];
            var vsY = theShape.CellsSRC[visConPts, intRowIndex1, visCellY];

            vsX.Formula = $"Width*{width}";
            vsY.Formula = $"Height*{height}";
            var vsDirX = theShape.CellsSRC[visConPts, intRowIndex1, visCellDirX];
            var vsDirY = theShape.CellsSRC[visConPts, intRowIndex1, visCellDirY];

            vsDirX.Formula = fromBelow?"0 mm":"0 mm";// was 1,0
            vsDirY.Formula = fromBelow ? "1 mm" : "0 mm";
        }

        static public Shape GroupShapes<T>(this T theShapeList) where T : IEnumerable<Shape>
        {
            if (theShapeList.Count() > 0)
            {
                Selection theSelection = theShapeList.First().ContainingPage.Application.ActiveWindow.Selection;
                theSelection.Select(theShapeList.First(), (short)VisSelectArgs.visDeselectAll + (short)VisSelectArgs.visSelect);
                foreach (Shape theNextShape in theShapeList.Skip(1))
                {
                    theSelection.Select(theNextShape, (short)VisSelectArgs.visSelect);
                }

                return theSelection.Group();
            }
            else
            {
                return null;
            }
        }
    }

}
