using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace CliniqueVet
{
    public partial class FormDashboard : Form
    {
        private readonly int _idUser;
        private string ConnStr => System.Configuration.ConfigurationManager
            .ConnectionStrings["CliniqueVet"].ConnectionString;

        public FormDashboard(int idUser)
        {
            InitializeComponent();
            _idUser = idUser;
        }

        private void FormDashboard_Load(object sender, EventArgs e)
        {
            button1.Click += (s, ev) => { var f = new FormDashboard(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button2.Click += (s, ev) => { var f = new FormUtilisateurs(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button3.Click += (s, ev) => { var f = new FormAnimauxAdmin(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button4.Click += (s, ev) => { var f = new FormMedicAdmin(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            button5.Click += (s, ev) => { var f = new FormFactureAdmin(_idUser); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };
            btnDeco.Click += (s, ev) => { var f = new FormLogin(); f.Show(); this.Hide(); f.FormClosed += (x, y) => this.Close(); };

            try
            {
                using (var conn = new SqlConnection(ConnStr))
                {
                    conn.Open();

                    // Animaux actifs
                    ChiffreAnimaux.Text = ExecScalar(conn,
                        "SELECT COUNT(*) FROM Animaux WHERE Actif=1").ToString();

                    // RDV aujourd'hui
                    label3.Text = ExecScalar(conn,
                        "SELECT COUNT(*) FROM RendezVous WHERE CAST(DateHeure AS DATE)=CAST(GETDATE() AS DATE)").ToString();

                    // Factures non payées
                    label5.Text = ExecScalar(conn,
                        "SELECT COUNT(*) FROM Factures WHERE Statut='Non payée'").ToString();

                    
                    // Alertes stock faible OU date expiration dans moins de 30 jours
                    label7.Text = ExecScalar(conn,
                        @"SELECT COUNT(*) FROM Medicaments 
      WHERE QuantiteStock <= StockMinimum 
      OR (DateExpiration IS NOT NULL AND DateExpiration <= DATEADD(DAY, 30, GETDATE()))").ToString();

                    // Revenu du mois (factures payées)
                    var cmdRevenu = new SqlCommand(
                        @"SELECT ISNULL(SUM(MontantTotal), 0)
                          FROM Factures
                          WHERE Statut = 'Payée'
                          AND MONTH(DateFacture) = MONTH(GETDATE())
                          AND YEAR(DateFacture)  = YEAR(GETDATE())", conn);
                    decimal revenu = Convert.ToDecimal(cmdRevenu.ExecuteScalar());
                    labelRevenuChiffre.Text = revenu.ToString("F2") + " DT";
                    labelRevenuChiffre.ForeColor = Color.SeaGreen;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur dashboard : " + ex.Message);
            }
        }

        private int ExecScalar(SqlConnection conn, string sql)
        {
            try
            {
                var result = new SqlCommand(sql, conn).ExecuteScalar();
                return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
            }
            catch { return 0; }
        }
    }
}