using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;

using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;




namespace Service
{
    [BroadcastReceiver(Label = "GpsService")]
    [IntentFilter(new[] { Intent.ActionBootCompleted })]
    class BootCompletedBroadcastMessageReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action == Intent.ActionBootCompleted)
            {
               Intent i = new Intent(context, typeof(GpsService));

               context.StartService(i);

               CustomNotification.ShowToastMessage(context, "GPS Service started");

            }
        }
    }

    [Activity(Label = "GpsService", MainLauncher = true)]
    public class MapsActivity : Activity
    {

        Button _StartServiceButton;
        Button _GetLocationsButton;
        MapFragment _mapFragment;
        GoogleMap _map;
        Handler _handler;
        AzureDB _azureRecieveInstance;
        System.Threading.Timer _timer;
        long _updateInterval = 30 * 1000;
        SeekBar _seekBar;


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);
            InitMapFragment();

            _StartServiceButton = FindViewById<Button>(Resource.Id.aButton);
            _StartServiceButton.Text = "Start Service";

            _GetLocationsButton = FindViewById<Button>(Resource.Id.bButton);
            _GetLocationsButton.Text = "Get Locations, 5 points";

            _seekBar = FindViewById<SeekBar>(Resource.Id.seekBar1);
            _seekBar.Max = 20;
            _seekBar.Progress = 5;

            _seekBar.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) =>
            {
                if (e.FromUser)
                {
                    _azureRecieveInstance._numberOfRequiredPoints = e.Progress;
                    _GetLocationsButton.Text = string.Format("Start Service, {0} points", e.Progress);
                }
            };

            _GetLocationsButton.Click += async(sender, e) =>
             {

                if (_azureRecieveInstance != null)
                {
                    await GetDataFromServer();
                    DrawPointsOnMap(_azureRecieveInstance._locationFromServer);
                }
            };

            _StartServiceButton.Click += (sender, e) =>
            {
                StartService(new Intent(this, typeof(GpsService)));
                    CustomNotification.ShowToastMessage(this, "Service started");
                //this.Finish();
            };



            _handler = new Handler();
        }


        protected override void OnPause()
        {
            base.OnPause();

            if (_timer!=null)
                _timer.Dispose();

            Log.Debug("OnPause", "OnPause()");

        }

        protected override void OnResume()
        {
            base.OnResume();
            //if (_timer == null)
            //    _handler.Post(delegate() { StartServerRequestTimer(); });

            Log.Debug("OnResume", "OnResume()");
        }


        protected override void OnStart()
        {
            base.OnStart();
            _azureRecieveInstance = new AzureDB(this);



            //_handler.Post(delegate() { StartServerRequestTimer(); });

            Log.Debug("OnStart", "OnStart()");
        }


        private void InitMapFragment()
        {
            _mapFragment = FragmentManager.FindFragmentByTag("map") as MapFragment;

            if (_mapFragment == null)
            {
                GoogleMapOptions mapOptions = new GoogleMapOptions()
                    .InvokeMapType(GoogleMap.MapTypeHybrid)
                    .InvokeZoomControlsEnabled(false)
                    .InvokeCompassEnabled(true);

                FragmentTransaction fragTx = FragmentManager.BeginTransaction();
                _mapFragment = MapFragment.NewInstance(mapOptions);
                fragTx.Add(Resource.Id.mapWithOverlay, _mapFragment, "map");
                fragTx.Commit();
            }
            
            _map = _mapFragment.Map;

            

        }
        
        public void StartServerRequestTimer()
        {


            _timer = new System.Threading.Timer(async(o) =>
            {
                try
                {
                    //periodic update of location provider information


                    if (_azureRecieveInstance != null)
                    {
                        Log.Debug("StartServerRequestTimer", "GetLocationAsync");
                        await GetDataFromServer(); 
                    }

                    if (_azureRecieveInstance._locationFromServer != null)
                    {
                        Log.Debug("StartServerRequestTimer", "LocationFromServer");
                        _handler.Post(delegate()
                        {
                            DrawPointsOnMap(_azureRecieveInstance._locationFromServer);
                        });
                        
                    }


                    Log.Debug("StartServerRequestTimer", "RequestTimerTick");
                    //ToastShow.ToastShowMethod(this, _locationProvider, ToastLength.Long).Show();
                }
                catch (System.Exception e)
                {
                    //ToastShow.ToastShowMethod(this, "Exception" + e.Message, ToastLength.Long).Show();
                    Log.Debug("StartServerRequestTimer", e.Message);
                }

            }, null, 0, _updateInterval);
        }

        private void DrawPointsOnMap(List<LocationsData> dataList)
        {
            try
            {
                _map = _mapFragment.Map;

                if (_map != null)
                {
                    _map.Clear();
                    
                    Log.Debug("StartServerRequestTimer", "DrawPointsOnMap");

                    foreach (var position in dataList)
                    {
                        
                        
                            LatLng latLang = new LatLng(position.latitude, position.longitude);

                            CameraPosition.Builder builder = CameraPosition.InvokeBuilder();
                            builder.Target(latLang);
                            builder.Zoom(14);
                            
                            CameraPosition cameraPosition = builder.Build();
                            CameraUpdate cameraUpdate = CameraUpdateFactory.NewCameraPosition(cameraPosition);

                            _map.MoveCamera(cameraUpdate);

                            MarkerOptions markerOpt1 = new MarkerOptions();
                            markerOpt1.SetPosition(latLang);
                            markerOpt1.SetTitle(position.time);
                            _map.AddMarker(markerOpt1);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Debug("DrawPointsOnMap", e.ToString());
            }

  
        }

       

        public async System.Threading.Tasks.Task GetDataFromServer()
        {
            await _azureRecieveInstance.GetLocationDataFromServerAsync();
        } 

    }

   
}