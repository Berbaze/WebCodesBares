using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using WebCodesBares.Data.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
namespace WebCodesBares.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {
        }

        // Ajoute tes tables ici
        public DbSet<Kunden> Kunden { get; set; }
        public DbSet<Barcodes> barcodes { get; set; }
        public DbSet<Mitarbeiter> Mitarbeiter { get; set; }
        public DbSet<Commande> Commande { get; set; }
        public DbSet<Produit> Produit { get; set; }
        public DbSet<CommandeProduit> CommandeProduit { get; set; }
        public DbSet<Licence> Licence { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }





        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration de la relation entre Commande et Clients via ClientId
            modelBuilder.Entity<Commande>()
       .HasOne(c => c.Client)
       .WithMany()
       .HasForeignKey(c => c.ClientId)
       .OnDelete(DeleteBehavior.Restrict);

            // Clé composite pour CommandeProduit
            modelBuilder.Entity<CommandeProduit>()
                .HasKey(cp => new { cp.Id_Commande, cp.Id_Produit });

            modelBuilder.Entity<CommandeProduit>()
                .HasOne(cp => cp.Commande)
                .WithMany(c => c.CommandeProduits)
                .HasForeignKey(cp => cp.Id_Commande)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CommandeProduit>()
                .HasOne(cp => cp.Produit)
                .WithMany(p => p.CommandeProduits)
                .HasForeignKey(cp => cp.Id_Produit)
                .OnDelete(DeleteBehavior.Cascade);

      modelBuilder.Entity<Licence>()
     .HasOne(l => l.Utilisateur)
     .WithMany()  // Si IdentityUser n'a pas de liste de licences, sinon .WithMany(u => u.Licences)
     .HasForeignKey(l => l.UserId)
     .OnDelete(DeleteBehavior.Cascade);
        }
     
    }
}

