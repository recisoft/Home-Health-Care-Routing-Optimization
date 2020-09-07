using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class FormCezalar : Form
    {
        public FormCezalar()
        {
            InitializeComponent();
        }

      

        private void buttonVasayilan_Click(object sender, EventArgs e)
        {
            textMetre.Text = "1";
            textDakika.Text = "0";
            textHastaZaman.Text = "2";
            textHastaPersonelSkill.Text = Islemler.M.ToString();
            textEkipZaman.Text = "0";
            textEkipFazlaMesai.Text = "0";
            textEkipOgleArasi.Text = "10";
            textEkipMesafeSapma.Text = "5";
            textEkipZamanSapma.Text = "50";
            buttonGuncelle_Click(sender,e);
        }

        private void buttonGuncelle_Click(object sender, EventArgs e)
        {
            try
            {
                int dakika = Int32.Parse(textDakika.Text);
                int metre = Int32.Parse(textMetre.Text);
                int hastaPeriyod = Int32.Parse(textHastaZaman.Text);
                int ekipPeriyod = Int32.Parse(textEkipZaman.Text);
                int skill = Int32.Parse(textHastaPersonelSkill.Text);
                int ekipFazlaMesai = Int32.Parse(textEkipFazlaMesai.Text);
                int ekipOgleArasi = Int32.Parse(textEkipOgleArasi.Text);
                int sapmaMetre = Int32.Parse(textEkipMesafeSapma.Text);
                int sapmaDakika = Int32.Parse(textEkipZamanSapma.Text);
                Islemler.CezaPuanlariniBelirle(dakika, metre, hastaPeriyod, ekipPeriyod, skill, ekipFazlaMesai, ekipOgleArasi, sapmaMetre, sapmaDakika);
                MessageBox.Show("GA ceza değerleri güncellendi");
                this.Close();
            }
            catch
            {
                MessageBox.Show("Ceza değerleri tamsayı olmalıdır");
            }
        }

        private void FormCezalar_Load(object sender, EventArgs e)
        {
           textDakika.Text=Islemler.CezaPuanlari[Cezalar.dakikaToplaminiCezala].ToString();
           textMetre.Text=Islemler.CezaPuanlari[Cezalar.metreToplaminiCezala].ToString();
           textHastaZaman.Text = Islemler.CezaPuanlari[Cezalar.hastaIstenmeyenPeriyod].ToString();
           textEkipZaman.Text= Islemler.CezaPuanlari[Cezalar.ekipIstenmeyenPeriyod].ToString(); ;
           textHastaPersonelSkill.Text= Islemler.CezaPuanlari[Cezalar.skillHatali].ToString();
           textEkipFazlaMesai.Text = Islemler.CezaPuanlari[Cezalar.ekipFazlaMesaiPeriyod].ToString();
           textEkipOgleArasi.Text= Islemler.CezaPuanlari[Cezalar.oglearasiihlali].ToString();
           textEkipMesafeSapma.Text= Islemler.CezaPuanlari[Cezalar.sSapmaMetre].ToString();
           textEkipZamanSapma.Text= Islemler.CezaPuanlari[Cezalar.sSapmaDakika].ToString(); 
        }
    }
}
