using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class Hasta
    {
        //yapıcı ile atanacak olan değerler
        public int indis;
        public int hastaID;//gerçek hasta Id sidir, noktalar arası uzaklık, bakım timewindow için kullanılacak
        public int gosterID; //programda hasta ıdlerin her zaman 1-2-... gitmesi için kullanılan görüntüde ki değer
        public Nokta konum;
        public double oncelik;
        public int bakimSuresi;
        public TimeWindow timeWindow;
        public int skill;
        public List<double> kitliklist = new List<double>();//bu liste pestfitekip atamasında kullanılacak
        public double ekipihtiyacsira;
        //
        public Hasta() //default yapıcı
        {

        }
        public Hasta(int hastaID, int gosterID, Nokta konum, double oncelik, int bakimSuresi, TimeWindow timeWindow, int skill)
        {
            this.hastaID = hastaID;
            this.gosterID = gosterID;
            this.konum = konum;
            this.oncelik = oncelik;
            this.bakimSuresi = bakimSuresi;
            this.timeWindow = timeWindow;
            this.skill = skill;
        }
       
    }
}
