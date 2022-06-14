using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace TestBD_1C_2
{
    public partial class Form1 : Form
    {
        string connectionString = @"Data Source=10.0.3.238;Initial Catalog=dns_m;Integrated Security=True;";

        public Form1()
        {
            InitializeComponent();
            this.Text = "Test BD 1C";
            label1.Text = "";
            button1.Text = "Start query";
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                label1.Text = "";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand myQuery = new SqlCommand(@"SELECT [Номер], Фирма.Наименование AS Фирма,
	                                                    CONCAT('ЗаказТовара ', Номер, ' от ', CONCAT(CONVERT(VARCHAR, Дата, 104), ' ', CONVERT(VARCHAR, Дата, 108))) AS ЗаказТовара
	                                                    FROM [dns_m].[dwh].[Документ.ЗаказТовара]
	                                                    INNER JOIN dns_m.dwh.[Справочник.Фирмы] AS Фирма ON Фирма.Ссылка = [dns_m].[dwh].[Документ.ЗаказТовара].Фирма
	                                                    WHERE Номер = 'ПзС-000100'", connection);
                    using (SqlDataReader myReader = await myQuery.ExecuteReaderAsync())
                    {
                        while (await myReader.ReadAsync())
                        {
                            label1.Text = label1.Text + myReader["Фирма"] + " - " + myReader["ЗаказТовара"] + "\n";
                        }
                    }
                    label1.Text += $"ClientConnectionId: {connection.ClientConnectionId}";
                }
            }
            catch (Exception ex)
            {
                label1.Text = ex.Message;
            }
        }
    }
}