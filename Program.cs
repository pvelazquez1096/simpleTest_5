using Microsoft.Extensions.Configuration;
using simpleTest_5.Auth;
using simpleTest_5.Graph;
using System;


namespace simpleTest_5
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            //var appConfig = LoadAppSettings();
            var appId = "5bba094d-693c-43ba-b50b-9b398591825b";
            Console.WriteLine(appId);
            var scopesString = "User.Read;User.ReadWrite.All;User.ManageIdentities.All;GroupMember.ReadWrite.All;Group.ReadWrite.All;Group.ReadWrite.All;People.Read.All";
            Console.WriteLine(scopesString);
            var scopes = scopesString.Split(';');
            var authProvider = new DeviceCodeAuthProvider(appId, scopes);
            //var accessToken = await authProvider.GetAccessToken();
            //Console.WriteLine($"Access token: {accessToken}\n");
            GraphHelper.Initialize(authProvider);
            var user = await GraphHelper.GetMeAsync();
            var users = await GraphHelper.getUserAsync();
            Console.WriteLine($"Welcome {user.DisplayName}!\n");
            Console.WriteLine($"Users: {users.DisplayName}\n");
            Console.ReadKey();
        }
    }
}