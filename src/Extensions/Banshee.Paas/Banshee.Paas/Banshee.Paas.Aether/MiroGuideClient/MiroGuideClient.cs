//
// MiroGuideClient.cs
//
// Author:
//       Mike Urbanski <michael.c.urbanski@gmail.com>
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

// This and AetherRequest are still a bit of an experiment.  This should be
// implemented as a more formal state machine.  This will not be used by users for sometime.

using System;
using System.IO;
using System.Net;
using System.Web;

using System.Linq;
using System.Text;

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using Hyena;
using Hyena.Json;

using Migo2.Async;

using Banshee.ServiceStack;

using Banshee.Base;
using Banshee.Paas.Data;
using Banshee.Paas.Aether;

namespace Banshee.Paas.Aether.MiroGuide
{
    public sealed class MiroGuideClient : AetherClient
    {
        private MiroGuideAccountInfo account;

        private string session_id;
        private Uri aether_service_uri;

        private AsyncStateManager asm;

        private AetherRequest request;

        public EventHandler<RequestCompletedEventArgs> Completed;

        public EventHandler<GetChannelsCompletedEventArgs> GetChannelsCompleted;
        public EventHandler<GetCategoriesCompletedEventArgs> GetCategoriesCompleted;

        public EventHandler<SubscriptionRequestedEventArgs> SubscriptionRequested;

        public MiroGuideAccountInfo Account {
            get { return Account; }
        }

        public ICredentials Credentials { get; set; }

        public string ServiceUri {
            get { return aether_service_uri.AbsoluteUri; }

            set {
                Uri tmp = new Uri (value.Trim ().TrimEnd ('/'));

                if (tmp.Scheme != "http" && tmp.Scheme != "https") {
                    throw new UriFormatException ("Scheme must be either http or https.");
                }

                aether_service_uri = tmp;
            }
        }

        public string SessionID {
            get {
                return session_id;
            }
        }

        public string UserAgent { get; set; }

        public MiroGuideClient (MiroGuideAccountInfo account)
        {
            if (account == null) {
                throw new ArgumentNullException ("account");
            }

            asm = new AsyncStateManager ();

            this.account = account;
            session_id = account.SessionID;

            this.account.Updated += (sender, e) => {
                lock (SyncRoot) {
                    if (!String.IsNullOrEmpty (account.ServiceUri)) {
                        ServiceUri = account.ServiceUri;
                    }

                    session_id = account.SessionID;
                }
            };

            if (!String.IsNullOrEmpty (account.ServiceUri)) {
                ServiceUri = account.ServiceUri;
            }
        }

        public void CancelAsync ()
        {
            lock (SyncRoot) {
                if (asm.Busy && asm.SetCancelled ()) {
                    if (request != null) {
                        request.CancelAsync ();
                    }
                }
            }
        }

        public void GetCategoriesAsync ()
        {
            GetCategoriesAsync (null);
        }

        public void GetCategoriesAsync (object userState)
        {
            lock (SyncRoot) {
                if (asm.Busy) {
                    return;
                }

                NameValueCollection nvc = new NameValueCollection ();
                nvc.Add ("datatype", "json");

                BeginRequest (
                    CreateGetRequestState (
                        MiroGuideClientMethod.GetCategories, "/api/list_categories", nvc,
                        ServiceFlags.None, null, userState
                    ), true
                );
            }
        }

        public void GetChannelsAsync (MiroGuideFilterType filterType,
                                      string filterValue,
                                      MiroGuideSortType sortType,
                                      bool reverse,
                                      uint limit, uint offset)
        {
            GetChannelsAsync (filterType, filterValue, sortType, reverse, limit, offset, null);
        }

        public void GetChannelsAsync (MiroGuideFilterType filterType,
                                      string filterValue,
                                      MiroGuideSortType sortType,
                                      bool reverse,
                                      uint limit,
                                      uint offset,
                                      object userState)
        {
            if (String.IsNullOrEmpty (filterValue)) {
                return;
            }

            GetChannelsAsync (new SearchContext (filterType, filterValue, sortType, reverse, limit, offset), userState);
        }

        public void GetChannelsAsync (SearchContext context)
        {
            GetChannelsAsync (context, null);
        }

        public void GetChannelsAsync (SearchContext context, object userState)
        {
            if (context == null) {
                throw new ArgumentNullException ("context");
            }

            NameValueCollection nvc = new NameValueCollection ();

            nvc.Add ("datatype", "json");

            nvc.Add ("filter", ToQueryPart (context.FilterType));
            nvc.Add ("filter_value", context.FilterValue);

            if (context.SortType != MiroGuideSortType.Relevance) {
                nvc.Add (
                    "sort", String.Format ("{0}{1}", ((context.Reverse) ? "-" : String.Empty),
                    ToQueryPart (context.SortType))
                );
            }

            nvc.Add ("limit", context.Limit.ToString ());
            nvc.Add ("offset", (context.Offset+context.Count).ToString ());

            lock (SyncRoot) {
                if (asm.Busy) { // Remove!!!  Allow requests to be queued in the future.
                    return;
                }

                BeginRequest (
                    CreateGetRequestState (
                        MiroGuideClientMethod.GetChannels, "/api/get_channels", nvc,
                        ServiceFlags.None, context, userState
                    ), true
                );
            }
        }

        public void RequestSubsubscription (Uri uri)
        {
            if (uri == null) {
                throw new ArgumentNullException ("uri");
            }

            OnSubscriptionRequested (uri);
        }

        public void RequestSubsubscription (IEnumerable<Uri> uris)
        {
            if (uris == null) {
                throw new ArgumentNullException ("uris");
            }

            OnSubscriptionRequested (uris);
        }

        private void GetSessionAsync (MiroGuideRequestState callingMethodState)
        {
            if (callingMethodState == null) {
                throw new ArgumentNullException ("callingMethodState");
            }

            NameValueCollection nvc = new NameValueCollection ();
            nvc.Add ("datatype", "json");

            lock (SyncRoot) {
                BeginRequest (
                    CreateGetRequestState (
                        MiroGuideClientMethod.GetSession, "/api/get_session", nvc,
                        ServiceFlags.None, null, null, callingMethodState
                    ), false
                );
            }
        }

        public void AddSubscriptionsAsync (IEnumerable<string> urls)
        {
            AddSubscriptionsAsync (urls, null);
        }

        public void AddSubscriptionsAsync (IEnumerable<string> urls, object userState)
        {
            ManageSubscriptionsAsync (MiroGuideClientMethod.AddSubscriptions, urls, userState);
        }

        public void DeleteSubscriptionsAsync (IEnumerable<string> urls)
        {
                DeleteSubscriptionsAsync (urls, null);
        }

        public void DeleteSubscriptionsAsync (IEnumerable<string> urls, object userState)
        {
            ManageSubscriptionsAsync (MiroGuideClientMethod.DelSubscriptions, urls, userState);
        }

        private void ManageSubscriptionsAsync (MiroGuideClientMethod method, IEnumerable<string> urls, object userState)
        {
            lock (SyncRoot) {
                if (asm.Busy) {
                    return;
                }

                string fragment;

                if (method == MiroGuideClientMethod.AddSubscriptions) {
                    fragment = "/api/add_subscriptions";
                } else {
                    fragment = "/api/del_subscriptions";
                }

                JsonObject json_data = new JsonObject ();
                JsonArray request_data = new JsonArray ();

                request_data.AddRange (urls.Select (u => HttpUtility.UrlEncode (u)).Cast<object>());
                json_data["urls"] = request_data;

                BeginRequest (
                    CreatePostRequestState (
                        method, fragment,
                        "application/x-www-form-urlencoded", null,
                        String.Format ("urls={0}", SerializeJson (json_data)),
                        ServiceFlags.RequireAuth, null, null
                    ), true
                );
            }
        }

        public void GetSubscriptionsAsync ()
        {
            GetSubscriptionsAsync (null);
        }

        public void GetSubscriptionsAsync (object userState)
        {
            lock (SyncRoot) {
                if (asm.Busy) {
                    return;
                }

                NameValueCollection nvc = new NameValueCollection ();
                nvc.Add ("datatype", "json");

                BeginRequest (
                    CreateGetRequestState (
                        MiroGuideClientMethod.GetSubscriptions, "/api/get_subscriptions", nvc,
                        ServiceFlags.RequireAuth, null, userState
                    ), true
                );
            }
        }

        private AetherRequest CreateRequest ()
        {
            AetherRequest req = new AetherRequest () {
                Timeout = (30 * 1000),
                Credentials = Credentials,
                UserAgent = UserAgent
            };

            req.Completed += OnRequestCompletedHandler;

            return req;
        }

        private void BeginRequest (MiroGuideRequestState state, bool changeState)
        {
            if (changeState) {
                asm.SetBusy ();
                OnStateChanged (AetherClientState.Idle, AetherClientState.Busy);
            }

            if (asm.Cancelled) {
                state = GetHead (state);
                state.Cancelled = true;
                Complete (state);
                return;
            }

            try {
                if (state.ServiceFlags != ServiceFlags.None) {
                    if ((state.ServiceFlags & ServiceFlags.RequireAuth) != 0) {
                        if (String.IsNullOrEmpty (SessionID)) {
                            GetSessionAsync (state);
                            return;
                        } else {
                            state.AddParameter ("session", SessionID);
                        }
                    }
                }

                request = CreateRequest ();

                switch (state.HttpMethod) {
                case HttpMethod.GET:
                    request.BeginGetRequest (state.GetFullUri (), state);
                    break;
                case HttpMethod.POST:
                    request.ContentType = state.ContentType;
                    request.BeginPostRequest (
                        state.GetFullUri (), Encoding.UTF8.GetBytes (state.RequestData), state
                    );
                    break;
                }
            } catch (Exception e) {
                state = GetHead (state);
                state.Error = e;
                Hyena.Log.Exception (e);
                Complete (state);
            }
        }

        private void Complete (MiroGuideRequestState state)
        {
            try {
                switch (state.Method) {
                case MiroGuideClientMethod.GetSession:
                    HandleGetSessionResponse (state.ResponseData);
                    break;
                }
            } catch (Exception e) {
                state = GetHead (state);
                state.Error = e;
                Hyena.Log.Exception (e);
            }

            if (state.CallingState != null) {
                BeginRequest (state.CallingState, false);
                return;
            } else {
                switch (state.Method) {
                case MiroGuideClientMethod.GetChannels:
                    HandleGetChannelsResponse (state);
                    break;
                case MiroGuideClientMethod.GetCategories:
                    HandleGetCategoriesResponse (state);
                    break;
                case MiroGuideClientMethod.GetSubscriptions:
                    HandleGetSubscriptionsResponse (state);
                    break;
                }

                asm.Reset ();
                OnStateChanged (AetherClientState.Busy, AetherClientState.Idle);
                OnCompleted (state);
            }
        }

        private MiroGuideRequestState CreateGetRequestState (MiroGuideClientMethod acm,
                                                             string path,
                                                             NameValueCollection parameters,
                                                             ServiceFlags flags,
                                                             object internalState,
                                                             object userState)
        {
            return CreateGetRequestState (
                acm, path, parameters, flags, internalState, userState, null
            );
        }

        private MiroGuideRequestState CreateGetRequestState (MiroGuideClientMethod acm,
                                                             string path,
                                                             NameValueCollection parameters,
                                                             ServiceFlags flags,
                                                             object internalState,
                                                             object userState,
                                                             MiroGuideRequestState callingState)
        {
            return CreateRequestState (
                acm, path, HttpMethod.GET, null, parameters, null,
                flags, internalState, userState, callingState
            );
        }

        private MiroGuideRequestState CreatePostRequestState (MiroGuideClientMethod acm,
                                                              string path,
                                                              string contentType,
                                                              NameValueCollection parameters,
                                                              string requestData,
                                                              ServiceFlags flags,
                                                              object internalState,
                                                              object userState)
        {
            return CreatePostRequestState (
                acm, path, contentType, parameters, requestData,
                flags, internalState, userState, null
            );
        }

        private MiroGuideRequestState CreatePostRequestState (MiroGuideClientMethod acm,
                                                              string path,
                                                              string contentType,
                                                              NameValueCollection parameters,
                                                              string requestData,
                                                              ServiceFlags flags,
                                                              object internalState,
                                                              object userState,
                                                              MiroGuideRequestState callingState)
        {
            return CreateRequestState (
                acm, path, HttpMethod.POST, contentType, parameters, requestData,
                flags, internalState, userState, callingState
            );
        }

        private MiroGuideRequestState CreateRequestState (MiroGuideClientMethod acm,
                                                          string path,
                                                          HttpMethod method,
                                                          string contentType,
                                                          NameValueCollection parameters,
                                                          string requestData,
                                                          ServiceFlags flags,
                                                          object internalState,
                                                          object userState,
                                                          MiroGuideRequestState callingState)
        {
            MiroGuideRequestState state = new MiroGuideRequestState () {
                Method = acm,
                RequestData = requestData,
                HttpMethod = method,
                ContentType = contentType,
                ServiceFlags = flags,
                UserState = userState,
                InternalState = internalState,
                CallingState = callingState,
                BaseUri = ServiceUri+path
            };

            if (parameters != null) {
                state.AddParameters (parameters);
            }

            return state;
        }

        private MiroGuideRequestState GetHead (MiroGuideRequestState state)
        {
            while (state.CallingState != null) {
                state = state.CallingState;
            }

            return state;
        }

        private object DeserializeJson (string response)
        {
            Deserializer d = new Deserializer ();
            d.SetInput (response);
            return d.Deserialize ();
        }

        private string SerializeJson (object json)
        {
            return new Serializer (json).Serialize ();
        }

        private void HandleGetSessionResponse (string response)
        {
            object session;
            JsonObject resp = DeserializeJson (response) as JsonObject;

            if (resp.TryGetValue ("session", out session) && !String.IsNullOrEmpty (session as string)) {
                session_id = session as string;
                account.SessionID = session_id;

                ThreadAssist.ProxyToMain (delegate {
                    Banshee.Web.Browser.Open (
                        account.ServiceUri+String.Format ("/api/authenticate?session={0}", session)
                    );
                });
            } else {
                throw new Exception ("Response did not contain session id");
            }
        }

        private void HandleGetCategoriesResponse (MiroGuideRequestState state)
        {
            List<MiroGuideCategoryInfo> categories = null;

            try {
                if (state.Succeeded) {
                    categories = new List<MiroGuideCategoryInfo> ();

                    foreach (JsonObject o in DeserializeJson (state.ResponseData) as JsonArray) {
                        try {
                            categories.Add (new MiroGuideCategoryInfo (o));
                        } catch { continue; }
                    }
                }
            } catch (Exception e) {
                state.Error = e;
            } finally {
                OnGetCategoriesCompleted (state, categories);
            }
        }

        private void HandleGetChannelsResponse (MiroGuideRequestState state)
        {
            List<MiroGuideChannelInfo> channels = new List<MiroGuideChannelInfo> ();

            try {
                if (state.Succeeded) {
                    foreach (JsonObject o in DeserializeJson (state.ResponseData) as JsonArray) {
                        try {
                            channels.Add (new MiroGuideChannelInfo (o));
                        } catch { continue; }
                    }

                    SearchContext context = state.InternalState as SearchContext;
                    context.IncrementResultCount ((uint)channels.Count);
                }
            } catch (Exception e) {
                state.Error = e;
            } finally {
                OnGetChannelsCompleted (state, channels);
            }
        }

        private void HandleGetSubscriptionsResponse (MiroGuideRequestState state)
        {
            List<Uri> urls = null;

            try {
                if (state.Succeeded) {
                    urls = new List<Uri> ();
                    JsonArray ary = (DeserializeJson (state.ResponseData) as JsonObject)["urls"] as JsonArray;

                    foreach (var o in ary) {
                        try {
                            urls.Add (new Uri (o.ToString ()));
                        } catch { continue; }
                    }

                    if (urls.Count > 0) {
                        RequestSubsubscription (urls);
                    }
                }
            } catch (Exception e) {
                state.Error = e;
            }
        }

        private void OnRequestCompletedHandler (object sender, AetherRequestCompletedEventArgs e)
        {
            lock (SyncRoot) {
                request.Completed -= OnRequestCompletedHandler;
                MiroGuideRequestState state = e.UserState as MiroGuideRequestState;

                state.Completed = true;
                state.ResponseData = (e.Data != null) ? Encoding.UTF8.GetString (e.Data) : String.Empty;

                if (e.Cancelled || asm.Cancelled) {
                    state = GetHead (state);
                    state.Cancelled = true;
                } else if (e.Timedout) {
                    state = GetHead (state);
                    state.Timedout = true;
                } else if (e.Error != null) {
                    state = GetHead (state);
                    state.Error = e.Error;
                    Hyena.Log.Exception (e.Error);
                }

                Complete (state);
            }
        }

        private void OnCompleted (MiroGuideRequestState state)
        {
            var handler = Completed;

            RequestCompletedEventArgs e = new RequestCompletedEventArgs (
                state.Error, state.Cancelled, state.Method, state.Timedout, state.UserState
            );

            if (handler != null) {
                EventQueue.Register (new EventWrapper<RequestCompletedEventArgs> (handler, this, e));
            }
        }

        private void OnGetCategoriesCompleted (MiroGuideRequestState state, IEnumerable<MiroGuideCategoryInfo> categories)
        {
            var handler = GetCategoriesCompleted;

            GetCategoriesCompletedEventArgs e = new GetCategoriesCompletedEventArgs (
                categories, state.Error, state.Cancelled, state.Timedout, state.UserState
            );

            if (handler != null) {
                EventQueue.Register (new EventWrapper<GetCategoriesCompletedEventArgs> (handler, this, e));
            }
        }

        private void OnGetChannelsCompleted (MiroGuideRequestState state, IEnumerable<MiroGuideChannelInfo> channels)
        {
            var handler = GetChannelsCompleted;

            GetChannelsCompletedEventArgs e = new GetChannelsCompletedEventArgs (
                state.InternalState as SearchContext, channels,
                state.Error, state.Cancelled, state.Timedout, state.UserState
            );

            if (handler != null) {
                EventQueue.Register (new EventWrapper<GetChannelsCompletedEventArgs> (handler, this, e));
            }
        }

        private void OnSubscriptionRequested (Uri uri)
        {
            OnSubscriptionRequested (new SubscriptionRequestedEventArgs (uri));
        }

        private void OnSubscriptionRequested (IEnumerable<Uri> uris)
        {
            OnSubscriptionRequested (new SubscriptionRequestedEventArgs (uris));
        }

        private void OnSubscriptionRequested (SubscriptionRequestedEventArgs e)
        {
            var handler = SubscriptionRequested;

            if (handler != null) {
                EventQueue.Register (
                    new EventWrapper<SubscriptionRequestedEventArgs> (handler, this, e)
                );
            }
        }

        private string ToQueryPart (MiroGuideFilterType type)
        {
            switch (type)
            {
            case MiroGuideFilterType.Category:  return "category";
            case MiroGuideFilterType.Language:  return "language";
            case MiroGuideFilterType.Name:      return "name";
            case MiroGuideFilterType.Search:    return "search";
            case MiroGuideFilterType.Tag:       return "tag";
            case MiroGuideFilterType.HD:        return "hd";
            case MiroGuideFilterType.Featured:  return "featured";
            case MiroGuideFilterType.TopRated:  return "feed";
            case MiroGuideFilterType.Popular:   goto case MiroGuideFilterType.TopRated;
            default:
                goto case MiroGuideFilterType.Search;
            }
        }

        private string ToQueryPart (MiroGuideSortType type)
        {
            switch (type)
            {
            case MiroGuideSortType.Age:       return "age";
            case MiroGuideSortType.ID:        return "id";
            case MiroGuideSortType.Name:      return "name";
            case MiroGuideSortType.Popular:   return "popular";
            case MiroGuideSortType.Rating:    return "rating";
            case MiroGuideSortType.Relevance: return String.Empty;
            default:
                goto case MiroGuideSortType.Name;
            }
        }
    }
}
