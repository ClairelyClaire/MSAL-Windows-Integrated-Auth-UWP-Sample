using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSALSample
{
    public class Login
    {
        // Set the scope for authentication to user.read
        string[] scopes = new string[] { "user.read" };

        public async void LoginUser()
        {
            // get auth tokens from app's cache
            IEnumerable<IAccount> accounts = await App.PublicClientApp.GetAccountsAsync();
            IAccount firstAccount = accounts.FirstOrDefault();

            // create empty list object to store exceptions
            List<Exception> exceptions = new List<Exception>();

            AuthenticationResult authResult = null;

            // handle authentication
            try
            {
                // if integrated auth is enabled, try that
                // otherwise, attempt auth with the first existing token from cache
                if (App.IntegratedAuth)
                    authResult = await App.PublicClientApp.AcquireTokenByIntegratedWindowsAuthAsync(scopes);
                else
                    authResult = await App.PublicClientApp.AcquireTokenSilentAsync(scopes, firstAccount);

            }
            // silent auth failed - catch exception
            catch (MsalException ex)
            {
                // silent auth failed
                exceptions.Add(ex);

                // try interactive auth
                try
                {
                    authResult = await App.PublicClientApp.AcquireTokenAsync(scopes);
                }
                catch (MsalException msalex)
                {
                    exceptions.Add(msalex);
                }
            }

            MainPage m = new MainPage();

            // if exceptions exist, pass the list to the handler
            if (exceptions.Count > 0)
            {
                m.ErrorHandler(exceptions);
            }
            else if (authResult != null)
            {
                // TODO: implement post-login logic
                m.PostLogin();
            }
        }
    }
}
