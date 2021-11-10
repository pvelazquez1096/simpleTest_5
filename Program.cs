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
            string[] csv = tool.LoadCSV(path);

            ////Initialize connection to Microsoft Graph/////
            var authProvider = GetDeviceCodeAuthProvider();
            GraphHelper.Initialize(authProvider);
            /////////////////////////////////////////////////


            
            usersDummie = tool.CreateUsers(csv);
            List<User> usersInAAD = new List<User>();

            //Create Design Group
            Group designGroup = await GraphHelper.CreateGroup("Design");
            Console.WriteLine($"Distribution group: {designGroup.DisplayName} emailAddress: {designGroup.Mail}\n");
            Console.ReadKey();

            //Create users to work with (Testing purposes only)
            for(int i = 0; i < 3; i++)
            {
                User newUser = await GraphHelper.CreateUser(usersDummie.ElementAt(i));
                usersInAAD.Add(newUser);
                Console.WriteLine($"User: {newUser.DisplayName} created!");
            }
            Console.WriteLine("3 users have been created\n");
            Console.ReadKey();

            //Add Users to group Design
            foreach(User user in usersInAAD)
                await GraphHelper.AddMemberToGroup(user, designGroup);
            List<DirectoryObject> membersInGroup = await GraphHelper.GetMembersFromGroup(designGroup);
            foreach(User user in membersInGroup)
                Console.WriteLine($"User: {user.DisplayName} in group: {designGroup.DisplayName}");
            Console.WriteLine("All users added to Design group\n");
            Console.ReadKey();

            //Remove one user from Design group
            Console.WriteLine($"Removing User: {usersInAAD.ElementAt(0).DisplayName} from group: {designGroup.DisplayName}");
            await GraphHelper.DeleteMemberFromGroup(usersInAAD.ElementAt(0), designGroup);
            membersInGroup = await GraphHelper.GetMembersFromGroup(designGroup);
            foreach (User user in membersInGroup)
                Console.WriteLine($"User: {user.DisplayName} in group: {designGroup.DisplayName}");
            Console.WriteLine("One user removed from design group\n");
            Console.ReadKey();

            //Create TestOps Group
            Group testOps = await GraphHelper.CreateGroup("TestOps");
            Console.WriteLine($"Distribution group: {testOps.DisplayName} emailAddress: {testOps.Mail}\n");
            Console.ReadKey();

            //Add two existing users and one new user to TestOps group
            usersInAAD.Add(await GraphHelper.CreateUser(usersDummie.ElementAt(3)));

            for (int i = 1; i < 4; i++)
            {
                Console.WriteLine($"Adding user: {usersInAAD.ElementAt(i).DisplayName} to group TestOps");
                await GraphHelper.AddMemberToGroup(usersInAAD.ElementAt(i), testOps);
            }
            membersInGroup = await GraphHelper.GetMembersFromGroup(testOps);
            foreach (User user in membersInGroup)
                Console.WriteLine($"User: {user.DisplayName} in group: {testOps.DisplayName}");
            Console.WriteLine("3 users added to TestOps group\n");
            Console.ReadKey();

            //Delete one common user
            List<DirectoryObject> groupsInUse = await GraphHelper.GetGroupsFromMember(usersInAAD.ElementAt(1));
            foreach(Group group in groupsInUse)
            {
                Console.WriteLine($"Removing user: {usersInAAD.ElementAt(1).DisplayName} from group: {group.DisplayName}");
                await GraphHelper.DeleteMemberFromGroup(usersInAAD.ElementAt(1), group);
            }
            Console.WriteLine("User deleted from all groups that it belonged to\n");
            Console.ReadKey();

            //Delete the Design Group
            Console.WriteLine($"All groups before deleting Design group");
            foreach (Group group in await GraphHelper.GetAllGroups())
                Console.WriteLine(group.DisplayName);
            await GraphHelper.DeleteGroup(designGroup);
            Console.WriteLine($"All groups after deleting Design group");
            foreach (Group group in await GraphHelper.GetAllGroups())
                Console.WriteLine(group.DisplayName);
            Console.WriteLine("Group Deleted\n");
            Console.ReadKey();

            //Update Test Ops group with a new user
            usersInAAD.Add(await GraphHelper.CreateUser(usersDummie.ElementAt(4)));
            await GraphHelper.AddMemberToGroup(usersInAAD.ElementAt(4), testOps);
            membersInGroup = await GraphHelper.GetMembersFromGroup(testOps);
            foreach (User user in membersInGroup)
                Console.WriteLine($"User: {user.DisplayName} in group: {testOps.DisplayName}");
            Console.WriteLine("1 user added to TestOps group\n");


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