using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Utils
{
    public class MultiSortedDictionary<TKey, TValue>
    {
        #region Field
        private SortedDictionary<TKey, List<TValue>> dic = null;
        #endregion


        #region Property
        public IEnumerable<TKey> Keys
        {
            get
            {
                return this.dic.Keys;
            }
        }

        public List<TValue> this[TKey key]
        {
            get
            {
                List<TValue> list;

                if (this.dic.TryGetValue(key, out list))
                {
                    return list;
                }
                else
                {
                    return new List<TValue>();
                }
            }
        }
        #endregion


        #region Contructor
        public MultiSortedDictionary()
        {
            this.dic = new SortedDictionary<TKey, List<TValue>>();
        }

        public MultiSortedDictionary(IComparer<TKey> comparer)
        {
            this.dic = new SortedDictionary<TKey, List<TValue>>(comparer);
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Add(TKey key, TValue value)
        {
            List<TValue> list;

            if (this.dic.TryGetValue(key, out list))
            {
                list.Add(value);
            }
            else
            {
                list = new List<TValue>();
                list.Add(value);

                this.dic.Add(key, list);
            }
        }

        public bool TryGetValue(TKey key, out List<TValue> value)
        {
            return this.dic.TryGetValue(key, out value);
        }

        public bool Remove(TKey key)
        {
            return this.dic.Remove(key);
        }

        public void RemoveAll()
        {
            foreach (var item in this.dic)
            {
                this.dic.Remove(item.Key);
            }
        }

        public void Clear()
        {
            this.dic.Clear();
        }
        #endregion
    }
}
