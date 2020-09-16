using ShoppingList.Model;
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
    public partial class EditItemPage : ContentPage
    {
        EditItemPageViewModel viewModel;
        public EditItemPage(Shop shop, Item item)
        {
            InitializeComponent();

            this.BindingContext = viewModel = new EditItemPageViewModel(Navigation, shop, item);
        }
    }
}