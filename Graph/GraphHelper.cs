using Microsoft.Graph;
using simpleTest_5.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace simpleTest_5.Graph
{
    public class GraphHelper
    {
        public static GraphServiceClient graphClient;
        private static Random rand = new Random();                     //Just for testing porpuses
        public static string extensionId = "extni4xuh4f_extras";
        private static string allUserAttributes = "accountEnabled,ageGroup,businessPhones,city,companyName,consentProvidedForMinor,country,createdDateTime,creationType,department,displayName,employeeId,externalUserState,givenName,id,identities,jobTitle,legalAgeGroupClassification,mail,mobilePhone,officeLocation,onPremisesSyncEnabled,otherMails,postalCode,proxyAddresses,state,streetAddress,surname,usageLocation,userPrincipalName,userType,schools";
        public static void Initialize(IAuthenticationProvider authProvider)
        {
            graphClient = new GraphServiceClient(authProvider);
        }

        public static async Task<User> GetUserByEmail(UserDummie userDummie)
        {
            try
            {
                return await graphClient.Users[userDummie.Email]
                        .Request()
                        .Select("accountEnabled,ageGroup,businessPhones,city,companyName,consentProvidedForMinor,country,createdDateTime,creationType,department,displayName,employeeId,externalUserState,givenName,id,identities,jobTitle,legalAgeGroupClassification,mail,mobilePhone,officeLocation,onPremisesSyncEnabled,otherMails,postalCode,proxyAddresses,state,streetAddress,surname,usageLocation,userPrincipalName,userType,extni4xuh4f_extras")
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
                        .Select(allUserAttributes)
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
                var users = await graphClient.Users
                           .Request()
                           .Select(allUserAttributes)
                           .GetAsync();
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
                User user;
                string extras = "{\"COE\":\"321\",\"Vertical\":\"Adele test02\"}";// "{" + string.Format("\"COE\":\"{0}\",\"Vertical\":\"{1}\"", (newUser.GetCOE() is null || newUser.GetCOE().Length == 0) ? "" : newUser.GetCOE(), (newUser.GetVertical() is null || newUser.GetVertical().Length == 0) ? "" : newUser.GetVertical()) + "}";
                JObject jsonObj = JObject.Parse(extras);

                user = new User
                {
                    AccountEnabled = true,
                    DisplayName = newUser.Name,
                    MailNickname = newUser.Email.Split('@')[0],
                    UserPrincipalName = newUser.Email,
                    PasswordProfile = new PasswordProfile
                    {
                        ForceChangePasswordNextSignIn = false,
                        Password = "Mision31$"
                    },
                    PreferredLanguage = null,
                    GivenName = newUser.Name.Split(' ')[0],
                    Surname = newUser.Name.Split(' ')[1],
                    JobTitle = "Test",
                    Department = "",
                    CompanyName = "PK",
                    EmployeeId = "E"+rand.Next(0,10000).ToString(),
                    StreetAddress = "Felipe Angeles",
                    State = "Queretaro",
                    OfficeLocation = "Parque TEC",
                    City = "Queretaro",
                    PostalCode = "76150",
                    Country = newUser.Resource_country,
                    BusinessPhones = new List<String>()
                    {
                        "1234567890"
                    },
                                        MobilePhone = "0987654321",
                                        Mail = "myEmail7@domail.com",
                                        AdditionalData = new Dictionary<string, object>()
                    {
                        {extensionId, extras}
                    }
                };
                
                return await graphClient.Users.Request().Select(allUserAttributes).AddAsync(user);
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                return null;
            }
        }

        public static async Task<Group> CreateGroup(string groupName, string description)
        {
            try
            {
                var group = new Group
                {
                    Description = description,
                    DisplayName = groupName,
                    GroupTypes = new List<String>()
                    {
                        "Unified"
                    },
                    MailEnabled = false,
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

        public static async Task<List<Group>> GetAllGroups()
        {
            try
            {
                var groups = await graphClient.Groups.Request().Filter("groupTypes/any(c:c eq 'Unified')").GetAsync();
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
        
        public static async Task<Group> GetGroupByDisplayName(string displayName)
        {
            try
            {
                var groups = await graphClient.Groups.Request()
                    .Header("ConsistencyLevel", "eventual")
                    .Header("Content-type", "application/json")
                    .Filter($"startswith(displayName, '{displayName}')")
                    .GetAsync();


                return groups.FirstOrDefault();
            }
            catch (ServiceException ex)
            {
                Console.WriteLine($"Error creating group: {ex.Message}");
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
        public static async Task<int> DeleteGroup(Group group)
        {
            try
            {
                await graphClient.Groups[group.Id].Request().DeleteAsync();
                return 0;
            }
            catch (ServiceException ex)
            {
                //Console.WriteLine($"Error deleting group: {ex.Message}");
                return -1;
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