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
    public class AddShopPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        public AddShopPageViewModel(INavigation navigation)
        {
            this.Navigation = navigation;
            AddShopCommand = new Command(AddShopAction);
        }

        public ICommand AddShopCommand { get; set; }

        public string ShopName
        {
            get { return _ShopName; }
            set { _ShopName = value; OnPropertyChanged("ShopName"); }
        }
        private string _ShopName;

        public INavigation Navigation { get; set; }


        public async void AddShopAction()
        {
            try
            {
                if(!string.IsNullOrEmpty(ShopName))
                {
                    await App.Database.SaveShopAsync(new Shop { Name = ShopName });
                    UserDialogs.Instance.Toast("Dodano nowy sklep.");
                    MessagingCenter.Send(this, "Refresh");
                    await Navigation.PopToRootAsync();
                }
                else
                {
                    UserDialogs.Instance.Alert("Wprowadź poprawną nazwę sklepu!", "Błąd", "OK");
                }
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }

    }
}
