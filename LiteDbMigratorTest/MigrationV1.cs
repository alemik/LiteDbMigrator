using LiteDbMigrator;

namespace LiteDbMigratorTest;
public class MigrationV1 : IMigration
{
    public int Version => 1;

    public void Define(Migrator m)
    {
        m.Collection("people")
            .Field("first_name", "FirstName")
            .Field("last_name", "LastName");
    }
}
