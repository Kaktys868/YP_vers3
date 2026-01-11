using Bank.Context.Common;
using Bank.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Bank.Context
{
    public class UserContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public UserContext() { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseMySql(Config.ConnectionCofig, Config.Version);
    }
}
