using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTestingWebAPI.Domain;

namespace UnitTestingWebAPI.Data.Configurations
{
    public class ArticleConfiguration: EntityTypeConfiguration<Article>
    {
        public ArticleConfiguration()
        {
            ToTable("Article");
            Property(article => article.Title).IsRequired().HasMaxLength(100);
            Property(article => article.Contents).IsRequired();
            Property(article => article.Author).IsRequired().HasMaxLength(50);
            Property(article => article.URL).IsRequired().HasMaxLength(200);
            Property(article => article.DateCreated).HasColumnType("datetime2");
            Property(article => article.DateEdited).HasColumnType("datetime2");
        }
    }
}
