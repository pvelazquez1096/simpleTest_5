using simpleTest_5.Auth;
using simpleTest_5.Graph;
using System;
using Microsoft.Graph;
using System.Collections.Generic;
using simpleTest_5.Database;
using System.Data.SqlClient;

namespace simpleTest_5
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            //Initialize connection to Microsoft Graph
            var appId = "5bba094d-693c-43ba-b50b-9b398591825b";
            var scopesString = "User.Read;User.ReadWrite.All;User.ManageIdentities.All;GroupMember.ReadWrite.All;Group.ReadWrite.All;Group.ReadWrite.All;People.Read.All";
            var scopes = scopesString.Split(';');
            var authProvider = new DeviceCodeAuthProvider(appId, scopes);
            GraphHelper.Initialize(authProvider);
            
            //Initialize connection to db
            DatabaseService database = new DatabaseService();
            database.inizialliceDatabaseService();
            
            
            var user = await GraphHelper.getMeAsync();
            Console.WriteLine($"Welcome {user.DisplayName}!\n");

            Console.WriteLine("Getting all users");
            List<User> users = await GraphHelper.getUserAsync();

            users.ForEach(printInfo);

            Console.WriteLine("Creating a new user");
            User newUser = await GraphHelper.createUserAsync();

            printInfo(newUser);

            Console.WriteLine("Getting all users again...");
            users = await GraphHelper.getUserAsync();

            users.ForEach(printInfo);

            
            Console.WriteLine("Reading Users from database");
            database.getUsers();
            Console.ReadKey();
        }

        public static void printInfo(User user)
        {
            Console.WriteLine($"User: {user.DisplayName} Email: {user.UserPrincipalName}");
        }
    }
}