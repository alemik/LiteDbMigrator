namespace LiteDbMigrator
{

    public interface IMigration
    {
        int Version { get; }
        void Define(Migrator migrator);
    }
}
