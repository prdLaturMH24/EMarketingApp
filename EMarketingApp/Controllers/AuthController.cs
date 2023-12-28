using EMarketingApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EMarketingApp.Controllers
{
    public class AuthController : Controller
    {
        dbemarketingEntities db = new dbemarketingEntities();
        private readonly string userRoleKey = ConfigurationManager.AppSettings["UserRoleKey"];
        // GET: Auth
        public ActionResult Index()
        {
            if (Session["IsLoggedIn"] !=null && Convert.ToBoolean(Session["IsLoggedIn"]) == true)
            {
                return RedirectToAction("Index","User");
            }
            Session["IsLoggedIn"] = false;
            return View();
        }

        [HttpGet]
        public ActionResult Login() 
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginDto login)
        {
            try
            {
                if (String.IsNullOrEmpty(login.Username) || String.IsNullOrEmpty(login.Password)) throw new ArgumentNullException(nameof(login));

                //tbl_admin ad = db.tbl_admin.Where(x => x.ad_username == login.Username && x.ad_password == login.Password).SingleOrDefault();

                tbl_user user = db.tbl_user.Where(x => (x.u_name == login.Username || x.u_email == login.Username) && x.u_password == login.Password).SingleOrDefault();
                //if (ad != null)
                //{

                //    Session["ad_id"] = ad.ad_id.ToString();
                //    Session["ad_username"] = ad.ad_username.ToString();
                //    Session["isAdmin"] = true;
                //    Response.StatusCode = 200; //Successfull Request
                //    return RedirectToAction("Index", "Admin");

                //}
                //else 
                if (user != null)
                {
                    Session["u_id"] = user.u_id.ToString();
                    Session["u_username"] = user.u_name.ToString();
                    Session["isAdmin"] = false;
                    Session["photoUrl"] = user.u_image.ToString();
                    Session["IsLoggedIn"] = true;
                    Response.StatusCode = 200; //Successfull Request
                    return RedirectToAction("Index", "User");
                }

                Response.StatusCode = 400; // Bad Request
                Session["isLoggedIn"] = false;
                ViewBag.error = "Invalid username or password";
                return View();
            }
            catch (ArgumentNullException ex)
            {
                return RedirectToAction("Error", "Home", ex);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", ex);
            }
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(RegisterDto register, HttpPostedFileBase imgfile)
        {
            if(register == null)
            {
                ViewBag.error = "Please check the details.";
                return View();

            }
            string path = String.Empty;
            if(imgfile != null)
            {
                path = UploadImgFile(imgfile,register.UserName);
            }
            //Get User Role Id
            var roleId = db.Roles.SingleOrDefault(r => r.RoleId.ToString() == userRoleKey).Id;

            tbl_user user = new tbl_user();
            user.u_name = register.UserName;
            user.u_email = register.Email;
            user.u_password = register.Password;
            user.u_contact = register.Phone;
            user.RoleId = roleId;
            user.u_image = path;

            db.tbl_user.Add(user);
            
            if(!((int)db.SaveChanges() > 0))
            {
                ViewBag.error = "Something went wrong.";
                Response.StatusCode = 500;
                return View();
            }

            return View("Login");

        }

        public ActionResult LogOut()
        {
            Session.RemoveAll();
            Session.Abandon();
            Session["IsLoggedIn"] = false;
            return RedirectToAction("Index", "Auth");
        }

        private string UploadImgFile(HttpPostedFileBase file,string userName)
        {
            string path = "-1";
            if (file != null && file.ContentLength > 0)
            {
                string extension = Path.GetExtension(file.FileName);
                if (extension.ToLower().Equals(".jpg") || extension.ToLower().Equals(".jpeg") || extension.ToLower().Equals(".png"))
                {
                    try
                    {

                        path = Path.Combine(Server.MapPath("~/Content/users/img"),userName + Path.GetFileName(file.FileName));
                        file.SaveAs(path);
                        path = "~/Content/users/img/"+ userName + Path.GetFileName(file.FileName);

                        //ViewBag.Message = "File uploaded successfully";
                    }
                    catch (Exception ex)
                    {
                        path = "-1";
                    }
                }
                else
                {
                    Response.Write("<script>alert('Only jpg ,jpeg or png formats are acceptable....'); </script>");
                }
            }

            else
            {
                Response.Write("<script>alert('Please select a file'); </script>");
                path = "-1";
            }
            return path;
        }

    }
}