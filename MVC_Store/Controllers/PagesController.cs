using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVC_Store.Controllers
{
    public class PagesController : Controller
    {
        // GET: Index/{page}
        
        public ActionResult Index( string page = "")
        {
            //получаем/устанавлеваем краткий заголового(SLUG)

            if (page == "")
                page = "home";

            // Обьявляем модель и класс DTO

            PageVM model;
            PagesDTO dto;

            // проверяем доступна ли страница

            using (Db db = new Db())
                    {
                if (!db.Pages.Any(x => x.Slug.Equals(page)))
                    return RedirectToAction("Index", new { page = "" });
                                    
            }

            // Получаем DTO Страницы
            using (Db db = new Db())
            {
               dto= db.Pages.Where(x => x.Slug == page).FirstOrDefault();

            }

            //Устанавливаем заголовки страницы (TITLE)

            ViewBag.PageTitle = dto.Title;

            // Проверяем боковую панель 

            if (dto.HasSidebar == true)

            {
                ViewBag.Sidebar = "Yes";

            }

            else
            {
                ViewBag.Sidebar = "No";
            }
            // Заполняем модель нашими данными

            model = new PageVM(dto);


            // Вернуть представление с моделями

            return View(model);
        }

        public ActionResult PagesMenuPartial()
        
        {
            //init List PageVM
            List<PageVM> pageVMList;
            // get all page, ex. home
            using (Db db = new Db())
            {
                pageVMList = db.Pages.ToArray().OrderBy(x => x.Sorting).Where(x => x.Slug != "home")
                        .Select(x => new PageVM(x)).ToList(); 
            }
                // Return with list Data       
                return PartialView("_PagesMenuPartial", pageVMList);
        }

        public ActionResult SidebarPartial()
        {
            // обьявляем модель
            SidebarVM model;
            using (Db db = new Db())
            {
                SidebarDTO dto = db.Sidebars.Find(1);

                model = new SidebarVM(dto);
            
            }
            return PartialView("_PagesMenuPartial", model);
        }
    }


}