// See https://aka.ms/new-console-template for more information
using LiteDB;
using LiteDbMigrator;
using ManualTest;


var liteMapper = new BsonMapper();

var schema = new Schema(4);


var travelSchema = new MigrationSchema<Travel>("travels")
    .Add("Arrival", "Arrival", 1)
    .Add("Arrival", "SDate", 2)
    .Add("SDate", "StartDate", 3);


schema.AddMigration(travelSchema);

var x = schema.GetMigration<Travel>();

try
{
    using var db = new LiteDB.LiteDatabase(DbSettings.DbPath);

    //var migrator = new Migrator(db)
    //    .Collection("travels")
    //    .RenameField("Arrival", "SDate")
    //    .ForEachInArray("Places", sub => sub.RenameField("Arrival", "StartDate"))
    //    .ForEachInArray("Places", sub => sub.RenameField("Departure", "EndDate"));
    //migrator.Execute(); 


    liteMapper.Entity<Travel>()
        .Id(x => x.Id)
        .Field(x => x.Name, "Name")
        .Field(x => x.Description, "Description")
        .Field(x => x.DefaultMedia, "DefaultMedia")
        .Field(x => x.StartDate, "StartDate")
        .Field(x => x.EndDate, "EndDate")
        .Field(x => x.Rate, "Rate")
        .Field(x => x.IsFavorite, "IsFavorite")
        .Field(x => x.IsActual, "IsActual")
        .Field(x => x.Companions, "Companions")
        .Field(x => x.Notes, "Notes")
        .Field(x => x.Gallery, "Gallery")
        .Field(x => x.Places, "Places");

    liteMapper.Entity<Place>()
        .Id(x => x.Id)
        .Field(x => x.Name, "Name")
        .Field(x => x.Description, "Description")
        .Field(x => x.Arrival, "Arrival")
        .Field(x => x.Departure, "Departure")
        .Field(x => x.Notes, "Notes")
        .Field(x => x.Rating, "Rating")
        .Field(x => x.IsFavorite, "IsFavorite")
        .Field(x => x.DefaultMedia, "DefaultMedia")
        .Field(x => x.Companions, "Companions")
        .Field(x => x.Gallery, "Gallery")
        .Ignore(x => x.ArrivalTime)
        .Ignore(x => x.DepartureTime);

    liteMapper.Entity<Media>()
        .Id(x => x.Id)
        .Field(x => x.Name, nameof(Media.Name))
        .Field(x => x.Path, nameof(Media.Path))
        .Field(x => x.FullPath, nameof(Media.FullPath))
        .Field(x => x.Description, nameof(Media.Description))
        .Field(x => x.Location, nameof(Media.Location))
        .Field(x => x.GeoLocation, nameof(Media.GeoLocation))
        .Field(x => x.Date, nameof(Media.Date))
        .Field(x => x.IsFavorite, nameof(Media.IsFavorite))
        .Field(x => x.MediaType, nameof(Media.MediaType));

    Console.WriteLine("Test pass");
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    Console.WriteLine("Test failed");
}

