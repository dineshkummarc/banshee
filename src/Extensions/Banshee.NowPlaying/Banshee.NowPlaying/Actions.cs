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

        private Dictionary<int, BaseContextPage> pages;
        private NowPlayingSource now_playing_source;

        public Actions (NowPlayingSource nowPlayingSource) : base ("NowPlaying")
        {
            int i = 0;
            now_playing_source = nowPlayingSource;
            pages = new Dictionary<int, BaseContextPage> ();
            ContextPane = new ContextPane.ContextPane ();
            List<RadioActionEntry> actions = new List<RadioActionEntry> ();

            actions.Add (new RadioActionEntry (TrackInfoId, null, null, null, "Track Information", i));

            foreach (BaseContextPage page in ContextPane.Pages) {
                i++;
                actions.Add (new RadioActionEntry (page.Id, null, null, null, page.Name, i));
                pages.Add (i, page);
            }

            Add (actions.ToArray (), 0, OnChanged);

            this[TrackInfoId].IconName = "applications-multimedia";
            foreach (BaseContextPage page in ContextPane.Pages) {
                foreach (string icon in page.IconNames) {
                    if (IconThemeUtils.HasIcon (icon)) {
                        this[page.Id].IconName = icon;
                        break;
                    }
                }
            }

            Register ();
        }

        // We've got 1 hard coded action available and the rest come from the context pane.
        // so we loop through our pages and get their ids and return an IEnumerable with them all.
        public IEnumerable<string> PageIds {
            get {
                return new string [] { TrackInfoId }.Concat (pages.Values.Select (p => p.Id));
            }
        }

        public void OnChanged (System.Object o, ChangedArgs args)
        {
            Log.DebugFormat ("There are {0} actions. {1} is current", this.ListActions().Count (), args.Current.CurrentValue);

            if (args.Current.CurrentValue == 0) {
                now_playing_source.SetSubstituteAudioDisplay (null);
            } else {
                ContextPane.SetActivePage (pages[args.Current.CurrentValue]);
                now_playing_source.SetSubstituteAudioDisplay (ContextPane);
            }
        }

        private ContextPane.ContextPane ContextPane { get; set; }
    }
}
