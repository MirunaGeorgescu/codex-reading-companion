﻿using Codex.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Codex.Data
{
    // The ApplivationDbContext is responsible for connecting the .NET application to the database
    // DbContext is a class provided by EntityFramework and it represents a session with the database
    // and allows me to query and save instances of entity classes
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Genre> Genres { get; set;  }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Shelf> Shelves { get; set; }
        public DbSet<BookOnShelf> BooksOnShelves { get; set;}
        public DbSet<ReadingBadge> ReadingBadges { get; set; }
        public DbSet<BadgeEarned> BadgesEarned { get; set; }
        public DbSet<ReadingChallenge> ReadingChallenges { get; set; }
        public DbSet<BuddyRead> BuddyReads { get; set; }
        public DbSet<Annotation> Annotations { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // MANY-TO-MANY RELATIONSHIPS
            // BOOKS-SHELVES
            // primary key made up of BookId and ShelfId
            modelBuilder.Entity<BookOnShelf>()
                .HasKey(bs => new { bs.BookId, bs.ShelfId });

            // relationship between books and shelves
            modelBuilder.Entity<BookOnShelf>()
                .HasOne(bs => bs.Book)
                .WithMany(b => b.BooksOnShelves)
                .HasForeignKey(bs => bs.BookId);

            modelBuilder.Entity<BookOnShelf>()
                .HasOne(bs => bs.Shelf)
                .WithMany(s => s.BooksOnShelves)
                .HasForeignKey(bs => bs.ShelfId);

            // USERS-BADGES
            // primary key made up of UserId and BadgeId
            modelBuilder.Entity<BadgeEarned>()
                .HasKey(be => new { be.UserId, be.BadgeId });

            // relationship between users and badges
            modelBuilder.Entity<BadgeEarned>()
                 .HasOne(be => be.User)
                 .WithMany(u => u.BadgesEarned)
                 .HasForeignKey(be => be.UserId);

            modelBuilder.Entity<BadgeEarned>()
                .HasOne(be => be.Badge)
                .WithMany(b => b.BadgesEarned)
                .HasForeignKey(be => be.BadgeId);

        }

    }
}
