using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft;
namespace simpleTest_5.Models
{
    class CustomProperties
    {
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore, PropertyName = "Vertical", Required = Newtonsoft.Json.Required.Default)]
        public string Vertical { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore, PropertyName = "COE", Required = Newtonsoft.Json.Required.Default)]
        public string COE { get; set; }
    }
}
