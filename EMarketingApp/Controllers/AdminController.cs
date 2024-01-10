using EMarketingApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EMarketingApp.Controllers
{
    public class AdminController : Controller
    {
        dbemarketingEntities db = new dbemarketingEntities();

        [HttpGet]
        public ActionResult Index()
        {
            if (Session["ad_id"] != null)
            {
                var categories = db.tbl_category.ToList();
                return View(categories);
            }
            return RedirectToAction("Login", "Admin");
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Login(LoginDto login)
        {
            if (String.IsNullOrEmpty(login.Username) || String.IsNullOrEmpty(login.Password)) throw new ArgumentNullException(nameof(login));
            tbl_admin ad = db.tbl_admin.Where(x => x.ad_username == login.Username && x.ad_password == login.Password).SingleOrDefault();
            if (ad != null)
            {

                Session["ad_id"] = ad.ad_id.ToString();
                Session["ad_username"] = ad.ad_username.ToString();
                Session["isAdmin"] = true;
                Response.StatusCode = 200;
                return Json(login,"application/json",contentEncoding: System.Text.Encoding.UTF8,JsonRequestBehavior.AllowGet);

            }
            Response.StatusCode = 400;
            ViewBag.error = "Invalid username or password";
            return Json(login);
        }

        //[HttpPost]
        //public ActionResult Login(tbl_admin avm)
        //{
        //    try
        //    {
        //        if (String.IsNullOrEmpty(avm.ad_username) || String.IsNullOrEmpty(avm.ad_password)) throw new ArgumentNullException(nameof(avm));
        //        tbl_admin ad = db.tbl_admin.Where(x => x.ad_username == avm.ad_username && x.ad_password == avm.ad_password).SingleOrDefault();
        //        if (ad != null)
        //        {

        //            Session["ad_id"] = ad.ad_id.ToString();
        //            Session["ad_username"] = ad.ad_username.ToString();
        //            Session["isAdmin"] = true;
        //            Response.StatusCode = 200; //Successfull Request
        //            return RedirectToAction("Index","Admin");

        //        }

        //        Response.StatusCode = 400; // Bad Request
        //        ViewBag.error = "Invalid username or password";
        //        return View();
        //    }
        //    catch (ArgumentNullException ex)
        //    {
        //        return RedirectToAction("Error", "Home", ex);
        //    }
        //    catch (Exception ex)
        //    {
        //        return RedirectToAction("Error","Home", ex);
        //    }
        //}

        public ActionResult Logout()
        {
            Session.RemoveAll();
            Session.Abandon();
            return RedirectToAction("Login", "Admin");
        }

        #region Category CRUD Operations
        [HttpGet]
        public ActionResult ViewCategories() {

            if (Session["ad_id"] != null)
            {
                var cats = db.tbl_category.ToList();
                return View(cats);
            }

            return RedirectToAction("Login", "Admin");
        }

        [HttpGet]
        public ActionResult CreateCategory()
        {
            if (Session["ad_id"] == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            return View();
        }

        [HttpPost]
        public ActionResult CreateCategory(tbl_category cvm, HttpPostedFileBase imgfile)
        {
            var category = db.tbl_category.FirstOrDefault(c => c.cat_name == cvm.cat_name);
            string path = UploadImgFile(imgfile);

            if (path.Equals("-1"))
            {
                // Image upload failed
                Response.StatusCode = 500; // Internal Server Error
                ViewBag.error = "Image could not be uploaded....";
            }
            else if (category != null)
            {
                // Category already exists
                Response.StatusCode = 400; // Bad Request
                ViewBag.error = $"{category.cat_name} is already exist, Enable it if is disabled.";
            }
            else
            {
                tbl_category cat = new tbl_category();
                cat.cat_name = cvm.cat_name;
                cat.cat_image = path;
                cat.cat_status = 1;
                cat.cat_created = DateTime.Now;
                cat.cat_updated = DateTime.Now;
                cat.cat_fk_ad = Convert.ToInt32(Session["ad_id"].ToString());
                db.tbl_category.Add(cat);
                db.SaveChanges();

                Response.StatusCode = 200; // OK
                return RedirectToAction("ViewCategories");

            }

            return View();
        }

        [HttpGet]
        public ActionResult EditCategory(int id)
        {
            if (Session["ad_id"] == null)
            {
                return RedirectToAction("Login", "Admin");
            }
            if(id == 0)
            {
                RedirectToAction("ViewCategories");
            }

            var cat = db.tbl_category.FirstOrDefault(c => c.cat_id == id);
            if (cat == null)
            {
                return RedirectToAction("ViewCategories");
            }

            return View(cat);


        }

        [HttpPost]
        public ActionResult EditCategory(tbl_category category, HttpPostedFileBase imgfile) 
        {
            var existingCategory = db.tbl_category.Include(c => c.tbl_admin).SingleOrDefault(c => c.cat_id == category.cat_id);
            var oldFilePath = existingCategory.cat_image;
            var fileName = Path.GetFileName(oldFilePath);
            string path = String.Empty;

            if(imgfile != null)
            {
                if (!oldFilePath.Contains(imgfile.FileName))
                {
                    path = UploadImgFile(imgfile);
                }
            }
           
           
            if (path.Equals("-1"))
            {
                // Image upload failed
                Response.StatusCode = 500; // Internal Server Error
                ViewBag.error = "Image could not be uploaded....";
                return View();
            }


            var editedCategory =  new tbl_category();
            editedCategory.cat_id = category.cat_id;
            editedCategory.cat_name = category.cat_name;
            editedCategory.cat_status = category.cat_status;
            editedCategory.cat_image = path == String.Empty ? oldFilePath : path;
            editedCategory.cat_updated = DateTime.Now;
            editedCategory.cat_created = category.cat_created;
            editedCategory.cat_fk_ad = category.cat_fk_ad;

            
            db.tbl_category.AddOrUpdate(editedCategory);
            db.SaveChanges();
            
            if(imgfile != null)
            {
                if (!RemoveImgFile(oldFilePath))
                {
                    ViewBag.error = "Error Occured while deleting old file";
                }
            }
            return View(editedCategory);
        }


        [HttpGet]
        public ActionResult CategoryDetails(int id)
        {
            var categoryDetails = db.tbl_category.Include(c => c.tbl_admin).Include(c => c.tbl_product).FirstOrDefault(c => c.cat_id == id);
            if (categoryDetails == null)
            {
                ViewBag.error = "Details Not Found!";
                return View("ViewCategories");
            }
            return View(categoryDetails);
        }

        public ActionResult DeleteCategory(int id)
        {
            if(id == 0)
            {
                ViewBag.error = "Category Id should not be 0";
                return RedirectToAction("ViewCategories");
            }

            var existingCategory = db.tbl_category.SingleOrDefault(c => c.cat_id == id);
            if(existingCategory == null)
            {
                ViewBag.error = "Category is not available";
                return RedirectToAction("ViewCategories");
            }
            var imagePath = existingCategory.cat_image; 
            db.tbl_category.Remove(existingCategory);
            db.SaveChanges();

            if (!RemoveImgFile(imagePath))
            {
                ViewBag.error = "Error Occured while deleting old file";
            }

            return RedirectToAction("ViewCategories");
        }
        #endregion

        #region Admin Private Methods
        private string UploadImgFile(HttpPostedFileBase file)
        {
            string path = "-1";
            if (file != null && file.ContentLength > 0)
            {
                string extension = Path.GetExtension(file.FileName);
                if (extension.ToLower().Equals(".jpg") || extension.ToLower().Equals(".jpeg") || extension.ToLower().Equals(".png"))
                {
                    try
                    {

                        path = Path.Combine(Server.MapPath("~/Content/uploads/img/categories"),Path.GetFileName(file.FileName));
                        file.SaveAs(path);
                        path = "~/Content/uploads/img/categories/" + Path.GetFileName(file.FileName);

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

        private bool RemoveImgFile(string path)
        {
            if (path.Equals("-1") || path == "-1") return false;
            
            var fileFullPath = Server.MapPath(path);
            FileInfo fileInfo = new FileInfo(fileFullPath);
            if (fileInfo.Exists) 
            {
                
                fileInfo.Delete();
                return true;
            }
            return false;
            
            
        }

        #endregion
    }


}