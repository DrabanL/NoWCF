using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NoWCF.Utilities
{
    public static class SynchronizedCollectionExtensions
    {
        public static IReadOnlyCollection<T> AsReadOnly<T>(this SynchronizedCollection<T> value)
        {
            lock (value.SyncRoot)
            {
                // this call is not expensive as it is just a thin wrapper around the IList<T>
                return new ReadOnlyCollection<T>(value);
            }
        }
    }
}
