using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;


namespace Mineral.Common.Stroage.LevelDB
{
    internal class DbCache<TKey, TValue>
        where TKey : IEquatable<TKey>, ISerializable, new()
        where TValue : class, ISerializable, new()
    {
        protected internal class Trackable
        {
            public TKey Key;
            public TValue Item;
            public TrackState State;
        }

        public enum TrackState : byte
        {
            None,
            Added,
            Changed,
            Deleted
        }

        protected DB _db = null;
        protected byte _prefix = 0x00;
        protected ReadOptions _opt = ReadOptions.Default;
        protected ConcurrentDictionary<TKey, Trackable> _cache = new ConcurrentDictionary<TKey, Trackable>();

        public DbCache(DB db, byte prefix, ReadOptions opt = null)
        {
            _db = db;
            if (opt != null)
                _opt = opt;
            _prefix = prefix;
        }

        public void Commit(WriteBatch batch)
        {
            foreach (Trackable trackable in GetChanged())
            {
                if (trackable.State == TrackState.Deleted)
                    batch.Delete(_prefix, trackable.Key);
                else
                    batch.Put(_prefix, trackable.Key, trackable.Item);
            }
        }

        protected IEnumerable<KeyValuePair<TKey, TValue>> FindInternal(byte[] keyPrefix)
        {
            return _db.Find(_opt, SliceBuilder.Begin(_prefix).Add(keyPrefix),
                (k, v) => new KeyValuePair<TKey, TValue>(k.ToArray().Serializable<TKey>(), v.ToArray().Serializable<TValue>()));
        }

        protected TValue GetInternal(TKey key)
        {
            return _db.Get<TValue>(_opt, _prefix, key);
        }

        protected TValue TryGetInternal(TKey key)
        {
            return _db.TryGet<TValue>(_opt, _prefix, key);
        }

        public void Add(TKey key, TValue value)
        {
            if (_cache.TryGetValue(key, out Trackable trackable) && trackable.State != TrackState.Deleted)
                throw new ArgumentException();

            _cache[key] = new Trackable
            {
                Key = key,
                Item = value,
                State = (trackable == null) ? TrackState.Added : TrackState.Changed
            };
        }

        public void Delete(TKey key)
        {
            if (_cache.TryGetValue(key, out Trackable trackable))
            {
                if (trackable.State == TrackState.Added)
                    _cache.TryRemove(key, out _);
                else
                    trackable.State = TrackState.Deleted;
            }
            else
            {
                TValue item = TryGetInternal(key);
                if (item == null)
                    return;
                _cache.TryAdd(key, new Trackable
                {
                    Key = key,
                    Item = item,
                    State = TrackState.Deleted
                });
            }
        }

        public void DeleteWhere(Func<TKey, TValue, bool> predicate)
        {
            List<TKey> dels = new List<TKey>();
            foreach (Trackable trackable in _cache.Where(p => p.Value.State != TrackState.Deleted && predicate(p.Key, p.Value.Item)).Select(p => p.Value))
            {
                if (trackable.State == TrackState.Added)
                    dels.Add(trackable.Key);
                else
                    trackable.State = TrackState.Deleted;
            }
            dels.Select(p => _cache.TryRemove(p, out _));
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> Find(byte[] keyPrefix = null)
        {
            foreach (var pair in FindInternal(keyPrefix ?? new byte[0]))
            {
                if (!_cache.ContainsKey(pair.Key))
                {
                    _cache.TryAdd(pair.Key, new Trackable
                    {
                        Key = pair.Key,
                        Item = pair.Value,
                        State = TrackState.None
                    });
                }
            }
            foreach (var pair in _cache)
            {
                if (pair.Value.State != TrackState.Deleted && pair.Key.ToArray().Take(keyPrefix.Length).SequenceEqual(keyPrefix))
                    yield return new KeyValuePair<TKey, TValue>(pair.Key, pair.Value.Item);
            }
        }

        protected internal IEnumerable<Trackable> GetChanged()
        {
            return _cache.Values.Where(p => p.State != TrackState.None);
        }

        public TValue GetAndChange(TKey key, Func<TValue> factory = null)
        {
            if (_cache.TryGetValue(key, out Trackable trackable))
            {
                if (trackable.State == TrackState.Deleted)
                {
                    if (factory == null)
                        return null;
                    
                    trackable.Item = factory();
                    trackable.State = TrackState.Changed;
                }
                else if (trackable.State == TrackState.None)
                {
                    trackable.State = TrackState.Changed;
                }
            }
            else
            {
                trackable = new Trackable
                {
                    Key = key,
                    Item = TryGetInternal(key)
                };
                if (trackable.Item == null)
                {
                    if (factory == null)
                        return null;
                    
                    trackable.Item = factory();
                    trackable.State = TrackState.Added;
                }
                else
                {
                    trackable.State = TrackState.Changed;
                }
                _cache.TryAdd(key, trackable);
            }
            return trackable.Item;
        }

        public TValue GetOrAdd(TKey key, Func<TValue> factory)
        {
            if (_cache.TryGetValue(key, out Trackable trackable))
            {
                if (trackable.State == TrackState.Deleted)
                {
                    trackable.Item = factory();
                    trackable.State = TrackState.Changed;
                }
            }
            else
            {
                trackable = new Trackable
                {
                    Key = key,
                    Item = TryGetInternal(key)
                };
                if (trackable.Item == null)
                {
                    trackable.Item = factory();
                    trackable.State = TrackState.Added;
                }
                else
                {
                    trackable.State = TrackState.None;
                }
                _cache.TryAdd(key, trackable);
            }
            return trackable.Item;
        }

        public TValue TryGet(TKey key)
        {
            if (_cache.TryGetValue(key, out Trackable trackable))
                return trackable.State == TrackState.Deleted ? null : trackable.Item;

            TValue value = TryGetInternal(key);
            if (value == null)
                return null;

            _cache.TryAdd(key, new Trackable
            {
                Key = key,
                Item = value,
                State = TrackState.None
            });
            return value;
        }

        public bool ContainsKey(TKey key)
        {
            if (_cache.ContainsKey(key)) return true;
            return TryGetInternal(key) != null;
        }
    }
}
