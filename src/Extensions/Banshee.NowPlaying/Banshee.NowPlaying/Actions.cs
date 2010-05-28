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
        private NowPlayingSource now_playing_source;

        public Actions (NowPlayingSource nowPlayingSource) : base ("NowPlaying")
        {
            now_playing_source = nowPlayingSource;

            Add (new RadioActionEntry [] {
                new RadioActionEntry ("StandardNpOpen", null, null, null, "Now Playing", 0),
                new RadioActionEntry ("LastFmOpen", null, null, null, "Last.fm recommendations", 1),
                new RadioActionEntry ("WikipediaOpen", null, null, null, "Wikipedia", 2)
            }, 0, OnChanged);

            this["StandardNpOpen"].IconName = "applications-multimedia";
            this["LastFmOpen"].IconName = "lastfm-audioscrobbler";
            this["WikipediaOpen"].IconName = "wikipedia";

            Register ();

            ContextPane = new ContextPane.ContextPane ();
        }

        public void OnChanged (System.Object o, ChangedArgs args)
        {
            Log.DebugFormat ("There are {0} actions. {1} is current", this.ListActions().Count (), args.Current.CurrentValue);

            if (args.Current.CurrentValue == 0) {
                now_playing_source.SetSubstituteAudioDisplay (null);
            } else {
                now_playing_source.SetSubstituteAudioDisplay (ContextPane);
            }
        }

        private ContextPane.ContextPane ContextPane { get; set; }
    }
}
