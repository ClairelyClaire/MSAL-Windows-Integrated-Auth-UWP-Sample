using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSALSample
{
    public class Logout
    {
        public async void LogoutUser()
        {
            IEnumerable<IAccount> accounts = await App.PublicClientApp.GetAccountsAsync();
            IAccount firstAccount = accounts.FirstOrDefault();

            // create empty list object to store exceptions
            List<Exception> exceptions = new List<Exception>();

            MainPage m = new MainPage();

            try
            {
                await App.PublicClientApp.RemoveAsync(firstAccount);

                //nullify authResult - needed to ensure log out is complete
                Globals.AuthResult = null;

                // TODO: implement post-logout logic
                m.PostLogout();
            }
            catch (MsalException ex)
            {
                exceptions.Add(ex);

                m.ErrorHandler(exceptions);
            }
        }
    }
}
