using MVC_Store.Models.Data;
using MVC_Store.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace MVC_Store.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {

            // Объявляем CartList
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            // проверить не пустая ли корзина
            if (cart.Count == 0 || Session["cart"] == null)

            {
                ViewBag.Message = "You cart is empty.";
                return View();
            }

            // Если не пуста то складываем и записываем в ViewBag
            decimal total = 0m;

            foreach (var item in cart)

            {

                total += item.Total;

            }

            ViewBag.GrandTotal = total;
            // Возвращаем лист в представление

            return View(cart);
        }

        public ActionResult CartPartial()

        {

            // обьявляем иодель CartVM

            CartVM model = new CartVM();

            // объявляем переменную количества
            int qty = 0;

            // объявляем переменную цены
            decimal price = 0m;
            // объявляем сессию корзины
            if (Session["cart"] != null)
            {
                // Получаем общее количество товаров и цену
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;

                }

                model.Quantity = qty;
                model.Price = price;

            }

            else

            {
                // или устанавливаем количество и цену в 0
                model.Quantity = 0;
                model.Price = 0m;

            }


            // вернуть частичное представление с моделью
            return PartialView("_CartPartial", model);

        }

        public ActionResult AddToCartPartial(int id)

        {
            //обьявляем лист, параметризированный типом CartVm
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();
            // обьявляем CartVM
            CartVM model = new CartVM();

            using (Db db = new Db())
            {
                // получаем продукт
                ProductDTO product = db.Products.Find(id);
                // Проверяем находится ли товар в корзине
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);
                //  Добавляем этот товар если нет
                if (productInCart == null)

                {
                    cart.Add(new CartVM()
                    {

                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });

                }
                // Если да то добовляем товар в корзину
                else
                {
                    productInCart.Quantity++;
                }
            }
            // Получить общее количесто товаров, цену и добовляем в модель

            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;
            // сохранить состояние корзины сессию

            Session["cart"] = cart;

            return PartialView("_AddToCartPartial", model);


        }

        public JsonResult IncrementProduct(int productId)
        {

            //name list cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;
            using (Db db = new Db())
            {
                // Get model CartVM from List
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);
                // Add qty
                model.Quantity++;
                // Savechanges data
                var result = new { qty = model.Quantity, price = model.Price };
                //return answer JSON

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DecrementProduct(int productId)

        {
            //name list cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;
            using (Db db = new Db())
            {
                // Get model CartVM from List
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);
                // Add qty
                if (model.Quantity > 1)
                    model.Quantity--;
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }

                // Savechanges data
                var result = new { qty = model.Quantity, price = model.Price };
                //return answer JSON

                return Json(result, JsonRequestBehavior.AllowGet);

            }
        }

        public void RemoveProduct(int productId)

        {
            //name list cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;
            using (Db db = new Db())
            {
                // Get model CartVM from List
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);


                cart.Remove(model);
            }


        }


        public ActionResult PaypalPartial()
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            return PartialView(cart);
        }


        [HttpPost]
        public void PlaceOrder()
        {
            // Get cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            // Get username
            string userName = User.Identity.Name;

            int orderId = 0;

            using (Db db = new Db())
            {
                // Init OrderDTO
                OrderDTO orderDTO = new OrderDTO();

                // Get user id
                var q = db.Users.FirstOrDefault(x => x.Username == userName);
                int userId = q.Id;

                // Add to OrderDTO and save
                orderDTO.UserId = userId;
                orderDTO.CreatedAT = DateTime.Now;

                db.Orders.Add(orderDTO);

                db.SaveChanges();

                // Get inserted id
                orderId = orderDTO.OrderId;

                // Init OrderDetailsDTO
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                // Add to OrderDetailsDTO
                foreach (var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDTO);

                    db.SaveChanges();
                }
            }

            // Email admin
            var client = new SmtpClient("mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("21f57cbb94cf88", "e9d7055c69f02d"),
                EnableSsl = true
            };
            client.Send("shop@example.com", "admin@example.com", "New Order",
                $"You have a new order. Order number:  {orderId}");

            // Reset session
            Session["cart"] = null;
        }
    }
}