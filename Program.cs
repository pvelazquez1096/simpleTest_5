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
            List<Group> groups = new List<Group>();
            string[] csv = tool.LoadCSV(path);
            List<UserDummie> usersDummie = tool.CreateUsers(csv);
            ////Initialize connection to Microsoft Graph/////
            var authProvider = GetDeviceCodeAuthProvider();
            GraphHelper.Initialize(authProvider);
            /*
            foreach(UserDummie user in usersDummie)
            {
                User newUser = await GraphHelper.CreateUser(user);
                if(newUser!=null)
                    Console.WriteLine($"{newUser.DisplayName} {newUser.UserPrincipalName} Vertical: {tool.GetAdditonalPropertiesFromUser(newUser)[0]} COE: {tool.GetAdditonalPropertiesFromUser(newUser)[1]}");
            }
            Console.WriteLine("Users created");
            List<User> usersNotAdded = await AddUserToGroupsDinamically(usersDummie);
            foreach(User user in usersNotAdded)
                Console.WriteLine($"User {user.DisplayName} not added");
            */

            User newUser = await GraphHelper.GetUserByEmail("pedro@dlsandbox.onmicrosoft.com");
            newUser = await GraphHelper.UpdateUser(newUser, "Solution Delivery Team", "Other");
            Console.WriteLine($"{newUser.DisplayName} {newUser.UserPrincipalName} Vertical: {tool.GetAdditonalPropertiesFromUser(newUser)[0]} COE: {tool.GetAdditonalPropertiesFromUser(newUser)[1]}");

            


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
        public static async Task<List<User>> AddUserToGroupsDinamically(List<UserDummie> userList)
        {
            List<User> usersNotAdded = new List<User>();
            foreach (UserDummie user in userList)
            {
                User userFromAAD = await GraphHelper.GetUserByEmail(user);
                List<DirectoryObject> directory = await GraphHelper.GetGroupsFromMember(userFromAAD);
                bool result = await addUsersToGroups(directory, userFromAAD);
                if (!result)
                {
                    Console.WriteLine("User Added incorrectly");
                    usersNotAdded.Add(userFromAAD);
                }
            }
            return usersNotAdded;
        }
        private static async Task<bool> addUsersToGroups(List<DirectoryObject> directory, User user)
        {
            ToolService tool = new ToolService();
            string[] additionalProperties = tool.GetAdditonalPropertiesFromUser(user);
            string vertical = additionalProperties[0];
            string coe = (additionalProperties[1] is null || additionalProperties[1].Length == 0) ? "" : additionalProperties[1];
            bool result = false;
            if (directory != null && directory.Count != 0)
            {
                foreach (Group group in directory)
                {
                    if (group.Description == "COE")
                    {
                        if (coe.Length == 0)
                        {
                            await GraphHelper.DeleteMemberFromGroup(user, group);
                            return true;
                        }
                        else
                            if (group.DisplayName != coe)
                            return await MoveUserFromGroupToGroup(user, group, coe);
                    }
                    else if (group.Description == "Vertical")
                        if (group.DisplayName != vertical)
                            return await MoveUserFromGroupToGroup(user, group, vertical);
                    else if (group.Description == "Resource_country")
                        if (group.DisplayName != user.Country)
                            return await MoveUserFromGroupToGroup(user, group, user.Country);
                }
            }
            else
            {
                result = await MoveUserToGroup(user, vertical, "Vertical");
                result = result && await MoveUserToGroup(user, user.Country, "Resource_country");
                if (coe.Length != 0)                                    //User that actually has COE assigned else is not moved to a group
                    result = result && await MoveUserToGroup(user, coe, "COE");
                return result;
            }
            return false;
        }
        private static async Task<bool> MoveUserFromGroupToGroup(User user, Group group, string groupDisplayName)
        {
            await GraphHelper.DeleteMemberFromGroup(user, group);
            var targetGroups = await GraphHelper.GetGroupByDisplayName(groupDisplayName);
            Group targeGroup;
            if (targetGroups != null)
                targeGroup = targetGroups;
            else
                targeGroup = await GraphHelper.CreateGroup(groupDisplayName, group.Description);

            if (await GraphHelper.AddMemberToGroup(user, targeGroup) == 0)
                return true;
            else
                return false;
        }
        private static async Task<bool> MoveUserToGroup(User user, string groupDisplayName, string description)
        {
            var targetGroups = await GraphHelper.GetGroupByDisplayName(groupDisplayName);
            Group targeGroup;
            if (targetGroups != null)
                targeGroup = targetGroups;
            else
                targeGroup = await GraphHelper.CreateGroup(groupDisplayName, description);

            if (await GraphHelper.AddMemberToGroup(user, targeGroup) == 0)
                return true;
            else
                return false;
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