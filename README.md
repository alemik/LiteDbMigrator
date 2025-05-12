# LiteDbMigrator - Usage Guide

The `LiteDbMigrator` provides a fluent API for managing field and collection renaming in LiteDB documents, including nested structures like embedded documents and arrays.  
Each migration is associated with a schema version that is stored in the database using the `USER_VERSION` pragma.

## Schema Versioning

You must provide the target schema version when initializing the migrator. The system checks if the current version is outdated and, if so, applies the migration logic and updates the version.

```csharp
using (var db = new LiteDatabase("mydb.db"))
{
    var migrator = new Migrator(db, schemaVersion: 2);
    
    // define collections and migrations here
    
    migrator.Execute();
}
```

## Example Scenario
Imagine a collection people with the following structure:

```json
{
  "FirstName": "John",
  "LastName": "Doe",
  "Age": 30,
  "Gender": "M",
  "Address": {
    "Street": "Main St",
    "City": "New York",
    "Zip": "10001",
    "Country": "USA"
  },
  "Contacts": [
    { "Type": "email", "Value": "john@example.com", "Verified": true, "Primary": true },
    { "Type": "phone", "Value": "1234567890", "Verified": false, "Primary": false }
  ]
}
```

## Migration Goals
FirstName → no change

Rename LastName → FamilyName

In Address: rename Street → Road, Zip → PostalCode

In Contacts[]: rename Type → ContactType, Value → ContactValue

```csharp
using (var db = new LiteDatabase("mydb.db"))
{
    var migrator = new Migrator(db, schemaVersion: 2);

    migrator
        .Collection("people")
            .Field("FirstName") // no change, useful for documentation
            .Field("LastName", "FamilyName")
            .Document("Address", address => 
                address
                    .Field("Street", "Road")
                    .Field("Zip", "PostalCode")
            )
            .Array("Contacts", contact => 
                contact
                    .Field("Type", "ContactType")
                    .Field("Value", "ContactValue")
            );

    migrator.Execute();
}
```

After migration, each document in the people collection will be updated with the new field names while leaving untouched fields intact.


## Migrating Multiple Collections and Renaming

You can define migrations for multiple collections in a single migration step.

You can also rename a collection by passing the new name as the second argument to .Collection().

```csharp
using (var db = new LiteDatabase("mydb.db"))
{
    var migrator = new Migrator(db, schemaVersion: 3);

    migrator
        .Collection("people", "users")
            .Field("FirstName", "GivenName");

    migrator
        .Collection("orders", "purchases")
            .Field("TotalAmount", "Amount")
            .Document("Shipping", ship => 
                ship.Field("ZipCode", "PostalCode")
            );

    migrator.Execute();
}
```

## Manually Setting the User Version
In some scenarios, you may want to manually set the database version (e.g., after a manual migration or when initializing a fresh schema):
```csharp
using (var db = new LiteDatabase("mydb.db"))
{
    var migrator = new Migrator(db, schemaVersion: 1);
    migrator.SetDbVersion(2); // sets USER_VERSION to 2 explicitly
}
```

## Using Migrations with Versioning

To define a database schema migration, implement the IMigration interface:
```csharp
public class MigrationV1 : IMigration
{
    public int Version => 1;

    public void Define(Migrator migrator)
    {
        migrator.Collection("Travels")
            .Field("OldName", "NewName");

        migrator.Collection("Places")
            .Field("OldLocation", "NewLocation");
    }
}
```

```csharp
public class MigrationV2 : IMigration
{
    public int Version => 2;

    public void Define(Migrator migrator)
    {
        migrator.Collection("Travels")
            .Field("NewName", "Name");

        migrator.Collection("Places")
            .Field("NewLocation", "Location");
    }
}
```

You can then apply one or more migrations like this:
```csharp
using var db = new LiteDatabase(dbPath);

new Migrator(db, latestVersion)
    .Apply<MigrationV1>()
    .Apply<MigrationV2>() // each migration must have a higher Version
    .Execute();
```

Each migration is versioned and only applied if its version is greater than the current database version (tracked in USER_VERSION). This allows incremental, deterministic upgrades.

✅ Advantages of this approach
You can deploy updated application versions without worrying about the client's current database structure.

Migrations are declarative, modular, and version-aware.

The Migrator ensures that migrations are applied only once, in version order.

You can update multiple collections, nested documents, and arrays.

Existing distributed applications will upgrade their local database schema automatically on startup.

This strategy is particularly useful for apps using LiteDB locally, such as mobile or desktop applications.



## ⚠️ Important Notes
The migration is only applied if the database version is older than the target schema version.

Nested documents and arrays are handled recursively using .Document() and .Array().

If the newName parameter is omitted, the field name remains unchanged.

## Disclaimer:
This library is currently under development. 

Use at your own risk and always ensure a backup of your database before use. The authors disclaim any liability for data loss or damage.