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
    public class AddNewItemPageViewModel : INotifyPropertyChanged
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
        public AddNewItemPageViewModel(INavigation navigation, Shop shop)
        {
            this.Navigation = navigation;
            this.Shop = shop;

            Title = Shop.Name;
            StepperValue = 1;

            AddCommand = new Command(AddAction);
        }

        public ICommand AddCommand { get; set; }

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

        public async void AddAction()
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
                    await App.Database.SaveItemAsync(new Item
                    {
                        ShopID = Shop.ShopID,
                        IsChecked = false,
                        Name = ItemName,
                        Quantity = StepperValue.Value,
                        IsPresent = true
                    });

                    UserDialogs.Instance.Toast("Dodano nowy przedmiot.");
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
