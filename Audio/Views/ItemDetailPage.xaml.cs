using Audio.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace Audio.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}