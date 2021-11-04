using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace simpleTest_5.Auth
{
    public class DeviceCodeAuthProvider : IAuthenticationProvider
    {
        private IPublicClientApplication _msalClient;
        private string[] _scopes;
        private IAccount _userAccount;
        #pragma warning disable CS8618 // Un campo que no acepta valores NULL debe contener un valor distinto de NULL al salir del constructor. Considere la posibilidad de declararlo como que admite un valor NULL.
        public DeviceCodeAuthProvider(string appId, string[] scopes)
        #pragma warning restore CS8618 // Un c'ampo que no acepta valores NULL debe contener un valor distinto de NULL al salir del constructor. Considere la posibilidad de declararlo como que admite un valor NULL.
        {
            _scopes = scopes;
            _msalClient = PublicClientApplicationBuilder
                .Create(appId)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/2895a67d-fae8-437d-9e57-3392f9a5de4e/"))
                .Build();
        }
        public async Task<string> GetAccessToken()
        {
            if (_userAccount == null)
            {
                try
                {
                    var result = await _msalClient.AcquireTokenWithDeviceCode(_scopes, callback => {
                        Console.WriteLine(callback.Message);
                        return Task.FromResult(0);
                    }).ExecuteAsync();
                    _userAccount = result.Account;
                    return result.AccessToken;
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Error getting access token: {exception.Message}");
                    return null;
                }
            }
            else
            {
                var result = await _msalClient
                    .AcquireTokenSilent(_scopes, _userAccount)
                    .ExecuteAsync();
                return result.AccessToken;
            }
        }
        public async Task AuthenticateRequestAsync(HttpRequestMessage requestMessage)
        {
            requestMessage.Headers.Authorization =
                new AuthenticationHeaderValue("bearer", await GetAccessToken());
        }
    }
}