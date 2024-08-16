using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using VPS.ControlCenter.Core.Entities;

namespace VPS.ControlCenter.Core
{
    public class VpsDbContext : DbContext
    {
        public DbSet<VoucherProvider> VoucherProviders { get; set; }
        public DbSet<VoucherType> VoucherTypes { get; set; }
        public DbSet<FeatureToggle> FeatureToggles { get; set; }
        public DbSet<DynamicSetting> DynamicSettings { get; set; }
        public VpsDbContext(DbContextOptions<VpsDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
        public VpsDbContext()
        {
            
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=10.3.104.104;Initial Catalog=HollyTopUp;user id=itsadmin;password=interit_123; trustServerCertificate=true");
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VoucherProvider>()
                .HasOne(vp => vp.VoucherType)
                .WithMany()
                .HasForeignKey(vp => vp.VoucherTypeId);

            PopulateDefault(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        private void PopulateDefault(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VoucherType>().HasData(
      new VoucherType { VoucherTypeId = 1, Name = "HollyTopUp", VoucherLength = "15,17" },
      new VoucherType { VoucherTypeId = 2, Name = "OTT", VoucherLength = "12" },
      new VoucherType { VoucherTypeId = 3, Name = "Flash_OneVoucher", VoucherLength = "16" },
      new VoucherType { VoucherTypeId = 4, Name = "BluVoucher", VoucherLength = "16" },
      new VoucherType { VoucherTypeId = 5, Name = "EasyLoad", VoucherLength = "14" }
  );
        }
    }
}
