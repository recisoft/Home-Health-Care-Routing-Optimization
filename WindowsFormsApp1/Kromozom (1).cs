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
                myrota.ekip = Islemler.ekipListGun[i];
                rotaListesi.Add(myrota);
            }
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
                    atanabilecekrotalar.Add(i);

                bool atadim = false;
                while (atanabilecekrotalar.Count != 0) //rastgele bir rotaı seçerek hastayı o rotaya atamaya çalışır
                {
                    int rndrota = rnd.Next(0, atanabilecekrotalar.Count);
                    atanacagirota = _rotaListesi[rndrota]; //atanması için rastgele rota seçildi
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
        private void HastaAtamasiYapBestFitEkip(List<int> myHastaList)
        {
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
            for (int s=0;(atanacakhastalar.Count!=0 && s<hastaskilldongu);s++)
            {
                
                List<Hasta> altlist = new List<Hasta>();
                int i = 0;
                while(i<atanacakhastalar.Count)
                {
                    Hasta myhasta = atanacakhastalar[i];
                    int hastaskill = myhasta.skill;
                if ((hastaskill & deger) == deger)//hasta skill değer ile AND yapıldı
                {
                    altlist.Add(myhasta);
                    atanacakhastalar.RemoveAt(i);
                }
                else
                    i++;//kayıt silinirse bir geri geleceğinden sadece silmediğimizde artacak                       
                }
                deger *= 2;
                while(altlist.Count>0)
                {
                    List<int> atanabilecekrotalar = new List<int>();
                    for (int j = 0; j < rotaListesi.Count; j++)
                        atanabilecekrotalar.Add(j);
                    bool atadim = false;

                    Rota atanacagirota;
                    int rndhasta = rnd.Next(0, altlist.Count);//0-count-1 aralığında üretir
                    Hasta secilenHasta = altlist[rndhasta];
                    while (atanabilecekrotalar.Count != 0)
                    {
                        int rndrota = rnd.Next(0, atanabilecekrotalar.Count);
                        atanacagirota = _rotaListesi[rndrota]; //atanması için rastgele rota seçildi
                        atadim = atanacagirota.AtamaYap(secilenHasta);//hasta rotaya atanmış ise true döner
                        if (atadim) break; //rotaya atanmış ise while ı kır
                        atanabilecekrotalar.RemoveAt(rndrota); //rotaya atanamadı ise o rotayı sil diğer rotlara bak
                    }
                    if (!atadim)//hasta herhangi bir rotaya atanmadıysa
                    {
                        atanamayanhastalar.Add(secilenHasta);
                        return;//herhangi bir hasta rotaya atanamadıysa diğerlerine bakmak gereksiz
                    }
                    altlist.RemoveAt(rndhasta);//seçilen hastayı tekrar atamasın diye listeden çıkar     
                }                              
            }
        }
        private void HastaAtamasiYapBestFitPeriyod(List<int> myHastaList)
        {
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
            //else //atanacak hastalistesi dışarıdan gödnerilmiş ise
            //    foreach (Hasta hasta in Islemler.hastaListGun)
            //    {
            //        if (hasta.hastaID == 0) continue;// sağlık merkezini atama
            //        int konum=0;//değer içerideki for döngüsünde değişiyor
            //        foreach(int hastaID in myHastaList)
            //            if(hastaID==hasta.hastaID)
            //                for (konum = 0; konum < atanacakhastalar.Count; konum++) //atama olasılığına göre liste yapıyor baştaki hastaların olasılığı daha yüksek
            //                {
            //                    if (hasta.oncelik * hasta.bakimSuresi / (hasta.timeWindow.t2 - hasta.timeWindow.t1) > atanacakhastalar[konum].oncelik * atanacakhastalar[konum].bakimSuresi / (atanacakhastalar[konum].timeWindow.t2 - atanacakhastalar[konum].timeWindow.t1))
            //                        break;//konum uygun yere gelmiş ise for kırılır                  
            //                }
            //        atanacakhastalar.Insert(konum, hasta);//hasta atanma olasılığına göre uygun bir yere yerleştirildi
            //    }
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
                        if (hasta.oncelik * hasta.bakimSuresi / (hasta.timeWindow.t2 - hasta.timeWindow.t1) > atanacakhastalar[konum].oncelik * atanacakhastalar[konum].bakimSuresi / (atanacakhastalar[konum].timeWindow.t2 - atanacakhastalar[konum].timeWindow.t1))
                            break;//konum uygun yere gelmiş ise for kırılır                  
                    }
                    atanacakhastalar.Insert(konum, hasta);//hasta atanma olasılığına göre uygun bir yere yerleştirildi
                }

            CryptoRandom rnd = new CryptoRandom();

            Rota atanacagirota;
            while (atanacakhastalar.Count != 0)
            {
                int rnd1 = rnd.Next(0, atanacakhastalar.Count);//0-count-1 aralığında üretir
                int rndhasta = rnd.Next(0, rnd1 + 1);//liste başındaki bireylerin seçilme şansını arttırmak için ilk seçilen değer ile 0 arasında yeni bir konum

                Hasta secilenHasta = atanacakhastalar[rndhasta];

                List<int> atanabilecekrotalar = new List<int>();
                for (int i = 0; i < rotaListesi.Count; i++)
                    atanabilecekrotalar.Add(i);
                bool atadim = false;
                while (atanabilecekrotalar.Count != 0)
                {
                    int rndrota = rnd.Next(0, atanabilecekrotalar.Count);
                    atanacagirota = _rotaListesi[rndrota]; //atanması için rastgele rota seçildi
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
        private void SkillMapOlustur()
        {
            //skill kıtlığını bulmak için kullanılıyor ekiplerin he hastaların ayrı ayrı skillerini
            //sayacak ve daha sonra bu sayılar oranlanarak en kıt skill bulunacak
            //sadece ilk nesne oluştuğunda çalışacak daha sonrak üretilen
            //neseler de çalışması gerekmiyor.
            //nesne programlama ilkeleri ile singleton olarak da yazılabilir daha sonra değerlendir
            if (_kromozomId > 1) return; //sadece bir kere çalışsın diye yazıldı
            foreach (Hasta myhasta in Islemler.hastaListGun)
            {
                int deger = 1;
                int skill = myhasta.skill;
                if (myhasta.hastaID == 0) continue;//eğer sağlık merkezi ise devam et
                int dongusayisi=0;
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
                    int mask=skill&deger;//deger ile andlendi
                    hastaskillmap[i] += mask >> i;//deger 1 ise artırdı değilse artırmadı
                    deger *= 2;
                }
            }

            foreach (Ekip myekip in Islemler.ekipListGun)
            {
                int deger = 1;
                int skill = myekip.skill;   
                int dongusayisi = Convert.ToInt32(Math.Log(skill, 2)); //32 bitin hepsini dönmüyor sadece ihtiyaç kadar dönecek
                for (int i = 0; i < dongusayisi; i++)
                {
                    int mask = skill & deger;//deger ile and lendi
                    ekipskillmap[i] += mask >> i;//deger 1 ise artırdı değilse artırmadı
                    deger *= 2;
                }
            }

            for (int i = 0;i < skillmaporan.Length; i++)
            {
                if (hastaskillmap[i] == 0)
                    skillmaporan[i] = Islemler.M;//sayı/0 hata vereceğinden büyük bir değer atandı
                else
                    skillmaporan[i] =ekipskillmap[i]/ hastaskillmap[i];
            }
               
        }        
        public void FitnessHesapla()
        {
            //kromozomun fitness değerini hesaplar
            //fitnes değerinde hem cezaların toplamı hemde toplam yolculuk mesafesi dikkate alınır.
            //yolculuk süresini dikkate aldığından 0 fitnesli kromozom olmaz
            _fitness = 0;
            foreach (Rota myrota in _rotaListesi)
            {
                myrota.RotaCezaGuncelle();
                _fitness += myrota.rotaceza;
            }
        }
        public void YeniRotaEkle(Rota yeniRota)
        {
            //rota listesine yenirota ile belirtilen rotayı ekler

        }
    }
}
