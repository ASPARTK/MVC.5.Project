using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Account;
using MVC_Store.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MVC_Store.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }

        // GET: account/crate-account
        [ActionName("create-account")]

        [HttpGet]
        public ActionResult CreateAccount() 
        
        
        {
            return View("CreateAccount");
        
        }

        // POST: account/crate-account
        [ActionName("create-account")]

        [HttpPost]
        public ActionResult CreateAccount(UserVM model)


        {
            //проверяеем модель на валидность

            if(!ModelState.IsValid)
                return View("CreateAccount", model);

            //проверяем соответствие пароля
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Password do not match!");
                return View("CreateAccount", model);
                
            }
            using (Db db = new Db())
            {
                // пролверяем имя на уникальность
                if (db.Users.Any(x => x.Username.Equals(model.Username)))
                {
                    ModelState.AddModelError("", $"Username {model.Username} is taken.");
                    model.Username = "";
                    return View("CreateAccount", model);

                }
                //Создаем экземпляр класса ЮзерДТО
                UserDTO userDTO = new UserDTO()
                {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        EmailAdress = model.EmailAdress,
                        Username = model.Username,
                        Password = model.Password


                };

                //Добавляем данные в модель
                db.Users.Add(userDTO);
                // Сохраняем данные
                db.SaveChanges(); 
                //Добавляем роль пользователя
                int id = userDTO.Id;
                UserRoleDTO userRoleDTO = new UserRoleDTO()
                { 
                    UserId = id,
                        RoleId = 2

            };
                db.UserRoles.Add(userRoleDTO);
                db.SaveChanges();

            }
            //Записываем сообщение в Темп
            TempData["SM"] = "You are now registred and can login";
            //Переадресовываем


            return RedirectToAction("Login");

        }

        [HttpGet]
        public ActionResult Login() 
        {
            // Подтвердить, что пользователь авторизован

            string userName = User.Identity.Name;

            if (!string.IsNullOrEmpty(userName))

                return RedirectToAction("user-profile");
            
            // Проверяем на валидность
            // Возвращаем представление

            return View();
        }

        [HttpPost]

        public ActionResult Login(LoginUserVM model) 
        
        
        {
            //Проверить модель на валидность

            if (!ModelState.IsValid)
                return View(model);

            // Проверить пользователя на валидность
            bool isValid = false;

            using (Db db = new Db())

            { 
                if (db.Users.Any(x => x.Username.Equals(model.Username)&& x.Password.Equals(model.Password)))
            
                 isValid = true;

                if (!isValid)

                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    return View();
                }

                else 
                {
                    FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                    return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
                }
            }


        }

        //GET: /account/logout
       
        public ActionResult Logout()
        { 
        FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        
        }

        [Authorize]

        public ActionResult UserNavPartial()
        {
            //Получаем имя пользователя
            string userName = User.Identity.Name;
            // Обьявляем модель
            UserNavPartialVM model;
            using (Db db = new Db())
            // Получаем пользователя
            {
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == userName);
                // Заполнить модель данными из ДТО
                model = new UserNavPartialVM()
                { 
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                                    
                };
            }
            // Возвращаем частичное представление

            return PartialView(model);
        }

        [HttpGet]
        [ActionName("user-profile")]

        public ActionResult UserProfile()
        {
            string username = User.Identity.Name;

            // Declare model
            UserProfileVM model;

            using (Db db = new Db())
            {
                // Get user
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);

                // Build model
                model = new UserProfileVM(dto);
            }

            // Return view with model
            return View("UserProfile", model);

        }

       
        // POST: /account/user-profile
        [HttpPost]
        [ActionName("user-profile")]
        
        public ActionResult UserProfile(UserProfileVM model)
        {

            bool userNameIsChanged = false;
            // Check model state
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            // Check if passwords match if need be
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Passwords do not match.");
                    return View("UserProfile", model);
                }
            }

            using (Db db = new Db())
            {
                // Get username
                string userName = User.Identity.Name;

                if (userName != model.Username)
                {
                    userName = model.Username;
                    userNameIsChanged = true;  
                
                
                }

                // Make sure username is unique
                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.Username == userName))
                {
                    ModelState.AddModelError("", $"Username  {model.Username} already exists.");
                    model.Username = "";
                    return View("UserProfile", model);
                }

                // Edit DTO
                UserDTO dto = db.Users.Find(model.Id);

                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAdress = model.EmailAdress;
                dto.Username = model.Username;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }

                // Save
                db.SaveChanges();
            }

            // Set TempData message
            TempData["SM"] = "You have edited your profile!";
            if (!userNameIsChanged)
                // Redirect
                return View("UserProfile", model);

            else

                return RedirectToAction("Logout");
            
        }

        [Authorize(Roles="User")]
        public ActionResult Orders()
        {
            // Init list of OrdersForUserVM
            List<OrdersForUserVM> ordersForUser = new List<OrdersForUserVM>();

            using (Db db = new Db())
            {
                // Get user id
                UserDTO user = db.Users.Where(x => x.Username == User.Identity.Name).FirstOrDefault();
                int userId = user.Id;

                // Init list of OrderVM
                List<OrderVM> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM(x)).ToList();

                // Loop through list of OrderVM
                foreach (var order in orders)
                {
                    // Init products dict
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();

                    // Declare total
                    decimal total = 0m;

                    // Init list of OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsDTO = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    // Loop though list of OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsDTO)
                    {
                        // Get product
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        // Get product price
                        decimal price = product.Price;

                        // Get product name
                        string productName = product.Name;

                        // Add to products dict
                        productsAndQty.Add(productName, orderDetails.Quantity);

                        // Get total
                        total += orderDetails.Quantity * price;
                    }

                    // Add to OrdersForUserVM list
                    ordersForUser.Add(new OrdersForUserVM()
                    {
                        OrderNumber = order.OrderId,
                        Total = total,
                        ProductsAndQty = productsAndQty,
                        CreatedAT = order.CreatedAT
                    });
                }

            }

            // Return view with list of OrdersForUserVM
            return View(ordersForUser);
        }

    }
}

