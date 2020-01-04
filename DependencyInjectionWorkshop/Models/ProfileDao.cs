using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Models
{
    public interface IProfile
    {
        string GetPasswordFromDatabase(string accountId);
    }

    public class ProfileDao : IProfile
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