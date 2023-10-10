using Microsoft.EntityFrameworkCore;

namespace MD_Web_Api.Models
{
    public class MyDbContext: DbContext
    {
        public MyDbContext()
        {
        }

        public MyDbContext(DbContextOptions<MyDbContext> options): base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<OrderMaster> OrderMasters { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(d => d.OrderMaster).WithMany(o => o.OrderDetail)
                .HasForeignKey(o => o.OrderId);
        }
    }
}
