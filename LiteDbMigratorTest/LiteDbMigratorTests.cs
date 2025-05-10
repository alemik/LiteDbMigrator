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

    [Fact]
    public void Migrate_Field_In_Nested_Document()
    {
        // Setup
        var dbPath = "TestNestedDocs.db";
        if (File.Exists(dbPath)) File.Delete(dbPath);

        using (var db = new LiteDatabase(dbPath))
        {
            var col = db.GetCollection("people");
            col.Insert(new BsonDocument
            {
                ["Name"] = "Mario",
                ["Address"] = new BsonDocument
                {
                    ["Street"] = "Via Roma",
                    ["City"] = "Torino"
                }
            });
        }

        // Act - apply migration
        using (var db = new LiteDatabase(dbPath))
        {
            var migrator = new Migrator(db, schemaVersion: 1);
            migrator
                .Collection("people")
                .Document("Address", d =>
                    d.Field("Street", "Via"));

            migrator.Execute();
        }

        // Assert
        using (var db = new LiteDatabase(dbPath))
        {
            var col = db.GetCollection("people");
            var person = col.FindAll().First();

            Assert.False(person["Address"].AsDocument.ContainsKey("Street"));
            Assert.True(person["Address"].AsDocument.ContainsKey("Via"));
            Assert.Equal("Via Roma", person["Address"]["Via"]);
        }

        File.Delete(dbPath);
    }

    [Fact]
    public void Migrate_Field_In_Deeply_Nested_Documents()
    {
        // Setup
        var dbPath = "NestedLevels.db";
        if (File.Exists(dbPath)) File.Delete(dbPath);

        // Inserimento dati
        using (var db = new LiteDatabase(dbPath))
        {
            var col = db.GetCollection("states");
            col.Insert(new BsonDocument
            {
                ["Name"] = "Stato A",
                ["Region"] = new BsonDocument
                {
                    ["Name"] = "Regione 1",
                    ["Province"] = new BsonDocument
                    {
                        ["Name"] = "Provincia 1",
                        ["Municipality"] = new BsonDocument
                        {
                            ["OldField"] = "Da Rinominare"
                        }
                    }
                }
            });
        }

        // Act
        using (var db = new LiteDatabase(dbPath))
        {
            var migrator = new Migrator(db, schemaVersion: 1);

            migrator
                .Collection("states")
                .Document("Region", region =>
                    region.Document("Province", province =>
                        province.Document("Municipality", municipality =>
                            municipality.Field("OldField", "NewField")
                        )
                    )
                );

            migrator.Execute();
        }

        // Assert
        using (var db = new LiteDatabase(dbPath))
        {
            var state = db.GetCollection("states").FindAll().First();
            var region = state["Region"].AsDocument;
            var province = region["Province"].AsDocument;
            var municipality = province["Municipality"].AsDocument;

            Assert.False(municipality.ContainsKey("OldField"));
            Assert.True(municipality.ContainsKey("NewField"));
            Assert.Equal("Da Rinominare", municipality["NewField"]);
        }

        File.Delete(dbPath);
    }


    [Fact]
    public void Migrate_Field_In_Deeply_Nested_Arrays()
    {
        var dbPath = "NestedArrays.db";
        if (File.Exists(dbPath)) File.Delete(dbPath);

        // Inserimento iniziale
        using (var db = new LiteDatabase(dbPath))
        {
            var col = db.GetCollection("states");
            col.Insert(new BsonDocument
            {
                ["Name"] = "Stato A",
                ["Regions"] = new BsonArray
            {
                new BsonDocument
                {
                    ["Name"] = "Regione 1",
                    ["Provinces"] = new BsonArray
                    {
                        new BsonDocument
                        {
                            ["Name"] = "Provincia 1",
                            ["Municipalities"] = new BsonArray
                            {
                                new BsonDocument
                                {
                                    ["OldField"] = "Da Rinominare"
                                }
                            }
                        }
                    }
                }
            }
            });
        }

        // Migrazione
        using (var db = new LiteDatabase(dbPath))
        {
            var migrator = new Migrator(db, schemaVersion: 1);

            migrator
                .Collection("states")
                .Array("Regions", region =>
                    region.Array("Provinces", province =>
                        province.Array("Municipalities", municipality =>
                            municipality.Field("OldField", "NewField")
                        )
                    )
                );

            migrator.Execute();
        }

        // Verifica
        using (var db = new LiteDatabase(dbPath))
        {
            var state = db.GetCollection("states").FindAll().First();
            var region = state["Regions"].AsArray[0].AsDocument;
            var province = region["Provinces"].AsArray[0].AsDocument;
            var municipality = province["Municipalities"].AsArray[0].AsDocument;

            Assert.False(municipality.ContainsKey("OldField"));
            Assert.True(municipality.ContainsKey("NewField"));
            Assert.Equal("Da Rinominare", municipality["NewField"]);
        }

        File.Delete(dbPath);
    }


    [Fact]
    public void Migrate_Field_In_Document_Inside_Document_Inside_Array()
    {
        var dbPath = "DocumentInDocInArray.db";
        if (File.Exists(dbPath)) File.Delete(dbPath);

        // Inserimento iniziale
        using (var db = new LiteDatabase(dbPath))
        {
            var col = db.GetCollection("roots");
            col.Insert(new BsonDocument
            {
                ["Name"] = "Root",
                ["Items"] = new BsonArray
            {
                new BsonDocument
                {
                    ["Metadata"] = new BsonDocument
                    {
                        ["OldKey"] = "Valore da migrare"
                    }
                }
            }
            });
        }

        // Migrazione
        using (var db = new LiteDatabase(dbPath))
        {
            var migrator = new Migrator(db, schemaVersion: 1);

            migrator
                .Collection("roots")
                .Array("Items", item =>
                    item.Document("Metadata", meta =>
                        meta.Field("OldKey", "NewKey")
                    )
                );

            migrator.Execute();
        }

        // Verifica
        using (var db = new LiteDatabase(dbPath))
        {
            var root = db.GetCollection("roots").FindAll().First();
            var item = root["Items"].AsArray[0].AsDocument;
            var metadata = item["Metadata"].AsDocument;

            Assert.False(metadata.ContainsKey("OldKey"));
            Assert.True(metadata.ContainsKey("NewKey"));
            Assert.Equal("Valore da migrare", metadata["NewKey"]);
        }

        File.Delete(dbPath);
    }

    [Fact]
    public void Test_MigrationV1_RenamesFields()
    {
        var dbPath = "Test.db";
        if (File.Exists(dbPath)) File.Delete(dbPath);

        // Setup: crea il database iniziale
        using (var db = new LiteDatabase(dbPath))
        {
            var col = db.GetCollection("people");
            col.Insert(new BsonDocument
            {
                ["_id"] = 1,
                ["first_name"] = "Alice",
                ["last_name"] = "Smith",
                ["age"] = 30
            });

            db.Pragma("USER_VERSION", 0);
        }

        // Esegue la migrazione usando Apply<T>()
        using (var db = new LiteDatabase(dbPath))
        {
            var migrator = new Migrator(db, 2);
            migrator
                .Apply<MigrationV1>()
                .Apply<MigrationV2>()
                .Execute();
        }

        // Verifica
        using (var db = new LiteDatabase(dbPath))
        {
            var col = db.GetCollection("people");
            var person = col.FindById(1);

            Assert.True(person.ContainsKey("Nome"));
            Assert.True(person.ContainsKey("Cognome"));
            Assert.False(person.ContainsKey("first_name"));
            Assert.False(person.ContainsKey("last_name"));
            Assert.Equal(30, person["age"].AsInt32);

            Assert.Equal(2, db.Pragma("USER_VERSION").AsInt32);
        }

        File.Delete(dbPath);
    }
}
