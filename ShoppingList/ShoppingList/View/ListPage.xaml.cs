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
    public partial class ListPage : ContentPage
    {
        ListPageViewModel viewModel;
        public ListPage(Shop shop)
        {
            InitializeComponent();
            this.BindingContext = viewModel = new ListPageViewModel(Navigation, shop);
        }
    }
}