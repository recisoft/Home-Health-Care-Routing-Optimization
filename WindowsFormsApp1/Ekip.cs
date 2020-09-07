using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    class Ekip
    {
        public int ekipID;
        public TimeWindow sabahMesai;
        public TimeWindow ogleMesai;
        public int skill;
        public List<double> kitliklist = new List<double>();// bu liste Gready algoritmada ekip skillerinin kıtlığı için kullanılacak, en kıt ekib en son atanacak
        public double kitlikdegeri;//ekibin kıtlı değeri bulunacak, Gready algoritmada kullanılyor GA da ise hasta nesnesindeki değer kullanılıyor
        private int _ekipCeza;
        public int ekipCeza
        {
            get { return _ekipCeza; }
        }
        public Ekip()
        {

        }
    
        public Ekip(int ekipID, TimeWindow sabahMesai,TimeWindow ogleMesai, int skill)
        {
            this.ekipID = ekipID;
            this.sabahMesai = sabahMesai;
            this.ogleMesai = ogleMesai;
            this.skill = skill;
        }

        private bool Between(int t, TimeWindow ogleArasi)
        {
            //belirtilen t zamanının öğle arasında olup olmadığını bulur
            if (t >= ogleArasi.t1 && t <= ogleArasi.t2)
                return true;
            return false;
        }
        private bool Between(int t, int t1, int t2)
        {
            //belirtilen t zamanının öğle arasında olup olmadığını bulur
            if (t>t1 && t <=t2)
                return true;
            return false;
        }
        public void EkipCezaGuncelle(Rota rota)
        {  
            /*
             ************ Yazması çok zahmetli bir yordam, değişiklik yapılacaksa dikkatli incele
             * eğer yavaş çalışmaya sebep olursa döngü yapısındki ifelse yapılarını elden geçir
             * Ekibe atanan ziyaret noktalarını tarayarak hatalı ve
             * istenmeyen noktaları cezalandırır, 
             * fazla mesasi
             * istenmeyen periyod
             * öğle arası ihlaline bakar
             */ 
            if (rota.ziyaretSirasi.Count <= 2)//rotada hasya yok demektir
                return;
      
            int fazlamesai = 0;
            int oglearasiihlali = 0;
            Gen ilkZiyaret = rota.ziyaretSirasi[1];//ilk nokta
            Gen sonZiyaret= rota.ziyaretSirasi[rota.ziyaretSirasi.Count-2];//sonZiyaret nokta
            bool sabahmesaisivar=false;
            if (this.sabahMesai.t2 - sabahMesai.t1 > 0)
                sabahmesaisivar = true;
            bool oglemesaisivar = false;
            if (this.ogleMesai.t2 - ogleMesai.t1 > 0)
                oglemesaisivar = true;       
            /* 
             * ilk hasta ziyaretinin mesai başlamdan önce planlanması durumunu kontrol ediyor
             * aşağıdaki ilk if eğer ilk hasta sabah mesaisinde ise,
             * else if ise eğer ilk hasta öğle mesaisinde ise çalışır. 
             * else if olaki ekibin hiç sabah mesaisi yoksa gideceği ilk hastanın öğle mesaisinden sonra olması gerekir
             * else if in çalışması için hastanın sabah mesaisini pas geçmesi gerek 
            */
            if (sabahmesaisivar&&(this.sabahMesai.t1 + Islemler.UzaklikGetir(0, ilkZiyaret.hasta.hastaID).dakika>ilkZiyaret.atandigiTimeWindow.t1))
                fazlamesai+= this.sabahMesai.t1 + Islemler.UzaklikGetir(0, ilkZiyaret.hasta.hastaID).dakika - ilkZiyaret.atandigiTimeWindow.t1;
            else if(oglemesaisivar && (this.ogleMesai.t1 + Islemler.UzaklikGetir(0, ilkZiyaret.hasta.hastaID).dakika > ilkZiyaret.atandigiTimeWindow.t1))
                fazlamesai += this.ogleMesai.t1 + Islemler.UzaklikGetir(0, ilkZiyaret.hasta.hastaID).dakika - ilkZiyaret.atandigiTimeWindow.t1;

            /*
             * son hasta ziyaretinin mesai bittikten sonra planlanması durumu
             * aşağığıdaki if, if else yukarıdaki yapıya benzer burada son ziyaret için mesai bitişi kontrol edildi
             * öğle mesaisi yoksa son hastanın sabah son mesai saatini aşmaması gerekir
             */
            if (oglemesaisivar && (sonZiyaret.atandigiTimeWindow.t2 + Islemler.UzaklikGetir(sonZiyaret.hasta.hastaID, 0).dakika > this.ogleMesai.t2))
               fazlamesai += sonZiyaret.atandigiTimeWindow.t2 + Islemler.UzaklikGetir(sonZiyaret.hasta.hastaID, 0).dakika - this.ogleMesai.t2;
            else if (sabahmesaisivar && (sonZiyaret.atandigiTimeWindow.t2 + Islemler.UzaklikGetir(sonZiyaret.hasta.hastaID, 0).dakika > this.sabahMesai.t2))
                fazlamesai += sonZiyaret.atandigiTimeWindow.t2 + Islemler.UzaklikGetir(sonZiyaret.hasta.hastaID, 0).dakika - this.sabahMesai.t2;
            /*
             * ziyaret öğle arasına sarkmış ise
             * öğle arasına sarkmada 4 durum var, her durum ayrı bir if,ile değerlendirilecek
             * 1) ziyaret.t1<oglearası.t1 AND  ziyaret.t2> between oglearası
             * 2) ziyaret.t1 between öğlearası AND ziyaret.t2>Öğlearası.t2
             * 3) ziyaret.t1 between öglearası AND ziyaret.t2 between öglearası
             * 4) ziyaret.t1<oglerası.t1 AND ziyaret.t2>oglearası.t2
             */
            int oa_t1 = this.sabahMesai.t2; //ekibin öğle arası başlangıcı
            int oa_t2 = this.ogleMesai.t1; //ekibin öğle arası bitişi
            for (int i = 1; i < rota.ziyaretSirasi.Count-1; i++) //ziyaret sırasındaki hastalara bakar ilk ve son hasta ya bakmaz sağlık merkezi
            {
                Gen g1 = rota.ziyaretSirasi[i];//i. hasta
                Gen g2 = rota.ziyaretSirasi[i + 1];
                int zi_t1 = rota.ziyaretSirasi[i].atandigiTimeWindow.t1;//ziyaret.t1
                int zi_t2 = rota.ziyaretSirasi[i].atandigiTimeWindow.t2;
                int dakika= Islemler.UzaklikGetir(rota.ziyaretSirasi[i], rota.ziyaretSirasi[i + 1]).dakika; //i, i+1 arasındaki uzaklığın dakika cinsinden değeri

                if (zi_t1 >= oa_t1 && zi_t1 <= oa_t2 && zi_t2 + dakika > oa_t2)  //2.durum
                {
                    oglearasiihlali += oa_t2 - zi_t1;
                    break; //ziyaret öğle arasını geçtiğine göre dögünn devamı na gerek yok
                }
                else if (zi_t1 <= oa_t1 && zi_t2 >= oa_t2) //4.durum
                {
                    oglearasiihlali += oa_t2 - oa_t1;
                    break; //ziyaret öğle arasını geçtipine göre döngünün devamına gerek yok
                }                    
                else if (zi_t1 <= oa_t1 && zi_t2 + dakika > oa_t1) //1.durum  (zi_t1 <= oa_t1 && zi_t2 + dakika > oa_t1 && zi_t2 + dakika <= oa_t2)
                    oglearasiihlali += zi_t2 + dakika - oa_t1;         
                   
                else if (zi_t1 >= oa_t1 && zi_t1 <= oa_t2 && zi_t2 + dakika >= oa_t1 && zi_t2 + dakika <= oa_t2)  //3.durum
                    oglearasiihlali += zi_t2 + dakika - zi_t1;                
              
            }
            _ekipCeza = fazlamesai*Islemler.CezaPuanlari[Cezalar.ekipFazlaMesaiPeriyod] + oglearasiihlali* Islemler.CezaPuanlari[Cezalar.oglearasiihlali];
        }
    }
}
