//
// WebView.cs
//
// Authors:
//   Aaron Bockover <abockover@novell.com>
//   Gabriel Burt <gburt@novell.com>
//
// Copyright 2010 Novell, Inc.
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

using Gtk;
using WebKit;

using Hyena;
using Hyena.Downloader;

using Banshee.Base;
using Banshee.IO;
using Banshee.WebBrowser;

namespace Banshee.WebSource
{
    public abstract class WebView : WebKit.WebView
    {
        protected string FixupJavascriptUrl { get; set; }
        private string fixup_javascript;
        private bool fixup_javascript_fetched;

        public bool IsReady { get; private set; }
        public bool CanSearch { get; protected set; }

        const float ZOOM_STEP = 0.05f;

        public event EventHandler Ready;
        public event Action<float> ZoomChanged;

        public WebView ()
        {
            Settings.EnablePlugins = false;
            Settings.EnablePageCache = true;
            Settings.EnableDefaultContextMenu = false;
            Settings.JavascriptCanOpenWindowsAutomatically = false;
            //Settings.UserAgent = ..

            FullContentZoom = true;

            CanSearch = false;

            LoadStatusChanged += HandleLoadStatusChanged;

            //CreateWebView += (o, a) => { Console.WriteLine ("{0} CreateWebView", this); a.Frame = MainFrame; };
            ResourceRequestStarting += (o, a) => Console.WriteLine ("{0} ResourceRequestStarting uri={1}", this, a.Request.Uri);
            LoadError += (o, a) => Console.WriteLine ("{0} LoadError uri={1}", this, a.Uri);
            ScriptAlert += (o, a) => Console.WriteLine ("{0} ScriptAlert msg={1}", this, a.Message);
            PopulatePopup += (o, a) => Console.WriteLine ("{0} PopulatePopup", this);
            ConsoleMessage += (o, a) => Console.WriteLine ("{0} ConsoleMessage msg={1}", this, a.Message);
        }

        protected override WebKit.WebView OnCreateWebView (WebKit.WebFrame frame)
        {
            Console.WriteLine ("{0} CreateWebView", this);
            return this;
        }

        public new float ZoomLevel {
            get { return base.ZoomLevel; }
            set {
                if (value != ZoomLevel) {
                    ZoomLevel = value;
                    var handler = ZoomChanged;
                    if (handler != null) {
                        handler (value);
                    }
                }
            }
        }

        protected override bool OnScrollEvent (Gdk.EventScroll scroll)
        {
            if ((scroll.State & Gdk.ModifierType.ControlMask) != 0) {
                ZoomLevel += (scroll.Direction == Gdk.ScrollDirection.Up) ? ZOOM_STEP : -ZOOM_STEP;
                return true;
            }

            return base.OnScrollEvent (scroll);
        }

        protected override bool OnDownloadRequested (Download download)
        {
            var dest_uri = OnDownloadRequested (
                download,
                download.GetContentType (),
                download.Uri,
                download.SuggestedFilename
            );

            if (dest_uri != null) {
                download.DestinationUri = dest_uri;
                return true;
            }

            return false;
        }

        protected virtual string OnDownloadRequested (Download download, string mimetype, string uri, string suggestedFilename)
        {
            return null;
        }

        private void HandleLoadStatusChanged (object o, EventArgs args)
        {
            OnLoadStatusChanged ();
        }

        protected virtual void OnLoadStatusChanged ()
        {
            Console.WriteLine ("{0} LoadStatusChanged to {1}", this, LoadStatus);
            if ((LoadStatus == LoadStatus.FirstVisuallyNonEmptyLayout ||
                LoadStatus == LoadStatus.Finished) && Uri != "about:blank") {
                if (fixup_javascript != null) {
                    ExecuteScript (fixup_javascript);
                }
            }
        }

        protected override bool OnMimeTypePolicyDecisionRequested (WebKit.WebFrame frame, WebKit.NetworkRequest request, string mimetype, WebKit.WebPolicyDecision policy_decision)
        {
            // We only explicitly accept (render) text/html -- everything else is ignored.
            switch (mimetype) {
                case "text/html":
                    policy_decision.Use ();
                    return true;
                default:
                    Log.Debug ("OssiferWebView: ignoring mime type", mimetype);
                    policy_decision.Ignore ();
                    return true;
            }
        }

        public abstract void GoHome ();

        public virtual void GoSearch (string query)
        {
        }

        public void FullReload ()
        {
            // This is an HTML5 Canvas/JS spinner icon. It is awesome
            // and renders immediately, going away when the store loads.
            LoadString (AssemblyResource.GetFileContents ("loading.html"),
                "text/html", "UTF-8", null);

            // Here we download and save for later injection some JavaScript
            // to fix-up the Amazon pages. We don't store this locally since
            // it may need to be updated if Amazon's page structure changes.
            // We're mainly concerned about hiding the "You don't have Flash"
            // messages, since we do the streaming of previews natively.
            if (FixupJavascriptUrl != null && !fixup_javascript_fetched) {
                fixup_javascript_fetched = true;
                new Hyena.Downloader.HttpStringDownloader () {
                    Uri = new Uri (FixupJavascriptUrl),
                    Finished = (d) => {
                        if (d.State.Success) {
                            fixup_javascript = d.Content;
                        }
                        LoadHome ();
                    },
                    AcceptContentTypes = new [] { "text/javascript" }
                }.Start ();
            } else {
                LoadHome ();
            }
        }

        private void LoadHome ()
        {
            // We defer this to another main loop iteration, otherwise
            // our load placeholder document will never be rendered.
            GLib.Idle.Add (delegate {
                GoHome ();

                // Emit the Ready event once we are first allowed
                // to load the home page (ensures we've downloaded
                // the fixup javascript, etc.).
                if (!IsReady) {
                    IsReady = true;
                    var handler = Ready;
                    if (handler != null) {
                        handler (this, EventArgs.Empty);
                    }
                }
                return false;
            });
        }
    }

    public static class WebKitExtensions
    {
        public static string GetContentType (this Download download)
        {
            return download.NetworkResponse.Message.ResponseHeaders.GetContentType (IntPtr.Zero);
        }
    }
}
