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
            
            
            ////Initialize connection to Microsoft Graph/////
            var authProvider = GetDeviceCodeAuthProvider();
            GraphHelper.Initialize(authProvider);
            

            do
            {
                string[] csv = tool.LoadCSV(path);
                List<UserDummie> usersDummie = tool.CreateUsers(csv);
                List<User> fails = await AddUserToGroupsDinamically(usersDummie);
                Console.WriteLine($"Fails: {fails.Count}");
                foreach (User user in fails)
                {
                    Console.WriteLine(user.DisplayName);
                }
                Console.WriteLine("---------------------End---------------------");
                Console.WriteLine("Again? y=yes n=no");
            } while (Console.ReadLine()=="y");
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
                List<DirectoryObject> groupListFromUser = await GraphHelper.GetGroupsFromMember(userFromAAD);
                Console.WriteLine("--------------------------------------------------------");
                Console.WriteLine($"Workin with userAAD: {userFromAAD.DisplayName}");
                bool result = await AddUsersToGroups(groupListFromUser, userFromAAD, user);
                if (!result)
                {
                    Console.WriteLine("User Added incorrectly");
                    usersNotAdded.Add(userFromAAD);
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("User Added correctly");
                }
            }
            return usersNotAdded;
        }
        private static async Task<bool> AddUsersToGroups(List<DirectoryObject> groupListFromUser, User user, UserDummie userdummie)
        {
            bool result = false;
            if (groupListFromUser != null && groupListFromUser.Count > 0)                              //if user already have groups do
            {                                                                                           //Execute the following 
                Console.WriteLine($"    Groups: {groupListFromUser.Count}");
                foreach (Group group in groupListFromUser)
                {
                    Console.WriteLine($"        DisplayName: {group.DisplayName} Description: {group.Description}");
                    if (group.Description == "COE")
                    {
                        if (userdummie.Coe.Length == 0)
                        {
                            Console.WriteLine($"            No COE group");
                            await GraphHelper.DeleteMemberFromGroup(user, group);
                            result = true;
                        }
                        else if (group.DisplayName != userdummie.Coe)
                        {
                            Console.WriteLine($"            Moving COE");
                            result = await MoveUserFromGroupToGroup(user, group, userdummie.Coe, "COE");
                        }
                    }
                    if (group.Description == "Vertical")
                    {
                        if (group.DisplayName != userdummie.Vertical)
                        {
                            Console.WriteLine($"            Moving Vertical");
                            result = await MoveUserFromGroupToGroup(user, group, userdummie.Vertical, "Vertical");
                        }
                    }
                    if (group.Description == "Resource_country")
                    {
                        if (group.DisplayName != user.Country)
                        {
                            Console.WriteLine($"            Moving Resource_country");
                            result = await MoveUserFromGroupToGroup(user, group, userdummie.Resource_country, "Resource_country");
                        }
                    }
                }
            }
            else                                                                                        //If user is new or doesn't have groups
            {
                Console.WriteLine($"    No groups assigned yet");
                result = await MoveUserToGroup(user, userdummie.Vertical, "Vertical");
                result = result && await MoveUserToGroup(user, userdummie.Resource_country, "Resource_country");
                if (userdummie.Coe.Length != 0)                                                         //User that actually has COE assigned else is not moved to a group
                    result = result && await MoveUserToGroup(user, userdummie.Coe, "COE");
            }
            return result;
        }
        private static async Task<bool> MoveUserFromGroupToGroup(User user, Group group, string groupDisplayName, string description)
        {
            Console.WriteLine($"                Deleting user: {user.DisplayName} from group: {group.DisplayName}");
            await GraphHelper.DeleteMemberFromGroup(user, group);
            return await MoveUserToGroup(user, groupDisplayName, description);
        }
        private static async Task<bool> MoveUserToGroup(User user, string groupDisplayName, string description)
        {
            Group targeGroup = await GraphHelper.GetGroupByDisplayName(groupDisplayName);       //try to get group call "groupDisplayName"
            if (targeGroup == null)
            {                                                                                   //returns null if not exists
                Console.WriteLine($"                    No group call: {groupDisplayName} found...creating one");
                targeGroup = await GraphHelper.CreateGroup(groupDisplayName, description);      //create group with DisplayName "groupDisplayName" and Description "COE/Vertical/Resource_country"
            }
            Console.WriteLine($"                    Adding user: {user.DisplayName} to group: {targeGroup.DisplayName} Description: {targeGroup.Description}");
            if (await GraphHelper.AddMemberToGroup(user, targeGroup) == 0)                      //0 means user added succesfully to group
                return true;
            else
                return false;
        }
    }
}
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
/*
foreach(UserDummie user in usersDummie)
            {
                User _user = await GraphHelper.CreateUser(user);
                Console.WriteLine($"{_user.DisplayName} {_user.UserPrincipalName} {_user.Country}");
            }
 */