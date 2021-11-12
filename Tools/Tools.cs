using simpleTest_5.Models;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using simpleTest_5.Graph;

namespace simpleTest_5.Tools
{
    public class ToolService
    {
        private string extensionId = GraphHelper.extensionId;
        public List<UserDummie> CreateUsers(string[] usersData)
        {
            
            List<UserDummie> result = new List<UserDummie>();

            foreach(string user in usersData)
            {
                string[] data = user.Split(',');
                result.Add(new UserDummie(data[0], data[1].Split('@')[0]+ "@dlsandbox.onmicrosoft.com", data[2], data[3], data[4]));
            }

            return result;
        }


        public string[] LoadCSV(string path)
        {            
            try
            {
                string[] lines = System.IO.File.ReadAllLines(path);
                return lines;
            }catch(Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return null;
            }
        }
    }
}
