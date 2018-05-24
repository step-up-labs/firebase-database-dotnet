using Firebase.Auth;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XamarinForms.Services
{
    public class FirebaseAuthService
    {
        private FirebaseAuthLink _authLink;
        private bool _didInit;

        private async Task InitFirebaseAuth()
        {
            if(string.IsNullOrEmpty(Config.ApiKeys.Firebase))
            {
                throw new Exception("The Firebase API key is empty. Make sure to set it according to your project.");
            }

            var authProvider = new FirebaseAuthProvider(new FirebaseConfig(Config.ApiKeys.Firebase));

            // First, check if our auth object/token is stored locally.
            FirebaseAuth auth = LoadFirebaseAuth();
            if(auth != null)
            {
                _authLink = new FirebaseAuthLink(authProvider, auth);
            }
            else
            {
                // If not, login and save it locally for next time.
                await Login(authProvider);
                SaveFirebaseAuth(_authLink);
            }

            // Save the auth object/token every time it's refreshed.
            _authLink.FirebaseAuthRefreshed += (s, e) => SaveFirebaseAuth(e.FirebaseAuth);
        }

        public async Task<string> GetFirebaseAuthToken()
        {
            if(!_didInit)
            {
                await InitFirebaseAuth();
                _didInit = true;
            }

            // This will refresh the auth object/token if it's expired.
            _authLink = await _authLink.GetFreshAuthAsync();

            return _authLink.FirebaseToken;
        }

        private async Task Login(FirebaseAuthProvider authProvider)
        {
            //var facebookAccessToken = "<login with facebook and get oauth access token>";
            //_authLink = await authProvider.SignInWithOAuthAsync(FirebaseAuthType.Facebook, facebookAccessToken);

            // Enable anonymous authentication in your Firebase panel.
            _authLink = await authProvider.SignInAnonymouslyAsync();
        }

        private FirebaseAuth LoadFirebaseAuth()
        {
            string json = Settings.FirebaseAuthJson;
            if(string.IsNullOrEmpty(json))
            {
                return null;
            }
            else
            {
                return JsonConvert.DeserializeObject<FirebaseAuth>(json);
            }
        }

        private void SaveFirebaseAuth(FirebaseAuth auth)
        {
            string json = JsonConvert.SerializeObject(auth);
            Settings.FirebaseAuthJson = json;
        }
    }
}
