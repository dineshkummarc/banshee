// 
// ThemeEngine.cs
//  
// Author:
//   Aaron Bockover <abockover@novell.com>
// 
// Copyright 2009 Aaron Bockover
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

namespace Hyena.Gui.Theming
{
    internal class SuperHackThemeEngineProviderArgs : EventArgs
    {
        private Gtk.Widget widget;
        public Gtk.Widget Widget {
            get { return widget; }
            set { widget = value; }
        }
    
        private Theme theme;
        public Theme Theme {
            set { theme = value; }
            get { return theme; }
        }
    }

    public static class ThemeEngine
    {   
        private static EventHandler provider;
        
        public static void SetProvider (EventHandler provider)
        {
            ThemeEngine.provider = provider;
        }
        
        public static Theme CreateTheme (Gtk.Widget widget)
        {
            if (provider == null) {
                return new GtkTheme (widget);
            }
            
            SuperHackThemeEngineProviderArgs args = new SuperHackThemeEngineProviderArgs ();
            args.Widget = widget;
            provider (null, args);
            return args.Theme ?? new GtkTheme (widget);
        }
    }
}
