// EditableLookup is from the Miscellaneous Utility Library Version 1.0
// Copyright (c) 2004-2008 Jon Skeet and Marc Gravell.
// All rights reserved.
// https://jonskeet.uk/csharp/miscutil/
// Included here as the official NuGet package does not support .NET Core.
using System;
using System.Collections.Generic;
using System.Linq;
using MiscUtil.Extensions;

namespace MiscUtil.Linq
{
    /// <summary>
    /// Simple non-unique map wrapper
    /// </summary>
    /// <remarks>
    /// ApplyResultSelector (from Lookup[TKey, TElement] is not implemented,
    /// since the caller could just as easily (or more-so) use .Select() with
    /// a Func[IGrouping[TKey, TElement], TResult], since
    /// IGrouping[TKey, TElement] already includes both the "TKey Key"
    /// and the IEnumerable[TElement].
    /// </remarks>
    public sealed partial class EditableLookup<TKey, TElement> : ILookup<TKey, TElement>
    {
        private readonly Dictionary<TKey, LookupGrouping> groups;
        /// <summary>
        /// Creates a new EditableLookup using the default key-comparer
        /// </summary>
        public EditableLookup() : this(null) {}
        /// <summary>
        /// Creates a new EditableLookup using the specified key-comparer
        /// </summary>
        /// <param name="keyComparer"></param>
        public EditableLookup(IEqualityComparer<TKey> keyComparer)
        {
            groups = new Dictionary<TKey, LookupGrouping>(
                keyComparer ?? EqualityComparer<TKey>.Default);
        }
        /// <summary>
        /// Does the lookup contain any value(s) for the given key?
        /// </summary>
        public bool Contains(TKey key)
        {
            LookupGrouping group;
            return groups.TryGetValue(key, out group) ? group.Count > 0 : false;
        }
        /// <summary>
        /// Does the lookup the specific key/value pair?
        /// </summary>
        public bool Contains(TKey key, TElement value)
        {
            LookupGrouping group;
            return groups.TryGetValue(key, out group) ? group.Contains(value) : false;
        }
        /// <summary>
        /// Adds a key/value pair to the lookup
        /// </summary>
        /// <remarks>If the value is already present it will be duplicated</remarks>
        public void Add(TKey key, TElement value)
        {
            LookupGrouping group;
            if (!groups.TryGetValue(key, out group))
            {
                group = new LookupGrouping(key);
                groups.Add(key, group);
            }
            group.Add(value);
        }
        /// <summary>
        /// Adds a range of values against a single key
        /// </summary>
        /// <remarks>Any values already present will be duplicated</remarks>
        public void AddRange(TKey key, IEnumerable<TElement> values)
        {
            values.ThrowIfNull("values");
            LookupGrouping group;
            if (!groups.TryGetValue(key, out group))
            {
                group = new LookupGrouping(key);
                groups.Add(key, group);
            }
            foreach (TElement value in values)
            {
                group.Add(value);
            }
            if (group.Count == 0)
            {
                groups.Remove(key); // nothing there after all!
            }
        }
        /// <summary>
        /// Add all key/value pairs from the supplied lookup
        /// to the current lookup
        /// </summary>
        /// <remarks>Any values already present will be duplicated</remarks>
        public void AddRange(ILookup<TKey, TElement> lookup)
        {
            lookup.ThrowIfNull("lookup");;
            foreach (IGrouping<TKey, TElement> group in lookup)
            {
                AddRange(group.Key, group);
            }
        }
        /// <summary>
        /// Remove all values from the lookup for the given key
        /// </summary>
        /// <returns>True if any items were removed, else false</returns>
        public bool Remove(TKey key)
        {
            return groups.Remove(key);
        }
        /// <summary>
        /// Remove the specific key/value pair from the lookup
        /// </summary>
        /// <returns>True if the item was found, else false</returns>
        public bool Remove(TKey key, TElement value)
        {
            LookupGrouping group;
            if (groups.TryGetValue(key, out group))
            {
                bool removed = group.Remove(value);
                if (removed && group.Count == 0)
                {
                    groups.Remove(key);
                }
                return removed;
            }
            return false;
        }
        /// <summary>
        /// Trims the inner data-structure to remove
        /// any surplus space
        /// </summary>
        public void TrimExcess()
        {
            foreach (var group in groups.Values)
            {
                group.TrimExcess();
            }
        }
        /// <summary>
        /// Returns the number of dictinct keys in the lookup
        /// </summary>
        public int Count
        {
            get { return groups.Count; }
        }

        private static readonly IEnumerable<TElement> Empty = new TElement[0];
        /// <summary>
        /// Returns the set of values for the given key
        /// </summary>
        public IEnumerable<TElement> this[TKey key]
        {
            get
            {
                LookupGrouping group;
                if (groups.TryGetValue(key, out group))
                {
                    return group;
                }
                return Empty;
            }
        }
        /// <summary>
        /// Returns the sequence of keys and their contained values
        /// </summary>
        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
        {
            foreach (var group in groups.Values)
            {
                yield return group;
            }
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        internal sealed class LookupGrouping : IGrouping<TKey, TElement>
        {
            private readonly TKey key;
            private List<TElement> items = new List<TElement>();
            public TKey Key { get { return key; } }
            public LookupGrouping(TKey key)
            {
                this.key = key;
            }
            public int Count
            {
                get { return items.Count; }
            }
            public void Add(TElement item)
            {
                items.Add(item);
            }
            public bool Contains(TElement item)
            {
                return items.Contains(item);
            }
            public bool Remove(TElement item)
            {
                return items.Remove(item);
            }
            public void TrimExcess()
            {
                items.TrimExcess();
            }

            public IEnumerator<TElement> GetEnumerator()
            {
                return items.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}

namespace MiscUtil.Extensions
{
    /// <summary>
    /// Extension methods on all reference types. 
    /// </summary>
    public static class ObjectExt
    {
        /// <summary>
        /// Throws an ArgumentNullException if the given data item is null.
        /// </summary>
        /// <param name="data">The item to check for nullity.</param>
        /// <param name="name">The name to use when throwing an exception, if necessary</param>
        public static void ThrowIfNull<T>(this T data, string name)where T : class
        {
            if (data == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        /// <summary>
        /// Throws an ArgumentNullException if the given data item is null.
        /// No parameter name is specified.
        /// </summary>
        /// <param name="data">The item to check for nullity.</param>
        public static void ThrowIfNull<T>(this T data)where T : class
        {
            if (data == null)
            {
                throw new ArgumentNullException();
            }
        }
    }
}