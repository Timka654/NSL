using System.Collections;

namespace DBEngine.DBMigrator
{
    public interface DBIdentityEntity<T> : DBIdentityEntity
    {
        T Id { get; set; }
    }

    public interface DBIdentityEntity
    {
        int GetIndex();

        void SetIndex(int index);
    }

    public interface DBDataEntity
    {
        IEnumerable GetData();
    }
}
