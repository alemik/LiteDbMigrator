// See https://aka.ms/new-console-template for more information
using LiteDbMigrator;
using ManualTest;
using System.Diagnostics;

Console.WriteLine("Testing Migrator");

try
{
    using var db = new LiteDB.LiteDatabase(DbSettings.DbPath);

    var migrator = new Migrator(db)
        .Collection("travels")
        .ForEachInArray("Places", sub => sub.RenameField("Arrival", "StartDate"))
        .ForEachInArray("Places", sub => sub.RenameField("Departure", "EndDate"));
    
    migrator.Execute();
}
catch (Exception ex)
{
    Debug.Print(ex.Message);
}