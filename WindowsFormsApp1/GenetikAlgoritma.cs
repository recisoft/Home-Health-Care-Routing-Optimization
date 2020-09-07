using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    class GenetikAlgoritma
    {
        public IlkAtamaYontem atamaYontem;
        public int kromozomSayisi;
        public int caprazlamaOrani;
        public int mutasyonOrani;    //başlangıçta nımlandı ama kullanılmadı  
        public int nesilSayisi;
        private int nesilanlik;
        public bool elitizmEniyi;
        public string caprazlasecim;
        public Populasyon populasyon;
        public List<IyiKromozomlar> IyiList;//her nesildeki sonucu tutacak, dosyaya yada database yazdırılabilir

        public GenetikAlgoritma(IlkAtamaYontem atamaYontem, int kromozomSayisi, int caprazlamaOrani, string caprazlasecim, int mutasyonOrani, int nesilSayisi, bool elitizmVarmi=true)
        {
            this.atamaYontem = atamaYontem;
            this.kromozomSayisi = kromozomSayisi;
            this.caprazlamaOrani = caprazlamaOrani;
            this.caprazlasecim = caprazlasecim;
            this.mutasyonOrani = mutasyonOrani;           
            this.nesilSayisi = nesilSayisi;
            this.nesilanlik = 0;
            this.elitizmEniyi = elitizmVarmi;
            populasyon = new Populasyon(kromozomSayisi, atamaYontem);

            if (populasyon.kromozomListesi.Count == 0)
            {
                MessageBox.Show("Verileri Kontrol Edin\nAtama yapılamadı!!!!!");

                return;
            }

            IyiList = new List<IyiKromozomlar>();
        }

        public void Calistir(ListBox mylistbox)
        {
            /*
             * ilk atalamar yapıldı işlem adımları aşağıdaki gibi olacak
             * 1- Fitness hesapla
             * 2- En iyi fitness değerini Iyikromozomlar listesine ekle
             * 3- Durma kriteri oluştuysa 7. adıma git
             * 4- Çaprazlama Yap (2 yöntem var)
             * 5- mutasyon yap
             * 6- 1. adıma git
             * 7- sonuçları yaz (dosya,ekran,database vs vs)
             * 
             */
            DateTime zaman1;
            DateTime zaman2;
            for (int i=0;i<nesilSayisi;i++)
            {
                //çaprazlama işlemi, 2 türlü yapılıyor birisi rastgele, diğeri ise iyi olan bireylerin seçim şansının yüksek olduğu yöntem
                zaman1 = DateTime.Now;
                if (caprazlasecim=="Random") 
                    populasyon.Caprazla2Grup(caprazlamaOrani);//Çaprazlanacak olan bireyleri random seçer
                else
                    populasyon.Caprazla2Grup_Olasilikli(caprazlamaOrani);//Fittnes değeri daha iyi olanların seçim şansıda yüksek olur

                zaman2 = DateTime.Now;
                TimeSpan fark = zaman2 - zaman1;
                string caprazlazaman = fark.Milliseconds.ToString();

                zaman1 = DateTime.Now;
                populasyon.FitnessHesapla();
                zaman2 = DateTime.Now;
                fark = zaman2 - zaman1;
                string fitzaman = fark.Milliseconds.ToString();

                zaman1 = DateTime.Now;
                //populasyon.ElitizimUygula(elitizmEniyi); //iyi sonuç vermeyince kullanılmadı
                populasyon.ElitizimEniyi();
                zaman2 = DateTime.Now;
                fark = zaman2 - zaman1;
                string elittzaman = fark.Milliseconds.ToString();

                if (populasyon.kromozomListesi.Count == 0)
                {
                    //List<string> yazdirstring = new List<string>();
                    //for (int j = 0; j < Islemler.atanamayanHastalarListesi.Count; j++)
                    //{
                       
                    //    bool buldum = false;
                    //    for (int n = 0; n < yazdirstring.Count; n++)
                    //    {
                    //        buldum = false;
                    //        if (Islemler.atanamayanHastalarListesi[j] == yazdirstring[n])
                    //        {
                    //            buldum = true;
                    //            break;
                    //        }
                    //    }
                    //    if (buldum == false) yazdirstring.Add(Islemler.atanamayanHastalarListesi[i]);
                    //}

                    //for (int n = 0; n < yazdirstring.Count; n++)
                    //    mylistbox.Items.Add(yazdirstring[n]);
                  return;
                }
                  

                IyiKromozomlar iyi = new IyiKromozomlar(i, populasyon.kromozomListesi[0]);
                string deger = "";
                // deger += i.ToString() +" Krom ID:"+ populasyon.kromozomListesi[0].kromozomId.ToString();
  
                //deger += String.Format("Iter:{0,3}",i);        
                //deger += " Fitn:" + populasyon.kromozomListesi[0].fitness.ToString();
                deger += String.Format("I:{0,3}", nesilanlik++);
                deger += " F:" + populasyon.kromozomListesi[0].fitness.ToString();
                deger += " M:" + populasyon.kromozomListesi[0].toplamUzaklik.metre.ToString();
                deger += " P:" + populasyon.kromozomListesi[0].toplamUzaklik.dakika.ToString();

                //  deger += " cz:" + caprazlazaman;
                //  deger += " fz:"+fitzaman;
                //  deger += " ez:" + elittzaman;
                mylistbox.Items.Insert(0,deger);
                mylistbox.Refresh();
                IyiList.Add(iyi);

                
            }

           
        }
    }
}
