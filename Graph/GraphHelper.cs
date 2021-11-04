using Microsoft.Graph;
using System;
using System.Collections.Generic;
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

        public static async Task<User> getMeAsync()
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

        public static async Task<List<User>> getUserAsync()
        {
            try
            {
                var users = await graphClient.Users.Request().GetAsync();
                List<User> userList = new List<User>();
                
                while(users.Count > 0)
                {
                    userList.AddRange(users);
                    if(users.NextPageRequest != null)
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
            catch(ServiceException ex)
            {
                Console.WriteLine($"Error getting users: {ex.Message}");
                return null;
            }
        }
        
        //TODO Add payload of a real user
        public static async Task<User> createUserAsync()
        {
            try
            {
                var user = new User
                {
                    AccountEnabled = true,
                    DisplayName = "User8",
                    MailNickname = "user8",
                    UserPrincipalName = "user8@dlsandbox.onmicrosoft.com",
                    PasswordProfile = new PasswordProfile
                    {
                        ForceChangePasswordNextSignIn = false,
                        Password = "Mision31$"
                    },
                    PreferredLanguage = null,
                    GivenName = "User 8",
                    Surname = "Number 8",
                    JobTitle = "Recruiter",
                    Department = "HR",
                    CompanyName = "PK",
                    EmployeeId = "E000007",
                    StreetAddress = "Felipe Angeles",
                    State = "Queretaro",
                    OfficeLocation = "Parque TEC",
                    City = "Queretaro",
                    PostalCode = "76150",
                    BusinessPhones = new List<String>()
                    {
                        "1234567890"
                    },
                    MobilePhone = "0987654321",
                    Mail = "myEmail8@domail.com"
                };
                
                var result = await graphClient.Users.Request().AddAsync(user);
                await graphClient.Users["user8@dlsandbox.onmicrosoft.com"].Manager.Reference
                                        .Request()
                                        .PutAsync("pedro@dlsandbox.onmicrosoft.com");

                return result;
            }catch (ServiceException ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
                return null;
            }
        }
    }
}