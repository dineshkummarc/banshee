//
// ContextPageManager.cs
//
// Authors:
//   Gabriel Burt <gburt@novell.com>
//
// Copyright (C) 2009 Novell, Inc.
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
using System.Collections.Generic;
using Mono.Addins;
using Gtk;
using Banshee.Gui;

namespace Banshee.ContextPane
{
    public class ContextPageManager
    {
        private Dictionary<string, BaseContextPage> pages = new Dictionary<string, BaseContextPage> ();

        public event Action<BaseContextPage> PageAdded;
        public event Action<BaseContextPage> PageRemoved;

        public ContextPageManager ()
        {
        }

        public void Init ()
        {
            Mono.Addins.AddinManager.AddExtensionNodeHandler ("/Banshee/ThickClient/ContextPane", OnExtensionChanged);
        }

        public IEnumerable<BaseContextPage> Pages {
            get {
                return pages.Values;
            }
        }

        private void OnExtensionChanged (object o, ExtensionNodeEventArgs args)
        {
            TypeExtensionNode node = (TypeExtensionNode) args.ExtensionNode;

            if (args.Change == ExtensionChange.Add) {
                var page = (BaseContextPage) node.CreateInstance ();
                pages.Add (node.Id, page);
                var handler = PageAdded;
                if (handler != null) {
                    handler (page);
                }
            } else {
                if (!pages.ContainsKey (node.Id)) {
                    return;
                }

                var page = pages[node.Id];
                var handler = PageRemoved;

                pages.Remove (node.Id);

                if (handler != null) {
                    PageRemoved (page);
                }

                page.Dispose ();
            }
        }
    }
}
