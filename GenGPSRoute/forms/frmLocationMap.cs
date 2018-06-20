using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms;
using GMap.NET;

namespace GenGPSRoute.forms
{
    public partial class frmLocationMap : Form
    {
        double latitude = 0;
        double longitude = 0;
        double latitude1 = 43.721667;
        double longitude1 = -79.391548;

        string strLat = "";
        string strLon = "";

        public frmLocationMap(string Lat, string Lon)
        {
            InitializeComponent();
            latitude = Convert.ToDouble(Lat);
            longitude = Convert.ToDouble(Lon);
            strLat = Lat;
            strLon = Lon;
        }

        private void frmLocationMap_Load(object sender, EventArgs e)
        {
            this.Text = strLat + ", " + strLon;
            gMapControl.MapProvider = GMap.NET.MapProviders.BingMapProvider.Instance;
            //gMap.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
            // gMap.SetPositionByKeywords("Paris, France");
            gMapControl.Position = new GMap.NET.PointLatLng(latitude, longitude);

            GMapOverlay markers = new GMapOverlay("markers");
            gMapControl.Overlays.Add(markers);

            GMapMarker marker = new GMarkerGoogle(new PointLatLng(latitude, longitude), GMarkerGoogleType.yellow_small);
            markers.Markers.Add(marker);

            //GMapMarker marker1 = new GMarkerGoogle(new PointLatLng(latitude1, longitude1), GMarkerGoogleType.yellow_small);
            //markers.Markers.Add(marker1);

            gMapControl.IgnoreMarkerOnMouseWheel = true;
            //gMapControl.ShowCenter = false;

        }

    }
}
