using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace CliniqueVet
{
    public partial class Form1 : Form
    {
        private string ConnStr => System.Configuration.ConfigurationManager
            .ConnectionStrings["CliniqueVet"].ConnectionString;

        public Form1()
        {
            InitializeComponent();
            ConfigurerUI();
        }

        private void ConfigurerUI()
        {
            this.Text = "VetClinic - Créer un compte";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            pnlHeader.BackColor = Color.FromArgb(0, 120, 212);

            lblTitre.Text = "Créer un compte";
            lblTitre.Font = new Font("Segoe UI", 18f, FontStyle.Bold);
            lblTitre.ForeColor = Color.White;
            lblTitre.Parent = pnlHeader;
            lblTitre.Location = new Point(20, 15);

            lblSousTitre.Text = "Remplissez les informations ci-dessous";
            lblSousTitre.Font = new Font("Segoe UI", 9f);
            lblSousTitre.ForeColor = Color.FromArgb(200, 230, 255);
            lblSousTitre.Parent = pnlHeader;
            lblSousTitre.Location = new Point(22, 60);

            lblNom.Text = "Nom *";
            lblNom.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblNom.ForeColor = Color.FromArgb(60, 60, 60);

            lblPrenom.Text = "Prénom *";
            lblPrenom.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblPrenom.ForeColor = Color.FromArgb(60, 60, 60);

            lblTelephone.Text = "Téléphone";
            lblTelephone.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblTelephone.ForeColor = Color.FromArgb(60, 60, 60);

            lblEmail.Text = "Email";
            lblEmail.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblEmail.ForeColor = Color.FromArgb(60, 60, 60);

            lblLogin.Text = "Identifiant *";
            lblLogin.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblLogin.ForeColor = Color.FromArgb(60, 60, 60);

            lblPwd.Text = "Mot de passe *";
            lblPwd.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblPwd.ForeColor = Color.FromArgb(60, 60, 60);

            lblConfirm.Text = "Confirmer mot de passe *";
            lblConfirm.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblConfirm.ForeColor = Color.FromArgb(60, 60, 60);

            // AJOUT - Label Rôle
            lblRole.Text = "Rôle *";
            lblRole.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblRole.ForeColor = Color.FromArgb(60, 60, 60);

            // AJOUT - ComboBox Rôle
            cmbRole.Items.AddRange(new string[] { "Secretaire", "Veterinaire", "Administrateur" });
            cmbRole.SelectedIndex = 0;
            cmbRole.Font = new Font("Segoe UI", 9f);
            cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;

            txtPwd.PasswordChar = '*';
            txtConfirm.PasswordChar = '*';

            lblMessage.Text = "";
            lblMessage.ForeColor = Color.Red;
            lblMessage.Font = new Font("Segoe UI", 9f);
            lblMessage.AutoSize = false;
            lblMessage.Size = new Size(300, 22);

            btnInscrire.Text = "Créer le compte";
            btnInscrire.BackColor = Color.FromArgb(0, 120, 212);
            btnInscrire.ForeColor = Color.White;
            btnInscrire.FlatStyle = FlatStyle.Flat;
            btnInscrire.FlatAppearance.BorderSize = 0;
            btnInscrire.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnInscrire.Cursor = Cursors.Hand;
            btnInscrire.Click += BtnInscrire_Click;

            btnAnnuler.Text = "Annuler";
            btnAnnuler.BackColor = Color.FromArgb(220, 220, 220);
            btnAnnuler.ForeColor = Color.FromArgb(60, 60, 60);
            btnAnnuler.FlatStyle = FlatStyle.Flat;
            btnAnnuler.FlatAppearance.BorderSize = 0;
            btnAnnuler.Font = new Font("Segoe UI", 10f);
            btnAnnuler.Cursor = Cursors.Hand;
            btnAnnuler.Click += (s, e) => this.Close();

            this.AcceptButton = btnInscrire;
        }

        private void BtnInscrire_Click(object sender, EventArgs e)
        {
            lblMessage.ForeColor = Color.Red;
            lblMessage.Text = "";

            if (string.IsNullOrWhiteSpace(txtNom.Text) ||
                string.IsNullOrWhiteSpace(txtPrenom.Text) ||
                string.IsNullOrWhiteSpace(txtLogin.Text) ||
                string.IsNullOrWhiteSpace(txtPwd.Text))
            {
                lblMessage.Text = "Les champs marqués * sont obligatoires.";
                return;
            }

            if (txtPwd.Text.Length < 6)
            {
                lblMessage.Text = "Mot de passe : 6 caractères minimum.";
                txtPwd.Focus();
                return;
            }

            if (txtPwd.Text != txtConfirm.Text)
            {
                lblMessage.Text = "Les mots de passe ne correspondent pas.";
                txtConfirm.Clear();
                txtConfirm.Focus();
                return;
            }

            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();

                    var cmdCheck = new SqlCommand(
                        "SELECT COUNT(*) FROM Utilisateurs WHERE Login = @l", conn);
                    cmdCheck.Parameters.AddWithValue("@l", txtLogin.Text.Trim());
                    if ((int)cmdCheck.ExecuteScalar() > 0)
                    {
                        lblMessage.Text = "Cet identifiant est déjà utilisé.";
                        txtLogin.Focus();
                        return;
                    }

                    string hashed = HashPassword(txtPwd.Text);
                    string roleChoisi = cmbRole.SelectedItem.ToString(); // AJOUT
                    var cmd = new SqlCommand(
                        @"INSERT INTO Utilisateurs (Nom, Prenom, Login, MotDePasse, Role, Telephone, Email, Actif)
                          VALUES (@nom, @prenom, @login, @pwd, @role, @tel, @email, 1)", conn);
                    cmd.Parameters.AddWithValue("@nom", txtNom.Text.Trim());
                    cmd.Parameters.AddWithValue("@prenom", txtPrenom.Text.Trim());
                    cmd.Parameters.AddWithValue("@login", txtLogin.Text.Trim());
                    cmd.Parameters.AddWithValue("@pwd", hashed);
                    cmd.Parameters.AddWithValue("@role", roleChoisi); // AJOUT
                    cmd.Parameters.AddWithValue("@tel",
                        string.IsNullOrWhiteSpace(txtTelephone.Text)
                        ? (object)DBNull.Value : txtTelephone.Text.Trim());
                    cmd.Parameters.AddWithValue("@email",
                        string.IsNullOrWhiteSpace(txtEmail.Text)
                        ? (object)DBNull.Value : txtEmail.Text.Trim());
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show(
                    "Compte créé avec succès !\n\n" +
                    "Identifiant : " + txtLogin.Text.Trim() + "\n" +
                    "Rôle : " + cmbRole.SelectedItem.ToString() + "\n\n" + // AJOUT
                    "Vous pouvez maintenant vous connecter.",
                    "Inscription réussie",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Erreur : " + ex.Message;
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

        private void lblTitre_Click(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }

        private void Form1_Load(object sender, EventArgs e) { }
    }
}