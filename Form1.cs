using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace TestBD_1C_2
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
            button1.Text = "Start query 1";
            button2.Text = "Start query 2";
            textBox1.Text = @"select * from Products where UnitPrice > 100";
        }

        private async void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                label1.Text = "LOADING ...";

                if (GetConnectionString("NortwindDB") != null)
                {
                    using (_connection = new SqlConnection(GetConnectionString("NortwindDB")))
                    {
                        await _connection.OpenAsync();

                        _query = new SqlCommand(@"select top 10 ProductID, ProductName, QuantityPerUnit from Products", _connection);

                        using (_reader = await _query.ExecuteReaderAsync())
                        {
                            label1.Text = $"{_reader.GetName(0)} {_reader.GetName(1)} {_reader.GetName(2)}\n";

                            while (await _reader.ReadAsync())
                            {
                                label1.Text += $"[{_reader.GetValue(0)}] {_reader["ProductName"]} - {_reader["QuantityPerUnit"]}\n";
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
            catch (Exception ex)
            {
                label1.Text = ex.Message;
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (GetConnectionString("NortwindDB") != null)
                {
                    using (_connection = new SqlConnection(GetConnectionString("NortwindDB")))
                    {
                        await _connection.OpenAsync();

                        SqlDataAdapter dataAdapter = new SqlDataAdapter(textBox1.Text, _connection);

                        DataSet dataSet = new DataSet();

                        dataAdapter.Fill(dataSet);

                        dataGridView1.DataSource = dataSet.Tables[0];
                    }
                }
                else
                {
                    MessageBox.Show("Не найдена строка подключения!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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