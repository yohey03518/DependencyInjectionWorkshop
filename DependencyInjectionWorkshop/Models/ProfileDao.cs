using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Models
{
    public class ProfileDao
    {
        public string GetPasswordFromDatabase(string accountId)
        {
            string pwdInDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                pwdInDb = connection.Query<string>("spGetUserPassword", new { Id = accountId },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return pwdInDb;
        }
    }
}