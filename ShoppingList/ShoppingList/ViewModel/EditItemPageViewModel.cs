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
    public class EditItemPageViewModel : INotifyPropertyChanged
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

        public INavigation Navigation { get; set; }
        public Shop Shop { get; set; }
        public Item Item { get; set; }

        public EditItemPageViewModel(INavigation navigation, Shop shop, Item item)
        {
            this.Navigation = navigation;
            this.Shop = shop;
            this.Item = item;

            Title = Item.Name;
            ItemName = Item.Name;
            StepperValue = item.Quantity;

            SaveCommand = new Command(SaveAction);
        }
        public ICommand SaveCommand { get; set; }

        public string ItemName
        {
            get { return _ItemName; }
            set { _ItemName = value; OnPropertyChanged("ItemName"); }
        }
        private string _ItemName;
        public string Title
        {
            get { return _Title; }
            set { _Title = value; OnPropertyChanged("Title"); }
        }
        private string _Title;
        public string ItemQuantity
        {
            get { return _ItemQuantity; }
            set { _ItemQuantity = value; OnPropertyChanged("ItemQuantity"); }
        }
        private string _ItemQuantity;
        public double? StepperValue
        {
            get { return _StepperValue; }
            set
            {
                _StepperValue = value; OnPropertyChanged("StepperValue");
                ItemQuantity = StepperValue.ToString();
            }
        }
        private double? _StepperValue;

        public async void SaveAction()
        {
            try
            {
                if (string.IsNullOrEmpty(ItemName))
                {
                    UserDialogs.Instance.Alert("Wprowadź nazwę przedmiotu.", "Błąd", "OK");
                }
                else if (StepperValue <= 0 || StepperValue == null)
                {
                    UserDialogs.Instance.Alert("Wprowadź ilość.", "Błąd", "OK");
                }
                else
                {
                    Item itemToUpdate = new Item
                    {
                        ItemID = Item.ItemID,
                        ShopID = Item.ShopID,
                        IsChecked = Item.IsChecked,
                        Name = ItemName,
                        Quantity = StepperValue.Value
                    };
                    await App.Database.UpdateItemAsync(itemToUpdate);

                    UserDialogs.Instance.Toast("Dokonano edycji przedmiotu.");
                    MessagingCenter.Send(this, "Refresh");
                    await Navigation.PopAsync();
                }
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }

        }
    }
}
