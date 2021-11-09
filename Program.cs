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
            //groups = await GraphHelper.GetAllGroups();
            //groups = await AddUsersToCOEGroups(usersDummie, groups);
            //Create Design Group
            Group designGroup = await GraphHelper.CreateGroup("Design");
            Console.WriteLine($"Distribution group: {designGroup.DisplayName} emailAddress: {designGroup.Mail}");
            Console.ReadKey();

            //Create users to work with (Testing purposes only)
            User user1 = await GraphHelper.CreateUser(usersDummie.ElementAt(0));
            User user2 = await GraphHelper.CreateUser(usersDummie.ElementAt(1));
            User user3 = await GraphHelper.CreateUser(usersDummie.ElementAt(2));
            Console.ReadKey();

            //Add Users to group Design
            await GraphHelper.AddMemberToGroup(user1, designGroup);
            await GraphHelper.AddMemberToGroup(user2, designGroup);
            await GraphHelper.AddMemberToGroup(user3, designGroup);
            Console.WriteLine("All users added to Design group");
            Console.ReadKey();

            //Remove one user from Design group
            await GraphHelper.DeleteMemberFromGroup(user1, designGroup);
            Console.WriteLine("One user removed from design group");
            Console.ReadKey();

            //Create TestOps Group
            Group testOps = await GraphHelper.CreateGroup("TestOps");
            Console.WriteLine($"Distribution group: {testOps.DisplayName} emailAddress: {testOps.Mail}");
            Console.ReadKey();

            //Add to existing users and one new user to TestOps group
            User user4 = await GraphHelper.CreateUser(usersDummie.ElementAt(3));

            await GraphHelper.AddMemberToGroup(user1, testOps);
            await GraphHelper.AddMemberToGroup(user2, testOps);
            await GraphHelper.AddMemberToGroup(user4, testOps);
            Console.WriteLine("All users added to TestOps group");
            Console.ReadKey();

            //Delete one common user
            List<DirectoryObject> groupsInUse = await GraphHelper.GetGroupsFromMember(user2);
            foreach(Group group in groupsInUse)
            {
                await GraphHelper.DeleteMemberFromGroup(user2, group);
            }
            Console.WriteLine("User deleted from all groups that it belonged to");
            Console.ReadKey();

            //Delete the Design Group
            await GraphHelper.DeleteGroup(designGroup);
            Console.WriteLine("Group Deleted");
            Console.ReadKey();

            Console.WriteLine("END");
            Console.ReadKey();
        }
        public static DeviceCodeAuthProvider GetDeviceCodeAuthProvider()
        {
            var appId = "5bba094d-693c-43ba-b50b-9b398591825b";
            var scopesString = "User.Read;User.ReadWrite.All;User.ManageIdentities.All;GroupMember.ReadWrite.All;Group.ReadWrite.All;Group.ReadWrite.All;People.Read.All";
            var scopes = scopesString.Split(';');
            return new DeviceCodeAuthProvider(appId, scopes);
        }
        private static List<Group> checkFieldsChanged(List<DirectoryObject> directory, UserDummie user)
        {
            List<Group> changes = new List<Group>();
            foreach (Group group in directory)
            {
                if (group.Description == "COE")
                {
                    if (group.DisplayName != user.GetCOE())
                        changes.Add(group);
                }else if(group.Description == "Vertical")
                {
                    if (group.DisplayName != user.GetVertical())
                        changes.Add(group);
                }else if(group.Description == "Resource_country")
                {
                    if (group.DisplayName != user.GetResource_country())
                        changes.Add(group);
                }
            }
            return changes;
        }
        public static async Task<List<Group>> AddUserToGroupsDinamically(List<UserDummie> userList)
        {
            List<Group> existingGroups = new List<Group>();
            existingGroups  = await GraphHelper.GetAllGroups();
            foreach (UserDummie user in userList)
            {
                User userFromAAD = await GraphHelper.GetUserByEmail(user);
                List<DirectoryObject> directory = await GraphHelper.GetGroupsFromMember(userFromAAD);
                List<Group> changes = checkFieldsChanged(directory, user);

                foreach(Group change in changes)
                {
                    await GraphHelper.DeleteMemberFromGroup(userFromAAD, change);
                }
                //Crear grupo y luego agregar o agregar y si no crear grupo
                //Como saber si el grupo ya existe?
                //Ademas que los grupos se estan creando como Office 365 y no como Distribution



            }
            return null;
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