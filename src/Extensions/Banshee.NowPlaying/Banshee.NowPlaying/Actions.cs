// 
// Actions.cs
// 
// Author:
//   Alex Launi <alex.launi@gmail.com>
// 
// Copyright (c) 2010 Alex Launi
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
using System.Collections.Generic;
using System.Linq;

using Mono.Unix;
using Gtk;

using Hyena;
using Hyena.Widgets;

using Banshee.ServiceStack;
using Banshee.Collection.Database;
using Banshee.ContextPane;
using Banshee.Gui;

namespace Banshee.NowPlaying
{
    public class Actions : BansheeActionGroup
    {
        public const string TrackInfoId = "StandardNpOpen";

        private NowPlayingSource now_playing_source;
        private Dictionary<int, BaseContextPage> pages;

        public Actions (NowPlayingSource nowPlayingSource) : base ("NowPlaying")
        {
            now_playing_source = nowPlayingSource;
            pages = new Dictionary<int, BaseContextPage> ();

            Manager = new ContextPageManager ();
            Manager.Init ();
            Manager.PageAdded += OnContextPagesChanged;
            Manager.PageRemoved += OnContextPagesChanged;

            LoadActions ();

            Register ();
        }

        // We've got 1 hard coded action available and the rest come from the context pane.
        // so we loop through our pages and get their ids and return an IEnumerable with them all.
        public IEnumerable<string> PageIds {
            get {
                return new string [] { TrackInfoId }.Concat (pages.Values.Select (p => p.Id));
            }
        }

        private ContextPageManager Manager { get; set; }

        private void LoadActions ()
        {
            // remove all of the existing actions
            foreach (Gtk.Action action in ListActions ()) {
                Remove (action);
            }

            // then add them all.
            int i = 0;
            List<RadioActionEntry> actions = new List<RadioActionEntry> ();
            actions.Add (new RadioActionEntry (TrackInfoId, null, null, null, "Track Information", i));

            foreach (BaseContextPage page in Manager.Pages) {
                i++;
                actions.Add (new RadioActionEntry (page.Id, null, null, null, page.Name, i));
                pages.Add (i, page);
            }

            Add (actions.ToArray (), 0, OnChanged);

            this[TrackInfoId].IconName = "applications-multimedia";
            foreach (BaseContextPage page in Manager.Pages) {
                foreach (string icon in page.IconNames) {
                    if (IconThemeUtils.HasIcon (icon)) {
                        this[page.Id].IconName = icon;
                        break;
                    }
                }
            }
        }

        private void OnChanged (System.Object o, ChangedArgs args)
        {
            if (args.Current.CurrentValue == 0) {
                now_playing_source.SetSubstituteAudioDisplay (null);
            } else {
                Widget widget = pages[args.Current.CurrentValue].Widget;
                now_playing_source.SetSubstituteAudioDisplay (widget);
                widget.Show ();
            }
        }

        private void OnContextPagesChanged (BaseContextPage page)
        {
            LoadActions ();
        }
    }
}
