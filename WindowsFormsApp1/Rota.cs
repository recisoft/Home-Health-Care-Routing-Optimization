using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    class Rota
    {
        public class Kayma{
            public int miktar;
            public double kaymaCeza;
        public Kayma()
            {
                this.miktar = 0;
                this.kaymaCeza = 0;
            }
        public void Ekle(int miktar, double oncelik)
            {
                this.kaymaCeza = (this.miktar * this.kaymaCeza + miktar * Islemler.CezaPuanlari[Cezalar.hastaIstenmeyenPeriyod] * oncelik) / (this.miktar + miktar);
                this.miktar += miktar;
            }
        }
        private int _rotaceza;
        private Uzaklik2 _toplamUzaklik;    
        public Ekip ekip;      
        public List<Gen> ziyaretSirasi= new List<Gen>();//rotaya atanan 
        public int rotaceza
        {
            get { return _rotaceza; }
            set { _rotaceza = value; }
        }
        public Uzaklik2 toplamUzaklik
        {
            get { return _toplamUzaklik; }
            set { _toplamUzaklik = value; }
        }
        public Kayma solaKayma = new Kayma();
        public Kayma sagaKayma = new Kayma();      
        
   
        public Rota()
        {
            //rota il başta 0. noktadan başlar ve 0. noktada biter
            TimeWindow tw = new TimeWindow();
            tw.t1 = Islemler.mesaiBaslangic;tw.t2 = Islemler.mesaiBaslangic; //sabahmesai başlangıç
            ziyaretSirasi.Add(new Gen(Islemler.hastaListGun[0],tw));//rota başla

            

            tw.t1 = Islemler.mesaiBitis; tw.t2 = Islemler.mesaiBitis;//öğlemesai bitiş
            ziyaretSirasi.Add(new Gen(Islemler.hastaListGun[0], tw));//rota bit
        }
        public Rota(Rota modelRota) //kopya çıkartmak için kullanılır
        {
            //model rotanın bir kopyasını çıkartır
            this._rotaceza = modelRota.rotaceza;
            this.ekip = modelRota.ekip;
            foreach (Gen kopyaGen in modelRota.ziyaretSirasi)
                this.ziyaretSirasi.Add(new Gen(kopyaGen));
        }
        public void YeniNoktaEkleAraya(int index, Hasta hasta, TimeWindow tw)
        {
            ziyaretSirasi.Insert(index, new Gen(hasta,tw));
        }
        public void YeniNoktaEkleAraya(int index, Hasta hasta, int t1, int t2)
        {
            ziyaretSirasi.Insert(index, new Gen(hasta, t1,t2));
        }
        private bool MesaiSarkmasiVar_eski(TimeWindow tw)
        {
            //true değer dönerse  ziyaret sabah mesaide başlayıp öğle mesaide bitiyor demektir
            //true dönen değerler için atama yapılmayacaktır
            if (tw.t1 <= ekip.sabahMesai.t2 && tw.t2 >= ekip.ogleMesai.t1)
                return true;
            return false;            
        }
        public bool MesaiSarkmasiVar2(int t1, int t2)
        {
            //mesai sarkmasını tam kontrol ediyor hiç bir şekilde öğle arasına girmeye izin vermez
           //return false; // Greedy test için  kapatılabilir

            //true değer dönerse  ziyaret sabah mesaide başlayıp öğle mesaide bitiyor demektir
            //true dönen değerler için atama yapılmayacaktır
            if (t1 >= ekip.sabahMesai.t1 && t2 <= ekip.sabahMesai.t2)
                return false;
            if (t1 >= ekip.ogleMesai.t1 && t2 <= ekip.ogleMesai.t2)
                return false;
            return true;
        }
        public bool MesaiSarkmasiVar2(Gen gen)
        {
            //mesai sarkmasını tam kontrol ediyor hiç bir şekilde öğle arasına girmeye izin vermez
            //return false; // Greedy test için  kapatılabilir

            //true değer dönerse  ziyaret sabah mesaide başlayıp öğle mesaide bitiyor demektir
            //true dönen değerler için atama yapılmayacaktır
            if (gen.atandigiTimeWindow.t1 >= ekip.sabahMesai.t1 && gen.atandigiTimeWindow.t2 <= ekip.sabahMesai.t2)
                return false;
            if (gen.atandigiTimeWindow.t1 >= ekip.ogleMesai.t1 && gen.atandigiTimeWindow.t2 <= ekip.ogleMesai.t2)
                return false;
            return true;
        }
        public bool MesaiSarkmasiVar(int t1, int t2)
        {
           // return false;
            //true değer dönerse  ziyaret sabah mesaide başlayıp öğle mesaide bitiyor demektir
            //true dönen değerler için atama yapılmayacaktır
            if (t1 <= ekip.sabahMesai.t2 && t2 >= ekip.ogleMesai.t1)
                return true;
            return false;
        }
        public void RotaCezaGuncelle()
        {
            //kromozomun fitness değerini hesaplamak için kullanılacaktır cezalandırma yöntemi olduğundan ismi CezaGuncelle
            //Rotaya atanmış olan toplam mesafeye ait km ve dakika değerlerini dikkate alır
            //Rotaya atamış olan ekibin istemediği bir zamana periyoduna atama varsa cezalandırır
            //rotaya atanmış olan hastaların istemediği bir zaman periyodu varsa cezalandırır
            //bu cezalarda kullanılmak isteyen olursa Islemler sınıfından ilgili ceza katsayısı değiştirilebilir.
            _rotaceza = 0;
            if (ziyaretSirasi.Count <= 2) return;
            
          
            //rota için yapılan toplam mesafeyi bulan kodlar
            ToplamUzaklikHesapla();//rota için yapılan toplam mesafeyi, km ve dak. cinsinden bulur
            //------------------------------
            //rotada ziyaret edilen nokta sayısına göre oranlanırsa
            //_rotaceza += (_toplamUzaklik.dakika /(ziyaretSirasi.Count-1))* Islemler.CezaPuanlari[Cezalar.dakikaToplaminiCezala];
            //_rotaceza += (int)(_toplamUzaklik.metre/ (1*(ziyaretSirasi.Count - 1))) * Islemler.CezaPuanlari[Cezalar.metreToplaminiCezala];

            //rotada kaç hastaya gidildiği önemsiz se
            _rotaceza += (_toplamUzaklik.dakika) * Islemler.CezaPuanlari[Cezalar.dakikaToplaminiCezala];
            _rotaceza += (int)(_toplamUzaklik.metre) * Islemler.CezaPuanlari[Cezalar.metreToplaminiCezala];

            //rotaya atanmış olan ekibe ait cezayı çeken kodlar
            this.ekip.EkipCezaGuncelle(this);//rotaya atanmış olan ekibe ait ceza varsa o değeri çeker
           //------------------------------
            _rotaceza += this.ekip.ekipCeza; //önce ekibe ait ceza değerleri güncellendi sonra değer okundu

            //rotadaki her bir gen için cezayı genden çeken kodlar
            for (int i=1;i<ziyaretSirasi.Count-1;i++) //
            {
                ziyaretSirasi[i].GenCezaHesapla();
                _rotaceza += ziyaretSirasi[i].genCeza;//gene ait bir ceza varsa bu değeri rotanın cezasına ekler
            }
            //--------------------------------------------
        }
        public bool _çalışanRotayaYeniHastaekle(Hasta yenihasta)
        {
            //********çalışan bir örnek, diğer kodda değişiklik yapılınca orjinal kod kalsın diye yazdım
            //rotada uygun bir konum bulursa atama yapacak, true dönecek
            //atama yapamaz ise false dönecek
            //atama yaparken hasta nın atanabileceği zç içinde uygun yere atayacak
            //atanmış olan diğer hastaların zaman çerçevesinde oynama yapmayacak

            if ((ekip.skill & yenihasta.skill) != yenihasta.skill) return false; //skill yetersiz ise atama yapma

            for (int i = 0; i < ziyaretSirasi.Count - 1; i++) //daha önce atanan noktaları bozup bozmadığına bakıyor
            {
                Gen g1 = ziyaretSirasi[i];
                Gen g2 = ziyaretSirasi[i + 1];
                //distanceMatrix.Add(new Tuple<int, int>(h1, h2), uzaklik);
                //Uzaklik u1 = distancematr[new Tuple<int, int>(hastalist[0].hastaID, hastalist[1].hastaID)];

                //uzaklik1=g1-yenihasta; uzaklik2=yenihasta-g2
                Uzaklik2 uzaklik1 = Islemler.distanceMatrix[new Tuple<int, int>(g1.hasta.hastaID, yenihasta.hastaID)];
                Uzaklik2 uzaklik2 = Islemler.distanceMatrix[new Tuple<int, int>(yenihasta.hastaID, g2.hasta.hastaID)];
                int twbaslama = g1.atandigiTimeWindow.t2 + Convert.ToInt32(uzaklik1.dakika);
                int twbitis = twbaslama + yenihasta.bakimSuresi;

                if (twbitis > yenihasta.timeWindow.t2)
                    continue; //ziyaret yapılamaz t2 süresinde ziyaret bitmiyor

                if (twbaslama < yenihasta.timeWindow.t1) //hastanın bakım zamanı gelmemiş ise ziyaret hasta bakım zamanında yapılır
                {
                    twbaslama = yenihasta.timeWindow.t1;
                    twbitis = twbaslama + yenihasta.bakimSuresi;
                }
                int g2yevaris = twbitis + uzaklik2.dakika; //yeni noktadan g2 ye gidiş periyodu

                if (MesaiSarkmasiVar(twbaslama, twbitis))
                    continue;//ziyaretler sabah başlamış öğlen bitmiş ziyaret yapılamaz
                if (g2yevaris > g2.atandigiTimeWindow.t1)
                    continue;//yeni atanacak hasta mevcut atamadaki ziyaret planını bozuyor

                //bu noktada ise; atama yapılabilir 

                YeniNoktaEkleAraya(i + 1, yenihasta, twbaslama, twbitis);
                return true;
            }
            return false; //üstteki döngüden kurtulmuş ise atama yapamıştır
        }

        //public bool AtamaYap_eski(Hasta yenihasta)
        //{
        //    //rotada uygun bir konum bulursa atama yapacak, true dönecek
        //    //atama yapamaz ise false dönecek
        //    //atama yaparken hasta nın atanabileceği zç içinde uygun yere atayacak
        //    //atanmış olan diğer hastaların zaman çerçevesinde oynama yapmayacak

        //    try
        //    {
        //        if ((ekip.skill & yenihasta.skill) != yenihasta.skill) return false; //skill yetersiz ise atama yapma
        //        for (int i = 0; i < ziyaretSirasi.Count - 1; i++)
        //        {
        //            Gen g1 = ziyaretSirasi[i];
        //            Gen g2 = ziyaretSirasi[i + 1];
        //            //distanceMatrix.Add(new Tuple<int, int>(h1, h2), uzaklik);
        //            //Uzaklik u1 = distancematr[new Tuple<int, int>(hastalist[0].hastaID, hastalist[1].hastaID)];

        //            //uzaklik1=g1-yenihasta; uzaklik2=yenihasta-g2
        //            Uzaklik2 uzaklik1 = Islemler.distanceMatrix[new Tuple<int, int>(g1.hasta.hastaID, yenihasta.hastaID)];
        //            Uzaklik2 uzaklik2 = Islemler.distanceMatrix[new Tuple<int, int>(yenihasta.hastaID, g2.hasta.hastaID)];
        //            int t1 = g1.atandigiTimeWindow.t2 + Convert.ToInt32(uzaklik1.dakika);
        //            int t2 = t1 + yenihasta.bakimSuresi;

        //            if (t2 > yenihasta.timeWindow.t2)
        //                continue; //ziyaret yapılamaz t2 süresinde ziyaret bitmiyor

        //            if (t1 < yenihasta.timeWindow.t1) //hastanın bakım zamanı gelmemiş ise ziyaret hasta bakım zamanında yapılır
        //            {
        //                t1 = yenihasta.timeWindow.t1;
        //                t2 = t1 + yenihasta.bakimSuresi;
        //            }
        //            int g2yevaris = t2 + uzaklik2.dakika; //yeni noktadan g2 ye gidiş periyodu

        //            if (MesaiSarkmasiVar(t1, t2))
        //                continue;//ziyaretler sabah başlamış öğlen bitmiş ziyaret yapılamaz
        //            if (g2yevaris > g2.atandigiTimeWindow.t1)
        //                continue;//yeni atanacak hasta mevcut atamadaki ziyaret planını bozuyor

        //            //bu noktada ise; atama yapılabilir 
        //            YeniNoktaEkleAraya(i + 1, yenihasta, t1, t2);
                    
                       
        //            return true;
        //        }
        //    }
            
        //    catch
        //    {
        //        return false;
        //    }
        //    return false; //üstteki döngüden kurtulmuş ise atama yapamamıştır
        //}
        public bool AtamaYap(Hasta yenihasta)
        {
            //rotada uygun bir konum bulursa atama yapacak, true dönecek
           
            //atama yapamaz ise false dönecek
            //atama yaparken hasta nın atanabileceği zç içinde uygun yere atayacak
            //atanmış olan diğer hastaların zaman çerçevesinde oynama yapmayacak

            try
            {
                if ((ekip.skill & yenihasta.skill) != yenihasta.skill) return false; //skill yetersiz ise atama yapma
                for (int i = 0; i < ziyaretSirasi.Count - 1; i++)
                {
                    Gen g1 = ziyaretSirasi[i];
                    Gen g2 = ziyaretSirasi[i + 1];
                    //distanceMatrix.Add(new Tuple<int, int>(h1, h2), uzaklik);
                    //Uzaklik u1 = distancematr[new Tuple<int, int>(hastalist[0].hastaID, hastalist[1].hastaID)];

                    //uzaklik1=g1-yenihasta; uzaklik2=yenihasta-g2
                    Uzaklik2 uzaklik1 = Islemler.distanceMatrix[new Tuple<int, int>(g1.hasta.hastaID, yenihasta.hastaID)];
                    Uzaklik2 uzaklik2 = Islemler.distanceMatrix[new Tuple<int, int>(yenihasta.hastaID, g2.hasta.hastaID)];
                    int t1 = g1.atandigiTimeWindow.t2 + Convert.ToInt32(uzaklik1.dakika);
                    int t2 = t1 + yenihasta.bakimSuresi;

                    if (t2 > yenihasta.timeWindow.t2)
                        continue; //ziyaret yapılamaz t2 süresinde ziyaret bitmiyor

                    if (t1 < yenihasta.timeWindow.t1) //hastanın bakım zamanı gelmemiş ise ziyaret hasta bakım zamanında yapılır
                    {
                        t1 = yenihasta.timeWindow.t1;
                        t2 = t1 + yenihasta.bakimSuresi;
                    }
                    int g2yevaris = t2 + uzaklik2.dakika; //yeni noktadan g2 ye gidiş periyodu

                    if (MesaiSarkmasiVar(t1, t2))
                        continue;//ziyaretler sabah başlamış öğlen bitmiş ziyaret yapılamaz
                    if (g2yevaris > g2.atandigiTimeWindow.t1)
                        continue;//yeni atanacak hasta mevcut atamadaki ziyaret planını bozuyor

                    //bu noktada ise; atama yapılabilir 
                    YeniNoktaEkleAraya(i + 1, yenihasta, t1, t2);

                
                    return true;
                }
            }

            catch
            {
                return false;
            }
            return false; //üstteki döngüden kurtulmuş ise atama yapamamıştır
        }
        private void SolaKaymaHesapla(int indis)
        {
            int miktar = 0;
            double kaymaCeza = 0;
            if (indis == 0 || indis == ziyaretSirasi.Count - 1) return;//sağlık merkezi ise çık
            for (int i = 1; i <= indis; i++) //1 den başlamasının sebebi 0 da sağlık merkezi var
            {
                Gen gen1 = ziyaretSirasi[i - 1]; //
                Gen gen2 = ziyaretSirasi[i];

                //gen2' nin sola en fazla kayabileceği dip nokta bulunuyor, bu noktadan daha sola kayması mümkün değil
                //gen1 t2+aradaki uzaklık değeri olarak hesaplandı fakat kayma buraya kadar yapılamayabilir
                //formüldeki -miktar işlemi eğer solundaki daha önce sola kaydırılmış ise bunu da dikkate alır.
                int dipperiyod = gen1.atandigiTimeWindow.t2 + Islemler.UzaklikGetir(gen1, gen2).dakika - miktar;//kayabileceği en erken konta

                int dipkayma = gen2.atandigiTimeWindow.t1 - dipperiyod; //kayma miktarıdır.
                int kayma = Islemler.HastaKayabilecegiPeriyod(gen2.hasta);//hastanın sola ne kadar esneyebileceğini bulur -miktar işlemi önceki kaymayı da dahil etmek için         

                //kayma için hem en dipi hemde mümkün olanı hesapla hangisi daha küçük ise ancak o kadar kayabilir
                if (dipkayma < kayma) kayma = dipkayma;//
                kaymaCeza = (miktar * kaymaCeza + kayma * Islemler.CezaPuanlari[Cezalar.hastaIstenmeyenPeriyod] * gen2.hasta.oncelik) / (miktar + kayma);
                miktar = kayma;//kayan miktar toplama eklendi
            }

            solaKayma.miktar = miktar;
            solaKayma.kaymaCeza = kaymaCeza;
        }
        private void SagaKaymaHesapla(int indis)
        {
            int miktar = 0;
            double kaymaCeza = 0;
            if (indis == 0 || indis == ziyaretSirasi.Count - 1) return;//sağlık merkezi ise çık
            for (int i = ziyaretSirasi.Count - 2; i>=indis; i--) //sondan 1. kayıttan başla indise kadar hesapla, son kayıt sağlık merkezi olduğundan o kayamaz
            {
                Gen gen1 = ziyaretSirasi[i]; //
                Gen gen2 = ziyaretSirasi[i+1];

                //gen1' in sağa kayabileceği en son nokta bulunur, bu noktadan daha sağa kayması mümkün değil
               //gen1 sağa doğru kaydığında gen1 den çıkış arı gen1-gen2 yolculuk süresinin g2.t1 den küçük eşit olması gerekir
               //formülde +miktar kısmı bir önceki adımda g2 sağa kaymış ise bu değerin dikkate alınması içindir
                int sonperiyod = gen2.atandigiTimeWindow.t1 - Islemler.UzaklikGetir(gen1, gen2).dakika + miktar;//kayabileceği en son nokta

                int maxkayma = sonperiyod-gen1.atandigiTimeWindow.t2; //sağa kayabileceği maksimum noktadır
                int kayma = Islemler.HastaKayabilecegiPeriyod(gen1.hasta);//hastanın sağa ne kadar esneyebileceğini bulur         

                //kayma için hem en dipi hemde mümkün olanı hesapla hangisi daha küçük ise ancak o kadar kayabilir
                if (maxkayma < kayma) kayma = maxkayma;//
                kaymaCeza = (miktar * kaymaCeza + kayma * Islemler.CezaPuanlari[Cezalar.hastaIstenmeyenPeriyod] * gen1.hasta.oncelik) / (miktar + kayma);
                miktar = kayma;//kayan miktar toplama eklendi
            }

            sagaKayma.miktar = miktar;
            sagaKayma.kaymaCeza = kaymaCeza;
        }
        private bool SolaKaydir(int konum, int kaymamiktari)
        {

            //Önceki atamaları sola doğru kaydırmaya çalışır eğer kaydırma işlemi başarılı olacaksa 
            //geriye true değer döner.
            //burada amaçi 0[0,0]-1[40,70]-0[540-540] gibi bir ziyaret için 1 nolu hastayı eğer mümkün ise sola doğru kaydırmaktır
            //hasta zaman çerçevesi bozulmuyor ise sola kaydırmaya izin verilebilir.
            //kodlar iteratif yazıldı, recursive yazılsa daha kolay yazılırdı ama iteratif olan kod recursive göre daha az bellek kullanır
            bool kayabilir = false;
            int nokta2 = konum;
            int nokta1 = konum;
   
            while (true) 
            {    
                //kayamaması için 2 ihtimal var
                //1 kayma işlemi salık merkezine kadar kalmıştır, sağlıkmerkezi bakımı 0 olduğundan kayma yapılamaz
                //2 kayması istenen geni kullanan hastanın timewindovu kayma işlemini karşılamıyordur.
                Gen gen2 = ziyaretSirasi[nokta2];
                if (gen2.hasta.hastaID==0)//0. konuma kadar gelmiş isek 0. konumasla kayamayacağından false olur
                {
                    kayabilir = false;
                    break;
                }
                nokta1 = nokta2 - 1;
                Gen gen1 = ziyaretSirasi[nokta1];
                if(gen2.atandigiTimeWindow.t1-kaymamiktari<gen2.hasta.timeWindow.t1-Islemler.HastaKayabilecegiPeriyod(gen2.hasta.oncelik))
                {
                    //yukarıdaki if kayamama durumunu kontrol eder
                    //hasta2.t1-kaymamiktarı< hasta2.t1+kayabilmehakkı,  önceliğe göre kaymaperio değerleri farklı
                    //bu koşul gerçekleş miş ise hasta  izin verilen time window dışına kayıyordur izin verme
                    kayabilir = false;
                    break;
                }
                //kayma yapılabilecek durumda ise kod burada

                //kyenikayma mikkari eğer 0 veya daha küçük ise soldaki diğer genlerin kaymasına gerek kalmadan kayma tamamlandı 
                int yenikaymamikari = kaymamiktari - (gen2.atandigiTimeWindow.t1 - (gen1.atandigiTimeWindow.t2 + Islemler.UzaklikGetir(gen1, gen2).dakika));
                gen2.solakaymaperiyod = kaymamiktari;
                if (yenikaymamikari <= 0)
                {
                    //ilgili gen kaydığı halde soldaki diğer genlerin kaymasına gerek kalmamış ise kayma işlemi  tamamlanmıştır                   
                    kayabilir = true;
                    break;
                }       
                kaymamiktari = yenikaymamikari; //algoritma tekrar başa döneceğinden yeni kayma miktarı güncellendi              
                nokta2 = nokta1;//noktalar bir sola doğru kaydırıldı
            }

            if (kayabilir)
                for (int i = nokta1; i < konum; i++) //kayması için işaretlenen genler sola doğru kaydırıldı
                {
                    ziyaretSirasi[i].atandigiTimeWindow.t1 -=ziyaretSirasi[i].solakaymaperiyod;
                    ziyaretSirasi[i].atandigiTimeWindow.t2 -= ziyaretSirasi[i].solakaymaperiyod;
                }
                   

            return kayabilir;
        }
        private bool SagaKaydir(int konum, int kaymamiktari)
        {

            //Önceki atamaları sola doğru kaydırmaya çalışır eğer kaydırma işlemi başarılı olacaksa 
            //geriye true değer döner.
            //burada amaçi 0[0,0]-1[40,70]-0[540-540] gibi bir ziyaret için 1 nolu hastayı eğer mümkün ise sola doğru kaydırmaktır
            //hasta zaman çerçevesi bozulmuyor ise sola kaydırmaya izin verilebilir.
            //kodlar iteratif yazıldı, recursive yazılsa daha kolay yazılırdı ama iteratif olan kod recursive göre daha az bellek kullanır
            bool kayabilir = false;
            int nokta1 = konum;
            int nokta2 = konum;

            while (true)
            {
                //kayamaması için 2 ihtimal var
                //1 kayma işlemi salık merkezine kadar kalmıştır, sağlıkmerkezi bakımı 0 olduğundan kayma yapılamaz
                //2 kayması istenen geni kullanan hastanın timewindovu kayma işlemini karşılamıyordur.
                Gen gen1 = ziyaretSirasi[nokta1];
                if (gen1.hasta.hastaID == 0)//0. konuma kadar gelmiş isek 0. konumasla kayamayacağından false olur
                {
                    kayabilir = false;
                    break;
                }
                nokta2 = nokta1+1;
                Gen gen2 = ziyaretSirasi[nokta2];

                if (gen1.atandigiTimeWindow.t2 + kaymamiktari > gen1.hasta.timeWindow.t2 + Islemler.HastaKayabilecegiPeriyod(gen1.hasta.oncelik))
                {
                    //yukarıdaki if kayamama durumunu kontrol eder                   
                    //bu koşul gerçekleş miş ise hasta  izin verilen time window dışına kayıyordur izin verme
                    kayabilir = false;
                    break;
                }
                //kayma yapılabilecek durumda ise kod burada

                //kyenikayma mikkari eğer 0 veya daha küçük ise soldaki diğer genlerin kaymasına gerek kalmadan kayma tamamlandı 
                int yenikaymamikari = kaymamiktari - (gen2.atandigiTimeWindow.t1 - (gen1.atandigiTimeWindow.t2 + Islemler.UzaklikGetir(gen1, gen2).dakika));
                gen1.sagakaymaperiyod = kaymamiktari;
                if (yenikaymamikari <= 0)
                {
                    //ilgili gen kaydığı halde soldaki diğer genlerin kaymasına gerek kalmamış ise kayma işlemi  tamamlanmıştır                   
                    kayabilir = true;
                    break;
                }

                kaymamiktari = yenikaymamikari; //algoritma tekrar başa döneceğinden yeni kayma miktarı güncellendi              
                nokta1 = nokta2;//noktalar bir sola doğru kaydırıldı
            }

            if (kayabilir)
                for (int i = konum; i < nokta2; i++) //kayması için işaretlenen genler sağa doğru kaydırıldı
                {
                    ziyaretSirasi[i].atandigiTimeWindow.t2 +=ziyaretSirasi[i].sagakaymaperiyod;
                    ziyaretSirasi[i].atandigiTimeWindow.t1 += ziyaretSirasi[i].sagakaymaperiyod;
                }
                   

            return kayabilir;
        }
        public bool KaydirmaUygula()
        {
            //List<Gen> kaymaGerekenGenler = new List<Gen>();
            //foreach (Gen gen in ziyaretSirasi)
            //    if (MesaiSarkmasiVar2(gen))
            //        kaymaGerekenGenler.Add(gen);

            for (int i=1;i<ziyaretSirasi.Count-1;i++)
            {
                Gen gen = ziyaretSirasi[i];
                if (gen.atandigiTimeWindow.t1<ekip.sabahMesai.t2 && gen.atandigiTimeWindow.t2>ekip.sabahMesai.t2)
                {
                    /*eğer bu şart sağlanmış ise, gen ekibe ait öğle arasına sarkmıştır 
                     * gen' in sola doğru kaydırılması gerekir. bu işlem  için
                     * 1 önce bu genin sola ne kadar kayabileceğine bakılır
                     * 2 ogle arası ihlali bulunur=gen.t2-sabah.t2
                     * 1. ve 2. değerlerde küçük olan bulunarak bu konumdan itibaren sola kaydırma yapılır
                     */
                    SolaKaymaHesapla(i);
                    int ihlal = gen.atandigiTimeWindow.t2 - ekip.sabahMesai.t2; //ogle arasi ihlali hesaplandi
                    if (solaKayma.miktar < ihlal) ihlal = solaKayma.miktar;//küçük olan dğer ihlale aktarıldı
                    SolaKaydir(i, ihlal);
                }
                else if(gen.atandigiTimeWindow.t2>ekip.ogleMesai.t1 &&gen.atandigiTimeWindow.t1<ekip.ogleMesai.t1)
                {
                    /*eğer bu şart sağlanmış ise, gen ekibe ait öğle arasına sarkmıştır 
                    * gen' in sağa doğru kaydırılması gerekir. bu işlem  için
                    * 1 önce bu genin sağa ne kadar kayabileceğine bakılır
                    * 2 ogle arası ihlali bulunur=ekip.ogle.t1-gen.t1
                    * 1. ve 2. değerlerde küçük olan bulunarak bu konumdan itibaren sola kaydırma yapılır
                     */
                    SagaKaymaHesapla(i);
                    int ihlal = ekip.ogleMesai.t1 - gen.atandigiTimeWindow.t1; //ogle arasi ihlali hesaplandi
                    if (sagaKayma.miktar < ihlal) ihlal = sagaKayma.miktar;//küçük olan dğer ihlale aktarıldı
                    SagaKaydir(i, ihlal);
                 
                }
            }
            return false;
        }
        public void ToplamUzaklikHesapla()
        {
            //rota ile yapılan toplam uzaklık değeri metre ve dakika cinsinden ikisni de hesaplar
            Uzaklik2 tUzaklik = new Uzaklik2();
            for (int i = 0; i < ziyaretSirasi.Count - 1; i++)
            {
                Gen g1 = ziyaretSirasi[i];
                Gen g2 = ziyaretSirasi[i + 1];
                if (g1.hasta.hastaID == g2.hasta.hastaID) continue;
                Uzaklik2 uzaklik = Islemler.UzaklikGetir(g1.hasta.hastaID, g2.hasta.hastaID);
                //  Uzaklik2 uzaklik = Islemler.distanceMatrix[new Tuple<int, int>(g1.hasta.hastaID, g2.hasta.hastaID)];
                tUzaklik.dakika += uzaklik.dakika;

                //bakım zamanı mecburi bir maliyet ve değiştirilemeyeceğinden zaman maliyetine eklenmedi, aşağıdaki kod ile kolayca eklenebilir
                //fakat GA nın azaltma imkanının olmadığı bir değerin fitness fonksiyonunu eklimememesi için bakım durumu eklenmedi sadce yolculuk zamanı eklendi
                //yolculuk zamanın eklenmesindeki ana sebep ise 1,2,3 gibi bir sıralama ile 1,3,2 gibi sıralamada tesr yön de trafik sıkışıklığı yada daha çok ışık gibi sebeplerden kaçınmak

                tUzaklik.dakika += g1.hasta.bakimSuresi; //Tek bir deney için eklendi daha sonr akapatılacak bakim süresini de ekle

                tUzaklik.metre += uzaklik.metre;
            }
            _toplamUzaklik = tUzaklik;
        }        

    }
}
