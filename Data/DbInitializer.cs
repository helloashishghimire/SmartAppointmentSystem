namespace SmartAppointments.App.Data
{
    public static class DbInitializer
    {
        public static void EnsureDatabase()
        {
            using var db = new AppDbContext();
            // For Update 1, EnsureCreated is enough (no migrations needed)
            db.Database.EnsureCreated();
        }
    }
}
