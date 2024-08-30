using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using App.Models.Contacts;

namespace App.Models
{
    //App.Models.AppDbcontext
    public class AppDbContext :DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            //..
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            // {
            //     string tableName = entityType.GetTableName() ?? "";
            //     if(tableName.StartsWith("AspNet"))
            //     {
            //         entityType.SetTableName(tableName.Substring(6));
            //     }
            // }
        }

        public DbSet<Contact> Contacts {set;get;}

    }
}