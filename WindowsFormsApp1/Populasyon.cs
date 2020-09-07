using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    class Populasyon
    {
        private int _buyukluk;
        private IlkAtamaYontem _iay;
        private  List<Kromozom> _kromozomlist;
        public int populasyonBuyukluk
        {
            get { return _buyukluk; }
        }
        public IlkAtamaYontem ilkAtamaYontem
        {
            get { return _iay; }
        }
        public List<Kromozom> kromozomListesi
        {
            get { return _kromozomlist; }
        }
        public Populasyon(int buyukluk, IlkAtamaYontem ilkatamayontem)
        {
            this._buyukluk = buyukluk;
            this._iay = ilkatamayontem;
   //         Islemler.CezaPuanlariniBelirle();

            _kromozomlist = new List<Kromozom>();
            int sayac = 0;
            int sayac2 = 0;
            for (int i = 1; _kromozomlist.Count < this._buyukluk; i++)
            {
                Kromozom k = new Kromozom(this._iay);
                if (k.atanamayanhastalar.Count == 0)
                {
                    sayac++;
                    _kromozomlist.Add(k);
                }
                sayac2++;
                Islemler.ilkatamaKromozomSayisi = _kromozomlist.Count();
                if (sayac2 >= Islemler.ilkAtamaMaxDenemeSayisi)
                {
                    // MessageBox.Show(Islemler.atamamesaj);
                    // _kromozomlist = null;
                    Islemler.ilkatamabasarili = false;                    
                    return;
                }
                
            }
            //Thread atama = new Thread(new ThreadStart(IlkAtamaThread));
            //atama.Start();
        }
         
      
        public void FitnessHesapla()
        {
    
            foreach (Kromozom myKromozom in kromozomListesi)
                myKromozom.FitnessHesapla();
        }

        //public void KromozomSec(ref Kromozom kromozom1, ref Kromozom kromozom2)
        //{
        //    /*
        //     * Kullanılmadı
        //     * Basitçe birbinin aynı olmayan 2 adet kromozom seçer
        //     */
        //    CryptoRandom rnd = new CryptoRandom();
        //    int indis1 = rnd.Next(0, _buyukluk); //önceki nesilden seçmek içini, kromozomlist.count olursa yeni eklenen çocukları da dikkate alır
        //    int indis2 = -1;
        //    do
        //    {
        //        indis2 = rnd.Next(0, _buyukluk);
        //    } while (indis1 == indis2);//farklı 2 değer üretinceye kadar devam eder
        //    kromozom1 = _kromozomlist[indis1];
        //    kromozom2 = _kromozomlist[indis2];
        //}
        //public void CaprazlaRandom(int caprazlamaorani)
        //{
        //    //yazıldı ama bu kod kullanılmadı
        //    //caprazla ve ekle ile seçilen 2 kromozomu çaprazlayarak ekliyor.
        //    //100 den büyük bir değer gelirse 100 kabul edilir
        //    if (caprazlamaorani > 100) caprazlamaorani = 100;
            
        //    do
        //    {
        //        Kromozom k1 = null;
        //        Kromozom k2 = null;
        //        KromozomSec(ref k1, ref k2);
        //        CaprazlaveEkle(k1, k2);
        //    } while (kromozomListesi.Count <= (1 + (double)caprazlamaorani / 100) * _buyukluk);
            
        //}
        public void Caprazla2Grup(int caprazlamaorani)
        {
            //caprazla ve ekle ile seçilen 2 kromozomu çaprazlayarak ekliyor.
            //2 ayrı grup yapıyor ve bu gruplardan rastgele seçerek çaprazlıyor
            //çaprazlama oranı 100 den büyük gönderilirse değer 100 kabul edilir
            if (caprazlamaorani > 100) caprazlamaorani = 100;
            List<int> listeGenel = new List<int>(); //genel kromozom listesi
            List<int> liste1 = new List<int>(); //kromozomları 2 listeye bölecek
            List<int> liste2 = new List<int>();

            for (int i = 0; i < kromozomListesi.Count; i++)
                listeGenel.Add(i);

            int dongusayisi = listeGenel.Count*caprazlamaorani / (100*2);//listedeki eleman sayısının yarısı
            CryptoRandom rnd = new CryptoRandom(); 
            int indis;
            for (int i=0;i<dongusayisi;i++) //çaprazlama oranı 50 ise %25 elemanı liste1 atıyor, %25 i liste2 ye
            {                
                indis = rnd.Next(0, listeGenel.Count);
                liste1.Add(listeGenel[indis]);
                listeGenel.Remove(indis);

                indis = rnd.Next(0, listeGenel.Count);
                liste2.Add(listeGenel[indis]);
                listeGenel.Remove(indis);
            }
            for (int i=0;i<dongusayisi;i++)
            {
                Kromozom k1 = kromozomListesi[liste1[i]];
                Kromozom k2 = kromozomListesi[liste2[i]];
                CaprazlaveEkle(k1, k2);
            } 

        }

        public void Caprazla2Grup_Olasilikli(int caprazlamaorani)
        {
            //caprazla ve ekle ile seçilen 2 kromozomu çaprazlayarak ekliyor.
            //2 ayrı grup yapıyor ve bu gruplardan rastgele seçerek çaprazlıyor
            //çaprazlama oranı 100 den büyük gönderilirse değer 100 kabul edilir
            if (caprazlamaorani > 100) caprazlamaorani = 100;
            CryptoRandom rnd = new CryptoRandom();
            List<int> listeGenel = new List<int>(); //genel kromozom listesi
            List<int> liste1 = new List<int>(); //kromozomları 2 listeye bölecek
            List<int> liste2 = new List<int>();
            FitnessHesapla();
            for (int i = 0; i < kromozomListesi.Count; i++)
                listeGenel.Add(i);
            for (int i=0;i<kromozomListesi.Count-1;i++)
                for (int j=i+1;j<kromozomListesi.Count;j++)
                    if (kromozomListesi[i].fitness< kromozomListesi[j].fitness)
                    {
                        Kromozom gecici = kromozomListesi[i];
                        kromozomListesi[i] = kromozomListesi[j];
                        kromozomListesi[j] = gecici;
                    }
            int silmesayisi = (100 - caprazlamaorani)* kromozomListesi.Count()/100;
            for (int i=0;i<silmesayisi;i++)
            {
                int silinecekindis = rnd.Next(0, silmesayisi);
                silinecekindis = rnd.Next(0, silinecekindis+1);
                listeGenel.RemoveAt(silinecekindis);
            }

            //genel listeden 1- çaprazlama oranı kadar eleman sil

            int dongusayisi = listeGenel.Count * caprazlamaorani / (100 * 2);//listedeki eleman sayısının yarısı
           
            int indis;
            for (int i = 0; i < dongusayisi; i++) //çaprazlama oranı 50 ise %25 elemanı liste1 atıyor, %25 i liste2 ye
            {
                indis = rnd.Next(0, listeGenel.Count);
                liste1.Add(listeGenel[indis]);
                listeGenel.Remove(indis);

                indis = rnd.Next(0, listeGenel.Count);
                liste2.Add(listeGenel[indis]);
                listeGenel.Remove(indis);
            }
            for (int i = 0; i < dongusayisi; i++)
            {
                Kromozom k1 = kromozomListesi[liste1[i]];
                Kromozom k2 = kromozomListesi[liste2[i]];
                CaprazlaveEkle(k1, k2);
            }

        }
        public int CaprazlaveEkle(Kromozom birey1, Kromozom birey2)
        {
            /*
             * birey1 ve birey2' nin kopyasını çıkartıyor,
             * daha sonra her iki kromozomda rastgele seçilen karşılıklı rotaları r0-r0, r1-r1 gibi yer değiştiriyor
             * yer değiştirme sonunda eksik ve fazla genleri düzeltmek için tamir operatörü çalışıyor
             * eğer tamir başarılı ekleyebildiği kromozom sayısını geriye dönüyor 0-1-2 değerlerinden birisini dönebilir
             */

            Kromozom kopya1 = new Kromozom(birey1); //birey1 in tam bir kopyası alındı, ama idler farklı
            Kromozom kopya2 = new Kromozom(birey2);
            CryptoRandom rnd = new CryptoRandom();
            int randomindex = rnd.Next(0,kopya1.rotaListesi.Count);//karşılıklı değişecek olan rotaların indisi, karşılıklı rotalar aynı ekib bilgisini içerir

            Rota r1 = kopya1.rotaListesi[randomindex]; //rotakarın referansları saklandı
            Rota r2 = kopya2.rotaListesi[randomindex];
            kopya1.rotaListesi.RemoveAt(randomindex); //kromozomdan ilgili rota önceki kromozomdan silindi
            kopya2.rotaListesi.RemoveAt(randomindex);

            /*
             * önce rotalar karşılıklı olarak yer değiştirecek
             * sonra rotalardaki fazla ve eksik genler bulunarak tamir yapılacak
             * eğer tamir edilemez ise bu kromozom kullanılmayacak
            */
            kopya1.rotaListesi.Insert(randomindex, r2); //rotalar karşılıklı olarak yer değiştirdi
            kopya2.rotaListesi.Insert(randomindex, r1);

            //burada tamir fonksiyonunun çalışması gerekli
            //tekrarlı ve eksik genler bulunarak düzeltme yapılacak
            int eklenenyenikromozomsayisi = 0;
            if (kopya1.Tamir(randomindex))//eğer tamir başarılı oluyorsa populasyona eklenebilir.
            {
                //mutasyon uygulanmayacağından dolayı kapattım, mutasyonun bir etkisi olmadı
                //değişik oranlar için mutasyon denemeleri yapıldı bazen daha kötü sonuçlar verdi 
                //if (rnd.NextDouble() <= 0.03) //eğer mutasyon olayı gerçekleşirse mutayon uyguluyor
                //{
                //    if (kopya1.Mutasyon(3))
                //    {
                //        kromozomListesi.Add(kopya1);
                //        eklenenyenikromozomsayisi++;
                //    }                       
                //}
                //else //mutayon ihtimali gerçekleşmez ise uygulamaz
                {
                    kromozomListesi.Add(kopya1);
                    eklenenyenikromozomsayisi++;
                }                            
            }
               
            if (kopya2.Tamir(randomindex))
            {
                //mutasyon uygulanmayacağından dolayı kapattım, mutasyonun bir etkisi olmadı
                //if (rnd.NextDouble() <= 0.03) //eğer mutasyon olayı gerçekleşirse mutayon uyguluyor
                //{
                //    if (kopya2.Mutasyon(3))
                //    {
                //        kromozomListesi.Add(kopya2);
                //        eklenenyenikromozomsayisi++;
                //    }
                //}
                //else //mutayon ihtimali gerçekleşmez ise uygulamaz
                {
                    kromozomListesi.Add(kopya2);
                    eklenenyenikromozomsayisi++;
                }
            }        
            return eklenenyenikromozomsayisi;
        }

         public bool Tamir(Kromozom myKromozom, int sabitRota)
        {
            //***Kromozom sınıfına yazıldı o çağrılıyor. Bu yüzden burası kullaılmıyor.***
            
            //kromozom tamiri için kullanıacaktır
            //kromozomdaki fazla olan hastaID leri bularak silecek,
            //eksik olanları ise ilk atama yöntemine göre rastgele ekleyecektir
            //sabitrota ile pas edilen rotadan silme işlemi yapılmayacaktır. bu rora yeni eklenen rotadır

            //kromozomdaki fazla hastalar bulunurak siliniyor
            //sabit rotadan silme yapılmayacak
            int[] hastaAtamaSayisi = new int[Islemler.hastaListGun.Count]; //hastaların atanma sayısını bulmak için, 0. hasta harriç, 0. hasta Sağlık merkezi
            foreach(Rota myrota in myKromozom.rotaListesi)         
                foreach (Gen mygen in myrota.ziyaretSirasi)//                 
                    hastaAtamaSayisi[Islemler.hastaListGun.IndexOf(mygen.hasta)]++; //mygen.hasta.hastaID için herbir hastaID sinin ne kadar sayıda kullanıldığını sayar
            List<int> fazlaAtama = new List<int>();//fazla atanan hastaların indisleri
            List<int> eksikAtama = new List<int>();//eksik atanan hastaların indisleri
            for (int i = 1; i < hastaAtamaSayisi.Length; i++) //0. indiste Sağlık merkezi olduğundan 1 den başladı
                if (hastaAtamaSayisi[i] > 1) //1 den büyük değer var ise,  değer=2 ise fazla atanmıştır
                    fazlaAtama.Add(Islemler.hastaListGun[i].hastaID);
                else if (hastaAtamaSayisi[i] < 1)//1 den küçük bbir değer varsa , değer=0 ise çaprazlama ile hasta kaybolmuştur
                    eksikAtama.Add(Islemler.hastaListGun[i].hastaID);

            //fazla olan hastaları bulup o genleri rotalardan silen kodlar
            //döngü yapısı değiştirilebilir
            //fazla atama içinde hastalar arandı istenirse tersi de yapılabilir, rota içinde fazla atamalar aranabilir
            for(int i=0;i<fazlaAtama.Count;i++)
                for(int j=0;j<myKromozom.rotaListesi.Count;j++)
                {
                    if (j == sabitRota) continue;//sabitrota çapralama ile kromozoma dahil edildi bu rota silinmemmeli
                    for (int k = 1; k < myKromozom.rotaListesi[j].ziyaretSirasi.Count - 1; k++)//0. ve sonuncu genler sağlık merkezi
                        if (fazlaAtama[i] == myKromozom.rotaListesi[j].ziyaretSirasi[k].hasta.hastaID)
                            myKromozom.rotaListesi[j].ziyaretSirasi.RemoveAt(k);//fazla olan hastaya ait gen silindi
                }

            //Eksik olan hastaların rotalara atanması işlemi yapılacak
            //atama sonunda atanamayan hasta listesinde hasta varsa bazı hastalar yerleşmemiş demektir
            //yerleşemeyen hastalar olması durumunda kromozomun tamiri mümkün olmamkıştır
            //tamir edilemeyen durumlarda geriye false değer döner
            myKromozom.ListedenAtamaYap(eksikAtama);//eksik olan mhastaların kromozoma atanmasını sağlar
            if (myKromozom.atanamayanhastalar.Count > 0)
                return false;//atanamayan hasta varsa eksik olan hastalardan bazıları yerleşmemiştir
            return true;
        }
        //public void ElitizimUygula(bool elitizimeniyi)
        //{
        //    //iyi sonuçlar vermeyince kullanılmadı testlerde bir başarı görülmedi
        //    if (elitizimeniyi)
        //        ElitizimEniyi();
        //    else
        //        ElitizimCesitlilik();
        //}

        public void ElitizimEniyi()
        {
            //kromozom sayısını başlangıç populasyonuna düşürür,
            //kromozomları sıralar ve en kötü olanları siler

            //kromozomlar sıraya konuyor
            bool degisti = true;
            int sayac = 0;
            while (degisti)//bubble sort sıralaması yapıyor
            {
                degisti = false;
                for (int i = 0; i < _kromozomlist.Count - 1 - sayac; i++)
                    if (_kromozomlist[i].fitness > _kromozomlist[i + 1].fitness)
                    {
                        Kromozom gecici = _kromozomlist[i];
                        _kromozomlist[i] = _kromozomlist[i + 1];
                        _kromozomlist[i + 1] = gecici;
                        degisti = true;
                    }
                sayac++;
            }

            //sıraya konduktan sonra başka daha iyi bireyler olduğundan sondan başlayarak silme yapılıyor
            while (_kromozomlist.Count > _buyukluk)
                _kromozomlist.RemoveAt(_kromozomlist.Count - 1);

        }

        //public void ElitizimCesitlilik()
        //{
        //    //iyi olanların belirli bir yüzdesini bırakarak silme yapar çeşitlilik olsun diye böyle yapıldı
        //    //kromozom sayısını başlangıç populasyonuna düşürür,
        //    //kromozomları sıralar ve en kötü olanları siler

        //    //kromozomlar sıraya konuyor
        //    bool degisti = true;
        //    int sayac = 0;
        //    while (degisti)//bubble sort sıralaması yapıyor
        //    {
        //        degisti = false;
        //        for (int i = 0; i < _kromozomlist.Count - 1 - sayac; i++)
        //            if (_kromozomlist[i].fitness > _kromozomlist[i + 1].fitness)
        //            {
        //                Kromozom gecici = _kromozomlist[i];
        //                _kromozomlist[i] = _kromozomlist[i + 1];
        //                _kromozomlist[i + 1] = gecici;
        //                degisti = true;
        //            }
        //        sayac++;
        //    }

        //    //sıraya konduktan sonra başka daha iyi bireyler olduğundan sondan başlayarak silme yapılıyor
        //    CryptoRandom rnd = new CryptoRandom();
        //    while (_kromozomlist.Count > _buyukluk)
        //    {             
        //        int randomindex = rnd.Next(Convert.ToInt32(_kromozomlist.Count*0.90), _kromozomlist.Count);
        //        _kromozomlist.RemoveAt(randomindex);
        //    }
                

        //}

    }
}

