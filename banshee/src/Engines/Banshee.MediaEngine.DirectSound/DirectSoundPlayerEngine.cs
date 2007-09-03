using System;
using System.Collections;
using System.Timers;

using Microsoft.DirectX.AudioVideoPlayback;

using Banshee.MediaEngine;

public static class PluginModuleEntry
{
    public static Type[] GetTypes()
    {
        return new Type[] {
            typeof(Banshee.MediaEngine.DirectSound.DirectSoundPlayerEngine)
        };
    }
}

namespace Banshee.MediaEngine.DirectSound
{
    class DirectSoundPlayerEngine : PlayerEngine
    {
        private Audio audio;
        private Timer timer;

        public override void Play()
        {
            if (audio != null) {
                audio.Play();
                timer.Start();
            }
            base.Play();
        }

        public override void Pause()
        {
            if (audio != null) {
                audio.Pause();
                timer.Stop();
            }
            base.Pause();
        }

        public override void Close()
        {
            if (audio != null) {
                audio.Stop();
                audio.Dispose();
                timer.Stop();
            }
            base.Close();
        }

        public override void Dispose()
        {
            if (audio != null) {
                audio.Dispose();
                audio = null;

                timer.Dispose();
                timer = null;
            }
            base.Dispose();
        }

        protected override void OpenUri(Banshee.Base.SafeUri uri)
        {
            try {
                audio = new Audio(uri.AbsolutePath, false);
                audio.Ending += new EventHandler(audio_Ending);
                if(timer == null) {
                    timer = new Timer(500);
                    timer.AutoReset = true;
                    timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
                    timer.Enabled = false;
                }
            } catch {
                // TODO handle error
            }
        }

        private void audio_Ending(object sender, EventArgs e)
        {
            Close();
            OnEventChanged(PlayerEngineEvent.EndOfStream);
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            OnEventChanged(PlayerEngineEvent.Iterate);
        }

        public override ushort Volume
        {
            // -10000 is silent and 0 is full volume. Go figure.
            // Volume is also logarithmic and there doens't seem to be a linear option.
            // TODO convert logarithmic scale to linear
            get {
                if(audio == null) {
                    return 0;
                } else {
                    int volume = audio.Volume;
                    volume = 0 - volume;
                    double perc = (double)volume / (double)10000;
                    perc = 1 - perc;
                    return (ushort)(100 * perc);
                }
            }
            set {
                if (audio != null) {
                    double perc = (double)value / (double)100;
                    perc = 1 - perc;
                    int volume = (int)(10000 * perc);
                    volume = 0 - volume;
                    audio.Volume = volume;
                }
            }
        }

        public override uint Position
        {
            get {
                return audio != null ? (uint)audio.CurrentPosition : 0;
            }
            set {
                if (audio != null) {
                    audio.SeekCurrentPosition(value * 10000000, SeekPositionFlags.AbsolutePositioning);
                }
            }
        }

        public override uint Length
        {
            get { return audio != null ? (uint)audio.Duration : 0; }
        }

        public override bool CanSeek
        {
            get {
                return audio != null && audio.SeekingCaps.CanSeekAbsolute;
            }
        }

        private static string[] source_capabilities = { "file" };
        public override IEnumerable SourceCapabilities
        {
            get { return source_capabilities; }
        }

        private static string[] decoder_capabilities = { "ogg", "wma", "asf", "flac" };
        public override IEnumerable ExplicitDecoderCapabilities
        {
            get { return decoder_capabilities; }
        }

        public override string Id
        {
            get { return "directshow"; }
        }

        public override string Name
        {
            get { return "Microsoft DirectShow"; }
        }
    }
}
