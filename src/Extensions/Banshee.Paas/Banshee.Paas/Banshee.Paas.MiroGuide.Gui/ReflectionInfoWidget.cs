using System;
using System.Collections.Generic;
using System.Linq;

using Cairo;
using Gdk;
using Gtk;

namespace Banshee.Paas.MiroGuide.Gui
{
    public abstract class AbstractInfoRenderer
    {
        int maxWidth;
        Style style;

        public int MaxWidth {
            get { return maxWidth; }
            set {
                maxWidth = value;
                OnMaxWidthSet (maxWidth);
            }
        }

        public Style Style {
            get { return Style; }
            set {
                style = value;
                OnStyleSet (style);
            }
        }

        public virtual int HeightRequest {
            get { return 0; }
        }

        public virtual int WidthRequest {
            get { return MaxWidth; }
        }

        protected virtual void OnMaxWidthSet (int width)
        {
            // do shit here
        }

        protected virtual void OnStyleSet (Style style)
        {
            // do shit here
        }

        public virtual void RenderOntoContext (Context cr, int x, int y)
        {
        }

    }

    public class TitleInfoRenderer : AbstractInfoRenderer
    {
        string text;

        public override int HeightRequest {
            get { return 32; }
        }

        public TitleInfoRenderer (string text)
        {
            this.text = text;
        }

        public override void RenderOntoContext (Context cr, int x, int y)
        {
            Pango.Layout layout = Pango.CairoHelper.CreateLayout (cr);
            layout.FontDescription = Style.FontDescription;
            layout.FontDescription.AbsoluteSize = 30;
            layout.FontDescription.Weight = Pango.Weight.Bold;

            layout.Ellipsize = Pango.EllipsizeMode.End;
            layout.SetText (text);
            layout.Width = Pango.Units.ToPixels (MaxWidth);
            cr.MoveTo (x, y);
            Pango.CairoHelper.LayoutPath (cr, layout);
            cr.Color = new Cairo.Color (1, 1, 1);
            cr.Fill ();
        }
    }

    public class ReflectionInfoWidget : DrawingArea
    {
        const double TopBorder = .2;
        const double ImageSize = .8;
        const double LeftRightSeparation = .5;
        const int Separation = 10;

        public double YAlign { get; set; }
        public double XAlign { get; set; }

        public ImageSurface ReflectionImage { get; set; }

        IEnumerable<AbstractInfoRenderer> renderers;
        public IEnumerable<AbstractInfoRenderer> Renderers {
            get { return renderers; }
            set {
                renderers = value.ToArray (); // prevent it from changing due to lazy eval
                if (Style == null)
                    return;
                foreach (AbstractInfoRenderer renderer in renderers) {
                    renderer.Style = Style;
                }
            }
        }

        public ReflectionInfoWidget ()
        {
            renderers = new List<AbstractInfoRenderer> ();

            AppPaintable = true;
            DoubleBuffered = true;
        }

        protected override void OnStyleSet (Gtk.Style previous_style)
        {
            foreach (AbstractInfoRenderer renderer in Renderers) {
                renderer.Style = Style;
            }
            base.OnStyleSet (previous_style);
        }

        protected override bool OnExposeEvent (Gdk.EventExpose evnt)
        {
            if (!IsRealized || ReflectionImage == null)
                return base.OnExposeEvent (evnt);

            using (Cairo.Context cr = Gdk.CairoHelper.Create (evnt.Window)) {
                cr.Operator = Operator.Source;
                cr.Color = new Cairo.Color (0, 0, 0);
                cr.Paint ();
                cr.Operator = Operator.Over;

                int midline = Allocation.Width / 2;
                int topline = (int) (Allocation.Height * TopBorder);
                int leftWidth = midline - 2 * Separation;

                cr.SetSource (ReflectionImage, midline + Separation, topline);
                cr.Paint ();

                foreach (AbstractInfoRenderer renderer in Renderers) {
                    renderer.MaxWidth = leftWidth;
                }

                int totalHeight = Renderers.Sum (r => r.HeightRequest);

                int y = (int) (topline + (ReflectionImage.Height - totalHeight) * YAlign);

                foreach (AbstractInfoRenderer renderer in Renderers) {
                    int x = (int) (Separation + (leftWidth - renderer.WidthRequest) * XAlign);
                    renderer.RenderOntoContext (cr, x, y);
                    y += renderer.HeightRequest;
                }
            }

            return true;
        }

    }
}