using Microsoft.EntityFrameworkCore;

namespace DotnetCampusP2PFileShareTracer.Data
{
    public class NodeContext : DbContext
    {
        public NodeContext (DbContextOptions<NodeContext> options)
            : base(options)
        {
        }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Node>().HasIndex(temp => temp.MainIp);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Node> Node { get; set; } = null!;
    }
}
