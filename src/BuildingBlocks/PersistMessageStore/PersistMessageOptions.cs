namespace BuildingBlocks.PersistMessageStore
{
    public class PersistMessageOptions
    {
        public string ConnectionString { get; set; }
        public int IntervalSecond { get; set; } = 30;
        public bool Enabled { get; set; } = true;
    }
}
