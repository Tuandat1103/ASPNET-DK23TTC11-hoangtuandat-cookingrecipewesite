using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace CookingRecipeWebsite.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        public string Name { get; set; }
        public List<Recipe> Recipes { get; set; } // Không có [Required], cho phép null
    }
}