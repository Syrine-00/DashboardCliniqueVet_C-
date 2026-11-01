using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CliniqueVet
{
    public partial class FormVaccination : Form
    {
        private readonly int _idUser;
        private string ConnStr => System.Configuration.ConfigurationManager
            .ConnectionStrings["CliniqueVet"].ConnectionString;

        public FormVaccination(int idUser)
        {
            InitializeComponent();
            _idUser = idUser;
        }

        private void FormVaccination_Load(object sender, EventArgs e)
        {
            button7.Click += (s, ev) => { var f = new FormPlanning(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button6.Click += (s, ev) => { var f = new FormConsultation(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button3.Click += (s, ev) => { var f = new FormVaccination(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button4.Click += (s, ev) => { var f = new FormMedicament(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button5.Click += (s, ev) => { var f = new FormLogin(); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            btnEnregistrer.Click += BtnEnregistrer_Click;
            ChargerAnimaux();
            ChargerRappels();
            ChargerToutesVaccinations();
        }

        private void ChargerAnimaux()
        {
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    var cmd = new SqlCommand(
                        "SELECT IdAnimal, Nom + ' (' + Espece + ')' AS Lib " +
                        "FROM Animaux WHERE Actif = 1 ORDER BY Nom", conn);
                    conn.Open();
                    var reader = cmd.ExecuteReader();
                    comboBox1.Items.Clear();
                    while (reader.Read())
                        comboBox1.Items.Add(new AnimalItem
                        {
                            Id = (int)reader["IdAnimal"],
                            Lib = reader["Lib"].ToString()
                        });
                    comboBox1.DisplayMember = "Lib";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur chargement animaux : " + ex.Message);
            }
        }

        private void ChargerRappels()
        {
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    var cmd = new SqlCommand(
                        "SELECT a.Nom AS Animal, v.NomVaccin, v.DateAdministration, v.DateRappel " +
                        "FROM Vaccinations v JOIN Animaux a ON v.IdAnimal = a.IdAnimal " +
                        "WHERE v.DateRappel BETWEEN GETDATE() AND DATEADD(DAY, 30, GETDATE()) " +
                        "ORDER BY v.DateRappel", conn);
                    conn.Open();
                    var dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);
                    dataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur rappels : " + ex.Message);
            }
        }

        private void ChargerToutesVaccinations()
        {
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    var cmd = new SqlCommand(
                        "SELECT a.Nom AS Animal, v.NomVaccin, v.DateAdministration, " +
                        "v.DateRappel, v.Lot " +
                        "FROM Vaccinations v JOIN Animaux a ON v.IdAnimal = a.IdAnimal " +
                        "ORDER BY v.DateAdministration DESC", conn);
                    conn.Open();
                    var dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);
                    dataGridView2.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur vaccinations : " + ex.Message);
            }
        }

        private void BtnEnregistrer_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null || string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Animal et vaccin sont obligatoires.");
                return;
            }

            try
            {
                int idAnimal = ((AnimalItem)comboBox1.SelectedItem).Id;
                using (var conn = new SqlConnection(ConnStr))
                {
                    var cmd = new SqlCommand(
                        "INSERT INTO Vaccinations (IdAnimal, IdVeterinaire, NomVaccin, " +
                        "DateAdministration, DateRappel, Lot) " +
                        "VALUES (@a, @v, @n, @da, @dr, @l)", conn);
                    cmd.Parameters.AddWithValue("@a", idAnimal);
                    cmd.Parameters.AddWithValue("@v", _idUser);
                    cmd.Parameters.AddWithValue("@n", textBox1.Text.Trim());
                    cmd.Parameters.AddWithValue("@da", dateTimePicker1.Value.Date);
                    cmd.Parameters.AddWithValue("@dr", dateTimePicker2.Value.Date);
                    cmd.Parameters.AddWithValue("@l",
                        string.IsNullOrWhiteSpace(textBox2.Text)
                        ? (object)DBNull.Value : textBox2.Text.Trim());
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Vaccination enregistrée avec succès !");
                textBox1.Clear();
                textBox2.Clear();
                comboBox1.SelectedIndex = -1;
                ChargerRappels();
                ChargerToutesVaccinations();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur : " + ex.Message);
            }
        }

        private class AnimalItem
        {
            public int Id { get; set; }
            public string Lib { get; set; }
            public override string ToString() => Lib;
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}