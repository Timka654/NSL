using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

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

    public interface DBDataEntity {
        IEnumerable GetData();
    }
}
