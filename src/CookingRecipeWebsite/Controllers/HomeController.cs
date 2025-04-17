using Microsoft.AspNetCore.Mvc;
using CookingRecipeWebsite.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq; // Đảm bảo có using này
using System.IO;
using System.Threading.Tasks;
namespace CookingRecipeWebsite.Controllers
{
    public class HomeController : Controller
    {
        private readonly RecipeContext _context;

        public HomeController(RecipeContext context)
        {
            System.Diagnostics.Debug.WriteLine("Initializing HomeController");
            _context = context;
        }

        public IActionResult Index(string searchString, int? categoryId, int page = 1)
        {
            System.Diagnostics.Debug.WriteLine("Entering Index action");

            int pageSize = 6;
            var recipes = _context.Recipes
                .Include(r => r.Category)
                .Include(r => r.User)
                .Select(r => new Recipe
                {
                    Id = r.Id,
                    Title = r.Title ?? "Không có tiêu đề",
                    Ingredients = r.Ingredients ?? "Không có nguyên liệu",
                    Instructions = r.Instructions ?? "Không có hướng dẫn",
                    ImagePath = r.ImagePath,
                    CategoryId = r.CategoryId,
                    Category = r.Category != null ? new Category { Id = r.Category.Id, Name = r.Category.Name ?? "Chưa phân loại" } : null,
                    UserId = r.UserId,
                    User = r.User != null ? new User { Id = r.User.Id, Email = r.User.Email ?? "unknown@example.com", Password = r.User.Password ?? "default123" } : null
                })
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                recipes = recipes.Where(r => r.Title.ToLower().Contains(searchString) ||
                                             r.Ingredients.ToLower().Contains(searchString));
                System.Diagnostics.Debug.WriteLine($"Searching for: {searchString}");
            }

            if (categoryId.HasValue)
            {
                recipes = recipes.Where(r => r.CategoryId == categoryId.Value);
                System.Diagnostics.Debug.WriteLine($"Filtering by CategoryId: {categoryId}");
            }

            int totalRecipes = recipes.Count();
            int totalPages = (int)Math.Ceiling(totalRecipes / (double)pageSize);

            if (page < 1) page = 1;

            try
            {
                var recipeList = recipes
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Page: {page}, Total Recipes: {totalRecipes}, Total Pages: {totalPages}, Showing: {recipeList.Count}");

                ViewBag.SearchString = searchString;
                ViewBag.CategoryId = categoryId.HasValue ? categoryId.Value : 0;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.Categories = _context.Categories?.ToList() ?? new List<Category>();
                ViewBag.UserId = HttpContext.Session.GetString("UserId");
                ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
                System.Diagnostics.Debug.WriteLine($"Current UserId from Session: {ViewBag.UserId}");

                return View(recipeList);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading recipes: {ex.Message}");
                return View("Error", new ErrorViewModel { RequestId = ex.Message });
            }
        }
        public IActionResult Detail(int id)
        {
            System.Diagnostics.Debug.WriteLine($"Entering Detail action with id: {id}");
            var recipe = _context.Recipes.FirstOrDefault(r => r.Id == id);
            if (recipe == null)
            {
                System.Diagnostics.Debug.WriteLine("Recipe not found");
                return NotFound();
            }

            // Truyền thông tin người dùng qua ViewBag
            ViewBag.UserId = HttpContext.Session.GetString("UserId");
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            System.Diagnostics.Debug.WriteLine($"UserId: {ViewBag.UserId}, UserEmail: {ViewBag.UserEmail}");

            return View(recipe);
        }
        [HttpGet]
        public IActionResult AddRecipe()
        {
            System.Diagnostics.Debug.WriteLine("Entering AddRecipe GET action");
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.UserId = HttpContext.Session.GetString("UserId");
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddRecipe(Recipe recipe, IFormFile imageFile)
        {
            System.Diagnostics.Debug.WriteLine("Entering AddRecipe POST action");
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                System.Diagnostics.Debug.WriteLine("User not logged in, redirecting to Login");
                return RedirectToAction("Login");
            }
            recipe.UserId = int.Parse(userId);
            System.Diagnostics.Debug.WriteLine($"Title: {recipe.Title}, Ingredients: {recipe.Ingredients}, Instructions: {recipe.Instructions}, ImageFile: {(imageFile != null ? imageFile.FileName : "null")}, CategoryId: {recipe.CategoryId}, UserId: {recipe.UserId}");

            // Bỏ qua validation cho các trường không cần thiết
            ModelState.Remove("imageFile");
            ModelState.Remove("ImagePath");
            ModelState.Remove("Category");
            ModelState.Remove("User"); // Thêm dòng này để bỏ qua User

            if (imageFile != null && imageFile.Length > 0)
            {
                System.Diagnostics.Debug.WriteLine("Processing image upload");
                if (imageFile.Length > 3 * 1024 * 1024)
                {
                    ModelState.AddModelError("imageFile", "Ảnh không được lớn hơn 3MB.");
                    System.Diagnostics.Debug.WriteLine("Image too large");
                    ViewBag.Categories = _context.Categories.ToList();
                    return View(recipe);
                }

                var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                try
                {
                    if (!Directory.Exists(imagesFolder))
                    {
                        Directory.CreateDirectory(imagesFolder);
                        System.Diagnostics.Debug.WriteLine("Created images folder");
                    }

                    var fileName = Path.GetFileNameWithoutExtension(imageFile.FileName) + "_" + Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(imagesFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    recipe.ImagePath = "/images/" + fileName;
                    System.Diagnostics.Debug.WriteLine($"Image uploaded: {recipe.ImagePath}");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("imageFile", "Lỗi khi upload ảnh: " + ex.Message);
                    System.Diagnostics.Debug.WriteLine("Image upload error: " + ex.Message);
                    ViewBag.Categories = _context.Categories.ToList();
                    return View(recipe);
                }
            }
            else
            {
                recipe.ImagePath = "/images/default.jpg";
                System.Diagnostics.Debug.WriteLine("No image uploaded, using default");
            }

            if (ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("ModelState is valid");
                try
                {
                    _context.Recipes.Add(recipe);
                    await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine("Recipe saved to database");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi lưu công thức: " + ex.Message);
                    System.Diagnostics.Debug.WriteLine("Database save error: " + ex.Message);
                    ViewBag.Categories = _context.Categories.ToList();
                    return View(recipe);
                }
            }

            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    System.Diagnostics.Debug.WriteLine($"ModelState Error for {state.Key}: {error.ErrorMessage}");
                }
            }
            System.Diagnostics.Debug.WriteLine("ModelState invalid, returning view");
            ViewBag.Categories = _context.Categories.ToList();
            return View(recipe);
        }
        [HttpGet]
        public IActionResult EditRecipe(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            System.Diagnostics.Debug.WriteLine($"Entering EditRecipe GET, UserId: {userId}, RecipeId: {id}");
            if (string.IsNullOrEmpty(userId))
            {
                System.Diagnostics.Debug.WriteLine("User not logged in, redirecting to Login");
                return RedirectToAction("Login");
            }

            var recipe = _context.Recipes.FirstOrDefault(r => r.Id == id);
            if (recipe == null)
            {
                System.Diagnostics.Debug.WriteLine("Recipe not found");
                return NotFound();
            }
            if (recipe.UserId != int.Parse(userId))
            {
                System.Diagnostics.Debug.WriteLine("User does not have permission");
                return Forbid();
            }

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.UserId = HttpContext.Session.GetString("UserId");
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            return View(recipe);
        }

        [HttpPost]
        public async Task<IActionResult> EditRecipe(Recipe recipe, IFormFile imageFile)
        {
            var userId = HttpContext.Session.GetString("UserId");
            System.Diagnostics.Debug.WriteLine($"Entering EditRecipe POST, UserId: {userId}, RecipeId: {recipe.Id}");
            if (string.IsNullOrEmpty(userId))
            {
                System.Diagnostics.Debug.WriteLine("User not logged in, redirecting to Login");
                return RedirectToAction("Login");
            }
            if (recipe.UserId != int.Parse(userId))
            {
                System.Diagnostics.Debug.WriteLine("User does not have permission");
                return Forbid();
            }

            ModelState.Remove("imageFile");
            ModelState.Remove("ImagePath");
            ModelState.Remove("Category");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine($"Updating recipe: Title={recipe.Title}, CategoryId={recipe.CategoryId}");
                if (imageFile != null && imageFile.Length > 0)
                {
                    var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    var fileName = Path.GetFileNameWithoutExtension(imageFile.FileName) + "_" + Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(imagesFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    recipe.ImagePath = "/images/" + fileName;
                    System.Diagnostics.Debug.WriteLine($"New image uploaded: {recipe.ImagePath}");
                }

                try
                {
                    _context.Update(recipe);
                    await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine("Recipe updated successfully");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi lưu: " + ex.Message);
                    System.Diagnostics.Debug.WriteLine($"Database error: {ex.Message}");
                }
            }

            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    System.Diagnostics.Debug.WriteLine($"ModelState Error for {state.Key}: {error.ErrorMessage}");
                }
            }
            System.Diagnostics.Debug.WriteLine("ModelState invalid, returning view");
            ViewBag.Categories = _context.Categories.ToList();
            return View(recipe);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var recipe = _context.Recipes.FirstOrDefault(r => r.Id == id);
            if (recipe == null || recipe.UserId != int.Parse(userId))
            {
                return Forbid();
            }

            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Thêm danh mục
        [HttpGet]
        public IActionResult AddCategory()
        {
            ViewBag.UserId = HttpContext.Session.GetString("UserId");
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.Categories = _context.Categories.ToList(); // Lấy tất cả danh mục
            System.Diagnostics.Debug.WriteLine($"Categories loaded: {_context.Categories.Count()}");
            return View();
        }

        // POST: Lưu danh mục mới
        [HttpPost]
        public async Task<IActionResult> AddCategory(Category category)
        {
            System.Diagnostics.Debug.WriteLine($"Adding category: {category.Name}");

            // Loại bỏ validation cho Recipes
            ModelState.Remove("Recipes");

            if (ModelState.IsValid)
            {
                category.Recipes = category.Recipes ?? new List<Recipe>(); // Khởi tạo nếu null
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine("Category added successfully");
                return RedirectToAction("AddCategory");
            }

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                System.Diagnostics.Debug.WriteLine($"Validation error: {error.ErrorMessage}");
            }
            ViewBag.UserId = HttpContext.Session.GetString("UserId");
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.Categories = _context.Categories.ToList();
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> EditCategory([FromBody] EditCategoryModel model)
        {
            System.Diagnostics.Debug.WriteLine($"Editing category: Id={model.Id}, Name={model.Name}");
            if (model == null || string.IsNullOrWhiteSpace(model.Name))
            {
                return Json(new { success = false, message = "Tên danh mục không hợp lệ" });
            }

            var category = await _context.Categories.FindAsync(int.Parse(model.Id));
            if (category == null)
            {
                return Json(new { success = false, message = "Danh mục không tồn tại" });
            }

            category.Name = model.Name;
            _context.Update(category);
            await _context.SaveChangesAsync();
            System.Diagnostics.Debug.WriteLine("Category updated successfully");
            return Json(new { success = true });
        }

        // POST: DeleteCategory (AJAX)
        [HttpPost]
        public async Task<IActionResult> DeleteCategory([FromBody] DeleteCategoryModel model)
        {
            System.Diagnostics.Debug.WriteLine($"Deleting category: Id={model.Id}");
            if (model == null || string.IsNullOrWhiteSpace(model.Id))
            {
                return Json(new { success = false, message = "ID danh mục không hợp lệ" });
            }

            var category = await _context.Categories
                .Include(c => c.Recipes)
                .FirstOrDefaultAsync(c => c.Id == int.Parse(model.Id));
            if (category == null)
            {
                return Json(new { success = false, message = "Danh mục không tồn tại" });
            }

            if (category.Recipes != null && category.Recipes.Any())
            {
                return Json(new { success = false, message = "Không thể xóa danh mục vì đang có công thức liên kết" });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            System.Diagnostics.Debug.WriteLine("Category deleted successfully");
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            ModelState.Remove("Recipes");
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã được đăng ký.");
                    ViewBag.UserId = null;
                    ViewBag.UserEmail = null;
                    return View(user);
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            ViewBag.UserId = null;
            ViewBag.UserEmail = null;
            return View(user);
        }

        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.UserId = HttpContext.Session.GetString("UserId");
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            return View();
        }

        [HttpPost]
        public IActionResult Login(User user)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == user.Email && u.Password == user.Password);
            if (existingUser != null)
            {
                HttpContext.Session.SetString("UserId", existingUser.Id.ToString());
                HttpContext.Session.SetString("UserEmail", existingUser.Email);
                System.Diagnostics.Debug.WriteLine($"User logged in: UserId={existingUser.Id}, Email={existingUser.Email}");
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
            ViewBag.UserId = null; // Chưa đăng nhập
            ViewBag.UserEmail = null;
            return View(user);
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.UserId = HttpContext.Session.GetString("UserId");
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            return View();
        }
        public IActionResult Logout()
        {
            System.Diagnostics.Debug.WriteLine("Entering Logout action");
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
    public class EditCategoryModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class DeleteCategoryModel
    {
        public string Id { get; set; }
    }
}