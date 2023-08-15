using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using media = System.Windows.Media;

namespace FaultTreeXl
{
    class DraggableAdorner : Adorner
    {
        //Rect renderRect;
        //Brush renderBrush;

        public System.Drawing.Point CenterOffset;
        public DraggableAdorner(UIElement adornedElement) : base(adornedElement)
        {
            //renderRect = new Rect(adornedElement.RenderSize);
            //this.IsHitTestVisible = false;
            ////Clone so that it can be modified with on modifying the original
            //renderBrush = adornedElement.Background.Clone();
            //CenterOffset = new Point(-renderRect.Width / 2, -renderRect.Height / 2);
        }
        protected override void OnRender(media.DrawingContext drawingContext)
        {
            var adornedElementRect = new Rect(this.AdornedElement.DesiredSize);
            var renderBrush = new media.SolidColorBrush(media.Colors.Green);
            renderBrush.Opacity = 0.2;
            var renderPen = new System.Windows.Media.Pen(new media.SolidColorBrush(media.Colors.Yellow), 1.5);
            var centre = adornedElementRect.Location;

            this.IsHitTestVisible = false;
            // Draw a rectantale around the shape
            drawingContext.DrawRectangle(renderBrush, renderPen, adornedElementRect);
            //drawingContext.DrawEllipse(renderBrush, renderPen, centre, 5, 5);
        }
    }

    class CanDropAdorner : Adorner
    {
        public CanDropAdorner(UIElement adornedElement) : base(adornedElement)
        { 
        }

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var adornedElementRect = new Rect(this.AdornedElement.DesiredSize);
            var renderBrush = new media.SolidColorBrush(media.Colors.Green);
            renderBrush.Opacity = 0.2;
            var renderPen = new media.Pen(new media.SolidColorBrush(media.Colors.Green), 1.5);

            this.IsHitTestVisible = false;
            drawingContext.DrawRectangle(renderBrush, renderPen, adornedElementRect);
        }
    }

    class CannotDropAdorner : Adorner
    {
        public CannotDropAdorner(UIElement adornedElement) : base(adornedElement)
        {
        }

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var adornedElementRect = new Rect(this.AdornedElement.DesiredSize);
            var renderBrush = new media.SolidColorBrush(media.Colors.Red);
            renderBrush.Opacity = 0.2;
            var renderPen = new media.Pen(new media.SolidColorBrush(media.Colors.Red), 1.5);

            this.IsHitTestVisible = false;
            drawingContext.DrawRectangle(renderBrush, renderPen, adornedElementRect);
        }
    }

    class BeginDraggedAdorner : Adorner
    {
        public BeginDraggedAdorner(UIElement adornedElement) : base(adornedElement)
        {
        }

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var adornedElementRect = new Rect(this.AdornedElement.DesiredSize);
            var renderBrush = new media.SolidColorBrush(media.Colors.LightGray);
            renderBrush.Opacity = 0.5;
            var renderPen = new media.Pen(new media.SolidColorBrush(media.Colors.LightGray), 1.5);

            this.IsHitTestVisible = false;
            drawingContext.DrawRectangle(renderBrush, renderPen, adornedElementRect);
        }
    }


}
