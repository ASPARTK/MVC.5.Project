using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MVC_Store.Models.Data
{
    [Table("TblOrders")]
    public class OrderDTO
    {
        [Key]
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAT { get; set; }

        [ForeignKey("UserId")]
        public virtual UserDTO Users { get; set; }

    }
}