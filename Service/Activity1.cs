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

                //StartService(new Intent(this, typeof(GpsService.GpsService)));
                Toast.MakeText
                (
                    context,
                    "GPS Service started",
                    ToastLength.Long
                ).Show();

            }
        }
    }

    [Activity(Label = "GpsService", MainLauncher = true)]
    public class MapsActivity : Activity
    {

        Button _button;
        MapFragment _mapFragment;
        GoogleMap _map;
        Handler _handler;
        AzureDB azureRecieveInstance;

        System.Threading.Timer _timer;
        long _updateInterval = 30 * 1000;
                
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            InitMapFragment();

            azureRecieveInstance = new AzureDB(this);
            
            
            _button = FindViewById<Button>(Resource.Id.aButton);
            _button.Text = "Start Service";

            _button.Click += (sender, e) =>
            {
                StartService(new Intent(this, typeof(GpsService)));

                Toast.MakeText
                    (this,
                    "Service started",
                    ToastLength.Long
                    ).Show();

                //this.Finish();
            };


            
            Handler handler = new Handler();
            handler.Post(delegate() { StartServerRequestTimer(); });
            
        }

         



        private void InitMapFragment()
        {
            _mapFragment = FragmentManager.FindFragmentByTag("map") as MapFragment;

            if (_mapFragment == null)
            {
                GoogleMapOptions mapOptions = new GoogleMapOptions()
                    .InvokeMapType(GoogleMap.MapTypeSatellite)
                    .InvokeZoomControlsEnabled(false)
                    .InvokeCompassEnabled(true);

                FragmentTransaction fragTx = FragmentManager.BeginTransaction();
                _mapFragment = MapFragment.NewInstance(mapOptions);
                fragTx.Add(Resource.Id.mapWithOverlay, _mapFragment, "map");
                fragTx.Commit();
            }
            
            _map = _mapFragment.Map;

            _handler = new Handler();

        }
        
        public void StartServerRequestTimer()
        {


            _timer = new System.Threading.Timer((o) =>
            {
                try
                {
                    //periodic update of location provider information


                    if (azureRecieveInstance != null)
                    {
                        Log.Debug("StartServerRequestTimer", "GetLocationAsync");
                        OnRefreshItemsSelected(); 
                    }

                    if (azureRecieveInstance._locationFromServer != null)
                    {
                        Log.Debug("StartServerRequestTimer", "LocationFromServer");
                        _handler.Post(delegate()
                        {
                        DrawPointsOnMap(azureRecieveInstance._locationFromServer);
                        });
                        
                    }


                    Log.Debug("StartServerRequestTimer", "RequestTimerTick");
                    //Toast.MakeText(this, _locationProvider, ToastLength.Long).Show();
                }
                catch (System.Exception e)
                {
                    //Toast.MakeText(this, "Exception" + e.Message, ToastLength.Long).Show();
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

       

        private async void OnRefreshItemsSelected()
        {
            await azureRecieveInstance.GetLocationDataFromServerAsync();
        } 

    }

   
}