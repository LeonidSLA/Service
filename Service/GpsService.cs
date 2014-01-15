using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Timers;
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
using Android.Telephony;


namespace Service
{
    [Service(Label = "GpsService")]
    public class GpsService : Android.App.Service, Android.Locations.ILocationListener 
    {
        System.Threading.Timer _GpsQualityTimer;
        Handler _handler;

        AzureDB azureDB;
        Location _currentLocation;
        Criteria _criteriaForLocationService;
        LocationManager _locationManager;
        Availability _gpsStatus;
        string _locationProvider;
        private System.Timers.Timer _gpsUpdateTimer;
        System.Threading.Timer _locationRequestTimer;
        string _locationText;
        //long _updateInterval = 40 * 1000;
        long _updateInterval = 20 * 60 * 1000;
        string _telephonyDeviceID;
        int i=0;

        public override void OnCreate()
        {
            base.OnCreate();

            TelephonyManager telephonyManager = (TelephonyManager)this.ApplicationContext.GetSystemService(Context.TelephonyService);

            if (telephonyManager != null)
            {
                if (!string.IsNullOrEmpty(telephonyManager.DeviceId))
                    _telephonyDeviceID = telephonyManager.DeviceId;
            }

            InitializeLocationManager();

            ToastShow.ToastShowMethod(this, "InitializeLocationManager()");
            
            azureDB = new AzureDB(this);

            ToastShow.ToastShowMethod(this, "new AzureDB();");

            _handler = new Handler();

            _handler.Post(delegate() { StartLocationRequestTimerStart(); });

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

            //locationRequest();
            Log.Debug("GPSService", "Location initialized");
                       
                                             
            return StartCommandResult.Sticky;
        }
        public override void OnLowMemory()
        {
            //_locationManager.RemoveUpdates(this);

            base.OnLowMemory();

            
            
            Log.Debug("GPSService", "LowMemory");
        }

        
               

        public void OnLocationChanged(Location location) 
        {
            Log.Debug("SimpleService", "OnLocationChanged");
            if (location != null)
            {
                

                //end of gps quality timer started on locationRequest 
                if (_GpsQualityTimer != null)
                {
                    qualityTimerStop();
                }
                
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

            //locationRequest();            
        }



        public void StartLocationRequestTimerStart()
        {

            

            _locationRequestTimer = new System.Threading.Timer((o) =>
            {
                try
                {
                    //periodic update of location provider information

                    locationRequest();

                    Log.Debug("SimpleService", "LocationTimerTick");
                    //ToastShow.ToastShowMethod(this, _locationProvider);
                    i++;
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

                //string path = Android.OS.Environment.ExternalStorageDirectory.Path;

                //var directoryName = Path.Combine(path, "GPSService");
                //if (Directory.Exists(directoryName))
                //{
                //    Directory.CreateDirectory(directoryName);
                //}
                //var filename = Path.Combine(directoryName, "locations.txt");


                //using (StreamWriter writeFile = new StreamWriter(filename, true))
                //{
                //    writeFile.WriteLine(" " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + " " + locationText + " " + location.Provider+ " \r\n");
                //    writeFile.Close();
                //}

                //FTPClient.FTPClient.SendFile(this, filename);
                ToastShow.ToastShowMethod(this, _locationProvider);
                if (azureDB != null)
                { azureDB.Addlocation(location, _telephonyDeviceID,i); }

            }
            catch (System.Exception e)
            {
                //ToastShow.ToastShowMethod(this, "Exception" + e.Message);
                Log.Debug("Exception", e.Message);
            }


        }

        private void locationRequest()
        {
            try
            {
                Log.Debug("SimpleService", "locationRequest()");

                _criteriaForLocationService.Accuracy = Accuracy.Fine;
                _locationProvider = _locationManager.GetBestProvider(_criteriaForLocationService, true);


                _handler.Post(delegate() { _locationManager.RequestLocationUpdates(_locationProvider, _updateInterval, 0, this); });

                //selecting Network provider if gps is not responding for a long time 
                if (_locationProvider == "gps")
                {
                    
                    if (_GpsQualityTimer != null)
                    {
                        _GpsQualityTimer.Dispose();
                    }

                    qualityTimerStart();

                }

                Log.Debug("SimpleService", "Provider "+_locationProvider.ToString());
                
            }
            catch(System.Exception e)
            {
                //ToastShow.ToastShowMethod(this, "Exception" + e.Message);
                Log.Debug("Exception", e.Message);
            }
        }



        private void qualityTimerStart()
        {
            int interval = 40000;
            Log.Debug("SimpleService", "qualityTimerStart()");

            // Create a timer with a ten second interval.
            _gpsUpdateTimer = new System.Timers.Timer(interval+500);

            // Hook up the Elapsed event for the timer.
            _gpsUpdateTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);

            // Set the Interval to 20 seconds (20000 milliseconds).
            _gpsUpdateTimer.Interval = interval;
            _gpsUpdateTimer.Enabled = true;
     
        }

        

        private void qualityTimerStop()
        {
            Log.Debug("SimpleService", "qualityTimerStop()");
            _gpsUpdateTimer.Dispose();
        }


        // Specify what you want to happen when the Elapsed event is raised. 
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            qualityTimerStop();
            Log.Debug("SimpleService", "OnTimedEvent()");
            
            _criteriaForLocationService.Accuracy = Accuracy.Coarse;
            _criteriaForLocationService.PowerRequirement = Power.Low;
            _locationProvider = _locationManager.GetBestProvider(_criteriaForLocationService, true);

            _handler.Post(delegate()
            {
                _locationManager.RemoveUpdates(this);
                
                _locationManager.RequestLocationUpdates(_locationProvider, _updateInterval, 0, this);
            });
        }

        public class Constants
        {
            public static string kFindPOIUrl = "http://locationserver.azurewebsites.net/api/Location/FindPointsOfInterestWithinRadius";
            public static string kAddPOIUrl = "http://locationserver.azurewebsites.net/api/location/postpointofinterest/";       
        }


                            


    }






}

