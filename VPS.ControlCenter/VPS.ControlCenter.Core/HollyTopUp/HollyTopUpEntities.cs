using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPS.ControlCenter.Core.HollyTopUp
{
    public class HollyTopUpEntities : DbContext
    {
        private readonly string _connectionString;
        public HollyTopUpEntities()
        {

        }

        public HollyTopUpEntities(DbContextOptions<HollyTopUpEntities> options, IConfiguration configuration) : base(options)
        {
            _connectionString = configuration.GetConnectionString("HollyTopUpEntities");
        }
        public IDbConnection Connection
        {
            get
            {
                var connection = new SqlConnection(_connectionString);
                connection.Open();
                return connection;
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // You can configure additional settings for the table here if needed
        }

        public virtual DbSet<AuditTrail> AuditTrail { get; set; }

        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<BluVoucherLog> BluVoucherLog { get; set; }
        public virtual DbSet<EasyLoadVoucherLog> EasyLoadVoucherLog { get; set; }
        public virtual DbSet<FlashVoucherLog> FlashVoucherLog { get; set; }
        public virtual DbSet<OTTVoucherLog> OTTVoucherLog { get; set; }
        public virtual DbSet<HTUVoucherLog> HTUVoucherLog { get; set; }
        public virtual DbSet<RAVoucherLog> RAVoucherLog { get; set; }

    }
}
