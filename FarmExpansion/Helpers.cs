using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Netcode;
using StardewValley;
using StardewValley.Network;

namespace FarmExpansion
{
    static class Helpers
    {
        public static void ReplaceWith<T>(this NetCollection<T> collection, IEnumerable<T> source)
            where T : INetObject<INetSerializable>
        {
            collection.Clear();
            foreach (var o in source)
            {
                collection.Add(o);
            }
        }

        public static void ReplaceWith<T, TField>(this NetVector2Dictionary<T, TField> collection, NetVector2Dictionary<T, TField> source)
            where TField : NetField<T, TField>, new()
        {
            collection.Clear();
            foreach (var kvp in source.Pairs)
            {
                collection.Add(kvp.Key, kvp.Value);
            }
        }

        public static void ReplaceWith<TKey, TValue>(this OverlaidDictionary<TKey, TValue> collection, OverlaidDictionary<TKey, TValue> source)
        {
            collection.Clear();
            foreach (var kvp in source.Pairs)
            {
                collection.Add(kvp.Key, kvp.Value);
            }
        }
    }
}
