using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using losol.EventManagement.Domain;

namespace losol.EventManagement.Infrastructure
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }




        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<EventInfo> EventInfos { get; set; }
        public DbSet<Registration> Registrations { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }

		public void DetachAllEntities()
		{
			var changedEntriesCopy = this.ChangeTracker.Entries()
				.Where(e => e.State == EntityState.Added ||
							e.State == EntityState.Modified ||
			           		e.State == EntityState.Unchanged ||
							e.State == EntityState.Deleted)
				.ToList();
			foreach (var entity in changedEntriesCopy)
			{
				this.Entry(entity.Entity).State = EntityState.Detached;
			}
		}
    }
}
