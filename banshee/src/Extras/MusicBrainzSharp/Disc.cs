using System;
using System.Xml;
using System.Text;

namespace MusicBrainzSharp
{
    public class Disc
    {
        string id;
        int sectors;
        byte first_track;
        byte last_track;
        int[] track_offsets = new int[100];

        protected Disc()
        {
        }
        
        internal Disc(XmlReader reader)
        {
            reader.Read();
            string sectors_string = reader["sectors"];
            if(sectors_string != null)
                sectors = int.Parse(sectors_string);
            id = reader["id"];
            reader.Close();
        }

        public string Id
        {
            get { return id; }
        }

        public int Sectors
        {
            get { return sectors; }
            protected set { sectors = value; }
        }

        protected byte FirstTrack
        {
            get { return first_track; }
            set { first_track = value; }
        }

        protected byte LastTrack
        {
            get { return last_track; }
            set { last_track = value; }
        }

        protected int[] TrackOffsets
        {
            get { return track_offsets; }
        }

        int[] track_durations;
        public int[] TrackDurations
        {
            get { return track_durations; }
        }

        void GenerateId()
        {
            SHA1MusicBrainz sha1 = new SHA1MusicBrainz();
            sha1.Update(string.Format("{0:X2}", FirstTrack));
            sha1.Update(string.Format("{0:X2}", LastTrack));
            for(int i = 0; i < 100; i++)
                sha1.Update(string.Format("{0:X8}", track_offsets[i]));

            // MB uses a slightly modified RFC822 for reasons of URL happiness.
            string base64 = Convert.ToBase64String(sha1.Final());
            StringBuilder builder = new StringBuilder(base64.Length);
            foreach(char c in base64)
                if(c == '+')
                    builder.Append('.');
                else if(c == '/')
                    builder.Append('_');
                else if(c == '=')
                    builder.Append('-');
                else
                    builder.Append(c);
            id = builder.ToString();
        }

        public static Disc GetFromDevice(string device)
        {
            Disc result = null;

            if(Environment.OSVersion.Platform != PlatformID.Unix)
                result = new DiscWin32(device);

            result.track_durations = new int[result.last_track];
            for(int i = 1; i <= result.last_track; i++) {
                result.track_durations[i - 1] = i < result.last_track
                    ? result.track_offsets[i + 1] - result.track_offsets[i]
                    : result.track_offsets[0] - result.track_offsets[i];
                result.track_durations[i - 1] /= 75; // 75 frames in a second
            }
            result.GenerateId();
            return result;
        }
    }
}
