using Bank.Context.Common;
using Bank.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Bank.Context
{
    public class TransactionContext : DbContext
    {
        public DbSet<Transaction> Transactions { get; set; }
        public TransactionContext() { }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseMySql(Config.ConnectionCofig, Config.Version);
    }
}
