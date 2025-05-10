using LiteDbMigrator;

namespace LiteDbMigratorTest;
public class MigrationV2 : IMigration
{
    public int Version => 2;

    public void Define(Migrator m)
    {
        m.Collection("people")
        .Field("FirstName", "Nome")
        .Field("LastName", "Cognome");
    }
}
