using CookingRecipeWebsite.Models;
using System.ComponentModel.DataAnnotations;

namespace CookingRecipeWebsite.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên món ăn")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập nguyên liệu")]
        public string Ingredients { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập hướng dẫn nấu")]
        public string Instructions { get; set; }
        public string ImagePath { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; } // Cho phép NULL
        public int UserId { get; set; }
        public User? User { get; set; } // Cho phép NULL
    }
}