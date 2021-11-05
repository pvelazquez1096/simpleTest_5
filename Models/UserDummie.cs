using System;
using System.Collections.Generic;
using System.Text;

namespace simpleTest_5.Models
{
    public class UserDummie
    {
        private string name;
        private string email;
        private string vertical;
        private string resource_country;
        private string coe;

        public UserDummie(string name, string email, string vertical, string resource_country, string coe)
        {
            this.name = name;
            this.email = email;
            this.vertical = vertical;
            this.resource_country = resource_country;
            this.coe = coe;
        }

        public string GetName()
        {
            return this.name;
        }
        public string GetEmail()
        {
            return this.email;
        }
        public string GetVertical()
        {
            return this.vertical;
        }
        public string GetResource_country()
        {
            return this.resource_country;
        }
        public string GetCOE()
        {
            return this.coe;
        }

        public void print()
        {
            Console.WriteLine($"{this.name} {this.email} {this.vertical} {this.resource_country} {this.coe}");
        }
    }
}
