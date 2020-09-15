using Acr.UserDialogs;
using ShoppingList.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ShoppingList.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShopsListPage : ContentPage
    {
        ShopsListViewModel viewModel;
        public ShopsListPage()
        {
            InitializeComponent();

            this.BindingContext = viewModel = new ShopsListViewModel(Navigation);
        }

        private async void DeleteCliced(object sender, EventArgs e)
        {
            try
            {
                var mi = ((MenuItem)sender);
                int id = Convert.ToInt32(mi.CommandParameter);
                var shop = viewModel.Shops.Where(x => x.ShopID == id).FirstOrDefault();

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
                    viewModel.Shops.Remove(shop);
                    viewModel.Renumber();

                }
                   

            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }

        private async void EditClicked(object sender, EventArgs e)
        {
            try
            {
                var mi = ((MenuItem)sender);
                int id = Convert.ToInt32(mi.CommandParameter);
                await Navigation.PushAsync(new EditShopPage(id));
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("Bład!\r\n\r\n" + ex.ToString(), "Błąd", "OK");
            }
        }
    }
}