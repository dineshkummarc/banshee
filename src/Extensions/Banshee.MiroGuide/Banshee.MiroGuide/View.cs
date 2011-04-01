//
// View.cs
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
using System.Web;
using Mono.Unix;

using Gtk;
using WebKit;

using Hyena;
using Hyena.Json;
using Hyena.Downloader;

using Banshee.Base;
using Banshee.IO;
using Banshee.ServiceStack;
using Banshee.WebBrowser;
using Banshee.WebSource;

namespace Banshee.MiroGuide
{
    public class View : Banshee.WebSource.WebView
    {
        public View ()
        {
            CanSearch = true;
            FixupJavascriptUrl = "http://integrated-services.banshee.fm/miro/guide-fixups.js";
            FullReload ();
        }

        public void UpdateSearchText ()
        {
            Shell.SearchEntry.EmptyMessage = LastPageWasAudio
                ? Catalog.GetString ("Search for podcasts")
                : Catalog.GetString ("Search for video podcasts");
        }

        protected override void OnLoadStatusChanged ()
        {
            if (LoadStatus == LoadStatus.Finished && Uri != null && Uri.StartsWith ("http://miroguide.com")) {
                LastPageWasAudio = Uri.Contains ("miroguide.com/audio/");
                UpdateSearchText ();
            }

            base.OnLoadStatusChanged ();
        }

        protected override bool OnMimeTypePolicyDecisionRequested (WebKit.WebFrame frame, WebKit.NetworkRequest request, string mimetype, WebKit.WebPolicyDecision policy_decision)
        {
            switch (mimetype) {
                case "application/x-miro":
                    policy_decision.Download ();
                    return true;
                default:
                    return base.OnMimeTypePolicyDecisionRequested (frame, request, mimetype, policy_decision);
            }
        }

        protected override string OnDownloadRequested (Download download, string mimetype, string uri, string suggestedFilename)
        {
            switch (mimetype) {
                case "application/x-miro":
                    var dest_uri_base = "file://" + Paths.Combine (Paths.TempDir, suggestedFilename);
                    var dest_uri = new SafeUri (dest_uri_base);
                    for (int i = 1; File.Exists (dest_uri);
                        dest_uri = new SafeUri (String.Format ("{0} ({1})", dest_uri_base, ++i)));
                    download.StatusChanged += OnDownloadStatusChanged;
                    return dest_uri.AbsoluteUri;
            }

            return null;
        }

        protected override bool OnNavigationPolicyDecisionRequested (WebKit.WebFrame frame, WebKit.NetworkRequest request, WebKit.WebNavigationAction navigation_action, WebKit.WebPolicyDecision policy_decision)
        {
            try {
                if (TryBypassRedirect (request.Uri) || TryInterceptListenWatch (request.Uri)) {
                    policy_decision.Ignore ();
                    return true;
                }
            } catch (Exception e) {
                Log.Exception ("MiroGuide caught error trying to shortcut navigation", e);
            }

            return false;
        }

        private void OnDownloadStatusChanged (object o, EventArgs args)
        {
            var download = (Download) o;

            // FIXME: handle the error case
            if (download.Status != DownloadStatus.Finished) {
                return;
            }

            switch (download.GetContentType ()) {
                case "application/x-miro":
                    Log.Debug ("MiroGuide: downloaded Miro subscription file", download.DestinationUri);
                    ServiceManager.Get<DBusCommandService> ().PushFile (download.DestinationUri);
                    break;
            }
        }

        public override void GoHome ()
        {
            LoadUri (GetActionUrl (LastPageWasAudio, "home/"));
        }

        public override void GoSearch (string query)
        {
            query = System.Uri.EscapeDataString (query);
            LoadUri (new Uri (GetActionUrl (LastPageWasAudio, "search/") + query).AbsoluteUri);
        }

        private string GetActionUrl (bool audio, string action)
        {
            return "http://integrated-services.banshee.fm/miro/" + (audio ? "audio/" : "video/") + action;
        }

        // The download and add-to-sidebar buttons take the user to a page that then redirects to the
        // actual media URL or .miro OPML file.  But the URL to that redirection page contains all the
        // info we need to start downloading or subscribe immediately.
        private bool TryBypassRedirect (string uri)
        {
            if (uri != null && uri.StartsWith ("http://subscribe.getmiro.com/") && uri.Contains ("url1")) {
                var direct_uri = HttpUtility.UrlDecode (uri.SubstringBetween ("url1=", "&"));
                var title = HttpUtility.UrlDecode (uri.SubstringBetween ("title1=", "&"));
                if (uri.Contains ("/download")) {
                    // FIXME download and import
                    //Banshee.Streaming.RadioTrackInfo.OpenPlay (direct_uri);
                    //Banshee.ServiceStack.ServiceManager.PlaybackController.StopWhenFinished = true;
                    Log.DebugFormat ("MiroGuide: downloading {0} ({1})", title, direct_uri);
                    Log.Information ("Downloading not yet implemented.  URL", direct_uri, true);
                } else {
                    // Subscribe to it straight away, don't redirect at all
                    ServiceManager.Get<DBusCommandService> ().PushFile (direct_uri);
                    Log.DebugFormat ("MiroGuide: subscribing straight away to {0} ({1})", title, direct_uri);
                }
                return true;
            }

            return false;
        }

        // The listen/watch links take the user to another page with an embedded player.  Instead of
        // going there, find the direct media URL and send it to Banshee's PlayerEngine.
        private bool TryInterceptListenWatch (string uri)
        {
            bool ret = false;
            if (uri != null && uri.Contains ("miroguide.com/items/")) {
                int i = uri.LastIndexOf ('/') + 1;
                var item_id = uri.Substring (i, uri.Length - i);

                // Get the actual media URL via the MiroGuide API
                new Hyena.Downloader.HttpStringDownloader () {
                    Uri = new Uri (String.Format ("http://www.miroguide.com/api/get_item?datatype=json&id={0}", item_id)),
                    Finished = (d) => {
                        if (d.State.Success) {
                            string media_url = null;
                            var item = new Deserializer (d.Content).Deserialize () as JsonObject;
                            if (item.ContainsKey ("url")) {
                                media_url = item["url"] as string;
                            }

                            if (media_url != null) {
                                Log.DebugFormat ("MiroGuide: streaming straight away {0}", media_url);
                                Banshee.Streaming.RadioTrackInfo.OpenPlay (media_url);
                                Banshee.ServiceStack.ServiceManager.PlaybackController.StopWhenFinished = true;
                                ret = true;
                            }
                        }
                    },
                    AcceptContentTypes = new [] { "text/javascript" }
                }.StartSync ();
            }

            return ret;
        }

        internal Banshee.WebSource.WebBrowserShell Shell { get; set; }

        private bool LastPageWasAudio {
            get { return last_was_audio.Get (); }
            set { last_was_audio.Set (value); }
        }

        private static Banshee.Configuration.SchemaEntry<bool> last_was_audio = new Banshee.Configuration.SchemaEntry<bool> (
            "plugins.miroguide", "last_was_audio", true, "", ""
        );
    }
}
