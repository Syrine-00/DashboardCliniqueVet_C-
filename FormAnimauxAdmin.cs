using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CliniqueVet
{
    public partial class FormAnimauxAdmin : Form
    {
        private readonly int _idUser;
        private string ConnStr => System.Configuration.ConfigurationManager
            .ConnectionStrings["CliniqueVet"].ConnectionString;

        public FormAnimauxAdmin(int idUser)
        {
            InitializeComponent();
            _idUser = idUser;
        }

        private void FormAnimauxAdmin_Load(object sender, EventArgs e)
        {
            button1.Click += (s, ev) => { var f = new FormDashboard(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button2.Click += (s, ev) => { var f = new FormUtilisateurs(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button3.Click += (s, ev) => { var f = new FormAnimauxAdmin(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button4.Click += (s, ev) => { var f = new FormMedicAdmin(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button5.Click += (s, ev) => { var f = new FormFactureAdmin(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            btnDeco.Click += (s, ev) => { var f = new FormLogin(); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };

            button6.Click += (s, ev) => ChargerAnimaux(textBox1.Text);
            button7.Click += BtnAjouter_Click;
            button8.Click += BtnModifier_Click;
            button9.Click += BtnVoirFiche_Click;

            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            comboBox1.Items.AddRange(new string[] { "Tous", "Chien", "Chat", "Lapin", "Oiseau", "Autre" });
            comboBox1.SelectedIndex = 0;
            comboBox1.SelectedIndexChanged += (s, ev) => ChargerAnimaux(textBox1.Text);

            ChargerAnimaux("");
        }

        private void ChargerAnimaux(string filtre)
        {
            try
            {
                string espece = comboBox1.SelectedItem?.ToString() ?? "Tous";
                using (var conn = new SqlConnection(ConnStr))
                {
                    var cmd = new SqlCommand(
                        @"SELECT a.IdAnimal AS ID, a.Nom, a.Espece, a.Race, a.Sexe,
                                 p.Nom + ' ' + p.Prenom AS Proprietaire
                          FROM Animaux a
                          JOIN Proprietaires p ON a.IdProprietaire = p.IdProprietaire
                          WHERE a.Actif=1
                            AND (@f='' OR a.Nom LIKE @f OR p.Nom LIKE @f)
                            AND (@e='Tous' OR a.Espece=@e)
                          ORDER BY a.Nom", conn);
                    cmd.Parameters.AddWithValue("@f", "%" + filtre.Trim() + "%");
                    cmd.Parameters.AddWithValue("@e", espece);
                    conn.Open();
                    var dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);
                    dataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private int GetSelectedId()
        {
            if (dataGridView1.SelectedRows.Count == 0) { MessageBox.Show("Sélectionnez un animal."); return -1; }
            return Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);
        }

        private void BtnAjouter_Click(object sender, EventArgs e)
        {
            var dlg = new Form { Text = "Ajouter un animal", Size = new System.Drawing.Size(380, 300), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog };
            int y = 20;
            var txtNom = Ch(dlg, "Nom *", ref y);
            var txtEspece = Ch(dlg, "Espèce *", ref y);
            var txtRace = Ch(dlg, "Race", ref y);
            dlg.Controls.Add(new Label { Text = "ID Proprio * :", AutoSize = true, Location = new System.Drawing.Point(20, y) });
            var txtProprio = new TextBox { Location = new System.Drawing.Point(160, y - 3), Size = new System.Drawing.Size(180, 26) };
            dlg.Controls.Add(txtProprio); y += 40;
            var btnOk = new Button { Text = "Enregistrer", Location = new System.Drawing.Point(100, y), Size = new System.Drawing.Size(150, 35), BackColor = Color.Teal, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK };
            btnOk.FlatAppearance.BorderSize = 0; dlg.Controls.Add(btnOk); dlg.AcceptButton = btnOk;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var conn = new SqlConnection(ConnStr))
                    {
                        var cmd = new SqlCommand(
                            "INSERT INTO Animaux (Nom,Espece,Race,IdProprietaire,Actif) VALUES(@n,@e,@r,@p,1)", conn);
                        cmd.Parameters.AddWithValue("@n", txtNom.Text.Trim());
                        cmd.Parameters.AddWithValue("@e", txtEspece.Text.Trim());
                        cmd.Parameters.AddWithValue("@r", string.IsNullOrWhiteSpace(txtRace.Text) ? (object)DBNull.Value : txtRace.Text.Trim());
                        cmd.Parameters.AddWithValue("@p", int.Parse(txtProprio.Text.Trim()));
                        conn.Open(); cmd.ExecuteNonQuery();
                    }
                    ChargerAnimaux("");
                }
                catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
            }
        }

        private void BtnModifier_Click(object sender, EventArgs e)
        {
            int id = GetSelectedId();
            if (id == -1) return;
            var row = dataGridView1.SelectedRows[0];

            var dlg = new Form { Text = "Modifier", Size = new System.Drawing.Size(380, 240), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog };
            int y = 20;
            var txtNom = Ch(dlg, "Nom *", ref y, row.Cells["Nom"].Value.ToString());
            var txtEspece = Ch(dlg, "Espèce *", ref y, row.Cells["Espece"].Value.ToString());
            var txtRace = Ch(dlg, "Race", ref y, row.Cells["Race"].Value?.ToString() ?? "");
            var btnOk = new Button { Text = "Enregistrer", Location = new System.Drawing.Point(100, y), Size = new System.Drawing.Size(150, 35), BackColor = Color.Teal, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK };
            btnOk.FlatAppearance.BorderSize = 0; dlg.Controls.Add(btnOk); dlg.AcceptButton = btnOk;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var conn = new SqlConnection(ConnStr))
                    {
                        var cmd = new SqlCommand("UPDATE Animaux SET Nom=@n,Espece=@e,Race=@r WHERE IdAnimal=@id", conn);
                        cmd.Parameters.AddWithValue("@n", txtNom.Text.Trim());
                        cmd.Parameters.AddWithValue("@e", txtEspece.Text.Trim());
                        cmd.Parameters.AddWithValue("@r", string.IsNullOrWhiteSpace(txtRace.Text) ? (object)DBNull.Value : txtRace.Text.Trim());
                        cmd.Parameters.AddWithValue("@id", id);
                        conn.Open(); cmd.ExecuteNonQuery();
                    }
                    ChargerAnimaux("");
                }
                catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
            }
        }

        private void BtnVoirFiche_Click(object sender, EventArgs e)
        {
            int id = GetSelectedId();
            if (id == -1) return;
            string nom = dataGridView1.SelectedRows[0].Cells["Nom"].Value.ToString();

            var dlg = new Form { Text = "Fiche - " + nom, Size = new System.Drawing.Size(700, 420), StartPosition = FormStartPosition.CenterParent };
            var dgvH = new DataGridView { Location = new System.Drawing.Point(10, 10), Size = new System.Drawing.Size(660, 350), ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White, RowHeadersVisible = false };
            dgvH.ColumnHeadersDefaultCellStyle.BackColor = Color.Teal;
            dgvH.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dlg.Controls.Add(dgvH);

            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    var cmd = new SqlCommand(
                        @"SELECT c.DateConsultation AS Date, c.Diagnostic, c.Traitement,
                                 u.Nom + ' ' + u.Prenom AS Veterinaire
                          FROM Consultations c
                          JOIN Utilisateurs u ON c.IdVeterinaire = u.IdUtilisateur
                          WHERE c.IdAnimal = @id ORDER BY c.DateConsultation DESC", conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    conn.Open();
                    var dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);
                    dgvH.DataSource = dt;
                }
                dlg.ShowDialog();
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private TextBox Ch(Form dlg, string label, ref int y, string val = "")
        {
            dlg.Controls.Add(new Label { Text = label + " :", AutoSize = true, Location = new System.Drawing.Point(20, y) });
            var txt = new TextBox { Location = new System.Drawing.Point(160, y - 3), Size = new System.Drawing.Size(180, 26), Text = val };
            dlg.Controls.Add(txt); y += 40; return txt;
        }
    }
}