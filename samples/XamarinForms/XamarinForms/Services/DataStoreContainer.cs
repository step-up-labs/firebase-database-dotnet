using System.Collections.Generic;
using XamarinForms.Models;

[assembly: Xamarin.Forms.Dependency(typeof(XamarinForms.Services.DataStoreContainer))]
namespace XamarinForms.Services
{
    public class DataStoreContainer
    {
        private FirebaseAuthService _firebaseAuthService;
        private IDataStore<Post> _postStore;
        private IDictionary<string, IDataStore<PostComment>> _parentKeyToPostCommentStoreDict;

        public DataStoreContainer(FirebaseAuthService firebaseAuthService)
        {
            _firebaseAuthService = firebaseAuthService;
        }

        public IDataStore<Post> PostStore
        {
            get
            {
                if(_postStore == null)
                {
                    _postStore = new FirebaseOfflineDataStore<Post>(_firebaseAuthService, "posts");
                }

                return _postStore;
            }
        }

        public IDataStore<PostComment> PostCommentStoreForKey(string key)
        {
            if(_parentKeyToPostCommentStoreDict == null)
            {
                _parentKeyToPostCommentStoreDict = new Dictionary<string, IDataStore<PostComment>>();
            }

            bool success = _parentKeyToPostCommentStoreDict.TryGetValue(key, out IDataStore<PostComment> dataStore);
            if(!success)
            {
                string path = string.Format("post-comments/{0}", key);
                dataStore = new FirebaseOfflineDataStore<PostComment>(_firebaseAuthService, path, key);
                _parentKeyToPostCommentStoreDict.Add(key, dataStore);
            }

            return dataStore;
        }
    }
}
