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

        public static async Task<User> getUserAsync()
        {
            try
            {
                return (User)await graphClient.Users.Request().GetAsync();
            }catch(ServiceException ex)
            {
                Console.WriteLine($"Error getting users: {ex.Message}");
                return null;
            }
        }
    }
}