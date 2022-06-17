using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace TestBD_1C
{
    public partial class Form1 : Form
    {
        private SqlConnection _connection = null;
        private SqlDataReader _reader = null;
        private SqlCommand _query = null;
        private readonly string number = "ПзС-000100";

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
	                                                    WHERE Номер = @number;SET @id=SCOPE_IDENTITY()", _connection);

                        // Задаем значение параметра @number в одну строку
                        //_query.Parameters.AddWithValue("number", "ПзС-000100");
                        // создаем параметр для номера документа отдельно
                        SqlParameter nameParam = new SqlParameter("@number", System.Data.SqlDbType.NVarChar, 12);
                        // определяем значение
                        nameParam.Value = number;
                        // добавляем параметр к команде
                        _query.Parameters.Add(nameParam);

                        // Выходной параметр для id, работает для INSERT
                        SqlParameter idParam = new SqlParameter
                        {
                            ParameterName = "@id",
                            SqlDbType = SqlDbType.Int,
                            Direction = ParameterDirection.Output // параметр выходной
                        };
                        _query.Parameters.Add(idParam);

                        using (_reader = await _query.ExecuteReaderAsync())
                        {
                            if (_reader.HasRows) // если есть данные
                            {
                                // Выводим названия колонок ColumnName
                                label1.Text = $"{_reader.GetName(0),10} {_reader.GetName(1),15} {_reader.GetName(2),50}\n\n";

                                while (await _reader.ReadAsync())
                                {
                                    string number = _reader.GetString(0);

                                    label1.Text += $"[{number}] {_reader["Фирма"]} - {_reader["ЗаказТовара"]}\n";
                                }
                            }
                            else
                            {
                                label1.Text += $"Таблица пустая!\n";
                            }
                        }

                        _query.CommandText = @"SELECT COUNT(*)
	                                                    FROM [dns_m].[dwh].[Документ.ЗаказТовара]
	                                                    WHERE Номер = 'ПзС-000100'";
                        object count = await _query.ExecuteScalarAsync();

                        label1.Text += $"\nВыходной параметр: {idParam.Value}";
                        label1.Text += $"\nКоличество документов: {count}\n\nClientConnectionId: {_connection.ClientConnectionId} [{DateTime.Now}]\n";
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