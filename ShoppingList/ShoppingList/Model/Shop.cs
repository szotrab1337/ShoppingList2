using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using ShoppingList.Model;

namespace ShoppingList.Model
{
    public class Shop
    {
        [PrimaryKey, AutoIncrement]
        public int ShopID { get; set; }
        public string Name { get; set; }
        [Ignore]
        public int Number { get; set; }
        [Ignore]
        public int NumberOfPositions { get; set; }
    }
}
