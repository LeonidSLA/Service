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
using Microsoft.WindowsAzure.MobileServices;

namespace Service
{
    
    class LocationsData
    {
        [JsonProperty(PropertyName = "id")]
        public string id{ get; set; }

        [JsonProperty(PropertyName = "number")]
        public int number { get; set; }

        [JsonProperty(PropertyName = "time")]
        public string time { get; set; }

        [JsonProperty(PropertyName = "longitude")]
        public double longitude { get; set; }

        [JsonProperty(PropertyName = "latitude")]
        public double latitude { get; set; }

        [JsonProperty(PropertyName = "phoneId")]
        public string phoneId { get; set; }
        
    }

    public static class ToastShow
    {
        public static void ToastShowMethod(Context context, string text)
        {
            bool isToastShow=false;

            if (isToastShow)
            { Toast.MakeText(context, text, ToastLength.Long); }
        }
    }
}