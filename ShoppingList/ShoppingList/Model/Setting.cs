using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShoppingList.Model
{
    public class Setting
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public bool SortByName { get; set; }
    }
}
