//
// ListViewGraphics.cs
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
using Gtk;
using Gdk;
using Cairo;

namespace Banshee.Data.Gui
{
    public enum GtkColorClass 
    {
        Light,
        Mid,
        Dark,
        Base,
        Background,
        Foreground
    }
    
    public class ListViewGraphics
    {
        private Cairo.Color [] gtk_colors;
        
        private Cairo.Color selection_fill;
        private Cairo.Color selection_stroke;
        
        private Cairo.Color view_fill;
        private Cairo.Color view_fill_transparent;
        
        private Widget widget;
        
        public ListViewGraphics(Widget widget)
        {
            this.widget = widget;
            widget.StyleSet += delegate { RefreshColors(); };
        }
        
        public Cairo.Color GetWidgetColor(GtkColorClass @class, StateType state)
        {
            if(gtk_colors == null) {
                RefreshColors();
            }
            
            return gtk_colors[(int)@class * (int)GtkColorClass.Foreground + (int)state];
        }
        
        private bool refreshing = false;
        
        public void RefreshColors()
        {
            if(refreshing) {
                return;
            }
            
            refreshing = true;
            
            int mc = (int)GtkColorClass.Foreground;
            int ms = (int)StateType.Insensitive;
            
            if(gtk_colors == null) {
                gtk_colors = new Cairo.Color[(mc + 1) * (ms + 1)];
            }
                
            for(int c = (int)GtkColorClass.Light; c <= mc; c++) {
                for(int s = (int)StateType.Normal; s <= ms; s++) {
                    Gdk.Color color = Gdk.Color.Zero;
                    
                    if(widget != null && widget.IsRealized) {
                        switch((GtkColorClass)c) {
                            case GtkColorClass.Light:      color = widget.Style.LightColors[s]; break;
                            case GtkColorClass.Mid:        color = widget.Style.MidColors[s];   break;
                            case GtkColorClass.Dark:       color = widget.Style.DarkColors[s];  break;
                            case GtkColorClass.Base:       color = widget.Style.BaseColors[s];  break;
                            case GtkColorClass.Background: color = widget.Style.Backgrounds[s]; break;
                            case GtkColorClass.Foreground: color = widget.Style.Foregrounds[s]; break;
                        }
                    } else {
                        color = new Gdk.Color(0, 0, 0);
                    }
                    
                    gtk_colors[c * mc + s] = CairoExtensions.GdkColorToCairoColor(color);
                }
            }
            
            selection_fill = GetWidgetColor(GtkColorClass.Dark, StateType.Active);
            selection_stroke = GetWidgetColor(GtkColorClass.Background, StateType.Selected);
            
            view_fill = GetWidgetColor(GtkColorClass.Base, StateType.Normal);
            view_fill_transparent = view_fill;
            view_fill_transparent.A = 0;
            
            refreshing = false;
        }
        
        public Cairo.Color ViewFill {
            get { return view_fill; }
        }
        
        public Cairo.Color ViewFillTransparent {
            get { return view_fill_transparent; }
        }
        
        public Cairo.Color SelectionFill {
            get { return selection_fill; }
        }
        
        public Cairo.Color SelectionStroke {
            get { return selection_stroke; }
        }
    }
}
