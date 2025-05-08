using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteDbMigrator
{
    public class Schema
    {
        public int SchemaVersion { get;}

        private Dictionary<Type, IMigrationSchema> _schemes = new Dictionary<Type, IMigrationSchema>();

        public Schema(int schemaVersion)
        {
            SchemaVersion = schemaVersion;
        }

        public bool NeedUpdate(LiteDatabase db)
        {
            var version = (int)db.Pragma("USER_VERSION").RawValue;
            if (version < SchemaVersion) return true;
            return false;        
        }

        public void Update(LiteDatabase db)
        {
            if (!NeedUpdate(db)) return;

            foreach (var item in _schemes)
            {
                var schema = item.Value.CollectionName;
                updateCollection(db, schema);
            }
        }

        public void AddMigration<T>(MigrationSchema<T> migrationSchema)
        {
            _schemes.Add(typeof(T), migrationSchema);
        }

        public MigrationSchema<T> GetMigration<T>()
        {
            var migration = _schemes.Single(x => x.Key == typeof(T));
            return (MigrationSchema<T>)migration.Value;
        }

        public string Get<T>(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("name can not be empty", nameof(name));

            var migration = GetMigration<T>();
            if (migration == null) return name;

            return migration.GetLast(name);
        }


        private void updateCollection(LiteDatabase db, string collectionName)
        {
            var migrator = new Migrator(db)
                .Collection(collectionName)
                .Field("Arrival", "SDate")
                .Array("Places", sub => sub.Field("Arrival", "StartDate"))
                .Array("Places", sub => sub.Field("Departure", "EndDate"));
            migrator.Execute();
        }
    }
}
