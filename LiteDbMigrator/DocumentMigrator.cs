using LiteDB;
using System;
using System.Linq;

namespace LiteDbMigrator
{
    public class DocumentMigrator
    {
        private readonly BsonDocument _doc;

        public DocumentMigrator(BsonDocument doc)
        {
            _doc = doc;
        }

        public DocumentMigrator Field(string oldName, string newName = null, Func<BsonValue, BsonValue> converter = null)
        {
            var migration = new FieldMigrator(oldName, newName, converter);
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
}
