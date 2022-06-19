using LostAndFound.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace LostAndFound.Data.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<UserPost> UserPosts { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<SimilarImage> SimilarImages { get; set; }



        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
