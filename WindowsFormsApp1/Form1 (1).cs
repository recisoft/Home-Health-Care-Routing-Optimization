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
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms.ToolTips;
using System.Data.SqlClient;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        List<MarkerBilgi> myMarkerList = new List<MarkerBilgi>();//haritadaki markerları tutar
                                                                //static string constr = "Data Source=.;Initial Catalog = EVDEBAKIM; Integrated Security = True";
                                                                 //SqlConnection conn = new SqlConnection(constr);
        GenetikAlgoritma ga;
        Color renkG=Color.Black;
        int nesil=0;
        public Form1()
        {
            InitializeComponent();
        }

        public void GunDoldur(ComboBox mycombo)
        {
            string sql = "select ID, cast(tarih as varchar(10))+'; '+aciklama as tarih_aciklama from GUN";
            SqlCommand sqlkomut = new SqlCommand(sql,Islemler.conn);
            Islemler.conn.Open();
            SqlDataReader dr = sqlkomut.ExecuteReader();
            DataTable dt = new DataTable("table1");
            dt.Load(dr);
            
            mycombo.DataSource = dt;
            mycombo.ValueMember = "ID";
            mycombo.DisplayMember = "tarih_aciklama";
            dr.Close();
            Islemler.conn.Close();
            
            this.comboBox2.SelectedIndexChanged += new System.EventHandler(this.GunCombo_SelectedIndexChanged);
        }
        public void ilkAtamaDoldur(ComboBox mycombo)
        {
            foreach (IlkAtamaYontem deger in (IlkAtamaYontem[])Enum.GetValues(typeof(IlkAtamaYontem)))
                mycombo.Items.Add(deger.ToString());
            if (mycombo.Items.Count>0)
                mycombo.SelectedIndex = 0;
        }
        public void MarkerEkleTek_eski(Nokta n1, GMarkerGoogleType markerTip, string aciklama, double oncelik)
        {
            //harita üzerine markerlerı ekler
            PointLatLng myNokta = new PointLatLng(n1.lat, n1.lon);
            GMapMarker myMarker = new GMarkerGoogle(myNokta, markerTip);
            myMarker.ToolTipText = aciklama;
            myMarkerList.Add(new MarkerBilgi(myMarker, "adres", "adi","soyadi",aciklama, oncelik));//markerları liste olarak ta tutmak için
            GMapMarkerRect rec = new GMapMarkerRect(myMarker.Position);
            

            GMapOverlay markerlar = new GMapOverlay();
            markerlar.Markers.Add(myMarker); //markerı ekler
            map.Overlays.Add(markerlar);
        }
        public void MarkerEkleTek(Nokta n1, GMarkerGoogleType markerTip, string hastaID, string aciklama, double oncelik)
        {

            //   GMarkerGoogle m= new GMarkerGoogle(n1,)
            //harita üzerine markerlerı ekler
            string resimyol = @"markers\" + hastaID + ".png";
            Bitmap resmim = new Bitmap(resimyol);
            PointLatLng myNokta = new PointLatLng(n1.lat, n1.lon);
            GMarkerGoogle myMarker = new GMarkerGoogle(myNokta,resmim);
           
            myMarker.ToolTipText = aciklama;
            myMarkerList.Add(new MarkerBilgi(myMarker, "adres", "adi", "soyadi", aciklama, oncelik));//markerları liste olarak ta tutmak için
            GMapMarkerRect rec = new GMapMarkerRect(myMarker.Position);
            GMapOverlay markerlar = new GMapOverlay();
            markerlar.Markers.Add(myMarker); //markerı ekler
            map.Overlays.Add(markerlar);
        }
        public void MarkerEkleHepsi(int gunID)
        {
           // string tarih = "01.01.2019"; //programda tarih seçilince seçilen tarih eklencek
            string sql = "exec GunHastaBilgisiGetir ";
            sql += gunID;
            SqlCommand sqlkomut = new SqlCommand(sql, Islemler.conn);
            Islemler.conn.Open();
            SqlDataReader dr = sqlkomut.ExecuteReader();
            Nokta mynokta;
            string aciklamatxt;
            GMarkerGoogleType markerTip;
            map.Overlays.Clear();
            while (dr.Read())
            {
                aciklamatxt = "";
                mynokta.lat = (double)dr["lat"];
                mynokta.lon = (double)dr["lon"];
                aciklamatxt = (string)dr["aciklama"];
                string hastaID = ((int)dr["hastaID"]).ToString();
                aciklamatxt += "\n";
                aciklamatxt += "zc1=" + (int)dr["zc1"];
                aciklamatxt += "   "+ "zc2=" + (int)dr["zc2"];
                if ((int)dr["hastaID"] == 0) markerTip = GMarkerGoogleType.yellow_dot;
                else if ((double)dr["oncelik"] >= 0.9) markerTip = GMarkerGoogleType.red_small;
                else if ((double)dr["oncelik"] >= 0.7) markerTip = GMarkerGoogleType.orange_small;
                else if ((double)dr["oncelik"] >= 0.5) markerTip = GMarkerGoogleType.blue_small;
                else markerTip = GMarkerGoogleType.green_small;
                MarkerEkleTek(mynokta,markerTip, hastaID, aciklamatxt, (double)dr["oncelik"]);
            }
            dr.Close();
            Islemler.conn.Close();
        }
        private void gMapControl1_Load(object sender, EventArgs e)
        {
            map.DragButton = MouseButtons.Left;//haritayı yadırma işlemi 
            map.MapProvider = GMapProviders.GoogleMap;
            map.Manager.Mode = AccessMode.ServerAndCache;
           
            HaritaKonumlan();
        }
        private void HaritaKonumlan()
        {
            double lang= 37.764546;
            double lng=30.556128;
            map.Position = new PointLatLng(lang, lng);//map konum
            map.MinZoom = 3;
            map.MaxZoom = 32;
            map.Zoom = 15;
           // map.SetPositionByKeywords("Isparta, Turkey"); //konumu girilen metne göre ypar
        }

        private void NesilDegistir(int nesil, ListBox listB2, ListBox listB3)
        {
            listB2.Items.Clear();
            listB3.Items.Clear();
            string rotalar = "";
            foreach (Rota myrota in ga.IyiList[nesil].kromozom.rotaListesi)
            {
                listB3.Items.Add("--" + myrota.ekip.ekipID.ToString() + "--");

                rotalar = "RotFit:" + myrota.rotaceza.ToString();
                rotalar += " Team " + myrota.ekip.ekipID.ToString()+">";
                
                foreach (Gen mygen in myrota.ziyaretSirasi)
                {
                    rotalar += " - " + mygen.hasta.hastaID.ToString();
                    rotalar += "[" + mygen.atandigiTimeWindow.t1.ToString() + ":" + mygen.atandigiTimeWindow.t2.ToString() + "]";
                    if (mygen.hasta.hastaID != 0) listB3.Items.Add(mygen.hasta.hastaID);
                }
                listB2.Items.Add(rotalar);
            }
            
        }
        private void map_OnMapZoomChanged()
        {
        //map zoom değişince çalışıyor
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
            this.WindowState = FormWindowState.Maximized;
            Islemler.gunID = 1; //ay_gün_yıl
            var distancematr = Islemler.distanceMatrix;
            List<Ekip> ekiplist = Islemler.ekipListGun;
            List<Hasta> hastalist = Islemler.hastaListGun;
            MarkerEkleHepsi(Islemler.gunID);
            HaritaKonumlan();
            GunDoldur(comboBox2);
            ilkAtamaDoldur(comboBox1);

            // Islemler.HastaEkipYule("01.01.2019");

            // Islemler.tarih = "03.01.2019";
            //distancematr = Islemler.distanceMatrix;
            //Uzaklik u1 = distancematr[new Tuple<int, int>(hastalist[0].hastaID, hastalist[1].hastaID)];
        }

        private void button1_Click(object sender, EventArgs e)
        {
            HaritaKonumlan();
            Application.Restart();
        }
       
        private void button2_Click(object sender, EventArgs e)
        {
            //// genel marker
            //PointLatLng nokta1 = new PointLatLng(37.764546, 30.556128);
            //GMapMarker marker1 = new GMarkerGoogle(nokta1, GMarkerGoogleType.gray_small);
            //marker1.ToolTipText = "Hasta 1";
            //mymarker.Add(new MarkerBilgi(marker1, "adres1", "hasta bilgi1"));

            //PointLatLng nokta2 = new PointLatLng(37.76464, 30.556118);
            //GMapMarker marker2 = new GMarkerGoogle(nokta2, GMarkerGoogleType.lightblue);
            //marker2.ToolTipText = "Hasta 2";
            //mymarker.Add(new MarkerBilgi(marker2, "adres2", "hasta bilgi2"));

            //GMapOverlay noktalar = new GMapOverlay("işaretler");
            //noktalar.Markers.Add(marker1); //markerı ekler
            //noktalar.Markers.Add(marker2);
            //map.Overlays.Add(noktalar);

            // özel marker eklenirse
            PointLatLng nokta3 = new PointLatLng(37.764546, 30.556128);
            var marker3 = new GmapMarkerWithLabel2(nokta3, "nokta 3", GMarkerGoogleType.orange_small);
            marker3.ToolTipText = "marker3";

            PointLatLng nokta4 = new PointLatLng(37.76464, 30.556118);
            var marker4 = new GmapMarkerWithLabel2(nokta4, "nokta 4", GMarkerGoogleType.blue_pushpin);
            marker3.ToolTipText = "marker4";


            GMapOverlay noktalar2 = new GMapOverlay("işaretler");
            noktalar2.Markers.Add(marker3);
           // noktalar2.Markers.Add(marker4);
            map.Overlays.Add(noktalar2);
        }

        private void map_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            //GMapMarker secilenmarker = item;
            MarkerBilgi secilen=null;
            for (int i = 0; i < myMarkerList.Count; i++)
                if (item == myMarkerList[i].marker)
                {
                    secilen = myMarkerList[i];
                    break;
                }
            string mesaj = "Adı:";
            mesaj += secilen.ad + "\n";
            mesaj += "Soyadı:"+secilen.soyad + "\n";
            mesaj += "Adresi:" + secilen.adres+ "\n";
            mesaj += "Hasta Bilgi:" + secilen.hastaBilgi+ "\n";
            mesaj += "Oncelik:" + secilen.oncelik;
            MessageBox.Show(mesaj);
        }
        
        private void button4_Click(object sender, EventArgs e)
        {
            //Islemler.CezaPuanlariniBelirle();
            //int a = Islemler.CezaPuanlari[Cezalar.hataliSkill];
           
            //MessageBox.Show(a.ToString());
            //return;

            listBox1.Items.Clear();
            IlkAtamaYontem atamayontem = IlkAtamaYontem.bestfitteam;
            int populasyonbuyuklugu = 100;

          //  listBox1.Items.Add(atamayontem.ToString());
            List<Kromozom> populasyon = new List<Kromozom>();

            DateTime zaman = DateTime.Now;
          //  listBox1.Items.Add(zaman.ToString());
            int sayac = 0;
            for (int i = 1; populasyon.Count< populasyonbuyuklugu; i++)
            {                           
                Kromozom k = new Kromozom(atamayontem);               
                if (k.atanamayanhastalar.Count==0)
                {
                    sayac++;
                    populasyon.Add(k);
                    for (int j = 0; j < k.rotaListesi.Count; j++)
                    {
                        string yazdir = sayac.ToString()+" "+"K";
                        yazdir += i.ToString() + "->";
                        yazdir += "R" + j.ToString() + ":";
                        for (int l = 0; l < k.rotaListesi[j].ziyaretSirasi.Count; l++)
                        {
                            yazdir += " -- " + k.rotaListesi[j].ziyaretSirasi[l].hasta.hastaID.ToString();
                            yazdir += "[";
                            yazdir += k.rotaListesi[j].ziyaretSirasi[l].atandigiTimeWindow.t1.ToString();
                            yazdir += "-";
                            yazdir += k.rotaListesi[j].ziyaretSirasi[l].atandigiTimeWindow.t2.ToString();
                            yazdir += "]";
                        }
                            listBox1.Items.Add(yazdir);
                    }       
                }                        
            }
             zaman = DateTime.Now;
          //  listBox1.Items.Add(zaman.ToString());


            return;

            Rota r1 = new Rota();
            r1.ekip = Islemler.ekipListGun[0];
            
            foreach (Hasta myhasta in Islemler.hastaListGun)
            {
                if (myhasta.hastaID == 0) continue;
                r1.AtamaYap( myhasta);
            }
            foreach (Gen mygen in r1.ziyaretSirasi)
            {
                string deger = mygen.hasta.hastaID.ToString();
                deger += ">>";
                deger += mygen.atandigiTimeWindow.t1.ToString();
                deger += "-";
                deger += mygen.atandigiTimeWindow.t2.ToString();
                deger += ".........." + mygen.hasta.timeWindow.t1 + "-" + mygen.hasta.timeWindow.t2;
               // listBox1.Items.Add(deger);
            }
                
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            return;
            DateTime t = DateTime.Now;
            string mesaj = "Atama Başl  " + t.ToString();
            listBox1.Items.Add(mesaj);
            Populasyon populasyon = new Populasyon(100,IlkAtamaYontem.bestfitteam);
            t = DateTime.Now;
            mesaj = "Atama Bitş  " + t.ToString();
            listBox1.Items.Add(mesaj);

            t = DateTime.Now;
            mesaj = "Fitness Başl  " + t.ToString();
            listBox1.Items.Add(mesaj);

            populasyon.FitnessHesapla();
            int sayac = 0;
            long toplam = 0;
            foreach (Kromozom mykrom in populasyon.kromozomListesi)
            {
                sayac++;
                mesaj = "Krom: "+sayac.ToString()+" Id:"+mykrom.kromozomId.ToString() + "-->" + mykrom.fitness.ToString();
                listBox1.Items.Add(mesaj);
                toplam += mykrom.fitness;
            }

            t = DateTime.Now;
            mesaj = "Fitness Bitş  " + t.ToString();
            listBox1.Items.Add(mesaj);
            
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Populasyon populasyon = new Populasyon(100, IlkAtamaYontem.bestfitperiod);
            populasyon.FitnessHesapla();
            //Kromozom k1 = populasyon.kromozomListesi[0];
            //Kromozom k2 = populasyon.kromozomListesi[1];
            //populasyon.Caprazla(k1,k2);


            //for (int i=0;i<40;i++)
            //{
            //    Kromozom k1=null;
            //    Kromozom k2=null;
            //    populasyon.KromozomSec(ref k1,ref k2);
            //    populasyon.CaprazlaveEkle(k1,k2);
            //}
            populasyon.Caprazla2Grup(80);

        }

        private void button7_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            listBox3.Items.Clear();
            DateTime zaman = DateTime.Now;
  //          listBox1.Items.Add(zaman);
            Kromozom.IDSifirla();
            IlkAtamaYontem ia=IlkAtamaYontem.firstfit;
            if (comboBox1.Text == "firstfit")
                ia = IlkAtamaYontem.firstfit;
            else if (comboBox1.Text == "bestfitperiod")
                ia = IlkAtamaYontem.bestfitperiod;
            else if (comboBox1.Text == "bestfitteam")
                ia = IlkAtamaYontem.bestfitteam;

            int kromozomsayisi = 0;
            try
            {
                kromozomsayisi = Int32.Parse(kromozomsayisiTxt.Text);
            }
            catch
            {
                MessageBox.Show("Kromozom sayısı değeri sayı olmalıdır");
                return;
            }

            int caprazlamaorani = 0;
            try
            {
                caprazlamaorani = Int32.Parse(caprazlamasayisiTxt.Text);
            }
            catch
            {
                MessageBox.Show("Çaprazlama oranı sayı olmalıdır");
                return;
            }

            int iterasyonsayisi = 0;
            try
            {
                iterasyonsayisi = Int32.Parse(iterasyonsayisiTxt.Text);
            }
            catch
            {
                MessageBox.Show("İterasyon sayısı degeri sayı olmalıdır");
                return;
            }

            ga = new GenetikAlgoritma(ia,kromozomsayisi,caprazlamaorani,5,iterasyonsayisi,true);

            ga.Calistir(listBox1);

            if (Islemler.ilkatamabasarili == false)
            {
                MessageBox.Show("Daha iyi bir rota için farklı bir atama yöntemi kullanın\nSorun devam ederse;\nEkip sayısını artırabilirsiniz\nHasta gereksinimlerini kontrol edin\nHasta zaman periyodlarını kontrol edin");
                Islemler.ilkatamabasarili = true;
            }
            else
                MessageBox.Show("Atamalar tamamlandı");
                
            zaman = DateTime.Now;
            //        listBox1.Items.Add(zaman);

         //   NesilDegistir(iterasyonsayisi-1,listBox2,listBox3);

            ////nesil değiştir yazıldıktan sonra bloklandı
            //listBox2.Items.Clear();
            //listBox3.Items.Clear();
            //string rotalar = "Genel Fitness:";
            //rotalar += ga.populasyon.kromozomListesi[0].fitness.ToString();
            //listBox2.Items.Add(rotalar);
            //foreach (Rota myrota in ga.populasyon.kromozomListesi[0].rotaListesi)
            //{
            //    listBox3.Items.Add("--- " + myrota.ekip.ekipID.ToString() + " ---");

            //    rotalar = "rota fit:" + myrota.rotaceza.ToString();
            //    rotalar += " ekp:" + myrota.ekip.ekipID.ToString();
            //    rotalar += " Güz:";
            //    foreach (Gen mygen in myrota.ziyaretSirasi)
            //    {
            //        rotalar += "-" + mygen.hasta.hastaID.ToString();
            //        rotalar += "(" + mygen.atandigiTimeWindow.t1.ToString() + ":" + mygen.atandigiTimeWindow.t2.ToString() + ";" + mygen.genCeza.ToString() + ")";
            //        if (mygen.hasta.hastaID != 0) listBox3.Items.Add(mygen.hasta.hastaID);
            //    }
            //    listBox2.Items.Add(rotalar);
            //}
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        //private void button8_Click(object sender, EventArgs e)
        //{
        //    string str = listBox1.Text;
        //    int index1 = str.IndexOf("Iter:")+5;
        //    int index2 = str.IndexOf("Fitn:");


        //    listBox2.Items.Clear();
        //    listBox3.Items.Clear();
        //    string rotalar = "";
        //    nesil = Int32.Parse(str.Substring(index1,index2-index1));

        //    NesilDegistir(nesil, listBox2, listBox3);
        //    ////nesil değiştir yazıldıktan sonra kapatıldı
        //    //foreach (Rota myrota in ga.IyiList[nesil].kromozom.rotaListesi)
        //    //{
        //    //    listBox3.Items.Add("--- " + myrota.ekip.ekipID.ToString() + " ---");

        //    //    rotalar = "rota fit:" + myrota.rotaceza.ToString();
        //    //    rotalar += " ekp:" + myrota.ekip.ekipID.ToString();
        //    //    rotalar += " Güz:";
        //    //    foreach (Gen mygen in myrota.ziyaretSirasi)
        //    //    {
        //    //        rotalar += "-" + mygen.hasta.hastaID.ToString();
        //    //     //   rotalar += "(" + mygen.atandigiTimeWindow.t1.ToString() + ":" + mygen.atandigiTimeWindow.t2.ToString() + ";" + mygen.genCeza.ToString() + ")";
        //    //        rotalar += "(" + mygen.atandigiTimeWindow.t1.ToString() + ":" + mygen.atandigiTimeWindow.t2.ToString() + ")";
        //    //        if (mygen.hasta.hastaID != 0) listBox3.Items.Add(mygen.hasta.hastaID);
        //    //    }
        //    //    listBox2.Items.Add(rotalar);
        //    //}
        //}

        private void RouteCiz(Rota myrota, Color renk)
        {
            GMapProviders.GoogleMap.ApiKey = "AIzaSyCwg7XDonTpCAsXdciNKmEImY8Cnp1a41c";
            //var route=GMapProviders.GoogleMap.  .GetRouteBetweenPoints(start, end, false, false, 1);
            map.Overlays.Clear();
            MarkerEkleHepsi(Islemler.gunID);

            if (myrota.ziyaretSirasi.Count <= 2)
                return;

         

            MapRoute route=null;
            GMapOverlay routes=null;
            // map.Overlays.Clear();
            string hastalar = "";
            GMapRoute r=null;
          
            for (int i=0;i<myrota.ziyaretSirasi.Count-1; i++)
            {
                PointLatLng start = new PointLatLng(myrota.ziyaretSirasi[i].hasta.konum.lat, myrota.ziyaretSirasi[i].hasta.konum.lon);
                PointLatLng end = new PointLatLng(myrota.ziyaretSirasi[i+1].hasta.konum.lat, myrota.ziyaretSirasi[i+1].hasta.konum.lon);
                route = GoogleMapProvider.Instance.GetRoute(start, end, false, true, 15);
                r = new GMapRoute(route.Points, myrota.ekip.ekipID.ToString());
                r.Stroke.Width = 2;
                r.Stroke.Color = renk;
                routes = new GMapOverlay("routes");
                routes.Routes.Add(r);
                start = end;
               // end= new PointLatLng(myrota.ziyaretSirasi[i].hasta.konum.lat, myrota.ziyaretSirasi[i].hasta.konum.lon);
                map.Overlays.Add(routes);
                //map.Refresh();
                //hastalar += myrota.ziyaretSirasi[i].hasta.hastaID.ToString() + "-";
            }
         //   map.Refresh();
            map.Zoom = map.Zoom - 1;
            map.Zoom = map.Zoom + 1;
           // MessageBox.Show(hastalar);
          
        }

        private void RouteCiz2(Rota myrota, Color renk)
        {
            GMapProviders.GoogleMap.ApiKey = "AIzaSyCwg7XDonTpCAsXdciNKmEImY8Cnp1a41c";
            //var route=GMapProviders.GoogleMap.  .GetRouteBetweenPoints(start, end, false, false, 1);


            if (myrota.ziyaretSirasi.Count <= 2)
                return;
            MapRoute route2 = null;
            GMapOverlay routes2 = null;
            // map.Overlays.Clear();
            string hastalar = "";
            GMapRoute r2 = null;

            for (int i = 0; i < myrota.ziyaretSirasi.Count - 1; i++)
            {
                PointLatLng start = new PointLatLng(myrota.ziyaretSirasi[i].hasta.konum.lat, myrota.ziyaretSirasi[i].hasta.konum.lon);
                PointLatLng end = new PointLatLng(myrota.ziyaretSirasi[i + 1].hasta.konum.lat, myrota.ziyaretSirasi[i + 1].hasta.konum.lon);
                route2 = GoogleMapProvider.Instance.GetRoute(start, end, false, false, 15);
                r2 = new GMapRoute(route2.Points, myrota.ekip.ekipID.ToString());
                r2.Stroke.Width = 3;
                r2.Stroke.Color = renk;

               
                routes2 = new GMapOverlay("routes");
                routes2.Routes.Add(r2);
                start = end;
                // end= new PointLatLng(myrota.ziyaretSirasi[i].hasta.konum.lat, myrota.ziyaretSirasi[i].hasta.konum.lon);
               
                map.Overlays.Add(routes2);
                map.Refresh();
                hastalar += myrota.ziyaretSirasi[i].hasta.hastaID.ToString() + "-";

              
            }
            map.Refresh();
            map.Zoom = map.Zoom - 1;
            map.Zoom = map.Zoom + 1;
            // MessageBox.Show(hastalar);

        }

        
        //private void button9_Click(object sender, EventArgs e)
        //{
        //    //foreach (Rota myrota in ga.populasyon.kromozomListesi[0].rotaListesi)
        //    //    RouteCiz(myrota);

        //    //  PointLatLng start = new PointLatLng(37.786069, 30.569835);
        //    //  PointLatLng end = new PointLatLng(37.785756, 30.530980);
        //    //  MapRoute route = GoogleMapProvider.Instance.GetRoute(start, end, false, false, 15);
        //    ////  route.Points.Add(new PointLatLng(37.785621, 30.534199));
        //    //  GMapRoute r = new GMapRoute(route.Points, "my route");
        //    //  r.Stroke.Width = 2;
        //    //  r.Stroke.Color = Color.Red;
        //    //  GMapOverlay routesOverlay = new GMapOverlay("routes");
        //    //  routesOverlay.Routes.Add(r);
        //    //  map.Overlays.Add(routesOverlay);
        //    //  map.Refresh();
          
        //        //RouteCiz(ga.populasyon.kromozomListesi[0].rotaListesi[Int32.Parse(textBox2.Text)],renkG);

        // //   RouteCiz(ga.IyiList[nesil].kromozom.rotaListesi[Int32.Parse(textBox2.Text)], renkG);

        //}

        private void button10_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            renkG = colorDialog1.Color;
           
            button10.ForeColor = renkG;
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                System.Text.StringBuilder copy_buffer = new System.Text.StringBuilder();
                foreach (object item in ((ListBox)sender).Items)
                    copy_buffer.AppendLine(item.ToString());
                if (copy_buffer.Length > 0)
                    Clipboard.SetText(copy_buffer.ToString());
            }
        }

        private void listBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                System.Text.StringBuilder copy_buffer = new System.Text.StringBuilder();
                foreach (object item in ((ListBox)sender).Items)
                    copy_buffer.AppendLine(item.ToString());
                if (copy_buffer.Length > 0)
                    Clipboard.SetText(copy_buffer.ToString());
            }
        }

        private void listBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C)
            {
                System.Text.StringBuilder copy_buffer = new System.Text.StringBuilder();
                foreach (object item in ((ListBox)sender).Items)
                    copy_buffer.AppendLine(item.ToString());
                if (copy_buffer.Length > 0)
                    Clipboard.SetText(copy_buffer.ToString());
            }
        }

        private void GunCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            //seçilen güne göre gün yükleyi çalıştıran kodlar
          //  return;
            ComboBox mycombo = (ComboBox)sender;
            Islemler.gunID = Int32.Parse(mycombo.SelectedValue.ToString());
            
            var distancematr = Islemler.distanceMatrix;
            List<Ekip> ekiplist = Islemler.ekipListGun;
            List<Hasta> hastalist = Islemler.hastaListGun;
            MarkerEkleHepsi(Islemler.gunID);
            HaritaKonumlan();


            
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void iterasyonsayisiTxt_TextChanged(object sender, EventArgs e)
        {

        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox lb = (ListBox)sender;
            if (!(lb.SelectedItems.Count > 0)) return; //seçim yoksa işlem yapma
            int rota = lb.SelectedIndex;
            RouteCiz(ga.IyiList[nesil].kromozom.rotaListesi[rota], renkG);

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            ListBox lb = (ListBox)sender;
            if (!(lb.SelectedItems.Count > 0)) return; //seçim yoksa işlem yapma

            string str = lb.Text;
            int index1 = str.IndexOf("Iter:") + 5;
            int index2 = str.IndexOf("Fitn:");


            listBox2.Items.Clear();
            listBox3.Items.Clear();
          
            nesil = Int32.Parse(str.Substring(index1, index2 - index1));

            NesilDegistir(nesil, listBox2, listBox3);
        }
    }
}
