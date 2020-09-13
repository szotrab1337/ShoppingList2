using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShoppingList.Model
{
    public class Item
    {
        [PrimaryKey, AutoIncrement]
        public int ItemID { get; set; }
        public string Name { get; set; }
        public bool IsChecked { get; set; }
        public double Quantity { get; set; }
        [Ignore]
        public int Number { get; set; }
    }
}
