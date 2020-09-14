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
    public partial class AddShopPage : ContentPage
    {
        AddShopPageViewModel viewModel;
        public AddShopPage()
        {
            InitializeComponent();

            this.BindingContext = viewModel = new AddShopPageViewModel(Navigation);
        }
    }
}