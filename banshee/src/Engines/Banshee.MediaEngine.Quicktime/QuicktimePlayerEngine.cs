/***************************************************************************
 *  QuicktimePlayerEngine.cs
 *
 *  Written by Scott Peterson <scottp@gnome.org>
 ****************************************************************************/

/*  THIS FILE IS LICENSED UNDER THE MIT LICENSE AS OUTLINED IMMEDIATELY BELOW: 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a
 *  copy of this software and associated documentation files (the "Software"),  
 *  to deal in the Software without restriction, including without limitation  
 *  the rights to use, copy, modify, merge, publish, distribute, sublicense,  
 *  and/or sell copies of the Software, and to permit persons to whom the  
 *  Software is furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 *  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 *  DEALINGS IN THE SOFTWARE.
 */
 
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Timers;
using Mono.Unix;

using Banshee.Base;
using Banshee.MediaEngine;
using Banshee.Gstreamer;

public static class PluginModuleEntry
{
    public static Type [] GetTypes()
    {
        return new Type [] {
            typeof(Banshee.MediaEngine.Quicktime.QuicktimePlayerEngine)
        };
    }
}

namespace Banshee.MediaEngine.Quicktime
{
    
    public class QuicktimePlayerEngine : PlayerEngine
    {
        private ActiveXControl control;
        private System.Timers.Timer timer;

        public QuicktimePlayerEngine()
        {
            control = new ActiveXControl();
            control.axQTControl.Error += new AxQTOControlLib._IQTControlEvents_ErrorEventHandler(axQTControl_Error);
        }

        void axQTControl_Error(object sender, AxQTOControlLib._IQTControlEvents_ErrorEvent e)
        {
            Console.WriteLine("Quicktime Error: " + e);
            OnEventChanged(PlayerEngineEvent.Error);
            Close();
        }

        public override void Dispose()
        {
            control.Dispose();
            timer.Dispose();
            base.Dispose();
        }

        protected override void OpenUri(SafeUri uri)
        {
            control.axQTControl.URL = uri.LocalPath;
            if(timer == null) {
                timer = new System.Timers.Timer(500);
                timer.AutoReset = true;
                timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
                timer.Enabled = false;
            }
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            OnEventChanged(PlayerEngineEvent.Iterate);
            if(control.axQTControl.Movie == null) {
                timer.Stop();
                return;
            }
            if(control.axQTControl.Movie.Time == control.axQTControl.Movie.Duration) {
                Close();
                OnEventChanged(PlayerEngineEvent.EndOfStream);
            }
        }

        public override void Play()
        {
            control.axQTControl.Movie.Play(1);
            timer.Start();
            base.Play();
        }

        public override void Pause()
        {
            control.axQTControl.Movie.Pause();
            timer.Stop();
            base.Pause();
        }

        public override void Close()
        {
            control.axQTControl.URL = "";
            timer.Stop();
            base.Close();
        }

        public override ushort Volume {
            get {
                return control.axQTControl.Movie != null
                    ? (ushort)(control.axQTControl.Movie.AudioVolume * 100)
                    : (ushort)0;
            }
            set {
                if(control.axQTControl.Movie != null) {
                    control.axQTControl.Movie.AudioVolume = (float)value / (float)100;
                }
            }
        }

        public override uint Position {
            get {
                return control.axQTControl.Movie != null
                  ? (uint)(control.axQTControl.Movie.Time / 600)
                  : 0;
            }
            set {
                if(control.axQTControl.Movie != null) {
                    control.axQTControl.Movie.Time = (int)(value * 600);
                }
            }
        }

        public override uint Length {
            get { return control.axQTControl.Movie != null
                    ? (uint)(control.axQTControl.Movie.Duration / 600)
                    : 0; }
        }

        private static string[] source_capabilities = { "file" };
        public override IEnumerable SourceCapabilities
        {
            get { return source_capabilities; }
        }

        private static string[] decoder_capabilities = { "m4p", "m4a" };
        public override IEnumerable ExplicitDecoderCapabilities
        {
            get { return decoder_capabilities; }
        }

        public override string Id
        {
            get { return "quicktime"; }
        }

        public override string Name
        {
            get { return Catalog.GetString("Quicktime"); }
        }
    }
}
