using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace WindowsFormsApp1
{
    class MarkerBilgi
    {
        public GMapMarker marker;
        public string adres;
        public string ad;
        public string soyad;
        public string hastaBilgi;
        public double oncelik;
        public MarkerBilgi(GMapMarker marker, string adres, string ad, string soyad, string hastabilgi, double oncelik)
        {
            this.marker = marker;
            this.adres = adres;
            this.ad = ad;
            this.soyad = soyad;
            this.hastaBilgi = hastabilgi;
            this.oncelik = oncelik;
        }
    }
}
