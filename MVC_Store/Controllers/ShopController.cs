using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Store.Controllers
{
    public class ShopController : Controller
    {
        // GET: Index/{page}
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");
        }
        public ActionResult CategoryMenuPartial()

        {
            List<CategoryVM> categoryVMList;
            using (Db db = new Db())
            {
                categoryVMList = db.Categories
                    .ToArray()
                    .OrderBy(x => x.Sorting).Select(x => new CategoryVM(x))
                    .ToList();
            }
            return PartialView("_CategoryMenuPartial", categoryVMList);

        }

        public ActionResult Category(string name)
        {
            List<ProductVM> productVMList;
            using (Db db = new Db()) 
                       
            {
                //получаем АЙДИ категории
                CategoryDTO categoryDTO =
                        db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                int catId = categoryDTO.Id;
                //Инициализируем список данных
                productVMList = db.Products.ToArray().Where(x => x.CategoryId == catId)                  
                    .Select(x => new ProductVM(x)).ToList();
                //Получаем имя категории
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();

                //Делаем проверку на налл

                if (productCat == null)

                {
                    var catName = db.Categories.Where(x => x.Slug == name).Select(x => x.Name).FirstOrDefault();
                    ViewBag.CategoryName = catName;
                }
                else
                {
                    ViewBag.CategoryName = productCat.CategoryName;
                }          
            
            
                return View(productVMList);

            }
        }

        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {
            //Объявляем модели DTO и VM
            ProductDTO dto;
            ProductVM model;
            //Инициализация продуктов ID
            int id = 0;
            // Проверяем доступен ли продукт
            using (Db db = new Db())
            {
                if (!db.Products.Any(x => x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");

                }
                // инициализируем модель DTO

                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();
                // Получаем АЙДИ
                id = dto.Id;
                //инициализируем модель VM
                model = new ProductVM(dto);
            }

            // Получаем модель из галереи

            model.GalleryImages = Directory
                .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                .Select(fn => Path.GetFileName(fn));
                // Возвращаем модель в представление
                return View("ProductDetails", model);
        }

    }
}