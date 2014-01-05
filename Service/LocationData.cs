using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Locations;
using Newtonsoft.Json;


namespace Service
{
    class LocationsData
    {
        [JsonProperty(PropertyName = "id")]
        public string id{ get; set; }

        [JsonProperty(PropertyName = "time")]
        public string time { get; set; }

        [JsonProperty(PropertyName = "longitude")]
        public double longitude { get; set; }

        [JsonProperty(PropertyName = "latitude")]
        public double latitude { get; set; }
        
    }
}