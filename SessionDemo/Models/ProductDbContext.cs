using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SessionDemo.Models;

public partial class ProductDbContext : DbContext
{
    public ProductDbContext()
    {
    }

    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Article> Articles { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("server=DESKTOP-J0P7OTP; database=productDB; trusted_connection=true; trustservercertificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasIndex(e => e.ProductProdId, "IX_Articles_ProductProdID");

            entity.Property(e => e.ProdId).HasColumnName("ProdID");
            entity.Property(e => e.ProductProdId).HasColumnName("ProductProdID");

            entity.HasOne(d => d.ProductProd).WithMany(p => p.Articles).HasForeignKey(d => d.ProductProdId);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProdId);

            entity.Property(e => e.ProdId).HasColumnName("ProdID");
            entity.Property(e => e.Description).HasDefaultValueSql("(N'')");
            entity.Property(e => e.ProdName).HasDefaultValueSql("(N'')");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
