//
// CairoExtensions.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2007 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using Gdk;
using Cairo;

namespace Banshee.Data.Gui
{
    [Flags]
    public enum CairoCorners
    {
        None = 0,
        TopLeft = 1,
        TopRight = 2,
        BottomLeft = 4,
        BottomRight = 8,
        All = 15
    }
    
    public static class CairoExtensions
    {        
        public static Cairo.Color GdkColorToCairoColor(Gdk.Color color)
        {
            return GdkColorToCairoColor(color, 1.0);
        }
        
        public static Cairo.Color GdkColorToCairoColor(Gdk.Color color, double alpha)
        {
            return new Cairo.Color(
                (double)(color.Red >> 8) / 255.0,
                (double)(color.Green >> 8) / 255.0,
                (double)(color.Blue >> 8) / 255.0,
                alpha);
        }
        
        public static void RoundedRectangle(Cairo.Context cr, double x, double y, double w, double h, double r)
        {
            RoundedRectangle(cr, x, y, w, h, r, CairoCorners.All);
        }
        
        public static void RoundedRectangle(Cairo.Context cr, double x, double y, double w, double h, 
            double r, CairoCorners corners)
        {
            if(r < 0.0001 || corners == CairoCorners.None) {
                cr.Rectangle(x, y, w, h);
                return;
            }
            
            if((corners & CairoCorners.TopLeft) != 0) {
                cr.MoveTo(x + r, y);
            } else {
                cr.MoveTo(x, y);
            }
            
            if((corners & CairoCorners.TopRight) != 0) {
                cr.Arc(x + w - r, y + r, r, Math.PI * 1.5, Math.PI * 2);
            } else {
                cr.LineTo(x + w, y);
            }
            
            if((corners & CairoCorners.BottomRight) != 0) {
                cr.Arc(x + w - r, y + h - r, r, 0, Math.PI * 0.5);
            } else {
                cr.LineTo(x + w, y + h);
            }
            
            if((corners & CairoCorners.BottomLeft) != 0) {
                cr.Arc(x + r, y + h - r, r, Math.PI * 0.5, Math.PI);
            } else {
                cr.LineTo(x, y + h);
            }
            
            if((corners & CairoCorners.TopLeft) != 0) {
                cr.Arc(x + r, y + r, r, Math.PI, Math.PI * 1.5);
            } else {
                cr.LineTo(x, y);
            }
        }
    }
}
