namespace ConferenceWebApp.Persistence
{
    public class AppConfig
    {
        public DataBase DataBase { get; set; } = new DataBase();
    }

    public class DataBase
    {
        public string? ConnectionString { get; set; }
    }
}