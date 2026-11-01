using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CliniqueVet
{
    public partial class FormRDV : Form
    {
        private string ConnStr => System.Configuration.ConfigurationManager
            .ConnectionStrings["CliniqueVet"].ConnectionString;

        private int _idSelectionne = -1;

        public FormRDV()
        {
            InitializeComponent();
        }

        // ── Requis par le Designer ─────────────────────────────────────
        private void FormRDV_Load(object sender, EventArgs e)
        {
            ConfigurerUI();
            ChargerAnimauxDansCombo();
            ChargerVeterinairesDansCombo();
            ChargerRendezVous();
        }

        private void ConfigurerUI()
        {
            label2.Text = "Animal:";
            label3.Text = "Veterinaire:";
            label4.Text = "Date & Heure:";
            label5.Text = "Motif:";
            label6.Text = "Statut:";
            label7.Text = "Gestion Rendez-Vous";

            dateTimePicker1.Format = DateTimePickerFormat.Custom;
            dateTimePicker1.CustomFormat = "dd/MM/yyyy HH:mm";

            comboBox3.Items.Clear();
            comboBox3.Items.AddRange(new object[] { "Planifié", "Confirmé", "Annulé", "Terminé" }); comboBox3.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox3.SelectedIndex = 0;

            button5.Text = "Ajouter";
            button5.BackColor = Color.SeaGreen;
            button5.ForeColor = Color.White;
            button5.Click += BtnAjouter_Click;

            button6.Text = "Modifier";
            button6.BackColor = Color.SteelBlue;
            button6.ForeColor = Color.White;
            button6.Click += BtnModifier_Click;

            button7.Text = "Annuler RDV";
            button7.BackColor = Color.Crimson;
            button7.ForeColor = Color.White;
            button7.Click += BtnAnnuler_Click;

            // Sidebar
            btnProprio.Click += (s, ev) => { var f = new FormProprio(); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button1.Click += (s, ev) => { var f = new FormAnimaux(); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button2.Click += (s, ev) => { var f = new FormRDV(); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button3.Click += (s, ev) => { var f = new FormFacture(); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button4.Click += (s, ev) => { var f = new FormLogin(); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };

            dataGridView1.ReadOnly = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.CellClick += DataGridView1_CellClick;
        }

        private void ChargerAnimauxDansCombo()
        {
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        "SELECT IdAnimal, Nom+'('+Espece+')' AS NomAnimal FROM Animaux WHERE Actif=1 ORDER BY Nom", conn);
                    var dt = new System.Data.DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);
                    comboBox1.DataSource = dt;
                    comboBox1.DisplayMember = "NomAnimal";
                    comboBox1.ValueMember = "IdAnimal";
                }
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private void ChargerVeterinairesDansCombo()
        {
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        "SELECT IdUtilisateur, Nom+' '+Prenom AS NomVet FROM Utilisateurs WHERE Role='Veterinaire' AND Actif=1 ORDER BY Nom", conn);
                    var dt = new System.Data.DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);
                    comboBox2.DataSource = dt;
                    comboBox2.DisplayMember = "NomVet";
                    comboBox2.ValueMember = "IdUtilisateur";
                }
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private void ChargerRendezVous()
        {
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        @"SELECT rv.IdRendezVous, rv.DateHeure, rv.Motif, rv.Statut,
                                 a.Nom AS Animal, u.Nom+' '+u.Prenom AS Veterinaire
                          FROM RendezVous rv
                          JOIN Animaux a     ON rv.IdAnimal      = a.IdAnimal
                          JOIN Utilisateurs u ON rv.IdVeterinaire = u.IdUtilisateur
                          ORDER BY rv.DateHeure DESC", conn);
                    var dt = new System.Data.DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);
                    dataGridView1.DataSource = dt;

                    if (dataGridView1.Columns["IdRendezVous"] != null)
                        dataGridView1.Columns["IdRendezVous"].Visible = false;
                }
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dataGridView1.Rows[e.RowIndex];
            _idSelectionne = Convert.ToInt32(row.Cells["IdRendezVous"].Value);
            textBox4.Text = row.Cells["Motif"].Value?.ToString();
            comboBox3.Text = row.Cells["Statut"].Value?.ToString();
            if (row.Cells["DateHeure"].Value != DBNull.Value)
                dateTimePicker1.Value = Convert.ToDateTime(row.Cells["DateHeure"].Value);
        }

        private void BtnAjouter_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue == null || comboBox2.SelectedValue == null)
            { MessageBox.Show("Selectionnez un animal et un veterinaire.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        "INSERT INTO RendezVous(IdAnimal,IdVeterinaire,DateHeure,Motif,Statut) VALUES(@ia,@iv,@dh,@m,@s)", conn);
                    cmd.Parameters.AddWithValue("@ia", comboBox1.SelectedValue);
                    cmd.Parameters.AddWithValue("@iv", comboBox2.SelectedValue);
                    cmd.Parameters.AddWithValue("@dh", dateTimePicker1.Value);
                    cmd.Parameters.AddWithValue("@m", Val(textBox4.Text));
                    cmd.Parameters.AddWithValue("@s", comboBox3.Text);
                    cmd.ExecuteNonQuery();
                }
                ChargerRendezVous();
                MessageBox.Show("Rendez-vous ajoute.", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private void BtnModifier_Click(object sender, EventArgs e)
        {
            if (_idSelectionne < 0) { MessageBox.Show("Selectionnez un rendez-vous."); return; }
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        "UPDATE RendezVous SET IdAnimal=@ia,IdVeterinaire=@iv,DateHeure=@dh,Motif=@m,Statut=@s WHERE IdRendezVous=@id", conn);
                    cmd.Parameters.AddWithValue("@ia", comboBox1.SelectedValue);
                    cmd.Parameters.AddWithValue("@iv", comboBox2.SelectedValue);
                    cmd.Parameters.AddWithValue("@dh", dateTimePicker1.Value);
                    cmd.Parameters.AddWithValue("@m", Val(textBox4.Text));
                    cmd.Parameters.AddWithValue("@s", comboBox3.Text);
                    cmd.Parameters.AddWithValue("@id", _idSelectionne);
                    cmd.ExecuteNonQuery();
                }
                ChargerRendezVous();
                MessageBox.Show("Rendez-vous modifie.", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private void BtnAnnuler_Click(object sender, EventArgs e)
        {
            if (_idSelectionne < 0) { MessageBox.Show("Selectionnez un rendez-vous."); return; }
            if (MessageBox.Show("Annuler ce rendez-vous ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    new SqlCommand("UPDATE RendezVous SET Statut='Annulé' WHERE IdRendezVous=@id", conn)
                    { Parameters = { new SqlParameter("@id", _idSelectionne) } }.ExecuteNonQuery();
                }
                ChargerRendezVous();
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private object Val(string s) => string.IsNullOrWhiteSpace(s) ? (object)DBNull.Value : s.Trim();

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}