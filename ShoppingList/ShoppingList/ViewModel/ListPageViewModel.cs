using ShoppingList.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace ShoppingList.ViewModel
{
    public class ListPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        public ListPageViewModel(INavigation navigation, Shop shop)
        {
            this.Navigation = navigation;
            this.Shop = shop;

            Title = Shop.Name;
        }

        public INavigation Navigation { get; set; }
        public Shop Shop { get; set; }

        public string Title
        {
            get { return _Title; }
            set { _Title = value; OnPropertyChanged("Title"); }
        }
        private string _Title;
    }
}
