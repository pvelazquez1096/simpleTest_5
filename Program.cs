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


            csv = tool.LoadCSV(path);
            usersDummie = tool.CreateUsers(csv);
            GroupCreation(csv, tool);
            groups = await GraphHelper.ReadAllGroups();
            groups = await AddUsersToCOEGroups(usersDummie, groups);
            Console.ReadKey();
        }
        public static DeviceCodeAuthProvider GetDeviceCodeAuthProvider()
        {
            var appId = "5bba094d-693c-43ba-b50b-9b398591825b";
            var scopesString = "User.Read;User.ReadWrite.All;User.ManageIdentities.All;GroupMember.ReadWrite.All;Group.ReadWrite.All;Group.ReadWrite.All;People.Read.All";
            var scopes = scopesString.Split(';');
            return new DeviceCodeAuthProvider(appId, scopes);
        }
        public static async Task<List<Group>> AddUsersToCOEGroups(List<UserDummie> userList, List<Group> groups_)
        {
            List<Group> groups = groups_;
            bool added;
            int output;
            foreach (UserDummie user in userList)
            {
                added = false;
                output = 0;
                //Console.WriteLine("Getting User");
                User result = await GraphHelper.GetUserByEmail(user);
                if (result == null)
                {
                    //Console.WriteLine("User not found creating User");
                    result = await GraphHelper.CreateUser(user);
                    //Console.WriteLine("User created");
                }
                if (result.Department != null)
                {
                    //Console.WriteLine("Deparment not null");
                    if (groups.Count > 0)
                    {
                        //Console.WriteLine("List group with elements");
                        //Console.WriteLine("Searching if the group is already created");
                        foreach (Group group in groups)
                        {
                            if (group.DisplayName == result.Department)
                            {
                                //Console.WriteLine("Group found");
                                output = await GraphHelper.AddMemberToGroup(result, group);
                                //Console.WriteLine($"User added to the group {output}");
                                added = true;
                                break;
                            }
                        }
                    }
                    if (!added)
                    {
                        //Console.WriteLine("Could not find group... creating a new one");
                        var newGroup = await GraphHelper.CreateGroup(result.Department);
                        output = await GraphHelper.AddMemberToGroup(result, newGroup);
                        //Console.WriteLine($"User added to the group {output}");
                        groups.Add(newGroup);
                        //Console.WriteLine("Group added to the group list");
                    }
                }
                else
                {
                    //Console.WriteLine("User without COE");
                }
            }
            return groups;
        }
        public static async void GroupCreation(string[] csv, ToolService tool)
        {
            //GROUP CREATION
            string[] coe = tool.ListDistinctValues(csv, "COE");
            foreach(string c in coe)
            {
                var group = await GraphHelper.CreateGroup(c);
                //Console.WriteLine($"{group.DisplayName} {group.MailNickname}");
            }
        }

        public static void printInfo(User user)
        {
            Console.WriteLine($"User: {user.DisplayName} Email: {user.UserPrincipalName}");
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