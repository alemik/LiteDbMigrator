using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteDbMigrator
{
    public class CollectionMigrator
    {
        private readonly LiteDatabase _db;
        private string _oldName;
        private string _newName;
        private readonly List<Action<BsonDocument>> _migrations = new List<Action<BsonDocument>>();
        private readonly List<FieldMigrator> _fieldMigrations = new List<FieldMigrator>();

        public CollectionMigrator(LiteDatabase db, string oldName, string newName)
        {
            _db = db;
            _oldName = oldName;
            _newName = newName;
        }

        private void RenameCollectionInternal()
        {
            var oldCol = _db.GetCollection(_oldName);
            var newCol = _db.GetCollection(_newName);

            newCol.InsertBulk(oldCol.FindAll());

            _db.DropCollection(_oldName);
            _oldName = _newName;
        }

        public CollectionMigrator Field(string oldName, string newName = null, Func<BsonValue, BsonValue> converter = null)
        {
            _fieldMigrations.Add(new FieldMigrator(oldName, newName, converter));
            return this;
        }

        public CollectionMigrator Field(string oldName, string newName = null)
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

        public CollectionMigrator Document(string fieldName, Action<DocumentMigrator> action)
        {
            _migrations.Add(doc =>
            {
                if (doc[fieldName] is BsonDocument subDoc)
                {
                    var migrator = new DocumentMigrator(subDoc);
                    action(migrator);
                }
            });
            return this;
        }

        public CollectionMigrator Array(string arrayName, Action<DocumentMigrator> itemAction)
        {
            _migrations.Add(doc =>
            {
                if (doc[arrayName] is BsonArray array)
                {
                    foreach (var item in array.OfType<BsonDocument>())
                    {
                        var migrator = new DocumentMigrator(item);
                        itemAction(migrator);
                    }
                }
            });
            return this;
        }

        internal void Execute()
        {
            if (_newName != null)
            {
                RenameCollectionInternal();
            }

            var col = _db.GetCollection(_oldName);

            foreach (var doc in col.FindAll())
            {
                foreach (var field in _fieldMigrations)
                    field.Apply(doc);

                foreach (var migration in _migrations)
                    migration(doc);

                col.Upsert(doc);
            }
        }
    }
}
