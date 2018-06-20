using GenGPSRoute.forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GenHeartbeats
{
    public partial class Form1 : Form
    {
        //string headerRow = "Lat,Lon,Avionics Bat,Alt(ASL),Alt(AGL),RSSI,Speed,Bearing,Yaw,Pitch,Roll,Servo,Status";
        //string standinData = ",12.4,-3589,-10,15,24,238,0.123,0.0,0.445,25.4,10";
        string[] heartbeatArray = new string[110];
        int[] Altitude = new int[110];
        string GMapAPIKey = ConfigurationManager.AppSettings["GoogleMapsAPI"];
        string startLat = ConfigurationManager.AppSettings["originLat"];
        string startLon = ConfigurationManager.AppSettings["originLon"];
        string endLat = ConfigurationManager.AppSettings["destinationLat"];
        string endLon = ConfigurationManager.AppSettings["destinationLon"];
        string numHeartbeats = ConfigurationManager.AppSettings["heartbeats"];
        double routeDistance = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "H e a r t b e a t    G e n e r a t o r - " + Application.ProductVersion;
            //string endLat = "42.184441"; //42.184441, -83.810576 6969 heatheridge
            //string endLon = "-83.810576";

            numericPoints.Value = Convert.ToInt16(numHeartbeats);
            txtStartLat.Text = startLat;
            txtStartLon.Text = startLon;
            txtEndLat.Text = endLat;
            txtEndLon.Text = endLon;

            //lblLatInterval.Text = CalcInterval(Convert.ToDecimal(startLat), Convert.ToDecimal(endLat), numGPSPoints);
            //lblLonInterval.Text = CalcInterval(Convert.ToDecimal(startLon), Convert.ToDecimal(endLon), numGPSPoints);
            //lineArray[0] = headerRow;
            txtAltitudeOrigin.Text = ConfigurationManager.AppSettings["originAltitude"];
            txtAltitudeMax.Text = ConfigurationManager.AppSettings["maxAltitude"];
            txtAscDescRate.Text = ConfigurationManager.AppSettings["ascendDescendRate"];
            txtFlightSpeed.Text = ConfigurationManager.AppSettings["flightSpeed"];
            txtAvionicsBatt.Text = "12.4";
            txtServoBatt.Text = "24.8";
            txtRSSI.Text = "15";
            txtYaw.Text = "12.3";
            txtPitch.Text = "0.3";
            txtRoll.Text = "1.4";
        }

        //private string CalcInterval(decimal start, decimal end, int numPoints)
        //{
        //    return ((end - start) / numPoints).ToString("##.00000000");
        //}

        //private void GenerateHeartbeats()
        //{
        //    int numPoints = (int)numericPoints.Value;
        //    double startLat = Convert.ToDouble(txtStartLat.Text);
        //    double startLon = Convert.ToDouble(txtStartLon.Text);
        //    double endLat = Convert.ToDouble(txtEndLat.Text);
        //    double endLon = Convert.ToDouble(txtEndLon.Text);
        //    double intervalLat = (endLat - startLat)/numPoints;
        //    double intervalLon = (endLon - startLon)/ numPoints;
        //    StringBuilder sb = new StringBuilder();
        //    double lat = 0f;
        //    double lon = 0f;
        //    Array.Resize(ref heartbeatArray, numPoints+1);
        //    for (int i = 0; i <= numPoints; i++)
        //    {
        //        lat = startLat + (i * intervalLat);
        //        lon = startLon + (i * intervalLon);

        //        //sb.Append(lat.ToString("##.00000000")).Append(", ").Append(lon.ToString("##.00000000")).Append(standinData).Append(Environment.NewLine);
        //        //lineArray[i] = lat.ToString("##.00000000") + ", " + lon.ToString("##.00000000") + standinData;
        //        sb.Append(lat.ToString("##.00000000")).Append(", ").Append(lon.ToString("##.00000000")).Append(", " + Altitude[i]).Append(Environment.NewLine);
        //        heartbeatArray[i] = lat.ToString("##.00000000") + ", " + lon.ToString("##.00000000") + standinData;
        //    }
        //    txtOut.Text = sb.ToString();
        //}

        private void calcHeartbeats()
        {
            int numPoints = (int)numericPoints.Value;
            Array.Resize(ref heartbeatArray, numPoints + 1);
            Array.Resize(ref Altitude, numPoints + 1);
            decimal AltitudeOrigin;
            decimal AltitudeMax;
            decimal AltitudeAboveG;
            decimal AscendRate;
            StringBuilder sb = new StringBuilder();
            if (chkFlyteFormat.Checked)
            {
                AltitudeOrigin = (Convert.ToDecimal(txtAltitudeOrigin.Text) * -8);//microPilot conversion factor: -8
                AltitudeMax = (Convert.ToDecimal(txtAltitudeMax.Text) * -8);
                AltitudeAboveG = AltitudeMax - AltitudeOrigin;
                AscendRate = (Convert.ToDecimal(txtAscDescRate.Text) * -8);
            }
            else
            {
                AltitudeOrigin = (Convert.ToDecimal(txtAltitudeOrigin.Text));//microPilot conversion factor: -8
                AltitudeMax = (Convert.ToDecimal(txtAltitudeMax.Text));
                AltitudeAboveG = AltitudeMax - AltitudeOrigin;
                AscendRate = (Convert.ToDecimal(txtAscDescRate.Text));
            }

            try
            {
                //calc number of ascend points to reach max height, assume vertical rise
                //same as number of descend points
                int numOfAscendDescendPoints = (int)Math.Truncate(AltitudeAboveG / AscendRate);
                decimal lat = Convert.ToDecimal(txtStartLat.Text);
                decimal lon = Convert.ToDecimal(txtStartLon.Text);
                decimal startLat = Convert.ToDecimal(txtStartLat.Text);
                decimal startLon = Convert.ToDecimal(txtStartLon.Text);
                decimal endLat = Convert.ToDecimal(txtEndLat.Text);
                decimal endLon = Convert.ToDecimal(txtEndLon.Text);

                decimal flightSpeed = Convert.ToDecimal(txtFlightSpeed.Text);
                decimal avionicsBatt = Convert.ToDecimal(txtAvionicsBatt.Text);
                decimal servoBatt = Convert.ToDecimal(txtServoBatt.Text);
                decimal RSSI = Convert.ToDecimal(txtRSSI.Text);
                decimal yaw = Convert.ToDecimal(txtYaw.Text);
                decimal pitch = Convert.ToDecimal(txtPitch.Text);
                decimal roll = Convert.ToDecimal(txtRoll.Text);
                string bearing = calcBearing();
                int flightStatus = 104;
                flightSpeed = 0;    //vertically rising

                //calc the GPS increment intervals for lat and long
                decimal intervalLat = (endLat - startLat) / (numPoints - numOfAscendDescendPoints*2);
                decimal intervalLon = (endLon - startLon) / (numPoints - numOfAscendDescendPoints*2);

                //resize array to allow for extra points when landing vertically at destination GPS
                Array.Resize(ref heartbeatArray, numPoints);
                Array.Resize(ref Altitude, numPoints);

                //ascend vertically to max altitude at origin GPS 
                for (int i = 0; i <= numOfAscendDescendPoints; i++)
                {
                    Altitude[i] = Convert.ToInt16((AltitudeOrigin) + AscendRate * i);
                    sb.Append(lat.ToString("##.000000"));
                    sb.Append(", ");
                    sb.Append(lon.ToString("##.000000"));
                    sb.Append(", " + avionicsBatt).ToString();
                    sb.Append(", " + Altitude[i]);
                    sb.Append(", " + (Altitude[i] - AltitudeOrigin));
                    sb.Append(", " + RSSI.ToString());
                    sb.Append(", " + flightSpeed.ToString());
                    sb.Append(", " + bearing);
                    sb.Append(", " + yaw.ToString());
                    sb.Append(", " + pitch.ToString());
                    sb.Append(", " + roll.ToString());
                    sb.Append(", " + servoBatt.ToString());
                    //sb.Append(", " + flightStatus.ToString("X2")); //formats in hex
                    sb.Append(", " + flightStatus.ToString());
                    sb.Append(Environment.NewLine);

                    heartbeatArray[i] = lat.ToString("##.000000") + "," +
                        lon.ToString("##.000000") + "," +
                        avionicsBatt.ToString() + "," +
                        Altitude[i] + "," +
                        (Altitude[i] - AltitudeOrigin) + "," +
                        RSSI.ToString() + "," +
                        flightSpeed.ToString() + "," +
                        bearing + "," +
                        yaw.ToString() + "," +
                        pitch.ToString() + "," +
                        roll.ToString() + "," +
                        servoBatt.ToString() + "," +
                        //flightStatus.ToString("X2"); //formats in hex
                        flightStatus.ToString();
                }
                flightStatus = 72;
                flightSpeed = Convert.ToDecimal(txtFlightSpeed.Text);
                //move GPS horizontally at max altitude 
                for (int i = (int)numOfAscendDescendPoints; i <= (numPoints - numOfAscendDescendPoints); i++)
                {
                    //lat = startLat + (i * intervalLat);// this leads to a big gap in points from start
                    //lon = startLon + (i * intervalLon);
                    lat = startLat + ((i- numOfAscendDescendPoints) * intervalLat);
                    lon = startLon + ((i - numOfAscendDescendPoints) * intervalLon);

                    Altitude[i] = Convert.ToInt16(AltitudeMax);
                    sb.Append(lat.ToString("##.000000"));
                    sb.Append(", ");
                    sb.Append(lon.ToString("##.000000"));
                    sb.Append(", " + avionicsBatt).ToString();
                    sb.Append(", " + Altitude[i]);
                    sb.Append(", " + (Altitude[i] - AltitudeOrigin));
                    sb.Append(", " + RSSI.ToString());
                    sb.Append(", " + flightSpeed.ToString());
                    sb.Append(", " + bearing);
                    sb.Append(", " + yaw.ToString());
                    sb.Append(", " + pitch.ToString());
                    sb.Append(", " + roll.ToString());
                    sb.Append(", " + servoBatt.ToString());
                    sb.Append(", " + flightStatus.ToString());
                    sb.Append(Environment.NewLine);

                    heartbeatArray[i] = lat.ToString("##.000000") + "," +
                        lon.ToString("##.000000") + "," +
                        avionicsBatt.ToString() + "," +
                        Altitude[i] + "," +
                        (Altitude[i] - AltitudeOrigin) + "," +
                        RSSI.ToString() + "," +
                        flightSpeed.ToString() + "," +
                        bearing + "," +
                        yaw.ToString() + "," +
                        pitch.ToString() + "," +
                        roll.ToString() + "," +
                        servoBatt.ToString() + "," +
                        flightStatus.ToString();
                }
                flightStatus = 74;
                flightSpeed = 0; // vertically descending
                //when GPS destination is reached, descend vertically to origin altitude 
                for (int i = (numPoints - numOfAscendDescendPoints); i < numPoints; i++)
                {
                    Altitude[i] = Convert.ToInt16((AltitudeMax - (AscendRate * (i - (numPoints - numOfAscendDescendPoints)))));
                    sb.Append(lat.ToString("##.000000"));
                    sb.Append(", ");
                    sb.Append(lon.ToString("##.000000"));
                    sb.Append(", " + avionicsBatt).ToString();
                    sb.Append(", " + Altitude[numPoints - i]);
                    sb.Append(", " + (Altitude[numPoints - i] - AltitudeOrigin));
                    sb.Append(", " + RSSI.ToString());
                    sb.Append(", " + flightSpeed.ToString());
                    sb.Append(", " + bearing);
                    sb.Append(", " + yaw.ToString());
                    sb.Append(", " + pitch.ToString());
                    sb.Append(", " + roll.ToString());
                    sb.Append(", " + servoBatt.ToString());
                    sb.Append(", " + flightStatus.ToString());
                    sb.Append(Environment.NewLine);

                    heartbeatArray[i] = lat.ToString("##.000000") + "," +
                        lon.ToString("##.000000") + "," +
                        avionicsBatt.ToString() + "," +
                        Altitude[numPoints - i] + "," +
                        (Altitude[numPoints - i] - AltitudeOrigin) + "," +
                        RSSI.ToString() + "," +
                        flightSpeed.ToString() + "," +
                        bearing + "," +
                        yaw.ToString() + "," +
                        pitch.ToString() + "," +
                        roll.ToString() + "," +
                        servoBatt.ToString() + "," +
                        flightStatus.ToString();
                }
            }
            catch (Exception ex)
            {
                txtMsg.Text = ex.Message;
            }

            txtOut.Text = sb.ToString();

        }

        //private void calcAltitudeArray()
        //{
        //    int numPoints = (int)numericPoints.Value;
        //    int AltitudeOrigin = Convert.ToInt16(txtAltitudeOrigin.Text);
        //    int AltitudeMax = Convert.ToInt16(txtAltitudeMax.Text);
        //    Array.Resize(ref Altitude, numPoints + 1);
        //    int ascendDescendRate = Convert.ToInt16(txtAscDescRate.Text);
        //    for (int i = 0; i <= numPoints; i++)
        //    {
        //        Altitude[i] = AltitudeOrigin + ascendDescendRate * i;
        //        if (Altitude[i] > AltitudeMax)
        //            Altitude[i] = AltitudeMax;
        //    }

        //    for (int i = 0; i <= 5; i++)
        //        Altitude[numPoints -i] = Altitude[numPoints - i] - ((AltitudeMax - AltitudeOrigin)/5)*(5-i);
        //}


        //private void calcInterval()
        //{
        //    decimal startLat = Convert.ToDecimal(txtStartLat.Text);
        //    decimal startLon = Convert.ToDecimal(txtStartLon.Text);
        //    decimal endLat = Convert.ToDecimal(txtEndLat.Text);
        //    decimal endLon = Convert.ToDecimal(txtEndLon.Text);
        //    int numGPSPoints = (int)numericPoints.Value;

        //    lblLatInterval.Text = CalcInterval(startLat, endLat, numGPSPoints);
        //    lblLonInterval.Text = CalcInterval(startLon, endLon, numGPSPoints);

        //}
        //private void numericPoints_Click(object sender, EventArgs e)
        //{
        //    //decimal startLat = Convert.ToDecimal(txtStartLat.Text);
        //    //decimal startLon = Convert.ToDecimal(txtStartLon.Text);
        //    //decimal endLat = Convert.ToDecimal(txtEndLat.Text);
        //    //decimal endLon = Convert.ToDecimal(txtEndLon.Text);
        //    //int numGPSPoints = (int)numericPoints.Value;

        //    //lblLatInterval.Text = CalcInterval(startLat, endLat, numGPSPoints);
        //    //lblLonInterval.Text = CalcInterval(startLon, endLon, numGPSPoints);

        //}

        private void btnSave_Click(object sender, EventArgs e)
        {
            txtMsg.Text = "";
            try
            {
                if (txtOut.Text.Length > 0)
                {
                    SaveFileDialog savefile = new SaveFileDialog();
                    // set a default file name
                    savefile.FileName = "heartbeat GPS file.csv";
                    // set filters - this can be done in properties as well
                    savefile.Filter = "GPS Heartbeat files (*.csv)|*.csv|All files (*.*)|*.*";

                    if (savefile.ShowDialog() == DialogResult.OK)
                    {
                        System.IO.File.WriteAllLines(savefile.FileName, heartbeatArray);
                    }
                }
            }
            catch (Exception ex)
            {
                txtMsg.Text = ex.Message;
            }
        }

        //private static double FindAngle(double x1, double y1, double x2, double y2)
        //{
        //    double Rad2Deg = 180.0 / Math.PI;
        //    double Deg2Rad = Math.PI / 180.0;
        //    double dx = x2 - x1;
        //    double dy = y2 - y1;
        //    double Angle = Math.Atan2(dy, dx) * Rad2Deg;
        //    if (Angle < 0)
        //    {
        //        Angle = Angle + 360; //This is simular to doing
        //                             // 360 Math.Atan2(y1 - y2, x1 - x2) * (180 / Math.PI)
        //    }
        //    return Angle;
        //}


        private void btnGenHeartbeats_Click(object sender, EventArgs e)
        {
            txtMsg.Text = "";
            calcHeartbeats();
            grpRoute.Enabled = true;
        }

        private string calcBearing()
        {
            double stlat = Convert.ToDouble(txtStartLat.Text);
            double stlon = Convert.ToDouble(txtStartLon.Text);
            double endlat = Convert.ToDouble(txtEndLat.Text);
            double endlon = Convert.ToDouble(txtEndLon.Text);
            double bearing = Bearing.DegreeBearing(stlat, stlon, endlat, endlon);
            if (chkFlyteFormat.Checked)
            {
                bearing = bearing * 100;    //adjust for MP; MicroPilot bearing values range from 0 - 36000
            }
            string returnValue = bearing.ToString("###.0");
            return returnValue;
        }

        private double calcDistance()
        {
            double stlat = Convert.ToDouble(txtStartLat.Text);
            double stlon = Convert.ToDouble(txtStartLon.Text);
            double endlat = Convert.ToDouble(txtEndLat.Text);
            double endlon = Convert.ToDouble(txtEndLon.Text);
            Position pos1 = new Position();
            pos1.Latitude = stlat;
            pos1.Longitude = stlon;

            Position pos2 = new Position();
            pos2.Latitude = endlat;
            pos2.Longitude = endlon;

            Haversine hv = new Haversine();
            double result = hv.Distance(pos1, pos2, DistanceType.Miles);
            return result;
        }

        private void btnMap_Click(object sender, EventArgs e)
        {
            string Lat = txtStartLat.Text;
            string Lon = txtStartLon.Text;
            txtMsg.Text = "";
            try
            {
                string location = Lat + ", " + Lon;
                frmLocationMap frm = new frmLocationMap(Lat, Lon);
                frm.StartPosition = FormStartPosition.CenterScreen;
                frm.MinimizeBox = false;
                frm.MaximizeBox = false;
                frm.ShowDialog();
            }
            catch (Exception ex)
            {
                txtMsg.Text = ex.Message;                
            }
        }

        private void btnMapEnd_Click(object sender, EventArgs e)
        {
            string Lat = txtEndLat.Text;
            string Lon = txtEndLon.Text;
            txtMsg.Text = "";
            try
            {
                string location = Lat + ", " + Lon;
                frmLocationMap frm = new frmLocationMap(Lat, Lon);
                frm.StartPosition = FormStartPosition.CenterScreen;
                frm.MinimizeBox = false;
                frm.MaximizeBox = false;
                frm.ShowDialog();
            }
            catch (Exception ex)
            {
                txtMsg.Text = ex.Message;
            }

        }

        private void btnRoute_Click(object sender, EventArgs e)
        {
            string Lat = txtEndLat.Text;
            string Lon = txtEndLon.Text;
            int numberOfmapmarkers = Convert.ToInt16(numericMapPts.Value);
            if (numberOfmapmarkers >= heartbeatArray.Length)
            {
               MessageBox.Show("The number of map markers can't be greater than the number of heartbeats", "Alert");
            }
            else
            {
                //string location = Lat + ", " + Lon;
                string location = calcRouteCenter();
                string[] gpsLatLon = location.Split(',');
                //frmRoute frm = new frmRoute(location, heartbeatArray, numberOfmapmarkers);
                frmGMapRoute frm = new frmGMapRoute(gpsLatLon[0], gpsLatLon[1], heartbeatArray, numberOfmapmarkers, routeDistance);
                frm.StartPosition = FormStartPosition.CenterScreen;
                frm.MinimizeBox = false;
                frm.MaximizeBox = false;
                frm.ShowDialog();
            }
 

        }
        private string calcRouteCenter()
        {
            double stLat = Convert.ToDouble(txtStartLat.Text);
            double stLon = Convert.ToDouble(txtStartLon.Text);
            double enLat = Convert.ToDouble(txtEndLat.Text);
            double enLon = Convert.ToDouble(txtEndLon.Text);
            double centerLat = (stLat + enLat) / 2;
            double centerLon = (stLon + enLon) / 2;
            return centerLat.ToString() + "," + centerLon.ToString();
        }

        private void txtStartLon_ValueChanged(object sender, EventArgs e)
        {
            updateStats();
        }

        private void txtStartLat_ValueChanged(object sender, EventArgs e)
        {
            updateStats();
        }

        private void txtEndLat_ValueChanged(object sender, EventArgs e)
        {
            updateStats();
        }

        private void txtEndLon_ValueChanged(object sender, EventArgs e)
        {
            updateStats();
        }
        private void updateStats()
        {
            try
            {
                txtMsg.Text = "";
                double bearing = Convert.ToDouble(calcBearing());
                if (chkFlyteFormat.Checked)
                {
                    bearing = bearing / 100; //this is because MP uses bearing * 100
                }
                lblBearing.Text = bearing.ToString("###.#");
                routeDistance = calcDistance();
                lblDistance.Text = routeDistance.ToString("###.0");
            }
            catch (Exception ex)
            {
                txtMsg.Text = ex.Message;
            }
        }

    }
}
