using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace ShoppingList.Model
{
    public class SqlItem
    {
        public int ItemID { get; set; }
        public int ShopID { get; set; }
        public string Name { get; set; }
        public double Quantity { get; set; }

        public virtual SqlShop SqlShop { get; set; }
    }
}
