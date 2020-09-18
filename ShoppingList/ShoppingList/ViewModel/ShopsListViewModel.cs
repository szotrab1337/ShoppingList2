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
            EditCommand = new Command(EditAction);
            DeleteCommand = new Command(DeleteAction);
           // OpenShopCommand = new Command(OpenShopAction);

            MessagingCenter.Subscribe<AddShopPageViewModel>(this, "Refresh", (LoadAgain) => 
            {
                LoadShops();
            });
            MessagingCenter.Subscribe<EditShopPageViewModel>(this, "Refresh", (LoadAgain) => 
            {
                LoadShops();
            });

            MessagingCenter.Subscribe<AddNewItemPageViewModel>(this, "Refresh", (LoadAgain) =>
            {
                LoadShops();
            });
            
            MessagingCenter.Subscribe<ListPageViewModel>(this, "Refresh", (LoadAgain) =>
            {
                LoadShops();
            });

            LoadShops();
        }

        public INavigation Navigation { get; set; }
        public ICommand OpneNewShopCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
       // public ICommand OpenShopCommand { get; set; }
        public ObservableCollection<Shop> Shops
        {
            get { return _Shops; }
            set { _Shops = value; OnPropertyChanged("Shops"); }
        }
        private ObservableCollection<Shop> _Shops;

        public Shop SelectedShop
        {
            get { return _SelectedShop; }
            set
            {
                _SelectedShop = value; OnPropertyChanged("SelectedShop");
                if (SelectedShop != null)
                {
                    OpenList();
                }
            }
        }
        private Shop _SelectedShop;

        public async void OpenList()
        {
            try
            {
                await Navigation.PushAsync(new ListPage(SelectedShop));
                LoadShops();
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }


        //public async void OpenShopAction(object sender)
        //{
        //    try
        //    {
        //        Shop shop = (Shop)sender;
        //        await Navigation.PushAsync(new ListPage(shop));
        //        LoadShops();
        //    }
        //    catch (Exception ex)
        //    {
        //        UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
        //    }
        //}

        public async void LoadShops()
        {
            try
            {
                Shops.Clear();
                List<Shop> allShops = await App.Database.GetShopsAsync();

                for (int i = 0; i < allShops.Count; i++)
                {
                    allShops[i].Number = i + 1;
                    string quantity = (await App.Database.GetItemsByShopAsync(allShops[i].ShopID)).Count.ToString();

                    switch (quantity)
                    {
                        case "1":
                            quantity = quantity + " pozycja";
                            break;
                        case "2":
                        case "3":
                        case "4":
                            quantity = quantity + " pozycje";
                            break;
                        default:
                            quantity = quantity + " pozycji";
                            break;
                    }
                    allShops[i].NumberOfPositions = quantity;
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
                //await Navigation.PushAsync(new AddShopPage());
                PromptResult result = await UserDialogs.Instance.PromptAsync(new PromptConfig
                {
                    Message = "Wprowadź nazwę sklepu",
                    CancelText = "Anuluj",
                    InputType = InputType.Name,
                    IsCancellable = true,
                    OkText = "Dodaj",
                    Placeholder = "wprowadź nazwę",
                    Title = "Nowy sklep",

                });

                if (result.Ok)
                {
                    if (!string.IsNullOrEmpty(result.Text))
                    {
                        //string text = result.Text.First().ToString().ToUpper() + result.Text.Substring(1);
                        await App.Database.SaveShopAsync(new Shop { Name = result.Text.First().ToString().ToUpper() + result.Text.Substring(1) });
                        Shop shop = (await App.Database.GetShopsAsync()).LastOrDefault();
                        shop.Number = Shops.Count + 1;
                        shop.NumberOfPositions = "0 pozycji";
                        Shops.Add(shop);
                        UserDialogs.Instance.Toast("Dodano nowy sklep.");
                        //MessagingCenter.Send(this, "Refresh");
                        //await Navigation.PopToRootAsync();
                    }
                    else
                    {
                        UserDialogs.Instance.Alert("Wprowadź poprawną nazwę sklepu!", "Błąd", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }

        public async void EditAction(object sender)
        {
            try
            {
                Shop shop = (Shop)sender;
                await Navigation.PushAsync(new EditShopPage(shop.ShopID));
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }

        public async void DeleteAction(object sender)
        {
            try
            {
                Shop shop = (Shop)sender;
                var result = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig
                {
                    Message = "Czy na pewno chcesz usunąć sklep " + shop.Name + "?",
                    OkText = "Tak",
                    CancelText = "Nie",
                    Title = "Potwierdzenie"
                });

                if (result)
                {
                    await App.Database.DeleteShopAsync(shop);
                    UserDialogs.Instance.Toast("Usunięto sklep.");
                    Shops.Remove(shop);
                    Renumber();
                }
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }
    }
}
