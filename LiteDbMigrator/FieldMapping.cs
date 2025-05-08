namespace LiteDbMigrator
{
    public class FieldMapping
    {
        public string OldName { get; set; }
        public string NewName { get; set; }
        public int SchemaVersion { get; set; }
    }
}
