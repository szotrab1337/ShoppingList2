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
    public partial class EditShopPage : ContentPage
    {
        EditShopPageViewModel viewModel;
        public EditShopPage(int id)
        {
            InitializeComponent();

            this.BindingContext = viewModel = new EditShopPageViewModel(Navigation, id);
        }
    }
}