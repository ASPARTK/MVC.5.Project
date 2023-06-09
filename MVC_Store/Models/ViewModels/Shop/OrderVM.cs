﻿using MVC_Store.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_Store.Models.ViewModels.Shop
{
    public class OrderVM
    {
        public OrderVM()
        {
        }
      

        public OrderVM(OrderDTO row)
        {
            OrderId = row.OrderId;
            UserId = row.UserId;
            CreatedAT = row.CreatedAT;
        }

        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAT { get; set; }
    }
}