using LiteDB;
using LiteDbMigrator;

namespace LiteDbMigratorTest;
public class LiteDbMigratorTests
{
    [Fact]
    public void Should_Rename_Field_In_Document_And_SubDocuments()
    {
        // Arrange
        using var db = new LiteDatabase("Filename=:memory:");
        var col = db.GetCollection("Persone");

        var persona = new BsonDocument
        {
            ["Nome"] = "Mario",
            ["Indirizzi"] = new BsonArray
            {
                new BsonDocument { ["Via"] = "Via Roma", ["CAP"] = "10100" },
                new BsonDocument { ["Via"] = "Via Milano", ["CAP"] = "20100" }
            }
        };

        col.Insert(persona);

        // Act
        var migrator = new Migrator(db, 10);
        migrator
            .Collection("Persone")
            .Field("Indirizzi", "Addresses")
            .Array("Addresses", sub =>
                sub.Field("Via", "Street")
                   .Field("CAP", "PostalCode")
            );

           migrator.Execute();

        // Assert
        var updated = col.FindAll().First();

        Assert.False(updated.ContainsKey("Indirizzi"));
        Assert.True(updated.ContainsKey("Addresses"));

        var addresses = updated["Addresses"].AsArray;
        Assert.All(addresses, addr =>
        {
            Assert.True(addr.AsDocument.ContainsKey("Street"));
            Assert.True(addr.AsDocument.ContainsKey("PostalCode"));
            Assert.False(addr.AsDocument.ContainsKey("Via"));
            Assert.False(addr.AsDocument.ContainsKey("CAP"));
        });
    }

    [Fact]
    public void Should_Rename_Fields_For_Multiple_Persons()
    {
        // Arrange
        using var db = new LiteDatabase("Filename=:memory:");
        var col = db.GetCollection("Persone");

        var persone = new[]
        {
            new BsonDocument
            {
                ["Nome"] = "Mario",
                ["Indirizzi"] = new BsonArray
                {
                    new BsonDocument { ["Via"] = "Via Roma", ["CAP"] = "10100" },
                    new BsonDocument { ["Via"] = "Via Milano", ["CAP"] = "20100" }
                }
            },
            new BsonDocument
            {
                ["Nome"] = "Luigi",
                ["Indirizzi"] = new BsonArray
                {
                    new BsonDocument { ["Via"] = "Via Napoli", ["CAP"] = "30100" }
                }
            },
            new BsonDocument
            {
                ["Nome"] = "Giovanni",
                ["Indirizzi"] = new BsonArray
                {
                    new BsonDocument { ["Via"] = "Via Torino", ["CAP"] = "40100" },
                    new BsonDocument { ["Via"] = "Via Bologna", ["CAP"] = "50100" }
                }
            }
        };

        col.InsertBulk(persone);

        // Act
        var migrator = new Migrator(db, 10);
        migrator
            .Collection("Persone")
            .Field("Indirizzi", "Addresses")
            .Array("Addresses", sub =>
                sub.Field("Via", "Street")
                   .Field("CAP", "PostalCode")
            );
        
        migrator.Execute();

        // Assert
        var updatedDocs = col.FindAll().ToList();
        foreach (var doc in updatedDocs)
        {
            Assert.False(doc.ContainsKey("Indirizzi"));
            Assert.True(doc.ContainsKey("Addresses"));

            var addresses = doc["Addresses"].AsArray;
            Assert.All(addresses, addr =>
            {
                Assert.True(addr.AsDocument.ContainsKey("Street"));
                Assert.True(addr.AsDocument.ContainsKey("PostalCode"));
                Assert.False(addr.AsDocument.ContainsKey("Via"));
                Assert.False(addr.AsDocument.ContainsKey("CAP"));
            });
        }
    }

    [Fact]
    public void Should_Rename_Fields_For_Complex_Hierarchy()
    {
        // Arrange
        using var db = new LiteDatabase("Filename=:memory:");
        var col = db.GetCollection("Stati");

        var stato = new BsonDocument
        {
            ["Nome"] = "Italia",
            ["Regioni"] = new BsonArray
            {
                new BsonDocument
                {
                    ["Nome"] = "Lazio",
                    ["Citta"] = new BsonArray
                    {
                        new BsonDocument
                        {
                            ["Nome"] = "Roma",
                            ["Comuni"] = new BsonArray
                            {
                                new BsonDocument
                                {
                                    ["Nome"] = "Centro",
                                    ["Frazioni"] = new BsonArray
                                    {
                                        new BsonDocument { ["Nome"] = "Trastevere" },
                                        new BsonDocument { ["Nome"] = "Testaccio" }
                                    }
                                }
                            }
                        }
                    }
                },
                new BsonDocument
                {
                    ["Nome"] = "Toscana",
                    ["Citta"] = new BsonArray
                    {
                        new BsonDocument
                        {
                            ["Nome"] = "Firenze",
                            ["Comuni"] = new BsonArray
                            {
                                new BsonDocument
                                {
                                    ["Nome"] = "Centro Storico",
                                    ["Frazioni"] = new BsonArray
                                    {
                                        new BsonDocument { ["Nome"] = "Santa Croce" },
                                        new BsonDocument { ["Nome"] = "San Giovanni" }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        col.Insert(stato);

        // Act
        var migrator = new Migrator(db, 10);
        migrator
            .Collection("Stati")
            .Field("Regioni", "Regions")
            .Array("Regions", region =>
                region.Field("Citta", "Cities")
                      .Array("Cities", city =>
                          city.Field("Comuni", "Municipalities")
                              .Array("Municipalities", municipality =>
                                  municipality.Field("Frazioni", "Districts")
                                              .Array("Districts", district =>
                                                  district.Field("Nome", "Name")
                                              )
                              )
                      )
            );
        
        migrator.Execute();

        // Assert
        var updatedDoc = col.FindAll().First();

        Assert.False(updatedDoc.ContainsKey("Regioni"));
        Assert.True(updatedDoc.ContainsKey("Regions"));

        var regions = updatedDoc["Regions"].AsArray;
        foreach (var region in regions)
        {
            Assert.True(region.AsDocument.ContainsKey("Cities"));
            var cities = region["Cities"].AsArray;
            foreach (var city in cities)
            {
                Assert.True(city.AsDocument.ContainsKey("Municipalities"));
                var municipalities = city["Municipalities"].AsArray;
                foreach (var municipality in municipalities)
                {
                    Assert.True(municipality.AsDocument.ContainsKey("Districts"));
                    var districts = municipality["Districts"].AsArray;
                    foreach (var district in districts)
                    {
                        Assert.True(district.AsDocument.ContainsKey("Name"));
                        Assert.False(district.AsDocument.ContainsKey("Nome"));
                    }
                }
            }
        }
    }

    [Fact]
    public void Should_Rename_Collection()
    {
        // Arrange
        using var db = new LiteDatabase("Filename=:memory:");
        var oldCollectionName = "VecchiaCollezione";
        var newCollectionName = "NuovaCollezione";

        var oldCol = db.GetCollection(oldCollectionName);
        oldCol.Insert(new BsonDocument { ["Nome"] = "Documento1" });
        oldCol.Insert(new BsonDocument { ["Nome"] = "Documento2" });

        // Act
        var migrator = new Migrator(db, 10);
        migrator
            .Collection(oldCollectionName, newCollectionName);

        migrator.Execute();

        // Assert
        var newCol = db.GetCollection(newCollectionName);
        var docs = newCol.FindAll().ToList();

        Assert.Equal(2, docs.Count);
        Assert.True(docs.Any(doc => doc["Nome"] == "Documento1"));
        Assert.True(docs.Any(doc => doc["Nome"] == "Documento2"));

        // Verifica che la vecchia collezione sia stata eliminata
        var oldColExists = db.CollectionExists(oldCollectionName);
        Assert.False(oldColExists);
    }
}
