using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShoppingList.Model
{
    public class Shop
    {
        [PrimaryKey, AutoIncrement]
        public int ShopID { get; set; }
        public string Name { get; set; }
        [Ignore]
        public int Number { get; set; }
    }
}
