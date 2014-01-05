using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Org.Json;

using Java.Net;
using Java.IO;
using Java.Lang;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Util;
using Android.Locations;
using Android.Net.Http;



namespace Service
{
    [Service(Label = "GpsService")]
    public class GpsService : Android.App.Service, Android.Locations.ILocationListener 
    {

        AzureDB azureDB;
        Location _currentLocation;
        Criteria _criteriaForLocationService;
        LocationManager _locationManager;
        Availability _gpsStatus;
        string _locationProvider;
        
        System.Threading.Timer _timer;

        string _locationText;
        long _updateInterval = 30*1000;
        
        public override void OnCreate()
        {
            base.OnCreate();

            InitializeLocationManager();

            Toast.MakeText(this, "InitializeLocationManager()", ToastLength.Long).Show();

            azureDB = new AzureDB(this);

            Toast.MakeText(this, "new AzureDB();", ToastLength.Long).Show();

            Handler handler = new Handler();

            handler.Post(delegate() { StartLocationRequestTimer(); });

        }
        public override void OnDestroy()
        {

            

            Log.Debug("SimpleService", "SimpleService stopped");
        }

        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }


        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {

            locationRequest();
            Log.Debug("GPSService", "Location initialized");
                       
                                             
            return StartCommandResult.Sticky;
        }
        public override void OnLowMemory()
        {
            _locationManager.RemoveUpdates(this);

            base.OnLowMemory();

            
            
            Log.Debug("GPSService", "LowMemory");
        }

        




        

        public void OnLocationChanged(Location location) 
        {
            Log.Debug("SimpleService", "OnLocationChanged");
            if (location != null)
            {
                _currentLocation = location;
                _locationText = string.Format("{0}, {1}", location.Latitude, location.Longitude);

                Log.Debug("SimpleService", _locationText);

                WriteLocationTextFile(_locationText, location);

               
                
            }
        }
        
        public void OnProviderDisabled(string provider) 
        {
            Log.Debug("SimpleService", "OnProviderDisabled");
            //locationRequest();
        }

        public void OnProviderEnabled(string provider) 
        {
            Log.Debug("SimpleService", "OnProviderEnabled");
            //locationRequest();  
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras) 
        {
            if (provider == "gps")
            {
                _gpsStatus = status;
                Log.Debug("SimpleService", "_gpsStatus");
            }

            Log.Debug("SimpleService", "OnStatusChanged");
            //locationRequest();
        }

        void InitializeLocationManager()
        {

            _locationManager = (LocationManager)GetSystemService(LocationService);

            _criteriaForLocationService = new Criteria();
            _criteriaForLocationService.Accuracy = Accuracy.NoRequirement;

            locationRequest();            
        }



        public void StartLocationRequestTimer()
        {

            

            _timer = new System.Threading.Timer((o) =>
            {
                try
                {
                    //periodic update of location provider information

                        
                    locationRequest();
                    
                    
                    Log.Debug("SimpleService", "LocationTimerTick");
                    Toast.MakeText(this, _locationProvider, ToastLength.Long).Show();
                }
                catch (System.Exception e)
                {
                    Log.Debug("Exception", e.Message);
                }

            }, null, 0, _updateInterval);
        }

              
        
        private void WriteLocationTextFile(string locationText, Location location)
        {
            try
            {

                string path = Android.OS.Environment.ExternalStorageDirectory.Path;

                var directoryName = Path.Combine(path, "GPSService");
                if (Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                var filename = Path.Combine(directoryName, "locations.txt");


                using (StreamWriter writeFile = new StreamWriter(filename, true))
                {
                    writeFile.WriteLine(" " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + " " + locationText + " " + location.Provider+ " \r\n");
                    writeFile.Close();
                }

                //FTPClient.FTPClient.SendFile(this, filename);
                Toast.MakeText(this, _locationProvider, ToastLength.Long).Show();
                if (azureDB != null)
                { azureDB.Addlocation(location); }

            }
            catch (System.Exception e)
            {
                Toast.MakeText(this, "Exception" + e.Message, ToastLength.Long).Show();
                Log.Debug("Exception", e.Message);
            }


        }

        private void locationRequest()
        {
            try
            {
                //if (_locationProvider == "gps")
                //{
                //    if (_gpsStatus == Availability.OutOfService || _gpsStatus == Availability.TemporarilyUnavailable)
                //    {
                //        _criteriaForLocationService.Accuracy = Accuracy.Coarse;
                //        _locationProvider = _locationManager.GetBestProvider(_criteriaForLocationService, true);
                //        Log.Debug("SimpleService", "network provider was set");
                //    }
                //}
                //else
                //{
                    _locationProvider = _locationManager.GetBestProvider(_criteriaForLocationService, true);
                //}

                _locationManager.RequestLocationUpdates(_locationProvider, _updateInterval, 0, this);
                Log.Debug("SimpleService", "locationRequest()");
            }
            catch(System.Exception e)
            {
                Toast.MakeText(this, "Exception" + e.Message, ToastLength.Long).Show();
                Log.Debug("Exception", e.Message);
            }
        }








        //private void loadPointsFromServer(Location location)
        //{
        //    try
        //    {
        //        string fetchUrl = Constants.kFindPOIUrl + "?latitude="
        //        + location.Latitude + "&longitude="
        //        + location.Longitude + "&radiusInMeters=1000";
        //        URL url = new URL(fetchUrl);

        //        HttpURLConnection urlConnection = (HttpURLConnection)url.OpenConnection();
        //        try
        //        {
        //            BufferedReader r = new BufferedReader(new InputStreamReader(urlConnection.InputStream));

        //            Java.Lang.StringBuilder stringBuilderResult = new Java.Lang.StringBuilder();
        //            string line;
        //            while ((line = r.ReadLine()) != null)
        //            {
        //                stringBuilderResult.Append(line);
        //            }

        //            JSONArray jsonArray = new JSONArray(
        //                    stringBuilderResult.ToString());
        //            for (int i = 0; i < jsonArray.Length(); i++)
        //            {
        //                JSONObject jsonObject = jsonArray.GetJSONObject(i);
        //                double latitude = jsonObject.GetDouble("Latitude");
        //                double longitude = jsonObject.GetDouble("Longitude");
        //                string description = jsonObject.GetString("Description");
        //                string itemUrl = jsonObject.GetString("Url");
        //                // The item URL comes back with quotes at the beginning,
        //                // so we strip them out
        //                itemUrl = itemUrl.Replace("\"", "");

        //                // Create a new geo point with this information and add it
        //                // to the overlay
        //                GeoPoint point = coordinatesToGeoPoint(new double[] { latitude, longitude });
        //                OverlayItem overlayitem = new OverlayItem(point, description, itemUrl);
        //                MarkerOptions markerOptions = new MarkerOptions();
        //                markerOptions.SetPosition(new LatLng(latitude, longitude));
        //                //_map.AddMarker(markerOptions);


        //            }
        //        }
        //        catch (System.Exception ex)
        //        {
        //            Log.Debug("MainActivity", "Error getting data from server: " + ex.Message);
        //        }
        //        finally
        //        {
        //            urlConnection.Disconnect();
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        Log.Debug("MainActivity", "Error creating connection: " + ex.Message);
        //    }
        //}
        
        //private string postPointOfInterestToServer(Location mCurrentLocation)
        //{
        //    try
        //    {
        //        // Make sure we have a location
        //        if (mCurrentLocation == null)
        //        {
        //            return "FAIL-LOCATION";
        //        }

        //        // We've posted succesfully, let's build the JSON data
        //        JSONObject jsonUrl = new JSONObject();
        //        try
        //        {
        //            jsonUrl.Put("Id", 1.ToString());
        //            jsonUrl.Put("Latitude", mCurrentLocation.Latitude);
        //            jsonUrl.Put("Longitude", mCurrentLocation.Longitude);
        //            jsonUrl.Put("Type", 1);
        //        }
        //        catch (JSONException e)
        //        {
        //            Log.Debug("AddPOI", "Exception building JSON: " + e.Message);
        //            e.PrintStackTrace();
        //        }

        //        HttpURLConnection newPOIUrlConnection = null;
        //        URL newPOIUrl = new URL(Constants.kAddPOIUrl);
        //        newPOIUrlConnection = (HttpURLConnection)newPOIUrl.OpenConnection();
        //        newPOIUrlConnection.DoOutput = true;
        //        newPOIUrlConnection.AddRequestProperty("Content-Type", "application/json");
        //        newPOIUrlConnection.SetRequestProperty("Content-Length", "" + Integer.ToString(Encoding.Unicode.GetBytes(jsonUrl.ToString()).Length));
        //        // Write json data to server
        //        DataOutputStream newPoiWR = new DataOutputStream(newPOIUrlConnection.OutputStream);
        //        newPoiWR.WriteBytes(jsonUrl.ToString());
        //        newPoiWR.Flush();
        //        newPoiWR.Close();

        //        return newPOIUrlConnection.ResponseMessage;

        //        // End of post of byte array to server
        //    }
        //    catch (System.Exception ex)
        //    {
        //        Log.Debug("AddPOI", "Error in image upload: " + ex.Message);
        //    }
        //    return "BIGFAIL";
        //}

        //public static GeoPoint coordinatesToGeoPoint(double[] coords)
        //{
        //    if (coords.Length > 2)
        //    {
        //        return null;
        //    }
        //    if (coords[0] == double.NaN || coords[1] == double.NaN)
        //    {
        //        return null;
        //    }
        //    int latitude = (int)(coords[0] * 1E6);
        //    int longitude = (int)(coords[1] * 1E6);
        //    return new GeoPoint(latitude, longitude);
        //}
 






        public class Constants
        {
            public static string kFindPOIUrl = "http://locationserver.azurewebsites.net/api/Location/FindPointsOfInterestWithinRadius";
            public static string kAddPOIUrl = "http://locationserver.azurewebsites.net/api/location/postpointofinterest/";       
        }





    }






}

