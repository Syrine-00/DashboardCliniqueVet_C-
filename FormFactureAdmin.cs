using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CliniqueVet
{
    public partial class FormFactureAdmin : Form
    {
        private readonly int _idUser;
        private string ConnStr => System.Configuration.ConfigurationManager
            .ConnectionStrings["CliniqueVet"].ConnectionString;

        public FormFactureAdmin(int idUser)
        {
            InitializeComponent();
            _idUser = idUser;
        }

        private void FormFactureAdmin_Load(object sender, EventArgs e)
        {
            button1.Click += (s, ev) => { var f = new FormDashboard(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button2.Click += (s, ev) => { var f = new FormUtilisateurs(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button3.Click += (s, ev) => { var f = new FormAnimauxAdmin(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button4.Click += (s, ev) => { var f = new FormMedicAdmin(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button5.Click += (s, ev) => { var f = new FormFactureAdmin(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            btnDeco.Click += (s, ev) => { var f = new FormLogin(); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };

            button6.Click += (s, ev) => ChargerFactures();
            button8.Click += BtnMarquerPayee_Click;

            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(new string[] { "Tous", "Non payée", "Payée", "Partiellement payée" });
            comboBox1.SelectedIndex = 0;

            dateTimePicker1.Value = DateTime.Today.AddYears(-1);
            dateTimePicker2.Value = DateTime.Today.AddDays(1);

            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            ChargerFactures();
        }

        private void ChargerFactures()
        {
            try
            {
                string statut = comboBox1.SelectedItem?.ToString() ?? "Tous";
                using (var conn = new SqlConnection(ConnStr))
                {
                    var cmd = new SqlCommand(
                        @"SELECT f.IdFacture AS ID,
                                 CONVERT(VARCHAR, f.DateFacture, 103) AS Date,
                                 p.Nom + ' ' + p.Prenom AS Proprietaire,
                                 a.Nom AS Animal,
                                 f.MontantConsultation AS MontantConsult,
                                 f.MontantMedicaments AS MontantMed,
                                 f.MontantTotal AS Total,
                                 f.Statut,
                                 f.ModePaiement
                          FROM Factures f
                          JOIN Consultations c ON f.IdConsultation = c.IdConsultation
                          JOIN Animaux a ON c.IdAnimal = a.IdAnimal
                          JOIN Proprietaires p ON f.IdProprietaire = p.IdProprietaire
                          WHERE (@s='Tous' OR f.Statut=@s)
                          ORDER BY f.DateFacture DESC", conn);
                    cmd.Parameters.AddWithValue("@s", statut);
                    conn.Open();
                    var dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);
                    dataGridView1.DataSource = dt;
                }

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow) continue;
                    string s = row.Cells["Statut"].Value?.ToString() ?? "";
                    if (s == "Payée") row.DefaultCellStyle.ForeColor = Color.FromArgb(0, 150, 0);
                    else if (s == "Non payée") row.DefaultCellStyle.ForeColor = Color.FromArgb(200, 0, 0);
                }

            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private int GetSelectedId()
        {
            if (dataGridView1.SelectedRows.Count == 0) { MessageBox.Show("Sélectionnez une facture."); return -1; }
            return Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);
        }

        

        private void BtnMarquerPayee_Click(object sender, EventArgs e)
        {
            int id = GetSelectedId();
            if (id == -1) return;
            string statut = dataGridView1.SelectedRows[0].Cells["Statut"].Value?.ToString();
            if (statut == "Payée") { MessageBox.Show("Déjà payée."); return; }

            if (MessageBox.Show("Marquer comme Payée ?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    using (var conn = new SqlConnection(ConnStr))
                    {
                        var cmd = new SqlCommand(
                            "UPDATE Factures SET Statut='Payée', DatePaiement=GETDATE() WHERE IdFacture=@id", conn);
                        cmd.Parameters.AddWithValue("@id", id);
                        conn.Open(); cmd.ExecuteNonQuery();
                    }
                    ChargerFactures();
                }
                catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
            }
        }
    }
}