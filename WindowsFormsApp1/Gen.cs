using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    class Gen
    {
        private int _sirasi; //ziyaret sırası
        private int _genCeza; //gen başka bir konuma taşındığında güncellenecek
        public Hasta hasta; //konuma atanan hastadır
        public TimeWindow atandigiTimeWindow;//genin atanadığı t1,t2 değeri
        public int solakaymaperiyod;//solakayması gereken periyod değeridir
        public int sagakaymaperiyod;
        public int sirasi
        {
            get { return _sirasi; }
        }
        public int genCeza //gen yanlış yerde olursa ceza puanı
        {
            get { return _genCeza; }
        }      
        public Gen()
        {

        }
        public Gen(Hasta hasta)
        {
            this.hasta = hasta;
          //  GenCezaHesapla();
        }
        public Gen (Hasta hasta, TimeWindow atanacagiTw)
        {
            this.hasta = hasta;
            this.atandigiTimeWindow = atanacagiTw;
            solakaymaperiyod = 0;
            sagakaymaperiyod = 0;
         //   GenCezaHesapla();
        }
        public Gen(Hasta hasta, int t1, int t2)
        {
            this.hasta = hasta;
            this.atandigiTimeWindow.t1 = t1;
            this.atandigiTimeWindow.t2 = t2;
            solakaymaperiyod = 0;
            sagakaymaperiyod = 0;
            // GenCezaHesapla();
        }
        public Gen(Gen modelGen)
        {
            //model genin kopyasını çıkartır
            this.hasta = modelGen.hasta;
            this.atandigiTimeWindow.t1 = modelGen.atandigiTimeWindow.t1;
            this.atandigiTimeWindow.t2 = modelGen.atandigiTimeWindow.t2;
            solakaymaperiyod = 0;
            sagakaymaperiyod = 0;
        }
         public void GenCezaHesapla()
         {
            //hastalar istedikleri timewindow dan önce yada sonraya atanırlarsa o kadar dakika cezalandırılır
            //gen taşındığında otomatik çalıştırılması durumunu düşünmek gerek.
            
            _genCeza = 0;
            if (hasta.timeWindow.t1 > atandigiTimeWindow.t1)
                _genCeza += (hasta.timeWindow.t1- atandigiTimeWindow.t1);
            if (hasta.timeWindow.t2 < atandigiTimeWindow.t2)
                _genCeza += (atandigiTimeWindow.t2- hasta.timeWindow.t2);
            _genCeza *= Convert.ToInt32(Islemler.CezaPuanlari[Cezalar.hastaIstenmeyenPeriyod] * hasta.oncelik);//hasta önceliği ile ceza değeri güncellendi önceliği yüksek olanın ihlali aynı değer için daha yüksek olacak
         }
        //public void GenTasindi()
        //{
        //    //gen çaprazlama veya mutasyon ile yer değiştirirse çalışacak
        //    //zaman cercevesi ve genceza güncellenecek
        //    _genCeza = 1;
        //}
    }
}

          