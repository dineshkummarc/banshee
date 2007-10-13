using System;
using System.Collections.Generic;
using System.Text;

namespace MusicBrainzSharp
{
    public abstract class QueryParameters
    {
        public abstract override string ToString();

        protected void EncodeAndAppend(StringBuilder builder, string value)
        {
            // percent-encode this biznatch
            foreach(char c in value) {
                if((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || 
                    c == '-' || c == '_' || c == '.' || c == '~')
                    builder.Append(c);
                else {
                    builder.Append('%');
                    foreach(byte b in Encoding.UTF8.GetBytes(new char[] { c }))
                        builder.Append(String.Format("{0:X}", b));
                } 
            }
        }
    }

    public class Query
    {
        static byte default_limit = 25;
        public static byte DefaultLimit
        {
            get { return default_limit; }
            set {
                if(value < 1 || value > 100)
                    throw new Exception("The limit must be between 1 and 100 inclusively.");
                default_limit = value;
            }
        }
    }
    
    public class Query<T> : IEnumerable<T> where T : MusicBrainzObject
    {
        string parameters;
        string url_extension;

        internal Inc[] ArtistReleaseIncs;

        internal Query(string url_extension, byte limit, int offset, string parameters)
        {
            if(limit < 1 || limit > 100)
                throw new ArgumentException("The limit must be between 1 and 100 inclusively.");
            this.url_extension = url_extension;
            this.limit = limit;
            this.offset = offset;
            this.parameters = parameters;
        }
        
        List<T> results;
        public List<T> ResultsWindow
        {
            get {
                if(results == null)
                    results = MusicBrainzObject.DoQuery<T>(url_extension, limit, offset, parameters, ArtistReleaseIncs, out count);
                return results;
            }
        }

        byte limit;
        public byte Limit { get { return limit; } }

        int offset;
        Dictionary<int, WeakReference> weak_references = new Dictionary<int, WeakReference>();
        public int Offset
        {
            get { return offset; }
            set {
                // We WeakReference the results from previous offsets just in case.
                if(results != null)
                    if(!weak_references.ContainsKey(offset))
                        weak_references.Add(offset, new WeakReference(results));
                    else
                        ((WeakReference)weak_references[offset]).Target = results;
                results = null;
                offset = value;
                if(weak_references.ContainsKey(offset)) {
                    WeakReference weak_reference = weak_references[offset] as WeakReference;
                    if(weak_reference.IsAlive)
                        results = weak_reference.Target as List<T>;
                }
            }
        }

        int? count;
        public int Count
        {
            get {
                if(!count.HasValue && ResultsWindow == null)
                    ; // just accessing ResultsWindow will give count a value
                return count.Value;
            }
        }

        public T this[int i]
        {
            get {
                if(i < 0 || i >= Count)
                    throw new IndexOutOfRangeException();
                if(i <= offset || i >= offset + limit)
                    Offset = i;
                return ResultsWindow[i - offset];
            }
        }

        public List<T> ToList()
        {
            List<T> list = new List<T>(Count);
            foreach(T result in this)
                list.Add(result);
            return list;
        }

        public IEnumerator<T> GetEnumerator()
        {
            Offset = 0;
            int count = 0;
            while(count < Count) {
                foreach(T result in ResultsWindow)
                    yield return result;
                count += ResultsWindow.Count;
                Offset = count;
            }
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
