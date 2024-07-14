using Dapper;
using Menu.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Net.Mail;
using System.Net;
using System.Reflection;

namespace Menu.Controllers
{
    public class LoginController : Controller
    {
        string connectionString = "TrustServerCertificate=True";

        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Index(Login model)
        {
            using var connection = new SqlConnection(connectionString);
            var login = connection.Query<Login>("SELECT * FROM Users").ToList();

            if (!ModelState.IsValid)
            {
                ViewData["Error"] = "Kullanıcı bulunamadı!";
                return View("Index", login);
            }
            foreach (var user in login)
            {
                if (user.UserName == model.UserName && user.Password == model.Password && user.IsApproved == 1)
                {
                    ViewData["Msg"] = "Giriş Başarılı";
                    AdminController.isApproved = true;
                    //TempData["isApproved"] = "1";
                    return RedirectToAction("Index", "Admin");

                }
                ViewData["Msg"] = "Kullanıcı adı veya şifre yanlış";

            }
            return View();
        }
        public IActionResult Exit()
        {
            AdminController.isApproved = false;
            return RedirectToAction("Index", "Home");
        }

        public IActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public IActionResult SignUp(Login model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MessageCssClass = "alert-danger";
                ViewBag.Message = "Eksik veya hatalı işlem yaptın";
                return View("Message");
            }
            using var connection = new SqlConnection(connectionString);
            var login = connection.QueryFirstOrDefault<Login>("SELECT * FROM Users WHERE Email = @Email OR Username = @UserName", new { Email = model.Email, UserName = model.UserName });

            if (login == null)
            {
                
                var Key = Guid.NewGuid().ToString();

                var signup = "INSERT INTO users (Username, Password, Email, Gender,ValidKey) VALUES (@UserName, @Password, @Email, @Gender,@ValidKey)";

                var data = new
                {
                    model.UserName,
                    model.Password,
                    model.Email,
                    model.Gender,
                    ValidKey = Key
                };

                var rowsAffected = connection.Execute(signup, data);

                ViewBag.Subject = "Hoş Geldiniz! Kayıt İşleminiz Başarıyla Tamamlandı";
                ViewBag.Body = $"<h1>Hoş Geldiniz, {model.UserName}!</h1>\r\n            <p>Web sitemize kayıt olduğunuz için teşekkür ederiz. Kayıt işleminiz başarıyla tamamlandı.</p>\r\n            <p>Aşağıdaki bilgileri gözden geçirebilirsiniz:</p>\r\n            <ul>\r\n                <li><strong>Kullanıcı Adı:</strong> {model.UserName}</li>\r\n                <li><strong>E-posta:</strong> {model.Email}</li>\r\n            </ul>\r\n            <p>Hesabınızı doğrulamak ve hizmetlerimizden yararlanmaya başlamak için <a href=\"https://menu.mkadirgulgun.com.tr/Login/Account/{Key}\">buraya tıklayın</a>.</p>\r\n            <p>İyi günler dileriz!</p>";
                ViewBag.MessageCssClass = "alert-success";
                ViewBag.Message = "Başarıyla kayıt olundu. Onaylamak için mail kutunuza gidin";
                ViewBag.Return = "Message";
                SendMail(model);
                return View("Message");
            }
            else if (login.Email == model.Email)
            {
                ViewData["Msg"] = "Bu mail kayıtlı";
                return View("SignUp");
            }
            else
            {
                ViewData["Msg"] = "Bu kullanıcı adı kayıtlı";
                return View("SignUp");
            }
        }
        public IActionResult SendMail(Login model)
        {
            
            var mailMessage = new MailMessage
            {
                From = new MailAddress("bildirim@menu.com.tr", "Menu.com"),
                //ReplyTo = new MailAddress("info@mkadirgulgun.com.tr", "Mehmet Kadir Gülgün"),
                Subject = ViewBag.Subject,
                Body = ViewBag.Body,
                IsBodyHtml = true,
            };

            mailMessage.ReplyToList.Add(model.Email);
            //mailMessage.To.Add("mkadirgulgun@gmail.com");
            mailMessage.To.Add(new MailAddress($"{model.Email}", $"{model.UserName}"));

            client.Send(mailMessage);
            return RedirectToAction(ViewBag.Return);

        }

        public IActionResult Account(string id)
        {
            using var connection = new SqlConnection(connectionString);
            var account = connection.QueryFirstOrDefault<Login>("SELECT * FROM Users WHERE ValidKey = @ValidKey", new { ValidKey = id });

            return View(account);
        }

        public IActionResult ConfirmAccount(int id)
        {
            using var connection = new SqlConnection(connectionString);
            var students = connection.QueryFirstOrDefault<Login>("SELECT * FROM Users", new { Id = id });

            var sql = "UPDATE Users SET IsApproved = 1 WHERE Id = @Id";
            var affectedRows = connection.Execute(sql, new { Id = id });

            return RedirectToAction("Index");
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public IActionResult ForgotPassword(Login model)
        {
            using var connection = new SqlConnection(connectionString);
            var login = connection.QueryFirstOrDefault<Login>("SELECT * FROM Users WHERE Email = @Email", new { Email = model.Email});

            if (!(login == null))
            {
               
                var Key = Guid.NewGuid().ToString();
                var change = "UPDATE users SET ResetKey = @ResetKey WHERE Email=@Email";
                var parameters = new
                {
                    Email = login.Email,
                    ResetKey = Key
                };
                var affectedRows = connection.Execute(change, parameters);

                ViewBag.Subject = "Şifre Sıfırlama Talebiniz";  
                  ViewBag.Body = $"<p>Merhaba {model.UserName},</p>\r\n            <p>Şifrenizi sıfırlamak için bir talepte bulunduğunuzu aldık. Lütfen aşağıdaki bağlantıya tıklayarak şifrenizi sıfırlayın:</p>\r\n            <p><a href=\"https://menu.mkadirgulgun.com.tr/Login/ResetPassword/{Key}\" class=\"button\">Şifreyi Sıfırla</a></p>\r\n            <p>Bu bağlantı, güvenliğiniz için 24 saat geçerli olacaktır. Eğer bu talebi siz yapmadıysanız, lütfen bu e-postayı dikkate almayın.</p>\r\n            <p>Şifrenizi sıfırlama konusunda herhangi bir sorun yaşarsanız, bizimle iletişime geçmekten çekinmeyin.</p>";
                ViewBag.MessageCssClass = "alert-success";
                ViewBag.Message = "Şifre Sıfırlama Talebiniz Başarıyla Alındı. Lütfen mail kutunuza gidin";
                ViewBag.Return = "Message";
                SendMail(model);
                return View("Message");
            }
            else
            {
                @ViewData["Msg"] = "Bu E-Postaya ait bir kayıt bulunamadı.";
                return View();
            }
        }
        public IActionResult ResetPassword(string id)
        {
            using var connection = new SqlConnection(connectionString);
            var account = connection.QueryFirstOrDefault<Login>("SELECT * FROM Users WHERE ResetKey = @ResetKey", new { ResetKey = id });

            return View(account);
        }
        [HttpPost]
        public IActionResult ResetPassword(Login model)
        {
            using var connection = new SqlConnection(connectionString);
            var mail = "SELECT * FROM Users";
            var sql = "UPDATE Users SET Password = @Password WHERE Id=@Id";

            var parameters = new
            {
                model.Password,
                model.Id,
            };

            var affectedRows = connection.Execute(sql, parameters);
            ViewBag.Message = "Şifre Güncellendi.";
            ViewBag.MessageCssClass = "alert-success";
            return View("Message");
        }
    }
}
