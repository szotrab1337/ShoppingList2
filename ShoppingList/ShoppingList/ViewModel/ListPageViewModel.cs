﻿using Acr.UserDialogs;
using ShoppingList.Model;
using ShoppingList.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
            DoubleTapCommand = new Command(DoubleTapAction);
            RefreshCommand = new Command(RefreshAction);
            SwipeRightCommand = new Command(SwipeRightAction);
            SwipeLeftCommand = new Command(SwipeLeftAction);

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
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
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
    }
}
