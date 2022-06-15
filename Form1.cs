using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace TestBD_1C
{
    public partial class Form1 : Form
    {
        private SqlConnection _connection = null;
        private SqlDataReader _reader = null;
        private SqlCommand _query = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Test BD 1C";
            label1.Text = "";
            button1.Text = "Start query";
        }

        private async void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                label1.Text = "LOADING ...";

                if (GetConnectionString("dns_m") != null)
                {
                    using (_connection = new SqlConnection(GetConnectionString("dns_m")))
                    {
                        await _connection.OpenAsync();

                        _query = new SqlCommand(@"SELECT Номер, Фирма.Наименование AS Фирма,
	                                                    CONCAT('ЗаказТовара ', Номер, ' от ', CONCAT(CONVERT(VARCHAR, Дата, 104), ' ', CONVERT(VARCHAR, Дата, 108))) AS ЗаказТовара
	                                                    FROM [dns_m].[dwh].[Документ.ЗаказТовара]
	                                                    INNER JOIN dns_m.dwh.[Справочник.Фирмы] AS Фирма ON Фирма.Ссылка = [dns_m].[dwh].[Документ.ЗаказТовара].Фирма
	                                                    WHERE Номер = 'ПзС-000100'", _connection);

                        using (_reader = await _query.ExecuteReaderAsync())
                        {
                            label1.Text = "";

                            while (await _reader.ReadAsync())
                            {
                                label1.Text += $"[{_reader["Номер"]}] {_reader["Фирма"]} - {_reader["ЗаказТовара"]}\n";
                            }
                        }

                        label1.Text += $"ClientConnectionId: {_connection.ClientConnectionId}\n";
                    }
                }
                else
                {
                    label1.Text = "Не найдена строка подключения!";
                }
            }
            catch (NullReferenceException ex)
            {
                label1.Text += ex.Message;
            }
            catch (Exception ex)
            {
                label1.Text += ex.Message;
            }
        }

        private string GetConnectionString(string nameField)
        {
            try
            {
                return ConfigurationManager.ConnectionStrings[nameField].ConnectionString;
            }
            catch
            {
                return null;
            }
        }
    }
}