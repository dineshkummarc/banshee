//
// BrowseChannelsSource.cs
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
using System.Linq;

using Mono.Unix;

using Gtk;

using Hyena;
using Banshee.Base;

using Banshee.Paas.Data;
using Banshee.Paas.MiroGuide.Gui;
using Banshee.Paas.Aether.MiroGuide;

namespace Banshee.Paas.MiroGuide
{
    public class BrowseChannelsSource : ChannelSource
    {
        private bool categories_received;
        private MiroGuideCategoryListModel category_model;

        public MiroGuideCategoryListModel CategoryModel {
            get { return category_model; }
        }

        public BrowseChannelsSource (MiroGuideClient client) : base (client,
                                                                     MiroGuideFilterType.Category,
                                                                     "MiroGuideBrowseChannels",
                                                                     Catalog.GetString ("Browse"),
                                                                     (int)MiroGuideSourcePosition.Browse)
        {
            ActiveSortType = MiroGuideSortType.Rating;
            category_model = new MiroGuideCategoryListModel ();

            Properties.SetStringList ("Icon.Name", "miro-browse");
            Properties.Set<bool> ("MiroGuide.Gui.Source.ShowSortPreference", true);
        }

        public override void Activate ()
        {
            base.Activate ();

            Client.GetCategoriesCompleted += OnGetCategoriesCompletedHandler;
            category_model.Selection.Changed += OnSelectionChangedHandler;

            if (!categories_received) {
                Client.GetCategoriesAsync (this);
            }
        }

        public override void Deactivate ()
        {
            Client.GetCategoriesCompleted -= OnGetCategoriesCompletedHandler;
            category_model.Selection.Changed -= OnSelectionChangedHandler;

            base.Deactivate ();
        }

        protected override ChannelSourceContents CreateChannelSourceContents ()
        {
            return new BrowserSourceContents ();
        }

        public override void Refresh ()
        {
            ThreadAssist.ProxyToMain (delegate {
                if (!categories_received) {
                    Client.GetCategoriesAsync (this);
                }

                base.Refresh ();
            });
        }

        private MiroGuideCategoryInfo selected_category = null;
        protected virtual void OnSelectionChangedHandler (object sender, EventArgs e)
        {
            MiroGuideCategoryInfo category = category_model.GetSelected ().FirstOrDefault ();

            if (category != null && category != selected_category) {
                Client.CancelAsync ();
                ClientHandle.WaitOne ();

                GetChannelsAsync (category.Name);
                selected_category = category;
            }
        }

        protected virtual void OnGetCategoriesCompletedHandler (object sender, GetCategoriesCompletedEventArgs e)
        {
            if (e.UserState != this) {
                return;
            }

            ThreadAssist.ProxyToMain (delegate {
                if (e.Cancelled) {
                    return;
                } else if (e.Error != null) {
                    SetErrorStatus ();
                    return;
                }

                if (e.Categories != null) {
                    CategoryModel.Add (e.Categories);
                    categories_received = true;
                }
            });
        }
    }
}