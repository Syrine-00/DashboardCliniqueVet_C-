using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CliniqueVet
{
    public partial class FormFacture : Form
    {
        private string ConnStr => System.Configuration.ConfigurationManager
            .ConnectionStrings["CliniqueVet"].ConnectionString;

        private int _idSelectionne = -1;

        public FormFacture()
        {
            InitializeComponent();
        }

        private void FormFacture_Load(object sender, EventArgs e)
        {
            ConfigurerUI();
            ChargerProprietairesDansCombo();
            ChargerFactures();
        }

        private void ConfigurerUI()
        {
            label2.Text = "Proprietaire:";
            label3.Text = "Consultation:";
            label4.Text = "Montant consult:";
            label5.Text = "Montant med:";
            label6.Text = "Total:";
            label7.Text = "Gestion Factures";
            label8.Text = "Statut:";
            label9.Text = "Mode Paiement:";

            // Total en lecture seule
            textBox2.ReadOnly = true;
            textBox2.BackColor = Color.WhiteSmoke;

            // Montant medicaments en lecture seule (calculé auto)
            textBox1.ReadOnly = true;
            textBox1.BackColor = Color.WhiteSmoke;
            textBox1.Text = "0.00";

            // Montant consultation saisissable
            textBox3.Text = "30.00";
            textBox3.TextChanged += (s, ev) => RecalculerTotal();

            comboBox3.Items.Clear();
            comboBox3.Items.AddRange(new object[] { "Non payée", "Payée", "Partiellement payée" });
            comboBox3.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox3.SelectedIndex = 0;

            comboBox1.SelectedIndexChanged += ComboProprietaire_Changed;

            button5.Text = "Generer Facture";
            button5.BackColor = Color.SeaGreen;
            button5.ForeColor = Color.White;
            button5.Click += BtnGenerer_Click;

            button6.Text = "Enregistrer Paiement";
            button6.BackColor = Color.SteelBlue;
            button6.ForeColor = Color.White;
            button6.Click += BtnEnregistrerPaiement_Click;

            btnProprio.Click += (s, ev) => { var f = new FormProprio(); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button1.Click += (s, ev) => { var f = new FormAnimaux(); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button2.Click += (s, ev) => { var f = new FormRDV(); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button3.Click += (s, ev) => { var f = new FormFacture(); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button4.Click += (s, ev) => { var f = new FormLogin(); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };

            dataGridView1.ReadOnly = true;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.CellClick += DataGridView1_CellClick;
        }

        private void ChargerProprietairesDansCombo()
        {
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        "SELECT IdProprietaire, Nom+' '+Prenom AS NomComplet FROM Proprietaires ORDER BY Nom", conn);
                    var dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);
                    comboBox1.DataSource = dt;
                    comboBox1.DisplayMember = "NomComplet";
                    comboBox1.ValueMember = "IdProprietaire";
                }
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private void ComboProprietaire_Changed(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null) return;
            var row = (DataRowView)comboBox1.SelectedItem;
            int idProprio = Convert.ToInt32(row["IdProprietaire"]);
            ChargerConsultationsDansCombo(idProprio);
        }

        private void ChargerConsultationsDansCombo(int idProprio)
        {
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        @"SELECT c.IdConsultation,
                                 CONVERT(VARCHAR, c.DateConsultation, 103) + ' - ' + a.Nom AS Label,
                                 ISNULL((SELECT SUM(cm.Quantite * m.PrixUnitaire)
                                         FROM ConsultationMedicaments cm
                                         JOIN Medicaments m ON cm.IdMedicament = m.IdMedicament
                                         WHERE cm.IdConsultation = c.IdConsultation), 0) AS MontantMed
                          FROM Consultations c
                          JOIN Animaux a ON c.IdAnimal = a.IdAnimal
                          WHERE a.IdProprietaire = @ip
                          ORDER BY c.DateConsultation DESC", conn);
                    cmd.Parameters.AddWithValue("@ip", idProprio);
                    var dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);

                    comboBox2.SelectedIndexChanged -= ComboConsultation_Changed;
                    comboBox2.DataSource = dt;
                    comboBox2.DisplayMember = "Label";
                    comboBox2.ValueMember = "IdConsultation";
                    comboBox2.SelectedIndexChanged += ComboConsultation_Changed;

                    if (dt.Rows.Count > 0)
                    {
                        textBox1.Text = Convert.ToDecimal(dt.Rows[0]["MontantMed"]).ToString("F2");
                        RecalculerTotal();
                    }
                    else
                    {
                        textBox1.Text = "0.00";
                        RecalculerTotal();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private void ComboConsultation_Changed(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex < 0 || comboBox2.DataSource == null) return;
            var dt = (DataTable)comboBox2.DataSource;
            if (dt.Rows.Count == 0 || comboBox2.SelectedIndex >= dt.Rows.Count) return;
            textBox1.Text = Convert.ToDecimal(dt.Rows[comboBox2.SelectedIndex]["MontantMed"]).ToString("F2");
            RecalculerTotal();
        }

        private void RecalculerTotal()
        {
            decimal c = 0, m = 0;
            decimal.TryParse(textBox3.Text, out c);
            decimal.TryParse(textBox1.Text, out m);
            textBox2.Text = (c + m).ToString("F2");
        }

        private void ChargerFactures()
        {
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        @"SELECT f.IdFacture, 
                                 CONVERT(VARCHAR, f.DateFacture, 103) AS DateFacture,
                                 f.MontantTotal, f.Statut,
                                 p.Nom+' '+p.Prenom AS Proprietaire, 
                                 f.ModePaiement
                          FROM Factures f
                          JOIN Proprietaires p ON f.IdProprietaire=p.IdProprietaire
                          ORDER BY f.DateFacture DESC", conn);
                    var dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);
                    dataGridView1.DataSource = dt;

                    if (dataGridView1.Columns["IdFacture"] != null)
                        dataGridView1.Columns["IdFacture"].Visible = false;
                }
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dataGridView1.Rows[e.RowIndex];
            _idSelectionne = Convert.ToInt32(row.Cells["IdFacture"].Value);
            comboBox3.Text = row.Cells["Statut"].Value?.ToString();
            textBox6.Text = row.Cells["ModePaiement"].Value?.ToString();
        }

        private void BtnGenerer_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue == null || comboBox2.SelectedValue == null)
            {
                MessageBox.Show("Sélectionnez un propriétaire et une consultation.");
                return;
            }

            decimal montantC = 0;
            if (!decimal.TryParse(textBox3.Text, out montantC))
            {
                MessageBox.Show("Montant consultation invalide.");
                return;
            }
            decimal montantM = 0;
            decimal.TryParse(textBox1.Text, out montantM);

            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    var check = new SqlCommand(
                        "SELECT COUNT(*) FROM Factures WHERE IdConsultation=@ic", conn);
                    check.Parameters.AddWithValue("@ic", comboBox2.SelectedValue);
                    if ((int)check.ExecuteScalar() > 0)
                    {
                        MessageBox.Show("Facture déjà générée pour cette consultation.");
                        return;
                    }

                    var cmd = new SqlCommand(
                        @"INSERT INTO Factures(IdConsultation,IdProprietaire,MontantConsultation,MontantMedicaments,Statut)
                          VALUES(@ic,@ip,@mc,@mm,'Non payée')", conn);
                    cmd.Parameters.AddWithValue("@ic", comboBox2.SelectedValue);
                    cmd.Parameters.AddWithValue("@ip", comboBox1.SelectedValue);
                    cmd.Parameters.AddWithValue("@mc", montantC);
                    cmd.Parameters.AddWithValue("@mm", montantM);
                    cmd.ExecuteNonQuery();
                }
                ChargerFactures();
                MessageBox.Show("Facture générée avec succès !");
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private void BtnEnregistrerPaiement_Click(object sender, EventArgs e)
        {
            if (_idSelectionne < 0)
            {
                MessageBox.Show("Sélectionnez une facture dans la liste.");
                return;
            }
            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();
                    var cmd = new SqlCommand(
                        "UPDATE Factures SET Statut=@s,ModePaiement=@mp,DatePaiement=GETDATE() WHERE IdFacture=@id", conn);
                    cmd.Parameters.AddWithValue("@s", comboBox3.SelectedItem?.ToString() ?? "Non payée");
                    cmd.Parameters.AddWithValue("@mp", string.IsNullOrWhiteSpace(textBox6.Text) ? (object)DBNull.Value : textBox6.Text.Trim());
                    cmd.Parameters.AddWithValue("@id", _idSelectionne);
                    cmd.ExecuteNonQuery();
                }
                ChargerFactures();
                MessageBox.Show("Paiement enregistré !");
            }
            catch (Exception ex) { MessageBox.Show("Erreur : " + ex.Message); }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}