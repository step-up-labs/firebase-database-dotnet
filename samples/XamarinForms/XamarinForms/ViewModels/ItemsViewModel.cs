using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

using Xamarin.Forms;

using XamarinForms.Models;
using XamarinForms.Services;
using XamarinForms.Views;

namespace XamarinForms.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        public ObservableCollection<Post> Items { get; set; }
        public Command LoadItemsCommand { get; set; }

        public ItemsViewModel()
        {
            Title = "Browse";
            Items = new ObservableCollection<Post>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            MessagingCenter.Subscribe<NewItemPage, Post>(this, "AddItem", async (obj, item) =>
            {
                var _item = item as Post;
                Items.Add(_item);
                await App.DataStoreContainer.PostStore.AddItemAsync(_item);
                //await DataStore.AddItemAsync(_item);
            });
        }

        async Task ExecuteLoadItemsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                Items.Clear();
                var items = await App.DataStoreContainer.PostStore.GetItemsAsync(true);
                //var items = await App.DataStoreContainer.PostCommentStoreForKey("uid1").GetItemsAsync(true);
                //var items = await DataStore.GetItemsAsync(true);
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}