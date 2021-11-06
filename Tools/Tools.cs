﻿using simpleTest_5.Models;
using System;
using System.Collections.Generic;
namespace simpleTest_5.Tools
{
    public class ToolService
    {
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

        public string[] ListDistinctValues(string[] users, string field)
        {
            List<string> coe = new List<string>();
            int column = FieldSelect(field);

            foreach (string line in users)
            {
                string[] columns = line.Split(',');
                if (columns[column].Length != 0)
                    coe.Add(columns[column]);
            }
            return RemoveDuplicates(coe);
        }

        private string[] RemoveDuplicates(List<string> s)
        {
            HashSet<string> set = new HashSet<string>(s);
            string[] result = new string[set.Count];
            set.CopyTo(result);
            return result;
        }

        private int FieldSelect(string field)
        {
            switch (field)
            {
                case "Vertical":
                    return 2;
                case "Resource_Country":
                    return 3;
                case "COE":
                    return 4;
                default:
                    return 4;
            }
        }
    }
}