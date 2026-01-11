using Bank.Context.Common;
using Bank.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Bank.Context
{
    public class GoalContext : DbContext
    {
        public DbSet<Goal> Goals { get; set; }
        public GoalContext() { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseMySql(Config.ConnectionCofig, Config.Version);
    }
}
