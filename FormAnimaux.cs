using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CliniqueVet
{
    public partial class FormAnimaux : Form
    {
        private string ConnStr => System.Configuration.ConfigurationManager
            .ConnectionStrings["CliniqueVet"].ConnectionString;

        private int _idSelectionne = -1;

        public FormAnimaux()
        {
            InitializeComponent();
        }

        // ── Requis par le Designer ─────────────────────────────────────
        private void FormAnimaux_Load(object sender, EventArgs e)
        {
            ConfigurerUI();
            ChargerAnimaux();
        }

        private void ConfigurerUI()
        {
            label2.Text = "Nom:";
            label3.Text = "Espece:";
            label4.Text = "Race:";
            label5.Text = "Sexe:";
            label6.Text = "Date Naissance:";
            label7.Text = "Gestion Animaux";

            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(new object[] { "M", "F" });
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

            button5.Text = "Ajouter";
            button5.BackColor = Color.SeaGreen;
            button5.ForeColor = Color.White;
            button5.Click += BtnAjouter_Click;

            button6.Text = "Modifier";
            button6.BackColor = Color.SteelBlue;
            button6.ForeColor = Color.White;
            button6.Click += BtnModifier_Click;

            button7.Text = "Supprimer";
            button7.BackColor = Color.Crimson;
            button7.ForeColor = Color.White;
            button7.Click += BtnSupprimer_Click;

            button8.Text = "Rechercher";
            button8.Click += BtnRechercher_Click;

            button9.Text = "Vider Champs";
            button9.Click += (s, ev) => ViderChamps();

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

        private void ChargerAnimaux(string filtre = "")
        {
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    string sql = @"SELECT a.IdAnimal, a.Nom, a.Espece, a.Race, a.Sexe,
                                          a.DateNaissance, p.Nom+' '+p.Prenom AS Proprietaire
                                   FROM Animaux a
                                   JOIN Proprietaires p ON a.IdProprietaire=p.IdProprietaire
                                   WHERE a.Actif=1";
                    if (!string.IsNullOrWhiteSpace(filtre))
                        sql += " AND (a.Nom LIKE @f OR a.Espece LIKE @f OR a.Race LIKE @f)";
                    sql += " ORDER BY a.Nom";

                    var cmd = new SqlCommand(sql, conn);
                    if (!string.IsNullOrWhiteSpace(filtre))
                        cmd.Parameters.AddWithValue("@f", "%" + filtre + "%");

                    var dt = new System.Data.DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);
                    dataGridView1.DataSource = dt;

                    if (dataGridView1.Columns["IdAnimal"] != null)
                        dataGridView1.Columns["IdAnimal"].Visible = false;
                }
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dataGridView1.Rows[e.RowIndex];
            _idSelectionne = Convert.ToInt32(row.Cells["IdAnimal"].Value);
            textBox1.Text = row.Cells["Nom"].Value?.ToString();
            textBox2.Text = row.Cells["Espece"].Value?.ToString();
            textBox3.Text = row.Cells["Race"].Value?.ToString();
            comboBox1.Text = row.Cells["Sexe"].Value?.ToString();
            if (row.Cells["DateNaissance"].Value != DBNull.Value && row.Cells["DateNaissance"].Value != null)
                dateTimePicker1.Value = Convert.ToDateTime(row.Cells["DateNaissance"].Value);
        }

        private void BtnAjouter_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text))
            { MessageBox.Show("Nom et Espece obligatoires.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            int idProprio = ChoisirProprietaire();
            if (idProprio < 0) return;

            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        "INSERT INTO Animaux(IdProprietaire,Nom,Espece,Race,Sexe,DateNaissance,Actif) VALUES(@ip,@n,@es,@r,@sx,@dn,1)", conn);
                    cmd.Parameters.AddWithValue("@ip", idProprio);
                    cmd.Parameters.AddWithValue("@n", textBox1.Text.Trim());
                    cmd.Parameters.AddWithValue("@es", textBox2.Text.Trim());
                    cmd.Parameters.AddWithValue("@r", Val(textBox3.Text));
                    cmd.Parameters.AddWithValue("@sx", Val(comboBox1.Text));
                    cmd.Parameters.AddWithValue("@dn", dateTimePicker1.Value.Date);
                    cmd.ExecuteNonQuery();
                }
                ViderChamps(); ChargerAnimaux();
                MessageBox.Show("Animal ajoute.", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private void BtnModifier_Click(object sender, EventArgs e)
        {
            if (_idSelectionne < 0) { MessageBox.Show("Selectionnez un animal."); return; }
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        "UPDATE Animaux SET Nom=@n,Espece=@es,Race=@r,Sexe=@sx,DateNaissance=@dn WHERE IdAnimal=@id", conn);
                    cmd.Parameters.AddWithValue("@n", textBox1.Text.Trim());
                    cmd.Parameters.AddWithValue("@es", textBox2.Text.Trim());
                    cmd.Parameters.AddWithValue("@r", Val(textBox3.Text));
                    cmd.Parameters.AddWithValue("@sx", Val(comboBox1.Text));
                    cmd.Parameters.AddWithValue("@dn", dateTimePicker1.Value.Date);
                    cmd.Parameters.AddWithValue("@id", _idSelectionne);
                    cmd.ExecuteNonQuery();
                }
                ViderChamps(); ChargerAnimaux();
                MessageBox.Show("Animal modifie.", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private void BtnSupprimer_Click(object sender, EventArgs e)
        {
            if (_idSelectionne < 0) { MessageBox.Show("Selectionnez un animal."); return; }
            if (MessageBox.Show("Supprimer ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    new SqlCommand("UPDATE Animaux SET Actif=0 WHERE IdAnimal=@id", conn)
                    { Parameters = { new SqlParameter("@id", _idSelectionne) } }.ExecuteNonQuery();
                }
                ViderChamps(); ChargerAnimaux();
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private void BtnRechercher_Click(object sender, EventArgs e) => ChargerAnimaux(textBox6.Text.Trim());

        // ── Sélection propriétaire dans une mini-fenêtre ───────────────
        private int ChoisirProprietaire()
        {
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        "SELECT IdProprietaire, Nom+' '+Prenom AS NomComplet FROM Proprietaires ORDER BY Nom", conn);
                    var dt = new System.Data.DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);

                    var f = new Form { Text = "Choisir un proprietaire", Size = new Size(340, 180), StartPosition = FormStartPosition.CenterParent };
                    var cmb = new ComboBox { Location = new Point(20, 30), Size = new Size(280, 28), DropDownStyle = ComboBoxStyle.DropDownList, DataSource = dt, DisplayMember = "NomComplet", ValueMember = "IdProprietaire" };
                    var btn = new Button { Text = "OK", Location = new Point(120, 80), Size = new Size(80, 30), DialogResult = DialogResult.OK };
                    f.Controls.AddRange(new Control[] { cmb, btn });
                    f.AcceptButton = btn;

                    if (f.ShowDialog() == DialogResult.OK)
                        return Convert.ToInt32(cmb.SelectedValue);
                }
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
            return -1;
        }

        private void ViderChamps()
        {
            textBox1.Clear(); textBox2.Clear(); textBox3.Clear(); textBox6.Clear();
            comboBox1.SelectedIndex = -1;
            dateTimePicker1.Value = DateTime.Today;
            _idSelectionne = -1;
        }

        private object Val(string s) => string.IsNullOrWhiteSpace(s) ? (object)DBNull.Value : s.Trim();
    }
}