using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace CliniqueVet
{
    public partial class FormUtilisateurs : Form
    {
        private readonly int _idUser;
        private string ConnStr => System.Configuration.ConfigurationManager
            .ConnectionStrings["CliniqueVet"].ConnectionString;

        private DataGridView dgv;
        private Label lblStatus;

        public FormUtilisateurs(int idUser)
        {
            InitializeComponent();
            _idUser = idUser;
        }

        private void FormUtilisateurs_Load(object sender, EventArgs e)
        {
            button1.Click += (s, ev) => { var f = new FormDashboard(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button2.Click += (s, ev) => { var f = new FormUtilisateurs(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button3.Click += (s, ev) => { var f = new FormAnimauxAdmin(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button4.Click += (s, ev) => { var f = new FormMedicAdmin(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button5.Click += (s, ev) => { var f = new FormFactureAdmin(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            btnDeco.Click += (s, ev) => { var f = new FormLogin(); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };

            button6.Click += (s, ev) => ChargerUtilisateurs(textBox1.Text);
            button7.Click += BtnAjouter_Click;
            button8.Click += BtnModifier_Click;
            button9.Click += BtnDesactiver_Click;

            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            ChargerUtilisateurs("");
        }

        private void ChargerUtilisateurs(string filtre)
        {
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    var cmd = new SqlCommand(
                        @"SELECT IdUtilisateur AS ID, Nom, Prenom, Login, Role,
                                 CASE WHEN Actif=1 THEN 'Actif' ELSE 'Inactif' END AS Statut
                          FROM Utilisateurs
                          WHERE (@f='' OR Nom LIKE @f OR Prenom LIKE @f OR Login LIKE @f)
                          ORDER BY Nom", conn);
                    cmd.Parameters.AddWithValue("@f", "%" + filtre.Trim() + "%");
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
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Sélectionnez un utilisateur.");
                return -1;
            }
            return Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);
        }

        private void BtnAjouter_Click(object sender, EventArgs e)
        {
            var f = new Form1();
            f.ShowDialog();
            ChargerUtilisateurs("");
        }

        private void BtnModifier_Click(object sender, EventArgs e)
        {
            int id = GetSelectedId();
            if (id == -1) return;
            var row = dataGridView1.SelectedRows[0];

            var dlg = new Form { Text = "Modifier", Size = new System.Drawing.Size(350, 260), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog };
            var txtNom = new TextBox { Text = row.Cells["Nom"].Value.ToString(), Location = new System.Drawing.Point(120, 20), Size = new System.Drawing.Size(180, 26) };
            var txtPrenom = new TextBox { Text = row.Cells["Prenom"].Value.ToString(), Location = new System.Drawing.Point(120, 60), Size = new System.Drawing.Size(180, 26) };
            var cmbRole = new ComboBox { Location = new System.Drawing.Point(120, 100), Size = new System.Drawing.Size(180, 26), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRole.Items.AddRange(new string[] { "Secretaire", "Veterinaire", "Administrateur" });
            cmbRole.SelectedItem = row.Cells["Role"].Value.ToString();
            dlg.Controls.Add(new Label { Text = "Nom :", AutoSize = true, Location = new System.Drawing.Point(20, 23) });
            dlg.Controls.Add(new Label { Text = "Prénom :", AutoSize = true, Location = new System.Drawing.Point(20, 63) });
            dlg.Controls.Add(new Label { Text = "Rôle :", AutoSize = true, Location = new System.Drawing.Point(20, 103) });
            dlg.Controls.Add(txtNom); dlg.Controls.Add(txtPrenom); dlg.Controls.Add(cmbRole);
            var btnOk = new Button { Text = "Enregistrer", Location = new System.Drawing.Point(80, 150), Size = new System.Drawing.Size(120, 35), BackColor = Color.Teal, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK };
            btnOk.FlatAppearance.BorderSize = 0;
            dlg.Controls.Add(btnOk); dlg.AcceptButton = btnOk;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var conn = new SqlConnection(ConnStr))
                    {
                        var cmd = new SqlCommand("UPDATE Utilisateurs SET Nom=@n,Prenom=@p,Role=@r WHERE IdUtilisateur=@id", conn);
                        cmd.Parameters.AddWithValue("@n", txtNom.Text.Trim());
                        cmd.Parameters.AddWithValue("@p", txtPrenom.Text.Trim());
                        cmd.Parameters.AddWithValue("@r", cmbRole.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@id", id);
                        conn.Open(); cmd.ExecuteNonQuery();
                    }
                    ChargerUtilisateurs("");
                }
                catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
            }
        }

        private void BtnDesactiver_Click(object sender, EventArgs e)
        {
            int id = GetSelectedId();
            if (id == -1) return;
            bool actif = dataGridView1.SelectedRows[0].Cells["Statut"].Value.ToString() == "Actif";
            if (MessageBox.Show($"Voulez-vous {(actif ? "désactiver" : "réactiver")} cet utilisateur ?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    using (var conn = new SqlConnection(ConnStr))
                    {
                        var cmd = new SqlCommand("UPDATE Utilisateurs SET Actif=@a WHERE IdUtilisateur=@id", conn);
                        cmd.Parameters.AddWithValue("@a", actif ? 0 : 1);
                        cmd.Parameters.AddWithValue("@id", id);
                        conn.Open(); cmd.ExecuteNonQuery();
                    }
                    ChargerUtilisateurs("");
                }
                catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
            }
        }

        private string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                var sb = new StringBuilder();
                foreach (var b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}