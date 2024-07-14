using Dapper;
using Menu.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Menu.Controllers
{
    public class AdminController(IConfiguration configuration) : Controller
    {
        string connectionString = "TrustServerCertificate=True";
        public static bool isApproved { get; set; } = false;
        public IActionResult Index()
        {
            if(isApproved)
            {
            using var connection = new SqlConnection(connectionString);
            var menus = connection.Query<MenuModel>("SELECT menus.*, CategoryName FROM menus LEFT JOIN categories ON menus.CategoryId = categories.Id ORDER BY categories.CategoryName ASC").ToList();
            return View(menus);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        public IActionResult Duzenle(int id)
        {
            if (isApproved)
            {
                using var connection = new SqlConnection(connectionString);
                var menus = connection.QuerySingleOrDefault<MenuModel>("SELECT menus.*, categories.CategoryName FROM menus LEFT JOIN categories ON menus.CategoryId = categories.Id WHERE menus.Id = @id", new { id = id });
                var category = connection.Query<MenuModel>("SELECT * FROM categories").ToList();
                ViewBag.Categories = category;

                return View(menus);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        [HttpPost]
        public IActionResult Duzenle(MenuModel model)
        {
            using var connection = new SqlConnection(connectionString);
            var imageName = model.ImgUrl;
            if (model.Image != null)
            {
                imageName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", imageName);
                using var stream = new FileStream(path, FileMode.Create);
                model.Image.CopyTo(stream);
            }
            var sql = "UPDATE menus SET Name = @Name, Detail = @Detail,Price = @Price, CategoryId = @CategoryId, ImgUrl = @ImgUrl WHERE Id=@Id";
            
            var parameters = new
            {
                model.Name,
                model.Detail,
                model.Price,
                model.CategoryId,
                model.Id,
                ImgUrl = imageName
            };

            var affectedRows = connection.Execute(sql, parameters);
            ViewBag.Message = "Güncellendi.";
            ViewBag.MessageCssClass = "alert-success";
            return View("Message");
        }
        public IActionResult Sil(int id)
        {
            using var connection = new SqlConnection(connectionString);
            var sql = "DELETE FROM menus WHERE Id = @Id";

            var rowsAffected = connection.Execute(sql, new { Id = id });

            return RedirectToAction("Index");
        }
        public IActionResult Ekle()
        {
            if (isApproved)
            {
                using var connection = new SqlConnection(connectionString);
                var category = connection.Query<MenuModel>("SELECT * FROM categories").ToList();
                return View(category);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        [HttpPost]
        public IActionResult Ekle(MenuModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Eksik veya hatalı işlem yaptın";
                return View("Message");
            }
            using var connection = new SqlConnection(connectionString);
            var menus = "INSERT INTO menus (Name, Price, ImgUrl, Detail, CategoryId ) VALUES (@Name, @Price, @ImgUrl, @Detail, @CategoryId)";
            
            var imageName = Guid.NewGuid().ToString() + Path.GetExtension(model.Image.FileName);

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", imageName);
            using var stream = new FileStream(path, FileMode.Create);
            model.Image.CopyTo(stream);
            model.ImgUrl = imageName;
            var data = new
            {
                model.Name,
                model.Price,
                model.Detail,
                model.ImgUrl,
                model.CategoryId
            };

            var rowsAffected = connection.Execute(menus, data);
            ViewBag.MessageCssClass = "alert-success";
            ViewBag.Message = "Ürün başarıyla eklendi.";
            return View("Message");
        }

        public IActionResult Category()
        {
            if (isApproved)
            {
                using var connection = new SqlConnection(connectionString);
                var category = connection.Query<MenuModel>("SELECT * FROM categories").ToList();

                return View(category);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        public IActionResult AddCategory()
        {
            if (isApproved)
            {

            return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        [HttpPost]
        public IActionResult AddCategory(Category model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Eksik veya hatalı işlem yaptın";
                return View("Message");
            }
            using var connection = new SqlConnection(connectionString);
            var sql = "INSERT INTO categories (CategoryName) VALUES (@CategoryName)";
            var data = new
            {
                model.CategoryName
            };
            var rowsAffected = connection.Execute(sql, data);
            ViewBag.MessageCssClass = "alert-success";
            ViewBag.Message = "Eklendi.";
            return View("Message");
        }
        public IActionResult DeleteCategory(int id)
        {
            using var connection = new SqlConnection(connectionString);
            var sql = "DELETE FROM categories WHERE Id = @Id";

            var rowsAffected = connection.Execute(sql, new { Id = id });

            return RedirectToAction("Category");
        }
        public IActionResult EditCategory(int id)
        {
            if (isApproved)
            {
                using var connection = new SqlConnection(connectionString);
                var menus = connection.QuerySingleOrDefault<MenuModel>("SELECT *FROM categories WHERE Id = @Id", new { Id = id });
                return View(menus);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        [HttpPost]
        public IActionResult EditCategory(Category model)
        {
            using var connection = new SqlConnection(connectionString);
            
            var sql = "UPDATE categories SET CategoryName = @CategoryName WHERE Id=@Id";

            var parameters = new
            {
                model.CategoryName,
                model.Id,
            };

            var affectedRows = connection.Execute(sql, parameters);
            ViewBag.Message = "Güncellendi.";
            ViewBag.MessageCssClass = "alert-success";
            return View("Message");
        }
    }
}
