using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RestApi.Model
{
    public class SqlItem
    {
        [Key]
        public int ItemID { get; set; }
        public int ShopID { get; set; }
        public string Name { get; set; }
        public double Quantity { get; set; }

        [NotMapped]
        public virtual SqlShop SqlShop { get; set; }
    }
}
