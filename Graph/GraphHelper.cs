using Microsoft.Graph;
using simpleTest_5.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace simpleTest_5.Graph
{
    public class GraphHelper
    {
        private static GraphServiceClient graphClient;
        public static void Initialize(IAuthenticationProvider authProvider)
        {
            graphClient = new GraphServiceClient(authProvider);
        }

        public static async Task<User> GetMeAsync()
        {
            try
            {
                // GET /me
                return await graphClient.Me.Request().GetAsync();
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error getting signed-in user: {ex.Message}");
                return null;
            }
        }

        public static async Task<User> GetUserByEmail(UserDummie userDummie)
        {
            try
            {
                return await graphClient.Users[userDummie.GetEmail()]
                        .Request()
                        .Select("accountEnabled,ageGroup,businessPhones,city,companyName,consentProvidedForMinor,country,createdDateTime,creationType,department,displayName,employeeId,externalUserState,givenName,id,identities,jobTitle,legalAgeGroupClassification,mail,mobilePhone,officeLocation,onPremisesSyncEnabled,otherMails,postalCode,proxyAddresses,state,streetAddress,surname,usageLocation,userPrincipalName,userType")
                        .GetAsync();
            }
            catch (ServiceException ex)
            {
                //Console.WriteLine($"Error getting user: {ex.Message}");
                return null;
            }
        }

        public static async Task<User> GetUserByEmail(string userDummie)
        {
            try
            {
                return await graphClient.Users[userDummie]
                        .Request()
                        .Select("accountEnabled,ageGroup,businessPhones,city,companyName,consentProvidedForMinor,country,createdDateTime,creationType,department,displayName,employeeId,externalUserState,givenName,id,identities,jobTitle,legalAgeGroupClassification,mail,mobilePhone,officeLocation,onPremisesSyncEnabled,otherMails,postalCode,proxyAddresses,state,streetAddress,surname,usageLocation,userPrincipalName,userType")
                        .GetAsync();
            }
            catch (ServiceException ex)
            {
                //Console.WriteLine($"Error getting user: {ex.Message}");
                return null;
            }
        }
        public static async Task<List<User>> GetUsers()
        {
            try
            {
                var users = await graphClient.Users.Request().GetAsync();
                List<User> userList = new List<User>();

                while (users.Count > 0)
                {
                    userList.AddRange(users);
                    if (users.NextPageRequest != null)
                    {
                        users = await users.NextPageRequest.GetAsync();
                    }
                    else
                    {
                        break;
                    }
                }

                return userList;
            }
            catch (ServiceException ex)
            {
                //Console.WriteLine($"Error getting users: {ex.Message}");
                return null;
            }
        }

        //TODO Add payload of a real user from db
        public static async Task<User> CreateUser(UserDummie newUser)
        {
            try
            {
                User user = null;
                if (newUser.GetCOE().Length != 0)
                {
                    user = new User
                    {
                        AccountEnabled = true,
                        DisplayName = newUser.GetName(),
                        MailNickname = newUser.GetEmail().Split('@')[0],
                        UserPrincipalName = newUser.GetEmail(),
                        PasswordProfile = new PasswordProfile
                        {
                            ForceChangePasswordNextSignIn = false,
                            Password = "Mision31$"
                        },
                        GivenName = newUser.GetName().Split(' ')[0],
                        Surname = newUser.GetName().Split(' ')[1],
                        JobTitle = "Test",
                        Department = newUser.GetCOE(),
                        CompanyName = "PK",
                        EmployeeId = "E000000",
                        Country = newUser.GetResource_country()
                    };
                }
                else
                {
                    user = new User
                    {
                        AccountEnabled = true,
                        DisplayName = newUser.GetName(),
                        MailNickname = newUser.GetEmail().Split('@')[0],
                        UserPrincipalName = newUser.GetEmail(),
                        PasswordProfile = new PasswordProfile
                        {
                            ForceChangePasswordNextSignIn = false,
                            Password = "Mision31$"
                        },
                        GivenName = newUser.GetName().Split(' ')[0],
                        Surname = newUser.GetName().Split(' ')[1],
                        JobTitle = "Test",
                        CompanyName = "PK",
                        EmployeeId = "E000000",
                        Country = newUser.GetResource_country()
                    };
                }

                return await graphClient.Users.Request().Select("accountEnabled,ageGroup,businessPhones,city,companyName,consentProvidedForMinor,country,createdDateTime,creationType,department,displayName,employeeId,externalUserState,givenName,id,identities,jobTitle,legalAgeGroupClassification,mail,mobilePhone,officeLocation,onPremisesSyncEnabled,otherMails,postalCode,proxyAddresses,state,streetAddress,surname,usageLocation,userPrincipalName,userType").AddAsync(user);
            } catch (ServiceException ex)
            {
                //Console.WriteLine($"Error creating user: {ex.Message}");
                return null;
            }
        }

        public static async Task<Group> CreateGroup(string groupName)
        {
            try
            {
                var group = new Group
                {
                    Description = null,
                    DisplayName = groupName,
                    GroupTypes = new List<String>()
                    {
                        "Unified"
                    },
                    MailEnabled = true,
                    MailNickname = RemoveWhitespace(groupName),
                    SecurityEnabled = false
                };

                return await graphClient.Groups.Request().AddAsync(group);
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error creating group: {ex.Message}");
                return null;
            }
        }

        public static async Task<List<Group>> ReadAllGroups()
        {
            try
            {
                var groups = await graphClient.Groups.Request().GetAsync();
                List<Group> groupList = new List<Group>();

                while (groups.Count > 0)
                {
                    groupList.AddRange(groups);
                    if (groups.NextPageRequest != null)
                        groups = await groups.NextPageRequest.GetAsync();
                    else
                        break;
                }

                return groupList;
            }
            catch (ServiceException ex)
            {
                //Console.WriteLine($"Error creating group: {ex.Message}");
                return null;
            }
        }

        public static async Task<int> AddMemberToGroup(User user, Group group)
        {
            try
            {
                var directoryObject = new DirectoryObject
                {
                    Id = user.Id
                };

                await graphClient.Groups[group.Id].Members.References.Request().AddAsync(directoryObject);
                return 0;
            }
            catch (ServiceException ex)
            {
                //Console.WriteLine($"Error creating group: {ex.Message}");
                return -1;
            }
        }

        public static async Task<List<DirectoryObject>> GetMembersFromGroup(Group group)
        {
            List<User> result = new List<User>();
            try
            {
                var members = await graphClient.Groups[group.Id].Request().Expand("members").GetAsync();

                return members.Members.ToList();
            }
            catch (ServiceException ex)
            {
                //Console.WriteLine($"Error getting members: {ex.Message}");
                return null;
            }
        }

        public static async Task<List<DirectoryObject>> GetMembersFromGroup(String id)
        {
            List<User> result = new List<User>();
            try
            {
                var members = await graphClient.Groups[id].Request().Expand("members").GetAsync();

                return members.Members.ToList();
            }
            catch (ServiceException ex)
            {
                //Console.WriteLine($"Error getting members: {ex.Message}");
                return null;
            }
        }

        public static async Task<List<DirectoryObject>> GetGroupsFromMember(User user)
        {
            List<Group> result = new List<Group>();
            try
            {
                var memberOf = await graphClient.Users[user.Id].MemberOf.Request().GetAsync();

                return memberOf.ToList();
            }
            catch (ServiceException ex)
            {
                //Console.WriteLine($"Error getting members: {ex.Message}");
                return null;
            }
        }

        public static async Task<int> DeleteMemberFromGroup(User user, Group group)
        {
            try
            {
                await graphClient.Groups[group.Id].Members[user.Id].Reference.Request().DeleteAsync();
                return 0;
            }
            catch (ServiceException ex)
            {
                //Console.WriteLine($"Error deleting member from group: {ex.Message}");
                return -1;
            }
        }

        public static async Task<bool> GroupExists(Group group)
        {
            try
            {
                var result = await graphClient.Groups[group.Id].Request().GetAsync();
                return true;
            }
            catch (ServiceException ex)
            {
                //Console.WriteLine($"Error deleting member from group: {ex.Message}");
                return false;
            }
        }
        private static string RemoveWhitespace(string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray()).ToLower();
        }
    }
}