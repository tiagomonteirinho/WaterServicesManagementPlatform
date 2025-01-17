﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WaterServices_WebApp.Data.Entities;
using WaterServices_WebApp.Models;

namespace WaterServices_WebApp.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        public DbSet<Meter> Meters { get; set; }

        public DbSet<Consumption> Consumptions { get; set; }

        public DbSet<Invoice> Invoices { get; set; }

        public DbSet<Tier> Tiers { get; set; }

        public DbSet<TierUsage> TierUsages { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Consumption>()
                .HasOne(c => c.Meter)
                .WithMany(m => m.Consumptions)
                .HasForeignKey(c => c.MeterId)
                .OnDelete(DeleteBehavior.Restrict);  // Restrict deletion of meters with consumptions.

            modelBuilder.Entity<Consumption>()
                .Property(c => c.Date)
                .HasColumnType("date"); // Store consumption date without time.

            modelBuilder.Ignore<TierViewModel>(); // Prevent table creation.
        }
    }
}