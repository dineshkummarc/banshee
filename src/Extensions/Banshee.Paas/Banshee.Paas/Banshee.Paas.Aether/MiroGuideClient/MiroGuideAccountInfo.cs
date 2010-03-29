//
// MiroGuideAccountInfo.cs
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

using System;

namespace Banshee.Paas.Aether.MiroGuide
{
    public class MiroGuideAccountInfo
    {
        private string username, password_hash, service_uri, session_id;

        public EventHandler<EventArgs> Updated;

        public string SessionID {
            get { return session_id; }

            set {
                if (!String.IsNullOrEmpty (value) && value != session_id) {
                    session_id = value;
                    Notify ();
                }
            }
        }

        public string PasswordHash {
            get { return password_hash; }

            set {
                if (!String.IsNullOrEmpty (value) && value != password_hash) {
                    password_hash = value;
                    Notify ();
                }
            }
        }

        public string ServiceUri {
            get { return service_uri; }

            set {
                if (!String.IsNullOrEmpty (value) && value != service_uri) {
                    service_uri = value;
                    Notify ();
                }
            }
        }

        public string Username {
            get { return username; }

            set {
                if (!String.IsNullOrEmpty (value) && value != username) {
                    username = value;
                    Notify ();
                }
            }
        }

        public MiroGuideAccountInfo (string serviceUri,
                                     string sessionID,
                                     string username,
                                     string passwordHash)
        {
            session_id = sessionID;

            password_hash = passwordHash;
            this.username = username;
            service_uri = serviceUri;
        }

        public void Notify ()
        {
            OnUpdated ();
        }

        protected virtual void OnUpdated ()
        {
            var handler = Updated;

            if (handler != null) {
                handler (this, EventArgs.Empty);
            }
        }
    }
}
