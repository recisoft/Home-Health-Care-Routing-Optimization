using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.ComponentModel;
using System.Net;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    static class Islemler
    {
        
        static public string myKey = " ";//your google distance matrix api key 
        static public int mesaiBaslangic = 0;
        static public int mesaiBitis = 540;
        static public int ilkAtamaMaxDenemeSayisi = 100000; //atama için yapılacak deneme sayısı
        static public int M = 100000; //büyük bir sayıdır
        static public double M2 = 1000000000000000000;//18 adet 0 var
       // static public double M2 = 1;

        static public string atamamesaj = "Atamalar yapılamadı \n  -Ekip sayısını, \n  -Ekip yeteneklerini\n  -Ekip periyodlarını\nKontrol edin."; //atama yapılamaz ise kullanıcı mesajı
        static public bool ilkatamabasarili = true;
        static string constr = "Data Source=.;Initial Catalog = EVDEBAKIM; Integrated Security = True"; //database yolu
        static public SqlConnection conn = new SqlConnection(constr);
        static int _gunID;//işlem tarihidir, hastalistesi, ekiplistesi,uzaklıkmatrisini belirler
        static List<Hasta> _hastaListGun;
        static List<Ekip> _ekipListGun;
        static Dictionary<Tuple<int, int>, Uzaklik2> _distanceMatrix; //iki hasta noktası arasındaki uzaklik matrisi
        static public Dictionary<Cezalar,int> CezaPuanlari= new Dictionary<Cezalar, int>();

        static public List<string> atanamayanHastalarListesi = new List<string>();
        static public int ilkatamaKromozomSayisi = -1;
       
        //özellikler
        static public int gunID //değer değiştiğinde o gne ait verileri yükler
        { 
            get { return _gunID; }
            set
            {
                HastaEkipYule(value);//o gün kü hasta ve ekip listesi
                DistanceMatrixYukle(value);//o gün kü ziyaret ekibi arasındaki uzaklık matrisi
                _gunID = value;
            }
        }
        static public List<Hasta> hastaListGun
        {
            get { return _hastaListGun; }
        }
        static public List<Ekip> ekipListGun
        {
            get { return _ekipListGun; }
        }
        static public Dictionary<Tuple<int, int>, Uzaklik2> distanceMatrix
        {
            get { return _distanceMatrix; }
        }

        static public  List<Rota> rotaLoadList=null;

        static public double cliklat = 0;
        static public double cliklng = 0;

        //olaylar

        static public bool InternetVarmi()
        {
            try
            {
                System.Net.Sockets.TcpClient kontrol_client = new System.Net.Sockets.TcpClient("www.google.com.tr", 80);
                 kontrol_client.Close();
                return true;
            }
            catch 
            {
                  return false;
            }
        }
        static Uzaklik2 GetDistanceDurationFromGoogle(Nokta n1, Nokta n2)
        {
            //Uzaklik değerini km ve dakika cinsinden döner
            //n1 ile n2 arasındaki değerlere bakar
            //Uzaklik, Nokta struct yapısındandır
            // aranan noktalarla ilgili bir yanlışlık olursa geriye -1 döner
            Uzaklik2 donen = new Uzaklik2();
            donen.metre = -1;
            donen.dakika = -1;
            string url = "https://maps.googleapis.com/maps/api/distancematrix/xml?";
            url += "origins=" + n1.lat.ToString().Replace(",", ".") + "," + n1.lon.ToString().Replace(",", ".");
            url += "&destinations=" + n2.lat.ToString().Replace(",", ".") + "," + n2.lon.ToString().Replace(",", ".");
            url += "&mode=driving&sensor=false";
            url += "&key="+myKey;//keyin sorgu süresi dolarsa para alıyor günlük 10.000 sorgu
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader sreader = new StreamReader(dataStream);
            string responsereader = sreader.ReadToEnd();
            response.Close();
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(responsereader);
            if (xmldoc.GetElementsByTagName("status")[0].ChildNodes[0].InnerText == "OK") //eleman varsa
            {
                XmlNodeList dst = xmldoc.GetElementsByTagName("distance");
                XmlNodeList dur = xmldoc.GetElementsByTagName("duration");
                donen.metre = Convert.ToDouble(dst[0].ChildNodes[0].ChildNodes[0].Value);
                donen.dakika = (int)Math.Round(Convert.ToDouble(dur[0].ChildNodes[0].ChildNodes[0].Value) / 60, 0);
            }
            return donen;
        }
        static Uzaklik2 GetDistanceDuration(Nokta n1, Nokta n2)
        {
            //kod tekrar elden geçmeli 31.10.2019
            //2 nokta arasındaki uzaklığı önce veritabanında arar,
            //veritabanında bulamaz ise googledistance matrix apisinden (GetDistanceDurationFromGoogle) çağırır sonra ki çağrılar için veritabanına yazar           
            Uzaklik2 uzaklik = new Uzaklik2();//geriye dönecek olan değer için tanımlandı
            string sql = "Select distance, duration from DISTANCEMATRIX Where ";
            sql += "n1.Lat=" + n1.lat.ToString().Replace(",", ".");
            sql += " AND n1.Lon=" + n1.lon.ToString().Replace(",", ".");
            sql += " AND n2.Lat=" + n2.lat.ToString().Replace(",", ".");
            sql += " AND n2.Lon=" + n2.lon.ToString().Replace(",", ".");
            SqlCommand sqlkomut = new SqlCommand(sql, Islemler.conn);
            Islemler.conn.Open();
            SqlDataReader dr = sqlkomut.ExecuteReader();          
            if (dr.Read()) //veritabanında kayıt var ise veritabanından getir
            {
                uzaklik.metre = (double)dr["distance"];
                uzaklik.dakika = (int)dr["duration"];
            }
            else //eğer database de kayıtlı değilse google dan ister
            {
                uzaklik = GetDistanceDurationFromGoogle(n1, n2);//veritabanında bulamazsa googledan alıyor
                                                                // uzaklik değerleri database yazılıyor
                sql = "exec InsertDistanceDuration ";
                sql += n1.lat;
                sql += "," + n1.lon;
                sql += "," + n2.lat;
                sql += "," + n2.lon;
                sql += "," + uzaklik.metre;
                sql += "," + uzaklik.dakika;
                sqlkomut.CommandText = sql;
                try
                {
                    Islemler.conn.Open();
                    sqlkomut.ExecuteNonQuery();
                    Islemler.conn.Close();
                }
                catch
                {
                    MessageBox.Show("Veritabanı hatası oldu");
                }
            }
            dr.Close();
            Islemler.conn.Close();
            return uzaklik;
        }
        static List<Hasta1Hasta2> EksikDistancelar()
        {
            //günlük hasta ziyaret listesinde olupda uzaklık matrixi olmayan nokların listesini bulur
            List<Hasta1Hasta2> olmayanList = new List<Hasta1Hasta2>();
            Hasta1Hasta2 olmayan;
            Uzaklik2 gecici;
            for (int i = 0; i < _hastaListGun.Count; i++)
                for (int j = 0; j < _hastaListGun.Count; j++)
                {
                    try
                    {
                        gecici = _distanceMatrix[new Tuple<int, int>(_hastaListGun[i].hastaID, _hastaListGun[j].hastaID)];
                    }
                    catch
                    {
                        olmayan.hasta1ID = i;
                        olmayan.hasta2ID = j;
                        olmayanList.Add(olmayan);
                    }
                }
            return olmayanList;
        }
        static void GunHastaYukle(int gunID)
        {
            //belirli bir tarihteki hasta listesini yukler
            if (_hastaListGun == null)
                _hastaListGun = new List<Hasta>();
            _hastaListGun.Clear(); //önceki yüklemeleri siler
          
            //Hasta listesi
            string sql = "exec GunHastaBilgisiGetir ";
            sql += gunID;

            SqlCommand sqlkomut = new SqlCommand(sql, Islemler.conn);
            Islemler.conn.Open();
            { 
                SqlDataReader dr = sqlkomut.ExecuteReader();
               
                int hastaID;
                int gosterID;
                Nokta mynokta;
                TimeWindow tw;
                double oncelik;
                int bakimsuresi;
                int skill;
                while (dr.Read())
                {
                    hastaID = (int)dr["hastaID"];
                    gosterID = (int)dr["gosterID"];
                    mynokta.lat = (double)dr["lat"];
                    mynokta.lon = (double)dr["lon"];
                    oncelik = (double)dr["oncelik"];
                    bakimsuresi = (int)dr["bakimsure"];
                    tw.t1 = (int)dr["zc1"];
                    tw.t2 = (int)dr["zc2"];
                    skill = (int)dr["skill"];
                    hastaListGun.Add(new Hasta(hastaID, gosterID, mynokta, oncelik, bakimsuresi, tw, skill));
                }
                dr.Close();
            }
            Islemler.conn.Close();
            //uzaklil matrisi için dizi tanımlandı
          
        }
        static void GunEkipYukle(int gunID)
        {
            //belirli bir tarihteki ekip listesini yukler
            if (_ekipListGun == null)
                _ekipListGun = new List<Ekip>();
            _ekipListGun.Clear(); //önceki yüklemeleri siler

            //Ekip listesi
            string sql = "exec GunEkipBilgisiGetir ";
            sql += gunID;
            SqlCommand sqlkomut = new SqlCommand(sql, Islemler.conn);
            Islemler.conn.Open();
            {
                SqlDataReader dr = sqlkomut.ExecuteReader();
                int ekipID;             
                TimeWindow sabahMesai;
                TimeWindow ogleMesai;        
                int skill;
                while (dr.Read())
                {
                    ekipID = (int)dr["EkipID"];
                    sabahMesai.t1 = (int)dr["sabahBaslama"];
                    sabahMesai.t2 = (int)dr["sabahBitis"];
                    ogleMesai.t1 = (int)dr["ogleBaslama"];
                    ogleMesai.t2 = (int)dr["ogleBitis"];
                    skill = (int)dr["skill"];
                    _ekipListGun.Add(new Ekip(ekipID, sabahMesai, ogleMesai,skill));
                }
                dr.Close();
            }
            Islemler.conn.Close();
        }
        static public void DistanceMatrixOlustur()
        {
            //sürekli kullanılmayacak, sadece ilk başta kullanıldı
            List<Hasta> hastalar = new List<Hasta>();
            SqlCommand sqlkomut = new SqlCommand("select hastaID,lat,lon from HASTALAR", Islemler.conn);
            Islemler.conn.Open();
            SqlDataReader dr = sqlkomut.ExecuteReader();
            while (dr.Read())
            {
                Hasta hasta = new Hasta();
                hasta.hastaID = (int)dr["hastaID"];
                hasta.konum.lat = (double)dr["lat"];
                hasta.konum.lon = (double)dr["lon"];
                hastalar.Add(hasta);
            }
            dr.Close();
            Islemler.conn.Close();
            Uzaklik2 uzaklik = new Uzaklik2();
            foreach (Hasta h1 in hastalar)
                foreach (Hasta h2 in hastalar)
                {
                    if (h1.Equals(h2)) continue;
                    uzaklik = GetDistanceDurationFromGoogle(h1.konum, h2.konum);
                    string sql = "exec InsertDistanceDuration ";
                    sql += h1.konum.lat;
                    sql += "," + h1.konum.lon;
                    sql += "," + h2.konum.lat;
                    sql += "," + h2.konum.lon;
                    sql += "," + uzaklik.metre;
                    sql += "," + uzaklik.dakika;
                    sqlkomut.CommandText = sql;
                    try
                    {
                        Islemler.conn.Open();
                        sqlkomut.ExecuteNonQuery();
                        Islemler.conn.Close();
                    }
                    catch
                    {
                        MessageBox.Show("Distance oluşumunda hata var");
                    }
                }

        }
        static public void HastaEkipYule(int gunID)
        {
            GunHastaYukle(gunID);
            GunEkipYukle(gunID);
        }
        static public void DistanceMatrixYukle(int gunID)
        {
            if (_distanceMatrix == null)
                _distanceMatrix = new Dictionary<Tuple<int, int>, Uzaklik2>();
            _distanceMatrix.Clear();

            string sql = "exec GunHastaDistanceMatrixGetir ";
            sql += gunID;
            SqlCommand sqlkomut = new SqlCommand(sql, Islemler.conn);
            Islemler.conn.Open();
            {
                SqlDataReader dr = sqlkomut.ExecuteReader();
                int h1, h2;
                Uzaklik2 uzaklik;
                while (dr.Read())
                {
                    h1 = (int)dr["hasta1ID"];
                    h2 = (int)dr["hasta2ID"];
                    uzaklik.metre = (double)dr["distance"];
                    uzaklik.dakika = Convert.ToInt32((double)dr["duration"]);
                    distanceMatrix.Add(new Tuple<int, int>(h1,h2), uzaklik);
                }
                dr.Close();
            }
            Islemler.conn.Close();

        }
        static public void CezaPuanlariniBelirle()
        {
            CezaPuanlari.Clear();
            CezaPuanlari.Add(Cezalar.dakikaToplaminiCezala, 0);//her dakika ile çarpılacak
            CezaPuanlari.Add(Cezalar.metreToplaminiCezala, 1);
            CezaPuanlari.Add(Cezalar.hastaIstenmeyenPeriyod, 2);//her dakikası için
            CezaPuanlari.Add(Cezalar.ekipIstenmeyenPeriyod, 0);//her dakikası için
            CezaPuanlari.Add(Cezalar.skillHatali, Islemler.M);
            CezaPuanlari.Add(Cezalar.ekipFazlaMesaiPeriyod, 0);//her dakikakasi ile çarpılacak
            CezaPuanlari.Add(Cezalar.oglearasiihlali, 10);//her hastanın her dakikası için
            CezaPuanlari.Add(Cezalar.sSapmaDakika, 50);//standart sapmanın her bir değeri için
            CezaPuanlari.Add(Cezalar.sSapmaMetre, 5);
        }

        static public void CezaPuanlariniBelirle(
            int dakika,
            int metre,
            int hastaPeriyod,
            int ekipPeriyod,
            int skill,
            int ekipMesai,
            int ekipOgleArasi,
            int sapmaMetre,
            int samaDakika      
        )
        {
            CezaPuanlari.Clear();
            CezaPuanlari.Add(Cezalar.dakikaToplaminiCezala, dakika);//her dakika ile çarpılacak
            CezaPuanlari.Add(Cezalar.metreToplaminiCezala, metre);
            CezaPuanlari.Add(Cezalar.hastaIstenmeyenPeriyod, hastaPeriyod);//her dakikası için
            CezaPuanlari.Add(Cezalar.ekipIstenmeyenPeriyod, ekipPeriyod);//her dakikası için
            CezaPuanlari.Add(Cezalar.skillHatali, skill);
            CezaPuanlari.Add(Cezalar.ekipFazlaMesaiPeriyod, ekipMesai);//her dakikakasi ile çarpılacak
            CezaPuanlari.Add(Cezalar.oglearasiihlali, ekipOgleArasi);//her hastanın her dakikası için
            CezaPuanlari.Add(Cezalar.sSapmaDakika, samaDakika);//standart sapmanın her bir değeri için
            CezaPuanlari.Add(Cezalar.sSapmaMetre, sapmaMetre);
        }
        static public Uzaklik2 UzaklikGetir(int h1ID, int h2ID)
        {
            //iki nokta rasındaki uzaklık değerini getirir
            //eğer değer veritabanında varsa veritabanından getirir
            //eğer degeri uzaklık matriksinde bulamaz ise google dan sorgular ve veritabanına yeni değeri yazar
            //bütün uzaklıklar her zaman sorgulanmayabilir.

            Uzaklik2 ayninokta = new Uzaklik2();
            ayninokta.dakika = 0;ayninokta.metre = 0;
            if (h1ID == h2ID) return ayninokta;
            Uzaklik2 yeniuzaklik = new Uzaklik2();
            try
            {
                yeniuzaklik = Islemler.distanceMatrix[new Tuple<int, int>(h1ID, h2ID)];
              
            }
            catch
            {
                //distance matrixte olmayan bir değer sorgulanıyor
                Nokta n1=new Nokta();
                Nokta n2 = new Nokta();

                //burası hata verme ihtimali olan bir kısım
                //eğer burada hata olursa değerleri database den sorgulama yap
                //hata vermez ise bu şekilde kalmalı bu daha hızlı
                foreach (Hasta hasta in _hastaListGun)
                {
                    if (h1ID == hasta.hastaID) n1 = hasta.konum;
                    if (h2ID == hasta.hastaID) n2 = hasta.konum;
                }

                yeniuzaklik = GetDistanceDurationFromGoogle(n1, n2);
               
                //yeni uzakliği veritabanina ekle
                SqlCommand sqlkomut = new SqlCommand();
                string sql = "exec InsertDistanceDuration ";
                sql += n1.lat;
                sql += "," + n1.lon;
                sql += "," + n2.lat;
                sql += "," + n2.lon;
                sql += "," + yeniuzaklik.metre;
                sql += "," + yeniuzaklik.dakika;
                sqlkomut.CommandText = sql;
                sqlkomut.Connection = Islemler.conn;
                try
                {
                    Islemler.conn.Open();
                    sqlkomut.ExecuteNonQuery(); //yeni değer tabloya yazıldı
                    Islemler.conn.Close();
                }
                catch
                {
                    MessageBox.Show("Veritabanı hatası oldu");

                }
                distanceMatrix.Add(new Tuple<int, int>(h1ID, h2ID), yeniuzaklik);//yeni değer distance matrixe eklendi
            }
            
            return Islemler.distanceMatrix[new Tuple<int, int>(h1ID, h2ID)];
        }
        static public Uzaklik2 UzaklikGetir(Hasta hasta1, Hasta hasta2)
        {
            return Islemler.distanceMatrix[new Tuple<int, int>(hasta1.hastaID,hasta2.hastaID)];
        }
        static public Uzaklik2 UzaklikGetir(Gen g1, Gen g2)
        {
            Uzaklik2 ayninokta = new Uzaklik2();
            ayninokta.dakika = 0; ayninokta.metre = 0;
            if (g1.hasta.hastaID == g2.hasta.hastaID) return ayninokta;

            //  return Islemler.distanceMatrix[new Tuple<int, int>(g1.hasta.hastaID, g2.hasta.hastaID)];
            return UzaklikGetir(g1.hasta.hastaID, g2.hasta.hastaID);
        }

        static public int HastaKayabilecegiPeriyod(double oncelik)
        {
            //Atama öncecliğine göre zaman penceresi dışına kaymasına izin verilen max periyod
            //değeler kolay olsun diye fonksiyondan alında yazılım yapıması istenirse veritabaınında çekilebilir
            int deger = 0;
            if (oncelik >= 1) deger = 0;
            else if (oncelik >= 0.9) deger = 10;
            else if (oncelik >= 0.8) deger = 15;
            else if (oncelik >= 0.7) deger = 20;
            else if (oncelik >= 0.6) deger = 25;
            else if (oncelik >= 0.5) deger = 30;
            else if (oncelik >= 0.4) deger = 40;
            else if (oncelik >= 0.3) deger = 50;
            else if (oncelik >= 0.2) deger = 60;
            else if (oncelik >= 0.1) deger = 75;
            else if (oncelik >= 0.0) deger = 90;

            return deger;
        }

        static public int HastaKayabilecegiPeriyod(Hasta hasta)
        {
            //Atama öncecliğine göre zaman penceresi dışına kaymasına izin verilen max periyod
            //değeler kolay olsun diye fonksiyondan alında yazılım yapıması istenirse veritabaınında çekilebilir
            int deger = 0;
            double oncelik = hasta.oncelik;
            if (oncelik >= 1) deger = 0;
            else if (oncelik >= 0.9) deger = 10;
            else if (oncelik >= 0.8) deger = 15;
            else if (oncelik >= 0.7) deger = 20;
            else if (oncelik >= 0.6) deger = 25;
            else if (oncelik >= 0.5) deger = 30;
            else if (oncelik >= 0.4) deger = 40;
            else if (oncelik >= 0.3) deger = 50;
            else if (oncelik >= 0.2) deger = 60;
            else if (oncelik >= 0.1) deger = 75;
            else if (oncelik >= 0.0) deger = 90;

            return deger;
        }

        
    }
}
