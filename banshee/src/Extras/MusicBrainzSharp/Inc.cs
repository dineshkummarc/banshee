using System;

namespace MusicBrainzSharp
{
    public abstract class Inc
    {
        protected Inc(int value)
        {
            this.value = value;
        }
        protected string name;
        protected int value;
        public int Value { get { return value; } }
        public string Name { get { return name; } }
        public static implicit operator Inc(BaseIncType e)
        {
            return new BaseInc(e);
        }
        public static implicit operator Inc(ArtistIncType e)
        {
            return new ArtistInc(e);
        }
        public static implicit operator Inc(ReleaseIncType e)
        {
            return new ReleaseInc(e);
        }
        public static implicit operator Inc(TrackIncType e)
        {
            return new TrackInc(e);
        }
        public static implicit operator Inc(LabelIncType e)
        {
            return new LabelInc(e);
        }
    }
}
