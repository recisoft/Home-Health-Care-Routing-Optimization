using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace WindowsFormsApp1
{
    public partial class FormRouteLoad : Form
    {
       List<Rota> myRotaList = new List<Rota>();
       // Int64 gunId;
        private List<Rota> RotaListesi()
        {
            List<Rota> myRotaList = new List<Rota>();
           //grid1 sonuçlar
           //grid2 rotalar
           //grid 3 genler var, 
          //rotaların hepsi grid 2 den döngü ile alınacak, sonra bu döngüde ziyaret listesi bir reader ile çekilecek
          //oluşturulan rotalar rotalistesine add yapılacak
          
            for (int i = 0; i < dataGridView2.Rows.Count-1; i++)
            {
                int rotaID= (int)dataGridView2.Rows[i].Cells[0].Value;
                Rota myrota = new Rota();
                myrota.ziyaretSirasi.Clear();//rota nesnesi doğası gereği her seferinde kendisi 2 adet gen ekliyor onlar silindi
                Ekip myekip = new Ekip();
                myekip.ekipID= (int)dataGridView2.Rows[i].Cells[3].Value;
                myrota.ekip = myekip;
                myrota.rotaceza= (int)dataGridView2.Rows[i].Cells[4].Value;
                Uzaklik2 myuzaklik = new Uzaklik2();
                myuzaklik.metre= (int)dataGridView2.Rows[i].Cells[5].Value;
                myuzaklik.dakika= (int)dataGridView2.Rows[i].Cells[6].Value;
                myrota.toplamUzaklik = myuzaklik;

                string sqlcumle = "exec ZiyaretSirasigetir " + rotaID.ToString();
                SqlCommand sqlkomut = new SqlCommand();
                sqlkomut.Connection = Islemler.conn;
                sqlkomut.CommandText = sqlcumle;

                Islemler.conn.Open();

                SqlDataReader drZiyaretler = sqlkomut.ExecuteReader();
                while (drZiyaretler.Read())
                {
                    Gen mygen = new Gen();
                    Hasta myhasta = new Hasta();
                    myhasta.hastaID = (int)drZiyaretler[1];
                    myhasta.gosterID = (int)drZiyaretler[2];
                    myhasta.bakimSuresi = (int)drZiyaretler[5];
                    myhasta.konum.lat=(double)drZiyaretler[6];
                    myhasta.konum.lon = (double)drZiyaretler[7];

                    mygen.hasta = myhasta;
                    mygen.atandigiTimeWindow.t1 = (int)drZiyaretler[3];
                    mygen.atandigiTimeWindow.t2 = (int)drZiyaretler[4];
                    myrota.ziyaretSirasi.Add(mygen);
                }
                drZiyaretler.Close();
                myRotaList.Add(myrota);
                Islemler.conn.Close();
            }
           
            
            return myRotaList;
        }

        private void RotalarıDoldur(List<Rota> RotaList, ListBox listB2, ListBox listB3)
        {
            listB2.Items.Clear();
            listB3.Items.Clear();
            string rotalar = "";
            foreach (Rota myrota in RotaList)
            {
                listB3.Items.Add("--" + myrota.ekip.ekipID.ToString() + "--");

                rotalar = "RF:" + myrota.rotaceza.ToString();
                rotalar += ";RM:" + myrota.toplamUzaklik.metre.ToString();
                rotalar += ";RP:" + myrota.toplamUzaklik.dakika.ToString();
                rotalar += ";Ek" + myrota.ekip.ekipID.ToString() + ">";

                foreach (Gen mygen in myrota.ziyaretSirasi)
                {
                    rotalar += " - " + mygen.hasta.gosterID.ToString();
                    rotalar += "[" + mygen.atandigiTimeWindow.t1.ToString() + ":" + mygen.atandigiTimeWindow.t2.ToString() + "]";
                    if (mygen.hasta.hastaID != 0) listB3.Items.Add(mygen.hasta.gosterID);
                }
                listB2.Items.Add(rotalar);
            }
            listB2.Refresh();
            listB3.Refresh();
        }
        public FormRouteLoad()
        {
            InitializeComponent();
        }

        private void FormRouteLoad_Load(object sender, EventArgs e)
        {
            string sqlcumle = "exec SonucGetir";
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(sqlcumle, Islemler.conn);
            da.Fill(dt);
            dataGridView1.DataSource = dt;
            dataGridView1.Columns[1].Visible = false;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {


        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            //       if (dataGridView1.SelectedRows.Count == 0) return;
            try
            {
                int ID = (int)dataGridView1.CurrentRow.Cells[1].Value;
                Islemler.gunID = (int)(Int64)dataGridView1.CurrentRow.Cells[2].Value;
                string sqlcumle = "exec RotaGetir " + ID.ToString();
                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(sqlcumle, Islemler.conn);
                da.Fill(dt);
                dataGridView2.DataSource = dt;
            }

            catch {
                return;
            }
          
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
    //        if (dataGridView2.SelectedRows.Count == 0) return;
            int ID = (int)dataGridView2.CurrentRow.Cells[0].Value;
            string sqlcumle = "exec ZiyaretSirasiGetir " + ID.ToString();
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(sqlcumle, Islemler.conn);
            da.Fill(dt);
            dataGridView3.DataSource = dt;
        }

        private void dataGridView2_SelectionChanged(object sender, EventArgs e)
        {
          //  if (dataGridView2.SelectedRows.Count == 0) return;
            int ID = (int)dataGridView2.CurrentRow.Cells[0].Value;
            string sqlcumle = "exec ZiyaretSirasiGetir " + ID.ToString(); ;
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter(sqlcumle, Islemler.conn);
            da.Fill(dt);
            dataGridView3.DataSource = dt;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
           
            Islemler.rotaLoadList = RotaListesi();

            this.Close();
            //Form1 myform = new Form1();
            //ListBox lsb2 = myform.listBox2;
            //ListBox lsb3 = myform.listBox3;
            
            //RotalarıDoldur(myRotaList, lsb2, lsb3);
           
        }
    }
}
