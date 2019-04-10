using Mineral.Database.LevelDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Database.BlockChain
{
    internal class BaseLevelDB : IDisposable
    {
        #region Internal Fields
        protected DB _db = null;
        #endregion


        #region Constructors
        public BaseLevelDB(string path)
        {
            _db = DB.Open(path, new Options { CreateIfMissing = true });
        }
        #endregion


        #region Properties
        public WriteOptions WriteOption { get; set; } = WriteOptions.Default;
        public ReadOptions ReadOption { get; set; } = ReadOptions.Default;
        #endregion


        #region External Method
        public void Put(Slice key, Slice value)
        {
            Put(WriteOption, key, value);
        }

        public void Put(WriteOptions option, Slice key, Slice value)
        {
            _db.Put(option, key, value);
        }

        public Slice Get(Slice key)
        {
            return Get(ReadOption, key);
        }

        public Slice Get(ReadOptions option, Slice key)
        {
            return _db.Get(option, key);
        }

        public bool TryGet(Slice key, out Slice value)
        {
            return TryGet(ReadOption, key, out value);
        }

        public bool TryGet(ReadOptions option, Slice key, out Slice value)
        {
            return _db.TryGet(option, key, out value);
        }

        public IEnumerable<T> Find<T>(byte prefix) where T : class, ISerializable, new()
        {
            return Find(ReadOption, SliceBuilder.Begin(prefix), (k, v) => v.ToArray().Serializable<T>());
        }

        public IEnumerable<T> Find<T>(ReadOptions options, byte prefix) where T : class, ISerializable, new()
        {
            return _db.Find(options, SliceBuilder.Begin(prefix), (k, v) => v.ToArray().Serializable<T>());
        }

        public IEnumerable<T> Find<T>(Slice prefix, Func<Slice, Slice, T> resultSelector)
        {
            return Find(ReadOption, prefix, resultSelector);
        }

        public IEnumerable<T> Find<T>(ReadOptions options, Slice prefix, Func<Slice, Slice, T> resultSelector)
        {
            return _db.Find(options, prefix, resultSelector);
        }

        public void Dispose()
        {
            _db.Dispose();
        }
        #endregion
    }
}
