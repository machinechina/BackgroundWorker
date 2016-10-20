namespace Infrastructure.UnitTest
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.ComponentModel.DataAnnotations;

    public partial class TestModel : DbContext
    {
        public TestModel()
            : base("name=TestModelContext")
        {
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }

        public DbSet<TestTable> TestTables { get; set; }
    }

    public class TestTable
    {
        [Key]
        public String TestColumn1 { get; set; }
    }
}
