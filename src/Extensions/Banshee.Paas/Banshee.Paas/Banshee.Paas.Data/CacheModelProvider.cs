//
// CacheModelProvider.cs
//
// Authors:
//   Gabriel Burt <gburt@novell.com>
//
// Copyright (C) 2008 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Threading;
using System.Collections.Generic;

using Hyena;
using Hyena.Data;
using Hyena.Data.Sqlite;

namespace Banshee.Paas.Data
{
    // Caches all results retrieved from the database, such that any subsequent retrieval will
    // return the same instance.
    public class CacheModelProvider<T> : SqliteModelProvider<T> where T : CacheableItem<T>, new()
    {
        private ReaderWriterLock rw_lock = new ReaderWriterLock ();
        private Dictionary<long, T> full_cache = new Dictionary<long, T> ();

        public CacheModelProvider (HyenaSqliteConnection connection, string table_name) : base (connection, table_name)
        {
        }

        #region Overrides

        public override T FetchSingle (long id)
        {
            return GetCached (id) ?? CacheResult (base.FetchSingle (id));
        }

        public override void Save (T target)
        {
            base.Save (target);
            rw_lock.AcquireWriterLock (-1);

            try {
                if (!full_cache.ContainsKey (target.DbId)) {
                    full_cache[target.DbId] = target;
                }
            } finally {
                rw_lock.ReleaseWriterLock ();
            }
        }

        public override T Load (IDataReader reader)
        {
            return GetCached (PrimaryKeyFor (reader)) ?? CacheResult (base.Load (reader));
        }

        public override void Delete (long id)
        {
            base.Delete (id);
            rw_lock.AcquireWriterLock (-1);

            try {
                full_cache.Remove (id);
            } finally {
                rw_lock.ReleaseWriterLock ();
            }
        }

        public override void Delete (IEnumerable<T> items)
        {
            base.Delete (items);
            rw_lock.AcquireWriterLock (-1);

            try {
                foreach (T item in items) {
                    if (item != null) {
                        full_cache.Remove (PrimaryKeyFor (item));
                    }
                }
            } finally {
                rw_lock.ReleaseWriterLock ();
            }
        }

        #endregion

#region Utility Methods

        private T GetCached (long id)
        {
            rw_lock.AcquireReaderLock (-1);

            try {
                if (full_cache.ContainsKey (id)) {
                    return full_cache[id];
                } else {
                    return null;
                }
            } finally {
                rw_lock.ReleaseReaderLock ();
            }
        }

        private T CacheResult (T item)
        {
            if (item == null) {
                return null;
            }

            rw_lock.AcquireWriterLock (-1);

            try {
                full_cache[item.DbId] = item;
                return item;
            } finally {
                rw_lock.ReleaseWriterLock ();
            }
        }

#endregion

    }
}