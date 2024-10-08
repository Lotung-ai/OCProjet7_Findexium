using Microsoft.EntityFrameworkCore;
using P7CreateRestApi.Domain;
using System.Reflection.Emit;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace P7CreateRestApi.Data
{
    public class LocalDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options) { }

        public DbSet<BidList> Bids { get; set; }
        public DbSet<CurvePoint> CurvePoints { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<RuleName> RuleNames { get; set; }
        public DbSet<Trade> Trades { get; set; }
        public DbSet<User> Users { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<BidList>()
           .HasKey(cp => cp.BidListId);
            builder.Entity<CurvePoint>()
           .HasKey(cp => cp.Id);
            builder.Entity<Rating>()
           .HasKey(cp => cp.Id);
            builder.Entity<RuleName>()
          .HasKey(cp => cp.Id);
            builder.Entity<Trade>()
         .HasKey(cp => cp.TradeId);
            builder.Entity<User>()
         .HasKey(cp => cp.Id);

            // Additional configuration for Identity
            builder.Entity<IdentityUserLogin<int>>().HasKey(l => new { l.LoginProvider, l.ProviderKey });
            builder.Entity<IdentityUserRole<int>>().HasKey(r => new { r.UserId, r.RoleId });
            builder.Entity<IdentityUserToken<int>>().HasKey(t => new { t.UserId, t.LoginProvider, t.Name });
        }
    }
    
}