using Acr.UserDialogs;
using Newtonsoft.Json;
using ShoppingList.Model;
using ShoppingList.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
                        ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, sslPolicyErrors) => true;

            this.Navigation = navigation;

            Shops = new ObservableCollection<Shop>();
            OpenNewShopCommand = new Command(OpenNewShopAcion);
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
            //TestJson();
        }

        //public async void TestJson()
        //{
        //    Uri uri = new Uri(string.Format("http://192.168.1.100:7500/items", string.Empty));
        //    HttpClient httpClient = new HttpClient();
        //    ObservableCollection<SqlItem> SqlItems = new ObservableCollection<SqlItem>();

        //    var resposne = await httpClient.GetStringAsync(uri);

        //    //var content = await resposne.Content.ReadAsStringAsync();
        //    List<SqlItem> sqlItems = JsonConvert.DeserializeObject<List<SqlItem>>(resposne);
        //    UserDialogs.Instance.Alert(sqlItems[0].Name.ToString());
        //}

        public interface IHTTPClientHandlerCreationService
        {
            HttpClientHandler GetInsecureHandler();
        }

        public INavigation Navigation { get; set; }
        public ICommand OpenNewShopCommand { get; set; }
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
                UserDialogs.Instance.ShowLoading("Ładowanie...", MaskType.Black);
                await Loading();
                UserDialogs.Instance.HideLoading();
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }

        public async Task Loading()
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

        public async void OpenNewShopAcion()
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
                        Shop checkShop = await App.Database.GetShopByName(result.Text.First().ToString().ToUpper() + result.Text.Substring(1));
                        if(checkShop!=null)
                        {
                            UserDialogs.Instance.Alert("Sklep o podanej nazwie już istnieje!", "Błąd", "OK");
                            return;
                        }

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
                //await Navigation.PushAsync(new EditShopPage(shop.ShopID));
                PromptResult result = await UserDialogs.Instance.PromptAsync(new PromptConfig
                {
                    Message = "Wprowadź nazwę sklepu",
                    CancelText = "Anuluj",
                    InputType = InputType.Name,
                    IsCancellable = true,
                    OkText = "Zapisz",
                    Placeholder = "wprowadź nazwę",
                    Title = "Edycja sklepu",
                    Text = shop.Name
                });

                if (result.Ok)
                {
                    if (!string.IsNullOrEmpty(result.Text))
                    {
                        //string text = result.Text.First().ToString().ToUpper() + result.Text.Substring(1);
                        Shop checkShop = await App.Database.GetShopByName(result.Text.First().ToString().ToUpper() + result.Text.Substring(1));
                        if (checkShop != null)
                        {
                            UserDialogs.Instance.Alert("Sklep o podanej nazwie już istnieje!", "Błąd", "OK");
                            return;
                        }
                        shop.Name = result.Text.First().ToString().ToUpper() + result.Text.Substring(1);
                        Shops.Where(x => x.ShopID == shop.ShopID).FirstOrDefault().Name = result.Text.First().ToString().ToUpper() + result.Text.Substring(1);
                        await App.Database.UpdateShopAsync(shop);
                        UserDialogs.Instance.Toast("Edytowano sklep.");
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
                    UserDialogs.Instance.ShowLoading("Usuwanie...", MaskType.Black);
                    await App.Database.DeleteShopAsync(shop);
                    UserDialogs.Instance.Toast("Usunięto sklep.");
                    Shops.Remove(shop);
                    Renumber();

                    List<Item> Items = await App.Database.GetItemsByShopAsync(shop.ShopID);
                    foreach (Item item in Items)
                    {
                        await App.Database.DeleteItemAsync(item);
                    }
                    UserDialogs.Instance.HideLoading();
                }
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }
    }
}
