using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public struct Nokta
    {
        public double lat;
        public double lon;
    }
    public struct Uzaklik
    {
        public double metre;
        public double dakika;
    }
    public struct Uzaklik2
    {
        public double metre;
        public int dakika;
    }
    //public struct _Hasta //kullanılmıyor yerine hasta sınıfı yazıldı
    //{
    //    public int hastaID;
    //    public Nokta konum;
    //}
    public struct TimeWindow
    {
        public int t1;
        public int t2;
    }
    public struct Hasta1Hasta2
    {
        public int hasta1ID;
        public int hasta2ID;
    }
   public enum IlkAtamaYontem
    {
        firstfit,
        bestfitteam,
        bestfitperiod       
    };
    public enum Cezalar
    {
        dakikaToplaminiCezala,
        metreToplaminiCezala,
        hastaIstenmeyenPeriyod,
        ekipIstenmeyenPeriyod,
        ekipFazlaMesaiPeriyod,
        oglearasiihlali,
        skillHatali,
        sSapmaMetre,
        sSapmaDakika          
    }

    class IyiKromozomlar
    {

        public int nesil;
        public double fitness;
        public Kromozom kromozom;
        public int kromozomID;

        public IyiKromozomlar(int nesil,Kromozom kromozom)
        {
            this.nesil = nesil;
            this.kromozom = kromozom;
            this.kromozomID = kromozom.kromozomId;
            this.fitness = kromozom.fitness;
            
        }
    }
    
    
    
}
