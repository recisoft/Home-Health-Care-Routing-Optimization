using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    class Rota
    {
        private int _rotaceza;
        private Uzaklik2 _toplamUzaklik;
        public Ekip ekip;
        public List<Gen> ziyaretSirasi= new List<Gen>();//rotaya atanan 
        public int rotaceza
        {
            get { return _rotaceza; }
        }
        public Uzaklik2 toplamUzaklik
        {
            get { return _toplamUzaklik; }
        }
        public Rota()
        {
            //rota il başta 0. noktadan başlar ve 0. noktada biter
            TimeWindow tw = new TimeWindow();
            tw.t1 = Islemler.mesaiBaslangic;tw.t2 = Islemler.mesaiBaslangic; //mesai başlangıç
            ziyaretSirasi.Add(new Gen(Islemler.hastaListGun[0],tw));//rota başla
            tw.t1 = Islemler.mesaiBitis; tw.t2 = Islemler.mesaiBitis;//mesai bitiş
            ziyaretSirasi.Add(new Gen(Islemler.hastaListGun[0],tw));//rota bit
        }
        public Rota(Rota modelRota) //kopya çıkartmak için kullanılır
        {
            //model rotanın bir kopyasını çıkartır
            this._rotaceza = modelRota.rotaceza;
            this.ekip = modelRota.ekip;
            foreach (Gen kopyaGen in modelRota.ziyaretSirasi)
                this.ziyaretSirasi.Add(new Gen(kopyaGen));
        }
        private void YeniNoktaEkleAraya(int index, Hasta hasta, TimeWindow tw)
        {
            ziyaretSirasi.Insert(index, new Gen(hasta,tw));
        }
        private void YeniNoktaEkleAraya(int index, Hasta hasta, int t1, int t2)
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

        private bool MesaiSarkmasiVar_yeni(int t1, int t2)
        {
         //   return false;
            //true değer dönerse  ziyaret sabah mesaide başlayıp öğle mesaide bitiyor demektir
            //true dönen değerler için atama yapılmayacaktır
            if (t1 >= ekip.sabahMesai.t1 && t2 <= ekip.sabahMesai.t2)
                return false;
            if (t1 >= ekip.ogleMesai.t1 && t2 <= ekip.ogleMesai.t2)
                return false;
            return true;
        }
        private bool MesaiSarkmasiVar(int t1, int t2)
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

            if (ziyaretSirasi.Count <= 2) return;
            _rotaceza = 0;
          
            //rota için yapılan toplam mesafeyi bulan kodlar
            ToplamUzaklikHesapla();//rota için yapılan toplam mesafeyi, km ve dak. cinsinden bulur
            //------------------------------
            _rotaceza += (_toplamUzaklik.dakika /(ziyaretSirasi.Count-2))* Islemler.CezaPuanlari[Cezalar.dakikaToplaminiCezala];
            _rotaceza += (int)(_toplamUzaklik.metre/ (1*(ziyaretSirasi.Count - 2))) * Islemler.CezaPuanlari[Cezalar.metreToplaminiCezala];
           
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
        public void ToplamUzaklikHesapla()
        {
            //rota ile yapılan toplam uzaklık değeri metre ve dakika cinsinden ikisni de hesaplar
            Uzaklik2 tUzaklik = new Uzaklik2();
            for (int i = 0; i < ziyaretSirasi.Count - 1; i++)
            {
                Gen g1 = ziyaretSirasi[i];
                Gen g2 = ziyaretSirasi[i + 1];              
                Uzaklik2 uzaklik = Islemler.distanceMatrix[new Tuple<int, int>(g1.hasta.hastaID, g2.hasta.hastaID)];
                tUzaklik.dakika += uzaklik.dakika;
                tUzaklik.metre += uzaklik.metre;
            }
            _toplamUzaklik = tUzaklik;
        }        

    }
}
