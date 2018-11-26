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
        public static void ReplaceWith<T, TField>(this NetVector2Dictionary<T, TField> collection, NetVector2Dictionary<T, TField> source)
            where TField : NetField<T, TField>, new()
        {
            collection.Clear();
            foreach (var kvp in source.Pairs)
            {
                collection.Add(kvp.Key, kvp.Value);
            }
        }

        public static void ReplaceWith(this OverlaidDictionary collection, OverlaidDictionary source)
        {
            collection.Clear();
            foreach (var kvp in source.Pairs)
            {
                collection.Add(kvp.Key, kvp.Value);
            }
        }

        public static void ReplaceWith<T>(this Netcode.NetCollection<T> collection, Netcode.NetCollection<T> source)
            where T : class, Netcode.INetObject<Netcode.INetSerializable>
        {
            collection.Clear();
            foreach (var item in source)
            {
                collection.Add(item);
            }
        }
    }
}
