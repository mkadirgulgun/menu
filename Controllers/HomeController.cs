using Dapper;
using Menu.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace Menu.Controllers
{
    public class HomeController : Controller
    {
        string connectionString = "TrustServerCertificate=True";
        
        public IActionResult Index()
        {
            using var connection = new SqlConnection(connectionString);
            var posts = connection.Query<MenuModel>("SELECT menus.*, CategoryName FROM menus LEFT JOIN categories ON menus.CategoryId = categories.Id ").ToList();
            return View(posts);
        }
    }
}
