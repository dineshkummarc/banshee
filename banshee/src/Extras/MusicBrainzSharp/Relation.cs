using System;
using System.Collections.Generic;

namespace MusicBrainzSharp
{
    public enum RelationDirection
    {
        Forward,
        Backward,
        None
    }
    
    public abstract class RelationPrimative<T>
    {
        T target;
        string type;
        string[] attributes;
        RelationDirection direction;
        DateTime begin;
        DateTime end;

        internal RelationPrimative(string type, T target, RelationDirection direction,
            DateTime begin, DateTime end, string[] attributes)
        {
            this.type = type;
            this.target = target;
            this.direction = direction;
            this.begin = begin;
            this.end = end;
            this.attributes = attributes;
        }

        public T Target { get { return target; } }
        public string Type { get { return type; } }
        public string[] Attributes { get { return attributes; } }
        public RelationDirection Direction { get { return direction; } }
        public DateTime Begin { get { return begin; } }
        public DateTime End { get { return end; } }
    }
    
    public sealed class Relation<T> : RelationPrimative<T> where T : MusicBrainzObject
    {
        internal Relation(string type, T target, RelationDirection direction,
            DateTime begin, DateTime end, string[] attributes)
            : base(type, target, direction, begin, end, attributes)
        {
        }
    }

    public sealed class Relation : RelationPrimative<string>
    {
        internal Relation(string type, string target, RelationDirection direction,
            DateTime begin, DateTime end, string[] attributes)
            : base(type, target, direction, begin, end, attributes)
        {
        }
    }
}
