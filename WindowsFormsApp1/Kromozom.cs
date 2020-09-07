using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    class Kromozom
    {
        static private int _staticId;
        static double[] ekipskillmap = new double[32];//skill int türünden olduğundan 32 bit
        static double[] hastaskillmap = new double[32];
        static double[] skillmaporan = new double[32]; //her skill değerinin kıtlığını bulmak için kullanılacak
        static int hastaskilldongu;
        private IlkAtamaYontem _iay;
        List<Rota> _rotaListesi;
        private int _kromozomId;
        private int _fitness;
        private Uzaklik2 _toplamUzaklik;
        public List<Hasta> atanamayanhastalar = new List<Hasta>();//bir şekilde atanamayan olursa burada olacak
        public List<Hasta> atanacakhastalar = new List<Hasta>();

        static public void IDSifirla()
        {
            _staticId = 0;
        }
        public int kromozomId //üretişen her kromozomun unique bir id si var
        {
            get { return _kromozomId; }
        }
        public List<Rota> rotaListesi
        {
            get { return _rotaListesi; }
        }
        public int fitness
        {
            get { return _fitness; }
        }
        public Uzaklik2 toplamUzaklik
        {
            get { return _toplamUzaklik; }
        }
        public IlkAtamaYontem iay
        {
            get
            { return _iay; }
        }
        public Kromozom(IlkAtamaYontem ilkAtamaYontem)
        {
            _staticId++;
            this._kromozomId = _staticId;
            this._rotaListesi = new List<Rota>();//rota listesi oluşturuldu
            this._iay = ilkAtamaYontem;
            SkillMapOlustur();//Hasta ve ekip için skill kıtlığı tespitinde kullanılacak
            EkipleriAta();
            IlkAtamalariYap();
            foreach (Rota myrota in rotaListesi)
                myrota.KaydirmaUygula();
            
        }
        public Kromozom(Kromozom modelKromozom)
        {
            //referansı pas edilen kromozomun kopyası ile yeni bir kromozom döner
            //çaprazlama işlemlerinde kullanılması için yazıldı
            //dönen kromozom modelkromozom ile aynı ama ID si Farklı olacaktır.
            _staticId++;
            this._kromozomId = _staticId; //yeni kromozom id alındı
            this._rotaListesi = new List<Rota>();//rota listesi oluştu
            this._iay = modelKromozom.iay;//ilk atama yöntem modelden alındı
            foreach (Rota kopyaRota in modelKromozom.rotaListesi) //kromozomdaki rotaların kopyasını alır
                this._rotaListesi.Add(new Rota(kopyaRota));
        }
        private void AtanacakHastaListesiYukle(List<int> hastalist)
        {
            //sadece listeden gönderilen hastaları rotalara atamaya çalışır
            atanacakhastalar.Clear();
            foreach (Hasta hasta in Islemler.hastaListGun)
            {              
                if (hasta.hastaID == 0) continue;// sağlık merkezini atama
                 foreach (int hastaID in hastalist)
                    if(hastaID==hasta.hastaID)
                        atanacakhastalar.Add(hasta);
            }
        }
        private void AtanacakHastaListesiYukle()
        {
            atanacakhastalar.Clear();
            foreach (Hasta hasta in Islemler.hastaListGun)
            {
                if (hasta.hastaID == 0) continue;// sağlık merkezini atama
                atanacakhastalar.Add(hasta);
            }
        }
        private void EkipleriAta() //o gün çalışacak olan ekipler için başlangıç rotası
        {
            for (int i = 0; i < Islemler.ekipListGun.Count; i++)
            {
                Rota myrota = new Rota();
                Ekip myekip= Islemler.ekipListGun[i];
                myrota.ekip = myekip;

                //sabah mesai ekibe göre ayarla
                myrota.ziyaretSirasi[0].atandigiTimeWindow.t1 = myekip.sabahMesai.t1;
                myrota.ziyaretSirasi[0].atandigiTimeWindow.t2 = myekip.sabahMesai.t1;

                //öğleden sonraki mesai ekibe göre ayarla
                myrota.ziyaretSirasi[myrota.ziyaretSirasi.Count-1].atandigiTimeWindow.t1 = myekip.ogleMesai.t2;
                myrota.ziyaretSirasi[myrota.ziyaretSirasi.Count - 1].atandigiTimeWindow.t2 = myekip.ogleMesai.t2;

                

                rotaListesi.Add(myrota);
            }
        }
        private Rota EkibinRotasiniBul(int ekipID)
        {
            int sayac = 0;
            while (sayac<_rotaListesi.Count)
            {
                if (_rotaListesi[sayac].ekip.ekipID == ekipID)
                    return _rotaListesi[sayac];
                sayac++;
            }
            return null;
        }
        private void IlkAtamalariYap()
        {
            if (this._iay == IlkAtamaYontem.firstfit)
                HastaAtamasiYapFirsfit(null);//null değer bütün hastaların atanmasını sağlar
            if (this._iay == IlkAtamaYontem.bestfitteam)
                HastaAtamasiYapBestFitEkip(null);
            if (this._iay == IlkAtamaYontem.bestfitperiod)
                HastaAtamasiYapBestFitPeriyod(null);
        }
        public void ListedenAtamaYap(List<int> ilaveHastaList)
        {
            //burassı tamirat için kullanılacaktır,
            //kromozomdaki eksik yerlere yenisini atayacak
            if (this._iay == IlkAtamaYontem.firstfit)
                HastaAtamasiYapFirsfit(ilaveHastaList);//sadece ilavehastalist te bulunan hastaları atar
            if (this._iay == IlkAtamaYontem.bestfitteam)
                HastaAtamasiYapBestFitEkip(ilaveHastaList);
            if (this._iay == IlkAtamaYontem.bestfitperiod)
                HastaAtamasiYapBestFitPeriyod(ilaveHastaList);
        }

        private void HastaAtamasiYapBestFitEkip_onceki(List<int> myHastaList)
        {
            //skillmap değişmeden önceki hali bu 
            atanacakhastalar.Clear();
            if (myHastaList == null || myHastaList.Count == 0) //herhangi bir hasta listesi gelmemiş işe hepsi atanacak
                foreach (Hasta hasta in Islemler.hastaListGun)
                {
                    if (hasta.hastaID == 0) continue;// sağlık merkezini atama
                    atanacakhastalar.Add(hasta);
                }
            else //atama yapılacak hasta listesi dışarıdan gönderildi ise sadece o hastalar için atama yapacak
                foreach (Hasta hasta in Islemler.hastaListGun)
                {
                    if (hasta.hastaID == 0) continue;// sağlık merkezini atama
                    foreach (int hastaID in myHastaList)
                        if (hastaID == hasta.hastaID)
                            atanacakhastalar.Add(hasta);
                }

            //burada atanması gereken hasta listesi elde edildi, aşağıda bu listedeki hastaların ataması yapılacak

            CryptoRandom rnd = new CryptoRandom();
            int deger = 1;
            for (int s = 0; (atanacakhastalar.Count != 0 && s < hastaskilldongu); s++)
            {
                List<Hasta> altlist = new List<Hasta>();
                int i = 0;
                while (i < atanacakhastalar.Count)
                {
                    //LSb' den başlayarak MSB' ye kadar bitlerin 1 olup olmadığınabakıyor
                    //eğer bit 1 ise  hastayı altliste ekliyor
                    Hasta myhasta = atanacakhastalar[i];
                    int hastaskill = myhasta.skill;
                    if ((hastaskill & deger) == deger)//hasta skill değer ile AND yapıldı, 0. biti 1 olanları aldı, sonra 1. bit devsm edecek
                    {
                        altlist.Add(myhasta);
                        atanacakhastalar.RemoveAt(i);
                    }
                    else
                        i++;//kayıt silinirse bir geri geleceğinden sadece silmediğimizde artacak                       
                }
                deger *= 2;
                while (altlist.Count > 0)
                {
                    List<int> atanabilecekrotalar = new List<int>();
                    for (int j = 0; j < rotaListesi.Count; j++)
                        atanabilecekrotalar.Add(rotaListesi[j].ekip.ekipID);
                    bool atadim = false;

                    Rota atanacagirota;
                    int rndhasta = rnd.Next(0, altlist.Count);//0-count-1 aralığında üretir
                    Hasta secilenHasta = altlist[rndhasta];
                    while (atanabilecekrotalar.Count != 0)
                    {
                        int rndrota = rnd.Next(0, atanabilecekrotalar.Count);
                        atanacagirota = EkibinRotasiniBul(atanabilecekrotalar[rndrota]); //atanması için rastgele rota seçildi
                        atadim = atanacagirota.AtamaYap(secilenHasta);//hasta rotaya atanmış ise true döner
                        if (atadim) break; //rotaya atanmış ise while ı kır
                        atanabilecekrotalar.RemoveAt(rndrota); //rotaya atanamadı ise o rotayı sil diğer rotlara bak
                    }
                    if (!atadim)//hasta herhangi bir rotaya atanmadıysa
                    {
                        atanamayanhastalar.Add(secilenHasta);
                        return;//herhangi bir hasta rotaya atanamadıysa diğerlerine bakmak gereksiz
                    }
                    else
                        altlist.RemoveAt(rndhasta);//seçilen hastayı tekrar atamasın diye listeden çıkar     
                }
            }
        }
        private void HastaAtamasiYapFirsfit(List<int> myHastaList)
        {
            atanacakhastalar.Clear();
            if (myHastaList==null || myHastaList.Count==0) //herhangi bir hasta listesi gelmemiş işe hepsi atanacak
                foreach (Hasta hasta in Islemler.hastaListGun)
                {
                    if (hasta.hastaID == 0) continue;// sağlık merkezini atama
                    atanacakhastalar.Add(hasta);
                }
            else //atama yapılacak hasta listesi dışarıdan gönderildi ise sadece o hastalar için atama yapacak
                foreach (Hasta hasta in Islemler.hastaListGun)
                {
                    if (hasta.hastaID == 0) continue;// sağlık merkezini atama
                    for(int i=0;i<myHastaList.Count;i++)
                        if (myHastaList[i] == hasta.hastaID)
                            atanacakhastalar.Add(hasta);
                }

            //burada atanması gereken hasta listesi elde edildi, aşağıda bu listedeki hastaların ataması yapılacak
            CryptoRandom rnd = new CryptoRandom();

            Rota atanacagirota;
            while (atanacakhastalar.Count != 0)
            {
                int rndhasta = rnd.Next(0, atanacakhastalar.Count);//0-count-1 aralığında üretir
                Hasta secilenHasta = atanacakhastalar[rndhasta];
                List<int> atanabilecekrotalar = new List<int>();
                for (int i = 0; i < rotaListesi.Count; i++)
                    atanabilecekrotalar.Add(rotaListesi[i].ekip.ekipID);

                bool atadim = false;
                while (atanabilecekrotalar.Count != 0) //rastgele bir rotaı seçerek hastayı o rotaya atamaya çalışır
                {
                    int rndrota = rnd.Next(0, atanabilecekrotalar.Count);
                    //atanacagirota = _rotaListesi[rndrota]; //atanması için rastgele rota seçildi
                    atanacagirota = EkibinRotasiniBul(atanabilecekrotalar[rndrota]);
                    atadim = atanacagirota.AtamaYap(secilenHasta);//hasta rotaya atanmış ise true döner
                    if (atadim) break; //rotaya atanmış ise while ı kır
                    atanabilecekrotalar.RemoveAt(rndrota); //rotaya atanamadı ise o rotayı sil diğer rotlara bak
                }
                if (!atadim)//hasta herhangi bir rotaya atanmadıysa
                {
                    atanamayanhastalar.Add(secilenHasta);

                    Islemler.atanamayanHastalarListesi.Add(secilenHasta.hastaID.ToString());
                   
                    return;//herhangi bir hasta rotaya atanamadıysa diğerlerine bakmak gereksiz
                }                
                else
                    atanacakhastalar.RemoveAt(rndhasta);//seçilen hastayı tekrar atamasın diye listeden çıkar                    
            }
        }
        private void HastaAtamasiYapBestFitEkip(List<int> myHastaList)
        {
            //skillmap değiştiğinden dolayı bu yordam değiştirildi,
            //bu hali ile hasta  skill gereksiniminde her bit için en kıt kaynağa sahip olanlar  hasta nesnesinde belli
            //hasta.ekipihtiyacsira ya göre küçükten büyüğe sıralanacak ve öncelik öndeki hastalarda olacak şekilde seçim yapılacak
            atanacakhastalar.Clear();
            if (myHastaList == null || myHastaList.Count == 0) //herhangi bir hasta listesi gelmemiş işe hepsi atanacak
                foreach (Hasta hasta in Islemler.hastaListGun)
                {
                    if (hasta.hastaID == 0) continue;// sağlık merkezini atama
                    atanacakhastalar.Add(hasta);
                }
            else //atama yapılacak hasta listesi dışarıdan gönderildi ise sadece o hastalar için atama yapacak
                foreach (Hasta hasta in Islemler.hastaListGun)
                {
                    if (hasta.hastaID == 0) continue;// sağlık merkezini atama
                    foreach (int hastaID in myHastaList)
                        if (hastaID == hasta.hastaID)
                            atanacakhastalar.Add(hasta);
                }
           
            //burada atanması gereken hasta listesi elde edildi, aşağıda bu listedeki hastaların ataması yapılacak
            for (int i=0;i<atanacakhastalar.Count-1;i++)
                for (int j=i+1;j<atanacakhastalar.Count;j++)
                    if (atanacakhastalar[i].ekipihtiyacsira> atanacakhastalar[j].ekipihtiyacsira)
                    {
                        //hastalarsıraya konuyor
                        Hasta gecici = atanacakhastalar[i];
                        atanacakhastalar[i] = atanacakhastalar[j];
                        atanacakhastalar[j] = gecici;
                    }

            CryptoRandom rnd = new CryptoRandom();

            Rota atanacagirota;
            while (atanacakhastalar.Count != 0)
            {
                //rnd1 10 ise, rnd 0-10 aralığında olacaktır bu yöntem baştaki hastaların seçilme şansını artırır.
                int rnd1 = rnd.Next(0, atanacakhastalar.Count);//0-count-1 aralığında üretir
                int rndhasta = rnd.Next(0, rnd1 + 1);//liste başındaki bireylerin seçilme şansını arttırmak için ilk seçilen değer ile 0 arasında yeni bir konum

                Hasta secilenHasta = atanacakhastalar[rndhasta];

                List<int> atanabilecekrotalar = new List<int>();
                for (int i = 0; i < rotaListesi.Count; i++)
                    atanabilecekrotalar.Add(rotaListesi[i].ekip.ekipID);
                bool atadim = false;
                while (atanabilecekrotalar.Count != 0)
                {
                    int rndrota = rnd.Next(0, atanabilecekrotalar.Count);
                    atanacagirota = EkibinRotasiniBul(atanabilecekrotalar[rndrota]); //atanması için rastgele rota seçildi
                    atadim = atanacagirota.AtamaYap(secilenHasta);//hasta rotaya atanmış ise true döner
                    if (atadim) break; //rotaya atanmış ise while ı kır
                    atanabilecekrotalar.RemoveAt(rndrota); //rotaya atanamadı ise o rotayı sil diğer rotlara bak
                }
                if (!atadim)//hasta herhangi bir rotaya atanmadıysa
                {
                    atanamayanhastalar.Add(secilenHasta);
                    return;//herhangi bir hasta rotaya atanamadıysa diğerlerine bakmak gereksiz
                }
                atanacakhastalar.RemoveAt(rndhasta);//seçilen hastayı tekrar atamasın diye listeden çıkar                    
            }
        }
        private void HastaAtamasiYapBestFitPeriyod(List<int> myHastaList)
        {
            //atanacak hastalar bir list bu listede hastalar atanması gereken önceliğe göre yerleştirilmiş
            //liste oluşturulurken hastanın atanması istenen periyoda atanma olasılığına bakılıyor, önceliği yüksek olanlar daha öne gelebiliyor
      
            atanacakhastalar.Clear();

            if (myHastaList == null || myHastaList.Count == 0) //herhangi bir hasta listesi gelmemiş işe hepsi atanacak
                foreach (Hasta hasta in Islemler.hastaListGun)
                {
                    if (hasta.hastaID == 0) continue;// sağlık merkezini atama
                    int konum;
                    for (konum = 0; konum < atanacakhastalar.Count; konum++) //atama olasılığına göre liste yapıyor baştaki hastaların olasılığı daha yüksek
                    {
                      if (hasta.oncelik * hasta.bakimSuresi / (hasta.timeWindow.t2 - hasta.timeWindow.t1) > atanacakhastalar[konum].oncelik * atanacakhastalar[konum].bakimSuresi / (atanacakhastalar[konum].timeWindow.t2 - atanacakhastalar[konum].timeWindow.t1))
                        break;//konum uygun yere gelmiş ise for kırılır                  
                    }
                    atanacakhastalar.Insert(konum, hasta);//hasta atanma olasılığına göre uygun bir yere yerleştirildi
                }
            else //atanacak hastalistesi dışarıdan gödnerilmiş ise
                foreach (int hastaID in myHastaList)
                {
                    Hasta hasta=null;
                    for (int i=0;i<Islemler.hastaListGun.Count;i++)
                        if (Islemler.hastaListGun[i].hastaID == hastaID)
                        {
                            hasta = Islemler.hastaListGun[i];
                            break;//hasta bilgisi bulundu
                        }
                    int konum;
                    for (konum = 0; konum < atanacakhastalar.Count; konum++) //atama olasılığına göre liste yapıyor baştaki hastaların olasılığı daha yüksek
                    {
                        //hastayı listedeki 1. hasta ile karşılaştırıp ondan daha öncelikli ise onun önüne ekler, bu şekilde önceki her hasta ile mukayeseye devam eder
                        if (hasta.oncelik * hasta.bakimSuresi / (hasta.timeWindow.t2 - hasta.timeWindow.t1) > atanacakhastalar[konum].oncelik * atanacakhastalar[konum].bakimSuresi / (atanacakhastalar[konum].timeWindow.t2 - atanacakhastalar[konum].timeWindow.t1))
                            break;//konum uygun yere gelmiş ise for kırılır                  
                    }
                    atanacakhastalar.Insert(konum, hasta);//hasta atanma olasılığına göre uygun bir yere yerleştirildi
                }

            CryptoRandom rnd = new CryptoRandom();

            Rota atanacagirota;
            while (atanacakhastalar.Count != 0)
            {
                //rnd1 10 ise, rnd 0-10 aralığında olacaktır bu yöntem baştaki hastaların seçilme şansını artırır.
                int rnd1 = rnd.Next(0, atanacakhastalar.Count);//0-count-1 aralığında üretir
                int rndhasta = rnd.Next(0, rnd1 + 1);//liste başındaki bireylerin seçilme şansını arttırmak için ilk seçilen değer ile 0 arasında yeni bir konum

                Hasta secilenHasta = atanacakhastalar[rndhasta];

                List<int> atanabilecekrotalar = new List<int>();
                for (int i = 0; i < rotaListesi.Count; i++)
                    atanabilecekrotalar.Add(rotaListesi[i].ekip.ekipID);
                bool atadim = false;
                while (atanabilecekrotalar.Count != 0)
                {
                    int rndrota = rnd.Next(0, atanabilecekrotalar.Count);
                    atanacagirota = EkibinRotasiniBul(atanabilecekrotalar[rndrota]); //atanması için rastgele rota seçildi
                    atadim = atanacagirota.AtamaYap(secilenHasta);//hasta rotaya atanmış ise true döner
                    if (atadim) break; //rotaya atanmış ise while ı kır
                    atanabilecekrotalar.RemoveAt(rndrota); //rotaya atanamadı ise o rotayı sil diğer rotlara bak
                }
                if (!atadim)//hasta herhangi bir rotaya atanmadıysa
                {
                    atanamayanhastalar.Add(secilenHasta);
                    return;//herhangi bir hasta rotaya atanamadıysa diğerlerine bakmak gereksiz
                }
                atanacakhastalar.RemoveAt(rndhasta);//seçilen hastayı tekrar atamasın diye listeden çıkar                    
            }
        }
        //private void SkillMapOlustur_onceki()
        //{
        //    //önceki kod idi 04.07.2020 de değiştirildi. ilaveler yapıldı, bu altyordam ihtiyaç olabilir diye kaldı
        //    //skill kıtlığını bulmak için kullanılıyor ekiplerin he hastaların ayrı ayrı skillerini
        //    //sayacak ve daha sonra bu sayılar oranlanarak en kıt skill bulunacak
        //    //sadece ilk nesne oluştuğunda çalışacak daha sonrak üretilen
        //    //neseler de çalışması gerekmiyor.
        //    //nesne programlama ilkeleri ile singleton olarak da yazılabilir daha sonra değerlendir
        //    if (_kromozomId > 1) return; //sadece bir kere çalışsın diye yazıldı
        //    foreach (Hasta myhasta in Islemler.hastaListGun)
        //    {
        //        int deger = 1;
        //        int skill = myhasta.skill;
        //        if (myhasta.hastaID == 0) continue;//eğer sağlık merkezi ise devam et
        //        int dongusayisi=0;
        //        if (skill > 1)
        //        {
        //            double dongu = Math.Log(skill, 2);
        //            dongusayisi = Convert.ToInt32((dongu));
        //            if (dongu == dongusayisi) dongusayisi++;
        //        }
                    
        //        else
        //            dongusayisi = 1;
              
        //        if (dongusayisi > hastaskilldongu)
        //            hastaskilldongu = dongusayisi;//atamalarda kullanılacak hastaların max skill bit sayısıdır

        //        for (int i = 0; i < dongusayisi; i++)
        //        {  
        //            int mask=skill&deger;//deger ile andlendi
        //            hastaskillmap[i] += mask >> i;//deger 1 ise artırdı değilse artırmadı
        //            deger *= 2;
        //        }
        //    }

        //    foreach (Ekip myekip in Islemler.ekipListGun)
        //    {
        //        int deger = 1;
        //        int skill = myekip.skill;   
        //        int dongusayisi = Convert.ToInt32(Math.Log(skill, 2)); //32 bitin hepsini dönmüyor sadece ihtiyaç kadar dönecek
        //        for (int i = 0; i < dongusayisi; i++)
        //        {
        //            int mask = skill & deger;//deger ile and lendi
        //            ekipskillmap[i] += mask >> i;//deger 1 ise artırdı değilse artırmadı
        //            deger *= 2;
        //        }
        //    }

        //    for (int i = 0;i < skillmaporan.Length; i++)
        //    {
        //        if (hastaskillmap[i] == 0)
        //            skillmaporan[i] = Islemler.M;//sayı/0 hata vereceğinden büyük bir değer atandı
        //        else
        //            skillmaporan[i] =ekipskillmap[i]/( hastaskillmap[i]* Islemler.M* Islemler.M* Islemler.M);
        //    }
           
               
        //}
        private void SkillMapOlustur()
        {
            //skill kıtlığını bulmak için kullanılıyor ekiplerin he hastaların ayrı ayrı skillerini
            //sayacak ve daha sonra bu sayılar oranlanarak en kıt skill bulunacak ekip/hasta, eğer değer>1 ise değeri 1 alacak
            //sadece ilk nesne oluştuğunda çalışacak daha sonrak üretilen
            //neseler de çalışması gerekmiyor.
            //nesne programlama ilkeleri ile singleton olarak da yazılabilirdi ama hızlıca yazıldığından bu şekilde kodlandı, eğer vakit olursa yapısallaık için değiştir
            if (_kromozomId > 1) return; //sadece bir kere çalışsın diye yazıldı

            int dongusayisi = 0; //hastalarda eğer skill durumu daha az ise daha kısa döngü ile halletsin diye
            foreach (Hasta myhasta in Islemler.hastaListGun)
            {
                int deger = 1;
                int skill = myhasta.skill;
                if (myhasta.hastaID == 0) continue;//eğer sağlık merkezi ise devam et, sağlık merkezi skill değeri=0
                dongusayisi = 0;
                if (skill > 1)
                {
                    double dongu = Math.Log(skill, 2);
                    dongusayisi = Convert.ToInt32((dongu));
                    if (dongu == dongusayisi) dongusayisi++;
                }

                else
                    dongusayisi = 1;

                if (dongusayisi > hastaskilldongu)
                    hastaskilldongu = dongusayisi;//atamalarda kullanılacak hastaların max skill bit sayısıdır

                for (int i = 0; i < dongusayisi; i++)
                {
                    int mask = skill & deger;//deger ile andlendi
                    hastaskillmap[i] += mask >> i;//hasta skill digitleri sayıyor, her diğitten kaçadet olduğunu bulacak
                    deger *= 2;
                }
            }

            foreach (Ekip myekip in Islemler.ekipListGun)
            {
                int deger = 1;
                int skill = myekip.skill;
                dongusayisi = Convert.ToInt32(Math.Log(skill, 2)); //32 bitin hepsini dönmüyor sadece ihtiyaç kadar dönecek
                for (int i = 0; i < dongusayisi; i++)
                {
                    int mask = skill & deger;//deger ile and lendi
                    ekipskillmap[i] += mask >> i;//ekip skill digitleri sayıyor
                    deger *= 2;
                }
            }
            double oran = 0;
            for (int i = 0; i < skillmaporan.Length; i++)
            {
                if (hastaskillmap[i] == 0)
                    skillmaporan[i] = 1.0/ Islemler.M2;//sayı/0 hata vereceğinden sabit bir değer atandı 
                else
                {
                    oran= ekipskillmap[i] / hastaskillmap[i];
                    if (oran > 1) oran = 1.0;
                    skillmaporan[i] = oran / Islemler.M2;
                }
                    
            }

            foreach(Hasta myhasta in Islemler.hastaListGun)
            {
              //her hasta için her bir işlemin (ikilik digitle ifade edilen) ne kadar kıtlık olduğunu bulur
              //kıtlığı daha çok olan değerler daha küçük olanlardır
              //daha sonra kıtlık değerine göre küçükten büyüğe sıraya kondupunda ilk değerleirn önce seçilmesi gerekecek
                int skill = myhasta.skill;
                for (int i = 0; i < skillmaporan.Length; i++)
                {
                    int bit = skill % 2;//mod 2 ile sayının lsb olan bitini al, 101 için 101 mod 2=1 olur
                    if (bit == 1)
                        myhasta.kitliklist.Add(skillmaporan[i]);
                    else
                      myhasta.kitliklist.Add(1.0 / (Islemler.M2 ));//m2 gibi bir değere bölünmese double' ın değer aralığını geçiyor
                    skill /= 2;//sıradaki digite ulaşmak için ise 2 ye bölünüyor 101 değeri için 101/2=50 oldu
                 
                }
                 myhasta.kitliklist.Sort(); //en kıt değer en küçük olduğundan kıtlık değerine göre sıralandı
                for (int i = 0; i <myhasta.kitliklist.Count; i++)
                {
                    //kıtlık değerine göre sıralanmış kıtlıklar bir sayıya çevriliyor
                    //0,1-0.2 nin değeri=12 / 0,1-0.3 ün değeri=13/ 0.2-1 in değeri 21 olacak, 1. durumdaki öncelikli demektir
                    //en küçük değere sahip olan hasta en kıt kaynaklara ihtiyaç duyan hastadır
                    //skill' e göre atama yapan yordam buna göre seçim şansını artırarak seçim yapacak
                    myhasta.ekipihtiyacsira *= 10;
                    myhasta.ekipihtiyacsira += myhasta.kitliklist[i];
                }
            }
        }
        public void FitnessHesapla()
        {
            //kromozomun fitness değerini hesaplar
            //fitnes değerinde hem cezaların toplamı hemde toplam yolculuk mesafesi dikkate alınır.
            //yolculuk süresini dikkate aldığından 0 fitnesli kromozom olmaz

           

            _fitness = 0;
            List<double>rotaUzaklikMetre = new List<double>();
            List<int> rotaUzaklikperyod = new List<int>();
            //krmozoma ait cezaların güncllenmesi
            foreach (Rota myrota in _rotaListesi)
            {
                myrota.RotaCezaGuncelle(); //function içinde uzaklıklar hesaplanmakta ve cezalı durumlarda cezalar eklenmekte
                _fitness += myrota.rotaceza;

                rotaUzaklikMetre.Add(myrota.toplamUzaklik.metre);
                rotaUzaklikperyod.Add(myrota.toplamUzaklik.dakika);
            }

            //metre ve dakika cinsinden sapmaları hesaplamak için aşağıdaki kısımlar kullanıldı
            //metre

            _toplamUzaklik.metre = rotaUzaklikMetre.Sum();
            _toplamUzaklik.dakika = rotaUzaklikperyod.Sum();
            double average = rotaUzaklikMetre.Average();
            double toplamfark = 0;
            foreach (double value in rotaUzaklikMetre)
                toplamfark += (value -average)* (value - average);
            double sapmametre = Math.Sqrt(toplamfark / (rotaUzaklikMetre.Count - 1));//exceldeki, STDSAPMA.S ile aynı sonucu veriyor

            //dakika
            average = rotaUzaklikperyod.Average();
            toplamfark = 0;
            foreach (double value in rotaUzaklikperyod)// değer int ama double a atandı çünkü average double
                toplamfark += (value - average) * (value - average);
            double sapmaperiyod = Math.Sqrt(toplamfark / (rotaUzaklikperyod.Count - 1));

            _fitness += (int)sapmametre * Islemler.CezaPuanlari[Cezalar.sSapmaMetre];//metre sapması cezaya eklendi
            _fitness+= (int)sapmaperiyod * Islemler.CezaPuanlari[Cezalar.sSapmaDakika];//metre sapması cezaya eklendi

           
            
        }
        public void YeniRotaEkle(Rota yeniRota)
        {
            //rota listesine yenirota ile belirtilen rotayı ekler

        }
        public bool Tamir( int sabitRota)
        {          
            //kromozom tamiri için kullanıacaktır
            //kromozomdaki fazla olan hastaID leri bularak silecek,
            //eksik olanları ise ilk atama yöntemine göre rastgele ekleyecektir
            //sabitrota ile pas edilen rotadan silme işlemi yapılmayacaktır. bu rora yeni eklenen rotadır

            //kromozomdaki fazla hastalar bulunurak siliniyor
            //sabit rotadan silme yapılmayacak
            int[] hastaAtamaSayisi = new int[Islemler.hastaListGun.Count]; //hastaların atanma sayısını bulmak için, 0. hasta harriç, 0. hasta Sağlık merkezi
            foreach (Rota myrota in rotaListesi)
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
            for (int i = 0; i < fazlaAtama.Count; i++)
                for (int j = 0; j < rotaListesi.Count; j++)
                {
                    if (j == sabitRota) continue;//sabitrota çapralama ile kromozoma dahil edildi bu rota silinmemmeli
                    for (int k = 1; k < rotaListesi[j].ziyaretSirasi.Count - 1; k++)//0. ve sonuncu genler sağlık merkezi
                        if (fazlaAtama[i] == rotaListesi[j].ziyaretSirasi[k].hasta.hastaID)
                            rotaListesi[j].ziyaretSirasi.RemoveAt(k);//fazla olan hastaya ait gen silindi
                }

            //Eksik olan hastaların rotalara atanması işlemi yapılacak
            //atama sonunda atanamayan hasta listesinde hasta varsa bazı hastalar yerleşmemiş demektir
            //yerleşemeyen hastalar olması durumunda kromozomun tamiri mümkün olmamkıştır
            //tamir edilemeyen durumlarda geriye false değer döner
            ListedenAtamaYap(eksikAtama);//eksik olan mhastaların kromozoma atanmasını sağlar
            if (atanamayanhastalar.Count > 0)
                return false;//atanamayan hasta varsa eksik olan hastalardan bazıları yerleşmemiştir


            foreach (Rota rota in rotaListesi)
                rota.KaydirmaUygula();

            return true;
        }
        public bool Mutasyon(int oran)
        {
            /*
             * Mutasyon denemelerinde ciddi bir etki göstermedi.
             * Sebep olarak zaten tamir olayı mutasyona benzer bir işlem gerçekleştiriyor
             * kromozomda mutasyon işlemi gerçekleştirecek
             * kaç gen silineceğine mutasyon oranı karar verecek, 50 gen için %1 ise 1 gen, %2 ise 1 gen, %5 ise 3 gen rastgele seçilerek silinecek
             * mutasyon işlemini sadece çocuk kromozomlara yapmayı deneyeceğiz
             * seçilen kromozomdan rastgele olarak bir gen seçerek  silecek ve tamir ile tekrar bu geni yerine koymaya çalışacak
             * eğer tamir başarılı olursa mutasyon başarılı olup kromozom populasyona eklenecek
             * eğer mutasyon sonunda genler tamir edilemez ise? normalde eski yerine eklenebilmesi gerekir populasyona eklenemeyecek
             */
            int silinecekgensayisi = (int)Math.Ceiling(1.0*(Islemler.hastaListGun.Count - 1) * oran / 100);
            CryptoRandom rnd = new CryptoRandom();
            while (silinecekgensayisi>=1)
            {
                int rotaId = rnd.Next(0,rotaListesi.Count);
                Rota secilenrota = rotaListesi[rotaId];
                if (secilenrota.ziyaretSirasi.Count>2)
                {
                    int secilengen = rnd.Next(1,secilenrota.ziyaretSirasi.Count-1);
                    secilenrota.ziyaretSirasi.RemoveAt(secilengen);
                    silinecekgensayisi--;
                }
            }
            //silme işlemi tamamlandı artık kromozom tamire gönderilebilir
            if (Tamir(0)) return true;//tamir edebilmiş ise true döner, edememişse false dönecek. gödnerilen 0 değerinin bir önemi yok çünkü sadece ekleme yapacak
            return false;
        }
    
    }
}
