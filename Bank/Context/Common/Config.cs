using Microsoft.EntityFrameworkCore;

namespace Bank.Context.Common
{
    public class Config
    {
        public static string ConnectionCofig = "server=127.0.0.1;port=3307;uid=root;database=piggybank;";
        public static MySqlServerVersion Version = new MySqlServerVersion(new Version(8, 0, 11));
    }
}
