using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CliniqueVet
{
    public partial class FormProprio : Form
    {
        private string ConnStr => System.Configuration.ConfigurationManager
            .ConnectionStrings["CliniqueVet"].ConnectionString;

        private int _idSelectionne = -1;

        public FormProprio()
        {
            InitializeComponent();
        }

        // ── Requis par le Designer ─────────────────────────────────────
        private void FormProprio_Load(object sender, EventArgs e)
        {
            ConfigurerUI();
            ChargerProprietaires();
        }

        private void ConfigurerUI()
        {
            label2.Text = "Nom:";
            label3.Text = "Prenom:";
            label4.Text = "Telephone:";
            label5.Text = "Email:";
            label6.Text = "Adresse:";
            label7.Text = "Gestion Proprietaires";

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

        private void ChargerProprietaires(string filtre = "")
        {
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    string sql = "SELECT IdProprietaire, Nom, Prenom, Telephone, Email, Adresse FROM Proprietaires";
                    if (!string.IsNullOrWhiteSpace(filtre))
                        sql += " WHERE Nom LIKE @f OR Prenom LIKE @f OR Telephone LIKE @f";
                    sql += " ORDER BY Nom, Prenom";

                    var cmd = new SqlCommand(sql, conn);
                    if (!string.IsNullOrWhiteSpace(filtre))
                        cmd.Parameters.AddWithValue("@f", "%" + filtre + "%");

                    var dt = new System.Data.DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);
                    dataGridView1.DataSource = dt;

                    if (dataGridView1.Columns["IdProprietaire"] != null)
                        dataGridView1.Columns["IdProprietaire"].Visible = false;
                }
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dataGridView1.Rows[e.RowIndex];
            _idSelectionne = Convert.ToInt32(row.Cells["IdProprietaire"].Value);
            textBox1.Text = row.Cells["Nom"].Value?.ToString();
            textBox2.Text = row.Cells["Prenom"].Value?.ToString();
            textBox3.Text = row.Cells["Telephone"].Value?.ToString();
            textBox4.Text = row.Cells["Email"].Value?.ToString();
            textBox5.Text = row.Cells["Adresse"].Value?.ToString();
        }

        private void BtnAjouter_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text))
            { MessageBox.Show("Nom et Prenom obligatoires.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        "INSERT INTO Proprietaires(Nom,Prenom,Telephone,Email,Adresse) VALUES(@n,@p,@t,@e,@a)", conn);
                    cmd.Parameters.AddWithValue("@n", textBox1.Text.Trim());
                    cmd.Parameters.AddWithValue("@p", textBox2.Text.Trim());
                    cmd.Parameters.AddWithValue("@t", Val(textBox3.Text));
                    cmd.Parameters.AddWithValue("@e", Val(textBox4.Text));
                    cmd.Parameters.AddWithValue("@a", Val(textBox5.Text));
                    cmd.ExecuteNonQuery();
                }
                ViderChamps(); ChargerProprietaires();
                MessageBox.Show("Proprietaire ajoute.", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private void BtnModifier_Click(object sender, EventArgs e)
        {
            if (_idSelectionne < 0) { MessageBox.Show("Selectionnez un proprietaire."); return; }
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        "UPDATE Proprietaires SET Nom=@n,Prenom=@p,Telephone=@t,Email=@e,Adresse=@a WHERE IdProprietaire=@id", conn);
                    cmd.Parameters.AddWithValue("@n", textBox1.Text.Trim());
                    cmd.Parameters.AddWithValue("@p", textBox2.Text.Trim());
                    cmd.Parameters.AddWithValue("@t", Val(textBox3.Text));
                    cmd.Parameters.AddWithValue("@e", Val(textBox4.Text));
                    cmd.Parameters.AddWithValue("@a", Val(textBox5.Text));
                    cmd.Parameters.AddWithValue("@id", _idSelectionne);
                    cmd.ExecuteNonQuery();
                }
                ViderChamps(); ChargerProprietaires();
                MessageBox.Show("Proprietaire modifie.", "Succes", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private void BtnSupprimer_Click(object sender, EventArgs e)
        {
            if (_idSelectionne < 0) { MessageBox.Show("Selectionnez un proprietaire."); return; }
            if (MessageBox.Show("Supprimer ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    new SqlCommand("DELETE FROM Proprietaires WHERE IdProprietaire=@id", conn)
                    { Parameters = { new SqlParameter("@id", _idSelectionne) } }.ExecuteNonQuery();
                }
                ViderChamps(); ChargerProprietaires();
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private void BtnRechercher_Click(object sender, EventArgs e) => ChargerProprietaires(textBox6.Text.Trim());

        private void ViderChamps()
        {
            textBox1.Clear(); textBox2.Clear(); textBox3.Clear();
            textBox4.Clear(); textBox5.Clear(); textBox6.Clear();
            _idSelectionne = -1;
        }

        private object Val(string s) => string.IsNullOrWhiteSpace(s) ? (object)DBNull.Value : s.Trim();

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }
    }
}