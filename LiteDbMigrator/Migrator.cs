using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteDbMigrator
{
    public class Migrator
    {

        private readonly LiteDatabase _db;
        private readonly string _dbPath;
        private string _collectionName;
        private string _newCollectionName;
        private readonly List<Action<BsonDocument>> _migrations = new List<Action<BsonDocument>>();

        public Migrator(string dbPath) => _dbPath = dbPath;


        public Migrator(LiteDatabase db) => _db = db;

        // Metodo per specificare la nuova collezione (dove i dati verranno migrati)
        public Migrator RenameCollection(string newCollectionName)
        {
            _newCollectionName = newCollectionName;
            return this;
        }

        public Migrator Collection(string name)
        {
            _collectionName = name;
            return this;
        }

        // Metodo privato per rinominare una collezione
        private void RenameCollectionInternal()
        {
            var oldCol = _db.GetCollection(_collectionName);
            var newCol = _db.GetCollection(_newCollectionName);

            // Copia i documenti dalla vecchia collezione a quella nuova
            foreach (var doc in oldCol.FindAll())
            {
                //TODO inserire con InsertBulk
                newCol.Insert(doc);
            }

            // Elimina la vecchia collezione
            _db.DropCollection(_collectionName);
            _collectionName = _newCollectionName;
        }

        public Migrator RenameField(string oldName, string newName)
        {
            _migrations.Add(doc =>
            {
                if (doc.ContainsKey(oldName))
                {
                    doc[newName] = doc[oldName];
                    doc.Remove(oldName);
                }
            });
            return this;
        }

        public Migrator ForEachInArray(string arrayField, Action<SubDocumentMigrator> config)
        {
            _migrations.Add(doc =>
            {
                if (doc[arrayField] is BsonArray array)
                {
                    foreach (var item in array.OfType<BsonDocument>())
                    {
                        var migrator = new SubDocumentMigrator(item);
                        config(migrator);
                    }
                }
            });
            return this;
        }

        public void Execute()
        {
            if (_newCollectionName != null)
            {
                // Rinominare la collezione (creando una nuova e copiare i dati)
                RenameCollectionInternal();
            }

            var col = _db.GetCollection(_collectionName);

            var docs = col.FindAll().ToList();
            foreach (var doc in docs)
            {
                foreach (var migration in _migrations)
                    migration(doc);

                col.Update(doc);
            }
        }
    }

    public class SubDocumentMigrator
    {
        private readonly BsonDocument _doc;

        public SubDocumentMigrator(BsonDocument doc) => _doc = doc;

        public SubDocumentMigrator RenameField(string oldName, string newName)
        {
            if (_doc.ContainsKey(oldName))
            {
                _doc[newName] = _doc[oldName];
                _doc.Remove(oldName);
            }
            return this;
        }

        public SubDocumentMigrator ForEachInArray(string fieldName, Action<SubDocumentMigrator> action)
        {
            if (_doc.ContainsKey(fieldName) && _doc[fieldName].IsArray)
            {
                var array = _doc[fieldName].AsArray;
                foreach (var item in array)
                {
                    var subDocMigrator = new SubDocumentMigrator(item.AsDocument);
                    action(subDocMigrator);
                }
            }
            return this;
        }
    }
}
