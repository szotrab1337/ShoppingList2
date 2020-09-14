using Acr.UserDialogs;
using ShoppingList.Model;
using ShoppingList.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace ShoppingList.ViewModel
{
    public class ShopsListViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //protected void OnPropertyChanged(string propertyName)
        //{
        //    OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        //}
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

        public ShopsListViewModel(INavigation navigation)
        {
            this.Navigation = navigation;

            Shops = new ObservableCollection<Shop>();
            OpneNewShopCommand = new Command(OpneNewShopAction);

            MessagingCenter.Subscribe<AddShopPageViewModel>(this, "Refresh", (LoadAgain) => 
            {
                LoadShops();
            });
            MessagingCenter.Subscribe<EditShopPageViewModel>(this, "Refresh", (LoadAgain) => 
            {
                LoadShops();
            });

            LoadShops();
        }

        public INavigation Navigation { get; set; }
        public ICommand OpneNewShopCommand { get; set; }
        public ObservableCollection<Shop> Shops
        {
            get { return _Shops; }
            set { _Shops = value; OnPropertyChanged("Shops"); }
        }
        private ObservableCollection<Shop> _Shops;
        public Shop SelectedShop
        {
            get { return _SelectedShop; }
            set { _SelectedShop = value; OnPropertyChanged("SelectedShop"); }
        }
        private Shop _SelectedShop;


        public async void LoadShops()
        {
            //await App.Database.SaveShopAsync(new Shop { Name = "Lidl1" });
            //await App.Database.SaveShopAsync(new Shop { Name = "Lidl2" });
            //await App.Database.SaveShopAsync(new Shop { Name = "Lidl3" });
            //await App.Database.SaveShopAsync(new Shop { Name = "Lidl4" });
            //await App.Database.SaveShopAsync(new Shop { Name = "Lidl5" });
            //await App.Database.SaveShopAsync(new Shop { Name = "Lidl6" });
            //await App.Database.SaveShopAsync(new Shop { Name = "Lidl7" });

            try
            {
                Shops.Clear();
                List<Shop> allShops = await App.Database.GetShopsAsync();

                for (int i = 0; i < allShops.Count; i++)
                {
                    allShops[i].Number = i + 1;
                    Shops.Add(allShops[i]);
                }
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }

        public void Renumber()
        {
            try
            {
                for (int i = 0; i < Shops.Count; i++)
                {
                    Shops[i].Number = i + 1;
                }

                ObservableCollection<Shop> tempShops = Shops;
                Shops = null;
                Shops = tempShops;
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }

        public async void OpneNewShopAction()
        {
            try
            {
                await Navigation.PushAsync(new AddShopPage());
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }
    }
}
