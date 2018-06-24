using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmioks.Common.Entity;
using Dmioks.Common.Json;

namespace Dmioks.Common.Utils
{
    public static class ControllerHelper
    {
        public static T[] ToUniqueArray<T>(T[] arrSource, int iMaxLength = int.MaxValue)
        {
            HashSet<T> hs = new HashSet<T>(arrSource);
            T[] arrTarget = hs.ToArray();

            if (iMaxLength < arrTarget.Length)
            {
                Array.Resize(ref arrTarget, iMaxLength);
            }

            return arrTarget;
        }

        public delegate D DelegateCreateJsonObject<D>() where D : JsonObject;

        public static Dictionary<K, List<D>> GetRelations<K, T, D>(PropertyKey<K> pkParent, List<T> lItems, DelegateCreateJsonObject<D> dcjo)
            where T : SimpleEntity
            where D : JsonObject
        {
            Dictionary<K, List<D>> di = new Dictionary<K, List<D>>();

            foreach (T objItem in lItems)
            {
                K key = pkParent.Value(objItem);

                if (!di.TryGetValue(key, out List<D> lItemsDto))
                {
                    lItemsDto = new List<D>();
                    di.Add(key, lItemsDto);
                }

                D objDto = dcjo();
                objItem.ToJsonObject<D>(objDto);
                lItemsDto.Add(objDto);
            }

            return di;
        }
    }
}
