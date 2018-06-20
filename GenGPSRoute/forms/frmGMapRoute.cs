using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.WindowsForms.Markers;

namespace GenGPSRoute.forms
{
    public partial class frmGMapRoute : Form
    {
        string[] mapRoute = new string[10];
        double[,] markerArray = new double[1000,2];
        int markerInterval = 0;
        int numberOfMarkers = 25;
        double latitude;
        double longitude;
        double distance;

        public AccessMode Mode { get; private set; }

        public frmGMapRoute(string lat, string lon, string[] routeArray, int mapMarkers, double routeDistance)
        {
            numberOfMarkers = mapMarkers;
            mapRoute = routeArray;
            latitude = Convert.ToDouble(lat);
            longitude = Convert.ToDouble(lon);
            distance = routeDistance;
            InitializeComponent();
        }

        private void frmGMapRoute_Load(object sender, EventArgs e)
        {
            this.Text = latitude.ToString() + ", " + longitude.ToString();
            gMapControl.MapProvider = BingMapProvider.Instance;
            //gMap.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
           Mode = AccessMode.ServerOnly;
            // gMap.SetPositionByKeywords("Paris, France");
            gMapControl.Position = new PointLatLng(latitude, longitude);
            gMapControl.Zoom = calcZoomLevel(distance);
            GMapOverlay markers = new GMapOverlay("markers");
            gMapControl.Overlays.Add(markers);
            try
            {
                int markerPoints = mapRoute.Count();
                //Array.Resize(ref mapRoute, markerPoints);   //resize array to number of points
                //Array.Resize(ref markerArray[0,0], markerPoints);   //resize array to number of points
                markerInterval = markerPoints / numberOfMarkers;

                createMarkerArray(mapRoute, markerPoints);                //create the marker array
                gMapControl.Overlays.Add(markers);
                for (int i = 0; i < markerPoints; i += markerInterval)
                {
                    GMapMarker marker = new GMarkerGoogle(new PointLatLng(markerArray[i,0], markerArray[i,1]), GMarkerGoogleType.yellow_small);
                    markers.Markers.Add(marker);

                }
            }
            catch (Exception ex)
            {
                //lblMsg.Text = ex.Message;
            }
        }

        private void createMarkerArray(string[] routeArray, int markerCount)
        {
            for (int i = 0; i < markerCount; i++)
            {
                var markerLocation = routeArray[i].Split(',');
                markerArray[i, 0] = Convert.ToDouble(markerLocation[0]);
                markerArray[i, 1] = Convert.ToDouble(markerLocation[1]);
            }
            //for (int i = 0; i < markerCount; i += markerInterval)
            //{
            //    var markerLocation = routeArray[i].Split(',');
            //    markerArray[i] = markerLocation[0] + "," + markerLocation[1];
            //}
        }
        private int calcZoomLevel(double distance)
        {
            int zoomLevel = 2;
            if (distance < 10)
                zoomLevel = 14;
            else if (distance >= 10 && distance < 25)
                zoomLevel = 12;
            else if (distance >= 25 && distance < 100)
                zoomLevel = 10;
            else if (distance >= 100 && distance < 250)
                zoomLevel = 9;
            else if (distance >= 250 && distance < 500)
                zoomLevel = 8;
            else if (distance >= 500 && distance < 750)
                zoomLevel = 7;
            else if (distance >= 750 && distance < 1000)
                zoomLevel = 5;
            else
                zoomLevel = 2;

            return zoomLevel;
        }
    }
}
