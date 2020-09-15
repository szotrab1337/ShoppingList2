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
    public partial class AddNewItemPage : ContentPage
    {
        AddNewItemPageViewModel viewModel;
        public AddNewItemPage()
        {
            InitializeComponent();

            this.BindingContext = viewModel = new AddNewItemPageViewModel(Navigation);
        }
    }
}