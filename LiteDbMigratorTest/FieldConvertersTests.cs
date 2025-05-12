using LiteDB;
using LiteDbMigrator;

namespace LiteDbMigratorTest;

public class FieldConvertersTests
{
    [Fact]
    public void FieldMigration_FromDecimalToInt_UpdatesDocumentsCorrectly()
    {
        using var db = new LiteDatabase(new MemoryStream());

        var col = db.GetCollection("Travels");
        col.Insert(new BsonDocument { ["_id"] = 1, ["Rate"] = 4.7m });
        col.Insert(new BsonDocument { ["_id"] = 2, ["Rate"] = 2.1m });

        var migrator = new Migrator(db,2);
        migrator.Collection("Travels")
                .Field("Rate", "Rating", FieldConverters.FromDecimalToInt);
        migrator.Execute();

        var updatedCol = db.GetCollection("Travels");
        var docs = updatedCol.FindAll().ToList();

        Assert.All(docs, doc =>
        {
            Assert.False(doc.ContainsKey("Rate"));
            Assert.True(doc.ContainsKey("Rating"));
            Assert.True(doc["Rating"].IsInt32);
        });

        Assert.Equal(5, docs[0]["Rating"].AsInt32); // 4.7 → 5
        Assert.Equal(2, docs[1]["Rating"].AsInt32); // 2.1 → 2
    }
}
