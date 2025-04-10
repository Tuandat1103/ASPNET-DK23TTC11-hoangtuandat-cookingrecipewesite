using Microsoft.EntityFrameworkCore;

namespace CookingRecipeWebsite.Models
{
    public class RecipeContext : DbContext
    {
        public RecipeContext(DbContextOptions<RecipeContext> options) : base(options) { }

        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; } // Thêm dòng này
    }
}