using Acr.UserDialogs;
using ShoppingList.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace ShoppingList.ViewModel
{
    public class EditShopPageViewModel : INotifyPropertyChanged
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

        public EditShopPageViewModel(INavigation navigation, int id)
        {
            this.Navigation = navigation;
            this.ShopID = id;

            SaveShopCommand = new Command(SaveShopAction);

            Prepare();
        }

        public INavigation Navigation { get; set; }
        public ICommand SaveShopCommand { get; set; }

        public string ShopName
        {
            get { return _ShopName; }
            set { _ShopName = value; OnPropertyChanged("ShopName"); }
        }
        private string _ShopName;

        public int ShopID { get; set; }

        public async void Prepare()
        {
            try
            {
                Shop shop = await App.Database.GetShopByID(ShopID);
                ShopName = shop.Name;
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }

        public async void SaveShopAction()
        {
            try
            {
                if (!string.IsNullOrEmpty(ShopName))
                {
                    Shop shop = await App.Database.GetShopByID(ShopID);
                    shop.Name = ShopName;
                    await App.Database.UpdateShopAsync(shop);
                    UserDialogs.Instance.Toast("Dokonano edycji sklepu.");
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
