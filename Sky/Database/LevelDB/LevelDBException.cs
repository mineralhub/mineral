using System.Data.Common;

namespace Sky.Database.LevelDB
{
    internal class LevelDBException : DbException
    {
        internal LevelDBException(string message)
            : base(message)
        {
        }
    }
}
