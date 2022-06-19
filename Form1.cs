using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Linq;

namespace TestBD_1C_2
{
    public partial class Form1 : Form
    {
        private SqlConnection _connection = null;
        private SqlDataReader _reader = null;
        private SqlCommand _query = null;
        private SqlDataAdapter _dataAdapter = null;
        private DataSet _dataSet = null;

        private List<string[]> _rows = null;
        private List<string[]> _filteredList = null;

        private readonly string _query2 = @"SELECT * FROM Products";
        private readonly string _query3 = @"SELECT ProductName, QuantityPerUnit, UnitPrice FROM Products";

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
            textBox1.Text = _query2;

            listView2.View = View.Details;
            listView2.GridLines = true;

            _connection = new SqlConnection(GetConnectionString("NortwindDB"));
            _connection.Open();

            _dataAdapter = new SqlDataAdapter(_query2, _connection);
            _dataSet = new DataSet();
            _dataAdapter.Fill(_dataSet);

            dataGridView2.DataSource = _dataSet.Tables[0];

            comboBox1.SelectedIndex = 3;

            _reader = null;
            _rows = new List<string[]>();
            string[] row = null;

            try
            {
                _query = new SqlCommand(_query3, _connection);
                _reader = _query.ExecuteReader();

                while (_reader.Read())
                {
                    row = new string[]
                    {
                        Convert.ToString(_reader["ProductName"]),
                        Convert.ToString(_reader["QuantityPerUnit"]),
                        Convert.ToString(_reader["UnitPrice"])
                    };

                    _rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (_reader != null && !_reader.IsClosed)
                {
                    _reader.Close();
                }
            }

            RefreshList(_rows);

            _connection.Close();
        }

        private void RefreshList(List<string[]> list)
        {
            listView2.Items.Clear();

            foreach (string[] s in list)
            {
                listView2.Items.Add(new ListViewItem(s));
            }
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

                        _dataAdapter = new SqlDataAdapter(textBox1.Text, _connection);

                        _dataSet = new DataSet();

                        _dataAdapter.Fill(_dataSet);

                        dataGridView1.DataSource = _dataSet.Tables[0];
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

        private async void button3_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();

            _reader = null;

            try
            {
                if (GetConnectionString("NortwindDB") != null)
                {
                    using (_connection = new SqlConnection(GetConnectionString("NortwindDB")))
                    {
                        await _connection.OpenAsync();

                        _query = new SqlCommand("SELECT ProductName, QuantityPerUnit, UnitPrice FROM Products", _connection);

                        _reader = _query.ExecuteReader();

                        ListViewItem item = null;

                        while (_reader.Read())
                        {
                            item = new ListViewItem(new string[] 
                            {
                                Convert.ToString(_reader["ProductName"]),
                                Convert.ToString(_reader["QuantityPerUnit"]),
                                Convert.ToString(_reader["UnitPrice"])
                            });

                            listView1.Items.Add(item);
                        }
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
            finally
            {
                if (_reader != null && !_reader.IsClosed)
                {
                    _reader.Close();
                }
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

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            (dataGridView2.DataSource as DataTable).DefaultView.RowFilter = $"ProductName LIKE '%{textBox2.Text}%'";
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    (dataGridView2.DataSource as DataTable).DefaultView.RowFilter = $"UnitsInStock <= 10";
                    break;
                case 1:
                    (dataGridView2.DataSource as DataTable).DefaultView.RowFilter = $"UnitsInStock >= 10 AND UnitsInStock <= 50";
                    break;
                case 2:
                    (dataGridView2.DataSource as DataTable).DefaultView.RowFilter = $"UnitsInStock >= 50";
                    break;
                case 3:
                    (dataGridView2.DataSource as DataTable).DefaultView.RowFilter = "";
                    break;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            _filteredList = _rows.Where((x) => x[0].ToLower().Contains(textBox3.Text.ToLower())).ToList();

            RefreshList(_filteredList);
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox2.SelectedIndex)
            {
                case 0:
                    _filteredList = _rows.Where((x) => Double.Parse(x[2]) <= 10).ToList();
                    RefreshList(_filteredList);
                    break;
                case 1:
                    _filteredList = _rows.Where((x) => Double.Parse(x[2]) > 10 && Double.Parse(x[2]) <= 100).ToList();
                    RefreshList(_filteredList);
                    break;
                case 2:
                    _filteredList = _rows.Where((x) => Double.Parse(x[2]) > 100).ToList();
                    RefreshList(_filteredList);
                    break;
                case 3:
                    RefreshList(_rows);
                    break;
            }
        }
    }
}