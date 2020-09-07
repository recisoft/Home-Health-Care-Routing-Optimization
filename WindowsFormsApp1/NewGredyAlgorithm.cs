using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    class NewGredyAlgorithm
    {
        private List<Rota> _rotaList;
        private List<Ekip> _ekipList;
        public  List<Hasta> _hastaList;
        public List<Rota> rotaList
        {
            get { return _rotaList; }
        }
      
        public NewGredyAlgorithm()
        {
            /*
             * Ekip listelelerini ve hasta listelerini ıslemler den yükler
             * Ekip listesi ve hasta listesi buraya aktarıldığı için GA çalışmasını etkilemeycek
             * Çalışma sıraında rastgele seçilecek ekip bilgisi bu listelerden alınacak
             * seçilen ekip listeden silinecek ve tekrar seçilmemiş olacak
             * hastalar ise ancak atama yapıldıktan sonra silinecek
             */
            _ekipList = new List<Ekip>();
            _hastaList = new List<Hasta>();
            _rotaList = new List<Rota>();
            foreach (Ekip ekip in Islemler.ekipListGun) //ekip listesi alındı
                _ekipList.Add(ekip);
            foreach (Hasta hasta in Islemler.hastaListGun)//hasta listesi alındı
            {
               // if (hasta.hastaID == 0) continue;//0 ID li hasta bakım merkezi onu listeye ekleme
                _hastaList.Add(hasta);
            }
                
        }       
        public Ekip EkipSec_onceki()
        {
            //önce yazılmıştı sonra değiştirildi artık kullanılmıyor ama deneylerde işe yarabilir

            //mevcut kalan ekiplerin içinde skill değerine göre en az skill değeri olan  ekibi seçer, 
            //bu sayede daha çok skill olan ekipler daha az skil olan hastalara atanmamış olur
            //seçimi yaparken hasta skill değerlerini de dikkate alır
            //her digit için hasta ve ekip yeteneklerini sayar,
            //yetenek_karsilama=ekip/hasta ile digit karşılayabilme oranını bulur.
            //Topla(ekipdigit[i]*yetenek_karsilama[i]), i burada digit indsididir
            //her ekip için yukarıdaki adımı tekarladıktan sonra en düşün değerli olan değeri seçer
            //yapılan bu seçimde en düşük değer seçildiğinden önce budeğere uyan hastalar atanır
            //bu sayede hastalar en dğşğk skill değerine sahip ekip' e atanmaya çalışılır.
            //hasta ve ekip atamaları yapılnıca listeden silinerek işlem tekrar edilir.
            double[] ekipdeger = new double[_ekipList.Count];

            #region dongusayisinibul  
            //zaman karmaşıklığı için döngü sayısını bulmaya çalışıyor
            //100 ise döngü 3 tür, 1000 ise 4 tür. 32 bit için 32 adet döngüden daha az gerekebilir diye yazıldı
            //eğer her seferinde 32 bit kullanılıyorsa (özellikle cinsiyet bitleri kullanılan durumlarda 32 bit kullanılıyor demektir) bu kısım kullanılmayabilir

            int ebskill = 0;//en büyük skill değeri ile döngü sayısı hesaplanacak
            int tumekiplerskill = 0;//bütün ekiplere ait skill değerlerinin OR ile değerini toplayacak, 100 OR 101=111 olacak, 110 OR 100=110 olacak
            foreach (Ekip ekip in _ekipList)
            {
                if (ekip.skill > ebskill) ebskill = ekip.skill;
                tumekiplerskill |= ekip.skill;// OR işlemi yapıyor ekip1=1001, ekip2=0011 ise somnuç 1011 olacak
            }
                

            string hastaskillkarsilanamayanId = "";//skill karşılanamayan hastaIdlerinin listesini tutacak

            foreach (Hasta hasta in _hastaList)    
                if ((hasta.skill & tumekiplerskill)> hasta.skill) //hasta skill ile tumekiplerskill değeri bitsel AND yapıldı hasta 1100 ise Tümekipler skill 1011 olsa bu hastaya atama yapacak ekip yok demektir
                    hastaskillkarsilanamayanId += hasta.hastaID.ToString() + " - ";

            if (hastaskillkarsilanamayanId!="")//skill karşılanamayan hastalar  var ise program atama yapamaz
            {
                MessageBox.Show("Bakım istekleri karşılanamayan hastalar var\n"+ hastaskillkarsilanamayanId);
                return null;
            }

            int dongusayisi = (int)Convert.ToDouble(Math.Log(ebskill, 2));//skill=2 ise 1, 3 ise 1, 4 ise 2 çıkar, 
            dongusayisi++;//log(2,4)=2 çıkar, ++ ile 3 olur;
            #endregion
            int[] ekipskilldigit = new int[dongusayisi];//skill digitleri tutacak 
            int[] hastaskilldigit = new int[dongusayisi];
            
            foreach(Hasta hasta in _hastaList) //hastalar için her bir skill'in digitlerini sayar
            {
                int deger = 1;
                for (int i = 0; i <dongusayisi ; i++)
                {
                    int mask = hasta.skill & deger;//deger ile and lendi
                    hastaskilldigit[i] += mask >> i;//deger 1 ise artırdı değilse artırmadı
                    deger *= 2;
                }
            }
            foreach (Ekip ekip in _ekipList)//Ekipler için her bir skill' in digitlerini sayar
            {
                int deger = 1;
                for (int i = 0; i < dongusayisi; i++)
                {
                    int mask = ekip.skill & deger;//deger ile and lendi
                    ekipskilldigit[i] += mask >> i;//deger 1 ise artırdı değilse artırmadı
                    deger *= 2;
                }
            }
            for (int i = 0; i < ekipdeger.Length; i++) //ekip in 1 olan digitlerinin kıtlığını bulur
                for (int j = 0; j < dongusayisi; j++)
                {
                    int digitdegeri = (_ekipList[i].skill & (int)Math.Pow(2, j));
                    digitdegeri = digitdegeri >> j;//1 veya 0 değeri verecek digit degeridir
                    ekipdeger[i] += (((double)ekipskilldigit[j]) / hastaskilldigit[j]) * digitdegeri;//ilgili digit 1 ise dikkate alır, aksi halde almaz
                }
                   
            int indis = 0;double ek = ekipdeger[0];
            for (int i=1;i<ekipdeger.Length;i++)
                if (ekipdeger[i] < ek)
                {
                    indis = i;//seçilecek olan indisi tutar
                    ek = ekipdeger[i];
                }
            return _ekipList[indis];         
        }

        public Ekip EkipSec()
        {
            //yeni yazılan ve kullanılan kod, Ga daki skill map ile aynı matnıkla çalışıyor
            //mevcut kalan ekiplerin içinde skill değerine göre en az skill değeri olan  ekibi seçer, 
            //bu sayede daha çok skill olan ekipler daha az skil olan hastalara atanmamış olur
            //seçimi yaparken hasta skill değerlerini de dikkate alır
            //her digit için hasta ve ekip yeteneklerini sayar,
            //yetenek_karsilama=ekip/hasta ile digit karşılayabilme oranını bulur.
            //Topla(ekipdigit[i]*yetenek_karsilama[i]), i burada digit indsididir
            //her ekip için yukarıdaki adımı tekarladıktan sonra en düşün değerli olan değeri seçer
            //yapılan bu seçimde en düşük değer seçildiğinden önce budeğere uyan hastalar atanır
            //bu sayede hastalar en dğşğk skill değerine sahip ekip' e atanmaya çalışılır.
            //hasta ve ekip atamaları yapılnıca listeden silinerek işlem tekrar edilir.
            double[] ekipdeger = new double[_ekipList.Count];//ekibe ait atanma olasılığını tuacak olan double değerdir

            #region dongusayisinibul  
            //zaman karmaşıklığı için döngü sayısını bulmaya çalışıyor
            //100 ise döngü 3 tür, 1000 ise 4 tür. 32 bit için 32 adet döngüden daha az gerekebilir diye yazıldı
            //eğer her seferinde 32 bit kullanılıyorsa (özellikle cinsiyet bitleri kullanılan durumlarda 32 bit kullanılıyor demektir) bu kısım kullanılmayabilir

            int ebskill = 0;//en büyük skill değeri ile döngü sayısı hesaplanacak
            int tumekiplerskill = 0;//bütün ekiplere ait skill değerlerinin OR ile değerini toplayacak, 100 OR 101=111 olacak, 110 OR 100=110 olacak
            foreach (Ekip ekip in _ekipList)
            {
                if (ekip.skill > ebskill) ebskill = ekip.skill;
                tumekiplerskill |= ekip.skill;// OR işlemi yapıyor ekip1=1001, ekip2=0011 ise somnuç 1011 olacak
            }


            string hastaskillkarsilanamayanId = "";//skill karşılanamayan hastaIdlerinin listesini tutacak

            foreach (Hasta hasta in _hastaList)
                if ((hasta.skill & tumekiplerskill) != hasta.skill) //hasta skill ile tumekiplerskill değeri bitsel AND yapıldı hasta 1100 ise Tümekipler skill 1011 olsa bu hastaya atama yapacak ekip yok demektir
                    hastaskillkarsilanamayanId += hasta.hastaID.ToString() + " - ";

            if (hastaskillkarsilanamayanId != "")//skill karşılanamayan hastalar  var ise program atama yapamaz
            {
                MessageBox.Show("Bakım istekleri karşılanamayan hastalar var\n" + hastaskillkarsilanamayanId);
                return null;
            }

            int dongusayisi = (int)Convert.ToDouble(Math.Log(ebskill, 2));//skill=2 ise 1, 3 ise 1, 4 ise 2 çıkar, 
            dongusayisi++;//log(2,4)=2 çıkar, ++ ile 3 olur;
            #endregion
            int[] ekipskilldigit = new int[dongusayisi];//skill digitleri tutacak 
            int[] hastaskilldigit = new int[dongusayisi];
            double[] skillmaporan = new double[dongusayisi];// her skill için toplam ekipskill/toplam hastaskill değerlerini tutacak olan dizi
            foreach (Hasta hasta in _hastaList) //hastalar için her bir skill'in digitlerini sayar
            {
                int deger = 1;
                for (int i = 0; i < dongusayisi; i++)
                {
                    int mask = hasta.skill & deger;//deger ile and lendi
                    hastaskilldigit[i] += mask >> i;//deger 1 ise artırdı değilse artırmadı
                    deger *= 2;
                }
            }
            foreach (Ekip ekip in _ekipList)//Ekipler için her bir skill' in digitlerini sayar
            {
                int deger = 1;
                for (int i = 0; i < dongusayisi; i++)
                {
                    int mask = ekip.skill & deger;//deger ile and lendi
                    ekipskilldigit[i] += mask >> i;//deger 1 ise artırdı değilse artırmadı
                    deger *= 2;
                }
            }
            
            foreach (Ekip ekip in _ekipList)
            {
                int ekipskill = ekip.skill;
                ekip.kitliklist.Clear();
                for (int i=0;i<dongusayisi;i++)
                {
                    if (ekipskill % 2 == 1) //ekipte ilgili skill kullanılıyor ise o digiti dikkate alacak
                        if (hastaskilldigit[i] == 0) //hasta skil ltoplamı eğer 0 ise sayı/0 hata vermesin diyeyazıldı
                            ekip.kitliklist.Add(1.0 / Islemler.M2); //değerlerin hepsi M2 gibi büyük bir sayıya bölünüyork, en sonda real bir sayıya çevrildiğinde double değerini aşmasın
                        else if ((double)ekipskilldigit[i] / (hastaskilldigit[i]) > 1)//eğer ekip skill toplamı hasta skillden fazla ise o skill yeterince var demektir atamada oran kullanıldığından 1 alınır
                            ekip.kitliklist.Add(1.0 / (hastaskilldigit[i] * Islemler.M2));
                        else
                            ekip.kitliklist.Add((double)ekipskilldigit[i] / (hastaskilldigit[i] * Islemler.M2));
                    else ekip.kitliklist.Add(1);
                    ekipskill /= 2;//sürekli 2 ye bölerek sıradaki skill değerini alıyor
                }
                ekip.kitliklist.Sort();//değerler kitlik drumlarına göre sıralandı
                ekip.kitlikdegeri = 0;//kitlik değeri yeniden hesaplanacağı için sıfırlandı
                for (int i=0;i<ekip.kitliklist.Count;i++)
                {
                    ekip.kitlikdegeri *= 10;
                    ekip.kitlikdegeri += ekip.kitliklist[i];
                }
            }
            int indis = 0; double enbuyuk = _ekipList[0].kitlikdegeri;
            for (int i = 1; i < _ekipList.Count; i++)//döngüden çıktığında en bol olan ekip bulunur ve atama önce ona yapılı, bu sayede kıtlık olan ekibin atanması sona bırakılır
                if (_ekipList[i].kitlikdegeri>enbuyuk)
                {
                    indis = i;//seçilecek olan indisi tutar
                    enbuyuk = _ekipList[0].kitlikdegeri;
                }
            return _ekipList[indis];
        }

        public void AtamalariYap_onceki()
        {
            //ekip seç ekipe ait rotayı doldur sonra bu ekibi sil, diğer ekibi seç
           
            bool atamalartamam = false;
            while (true)
            {
                Ekip secilenekip = EkipSec();
                Rota myrota = new Rota();
                myrota.ekip = secilenekip;
                int periyod = myrota.ekip.sabahMesai.t1;
                int nokta = 0;//rota başlangıç noktası her zaman 0. nokta, 0 sağlık merkezi
                int i = 0; //hasta listesindeki ilk hasta Id si

                bool ziyaretedilebilirvarmi = false;
                int fark = Islemler.M;
                int t1 = 0;//zç1
                int t2 = 0;//zç2

                //aşağıdaki for tüm hasta listesini dolaşarak atama için en uygun hastayı bulur 
                // atanabilecek hasta bulursa ziyaretedilebilirvarmı true olur
                for (int j=1;j<_hastaList.Count;j++)
                {                   
                    int m1 = periyod + Islemler.UzaklikGetir(_hastaList[i].hastaID, _hastaList[j].hastaID).dakika + _hastaList[j].bakimSuresi;
                    int m2 = m1 + Islemler.UzaklikGetir(_hastaList[j].hastaID, _hastaList[0].hastaID).dakika;                  

                    if (m1 > _hastaList[j].timeWindow.t1) //ziyaret edilecek hastaya ulaşıldığında ziyaret başlayacak
                        t1 = m1; //eğer hastaya ulaşma zamanı t1(ZÇ1) den daha sonra ise
                    else
                        t1 = _hastaList[j].timeWindow.t1; //eğer hastaya ulaşma zamanı t1 den önce ise ziyaret t1 de başlayacak

                     t2 = t1 + _hastaList[j].bakimSuresi; //ziyaret bitişi gereken t2 zamanı, t2=t1+bakim
                 
                    bool skillyeterli = (myrota.ekip.skill & _hastaList[j].skill) != _hastaList[j].skill;

                    bool mesaisarkmasiyok = !myrota.MesaiSarkmasiVar(t1, t2); //rotada bu t1, t2 atamasında mesai sarkması varmı bakılıyor, dönen değer değillendi

                    if (skillyeterli && mesaisarkmasiyok && m1 < _hastaList[j].timeWindow.t2 && m2 < myrota.ekip.ogleMesai.t2)
                    {
                        ziyaretedilebilirvarmi = true;
                        int yenifark = _hastaList[j].timeWindow.t1 - (periyod+ Islemler.UzaklikGetir(i, _hastaList[j].hastaID).dakika);
                        if (yenifark < fark)
                        {
                            fark = yenifark;
                            nokta = j;//nokta dizinin indisi bu indissteki değeri rotaya eklemek gerek
                        }                           
                    }                                                     
                }

                if (ziyaretedilebilirvarmi) //rotaya yeni bir hasta daha eklenebilir mi?
                {                                               
                    //****burada nokta değerine ait hastayı rotaya ekle****
                    myrota.YeniNoktaEkleAraya(myrota.ziyaretSirasi.Count-1, _hastaList[nokta],t1,t2);

                    if (_hastaList[i].timeWindow.t1 > periyod + Islemler.UzaklikGetir(_hastaList[i].hastaID, _hastaList[nokta].hastaID).dakika)
                        periyod = _hastaList[i].timeWindow.t1;
                    else
                        periyod = periyod + Islemler.UzaklikGetir(_hastaList[i].hastaID, _hastaList[nokta].hastaID).dakika;

                    _hastaList.RemoveAt(nokta); //hasta rotaya eklendiği için listeden çıkarıldı

                    i = nokta;//arama başlangıç için yeni i değeri nokta oldu
                }
                else 
                {
                    //ziyaret edilebilecek hasta bulunamadı demektir
                    //bu durumda son rota için artık yeni hasta eklenemez anlamına gelir
                    //bu rota rotalistesine eklenecek
                    //seçilen ekip ekip listten silinecek
                    //eğer hala listede hasta varsa count>1 (0 nolu hasta sağlık merkezi o silinmiyor), while dönmeye devam edecek

                    _rotaList.Add(myrota); //myrota için artık atanacak yeni hasta yok demektir
                    _ekipList.Remove(secilenekip);//myrota için belirlenen ekip için atanacak yeni hasta yok tekrar seçilmemesi çin listeden silindi

                    //atanacak hasta yoksa count=1 atama tamamlanmış demektir break ile çıkılır
                    //atanacak hasta kaldıysa fakat ekiplist te ekip kalmadıyda atamalar başarılamamış demektir break ile çıkılır
                    //atanacak hasta varsa fakat  ekiplistte ekip varsa yeni rota başlatılır, üstteki 2 durum değilse bu duurmdur
                    if(_hastaList.Count<=1)
                    {
                        atamalartamam = true;
                        break;//while kırıldı
                    }
                    if(_hastaList.Count>1 &&_ekipList.Count==0)
                    {
                        atamalartamam = false;
                        break;//while kırıldı atamalar tamamlanamadı
                    }

                    //akış buraya gelmiş ise while dönmeye devam eder
                }

            }

            if (atamalartamam)
                MessageBox.Show("atamalar tamam");
            else
                MessageBox.Show("atamalar yapılamadı");
        }
        public void AtamalariYap_onceki2()
        {
            //daha önce yazılmıştı artık kullanılmıyor
            //ekip seç ekipe ait rotayı doldur sonra bu ekibi sil, diğer ekibi seç


            while (_ekipList.Count > 0 && _hastaList.Count > 1)
            {
                Ekip secilenekip = EkipSec();//en uygun ekibi seçer
                Rota myrota = new Rota();//yeni bir rota başlattı
                myrota.ekip = secilenekip;//rotanın ekibi atandı
                int periyod = myrota.ekip.sabahMesai.t1;//rota için başlangıç periyodu, ekibin sabah mesai başlangıcı
                int nokta = 0;//rota başlangıç noktası her zaman 0. nokta, 0 sağlık merkezi
                int i = 0; //hasta listesindeki ilk hasta Id si
                bool ziyaretedilebilirvarmi = true;

                while (ziyaretedilebilirvarmi) //seçilen ekip için uygun noktaları bularak gredy olarak atamalar yapacak
                {
                    ziyaretedilebilirvarmi = false;//ziyaret edilebilir nokta bulamaz ise false kalsın diye güncellendi
                    int fark = Islemler.M;//fark değeri başlangıcta büyük bir değer alındı
                    int t1 = 0;//atama yapılacak zç1 değeri
                    int t2 = 0;//zç2

                    //aşağıdaki for tüm hasta listesini dolaşarak atama için en uygun hastayı bulur 
                    // atanabilecek hasta bulursa ziyaretedilebilirvarmı true olur
                    for (int j = 1; j < _hastaList.Count; j++)//hasta listesi-1 kadar döngü ile mevcut hastalar taranıyor
                    {
                        if (i == j) continue;//en son hasta 
                        int m1 = periyod + Islemler.UzaklikGetir(_hastaList[i].hastaID, _hastaList[j].hastaID).dakika + _hastaList[j].bakimSuresi;
                        int m2 = m1 + Islemler.UzaklikGetir(_hastaList[j].hastaID, _hastaList[0].hastaID).dakika; //son noktadan sonra sağlık merkezine gidilebiliyormu

                        bool skillyeterli = (myrota.ekip.skill & _hastaList[j].skill) == _hastaList[j].skill;//skill ziayaret için yeteliyse true, değilse false

                        bool mesaisarkmasiyok = !myrota.MesaiSarkmasiVar(t1, t2); //rotada bu t1, t2 atamasında mesai sarkması varmı bakılıyor, dönen değer değillendi

                        if (skillyeterli && mesaisarkmasiyok && m1 < _hastaList[j].timeWindow.t2 && m2 < myrota.ekip.ogleMesai.t2)
                        {
                            //koşul sağlanmış ise ziyaret edilebilece bir hasta noktası var demektir. bu duurmda farklara bakılacak
                            ziyaretedilebilirvarmi = true; //ziyaret edilebilir bir nokta bulunduğundan true, hiç nokta bulamaz ise false olarak kalmaya devam edecek
                            int yenifark = _hastaList[j].timeWindow.t1 - (periyod + Islemler.UzaklikGetir(_hastaList[i].hastaID, _hastaList[j].hastaID).dakika);
                            if (yenifark < fark)
                            {
                                fark = yenifark;
                                nokta = j;//en düşük farka sahip olan noktayı bulmak için fark değei küçüldüğünde nokta güncelleniyor

                                if (m1 > _hastaList[j].timeWindow.t1) //ziyaret edilecek hastaya ulaşıldığında ziyaret başlayacak
                                    t1 = m1; //eğer hastaya ulaşma zamanı ile hastanın istediği zç1 kıyaslayarak büyük olanını seçiyor
                                else
                                    t1 = _hastaList[j].timeWindow.t1; //eğer hastaya ulaşma zamanı t1 den önce ise ziyaret t1 de başlayacak, bu durumda ekip beklemiş sayılır

                                t2 = t1 + _hastaList[j].bakimSuresi; //ziyaret bitişi için gereken t2 zamanı, t2=t1+bakim
                            }
                        }
                    }

                    if (ziyaretedilebilirvarmi) //rotaya yeni bir hasta daha eklenebilir mi?
                    {
                        //****burada nokta değerine ait hastayı rotaya ekle****
                        myrota.YeniNoktaEkleAraya(myrota.ziyaretSirasi.Count - 1, _hastaList[nokta], t1, t2);//fark değiri en düşük olan hasta rotaya eklendi

                        periyod = t2;//t2 de yeni yapılan ziyaretin bitişi var

                        _hastaList.RemoveAt(nokta); //hasta rotaya eklendiği için ziyaret listesinden çıkarıldı

                        i = nokta;//arama başlangıç için yeni i değeri nokta oldu
                    }
                    else
                    {
                        //ziyaret edilebilecek hasta bulunamadı demektir
                        //bu durumda son rota için artık yeni hasta eklenemez anlamına gelir
                        //bu rota rotalistesine eklenecek
                        //seçilen ekip ekip listten silinecek                       
                        _rotaList.Add(myrota); //myrota için artık atanacak yeni hasta yok demektir
                        _ekipList.Remove(secilenekip);//myrota için belirlenen ekip için atanacak yeni hasta yok tekrar seçilmemesi çin listeden silindi                     
                    }
                }

            }

            if (_hastaList.Count <= 1)
                MessageBox.Show("atamalar tamam");
            else
                MessageBox.Show("atamalar yapılamadı");


        }

        public void AtamalariYap()
        {
            //ekip seç ekipe ait rotayı doldur sonra bu ekibi sil, diğer ekibi seç

           
            while(_ekipList.Count>0&&_hastaList.Count>1)//while ya hasta listesi biterse yada ataacak ekip kalmaz ise boşalır
            {
                Ekip secilenekip = EkipSec();//en uygun ekibi seçer
                Rota myrota = new Rota();//yeni bir rota başlattı
                myrota.ekip = secilenekip;//rotanın ekibi atandı
                if (myrota.ekip == null) break;//programda runtime de bir hata olup ekip seçilemez ise kırılmasın diy eeklendi: algoritmaya bir etkisi yok

                //ekip rotaya atandığı için rota başlangıç ve bitiş periyodları ekibe ait zaman periyodları olarak ayarlanıyor
                myrota.ziyaretSirasi[0].atandigiTimeWindow.t1 = secilenekip.sabahMesai.t1;//başlangıç noktası
                myrota.ziyaretSirasi[0].atandigiTimeWindow.t2 = secilenekip.sabahMesai.t1;
                myrota.ziyaretSirasi[myrota.ziyaretSirasi.Count - 1].atandigiTimeWindow.t1 = secilenekip.ogleMesai.t2;//bitiş noktası
                myrota.ziyaretSirasi[myrota.ziyaretSirasi.Count - 1].atandigiTimeWindow.t2 = secilenekip.ogleMesai.t2;

               
                int periyod = myrota.ekip.sabahMesai.t1;//rota için başlangıç periyodu, ekibin sabah mesai başlangıcı
                int nokta = 0;//rota başlangıç noktası her zaman 0. nokta, 0 sağlık merkezi
               
                bool ziyaretedilebilirvarmi = true;
                int saglikmerkezi = 0;
                Hasta eskihasta = _hastaList[nokta];
                while (ziyaretedilebilirvarmi) //seçilen ekip için uygun noktaları bularak gredy olarak atamalar yapacak
                {                                 
                    ziyaretedilebilirvarmi = false;//ziyaret edilebilir nokta bulamaz ise false kalsın diye güncellendi
                   
                    Hasta kararverilenhasta=null;
                    int fark = Islemler.M;//fark değeri başlangıcta büyük bir değer alındı
                    int t1 = 0;//atama yapılacak zç1 değeri
                    int t2 = 0;//zç2

                    int atamaicint1 = 0;
                    int atamaicint2 = 0;
                   
                    //aşağıdaki for tüm hasta listesini dolaşarak atama için en uygun hastayı bulur 
                    // atanabilecek hasta bulursa ziyaretedilebilirvarmı true olur
                    for (int j = 1; j < _hastaList.Count; j++)//hasta listesi-1 kadar döngü ile mevcut hastalar taranıyor
                    {
                        
                        Hasta yenihasta = _hastaList[j];
                        if (eskihasta==yenihasta) continue;//aynı hasta için işlem yapmamalı

                        int eskihastayenihastauzaklik = Islemler.UzaklikGetir(eskihasta.hastaID, yenihasta.hastaID).dakika;
                        int yenihastasaglikmerkeziuzaklik = Islemler.UzaklikGetir(yenihasta.hastaID, saglikmerkezi).dakika;

                        int m1 = periyod + eskihastayenihastauzaklik + yenihasta.bakimSuresi;
                        int m2 = m1 + yenihastasaglikmerkeziuzaklik; //son noktadan sonra sağlık merkezine gidilebiliyormu

                        bool skillyeterli = (myrota.ekip.skill & yenihasta.skill) == yenihasta.skill;//skill ziayaret için yeteliyse true, değilse false

                        if (periyod + eskihastayenihastauzaklik > yenihasta.timeWindow.t1) //yeni hasta ziyaretinin başlaması gereken t1 değeri
                            t1 = periyod + eskihastayenihastauzaklik;
                        else
                            t1 = yenihasta.timeWindow.t1;
                        t2 = t1 + yenihasta.bakimSuresi;//yeni hasta t2 zamanı, hasta ziyaretinin bittiği time windows

                        bool mesaisarkmasiyok = !myrota.MesaiSarkmasiVar2(t1, t2); //rotada bu t1, t2 atamasında mesai sarkması varmı bakılıyor, dönen değer değillendi

                       

                        if (skillyeterli && mesaisarkmasiyok && m1 <= yenihasta.timeWindow.t2 && m2 <= myrota.ekip.ogleMesai.t2)
                        {
                            //koşul sağlanmış ise ziyaret edilebilece bir hasta noktası var demektir. bu duurmda farklara bakılacak
                            ziyaretedilebilirvarmi = true; //ziyaret edilebilir bir nokta bulunduğundan true, hiç nokta bulamaz ise false olarak kalmaya devam edecek
                            int yenifark = yenihasta.timeWindow.t1 - (periyod + eskihastayenihastauzaklik); //bu formül ekibin en az bekleme yapacağı hastayı seçer eğer negatif ise hasta ziyareti hasta zç1 başladıkatn sonra başlayacaktır

                            if (yenifark < fark)
                            {
                                fark = yenifark;
                                kararverilenhasta = yenihasta;
                                nokta = j;//en düşük farka sahip olan noktayı bulmak için fark değei küçüldüğünde nokta güncelleniyor                              

                                atamaicint1 = t1;
                                atamaicint2 = t2;
                            }
                        }
                    }

                    if (ziyaretedilebilirvarmi) //rotaya yeni bir hasta daha eklenebilir mi?
                    {
                        //****burada nokta değerine ait hastayı rotaya ekle****
                        myrota.YeniNoktaEkleAraya(myrota.ziyaretSirasi.Count - 1, kararverilenhasta, atamaicint1, atamaicint2);//fark değiri en düşük olan hasta rotaya eklendi

                        myrota.KaydirmaUygula();

                        periyod = atamaicint2;//t2 de yeni yapılan ziyaretin bitişi var
                        eskihasta = kararverilenhasta;
                        _hastaList.Remove(kararverilenhasta); //hasta rotaya eklendiği için ziyaret listesinden çıkarıldı

                     //   i = nokta;//arama başlangıç için yeni i değeri nokta oldu
                    }
                    else
                    {
                        //ziyaret edilebilecek hasta bulunamadı demektir
                        //bu durumda son rota için artık yeni hasta eklenemez anlamına gelir
                        //bu rota rotalistesine eklenecek
                        //seçilen ekip ekip listten silinecek    
                        
                        _rotaList.Add(myrota); //myrota için artık atanacak yeni hasta yok demektir
                        _ekipList.Remove(secilenekip);//myrota için belirlenen ekip için atanacak yeni hasta yok tekrar seçilmemesi çin listeden silindi                     
                    }
                }

                myrota.ToplamUzaklikHesapla();

            }


        }



    }
}
