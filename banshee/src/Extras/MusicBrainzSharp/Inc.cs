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
    }
}
