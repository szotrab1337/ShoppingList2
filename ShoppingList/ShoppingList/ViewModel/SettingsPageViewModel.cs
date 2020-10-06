using Acr.UserDialogs;
using Newtonsoft.Json;
using ShoppingList.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ShoppingList.ViewModel
{
    public class SettingsPageViewModel : INotifyPropertyChanged
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
        public Setting Setting { get; set; }
        public SettingsPageViewModel()
        {
            Setting Setting = new Setting();
            LoadSettings();

            DeleteAllShopsCommand = new Command(DeleteAllShopsAction);
            ExportCommand = new Command(ExportAction);
            ImportCommand = new Command(ImportAction);
            DeleteFromCloudCommand = new Command(DeleteFromCloudAction);
        }

        public ICommand DeleteAllShopsCommand { get; set; }
        public ICommand ExportCommand { get; set; }
        public ICommand ImportCommand { get; set; }
        public ICommand DeleteFromCloudCommand { get; set; }

        public bool SortIsToggled
        {
            get { return _SortIsToggled; }
            set
            {
                _SortIsToggled = value;
                OnPropertyChanged("SortIsToggled");
                UpdateSetting();
            }
        }
        private bool _SortIsToggled;


        public async void LoadSettings()
        {
            try
            {
                Setting = await App.Database.GetSettingsAsync();
                if (Setting == null)
                {
                    await App.Database.InsertSettingAsync(new Setting
                    {
                        SortByName = true
                    });
                    Setting = await App.Database.GetSettingsAsync();
                }
                SortIsToggled = (await App.Database.GetSettingsAsync()).SortByName;
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }

        public async void UpdateSetting()
        {
            try
            {
                Setting.SortByName = SortIsToggled;
                await App.Database.UpdateSettingAsync(Setting);
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }

        public async void DeleteAllShopsAction()
        {
            try
            {
                var result = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig
                {
                    Message = "Czy na pewno chcesz usunąć wszystkie sklepy?",
                    OkText = "Tak",
                    CancelText = "Nie",
                    Title = "Potwierdzenie"
                });

                if (result)
                {
                    UserDialogs.Instance.ShowLoading("Usuwanie...", MaskType.Black);
                    List<Shop> Shops = await App.Database.GetShopsAsync();

                    foreach (Shop shop in Shops)
                    {
                        await App.Database.DeleteShopAsync(shop);

                        List<Item> Items = await App.Database.GetItemsByShopAsync(shop.ShopID);
                        foreach (Item item in Items)
                        {
                            await App.Database.DeleteItemAsync(item);
                        }
                    }

                    UserDialogs.Instance.HideLoading();
                    UserDialogs.Instance.Toast("Usunięto sklepy.");
                }
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }

        public async void ExportAction()
        {
            try
            {
                List<Shop> Shops = await App.Database.GetShopsAsync();
                if (Shops.Count == 0)
                {
                    UserDialogs.Instance.Alert("Nie ma żadnych sklepów możliwych do wyeksportowania.", "Błąd", "OK");
                    return;
                }
                List<string> shopsString = new List<string>();
                var shopsChoices = new[] { string.Empty };
                foreach (Shop shop in Shops)
                {
                    shopsString.Add(shop.Name + " Id:" + shop.ShopID.ToString());
                }
                shopsChoices = shopsString.ToArray();

                var choiceShop = await UserDialogs.Instance.ActionSheetAsync("Wybierz", "Anuluj", string.Empty, CancellationToken.None, shopsChoices);
                int shopId = Convert.ToInt32(choiceShop.Substring(choiceShop.IndexOf("Id:") + 3));
                List<Item> Items = await App.Database.GetItemsByShopAsync(shopId);
                Shop Shop = await App.Database.GetShopByID(shopId);
                if (Items.Count == 0)
                {
                    UserDialogs.Instance.Alert("W sklepie nie ma żadnych przedmiotów", "Błąd", "OK");
                    return;
                }

                var choices = new[] { "Do chmury", "Do schowka" };

                var choice = await UserDialogs.Instance.ActionSheetAsync("Wybierz", "Anuluj", string.Empty, CancellationToken.None, choices);

                if (choice == "Do chmury")
                {
                    if (Items.Count == 0)
                    {
                        UserDialogs.Instance.Alert("W sklepie nie ma żadnych przedmiotów", "Błąd", "OK");
                        return;
                    }

                    Uri defaultUri = new Uri(string.Format("http://192.168.1.100:7500/", string.Empty));
                    Uri uri = new Uri(defaultUri, "items/addshop/" + Shop.Name);

                    HttpClient httpClient = new HttpClient();
                    string resposne = await httpClient.GetStringAsync(uri);


                    foreach (Item item in Items)
                    {
                        Uri AddUri = new Uri(string.Format("http://192.168.1.100:7500/", string.Empty));
                        Uri addUri = new Uri(defaultUri, "items/additem/" + Shop.Name + "/" + item.Name + "/" + item.Quantity.ToString().Replace(',', '.'));

                        HttpClient Client = new HttpClient();
                        string resposne1 = await Client.GetStringAsync(addUri);
                    }
                    UserDialogs.Instance.Alert("Pomyślnie wyeksportowano " + Shop.Name + ".", "Powodzenie!", "Ok");
                }
                else if (choice == "Do schowka")
                {
                    if (Items.Count == 0)
                    {
                        UserDialogs.Instance.Alert("W sklepie nie ma żadnych przedmiotów", "Błąd", "OK");
                        return;
                    }

                    XElement items = new XElement("items");

                    foreach (Item item in Items)
                    {
                        XElement newItem = new XElement("item",
                            new XElement("name", item.Name),
                            new XElement("quantity", item.Quantity),
                            new XElement("isPresent", item.IsPresent),
                            new XElement("isChecked", item.IsChecked)
                            );

                        items.Add(newItem);
                    }

                    XElement shop =
                        new XElement("shop", new XAttribute("type", "single"),
                            new XElement("name", Shop.Name),
                                items
                                );

                    await Share.RequestAsync(new ShareTextRequest
                    {
                        Text = shop.ToString(),
                        Title = "Udostępniona lista zakupów"
                    });
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType().IsAssignableFrom(typeof(HttpRequestException)))
                {
                    UserDialogs.Instance.Alert("Podłącz się do sieci domowej.", "Błąd", "Ok");
                }
                else if (ex.GetType().IsAssignableFrom(typeof(WebException)))
                {
                    UserDialogs.Instance.Alert("Podłącz się do sieci domowej.", "Błąd", "Ok");
                }
                else if (ex.GetType().IsAssignableFrom(typeof(OperationCanceledException)))
                {
                    UserDialogs.Instance.Alert("Podłącz się do sieci domowej.", "Błąd", "Ok");
                }
                else if (ex.GetType().IsAssignableFrom(typeof(FormatException)))
                {
                    UserDialogs.Instance.Alert("Nie wybrano sklepu.", "Błąd", "Ok");
                }
                else
                {
                    UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
                }
            }
        }

        public async void ImportAction()
        {
            try
            {
                var choices = new[] { "Z chmury", "Ze schowka" };

                var choice = await UserDialogs.Instance.ActionSheetAsync("Wybierz", "Anuluj", string.Empty, CancellationToken.None, choices);

                if (choice == "Z chmury")
                {
                    Uri defaultUri = new Uri(string.Format("http://192.168.1.100:7500/", string.Empty));
                    HttpClient httpClient = new HttpClient();

                    Uri shopsUri = new Uri(defaultUri, "items/listshops/");
                    string resposneShops = await httpClient.GetStringAsync(shopsUri);

                    List<SqlShop> sqlShops = JsonConvert.DeserializeObject<List<SqlShop>>(resposneShops);
                    if (sqlShops.Count == 0)
                    {
                        UserDialogs.Instance.Toast("Brak sklepów do załadowania.");
                        return;
                    }

                    var shopsChoices = new[] { string.Empty };
                    List<string> Shops = new List<string>();
                    foreach (SqlShop shop in sqlShops)
                    {
                        Shops.Add(shop.Name + " " + shop.CreatedOn.ToString("yyyy-MM-dd HH:mm:ss") + " Id:" + shop.ShopID.ToString());
                    }
                    shopsChoices = Shops.ToArray();

                    var choiceShop = await UserDialogs.Instance.ActionSheetAsync("Wybierz", "Anuluj", string.Empty, CancellationToken.None, shopsChoices);
                    string shopId = choiceShop.Substring(choiceShop.IndexOf("Id:") + 3);


                    Uri uri = new Uri(defaultUri, "items/listbyid/" + shopId);
                    string resposne = await httpClient.GetStringAsync(uri);

                    List<SqlItem> sqlItems = JsonConvert.DeserializeObject<List<SqlItem>>(resposne);
                    httpClient.Dispose();

                    Shop shop1 = await App.Database.GetShopByName(sqlItems[0].SqlShop.Name);
                    if(shop1 == null)
                    {
                        await App.Database.SaveShopAsync(new Shop
                        {
                            Name = sqlItems[0].SqlShop.Name
                        });
                    }
                    shop1 = await App.Database.GetShopByName(sqlItems[0].SqlShop.Name);
                    foreach (SqlItem item in sqlItems)
                    {
                        Item newItem = new Item()
                        {
                            ShopID = shop1.ShopID,
                            IsChecked = false,
                            Name = item.Name,
                            Quantity = item.Quantity,
                            IsPresent = true,
                        };

                        await App.Database.SaveItemAsync(newItem);
                    }
                    UserDialogs.Instance.Alert("Pomyślnie załadowano przedmioty.", "Powodzenie!", "Ok");
                }
                else if (choice == "Ze schowka")
                {
                    string ShopName = "";
                    string listXml = await Clipboard.GetTextAsync();
                    XmlDocument list = new XmlDocument();
                    list.LoadXml(listXml);

                    foreach (XmlElement node in list)
                    {
                        if (node.Attributes["type"].Value != "single")
                        {
                            UserDialogs.Instance.Alert("Podjęto próbę załadowania złej listy.", "Błąd", "OK");
                            return;
                        }
                        foreach (XmlElement node1 in node)
                        {
                            if (node1.Name == "name")
                            {
                                ShopName = node1.InnerText;
                            }

                            if (node1.Name == "items")
                            {
                                foreach (XmlNode node2 in node1)
                                {
                                    string Name = "";
                                    double Quantity = 1;
                                    bool IsChecked = false, IsPresent = true;
                                    foreach (XmlNode node3 in node2)
                                    {
                                        if (node3.Name == "name")
                                            Name = node3.InnerText;
                                        if (node3.Name == "quantity")
                                            Quantity = Convert.ToDouble((node3.InnerText).Replace('.', ','));
                                        if (node3.Name == "isChecked")
                                            IsChecked = Convert.ToBoolean(node3.InnerText);
                                        if (node3.Name == "isPresent")
                                            IsPresent = Convert.ToBoolean(node3.InnerText);
                                    }

                                    Shop shop = await App.Database.GetShopByName(ShopName);
                                    if (shop == null)
                                    {
                                        await App.Database.InsertShopAsync(new Shop
                                        {
                                            Name = ShopName
                                        });
                                    }
                                    shop = await App.Database.GetShopByName(ShopName);

                                    await App.Database.SaveItemAsync(new Item
                                    {
                                        ShopID = shop.ShopID,
                                        IsChecked = IsChecked,
                                        IsPresent = IsPresent,
                                        Name = Name,
                                        Quantity = Quantity
                                    });
                                }
                                UserDialogs.Instance.Alert("Pomyślnie załadowano przedmioty.", "Powodzenie!", "Ok");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.GetType().IsAssignableFrom(typeof(HttpRequestException)))
                {
                    UserDialogs.Instance.Alert("Brak przedmiotów do załadowania.", "Błąd", "Ok");
                }
                else if (ex.GetType().IsAssignableFrom(typeof(WebException)))
                {
                    UserDialogs.Instance.Alert("Podłącz się do sieci domowej.", "Błąd", "Ok");
                }
                else if (ex.GetType().IsAssignableFrom(typeof(OperationCanceledException)))
                {
                    UserDialogs.Instance.Alert("Podłącz się do sieci domowej.", "Błąd", "Ok");
                }
                else
                {
                    UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
                }
            }
        }

        public async void DeleteFromCloudAction()
        {
            try
            {
                Uri defaultUri = new Uri(string.Format("http://192.168.1.100:7500/", string.Empty));
                HttpClient httpClient = new HttpClient();

                Uri shopsUri = new Uri(defaultUri, "items/listshops/");
                string resposneShops = await httpClient.GetStringAsync(shopsUri);

                List<SqlShop> sqlShops = JsonConvert.DeserializeObject<List<SqlShop>>(resposneShops);
                if (sqlShops.Count == 0)
                {
                    UserDialogs.Instance.Toast("Brak sklepów do załadowania.");
                    return;
                }

                var shopsChoices = new[] { string.Empty };
                List<string> Shops = new List<string>();
                foreach (SqlShop shop in sqlShops)
                {
                    Shops.Add(shop.Name + " " + shop.CreatedOn.ToString("yyyy-MM-dd HH:mm:ss") + " Id:" + shop.ShopID.ToString());
                }
                shopsChoices = Shops.ToArray();

                var choiceShop = await UserDialogs.Instance.ActionSheetAsync("Wybierz", "Anuluj", string.Empty, CancellationToken.None, shopsChoices);
                string shopId = choiceShop.Substring(choiceShop.IndexOf("Id:") + 3);

                var result = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig
                {
                    CancelText = "Anuluj",
                    Message = "Czy na pewno chcesz usunąć sklep z bazy?",
                    OkText = "Tak",
                    Title = "Potwierdzenie"
                });

                if (!result)
                {
                    return;
                }

                Uri uri = new Uri(defaultUri, "items/removeitems/" + shopId);
                string resposne = await httpClient.GetStringAsync(uri);
                httpClient.Dispose();
                UserDialogs.Instance.Toast("Usunieto sklep.");
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }
    }
}
