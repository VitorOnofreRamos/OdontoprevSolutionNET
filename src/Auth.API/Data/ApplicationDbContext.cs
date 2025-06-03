using Auth.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContext<ApplicationDbContext> options) : base(options){}

    public DbSet<User> Users {get; set;} 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("GS_users");
            
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Email)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.Phone)
                .HasColumnName("phone")
                .HasMaxLength(20);

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(255);

            entity.Property(e => e.Role)
                .HasColumnName("role")
                .HasMaxLength(20);

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasMaxLength(1)
                .HasDefaultValue("Y");

            entity.Property(e => e.LastLogin)
                .HasColumnName("last_login");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at");

            entity.Property(e => e.OrganizationId)
                .HasColumnName("organization_id");
                
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}