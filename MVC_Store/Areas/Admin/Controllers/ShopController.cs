using MVC_Store.Areas.Admin.Models.ViewModels.Shop;
using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Shop;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using static System.Net.WebRequestMethods;

namespace MVC_Store.Areas.Admin.Controllers
{
    
    public class ShopController : Controller
    {
        // GET: Admin/Shop

        public ActionResult Categories()
        {
            //Обьявляем модель типа лист

            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {
                //Инициализируем модель данными

                categoryVMList = db.Categories
                    .ToArray()
                    .OrderBy(x => x.Sorting)
                    .Select(x => new CategoryVM(x))
                    .ToList();
            }
            //Возвращаем Лист в представление


            return View(categoryVMList);
        }

        [HttpPost]
        // POST: Admin/Shop
        public string AddNewCategory(string catName)

        {
            //обьявляем строковую переменную ID

            string id;


            using (Db db = new Db())

            {
                // проверяем имя категории на уникальность
                if (db.Categories.Any(x => x.Name == catName))
                    return "titletaken";
                // Инициализируем модель DTO
                CategoryDTO dto = new CategoryDTO();
                // добавляем данные в модель
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", " - ").ToLower();
                dto.Sorting = 100;
                // Сохранить
                db.Categories.Add(dto);
                db.SaveChanges();
                // Получаем ID
                id = dto.Id.ToString();

            }
            // возвращаем айди в представлении

            return id;

        }

        // создаем метод сортировки
        // POST: Admin/ырщз/ReorderCategories
        [HttpPost]
        public void ReorderCategories(int[] id)

        {
            using (Db db = new Db())
            {
                //реализуем счетчик
                int count = 1;
                // инициализируем модель данных
                CategoryDTO dto;
                //Устанавливаем сортировку для каждой страниц

                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;



                }
            }
        }

        // GET: Admin/Shop/DeleteCategory/id

        public ActionResult DeleteCategory(int id)


        {

            using (Db db = new Db())
            {
                //получаем модель категории

                CategoryDTO dto = db.Categories.Find(id);

                // удаляем Категорию
                db.Categories.Remove(dto);

                // сохранение изменений в базе
                db.SaveChanges();

            }

            //Добавляем сообщеение об успешном удалении 

            TempData["SM"] = "You have delete a category";
            // Переадресовывание пользователя


            return RedirectToAction("Categories");

        }

        // GET: Admin/Shop/RenameCategory/id
        [HttpPost]

        public string RenameCategory(string newCatName, int id)

        {
            using (Db db = new Db())

            {
                // проверим имя на уникальность
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "titletaken";
                // получаем все данные из БД (модельДТО)
                CategoryDTO dto = db.Categories.Find(id);

                // Редактируем модель ДТО

                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                //Сохранить изменения
                db.SaveChanges();
            }
            //Возвращаем результат(вернем слово-для проработки метода запуск)

            return "ok";

        }

        //
        [HttpGet]
        public ActionResult AddProduct()

        {
            //объявляем модель данных

            ProductVM model = new ProductVM();

            //Добавляем список категорий из базы в модель

            using (Db db = new Db())

            {
                model.Categories = new SelectList(db.Categories.ToList(), dataValueField: "id", dataTextField: "Name");
            }

            // Возвращаем в представление

            return View(model);

        }

        // POST: Admin/Shop/AddProduct

        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)

        {
            //проверяем модель на валидность

            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }

            }
            // Проверяем имя продукта на уникальность
            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {

                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "That product name is taken!");
                    return View(model);

                }

            }

            // Обьявляем переменную ProductID
            int id;

            // Инициализируем и сохраняем модель на основе Product DTO
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();
                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;
                
                db.Products.Add(product);
                db.SaveChanges();

                id = product.Id;
            }

            // Добавление сообщение в TemData

            TempData["SM"] = "You have added a product!";

            #region Upload Image

            // Создаем необходимые директории
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));
            var pathString1 = Path.Combine(originalDirectory.ToString(),"Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(),"Products\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(),"Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(),"Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(),"Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            // Проверка директории на наличие, если нет то создать



            // Проверка директории на наличие, если нет то создать
            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);
            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);
            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);
            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);
            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);


            // Был ли файл загружен
            if (file != null && file.ContentLength > 0)


            {
                // получаем расширение файла
                string ext = file.ContentType.ToLower();
                // проверяем расширение файла
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError(" ", "The image was not uploaded - wrong image extension");
                        return View(model);
                    }

                }

                // Объявляем переменную с именем изображения
                string imageName = file.FileName;

                // Сохраняем имя изображения в модель DTO
                using (Db db = new Db())

                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }

                // Назначаем пути к оригинальному и уменьшенному изображению
                var path = string.Format($"{pathString2}\\{imageName}");
                var path2 = string.Format($"{pathString3}\\{imageName}");

                // Сохраняем  original



                file.SaveAs(path);
                // Сохраняем и создаем mini
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200).Crop(1, 1);
                img.Save(path2);
            }



            #endregion
            //Переадресовать пользователя

            return RedirectToAction("AddProduct");


        }

        //создаем метод списка товаров

        // POST: Admin/Shop/Products
        [HttpGet]

        public ActionResult Products(int? page, int? catId)

        {

            // refresh ProductVM type List

            List<ProductVM> listOfProductVM;

            //Install nomber of page
            var pageNumber = page ?? 1;

            using (Db db = new Db())
            {
                //Init our list and fill data
                listOfProductVM = db.Products.ToArray()
                .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                .Select(x => new ProductVM(x))
                .ToList();

                //Fill categories data

                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //Install chosed category 
                ViewBag.SelectedCat = catId.ToString();

            }

            //Install page navigation
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);

            ViewBag.onePageOfProducts = onePageOfProducts;

            // Return in method

            return View(listOfProductVM);


        }


        [HttpGet]
        //GET: Admin/Shop/Products/id
        public ActionResult EditProduct(int id)

        {
            //Объявить модель ProductVM

            ProductVM model;


            using (Db db = new Db())


            {
                //Получаем продукт
                ProductDTO dto = db.Products.Find(id);

                //Проверяем доступен ли продукт

                if (dto == null)

                {
                    return Content("That product does not exist.");

                }

                // Инициализируем модель данными
                model = new ProductVM(dto);


                // Создаем список категорий

                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                // Получаем все изображения из галереи

                model.GalleryImages = Directory
                    .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));
            }

            // Вернуть модель в представление

            return View(model);

        }

        // POST: Admin/Shop/EditProducts
        [HttpPost]

        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)

        {

            //Получаем ID продукта

            int id = model.Id;

            //Заполняем список категориями и изображениями

            using (Db db = new Db())
            {

                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

            }
            model.GalleryImages = Directory
                     .EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                     .Select(fn => Path.GetFileName(fn));

            //Проверяем модель на валидность

            if (!ModelState.IsValid)

            {
                return View(model);
            }

            //Проверяем имя продукта на уникальность
            using (Db db = new Db())
            {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "That product is taken!");
                    return View(model);
                }
            }

            //// Обновляем продукт в базе данных
            using (Db db = new Db())

            {
                ProductDTO dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;

                db.SaveChanges();
            }
            // Установить сообщение в темпдата

            TempData["SM"] = "You have edited the product";




            #region Image Upload

            //загружен ли вообще вайл
            if (file != null && file.ContentLength > 0)
            {
                //получить расширение файла

                string ext = file.ContentType.ToLower();

                //Проверить расширение
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        ModelState.AddModelError(" ", "The image was not uploaded - wrong image extension");
                        return View(model);
                    }

                }
                //Установить пути для загрузки
                var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                //Удаляем существующие файлы и директории

                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (var file2 in di1.GetFiles())
                {
                    file2.Delete();

                }
                foreach (var file3 in di1.GetFiles())
                {
                    file3.Delete();

                }

                string imageName = file.FileName;
                //Сохраняем изображение
                using (Db db = new Db())

                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();

                }

                //Сохраняем оригинал и превью версии
                var path = string.Format($"{pathString1} \\ {imageName}");
                var path2 = string.Format($"{pathString2} \\ {imageName}");

                // Сохраняем  original



                file.SaveAs(path);
                // Сохраняем и создаем mini
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200).Crop(1, 1);
                img.Save(path2);

            }

            #endregion
            //переадресовать пользователя
            return RedirectToAction("EditProduct");


        }



        // POST: Admin/Shop/DeleteProduct

        public ActionResult DeleteProduct(int id)
        {


            // удаляем товар из базы данных
            using (Db db = new Db()) 
            
            { 
            ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);
                db.SaveChanges();            
            
            }
            // Удаляем директории 

            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

       
            var pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString))
                Directory.Delete(pathString, true);

            // Переадресовываем пользователя
            return RedirectToAction("Products");
        }
        //Добавляем изображение
        // POST: Admin/Shop/SaveGalleryImag/id

        public void SaveGalleryImages(int id)
        {

            // перебираем все полученные файлы

            foreach (string fileName in Request.Files)
            {
                //Инициализируем файлы
                HttpPostedFileBase file = Request.Files[fileName];
                // Проверяем на Null
                if (file != null && file.ContentLength > 0)
                {
                    // Назначаем все путти к директориям
                    var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));

                    string pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Galerry");
                    string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Galerry\\Thumbs");
                    // Назначаем пути изображений

                    var path = string.Format($"{pathString1}\\{file.FileName}");
                    var path2 = string.Format($"{pathString2}\\{file.FileName}");

                    // Сохраняем оригин изобр и копии

                    file.SaveAs(path);
                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200).Crop(1, 1);
                    img .Save(path2);

                }
            }
        
        
        }

        //Метод удаления изображений из гаалереи
        // POST: Admin/Shop/SaveGalleryImag/id/imageName
        public void DeleteImage(int id, string imageName)
        { 
            string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName );
            string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            if (System.IO.File.Exists(fullPath1))
                System.IO.File.Delete(fullPath1);

            if (System.IO.File.Exists(fullPath2))
                System.IO.File.Delete(fullPath2);
        }


        public ActionResult Orders()
        {
            // Init list of OrdersForAdminVM
            List<OrdersForAdminVM> ordersForAdmin = new List<OrdersForAdminVM>();

            using (Db db = new Db())
            {
                // Init list of OrderVM
                List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();

                // Loop through list of OrderVM
                foreach (var order in orders)
                {
                    // Init product dict
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();

                    // Declare total
                    decimal total = 0m;

                    // Init list of OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(X => X.OrderId == order.OrderId).ToList();

                    // Get username
                    UserDTO user = db.Users.Where(x => x.Id == order.UserId).FirstOrDefault();
                    string username = user.Username;

                    // Loop through list of OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsList)
                    {
                        // Get product
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        // Get product price
                        decimal price = product.Price;

                        // Get product name
                        string productName = product.Name;

                        // Add to product dict
                        productsAndQty.Add(productName, orderDetails.Quantity);

                        // Get total
                        total += orderDetails.Quantity * price;
                    }

                    // Add to ordersForAdminVM list
                    ordersForAdmin.Add(new OrdersForAdminVM()
                    {
                        OrderNumber = order.OrderId,
                        Username = username,
                        Total = total,
                        ProductsAndQty = productsAndQty,
                        CreatedAT = order.CreatedAT
                    });
                }
            }

            // Return view with OrdersForAdminVM list
            return View(ordersForAdmin);
        }



    }
}