using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteDbMigrator
{
    public class MigratorNew
    {
        private readonly LiteDatabase _db;

        public MigratorNew(LiteDatabase db)
        {
            _db = db;
        }

        public CollectionMigratorNew Collection(string name, string newName = null)
        {
            return new CollectionMigratorNew(_db, name, newName);
        }
    }

    public class CollectionMigratorNew
    {
        private readonly LiteDatabase _db;
        private readonly string _oldName;
        private readonly string _newName;
        private readonly List<Action<BsonDocument>> _migrations = new List<Action<BsonDocument>>();

        public CollectionMigratorNew(LiteDatabase db, string oldName, string newName)
        {
            _db = db;
            _oldName = oldName;
            _newName = newName;
        }

        public CollectionMigratorNew Field(string oldName, string newName = null)
        {
            _migrations.Add(doc =>
            {
                if (newName != null && doc.ContainsKey(oldName))
                {
                    doc[newName] = doc[oldName];
                    doc.Remove(oldName);
                }
            });
            return this;
        }

        public CollectionMigratorNew Array(string arrayName, Action<DocumentMigratorNew> itemAction)
        {
            _migrations.Add(doc =>
            {
                if (doc[arrayName] is BsonArray array)
                {
                    foreach (var item in array.OfType<BsonDocument>())
                    {
                        var migrator = new DocumentMigratorNew(item);
                        itemAction(migrator);
                    }
                }
            });
            return this;
        }

        public void Execute()
        {
            var source = _db.GetCollection(_oldName);
            var target = _db.GetCollection(_newName);

            foreach (var doc in source.FindAll())
            {
                foreach (var migration in _migrations)
                    migration(doc);

                target.Upsert(doc);
            }

            if (_oldName != _newName)
                _db.DropCollection(_oldName);
        }
    }

    public class DocumentMigratorNew
    {
        private readonly BsonDocument _doc;

        public DocumentMigratorNew(BsonDocument doc)
        {
            _doc = doc;
        }

        public DocumentMigratorNew Field(string oldName, string newName = null)
        {
            if (newName != null && _doc.ContainsKey(oldName))
            {
                _doc[newName] = _doc[oldName];
                _doc.Remove(oldName);
            }
            return this;
        }

        public DocumentMigratorNew Array(string arrayName, Action<DocumentMigratorNew> itemAction)
        {
            if (_doc[arrayName] is BsonArray array)
            {
                foreach (var item in array.OfType<BsonDocument>())
                {
                    var migrator = new DocumentMigratorNew(item);
                    itemAction(migrator);
                }
            }
            return this;
        }
    }

}
