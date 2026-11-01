using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CliniqueVet
{
    public partial class FormConsultation : Form
    {
        private readonly int _idUser;
        private string ConnStr => System.Configuration.ConfigurationManager
            .ConnectionStrings["CliniqueVet"].ConnectionString;

        public FormConsultation(int idUser)
        {
            InitializeComponent();
            _idUser = idUser;
        }

        private void FormConsultation_Load(object sender, EventArgs e)
        {
            // Navigation Sidebar
            button7.Click += (s, ev) => { var f = new FormPlanning(_idUser); f.Show(); this.Hide(); };
            button6.Click += (s, ev) => { var f = new FormConsultation(_idUser); f.Show(); this.Hide(); };
            button3.Click += (s, ev) => { var f = new FormVaccination(_idUser); f.Show(); this.Hide(); };
            button4.Click += (s, ev) => { var f = new FormMedicament(_idUser); f.Show(); this.Hide(); };
            button5.Click += (s, ev) => { var f = new FormLogin(); f.Show(); this.Hide(); };

            // Actions
            button1.Click += (s, ev) => ChargerAnimaux(txtRecherche.Text);
            button2.Click += BtnOuvrirDossier_Click;
            txtRecherche.KeyDown += (s, ev) => { if (ev.KeyCode == Keys.Enter) ChargerAnimaux(txtRecherche.Text); };

            ChargerAnimaux("");
        }

        private void ChargerAnimaux(string recherche)
        {
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    string sql = @"SELECT a.IdAnimal, a.Nom, a.Espece, a.Race, a.Sexe, 
                                 p.Nom + ' ' + p.Prenom AS Proprietaire 
                                 FROM Animaux a 
                                 JOIN Proprietaires p ON a.IdProprietaire = p.IdProprietaire 
                                 WHERE a.Actif = 1 ";

                    if (!string.IsNullOrWhiteSpace(recherche))
                        sql += "AND (a.Nom LIKE @t OR a.Espece LIKE @t OR p.Nom LIKE @t) ";

                    sql += "ORDER BY a.Nom";

                    var cmd = new SqlCommand(sql, conn);
                    if (!string.IsNullOrWhiteSpace(recherche))
                        cmd.Parameters.AddWithValue("@t", "%" + recherche + "%");

                    conn.Open();
                    var dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);
                    dataGridView2.DataSource = dt;

                    if (dataGridView2.Columns["IdAnimal"] != null)
                        dataGridView2.Columns["IdAnimal"].Visible = false;
                }
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private void BtnOuvrirDossier_Click(object sender, EventArgs e)
        {
            if (dataGridView2.CurrentRow == null) return;

            // Récupération de l'ID de l'animal sélectionné
            int idAnimal = Convert.ToInt32(dataGridView2.CurrentRow.Cells["IdAnimal"].Value);

            // Appel de votre formulaire FormOuvrirConsultation
            // On passe 0 pour idRdv car c'est une consultation hors planning
            var frm = new FormOuvrirConsultation(idAnimal, 0, _idUser);
            frm.ShowDialog();
        }
    }
}