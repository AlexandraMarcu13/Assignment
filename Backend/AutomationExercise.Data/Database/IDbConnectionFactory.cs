using Microsoft.Data.SqlClient;
namespace AutomationExercise.Data.Database
{
    public interface IDbConnectionFactory
    {
        SqlConnection CreateConnection();
    }
}
