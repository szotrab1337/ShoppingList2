using Acr.UserDialogs;
using ShoppingList.Model;
using ShoppingList.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
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

            AddNewItemCommand = new Command(AddNewItemAction);
        }

        public INavigation Navigation { get; set; }

        public ICommand AddNewItemCommand { get; set; }
        public Shop Shop { get; set; }

        public string Title
        {
            get { return _Title; }
            set { _Title = value; OnPropertyChanged("Title"); }
        }
        private string _Title;

        public async void AddNewItemAction()
        {
            try
            {
                await Navigation.PushAsync(new AddNewItemPage());
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }
    }
}
