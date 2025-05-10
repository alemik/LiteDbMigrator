namespace LiteDbMigrator
{
    public static class MigratorExtensions
    {
        public static Migrator Apply<T>(this Migrator migrator) where T : IMigration, new()
        {
            var instance = new T();

            if (instance.Version <= migrator.CurrentDbVersion)
            {
                return migrator;
            }
            
            instance.Define(migrator);
            
            return migrator;
        }
    }
}
