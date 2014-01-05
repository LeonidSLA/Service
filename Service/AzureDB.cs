using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Locations;
using Microsoft.WindowsAzure.MobileServices;

namespace Service
{
    class AzureDB
    {
        
        private Context _context;
        private MobileServiceClient _client; // Mobile Service Client references
        private IMobileServiceTable<LocationsData> _LocationTable; // Mobile Service Table used to access data

        private const string ApplicationURL = @"https://gpsservice.azure-mobile.net/";
        private const string ApplicationKey = @"rvKyvMhSpXhiHEtlGhbIyZfdudbwvj61";
        public List<LocationsData> _locationFromServer;
        
       

        public AzureDB(Context context)
        {
            try
            {
                _context = context;
                _client = new MobileServiceClient(ApplicationURL, ApplicationKey);
                Log.Debug("AzureDB", "new MobileServiceClient");

                Toast.MakeText(_context, "MobileServiceClient", ToastLength.Long).Show();

                _LocationTable = _client.GetTable<LocationsData>();
                Log.Debug("AzureDB", "_client.GetTable");
                Toast.MakeText(_context, "GetTable", ToastLength.Long).Show();
            }
            catch (Exception e)
            {
                Toast.MakeText(_context, e.ToString(), ToastLength.Long).Show();
                Log.Debug("AzureDB", e.ToString());
            }

        }

        public async void Addlocation(Location location)
        {

            
            LocationsData locationData = new LocationsData { time = DateTime.Now.ToString(), latitude = location.Latitude, longitude = location.Longitude };
            try
            {

                Toast.MakeText(_context, "Addlocation ", ToastLength.Long).Show();
                
                if (_LocationTable != null)
                {
                    CurrentPlatform.Init();

                    await _LocationTable.InsertAsync(locationData);
                    //Toast.MakeText(_context, "InsertAsync", ToastLength.Long).Show();
                    
                }

            }
            catch (Exception e)
            {
                Log.Debug("Addlocation", e.ToString());
                Toast.MakeText(_context, e.ToString(), ToastLength.Long).Show();
            }

        }


        public async Task GetLocationDataFromServerAsync()
        {
            try
            {
                //Get last 3 locations

                _locationFromServer = await _LocationTable.Take(10).ToListAsync();
                Log.Debug("AzureDB", "Where");

            }
            catch (Exception e)
            {
                Log.Debug("GetLocationDataFromServerAsync", e.Message);
                Toast.MakeText(_context, e.ToString(), ToastLength.Long).Show();
            }
        }
        
    }
}