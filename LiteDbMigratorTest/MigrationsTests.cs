using LiteDB;
using LiteDbMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteDbMigratorTest;
public class MigrationsTests
{
    public class MigrationV1 : IMigration
    {
        public int Version => 1;

        public void Define(Migrator migrator)
        {
            migrator.Collection("Travels")
                .Field("OldName1", "NewName1");

            migrator.Collection("Places")
                .Field("OldPlaceField", "NewPlaceField");
        }
    }

    public class MigrationV2 : IMigration
    {
        public int Version => 2;

        public void Define(Migrator migrator)
        {
            migrator.Collection("Travels")
                .Field("AnotherOld", "AnotherNew");

            migrator.Collection("People")
                .Field("OldPersonField", "NewPersonField");
        }
    }

    [Fact]
    public void CanApplyMultipleMigrationsWithMultipleCollections()
    {
        using var db = new LiteDatabase(new MemoryStream());

        var travels = db.GetCollection("Travels");
        travels.Insert(new BsonDocument { ["OldName1"] = "Rome", ["AnotherOld"] = "2023" });

        var places = db.GetCollection("Places");
        places.Insert(new BsonDocument { ["OldPlaceField"] = "Colosseum" });

        var people = db.GetCollection("People");
        people.Insert(new BsonDocument { ["OldPersonField"] = "John" });

        var migrator = new Migrator(db, 2);
        migrator.Apply<MigrationV1>()
                .Apply<MigrationV2>()
                .Execute();

        var updatedTravel = travels.FindAll().First();
        Assert.True(updatedTravel.ContainsKey("NewName1"));
        Assert.True(updatedTravel.ContainsKey("AnotherNew"));

        var updatedPlace = places.FindAll().First();
        Assert.True(updatedPlace.ContainsKey("NewPlaceField"));

        var updatedPerson = people.FindAll().First();
        Assert.True(updatedPerson.ContainsKey("NewPersonField"));

        Assert.Equal(2, db.Pragma("USER_VERSION").AsInt32);
    }
}
