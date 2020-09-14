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

            this.BindingContext = viewModel = new ShopsListViewModel();
        }

        private async void DeleteCliced(object sender, EventArgs e)
        {
            var mi = ((MenuItem)sender);
            int id = Convert.ToInt32(mi.CommandParameter);
            var shop = viewModel.Shops.Where(x => x.ShopID == id).FirstOrDefault();
            await App.Database.DeleteShopAsync(shop);
            viewModel.LoadShops();
        }
    }
}