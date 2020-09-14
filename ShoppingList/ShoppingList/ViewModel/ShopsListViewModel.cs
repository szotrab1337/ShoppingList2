using Acr.UserDialogs;
using ShoppingList.Model;
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

        public ShopsListViewModel()
        {
            Shops = new ObservableCollection<Shop>();

            LoadShops();

            DeleteShop = new Command(DeleteShopAction);
        }

        public ICommand DeleteShop { get; set; }

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
            Shops.Clear();
            List<Shop> allShops = await App.Database.GetShopsAsync();

            for (int i = 0; i < allShops.Count; i++)
            {
                allShops[i].Number = i + 1;
                Shops.Add(allShops[i]);
            }
        }

        public async void DeleteShopAction()
        {
            //UserDialogs.Instance.Alert("Hej", SelectedShop.Name + " " + SelectedShop.ShopID, "OK");

            //await App.Database.DeleteShopAsync(SelectedShop);
            //LoadShops();
            //SelectedShop = null;
        }

        //public async void DeleteClicked(object sender, EventArgs e)
        //{           
        //    var mi = ((MenuItem)sender);
        //    int id = Convert.ToInt32(mi.CommandParameter);
        //    var shop = Shops.Where(x => x.ShopID == id).FirstOrDefault();
        //    await App.Database.DeleteShopAsync(SelectedShop);
        //    LoadShops();
        //}
    }
}
