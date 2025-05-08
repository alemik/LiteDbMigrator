using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteDbMigrator
{
    public class MigrationSchema<T> : IMigrationSchema
    {
        private readonly List<FieldMapping> _mappings = new List<FieldMapping>();

        public string CollectionName { get; }

        public MigrationSchema(string collectionName)
        {
            if (string.IsNullOrWhiteSpace(collectionName))
                throw new ArgumentException("collectionName can not be empty", nameof(collectionName));
            
            CollectionName = collectionName;
        }

        public MigrationSchema<T> Add(string oldName, string newName, int version)
        {
            if (string.IsNullOrWhiteSpace(oldName))
                throw new ArgumentException("oldName can not be empty", nameof(oldName));
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("newName can not be empty", nameof(newName));

            _mappings.Add(new FieldMapping
            {
                OldName = oldName,
                NewName = newName,
                SchemaVersion = version
            });
            return this;
        }

        public string GetLast(string name)
        {
            var m = _mappings
                    .Where(x => x.NewName == name)
                    .OrderByDescending(x => x.SchemaVersion)
                    .FirstOrDefault();

            if (m == null) return name;
            return m.OldName;
        }
    }
}
