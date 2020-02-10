using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;


namespace PhoneBook
{

    public partial class AccesWindowBox : Form
    {
        public AccesWindowBox()
        {

            InitializeComponent();

        }

        public void buttonCancel_Click(object sender, EventArgs e)
        {

            this.Close();
        }

        public void buttonSubmit_Click(object sender, EventArgs e)
        {

            PhoneBook phoneBook = new PhoneBook();
            SqlConnection sqlConnection = new SqlConnection(phoneBook.sqlconnectionstring);

            try
            {
                sqlConnection.Open();
                SqlDataAdapter dataAdapter = new SqlDataAdapter("AuthorizedLogin", sqlConnection);
                dataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                dataAdapter.SelectCommand.Parameters.AddWithValue("@Login", LoginTextBox.Text);
                dataAdapter.SelectCommand.Parameters.AddWithValue("@Password", PasswordtextBox.Text);

                DataTable dataTable = new DataTable();
                dataAdapter.Fill(dataTable);

                if (dataTable.Rows.Count != 0)
                {
                    SqlCommand command = new SqlCommand("Truncate table PhoneBook", sqlConnection);
                    command.ExecuteNonQuery();

                    MessageBox.Show("Entire PhoneBook was deleted successfully.",
                        "Success", MessageBoxButtons.OK);
                    this.Close();
                    phoneBook.Clear();
                    phoneBook.CurrentGridView();
                }
                else
                {
                    MessageBox.Show("You inserted wrong Login or Password.",
                         "Wrong.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (SqlException)
            {
                MessageBox.Show("Sorry,an error occured, while attempting to connect with DataBase.",
                    "Contact IT", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            finally
            {
                if (sqlConnection.State == ConnectionState.Open)
                {
                    sqlConnection.Close();
                }
            }
        }
    }

}
