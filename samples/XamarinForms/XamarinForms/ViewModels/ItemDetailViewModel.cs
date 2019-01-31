using System;

using XamarinForms.Models;

namespace XamarinForms.ViewModels
{
    public class ItemDetailViewModel : BaseViewModel
    {
        public Post Item { get; set; }
        public ItemDetailViewModel(Post item = null)
        {
            Title = item?.Title;
            Item = item;
        }
    }
}
