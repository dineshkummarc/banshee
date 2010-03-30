//
// ChannelSource.cs
//
// Author:
//   Mike Urbanski <michael.c.urbanski@gmail.com>
//
// Copyright (c) 2009 Michael C. Urbanski
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
using System.Threading;

using System.Linq;
using System.Collections.Generic;

using Gtk;

using Mono.Unix;

using Hyena;

using Banshee.Gui;
using Banshee.Base;
using Banshee.Sources;
using Banshee.Sources.Gui;
using Banshee.ServiceStack;

using Banshee.Paas.Aether;
using Banshee.Paas.Aether.MiroGuide;

using Banshee.Paas.MiroGuide.Gui;

namespace Banshee.Paas.MiroGuide
{
    public abstract class ChannelSource : Source, IDisposable
    {
        private bool ignore_scroll;
        private MiroGuideClient client;
        private MiroGuideActions actions;

        private SourceMessage status_message;
        private SourceMessage error_status_message;

        private ChannelSourceContents contents;
        private MiroGuideChannelListModel channel_model;

        private MiroGuideSortType active_sort_type;
        private readonly MiroGuideFilterType filter_type;

        protected MiroGuideSortType ActiveSortType {
            get { return active_sort_type; }
            set { active_sort_type = value; }
        }

        protected MiroGuideActions Actions {
            get { return actions; }
        }

        protected MiroGuideClient Client {
            get { return client; }
        }

        protected ChannelSourceContents Contents {
            get { return contents; }
        }

        protected string BusyStatusMessage { get; set; }
        protected SearchContext Context { get; set; }

        public MiroGuideChannelListModel ChannelModel {
            get { return channel_model; }
        }

        private static ManualResetEvent client_handle = new ManualResetEvent (true);
        protected static ManualResetEvent ClientHandle {
            get { return client_handle; }
        }

        public ChannelSource (MiroGuideClient client,
                              MiroGuideFilterType filterType,
                              string genericName, string name, int order) : base (genericName, name, order)
        {
            if (client == null) {
                throw new ArgumentNullException ("client");
            }

            this.client = client;
            this.filter_type = filterType;

            active_sort_type = MiroGuideSortType.Name;
            actions = ServiceManager.Get <InterfaceActionService> ().FindActionGroup ("MiroGuide") as MiroGuideActions;

            BusyStatusMessage = Catalog.GetString ("Updating");

            channel_model = new MiroGuideChannelListModel ();

            channel_model.Cleared  += (sender, e) => { OnUpdated (); };
            channel_model.Reloaded += (sender, e) => { QueueDraw (); OnUpdated (); };

            this.client.StateChanged += OnMiroGuideClientStateChanged;

            TypeUniqueId = String.Concat (genericName, "ChannelSource");

            Properties.SetString ("ActiveSourceUIResource", "MiroGuideActiveSourceUI.xml");
            Properties.Set<bool> ("ActiveSourceUIResourcePropagate", false);
            Properties.Set<System.Reflection.Assembly> ("ActiveSourceUIResource.Assembly", typeof(ChannelSource).Assembly);

            Properties.SetString ("GtkActionPath", "/MiroGuideChannelSourcePopup");

            contents = CreateChannelSourceContents ();
            contents.Initialize ();

            (contents.ScrolledWindow.VScrollbar as VScrollbar).ValueChanged += OnVScrollbarValueChangedHandler;

            Properties.Set<ISourceContents> ("Nereid.SourceContents", contents);
            Properties.Set<bool> ("Nereid.SourceContentsPropagate", false);
        }

        public override void Activate ()
        {
            ClientHandle.WaitOne ();

            if (active_sort_type != MiroGuideSortType.None) {
                actions.SetActiveSortPreference (active_sort_type);
            }

            client.GetChannelsCompleted += OnGetChannelsCompletedHandler;
            actions.SortPreferenceChanged += OnSortPreferenceChangedHandler;
        }

        public override void Deactivate ()
        {
            client.GetChannelsCompleted -= OnGetChannelsCompletedHandler;
            actions.SortPreferenceChanged -= OnSortPreferenceChangedHandler;
            client.CancelAsync ();
        }

        public virtual void Dispose ()
        {
        }

        public virtual void QueueDraw ()
        {
            ThreadAssist.ProxyToMain (delegate {
                contents.Widget.QueueDraw ();
            });
        }

        public virtual void Refresh ()
        {
            ThreadAssist.ProxyToMain (delegate {
               if (Context != null) {
                    Context.Reset ();
                    GetChannelsAsync (Context);
               } else {
                    GetChannelsAsync ();
               }
            });
        }

        protected virtual void GetChannelsAsync ()
        {
        }

        protected virtual void GetChannelsAsync (string filterValue)
        {
            Client.GetChannelsAsync (
                filter_type, filterValue, active_sort_type,
                !(active_sort_type == MiroGuideSortType.Name), 50, 0, this
            );
        }

        protected virtual void GetChannelsAsync (SearchContext context)
        {
            if (context != null) {
                Client.GetChannelsAsync (context, this);
            }
        }

        protected virtual void CheckVScrollbarValue (VScrollbar vsb)
        {
            if (!ignore_scroll && (vsb.Value == vsb.Adjustment.Upper-vsb.Adjustment.PageSize ||
                vsb.Adjustment.Upper-vsb.Adjustment.PageSize < 0)) {
                if (Context != null && Context.ChannelsAvailable) {
                    GetChannelsAsync (Context);
                }
            }
        }

        protected virtual void ClearModel ()
        {
            ChannelModel.Selection.Clear ();
            ChannelModel.Clear ();
        }

        protected virtual ChannelSourceContents CreateChannelSourceContents ()
        {
            return new ChannelSourceContents ();
        }

        protected virtual void RefreshArtworkFor (MiroGuideChannelInfo channel)
        {
            if (!CoverArtSpec.CoverExists (PaasService.ArtworkIdFor (channel.Name))) {
                Banshee.Kernel.Scheduler.Schedule (
                    new MiroGuideImageFetchJob (channel), Banshee.Kernel.JobPriority.AboveNormal
                );
            }
        }

        protected SourceMessage CreateStatusMessage (string messageStr, string iconName)
        {
            return CreateStatusMessage (messageStr, iconName, null, null);
        }

        protected SourceMessage CreateStatusMessage (string messageStr, string iconName, string actionStr, EventHandler action)
        {
            SourceMessage message = new SourceMessage (this) {
                Text = messageStr
            };

            message.SetIconName (iconName);

            if (action != null) {
                message.AddAction (new MessageAction (actionStr, false, action));
            }

            return message;
        }

        protected void SetStatus (string messageStr)
        {
            SetStatus (CreateStatusMessage (messageStr, Stock.Info));
        }

        protected void SetStatus (SourceMessage message)
        {
            ThreadAssist.AssertInMainThread ();
            status_message = message;
        }

        protected void SetErrorStatus ()
        {
            SetErrorStatus (Catalog.GetString ("An error occurred while communicating with Miro Guide."));
        }

        protected void SetErrorStatus (string message_str)
        {
            SetErrorStatus (CreateStatusMessage (
                message_str, Stock.DialogError, Catalog.GetString ("Retry"), delegate { Refresh (); }
            ));
        }

        protected void SetErrorStatus (SourceMessage message)
        {
            ThreadAssist.AssertInMainThread ();
            error_status_message = message;
        }

        // Terrible name.
        protected void SetFetchStatus ()
        {
            ThreadAssist.AssertInMainThread ();

            SetStatus (CreateStatusMessage (
                Catalog.GetString ("Fetch additional channels?"), Stock.DialogQuestion,
                Catalog.GetString ("Fetch"), delegate { GetChannelsAsync (Context); }
            ));
        }

        protected virtual void SetRequestStatus (string message)
        {
            SetRequestStatus (message, null);
        }

        protected virtual void SetRequestStatus (string message, string iconName)
        {
            ThreadAssist.AssertInMainThread ();

            SourceMessage m = new SourceMessage (this) {
                Text = message,
                CanClose = true,
                IsSpinning = true
            };

            m.Updated += (sender, e) => {
                if (m.IsHidden) {
                    Client.CancelAsync ();
                }
            };

            PushMessage (m);
        }

        protected virtual void OnMiroGuideClientStateChanged (object sender, AetherClientStateChangedEventArgs e)
        {
            if (e.NewState == AetherClientState.Busy) {
                ClientHandle.Reset ();
                ThreadAssist.ProxyToMain (delegate {
                    ClearMessages ();
                    status_message = null;
                    error_status_message = null;
                    actions["MiroGuideRefreshChannelsAction"].Sensitive = false;
                    ChannelSourceContents.SortPreferenceButtonSensitive = false;
                    SetRequestStatus (String.Format ("{0}...", BusyStatusMessage));
                });
            } else {
                ClientHandle.Set ();
                ThreadAssist.ProxyToMain (delegate {
                    actions["MiroGuideRefreshChannelsAction"].Sensitive = true;
                    ChannelSourceContents.SortPreferenceButtonSensitive = true;
                    PopMessage ();

                    if (status_message != null) {
                        PushMessage (status_message);
                    } else if (error_status_message != null) {
                        PushMessage (error_status_message);
                    }
                });
            }
        }

        protected virtual void OnVScrollbarValueChangedHandler (object sender, EventArgs e)
        {
            CheckVScrollbarValue (sender as VScrollbar);
        }

        protected virtual void OnGetChannelsCompletedHandler (object sender, GetChannelsCompletedEventArgs e)
        {
            if (e.UserState != this) {
                return;
            }

            ThreadAssist.ProxyToMain (delegate {
                if (e.Cancelled) {
                    if (Context != null && Context.ChannelsAvailable) {
                        SetFetchStatus ();
                    }
                    return;
                } else if (e.Error != null) {
                    SetErrorStatus ();
                    return;
                }

                Context = e.Context;

                ignore_scroll = true;
                bool check_sb_pos = false;

                foreach (MiroGuideChannelInfo channel in e.Channels.Reverse ()) {
                    RefreshArtworkFor (channel);
                }

                if (Context.Page == 0) {
                    ClearModel ();
                }

                if (Context.Count > 0) {
                    ChannelModel.Add (e.Channels);

                    if (Context.ChannelsAvailable) {
                        Contents.ScrolledWindow.VscrollbarPolicy = PolicyType.Always;
                        check_sb_pos = true;
                    }
                } else {
                    if (Context.Page == 0) {
                        SetStatus (Catalog.GetString ("No matches found."));
                    }

                    Contents.ScrolledWindow.VscrollbarPolicy = PolicyType.Automatic;
                    ChannelModel.Reload ();
                }

                ignore_scroll = false;

                if (check_sb_pos) {
                    SetFetchStatus ();
                    CheckVScrollbarValue (Contents.ScrolledWindow.VScrollbar as VScrollbar);
                }
            });
        }

        protected virtual void OnSortPreferenceChangedHandler (object sender, SortPreferenceChangedEventArgs e)
        {
            if (e.ActiveSource != this) {
                return;
            }

            ThreadAssist.ProxyToMain (delegate {
                if (Context != null && Context.SortType != e.Sort) {
                    Context.Reset ();
                    Context.SortType = e.Sort;
                    active_sort_type = e.Sort;

                    Context.Reverse = !(active_sort_type == MiroGuideSortType.Name);

                    ClearModel ();
                    GetChannelsAsync (Context);
                } else {
                    GetChannelsAsync ();
                }
            });
        }
    }
}
