using Microsoft.EntityFrameworkCore;

namespace ExcelImporter.WebApi;

/// <summary>
/// The data context, to store and read to and from the database.
/// </summary>
public class DataContext : DbContext
{
	public DataContext(DbContextOptions<DataContext> opt) : base(opt)
	{
	}

	/// <summary>
	/// The products table.
	/// </summary>
	/// <value></value>
    public DbSet<Product> Products { get; set; }
}
