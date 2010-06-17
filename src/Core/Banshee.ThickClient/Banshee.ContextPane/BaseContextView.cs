// 
// BaseContextView.cs
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

using Gtk;
using Mono.Unix;

using Banshee.Collection;
using Banshee.Configuration;
using Banshee.MediaEngine;
using Banshee.ServiceStack;

using Hyena.Gui;
using Hyena.Widgets;

namespace Banshee.ContextPane
{
    public abstract class BaseContextView : HBox
    {
        protected Gtk.Notebook notebook;

        protected RoundedFrame loading;
        protected RoundedFrame no_active;
        protected BaseContextPage active_page;

        protected Dictionary<BaseContextPage, Widget> pane_pages;

        public BaseContextView ()
        {
            pane_pages = new Dictionary<BaseContextPage, Widget> ();
            CreateContextNotebook ();

            Manager = new ContextPageManager ();
            Manager.PageAdded += OnPageAdded;
            Manager.PageRemoved += OnPageRemoved;

            ServiceManager.PlayerEngine.ConnectEvent (OnPlayerEvent, PlayerEvent.StartOfStream | PlayerEvent.TrackInfoUpdated);
        }

        public ContextPageManager Manager { get; protected set; }

        public virtual void SetActivePage (BaseContextPage page)
        {
            if (page == null || page == active_page) {
                return;
            }

            if (active_page != null) {
                active_page.StateChanged -= OnActivePageStateChanged;
            }

            active_page = page;
            active_page.StateChanged += OnActivePageStateChanged;
            OnActivePageStateChanged (active_page.State);
            SetCurrentTrackForActivePage ();
        }

        protected abstract bool Enabled { get; set; }

        protected virtual void OnPageAdded (BaseContextPage page)
        {
            Hyena.Log.DebugFormat ("Adding context page {0}", page.Id);

            // TODO delay adding the page.Widget until the page is first activated,
            // that way we don't even create those objects unless used
            var frame = new Hyena.Widgets.RoundedFrame ();
            frame.Add (page.Widget);
            frame.Show ();

            page.Widget.Show ();
            notebook.AppendPage (frame, null);
            pane_pages[page] = frame;
        }

        protected virtual void OnPageRemoved (BaseContextPage page)
        {
            Hyena.Log.DebugFormat ("Removing context page {0}", page.Id);
            // Remove the notebook page
            notebook.RemovePage (notebook.PageNum (pane_pages[page]));
            pane_pages.Remove (page);
        }

        protected virtual void OnActivePageStateChanged (ContextState state)
        {
            if (active_page == null) {
                return;
            }

            if (state == ContextState.NotLoaded)
                notebook.CurrentPage = notebook.PageNum (no_active);
            else if (state == ContextState.Loading)
                notebook.CurrentPage = notebook.PageNum (loading);
            else if (state == ContextState.Loaded)
                notebook.CurrentPage = notebook.PageNum (pane_pages[active_page]);
        }

        protected virtual void OnPlayerEvent (PlayerEventArgs args)
        {
            if (Enabled) {
                SetCurrentTrackForActivePage ();
            }

        }

        protected void SetCurrentTrackForActivePage ()
        {
            TrackInfo track = ServiceManager.PlayerEngine.CurrentTrack;
            if (track != null && active_page != null) {
                active_page.SetTrack (track);
            }
        }

        private void CreateContextNotebook ()
        {
            notebook = new Notebook () {
                ShowBorder = false,
                ShowTabs = false
            };

            // 'No active track' and 'Loading' widgets
            no_active = new RoundedFrame ();
            no_active.Add (new Label () {
                Markup = String.Format ("<b>{0}</b>", Catalog.GetString ("Waiting for playback to begin..."))
            });
            no_active.ShowAll ();
            notebook.Add (no_active);

            loading = new RoundedFrame ();
            loading.Add (new Label () { Markup = String.Format ("<b>{0}</b>", Catalog.GetString ("Loading...")) });
            loading.ShowAll ();
            notebook.Add (loading);

            PackStart (notebook, true, true, 0);
            notebook.Show ();
        }

    }
}
