using Identity.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<VerificationCode> VerificationCodes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<VerificationCode>()
            .HasOne(cc => cc.User)
            .WithMany(x => x.EmailVerifications)
            .HasForeignKey(cc => cc.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        base.OnModelCreating(
            builder);
    }
}