using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CliniqueVet
{
    public partial class FormMedicament : Form
    {
        private readonly int _idUser;
        private string ConnStr => System.Configuration.ConfigurationManager
            .ConnectionStrings["CliniqueVet"].ConnectionString;

        public FormMedicament(int idUser)
        {
            InitializeComponent();
            _idUser = idUser;
        }

        private void FormMedicament_Load(object sender, EventArgs e)
        {
            button7.Click += (s, ev) => { var f = new FormPlanning(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button6.Click += (s, ev) => { var f = new FormConsultation(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button3.Click += (s, ev) => { var f = new FormVaccination(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button4.Click += (s, ev) => { var f = new FormMedicament(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button5.Click += (s, ev) => { var f = new FormLogin(); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            ChargerStock();
        }

        private void ChargerStock()
        {
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    var cmd = new SqlCommand(
                        "SELECT Nom, Unite, QuantiteStock, StockMinimum, " +
                        "PrixUnitaire, DateExpiration, Fournisseur " +
                        "FROM Medicaments ORDER BY Nom", conn);
                    conn.Open();
                    var dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);
                    dataGridView1.DataSource = dt;
                }

                // Colorier en rouge les lignes avec stock faible
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow) continue;
                    int qte = Convert.ToInt32(row.Cells["QuantiteStock"].Value);
                    int min = Convert.ToInt32(row.Cells["StockMinimum"].Value);
                    if (qte <= min)
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 200, 200);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }
    }
}