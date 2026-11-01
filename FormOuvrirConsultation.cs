using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CliniqueVet
{
    public partial class FormOuvrirConsultation : Form
    {
        private int _idAnimal, _idRdv, _idVet;
        private string ConnStr => System.Configuration.ConfigurationManager
            .ConnectionStrings["CliniqueVet"].ConnectionString;

        // Constructeur modifié pour recevoir les données du Planning
        public FormOuvrirConsultation(int idAnimal, int idRdv, int idVet)
        {
            InitializeComponent();
            _idAnimal = idAnimal;
            _idRdv = idRdv;
            _idVet = idVet;

            // Liaison de l'événement clic du bouton Enregistrer (button1 dans votre Designer)
            this.button1.Click += BtnEnregistrer_Click;
        }

        private void BtnEnregistrer_Click(object sender, EventArgs e)
        {
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();

                    // 1. Insertion de la consultation
                    string queryIns = @"INSERT INTO Consultations (IdRendezVous, IdAnimal, IdVeterinaire, DateConsultation, 
                                        Diagnostic, Traitement, Poids, Temperature) 
                                        VALUES (@rv, @a, @v, GETDATE(), @diag, @trai, @p, @t)";

                    using (var cmd = new SqlCommand(queryIns, conn))
                    {
                        cmd.Parameters.AddWithValue("@rv", _idRdv);
                        cmd.Parameters.AddWithValue("@a", _idAnimal);
                        cmd.Parameters.AddWithValue("@v", _idVet);
                        cmd.Parameters.AddWithValue("@diag", richTextBox1.Text.Trim()); // Diagnostic
                        cmd.Parameters.AddWithValue("@trai", richTextBox2.Text.Trim()); // Traitement
                        cmd.Parameters.AddWithValue("@p", numericUpDown1.Value);        // Poids
                        cmd.Parameters.AddWithValue("@t", numericUpDown2.Value);        // Température
                        cmd.ExecuteNonQuery();
                    }

                    // 2. Mise à jour du statut du RDV pour qu'il apparaisse en "Terminé"
                    string queryUpd = "UPDATE RendezVous SET Statut = 'Terminé' WHERE IdRendezVous = @id";
                    using (var cmdUpd = new SqlCommand(queryUpd, conn))
                    {
                        cmdUpd.Parameters.AddWithValue("@id", _idRdv);
                        cmdUpd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Consultation enregistrée avec succès !");
                this.DialogResult = DialogResult.OK; // Indique au Planning que c'est réussi
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }
    }
}