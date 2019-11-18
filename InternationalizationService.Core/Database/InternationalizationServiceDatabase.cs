using System.Data.Common;
using NPoco;
using InternationalizationService.Core.Database.Interface;

namespace InternationalizationService.Core.Database
{
    public class RulesServiceDatabase : NPoco.Database, IInternationalizationServiceDatabase
    {
        public RulesServiceDatabase(DbConnection connection) : base(connection) { }

        public RulesServiceDatabase(string connectionString, DatabaseType databaseType, DbProviderFactory provider)
         : base(connectionString, databaseType, provider)
        {
        }
    }
}
