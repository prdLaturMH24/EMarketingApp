using EMarketingApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EMarketingApp.Controllers
{
    public class UserController : Controller
    {
        dbemarketingEntities db = new dbemarketingEntities();

        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Categories()
        {
            var categories = db.tbl_category.ToList();
            return View(categories);
        }

        [HttpGet]
        public ActionResult Products() 
        {
            if (Session["u_username"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var id = Convert.ToInt32(Session["u_id"]);
            var products = db.tbl_product.Where(p => p.pro_fk_user == id ).ToList(); 
            return View(products);
        }

        [HttpGet]
        public ActionResult ProductsByCategory(int categoryId)
        {
            var products = db.tbl_product.Where(p => p.pro_fk_cat == categoryId).ToList();
            return View(products);

        }

        [HttpGet]
        public ActionResult AddProduct()
        {
            if (Session["u_username"] == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            //Get only active categories
            List<tbl_category> li = db.tbl_category.Where(c => c.cat_status == 1).ToList();
            ViewBag.categorylist = new SelectList(li, "cat_id", "cat_name");
            return View();
        }

        [HttpPost]
        public ActionResult AddProduct(tbl_product product, HttpPostedFileBase imgfile)
        {
            try
            {
                if (product == null)
                {
                    throw new ArgumentNullException(nameof(product));
                }
                var path = String.Empty;
                if (imgfile != null)
                {
                    path = UploadImgFile(imgfile,product.pro_name);
                }

                tbl_product newProduct = new tbl_product();
                newProduct.pro_name = product.pro_name;
                newProduct.pro_updated = DateTime.Now;
                newProduct.pro_price = product.pro_price;
                newProduct.pro_des = product.pro_des;
                newProduct.pro_image = path;
                newProduct.pro_created = DateTime.Now;
                newProduct.pro_fk_cat = product.pro_fk_cat;
                newProduct.pro_fk_user = product.pro_fk_user?? Convert.ToInt32(Session["u_id"]);

                db.tbl_product.Add(newProduct);

                if (!((int)db.SaveChanges() > 0))
                {
                    ViewBag.error = "Something went wrong.";
                    Response.StatusCode = 500;
                    return View();
                }
                return RedirectToAction("Products");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", ex);
            }
        }

        public ActionResult DeleteProduct(int productId)
        {
            try
            {

                if (productId == 0)
                {
                    return RedirectToAction("Login","Auth");
                }

                var existingProduct = db.tbl_product.SingleOrDefault(c => c.pro_id == productId);
                if (existingProduct == null)
                {
                    return RedirectToAction("Products");
                }
                var imagePath = existingProduct.pro_image;
                db.tbl_product.Remove(existingProduct);
                db.SaveChanges();

                if (!RemoveImgFile(imagePath))
                {
                    return RedirectToAction("Products");
                }

                return RedirectToAction("Products");
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return RedirectToAction("Error", "Home",ex);
            }
        }

        public ActionResult EditProduct(int productId)
        {
            try
            {
                if (Session["u_id"] == null)
                {
                    return RedirectToAction("Login", "Auth");
                }
                if (productId == 0)
                {
                    return RedirectToAction("Login","Auth");
                }

                var product = db.tbl_product.Include(p => p.tbl_category).SingleOrDefault(p => p.pro_id == productId);

                if (product == null)
                {
                    return RedirectToAction("Products");
                }
                //Get only active categories
                List<tbl_category> li = db.tbl_category.Where(c => c.cat_status == 1).ToList();
                ViewBag.categorylist = new SelectList(li, "cat_id", "cat_name");
                return View(product);
            }
            catch (Exception ex)
            {
               return RedirectToAction("Error", "Home", ex);  
            }
        }

        [HttpPost]
        public ActionResult EditProduct(tbl_product product, HttpPostedFileBase imgfile)
        {
            try
            {
                if (Session["u_id"] == null)
                {
                    return RedirectToAction("Login","Auth");
                }
                if (product == null) throw new ArgumentNullException(nameof(product));

                var existingProduct = db.tbl_product.Include(p => p.tbl_category).Include(p => p.tbl_user).SingleOrDefault(p => p.pro_id == product.pro_id);
                if (existingProduct == null)
                {
                    return RedirectToAction("Products");
                }

                var oldImgPath = existingProduct.pro_image;
                var fileName = Path.GetFileName(oldImgPath);
                var path = String.Empty;

                if (imgfile != null)
                {
                    if (!oldImgPath.Contains(imgfile.FileName))
                    {
                        path = UploadImgFile(imgfile,product.pro_name);
                    }
                }

                if (path.Equals("-1"))
                {
                    // Image upload failed
                    Response.StatusCode = 500; // Internal Server Error
                    ViewBag.error = "Image could not be uploaded....";
                    return View();
                }

                var editedProduct = new tbl_product();
                editedProduct.pro_id = product.pro_id;
                editedProduct.pro_name = product.pro_name;
                editedProduct.pro_des = product.pro_des;
                editedProduct.pro_price = product.pro_price;
                editedProduct.pro_updated = DateTime.Now;
                editedProduct.pro_created = product.pro_created;
                editedProduct.pro_fk_cat = product.pro_fk_cat;
                editedProduct.pro_fk_user = product.pro_fk_user;
                editedProduct.pro_image = String.IsNullOrEmpty(path) ? oldImgPath : path;

                db.tbl_product.AddOrUpdate(editedProduct);
                if(db.SaveChanges() > 0 )
                {
                   if(imgfile != null)
                    {
                        if (!RemoveImgFile(oldImgPath))
                        {
                            Response.StatusCode = 500;
                            ViewBag.error = "Error Occured while deleting old file";
                            return View(editedProduct);
                        }
                    }
                    return RedirectToAction("EditProduct","User",new {productId = editedProduct.pro_id});
                }
                throw new DbUpdateException("Exception occured while editing data");
            }
            catch(DbUpdateException ex)
            {
                return RedirectToAction("Error", "Home", ex);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", ex);
            }
        }

        [HttpGet]
        public ActionResult ProductDetails(int productId)
        {
            try
            {
                if (Session["u_id"] == null)
                {
                    return RedirectToAction("Login", "Auth");
                }
                if (productId == 0)
                {
                    return RedirectToAction("Login", "Auth");
                }

                var product = db.tbl_product.Include(p => p.tbl_category).Include(p => p.tbl_user).SingleOrDefault(p => p.pro_id == productId);

                if (product == null)
                {
                    return RedirectToAction("Products");
                }

                return View(product);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", ex);
            }
        }
        [HttpGet]
        public ActionResult Details(int productId)
        {
            try
            {
                if (productId == 0)
                {
                    return RedirectToAction("ProductsByCategory");
                }

                var product = db.tbl_product.Include(p => p.tbl_category).SingleOrDefault(p => p.pro_id == productId);

                if (product == null)
                {
                    return RedirectToAction("ProductsByCategory");
                }

                return View(product);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", ex);
            }
        }

        private string UploadImgFile(HttpPostedFileBase file, string productName)
        {
            string path = "-1";
            productName = productName.Replace(" ","").Trim();
            if (file != null && file.ContentLength > 0)
            {
                string extension = Path.GetExtension(file.FileName);
                if (extension.ToLower().Equals(".jpg") || extension.ToLower().Equals(".jpeg") || extension.ToLower().Equals(".png"))
                {
                    try
                    {

                        path = Path.Combine(Server.MapPath("~/Content/uploads/img/products"), productName + Path.GetFileName(file.FileName));
                        file.SaveAs(path);
                        path = "~/Content/uploads/img/products/" + productName + Path.GetFileName(file.FileName);

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

    }
}