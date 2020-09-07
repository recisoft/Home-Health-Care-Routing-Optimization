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
        NewGredyAlgorithm nga;

        double clicklat = 0;
        double cliclon = 0;

        int secim_ga_gredy = 0;//1 ga seçim, 2 gredy seçim, 0 ikiside değil
        Color renkG=Color.Black;
        float penwidth = 1;
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
        public void MarkerEkleTek_eski(Nokta n1, GMarkerGoogleType markerTip,string hastaID, string aciklama, double oncelik)
        {
            //artık kullanılmıyor
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
                string hastaID = ((int)dr["hastaID"]).ToString();
                string gosterID = ((int)dr["gosterID"]).ToString();

                aciklamatxt = (string)dr["aciklama"];
                 aciklamatxt += "    "+"Görünen ID=" + gosterID;
                aciklamatxt += "\n";
                
                aciklamatxt += "zc1=" + (int)dr["zc1"];
                aciklamatxt += "  "+ "zc2=" + (int)dr["zc2"];
                aciklamatxt += "  " + "bs=" + (int)dr["bakimsure"];
                aciklamatxt += "\n";
                aciklamatxt += mynokta.lat.ToString()+","+mynokta.lon.ToString();
                if ((int)dr["hastaID"] == 0) markerTip = GMarkerGoogleType.yellow_dot;
                else if ((double)dr["oncelik"] >= 0.9) markerTip = GMarkerGoogleType.red_small;
                else if ((double)dr["oncelik"] >= 0.7) markerTip = GMarkerGoogleType.orange_small;
                else if ((double)dr["oncelik"] >= 0.5) markerTip = GMarkerGoogleType.blue_small;
                else markerTip = GMarkerGoogleType.green_small;
                MarkerEkleTek(mynokta,markerTip, gosterID, aciklamatxt, (double)dr["oncelik"]);
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

                rotalar = "RF:" + myrota.rotaceza.ToString();
                rotalar += ";RM:" + myrota.toplamUzaklik.metre.ToString();
                rotalar += ";RP:" + myrota.toplamUzaklik.dakika.ToString();
                rotalar += ";Ek" + myrota.ekip.ekipID.ToString()+">";
                
                foreach (Gen mygen in myrota.ziyaretSirasi)
                {
                    rotalar += " - " + mygen.hasta.gosterID.ToString();
                    rotalar += "[" + mygen.atandigiTimeWindow.t1.ToString() + ":" + mygen.atandigiTimeWindow.t2.ToString() + "]";
                    if (mygen.hasta.hastaID != 0) listB3.Items.Add(mygen.hasta.gosterID);
                }
                listB2.Items.Add(rotalar);
            }           
        }

        private void RotalarıDoldurLoad(List<Rota> RotaList, ListBox listB2, ListBox listB3)
        {
            listB2.Items.Clear();
            listB3.Items.Clear();
            string rotalar = "";
            foreach (Rota myrota in RotaList)
            {
                listB3.Items.Add("--" + myrota.ekip.ekipID.ToString() + "--");

                rotalar = "RF:" + myrota.rotaceza.ToString();
                rotalar += ";RM:" + myrota.toplamUzaklik.metre.ToString();
                rotalar += ";RP:" + myrota.toplamUzaklik.dakika.ToString();
                rotalar += ";Ek" + myrota.ekip.ekipID.ToString() + ">";

                foreach (Gen mygen in myrota.ziyaretSirasi)
                {
                    rotalar += " - " + mygen.hasta.gosterID.ToString();
                    rotalar += "[" + mygen.atandigiTimeWindow.t1.ToString() + ":" + mygen.atandigiTimeWindow.t2.ToString() + "]";
                    if (mygen.hasta.hastaID != 0) listB3.Items.Add(mygen.hasta.gosterID);
                }
                listB2.Items.Add(rotalar);
            }
            listB2.Refresh();
            listB3.Refresh();
        }

        private void GredyRoralar(NewGredyAlgorithm gra, ListBox listB2, ListBox listB3)
        {
            listB2.Items.Clear();
            listB3.Items.Clear();
            string rotalar = "";
            foreach (Rota myrota in gra.rotaList)
            {
                listB3.Items.Add("--" + myrota.ekip.ekipID.ToString() + "--");

                //rotalar = "RotFit:" + myrota.rotaceza.ToString();
                //rotalar += " Team " + myrota.ekip.ekipID.ToString() + ">";

                rotalar = "RF:" + myrota.rotaceza.ToString();
                rotalar += ";RM:" + myrota.toplamUzaklik.metre.ToString();
                rotalar += ";RP:" + myrota.toplamUzaklik.dakika.ToString();
                rotalar += ";Ek" + myrota.ekip.ekipID.ToString() + ">";

                foreach (Gen mygen in myrota.ziyaretSirasi)
                {
                    rotalar += " - " + mygen.hasta.gosterID.ToString();
                    rotalar += "[" + mygen.atandigiTimeWindow.t1.ToString() + ":" + mygen.atandigiTimeWindow.t2.ToString() + "]";
                    if (mygen.hasta.hastaID != 0) listB3.Items.Add(mygen.hasta.gosterID);
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
              

            Islemler.CezaPuanlariniBelirle();
            this.WindowState = FormWindowState.Maximized;
            Islemler.gunID = 1; //ay_gün_yıl
            var distancematr = Islemler.distanceMatrix;
            List<Ekip> ekiplist = Islemler.ekipListGun;
            List<Hasta> hastalist = Islemler.hastaListGun;
          //  MarkerEkleHepsi(Islemler.gunID);
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
        
        //private void button4_Click(object sender, EventArgs e)
        //{
        //    //deneme için yazılmıştı kullanılmıyor
        //    //Islemler.CezaPuanlariniBelirle();
        //    //int a = Islemler.CezaPuanlari[Cezalar.hataliSkill];
           
        //    //MessageBox.Show(a.ToString());
        //    //return;

        //    listBox1.Items.Clear();
        //    IlkAtamaYontem atamayontem = IlkAtamaYontem.bestfitteam;
        //    int populasyonbuyuklugu = 100;

        //  //  listBox1.Items.Add(atamayontem.ToString());
        //    List<Kromozom> populasyon = new List<Kromozom>();

        //    DateTime zaman = DateTime.Now;
        //  //  listBox1.Items.Add(zaman.ToString());
        //    int sayac = 0;
        //    for (int i = 1; populasyon.Count< populasyonbuyuklugu; i++)
        //    {                           
        //        Kromozom k = new Kromozom(atamayontem);               
        //        if (k.atanamayanhastalar.Count==0)
        //        {
        //            sayac++;
        //            populasyon.Add(k);
        //            for (int j = 0; j < k.rotaListesi.Count; j++)
        //            {
        //                string yazdir = sayac.ToString()+" "+"K";
        //                yazdir += i.ToString() + "->";
        //                yazdir += "R" + j.ToString() + ":";
        //                for (int l = 0; l < k.rotaListesi[j].ziyaretSirasi.Count; l++)
        //                {
        //                    yazdir += " -- " + k.rotaListesi[j].ziyaretSirasi[l].hasta.hastaID.ToString();
        //                    yazdir += "[";
        //                    yazdir += k.rotaListesi[j].ziyaretSirasi[l].atandigiTimeWindow.t1.ToString();
        //                    yazdir += "-";
        //                    yazdir += k.rotaListesi[j].ziyaretSirasi[l].atandigiTimeWindow.t2.ToString();
        //                    yazdir += "]";
        //                }
        //                    listBox1.Items.Add(yazdir);
        //            }       
        //        }
        //        else
        //        {
        //            listBox1.Items.Add("---------Popülasyon" + i.ToString() + "---------");
        //            for (int j = 0; j < k.atanamayanhastalar.Count; j++)
        //                listBox1.Items.Add(k.atanamayanhastalar[j].hastaID.ToString());
        //        }
              
        //    }
        //     zaman = DateTime.Now;
        //  //  listBox1.Items.Add(zaman.ToString());


        //    return;

        //    //Rota r1 = new Rota();
        //    //r1.ekip = Islemler.ekipListGun[0];

        //    //foreach (Hasta myhasta in Islemler.hastaListGun)
        //    //{
        //    //    if (myhasta.hastaID == 0) continue;
        //    //    r1.AtamaYap(myhasta);
        //    //}
        //    //foreach (Gen mygen in r1.ziyaretSirasi)
        //    //{
        //    //    string deger = mygen.hasta.hastaID.ToString();
        //    //    deger += ">>";
        //    //    deger += mygen.atandigiTimeWindow.t1.ToString();
        //    //    deger += "-";
        //    //    deger += mygen.atandigiTimeWindow.t2.ToString();
        //    //    deger += ".........." + mygen.hasta.timeWindow.t1 + "-" + mygen.hasta.timeWindow.t2;
        //    //    listBox1.Items.Add(deger);
        //    //}

        //}

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            return;
            //DateTime t = DateTime.Now;
            //string mesaj = "Atama Başl  " + t.ToString();
            //listBox1.Items.Add(mesaj);
            //Populasyon populasyon = new Populasyon(100,IlkAtamaYontem.bestfitteam);
            //t = DateTime.Now;
            //mesaj = "Atama Bitş  " + t.ToString();
            //listBox1.Items.Add(mesaj);

            //t = DateTime.Now;
            //mesaj = "Fitness Başl  " + t.ToString();
            //listBox1.Items.Add(mesaj);

            //populasyon.FitnessHesapla();
            //int sayac = 0;
            //long toplam = 0;
            //foreach (Kromozom mykrom in populasyon.kromozomListesi)
            //{
            //    sayac++;
            //    mesaj = "Krom: "+sayac.ToString()+" Id:"+mykrom.kromozomId.ToString() + "-->" + mykrom.fitness.ToString();
            //    listBox1.Items.Add(mesaj);
            //    toplam += mykrom.fitness;
            //}

            //t = DateTime.Now;
            //mesaj = "Fitness Bitş  " + t.ToString();
            //listBox1.Items.Add(mesaj);
            
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
            secim_ga_gredy = 1;
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            listBox3.Items.Clear();
            Islemler.atanamayanHastalarListesi.Clear();
            DateTime zaman = DateTime.Now;
            label9.Text = zaman.ToString();

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

            bool elitizmeniyi = true;
            //if (comboBox3.SelectedIndex==0)//ilk deper en iyi seçimine karşılık geliyor
            //    elitizmeniyi = true;
            //else
            //    elitizmeniyi = false;

            string caprazlasecim = comboBox3.Text;
                ga = new GenetikAlgoritma(ia, kromozomsayisi, caprazlamaorani, caprazlasecim, 5, iterasyonsayisi, elitizmeniyi);

            ga.Calistir(listBox1);

            zaman = DateTime.Now;

            if (Islemler.ilkatamabasarili == false)
            {
                string mesaj = "İstenen Kromozom sayısı:" + ga.populasyon.populasyonBuyukluk.ToString();
                mesaj+="\n" + "İlk atamada oluşturulan Kromozom sayısı:" + Islemler.ilkatamaKromozomSayisi.ToString();
                mesaj += "\nDaha iyi bir rota için farklı bir atama yöntemi kullanın\nSorun devam ederse;\nEkip sayısını artırabilirsiniz\nHasta gereksinimlerini kontrol edin\nHasta zaman periyodlarını kontrol edin";
                MessageBox.Show(mesaj);
                Islemler.ilkatamabasarili = true;

                //atamalar yapılamadı atanamayan hastaları yazdır
               
             
            }
            else
                MessageBox.Show("Atamalar tamamlandı");
                
           

            if (ga.populasyon.kromozomListesi.Count == 0) return;

            label6.Text = listBox1.Items[listBox1.Items.Count - 1].ToString();
            label7.Text = listBox1.Items[0].ToString();
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
          
            label8.Text = zaman.ToString();
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {
          //  Islemler.CezaPuanlariniBelirle();
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

        private void RouteCiz(Rota myrota, Color renk, float kalemkalinlik)
        {
        
            GMapProviders.GoogleMap.ApiKey = Islemler.myKey;
            //var route=GMapProviders.GoogleMap.  .GetRouteBetweenPoints(start, end, false, false, 1);
            map.Overlays.Clear();
            MarkerEkleHepsi(Islemler.gunID);

            if (myrota.ziyaretSirasi.Count <= 2)
                return;
            MapRoute route=null;
            GMapOverlay routes=null;
            // map.Overlays.Clear();
          //  string hastalar = "";
            GMapRoute r=null;
            for (int i = 0; i < myrota.ziyaretSirasi.Count - 1; i++)
            {
              
                PointLatLng start = new PointLatLng(myrota.ziyaretSirasi[i].hasta.konum.lat, myrota.ziyaretSirasi[i].hasta.konum.lon);
                PointLatLng end = new PointLatLng(myrota.ziyaretSirasi[i + 1].hasta.konum.lat, myrota.ziyaretSirasi[i + 1].hasta.konum.lon);
                try
                {
                    route = GoogleMapProvider.Instance.GetRoute(start, end, false, true, 15);
                    r = new GMapRoute(route.Points, myrota.ekip.ekipID.ToString());
                }
                catch
                {
                    MessageBox.Show("İnternet bağlantıı yok\nRota ön bellekten çizdirildi\nRotada eksik noktalar olabilir");
                    break;
                }
               
                r.Stroke.Width = kalemkalinlik;
                r.Stroke.Color = renk;
                routes = new GMapOverlay("routes");
                routes.Routes.Add(r);
                //  start = end;
                // end= new PointLatLng(myrota.ziyaretSirasi[i].hasta.konum.lat, myrota.ziyaretSirasi[i].hasta.konum.lon);
                map.Overlays.Add(routes);
                //map.Refresh();
                //hastalar += myrota.ziyaretSirasi[i].hasta.hastaID.ToString() + "-";
            }
            //   map.Refresh();
            map.Zoom = map.Zoom - 0.5;
            map.Zoom = map.Zoom + 0.5;
            // MessageBox.Show(hastalar);

            
           
          
        }

        private void RouteCiz2(Rota myrota, Color renk)
        {
            //kullanılmıyor
           
            GMapProviders.GoogleMap.ApiKey = Islemler.myKey;
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
            listBox2_SelectedIndexChanged(listBox2, e);
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
          //  if (mycombo.SelectedText.ToString() == "") return;
            try
            {
                Islemler.gunID = Int32.Parse(mycombo.SelectedValue.ToString());
            }
            catch
            {
                return;
            }
                        
            var distancematr = Islemler.distanceMatrix;
            List<Ekip> ekiplist = Islemler.ekipListGun;
            List<Hasta> hastalist = Islemler.hastaListGun;
            MarkerEkleHepsi(Islemler.gunID);
            HaritaKonumlan();
            button7.Enabled = true;
            button3.Enabled = true;
            button9.Enabled = true;


            
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void iterasyonsayisiTxt_TextChanged(object sender, EventArgs e)
        {

        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
         
            if (!Islemler.InternetVarmi())
            {
                MessageBox.Show("Internet bağlantısında sorun var\nRotalar oluşturulur ama düzgün çizilemez");
            }
            ListBox lb = (ListBox)sender;
            if (!(lb.SelectedItems.Count > 0)) return; //seçim yoksa işlem yapma
            if (secim_ga_gredy == 1) //ga ile route lar oluştu ise
            {
                int rota = lb.SelectedIndex;
                RouteCiz(ga.IyiList[nesil].kromozom.rotaListesi[rota], renkG, penwidth);

            }
            else if (secim_ga_gredy == 2)//gredy ile route lar oluştu ise
            {
                int rota = lb.SelectedIndex;
                RouteCiz(nga.rotaList[rota], renkG, penwidth);
            }

            else if (secim_ga_gredy == 3)//Yüklenen Rotadan çiziliyordur
            {
                int rota = lb.SelectedIndex;
                RouteCiz(Islemler.rotaLoadList[rota], renkG, penwidth);
            }
            else
                return;//yukarıdaki 3 değerde seçilmemiş ise işlem yapma


        }

        private void listBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
           // return;
            secim_ga_gredy = 1;
            ListBox lb = (ListBox)sender;
            if (!(lb.SelectedItems.Count > 0)) return; //seçim yoksa işlem yapma

            string str = lb.Text;
            int index1 = str.IndexOf("I:") + 2;
            int index2 = str.IndexOf("F:");


            listBox2.Items.Clear();
            listBox3.Items.Clear();
          
            nesil = Int32.Parse(str.Substring(index1, index2 - index1));

            //kaydetme textine değerleri yazdır:

            string ifade = "KN:" + kromozomsayisiTxt.Text.ToString() + "; CR:" + caprazlamasayisiTxt.Text.ToString() + "; CY:" + comboBox3.Text.ToString() + "; IN:" + iterasyonsayisiTxt.Text.ToString() + "; IAY:" + comboBox1.Text.ToString() + "; Nes:" + nesil.ToString();
            ifade += "; Cezalar--> M:" + Islemler.CezaPuanlari[Cezalar.metreToplaminiCezala];
            ifade += "; P:" + Islemler.CezaPuanlari[Cezalar.dakikaToplaminiCezala];
            ifade += "; HTW:" + Islemler.CezaPuanlari[Cezalar.hastaIstenmeyenPeriyod];
            ifade += "; ETW:" + Islemler.CezaPuanlari[Cezalar.ekipIstenmeyenPeriyod];
            ifade += "; EFM:" + Islemler.CezaPuanlari[Cezalar.ekipFazlaMesaiPeriyod];
            ifade += "; EOA:" + Islemler.CezaPuanlari[Cezalar.oglearasiihlali];
            ifade += "; SsM:" + Islemler.CezaPuanlari[Cezalar.sSapmaMetre];
            ifade += "; SsP:" + Islemler.CezaPuanlari[Cezalar.sSapmaDakika];
            textBox1.Text = ifade;

            NesilDegistir(nesil, listBox2, listBox3);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DateTime zaman = DateTime.Now;
            label9.Text = zaman.ToString();

            secim_ga_gredy = 2;
            nga = new NewGredyAlgorithm();
            nga.AtamalariYap();
            listBox4.Items.Clear();
            foreach (Hasta hasta in nga._hastaList)
                if(hasta.hastaID!=0)
                     listBox4.Items.Add(hasta.gosterID.ToString());
            GredyRoralar(nga,listBox2,listBox3);

            zaman= DateTime.Now;
            label8.Text = zaman.ToString();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            FormCezalar frm = new FormCezalar();
            frm.ShowDialog();
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
             HaritaKonumlan();
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            if (textBox1.Text == "Route description")
                textBox1.Clear();
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            if (listBox2.Items.Count==0)
            {
                MessageBox.Show("Kaydedilebilecek Herhangib Bir Rota Yok");
                return;
            }
            List<Rota> myRotaList = new List<Rota>();
            string tur = "";
            int fitnes = 0;
            int periyod=0;
            int metre = 0;
            int sonucID = 0;
            int kromozomID = 0;
            if (secim_ga_gredy == 1) //ga ile route lar oluştu ise
            {
                tur = "GA";
                myRotaList = ga.IyiList[nesil].kromozom.rotaListesi; //rota listesi alındı
                fitnes = ga.IyiList[nesil].kromozom.fitness;//kromozoma ait fitness değeri
                periyod = ga.IyiList[nesil].kromozom.toplamUzaklik.dakika;
                metre = (int)ga.IyiList[nesil].kromozom.toplamUzaklik.metre;
                kromozomID = ga.IyiList[nesil].kromozom.kromozomId;
            }
            if (secim_ga_gredy == 2)//gredy ile route lar oluştu ise
            {
                tur = "Greedy";
                myRotaList = nga.rotaList;  
                foreach(Rota rota in myRotaList)
                {
                    periyod += rota.toplamUzaklik.dakika;
                    metre += (int)rota.toplamUzaklik.metre;
                }
            }
            SqlCommand komut = new SqlCommand();
            komut.Connection = Islemler.conn;
            string sqlcumle = "Insert SONUCLAR(gunID,aciklama, tur, fitnes, dakika, metre)";
            sqlcumle += " values(";
            sqlcumle += Islemler.gunID;
            sqlcumle += ","+"'" + textBox1.Text + "'";
            sqlcumle += "," + "'" + tur + "'";
            sqlcumle += "," + fitnes;
            sqlcumle += "," + periyod;
            sqlcumle += "," + metre;
            sqlcumle += ")";
            komut.CommandText = sqlcumle;

            Islemler.conn.Open();
            komut.ExecuteNonQuery();//sonuc genel bilgiler kaydedildi

            sqlcumle = "select max(ID) as maxID from SONUCLAR";
            komut.CommandText = sqlcumle;
            SqlDataReader dr = komut.ExecuteReader();
            if (dr.Read()) sonucID = (int)dr["maxID"]; //Sonuc kaydedilen ID alındı en son ID 
            dr.Close();

            //rotalar kaydedilecek
            int rotasira = 0;
            foreach (Rota rota in myRotaList) //bütün rotalar kaydediliyor
            {
                int ekipID = rota.ekip.ekipID;
                int ceza = rota.rotaceza;
                periyod = rota.toplamUzaklik.dakika;
                metre = (int)rota.toplamUzaklik.metre;

                sqlcumle = "Insert ROTALAR(sonucID,kromozomID,sira, ekipID, ceza, dakika, metre)";
                sqlcumle += " values(";
                sqlcumle += sonucID;
                sqlcumle += "," + kromozomID;
                sqlcumle += "," + rotasira;
                sqlcumle += "," + ekipID;
                sqlcumle += "," + ceza;
                sqlcumle += "," + periyod;
                sqlcumle += "," + metre;
                sqlcumle += ")";
                komut.CommandText = sqlcumle;
                komut.ExecuteNonQuery();

                int rotaID = 0;
                sqlcumle = "select max(ID) as maxID from ROTALAR";
                komut.CommandText = sqlcumle;
                SqlDataReader dr2 = komut.ExecuteReader();
                if (dr2.Read()) rotaID = (int)dr2["maxID"]; //rotaID alındı ziyaret sırası buna göre kaydedilecek
                dr2.Close();

                //rota kaydı tamamlandı başka bir döngü ile o rotaya ait ziyaret listesi kaydedilecek
                int gensira = 0;
                foreach (Gen gen in rota.ziyaretSirasi)
                {
                    int hastaID = gen.hasta.hastaID;
                    int gosterID = gen.hasta.gosterID;
                    int t1 = gen.atandigiTimeWindow.t1;
                    int t2 = gen.atandigiTimeWindow.t2;
                    sqlcumle = "exec ZiyaretSiraKaydet ";
                    sqlcumle += rotaID;
                    sqlcumle += "," + gensira;
                    sqlcumle += "," + hastaID;
                    sqlcumle += "," + gosterID;
                    sqlcumle += "," + t1;
                    sqlcumle += "," + t2;
                 
                    komut.CommandText = sqlcumle;
                    komut.ExecuteNonQuery();


                    gensira++;
                }

                rotasira++;
            }

            Islemler.conn.Close();
            MessageBox.Show("Rotalar Veritabanına Kaydedildi");
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            Islemler.rotaLoadList = null;
            FormRouteLoad frm = new FormRouteLoad();
            frm.ShowDialog();
          
            if (Islemler.rotaLoadList!=null)
            {
                secim_ga_gredy = 3;
                RotalarıDoldurLoad(Islemler.rotaLoadList, listBox2, listBox3);

                //var distancematr = Islemler.distanceMatrix;
                //List<Ekip> ekiplist = Islemler.ekipListGun;
                //List<Hasta> hastalist = Islemler.hastaListGun;
                MarkerEkleHepsi(Islemler.gunID);
                HaritaKonumlan();

            }
           
        }

        private void map_MouseClick(object sender, MouseEventArgs e)
        {
         
        }

       

        private void map_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button==MouseButtons.Right)
            {
                var lanlon = map.FromLocalToLatLng(e.X, e.Y);
                Islemler.cliklat = lanlon.Lat;
                Islemler.cliklng = lanlon.Lng;
            }
            
        }

       

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {
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

            bool elitizmeniyi = true;
            //if (comboBox3.SelectedIndex==0)//ilk deper en iyi seçimine karşılık geliyor
            //    elitizmeniyi = true;
            //else
            //    elitizmeniyi = false;

            string caprazlasecim = comboBox3.Text;

            ga.kromozomSayisi = kromozomsayisi;
            ga.caprazlamaOrani = caprazlamaorani;
            ga.nesilSayisi = iterasyonsayisi;
            ga.caprazlasecim = caprazlasecim;
            ga.Calistir(listBox1);
            MessageBox.Show("İlave Çalışma Tamamlandı");

        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            penwidth = (float)Convert.ToDouble(comboBox4.Text);
            listBox2_SelectedIndexChanged(listBox2, e);
        }
    }
}
