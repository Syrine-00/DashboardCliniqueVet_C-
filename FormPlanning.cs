using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CliniqueVet
{
    public partial class FormPlanning : Form
    {
        private readonly int _idUser;
        private string ConnStr => System.Configuration.ConfigurationManager
            .ConnectionStrings["CliniqueVet"].ConnectionString;

        public FormPlanning(int idUser)
        {
            InitializeComponent();
            _idUser = idUser;
        }

        private void FormPlanning_Load(object sender, EventArgs e)
        {
            button7.Click += (s, ev) => { var f = new FormPlanning(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button6.Click += (s, ev) => { var f = new FormConsultation(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button3.Click += (s, ev) => { var f = new FormVaccination(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button4.Click += (s, ev) => { var f = new FormMedicament(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button5.Click += (s, ev) => { var f = new FormLogin(); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            btnConsultation.Click += BtnOuvrirConsultation_Click;
            btnFiltrer.Click += button1_Click;
            dataGridView1.DataSource = null;
            dataGridView1.AutoGenerateColumns = true;
            ChargerPlanning(dateTimePicker1.Value);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ChargerPlanning(dateTimePicker1.Value);
        }

        private void ChargerPlanning(DateTime date)
        {
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    // On utilise la même logique de JOIN que FormRDV 
                    // mais on garde les filtres spécifiques au planning (Vétérinaire connecté + Date choisie)
                    var cmd = new SqlCommand(
                        @"SELECT rv.IdRendezVous, rv.DateHeure, rv.Motif, rv.Statut, 
                         a.IdAnimal, a.Nom AS Animal, a.Espece, 
                         u.Nom + ' ' + u.Prenom AS Veterinaire,
                         p.Nom + ' ' + p.Prenom AS Proprietaire 
                  FROM RendezVous rv 
                  JOIN Animaux a ON rv.IdAnimal = a.IdAnimal 
                  JOIN Proprietaires p ON a.IdProprietaire = p.IdProprietaire
                  JOIN Utilisateurs u ON rv.IdVeterinaire = u.IdUtilisateur
                  WHERE rv.IdVeterinaire = @v AND CAST(rv.DateHeure AS DATE) = @d 
                  ORDER BY rv.DateHeure", conn);

                    cmd.Parameters.AddWithValue("@v", _idUser);
                    cmd.Parameters.AddWithValue("@d", date.Date);

                    conn.Open();
                    var dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);
                    dataGridView1.DataSource = dt;

                    // Masquer les IDs pour que ce soit propre comme dans FormRDV
                    if (dataGridView1.Columns["IdRendezVous"] != null) dataGridView1.Columns["IdRendezVous"].Visible = false;
                    if (dataGridView1.Columns["IdAnimal"] != null) dataGridView1.Columns["IdAnimal"].Visible = false;
                }

                // Coloration des lignes selon le statut
                AppliquerCouleursStatut();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        private void AppliquerCouleursStatut()
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow) continue;
                string statut = row.Cells["Statut"].Value?.ToString() ?? "";

                if (statut == "Terminé")
                    row.DefaultCellStyle.BackColor = Color.FromArgb(220, 255, 220); // Vert clair
                else if (statut == "Annulé")
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220); // Rouge clair
                else if (statut == "Confirmé")
                    row.DefaultCellStyle.BackColor = Color.FromArgb(220, 235, 255); // Bleu clair
            }
        }

        private void BtnOuvrirConsultation_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null) return;

            // Récupération des IDs de la ligne sélectionnée dans le DataGridView
            int idAnimal = Convert.ToInt32(dataGridView1.CurrentRow.Cells["IdAnimal"].Value);
            int idRdv = Convert.ToInt32(dataGridView1.CurrentRow.Cells["IdRendezVous"].Value);

            // On ouvre VOTRE formulaire personnalisé
            // On lui passe l'animal, le RDV et le vétérinaire connecté (_idUser)
            var frm = new FormOuvrirConsultation(idAnimal, idRdv, _idUser);

            if (frm.ShowDialog() == DialogResult.OK)
            {
                // Si l'enregistrement a réussi, on rafraîchit la liste pour voir le statut "Terminé"
                ChargerPlanning(dateTimePicker1.Value);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {

        }
    }
}