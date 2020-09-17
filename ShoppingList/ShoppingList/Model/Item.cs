using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace ShoppingList.Model
{
    public class Item : INotifyPropertyChanged
    {
        [PrimaryKey, AutoIncrement]
        public int ItemID { get; set; }
        public int ShopID { get; set; }
        public string Name { get; set; }
        public bool IsChecked
        {
            get { return _IsChecked; }
            set
            {
                if (_IsChecked != value)
                {
                    _IsChecked = value;
                    NotifyPropertyChanged();
                }
            }
        }
        [Ignore]
        public TextDecorations TextDec
        {
            get
            {
                return _TextDec;
            }
            set
            {
                _TextDec = value;
                NotifyPropertyChanged();              
            }
        }

        [Ignore]
        private TextDecorations _TextDec { get; set; }
        public double Quantity { get; set; }
        [Ignore]
        public int Number { get; set; }
        [Ignore]
        private bool _IsChecked { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
