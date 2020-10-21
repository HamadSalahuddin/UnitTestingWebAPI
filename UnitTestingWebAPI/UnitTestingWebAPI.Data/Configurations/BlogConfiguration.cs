using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTestingWebAPI.Domain;

namespace UnitTestingWebAPI.Data.Configurations
{
    public class BlogConfiguration: EntityTypeConfiguration<Blog>
    {
        public BlogConfiguration()
        {
            ToTable("Blog");
            Property(blog => blog.Name).IsRequired().HasMaxLength(100);
            Property(blog => blog.URL).IsRequired().HasMaxLength(200);
            Property(blog => blog.Owner).IsRequired().HasMaxLength(50);
            Property(blog => blog.DateCreated).HasColumnType("datetime2");
        }
    }
}
