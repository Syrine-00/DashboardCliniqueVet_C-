using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace CliniqueVet
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
            ConfigurerUI();
        }

        private void ConfigurerUI()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            lblLogin.AutoSize = true;
            txtPwd.PasswordChar = '*';

            btnConnexion.Click += BtnConnexion_Click;
            this.AcceptButton = btnConnexion;
        }

        private void BtnConnexion_Click(object sender, EventArgs e)
        {
            label1.Text = "";
            string login = txtLogin.Text.Trim();
            string pwd = txtPwd.Text.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(pwd))
            {
                label1.Text = "Veuillez remplir tous les champs.";
                return;
            }

            try
            {
                string connStr = System.Configuration.ConfigurationManager
                    .ConnectionStrings["CliniqueVet"].ConnectionString;

                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        "SELECT IdUtilisateur, Nom, Prenom, Role FROM Utilisateurs " +
                        "WHERE Login=@l AND MotDePasse=@p AND Actif=1", conn);
                    cmd.Parameters.AddWithValue("@l", login);
                    cmd.Parameters.AddWithValue("@p", HashPassword(pwd)); // hash comme dans Register

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            label1.Text = "Identifiant ou mot de passe incorrect.";
                            txtPwd.Clear();
                            return;
                        }

                        int id = (int)reader["IdUtilisateur"];
                        string nom = reader["Nom"].ToString();
                        string prenom = reader["Prenom"].ToString();
                        string role = reader["Role"].ToString();
                        reader.Close();

                        Form mainForm = null;
                        if (role == "Secretaire") mainForm = new FormProprio();
                        else if (role == "Veterinaire") mainForm = new FormPlanning(id);
                        else if (role == "Administrateur") mainForm = new FormDashboard(id);

                        if (mainForm != null)
                        {
                            this.Hide();
                            mainForm.FormClosed += (s, a) => this.Close();
                            mainForm.Show();
                        }
                        else
                        {
                            label1.Text = "Role inconnu : " + role;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                label1.Text = "Erreur BD : " + ex.Message;
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

        private void FormLogin_Load(object sender, EventArgs e)
        {
        }
    }
}