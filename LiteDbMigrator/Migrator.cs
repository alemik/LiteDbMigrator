using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteDbMigrator
{
    public class Migrator
    {
        private readonly LiteDatabase _db;
        private readonly int schemaVersion;
        private readonly List<CollectionMigrator> _collections = new List<CollectionMigrator>();

        public int CurrentDbVersion => GetDbVersion(); 

        public Migrator(LiteDatabase db, int schemaVersion)
        {
            _db = db;
            this.schemaVersion = schemaVersion;
        }

        public CollectionMigrator Collection(string name, string newName = null)
        {
            var collectionMigrator = new CollectionMigrator(_db, name, newName);
            _collections.Add(collectionMigrator);
            return collectionMigrator;
        }

        public void SetDbVersion(int schemaVersion)
        {
            _db.Pragma("USER_VERSION", schemaVersion);
        }

        private int GetDbVersion()
        {
            return (int)_db.Pragma("USER_VERSION").RawValue;
        }

        public Migrator Apply<T>() where T : IMigration, new()
        {
            var instance = new T();

            if (instance.Version <= CurrentDbVersion)
            {
                return this;
            }

            instance.Define(this);
            return this;
        }

        public void Execute()
        {
            if (schemaVersion <= CurrentDbVersion) throw new Exception("Actual database version is newer than the one being applied");

            foreach (var collection in _collections)
            {
                collection.Execute();
            }

            SetDbVersion(schemaVersion);
        }
    }

    /*
    public class CollectionMigrator
    {
        private readonly LiteDatabase _db;
        private string _oldName;
        private string _newName;
        private readonly List<Action<BsonDocument>> _migrations = new List<Action<BsonDocument>>();
        private readonly List<FieldMigration> _fieldMigrations = new List<FieldMigration>();

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
            _fieldMigrations.Add(new FieldMigration(oldName, newName, converter));
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

    public class DocumentMigrator
    {
        private readonly BsonDocument _doc;

        public DocumentMigrator(BsonDocument doc)
        {
            _doc = doc;
        }

        public DocumentMigrator Field(string oldName, string newName = null, Func<BsonValue, BsonValue> converter = null)
        {
            var migration = new FieldMigration(oldName, newName, converter);
            migration.Apply(_doc);
            return this;
        }

        public DocumentMigrator Field(string oldName, string newName = null)
        {
            if (newName != null && _doc.ContainsKey(oldName))
            {
                _doc[newName] = _doc[oldName];
                _doc.Remove(oldName);
            }
            return this;
        }

        public DocumentMigrator Document(string fieldName, Action<DocumentMigrator> action)
        {
            if (_doc.ContainsKey(fieldName) && _doc[fieldName] is BsonDocument subDoc)
            {
                var migrator = new DocumentMigrator(subDoc);
                action(migrator);
            }
            return this;
        }

        public DocumentMigrator Array(string arrayName, Action<DocumentMigrator> itemAction)
        {
            if (_doc[arrayName] is BsonArray array)
            {
                foreach (var item in array.OfType<BsonDocument>())
                {
                    var migrator = new DocumentMigrator(item);
                    itemAction(migrator);
                }
            }
            return this;
        }
    }
    */

    /*
    public class FieldMigrator
    {
        public string OldName { get; }
        public string NewName { get; }
        public Func<BsonValue, BsonValue> Converter { get; }

        public FieldMigrator(string oldName, string newName, Func<BsonValue, BsonValue> converter = null)
        {
            OldName = oldName;
            NewName = newName ?? oldName;
            Converter = converter;
        }

        public void Apply(BsonDocument doc)
        {
            if (doc.ContainsKey(OldName))
            {
                var value = doc[OldName];
                if (Converter != null)
                    value = Converter(value);

                doc[NewName] = value;
                if (OldName != NewName)
                    doc.Remove(OldName);
            }
        }
    }
    */

    /*
    public static class FieldConverters
    {
        public static readonly Func<BsonValue, BsonValue> FromIntToDecimal = bv =>
        {
            if (bv.IsInt32) return new BsonValue(Convert.ToDecimal(bv.AsInt32));
            if (bv.IsDecimal) return bv;
            return new BsonValue(0.0m); // fallback
        };

        public static readonly Func<BsonValue, BsonValue> FromDecimalToInt = bv =>
        {
            if (bv.IsDecimal) return new BsonValue((int)Math.Round(bv.AsDecimal));
            if (bv.IsInt32) return bv;
            return new BsonValue(0); // fallback
        };

        public static readonly Func<BsonValue, BsonValue> ToString = bv =>
        {
            var val = bv.RawValue.ToString();
            return val;
        };
    }
    */
}
