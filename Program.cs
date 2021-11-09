using simpleTest_5.Auth;
using simpleTest_5.Graph;
using simpleTest_5.Tools;
using simpleTest_5.Database;

using System;
using Microsoft.Graph;
using System.Collections.Generic;
using System.Data.SqlClient;
using simpleTest_5.Models;
using System.Linq;
using System.Threading.Tasks;

namespace simpleTest_5
{
    public class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            string path = "C:/Users/pvelazquez/Downloads/EmployeesListPedro2.csv";
            ToolService tool = new ToolService();
            List<UserDummie> usersDummie = new List<UserDummie>();
            List<Group> groups = new List<Group>();
            string[] csv = null;

            ////Initialize connection to Microsoft Graph/////
            var authProvider = GetDeviceCodeAuthProvider();
            GraphHelper.Initialize(authProvider);
            /////////////////////////////////////////////////
            var user = await GraphHelper.GetUserByEmail("pedro@dlsandbox.onmicrosoft.com");

            //csv = tool.LoadCSV(path);
            //usersDummie = tool.CreateUsers(csv);

            Console.WriteLine("End");
            Console.ReadKey();
        }
        private static DeviceCodeAuthProvider GetDeviceCodeAuthProvider()
        {
            var appId = "5bba094d-693c-43ba-b50b-9b398591825b";
            var scopesString = "User.Read;User.ReadWrite.All;User.ManageIdentities.All;GroupMember.ReadWrite.All;Group.ReadWrite.All;Group.ReadWrite.All;People.Read.All;Application.ReadWrite.All;Directory.AccessAsUser.All";
            var scopes = scopesString.Split(';');
            return new DeviceCodeAuthProvider(appId, scopes);
        }
        private static List<Group> checkFieldsChanged(List<DirectoryObject> directory, UserDummie user)
        {
            List<Group> changedGroups = new List<Group>();
            foreach (Group group in directory)
            {
                if (group.Description == "COE")
                {
                    if (group.DisplayName != user.GetCOE())
                        changedGroups.Add(group);
                }else if(group.Description == "Vertical")
                {
                    if (group.DisplayName != user.GetVertical())
                        changedGroups.Add(group);
                }else if(group.Description == "Resource_country")
                {
                    if (group.DisplayName != user.GetResource_country())
                        changedGroups.Add(group);
                }
            }
            return changedGroups;
        }
        public static async Task<List<Group>> AddUserToGroupsDinamically(List<UserDummie> userList)
        {
            foreach (UserDummie user in userList)
            {
                User userFromAAD = await GraphHelper.GetUserByEmail(user);
                List<DirectoryObject> directory = await GraphHelper.GetGroupsFromMember(userFromAAD);
                List<Group> changedGroups = checkFieldsChanged(directory, user);

            }
            return null;
        }
    }
}

/*
//Initialize connection to db
DatabaseService database = new DatabaseService();
database.inizialliceDatabaseService();
            
Console.WriteLine("Reading Users from database");
database.getUsers();
*/
/*
            var schemaExtension = new SchemaExtension
            {
                Id = "extras",
                Description = "Extra properties from salesforce",
                TargetTypes = new List<String>()
                {
                    "User"
                },
                Properties = new List<ExtensionSchemaProperty>()
                {
                    new ExtensionSchemaProperty
                    {
                        Name = "COE",
                        Type = "String"
                    },
                    new ExtensionSchemaProperty
                    {
                        Name = "Vertical",
                        Type = "String"
                    }
                }
            };
            try
            {
                var extension = await GraphHelper.graphClient.SchemaExtensions
                .Request()
                .AddAsync(schemaExtension);
                Console.WriteLine(extension.Id);
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
*/