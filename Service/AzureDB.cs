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
        public int _numberOfRequiredPoints=5;
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

                ToastShow.ToastShowMethod(_context, "MobileServiceClient");

                _LocationTable = _client.GetTable<LocationsData>();
                Log.Debug("AzureDB", "_client.GetTable");
                ToastShow.ToastShowMethod(_context, "GetTable");
            }
            catch (Exception e)
            {
                ToastShow.ToastShowMethod(_context, e.ToString());
                Log.Debug("AzureDB", e.ToString());
            }

        }

        public async void Addlocation(Location location)
        {

            
            LocationsData locationData = new LocationsData { time = DateTime.Now.ToString(), latitude = location.Latitude, longitude = location.Longitude };
            try
            {

                ToastShow.ToastShowMethod(_context, "Addlocation ");
                
                if (_LocationTable != null)
                {
                    CurrentPlatform.Init();

                    await _LocationTable.InsertAsync(locationData);
                    //ToastShow.ToastShowMethod(_context, "InsertAsync");
                    
                }

            }
            catch (Exception e)
            {
                Log.Debug("Addlocation", e.ToString());
                ToastShow.ToastShowMethod(_context, e.ToString());
            }

        }


        public async Task GetLocationDataFromServerAsync()
        {
            try
            {
                //Get last 3 locations
                
                //_locationFromServer = await _LocationTable.Take(_numberOfRequiredPoints).ToListAsync();
                _locationFromServer = await _LocationTable.OrderByDescending<string>(locations => locations.time).Take(_numberOfRequiredPoints).ToListAsync();
                               

                Log.Debug("AzureDB", _numberOfRequiredPoints.ToString());

            }
            catch (Exception e)
            {
                Log.Debug("GetLocationDataFromServerAsync", e.Message);
                ToastShow.ToastShowMethod(_context, e.ToString());
            }
        }
        
    }
}