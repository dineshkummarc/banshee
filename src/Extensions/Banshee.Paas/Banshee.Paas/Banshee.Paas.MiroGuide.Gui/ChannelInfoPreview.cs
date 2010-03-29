//
// ChannelInfoPreview.cs
//
// Authors:
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

using Cairo;

using Hyena.Gui;
using Banshee.Gui;

using Banshee.ServiceStack;
using Banshee.Collection.Gui;

using Banshee.Paas;
using Banshee.Paas.Aether.MiroGuide;

namespace Banshee.Paas.MiroGuide.Gui
{
    // This needs a good default state...
    public class ChannelInfoPreview : ReflectionInfoWidget
    {
        private ArtworkManager artwork_manager;
        private MiroGuideChannelInfo channel_info;

        private static ImageSurface default_channel_image = new PixbufImageSurface (
            IconThemeUtils.LoadIcon ("miroguide-default-channel", 256)
        );

        public MiroGuideChannelInfo ChannelInfo {
            get { return channel_info; }
            set {
                if (value != channel_info) {
                    channel_info = value;
                    QueueDraw ();
                }
            }
        }

        public ChannelInfoPreview ()
        {
            artwork_manager = ServiceManager.Get<ArtworkManager> ();
        }

        protected override bool OnExposeEvent (Gdk.EventExpose evnt)
        {
            // Reset the text / image data on draw in case the image has changed since
            // the ChannelInfo was set.  (e.g.  the image was just downloaded.)

            // There is a flicker on draw, but it was here before I added this.

            if (channel_info != null) {
                /* Set renderer info... */
                ImageSurface image = (artwork_manager == null) ? null
                    : artwork_manager.LookupSurface (PaasService.ArtworkIdFor (channel_info.Name));

                ReflectionImage = image ?? default_channel_image;
            } else {
                ReflectionImage = default_channel_image;
                /* Clear renderer info, show default image / text */
            }

            return base.OnExposeEvent (evnt);
        }
    }
}
