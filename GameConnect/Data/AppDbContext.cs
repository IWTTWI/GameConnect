using GameConnect.Models;
using Microsoft.EntityFrameworkCore;  // ← ЭТА СТРОКА ВАЖНА!
using System.Collections.Generic;
using System.Reflection.Emit;

namespace GameConnect.Data{
    public class AppDbContext : DbContext{
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<SocialLink> SocialLinks { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder){
            base.OnModelCreating(modelBuilder);}}}