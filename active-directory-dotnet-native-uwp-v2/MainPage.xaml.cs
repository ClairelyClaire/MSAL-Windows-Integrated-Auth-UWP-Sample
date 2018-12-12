using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Diagnostics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace active_directory_dotnet_native_uwp_v2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //Set the API Endpoint to Graph 'me' endpoint
        string graphAPIEndpoint = "https://graph.microsoft.com/v1.0/me";

        //Set the scope for API call to user.read
        string[] scopes = new string[] { "user.read" };

        // create authentication result object
        AuthenticationResult authResult = null;


        public MainPage()
        {
            this.InitializeComponent();
            CallGraphButton.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Call AcquireTokenAsync - to acquire a token requiring user to sign-in
        /// </summary>
        private async void CallGraphButton_Click(object sender, RoutedEventArgs e)
        {
            Button cmd = sender as Button;
            cmd.IsEnabled = false;
            cmd.Content = "Working...";

            ResultText.Text = string.Empty;
            TokenInfoText.Text = string.Empty;

            if (authResult != null)
            {
                ResultText.Text = await GetHttpContentWithToken(graphAPIEndpoint, authResult.AccessToken);
                DisplayBasicTokenInfo(authResult);
            }

            cmd.IsEnabled = true;
            cmd.Content = "Call Microsoft Graph API";
        }

        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            Button cmd = sender as Button;

            if ((string)cmd.Content == "Log In")
                LoginUser();
            else
                LogoutUser();
        }

        private async void LoginUser()
        {
            // get auth tokens from app's cache
            IEnumerable<IAccount> accounts = await App.PublicClientApp.GetAccountsAsync();
            IAccount firstAccount = accounts.FirstOrDefault();

            // create empty list object to store exceptions
            List<Exception> exceptions = new List<Exception>();

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

            // if exceptions exist, pass the list to the handler
            if (exceptions.Count > 0)
            {
                ErrorHandler(exceptions);
            }
            else if (authResult != null)
            {
                CallGraphButton.Visibility = Visibility.Visible;
                UserButton.Content = "Log Out";
            }
        }

        private async void LogoutUser()
        {
            IEnumerable<IAccount> accounts = await App.PublicClientApp.GetAccountsAsync();
            IAccount firstAccount = accounts.FirstOrDefault();

            // create empty list object to store exceptions
            List<Exception> exceptions = new List<Exception>();

            try
            {
                await App.PublicClientApp.RemoveAsync(firstAccount);
                // logged out successfully - nullify authResult
                // hide Graph button
                // rewrite log out button to log in

                UserButton.Content = "Log In";
                CallGraphButton.Visibility = Visibility.Collapsed;
                authResult = null;
            }
            catch (MsalException ex)
            {
                exceptions.Add(ex);

                ErrorHandler(exceptions);
            }
        }

        private void DisplayResult(string resultText, Control target)
        {
            if (target is TextBox)
                ((TextBox)target).Text = resultText;
        }

        private void ErrorHandler (List<Exception> exceptions)
        {
            // TODO add error-handling steps
            // details of MSAL exception types here:
            // https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/exceptions

            // if the sample is included, display the error(s)
            if (App.GraphSample)
                DisplayResult(string.Join("\n\n",(exceptions.OfType<MsalException>())),ResultText);
        }

        /// <summary>
        /// Perform an HTTP GET request to a URL using an HTTP Authorization header
        /// </summary>
        /// <param name="url">The URL</param>
        /// <param name="token">The token</param>
        /// <returns>String containing the results of the GET operation</returns>
        public async Task<string> GetHttpContentWithToken(string url, string token)
        {
            var httpClient = new System.Net.Http.HttpClient();
            System.Net.Http.HttpResponseMessage response;
            try
            {
                var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
                //Add the token in Authorization header
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                response = await httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        /// <summary>
        /// Display basic information contained in the token
        /// </summary>
        private void DisplayBasicTokenInfo(AuthenticationResult authResult)
        {
            TokenInfoText.Text = "";
            if (authResult != null)
            {
                TokenInfoText.Text += $"User Name: {authResult.Account.Username}" + Environment.NewLine;
                TokenInfoText.Text += $"Token Expires: {authResult.ExpiresOn.ToLocalTime()}" + Environment.NewLine;
                TokenInfoText.Text += $"Access Token: {authResult.AccessToken}" + Environment.NewLine;
            }
        }
    }
}
