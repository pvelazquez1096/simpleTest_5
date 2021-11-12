using System;
using System.Collections.Generic;
using System.Text;

namespace simpleTest_5.Models
{
    public class UserDummie
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Vertical { get; set; }
        public string Resource_country { get; set; }
        public string Coe { get; set; }
        public UserDummie(string name, string email, string vertical, string resource_country, string coe)
        {
            this.Name = name;
            this.Email = email;
            this.Vertical = vertical;
            this.Resource_country = resource_country;
            this.Coe = coe;
        }
    }
}
