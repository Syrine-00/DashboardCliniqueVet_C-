using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CliniqueVet
{
    public partial class FormMedicAdmin : Form
    {
        private readonly int _idUser;
        private string ConnStr => System.Configuration.ConfigurationManager
            .ConnectionStrings["CliniqueVet"].ConnectionString;

        public FormMedicAdmin(int idUser)
        {
            InitializeComponent();
            _idUser = idUser;
        }

        private void FormMedicAdmin_Load(object sender, EventArgs e)
        {
            button1.Click += (s, ev) => { var f = new FormDashboard(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button2.Click += (s, ev) => { var f = new FormUtilisateurs(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button3.Click += (s, ev) => { var f = new FormAnimauxAdmin(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button4.Click += (s, ev) => { var f = new FormMedicAdmin(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button5.Click += (s, ev) => { var f = new FormFactureAdmin(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            btnDeco.Click += (s, ev) => { var f = new FormLogin(); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };

            button7.Click += BtnAjouter_Click;
            button8.Click += BtnModifier_Click;
            button9.Click += BtnSupprimer_Click;

            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            ChargerMedicaments();
        }

        private void ChargerMedicaments()
        {
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    var cmd = new SqlCommand(
                        @"SELECT IdMedicament AS ID, Nom, Unite, QuantiteStock, StockMinimum,
                                 PrixUnitaire, DateExpiration
                          FROM Medicaments ORDER BY Nom", conn);
                    conn.Open();
                    var dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);
                    dataGridView1.DataSource = dt;
                }

                int alertes = 0;
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow) continue;
                    bool stockFaible = false;
                    bool expirationProche = false;

                    int qte = Convert.ToInt32(row.Cells["QuantiteStock"].Value);
                    int min = Convert.ToInt32(row.Cells["StockMinimum"].Value);
                    if (qte <= min) stockFaible = true;

                    if (row.Cells["DateExpiration"].Value != null && row.Cells["DateExpiration"].Value != DBNull.Value)
                    {
                        DateTime exp = Convert.ToDateTime(row.Cells["DateExpiration"].Value);
                        if (exp <= DateTime.Today.AddDays(30)) expirationProche = true;
                    }

                    if (stockFaible || expirationProche)
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 210, 210);
                        row.DefaultCellStyle.ForeColor = Color.DarkRed;
                        alertes++;
                    }
                }

                // Afficher le résumé dans le label txtStock
                if (alertes > 0)
                    txtStock.Text = alertes + " alerte(s) : stock insuffisant ou expiration proche (30 jours)";
                else
                    txtStock.Text = "Aucune alerte de stock.";
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e) { }

        private int GetSelectedId()
        {
            if (dataGridView1.SelectedRows.Count == 0) { MessageBox.Show("Sélectionnez un médicament."); return -1; }
            return Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);
        }

        private void BtnAjouter_Click(object sender, EventArgs e)
        {
            var dlg = new Form { Text = "Ajouter médicament", Size = new System.Drawing.Size(380, 340), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog };
            int y = 20;
            var txtNom = Ch(dlg, "Nom *", ref y);
            var txtUnite = Ch(dlg, "Unité *", ref y, "comprimé");
            var txtQte = Ch(dlg, "Quantité *", ref y, "0");
            var txtMin = Ch(dlg, "Stock min *", ref y, "10");
            var txtPrix = Ch(dlg, "Prix unitaire *", ref y, "0.00");

            dlg.Controls.Add(new Label { Text = "Date exp. :", AutoSize = true, Location = new System.Drawing.Point(20, y) });
            var dtp = new DateTimePicker { Location = new System.Drawing.Point(160, y - 3), Size = new System.Drawing.Size(180, 26), Format = DateTimePickerFormat.Short };
            dlg.Controls.Add(dtp); y += 40;

            var btnOk = new Button { Text = "Enregistrer", Location = new System.Drawing.Point(100, y), Size = new System.Drawing.Size(150, 35), BackColor = Color.Teal, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK };
            btnOk.FlatAppearance.BorderSize = 0; dlg.Controls.Add(btnOk); dlg.AcceptButton = btnOk;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var conn = new SqlConnection(ConnStr))
                    {
                        var cmd = new SqlCommand(
                            "INSERT INTO Medicaments (Nom,Unite,QuantiteStock,StockMinimum,PrixUnitaire,DateExpiration) VALUES(@n,@u,@q,@m,@p,@d)", conn);
                        cmd.Parameters.AddWithValue("@n", txtNom.Text.Trim());
                        cmd.Parameters.AddWithValue("@u", txtUnite.Text.Trim());
                        cmd.Parameters.AddWithValue("@q", int.Parse(txtQte.Text.Trim()));
                        cmd.Parameters.AddWithValue("@m", int.Parse(txtMin.Text.Trim()));
                        cmd.Parameters.AddWithValue("@p", decimal.Parse(txtPrix.Text.Trim()));
                        cmd.Parameters.AddWithValue("@d", dtp.Value.Date);
                        conn.Open(); cmd.ExecuteNonQuery();
                    }
                    ChargerMedicaments();
                }
                catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
            }
        }

        private void BtnModifier_Click(object sender, EventArgs e)
        {
            int id = GetSelectedId();
            if (id == -1) return;
            var row = dataGridView1.SelectedRows[0];

            var dlg = new Form { Text = "Modifier médicament", Size = new System.Drawing.Size(380, 340), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog };
            int y = 20;
            var txtNom = Ch(dlg, "Nom *", ref y, row.Cells["Nom"].Value.ToString());
            var txtUnite = Ch(dlg, "Unité *", ref y, row.Cells["Unite"].Value.ToString());
            var txtQte = Ch(dlg, "Quantité *", ref y, row.Cells["QuantiteStock"].Value.ToString());
            var txtMin = Ch(dlg, "Stock min *", ref y, row.Cells["StockMinimum"].Value.ToString());
            var txtPrix = Ch(dlg, "Prix unitaire *", ref y, row.Cells["PrixUnitaire"].Value.ToString());

            dlg.Controls.Add(new Label { Text = "Date exp. :", AutoSize = true, Location = new System.Drawing.Point(20, y) });
            var dtp = new DateTimePicker { Location = new System.Drawing.Point(160, y - 3), Size = new System.Drawing.Size(180, 26), Format = DateTimePickerFormat.Short };
            if (row.Cells["DateExpiration"].Value != null && row.Cells["DateExpiration"].Value != DBNull.Value)
                dtp.Value = Convert.ToDateTime(row.Cells["DateExpiration"].Value);
            dlg.Controls.Add(dtp); y += 40;

            var btnOk = new Button { Text = "Enregistrer", Location = new System.Drawing.Point(100, y), Size = new System.Drawing.Size(150, 35), BackColor = Color.Teal, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, DialogResult = DialogResult.OK };
            btnOk.FlatAppearance.BorderSize = 0; dlg.Controls.Add(btnOk); dlg.AcceptButton = btnOk;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var conn = new SqlConnection(ConnStr))
                    {
                        var cmd = new SqlCommand(
                            "UPDATE Medicaments SET Nom=@n,Unite=@u,QuantiteStock=@q,StockMinimum=@m,PrixUnitaire=@p,DateExpiration=@d WHERE IdMedicament=@id", conn);
                        cmd.Parameters.AddWithValue("@n", txtNom.Text.Trim());
                        cmd.Parameters.AddWithValue("@u", txtUnite.Text.Trim());
                        cmd.Parameters.AddWithValue("@q", int.Parse(txtQte.Text.Trim()));
                        cmd.Parameters.AddWithValue("@m", int.Parse(txtMin.Text.Trim()));
                        cmd.Parameters.AddWithValue("@p", decimal.Parse(txtPrix.Text.Trim()));
                        cmd.Parameters.AddWithValue("@d", dtp.Value.Date);
                        cmd.Parameters.AddWithValue("@id", id);
                        conn.Open(); cmd.ExecuteNonQuery();
                    }
                    ChargerMedicaments();
                }
                catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
            }
        }

        private void BtnSupprimer_Click(object sender, EventArgs e)
        {
            int id = GetSelectedId();
            if (id == -1) return;
            if (MessageBox.Show("Supprimer ce médicament ?", "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    using (var conn = new SqlConnection(ConnStr))
                    {
                        var cmd = new SqlCommand("DELETE FROM Medicaments WHERE IdMedicament=@id", conn);
                        cmd.Parameters.AddWithValue("@id", id);
                        conn.Open(); cmd.ExecuteNonQuery();
                    }
                    ChargerMedicaments();
                }
                catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
            }
        }

        private TextBox Ch(Form dlg, string label, ref int y, string val = "")
        {
            dlg.Controls.Add(new Label { Text = label + " :", AutoSize = true, Location = new System.Drawing.Point(20, y) });
            var txt = new TextBox { Location = new System.Drawing.Point(160, y - 3), Size = new System.Drawing.Size(180, 26), Text = val };
            dlg.Controls.Add(txt); y += 40; return txt;
        }
    }
}