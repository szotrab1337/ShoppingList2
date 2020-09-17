using Acr.UserDialogs;
using ShoppingList.Model;
using ShoppingList.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
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
            Items = new ObservableCollection<Item>();

            AddNewItemCommand = new Command(AddNewItemAction);
            //EditCommand = new Command(async (object obj) => await EditAction(obj));
            EditCommand = new Command(EditAction);
            DeleteCommand = new Command(DeleteAction);

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
                Items.Clear();
                List<Item> allItems = await App.Database.GetItemsByShopAsync(Shop.ShopID);

                for (int i = 0; i < allItems.Count; i++)
                {
                    allItems[i].Number = i + 1;
                    Items.Add(allItems[i]);
                }

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

        private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsChecked")
            {
                UserDialogs.Instance.Toast("ZmienionoItems");
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
    }
}
