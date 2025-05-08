using LiteDB;
using LiteDbMigrator;

namespace ManualTest;
internal static class MigratorTest
{
    public static void Migration(LiteDatabase db)
    {
        var x = new Migrator(db);

        var migrator = new Migrator(db);
        migrator
            .Collection("travels","Viaggi")
            .Field("Name")
            .Field("Description")
            .Field("DefaultMedia")
            .Field("StartDate")
            .Field("EndDate")
            .Field("Rate")
            .Field("IsFavorite")
            .Field("IsActual")
            .Field("Companions")
            .Field("Notes")
            .Field("Gallery")
            .Array("Gallery", media =>
                media
                    .Field("Name","Nome")
                    .Field("Path")
                    .Field("FullPath")
                    .Field("Description")
                    .Field("Location")
                    .Field("GeoLocation")
                    .Field("Date")
                    .Field("IsFavorite")
                    .Field("MediaType")
            )
            .Field("Places")
            .Array("Places", places =>
                places
                    .Field("Name")
                    .Field("Description")
                    .Field("Arrival")
                    .Field("Departure")
                    .Field("Notes")
                    .Field("Rating")
                    .Field("IsFavorite")
                    .Field("DefaultMedia")
                    .Field("Companions")
                    .Field("Gallery")
                    .Array("Gallery", media =>
                        media
                            .Field("Name")
                            .Field("Path")
                            .Field("FullPath")
                            .Field("Description")
                            .Field("Location")
                            .Field("GeoLocation")
                            .Field("Date")
                            .Field("IsFavorite")
                            .Field("MediaType")
                    )
            )
            .Execute();
    }
}
