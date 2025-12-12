using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ProjectManager.API.Data;

namespace Dinfo.Test.helpers
{
    class DatabaseHelper
    {
        public static ApplicationDbContext GetDbContext()
        {
            var dbbuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var conf = ConfigurationHelper.InitConfiguration();
            string connection = "Host=localhost;Port=5432;Database=ProjectmanagamentDb;Username=postgres;Password=password;Include Error Detail=true";
            dbbuilder.UseNpgsql(connection);
            dbbuilder.EnableSensitiveDataLogging();
            dbbuilder.LogTo(m => Debug.WriteLine(m));
            var options = dbbuilder.Options;
            return new ApplicationDbContext(options);
        }
    }
}
