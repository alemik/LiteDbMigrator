using LiteDB;
using System;

namespace LiteDbMigrator
{
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
}
