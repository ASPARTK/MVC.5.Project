using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;

namespace MVC_Store.Areas.Admin.Controllers
{
   
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {



            //Объявляем список для представления
            List<PageVM> pageList;

            //Инициализируем список (DB)
            using (Db db = new Db())
            {
                pageList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
            }

            //Возвращаем список в представление
            return View(pageList);


        }

        // GET: Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()

        {
            return View();
        }
        // POST: Admin/Pages/AddPage/
        [HttpPost]
        public ActionResult AddPage(PageVM model)

        {
            //Проверка модели на валидность\

            if (!ModelState.IsValid)

            {
                return View(model);

            }

            using (Db db = new Db())
            {

                //Обявляем переменную для краткого описания (Slug)
                string slug;

                //Инициализируем класс PageDTO
                PagesDTO dto = new PagesDTO();


                //Присваевываем заголовок модели
                dto.Title = model.Title.ToUpper();

                //Проверяем есть ли описание, если нет, присваевываем его
                if (string.IsNullOrWhiteSpace(model.Slug))

                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();

                }

                // Проверка на уникальность(заголовок и краткое описание уникальны)

                if (db.Pages.Any(x => x.Title == model.Title))

                {
                    ModelState.AddModelError("", "That title alredy exist.");
                    return View(model);
                }

                else if (db.Pages.Any(x => x.Slug == model.Slug))

                {

                    ModelState.AddModelError("", "That slug alredy exist.");
                    return View(model);


                }

                // Присваиваем оставшееся значение модели

                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;

                // Сохраняем модель в базу данных
                db.Pages.Add(dto);
                db.SaveChanges();
            }

            // Передаем сообщение через TempData

            TempData["SM"] = "You have added a new page!";

            // Переадресовываем на метод INDEX

            return RedirectToAction("Index");
        }

        // GET: Admin/Pages/EditPage/id
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            ///Объявляем модель PageVM
            PageVM model;

            using (Db db = new Db())
            {
                ///Получаем страницу
                PagesDTO dto = db.Pages.Find(id);

                ///Проверяем достпна ли страница
                if (dto == null)
                {

                    return Content("Page dose not exit.");

                }
                ///Инициализируем модель данных
                model = new PageVM(dto);

            }
            ///Возвращаем модель в представленние данных
            return View(model);


        }

        // POST: Admin/Pages/AddPage

        public ActionResult EditPage(PageVM model)

        {
            // проверяем модель на валидность 

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                //получайм  id страницы

                int id = model.Id;

                // Объявим переменную краткого заголовка

                string slug = "home";

                // Получаем информацию страницу (id)

                PagesDTO dto = db.Pages.Find(id);

                // Присвоить название из модели в DTO

                dto.Title = model.Title;

                // Проверяем краткий заголовок и присваевыаем его, если это необходимо

                if (model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))

                    {

                        slug = model.Title.Replace(" ", "-").ToLower();

                    }

                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();

                    }
                }

                // Проверить краткий заголовок на тайтл на уникальность

                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("", "That title alredy exist.");
                    return View(model);

                }

                else if (db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {

                    ModelState.AddModelError("", "That slug alredy exist.");
                    return View(model);

                }

                // присваиваем остальные значения в класс ДТО

                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;

                // Сохраняем в БД

                db.SaveChanges();
            }
            // Сообщение пользователю в Темп дате

            TempData["SM"] = "You have edited the page";

            // Переадресовать пользователя обратно

            return RedirectToAction("EditPage");
        }

        // GET: Admin/Pages/EditPage/id

        public ActionResult PageDetails(int id)
        {

            // обьявим модель pageVM

            PageVM model;


            using (Db db = new Db())
            {
                PagesDTO dto = db.Pages.Find(id);

                
                // Подтверждаем что страница доступна
                if (dto == null)
                {

                    return Content("The page does not exist.");

                }

                // Присваевыем модели инф из базы
                model = new PageVM(dto);
            }
            // Возвращаем модель о представлении 

            return View("PageDetails", model);
        }

        // GET: Admin/Pages/DeletePage/id

        public ActionResult DeletePage(int id)


        {

            using (Db db = new Db())
            {
                //получаем страницу

                PagesDTO dto = db.Pages.Find(id);

                // удаляем страницу
                db.Pages.Remove(dto);

                // сохранение изменений в базе
                db.SaveChanges();

            }

            //Добавляем сообщеение об успешном удалении 

            TempData["SM"] = "You have delete a page";
            // Переадресовывание пользователя


            return RedirectToAction("Index");

        }

        // создаем метод сортировки
        // GET: Admin/Pages/ReorderPages
        [HttpPost]
        public void ReorderPages(int[] id)

        {
            using (Db db = new Db())
            {
                //реализуем счетчик
                int count = 1;
                // инициализируем модель данных
                PagesDTO dto;
                //Устанавливаем сортировку для каждой страниц

                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;



                }
            }
        }

        // GET: Admin/Pages/EdiSidebar

        public ActionResult EditSidebar()
        {
            //обьявляем модеель

            SidebarVM model;

            using (Db db = new Db())
            {
                //Получаем данные из DTO

                SidebarDTO dto = db.Sidebars.Find(1); //говнокод Жесткие значения

                // Заполняем модель
                model = new SidebarVM(dto);
            }
            // вернуть модль                     

            return View(model);

        }

        // GET: Admin/Pages/EdiSidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
           
            using (Db db = new Db())

            {
                ////Получаем данные из DTO
                SidebarDTO dto = db.Sidebars.Find(1); //говнокод Жесткие значения

                ////Присвоить  данные в тело (в свойство Боди)

                dto.Body = model.Body;

                /// сохранить 
                db.SaveChanges();
            }
            /// присвоить в темп дата
            TempData["SM"] = "You have edited the Sidebar";
            /// переадресация пользователя Администраторов
            
            return RedirectToAction("EditSidebar");
        
        }

    }

}