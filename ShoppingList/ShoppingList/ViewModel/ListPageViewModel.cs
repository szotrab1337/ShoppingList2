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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

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
            Items = new ObservableCollection<Item>();

            AddNewItemCommand = new Command(AddNewItemAction);
            //EditCommand = new Command(async (object obj) => await EditAction(obj));
            EditCommand = new Command(EditAction);
            DeleteCommand = new Command(DeleteAction);
            DoubleTapCommand = new Command(DoubleTapAction);
            RefreshCommand = new Command(RefreshAction);
            SwipeRightCommand = new Command(SwipeRightAction);
            SwipeLeftCommand = new Command(SwipeLeftAction);
            ImportCommand = new Command(ImportAction);
            ExportCommand = new Command(ExportAction);
            UnCheckAllCommand = new Command(UnCheckAllAction);
            DeleteCheckedCommand = new Command(DeleteCheckedAction);
            DeleteAllCommand = new Command(DeleteAllAction);

            MessagingCenter.Subscribe<AddNewItemPageViewModel>(this, "Refresh", (LoadAgain) =>
            {
                LoadItems();
            });
            MessagingCenter.Subscribe<EditItemPageViewModel>(this, "Refresh", (LoadAgain) =>
            {
                LoadItems();
            });

            LoadItems();
        }

        public INavigation Navigation { get; set; }

        public ICommand AddNewItemCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand DoubleTapCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand SwipeRightCommand { get; set; }
        public ICommand SwipeLeftCommand { get; set; }
        public ICommand ImportCommand { get; set; }
        public ICommand ExportCommand { get; set; }
        public ICommand UnCheckAllCommand { get; set; }
        public ICommand DeleteCheckedCommand { get; set; }
        public ICommand DeleteAllCommand { get; set; }
        public Shop Shop { get; set; }

        public string Title
        {
            get { return _Title; }
            set { _Title = value; OnPropertyChanged("Title"); }
        }
        private string _Title;

        public ObservableCollection<Item> Items
        {
            get { return _Items; }
            set { _Items = value; OnPropertyChanged("Items"); }
        }
        private ObservableCollection<Item> _Items;

        public async void AddNewItemAction()
        {
            try
            {
                await Navigation.PushAsync(new AddNewItemPage(Shop));
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }

        public async void LoadItems()
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Ładowanie...", MaskType.Black);
                await LoadData();
                UserDialogs.Instance.HideLoading();
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }

        public async Task LoadData()
        {
            try
            {
                Items.Clear();
                List<Item> allItems = (await App.Database.GetItemsByShopAsync(Shop.ShopID)).OrderByDescending(x => x.IsPresent).ThenBy(x => x.IsChecked).ThenBy(x => x.Name).ToList();

                for (int i = 0; i < allItems.Count; i++)
                {
                    allItems[i].Number = i + 1;
                    if (allItems[i].IsChecked == true)
                    {
                        allItems[i].TextDec = TextDecorations.Strikethrough;
                        allItems[i].BgdColor = Color.FromRgb(179, 255, 204);
                    }
                    else
                    {
                        allItems[i].TextDec = TextDecorations.None;
                        allItems[i].BgdColor = Color.Transparent;

                    }

                    if (allItems[i].IsPresent && !allItems[i].IsChecked)
                    {
                        allItems[i].BgdColor = Color.Transparent;
                    }
                    else if (!allItems[i].IsPresent && !allItems[i].IsChecked)
                    {
                        allItems[i].BgdColor = Color.FromRgb(255, 153, 128);
                    }
                    Items.Add(allItems[i]);

                }
                OnPropertyChanged("Items");

                foreach (var item in Items)
                {
                    item.PropertyChanged += ItemPropertyChanged;
                }
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }

        private async void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == "IsChecked")
                {
                    Item item = (Item)sender;
                    item.IsPresent = true;
                    await App.Database.UpdateItemAsync(item);
                    if (item.IsChecked == true)
                    {
                        Items.Where(x => x.ItemID == item.ItemID).FirstOrDefault().TextDec = TextDecorations.Strikethrough;
                        Items.Where(x => x.ItemID == item.ItemID).FirstOrDefault().BgdColor = Color.FromRgb(179, 255, 204);
                    }
                    else
                    {
                        Items.Where(x => x.ItemID == item.ItemID).FirstOrDefault().TextDec = TextDecorations.None;
                        Items.Where(x => x.ItemID == item.ItemID).FirstOrDefault().BgdColor = Color.Transparent;
                    }

                    OnPropertyChanged("Items");
                }
            }
            catch (Exception ex)
            {
                //UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }

        public void CheckIsPresent(Item item)
        {
            try
            {
                if (!item.IsChecked)
                {
                    if (item.IsPresent)
                    {
                        Items.Where(x => x.ItemID == item.ItemID).FirstOrDefault().BgdColor = Color.Transparent;
                    }
                    else
                    {
                        Items.Where(x => x.ItemID == item.ItemID).FirstOrDefault().BgdColor = Color.FromRgb(255, 153, 128);
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
                Item item = (Item)sender;
                await Navigation.PushAsync(new EditItemPage(Shop, item));
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
                Item item = (Item)sender;
                var result = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig
                {
                    Message = "Czy na pewno chcesz usunąć przedmiot " + item.Name + "?",
                    OkText = "Tak",
                    CancelText = "Nie",
                    Title = "Potwierdzenie"
                });

                if (result)
                {
                    await App.Database.DeleteItemAsync(item);
                    UserDialogs.Instance.Toast("Usunięto przedmiot.");
                    Items.Remove(item);
                    Renumber();
                    MessagingCenter.Send(this, "Refresh");
                }
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }

        public async void DoubleTapAction(object sender)
        {
            try
            {
                Item item = (Item)sender;
                await App.Database.UpdateItemAsync(item);
                Items.Where(x => x.ItemID == item.ItemID).FirstOrDefault().IsChecked = !item.IsChecked;
                if (item.IsChecked == true)
                {
                    Items.Where(x => x.ItemID == item.ItemID).FirstOrDefault().TextDec = TextDecorations.Strikethrough;
                    Items.Where(x => x.ItemID == item.ItemID).FirstOrDefault().BgdColor = Color.FromRgb(179, 255, 204);
                }
                else
                {
                    Items.Where(x => x.ItemID == item.ItemID).FirstOrDefault().TextDec = TextDecorations.None;
                    Items.Where(x => x.ItemID == item.ItemID).FirstOrDefault().BgdColor = Color.Transparent;
                }

                OnPropertyChanged("Items");
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }

        public async void SwipeRightAction(object sender)
        {
            try
            {
                Item item = (Item)sender;
                if (!item.IsChecked && item.IsPresent)
                {
                    item.IsPresent = !item.IsPresent;
                    await App.Database.UpdateItemAsync(item);
                    Items.Where(x => x.ItemID == item.ItemID).FirstOrDefault().BgdColor = Color.FromRgb(255, 153, 128);
                }
                OnPropertyChanged("Items");
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }

        }

        public async void SwipeLeftAction(object sender)
        {
            try
            {
                Item item = (Item)sender;
                if (!item.IsChecked && !item.IsPresent)
                {
                    item.IsPresent = !item.IsPresent;
                    await App.Database.UpdateItemAsync(item);
                    Items.Where(x => x.ItemID == item.ItemID).FirstOrDefault().BgdColor = Color.Transparent;
                }
                OnPropertyChanged("Items");
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
                for (int i = 0; i < Items.Count; i++)
                {
                    Items[i].Number = i + 1;
                }

                ObservableCollection<Item> tempItems = Items;
                Items = null;
                Items = tempItems;
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }

        public async void RefreshAction()
        {
            try
            {
                UserDialogs.Instance.ShowLoading("Ładowanie...", MaskType.Black);
                await LoadData();
                UserDialogs.Instance.HideLoading();
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
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
                    Uri uri = new Uri(defaultUri, "items/listbyname/" + Shop.Name);

                    HttpClient httpClient = new HttpClient();
                    string resposne = await httpClient.GetStringAsync(uri);

                    List<SqlItem> sqlItems = JsonConvert.DeserializeObject<List<SqlItem>>(resposne);
                    httpClient.Dispose();

                    foreach (SqlItem item in sqlItems)
                    {
                        Item newItem = new Item()
                        {
                            ShopID = Shop.ShopID,
                            IsChecked = false,
                            Name = item.Name,
                            Quantity = item.Quantity,
                            IsPresent = true,
                        };

                        await App.Database.SaveItemAsync(newItem);
                    }
                    LoadItems();
                    UserDialogs.Instance.Alert("Pomyślnie załadowano " + sqlItems.Count + " " + GetInfo(sqlItems.Count) + ".", "Powodzenie!", "Ok");
                }
                else if (choice == "Ze schowka")
                {
                    string listXml = await Clipboard.GetTextAsync();
                    XmlDocument list = new XmlDocument();
                    list.LoadXml(listXml);

                    foreach (XmlElement node in list)
                    {
                        if(node.Attributes["type"].Value != "single")
                        {
                            UserDialogs.Instance.Alert("Podjęto próbę załadowania złej listy.", "Błąd", "OK");
                            return;
                        }
                        foreach (XmlElement node1 in node)
                        {
                            if(node1.Name == "name")
                            {
                                if(node1.InnerText != Shop.Name)
                                {
                                    var result = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig
                                    {
                                        CancelText = "Anuluj",
                                        Message = "Obecna nazwa sklepu różni się od obecnie włączonego.\r\nCzy chcesz kontynuować?",
                                        OkText = "Tak",
                                        Title = "Informacja"
                                    });

                                    if(!result)
                                    {
                                        break;
                                    }
                                }
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
                                            Quantity = Convert.ToDouble((node3.InnerText).Replace('.',','));
                                        if (node3.Name == "isChecked")
                                            IsChecked = Convert.ToBoolean(node3.InnerText);
                                        if (node3.Name == "isPresent")
                                            IsPresent = Convert.ToBoolean(node3.InnerText);
                                    }

                                    await App.Database.SaveItemAsync(new Item
                                    {
                                        ShopID = Shop.ShopID,
                                        IsChecked = IsChecked,
                                        IsPresent = IsPresent,
                                        Name = Name,
                                        Quantity = Quantity
                                    });
                                }
                            }
                        }
                    }
                    LoadItems();
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

        public string GetInfo(int count)
        {
            string info;
            switch (count)
            {
                case 1:
                    info = "przedmiot";
                    break;
                case 2:
                case 3:
                case 4:
                    info = "przedmioty";
                    break;
                default:
                    info = "przedmiotów";
                    break;
            }
            return info;
        }

        public async void ExportAction()
        {
            try
            {
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
                    UserDialogs.Instance.Alert("Pomyślnie wyeksportowano " + Items.Count + " " + GetInfo(Items.Count) + ".", "Powodzenie!", "Ok");
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
                else
                {
                    UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
                }
            }
        }

        public async void UnCheckAllAction()
        {
            try
            {
                List<Item> items = Items.Where(x => x.IsChecked == true).ToList();

                if(items.Count == 0)
                {
                    UserDialogs.Instance.Alert("Brak przedmiotów do odznaczenia w sklepie.", "Błąd", "OK");
                    return;
                }

                var result = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig
                {
                    Message = "Czy na pewno chcesz odznaczyć wszystkie przedmioty?",
                    OkText = "Tak",
                    CancelText = "Nie",
                    Title = "Potwierdzenie"
                });

                if (!result)
                {
                    return;
                }

                foreach (Item item in items)
                {
                    item.IsChecked = false;
                    await App.Database.UpdateItemAsync(item);
                }
                LoadItems();
                UserDialogs.Instance.Alert("Pomyślnie odznaczono " + items.Count + " " + GetInfo(items.Count) + ".", "Informacja", "OK");
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }

        public async void DeleteCheckedAction()
        {
            try
            {
                List<Item> items = Items.Where(x => x.IsChecked == true).ToList();

                if (items.Count == 0)
                {
                    UserDialogs.Instance.Alert("Brak przedmiotów do usunięcia w sklepie.", "Błąd", "OK");
                    return;
                }

                var result = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig
                {
                    Message = "Czy na pewno chcesz usunać wszystkie kupione przedmioty?",
                    OkText = "Tak",
                    CancelText = "Nie",
                    Title = "Potwierdzenie"
                });

                if (!result)
                {
                    return;
                }

                foreach (Item item in items)
                {
                    await App.Database.DeleteItemAsync(item);
                }
                LoadItems();
                MessagingCenter.Send(this, "Refresh");
                UserDialogs.Instance.Alert("Pomyślnie usunięto " + items.Count + " " + GetInfo(items.Count) + ".", "Informacja", "OK");
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }

        public async void DeleteAllAction()
        {
            try
            {
                if (Items.Count == 0)
                {
                    UserDialogs.Instance.Alert("Brak przedmiotów w sklepie.", "Błąd", "OK");
                    return;
                }

                var result = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig
                {
                    Message = "Czy na pewno chcesz usunąć wszystkie przedmioty?",
                    OkText = "Tak",
                    CancelText = "Nie",
                    Title = "Potwierdzenie"
                });

                if (!result)
                {
                    return;
                }

                foreach (Item item in Items)
                {
                    await App.Database.DeleteItemAsync(item);
                }
                LoadItems();
                MessagingCenter.Send(this, "Refresh");
                UserDialogs.Instance.Alert("Pomyślnie usunięto wszystkie przedmioty.", "Informacja", "OK");
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }
    }
}
