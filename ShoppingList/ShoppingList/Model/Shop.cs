using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using ShoppingList.Model;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ShoppingList.Model
{
    public class Shop : INotifyPropertyChanged
    {
        [PrimaryKey, AutoIncrement]
        public int ShopID { get; set; }
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                NotifyPropertyChanged();
            }
        }
        private string _Name { get; set; }

        public bool IsPresent
        {
            get
            {
                return _IsPresent;
            }
            set
            {
                _IsPresent = value;
                NotifyPropertyChanged();
            }
        }
        private bool _IsPresent { get; set; }
        [Ignore]
        public int Number { get; set; }
        [Ignore]
        public string NumberOfPositions { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
