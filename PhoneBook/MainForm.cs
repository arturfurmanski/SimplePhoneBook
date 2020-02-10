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
using System.Globalization;

namespace PhoneBook
{
    public partial class PhoneBook : Form
    {
        public string sqlconnectionstring = @"Data Source=DESKTOP-UPFF4R0\SQLEXPRESS;Initial Catalog=PhoneBook;Integrated Security=True";
        /// <summary>
        /// Primary key in table, which store PhoneBook data.
        /// </summary>
        int PhonebookID;

        public PhoneBook()
        {

            InitializeComponent();
            CurrentGridView();

            buttonUpDate.Enabled = false;
            buttonDelete.Enabled = false;

        }

        private void textBoxFirstName_KeyPress(object sender, KeyPressEventArgs e)
        {
            NotAllowDigit(e);
        }
        private void textBoxLastName_KeyPress(object sender, KeyPressEventArgs e)
        {
            NotAllowDigit(e);
        }

        private void textBoxSearchByName_KeyPress(object sender, KeyPressEventArgs e)
        {
            NotAllowDigit(e);
        }
        /// <summary>
        /// Disable inserting digit into control.
        /// </summary>
        /// <param name="e"></param>
        void NotAllowDigit(KeyPressEventArgs e)
        {

            if (Char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }

        }
        /// <summary>
        /// Display current PhoneBook members.
        /// </summary>
        public void CurrentGridView()
        {
            SqlConnection sqlConnection = new SqlConnection(sqlconnectionstring);
            try
            {
                sqlConnection.Open();
                SqlDataAdapter dataAdapter = new SqlDataAdapter("DisplayAllPhoneBook", sqlconnectionstring);
                dataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                DataTable dataTable = new DataTable();

                dataAdapter.Fill(dataTable);

                dataGridViewPhoneBook.DataSource = dataTable;

                if (dataGridViewPhoneBook.Rows.Count == 0)
                {
                    buttonResetAll.Enabled = false;
                }
                else
                {
                    buttonResetAll.Enabled = true;
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

        /// <summary>
        /// Clear fields are mandatory to fill.
        /// </summary>
        public void Clear()
        {
            textBoxFirstName.Text = textBoxLastName.Text = maskedTextPhoneNumber.Text = "";
            textBoxFirstName.Focus();
        }
        /// <summary>
        /// Set cursor into empty mandatory field after unsuccessfull save or update operation. 
        /// </summary>
        void SetCursorPosition()
        {
            if (textBoxFirstName.Text.Trim() == "")
            {
                textBoxFirstName.Focus();
            }
            else if (textBoxLastName.Text.Trim() == "")
            {
                textBoxLastName.Focus();
            }
            else if (maskedTextPhoneNumber.Text.Trim().Length < maskedTextPhoneNumber.TextLength)
            {
                maskedTextPhoneNumber.Focus();
            }
            else
            {
                comboBoxSex.Focus();

            }

        }
        /// <summary>
        /// Search particular value in PhoneBook database.
        /// </summary>
        /// <param name="procedure">One of two Procedures from Sql "SearchByNames" or "SearchByNumber". </param>
        /// <param name="parameter"> Parameter included in Inserted procedure. </param>
        /// <param name="textcontrol"> TextValue passed to parameter in SQL stored procedure. </param>
        void SearchByValue(string procedure, string parameter, string textcontrol)
        {
            SqlConnection sqlConnection = new SqlConnection(sqlconnectionstring);

            try
            {
                sqlConnection.Open();
                SqlDataAdapter dataAdapter = new SqlDataAdapter(procedure, sqlconnectionstring);
                dataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                DataTable dataTable = new DataTable();
                dataAdapter.SelectCommand.Parameters.AddWithValue(parameter, textcontrol);
                dataAdapter.Fill(dataTable);


                dataGridViewPhoneBook.DataSource = dataTable;
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

        /// <summary>
        /// Add or Update PhoneBook member depending on inserted variables.
        /// </summary>
        /// <param name="procedure">One of two Procedures from Sql "Add_Member" or "UpdatePhoneBook"</param>
        /// <param name="sqlConnection">Connection string</param>
        void AddOrUpdate(string procedure, SqlConnection sqlConnection)
        {
            if (textBoxFirstName.Text.Trim() != "" && textBoxLastName.Text.Trim() != "" && maskedTextPhoneNumber.MaskCompleted
                    && comboBoxSex.Text.Trim() != "")
            {
                SqlCommand command = new SqlCommand(procedure, sqlConnection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@PhoneBookID", PhonebookID);
                command.Parameters.AddWithValue("@First_Name", textBoxFirstName.Text);
                command.Parameters.AddWithValue("@Last_Name", textBoxLastName.Text);
                command.Parameters.AddWithValue("@Phone_Number", maskedTextPhoneNumber.Text);
                command.Parameters.AddWithValue("@Sex", comboBoxSex.Text);

                command.ExecuteNonQuery();
                if (PhonebookID == 0)
                {
                    MessageBox.Show($"{textBoxFirstName.Text} {textBoxLastName.Text} was added succesfully."
                         , "Succes", MessageBoxButtons.OK);
                }
                else
                {
                    MessageBox.Show($"{textBoxFirstName.Text} {textBoxLastName.Text} was updated succesfully."
                , "Succes", MessageBoxButtons.OK);
                }

                CurrentGridView();
                Clear();
            }

            else
            {
                MessageBox.Show("Fill all fields marked as  * .", "Mandatory Fields", MessageBoxButtons.OK,
                  MessageBoxIcon.Information);
                PhonebookID = 0;
                SetCursorPosition();
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            SqlConnection sqlConnection = new SqlConnection(sqlconnectionstring);

            try
            {
                sqlConnection.Open();

                /* "SameNumber" check compatibility between PhoneNumber 
                 inserted by user and PhoneNumbers stored in PhoneBook DataBase to avoid duplicate.
                */
                SqlDataAdapter adapter = new SqlDataAdapter("SameNumber", sqlConnection);
                adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand.Parameters.AddWithValue("@Phone_Number", maskedTextPhoneNumber.Text.Trim());
                DataTable table = new DataTable();
                adapter.Fill(table);

                if (table.Rows.Count == 0)
                {
                    /* "SameNumber" check compatibility between Fisrt and Last Name 
                 inserted by user and Fisrt and Last Name stored in PhoneBook DataBase to avoid duplicate.
                */
                    SqlDataAdapter dataAdapter = new SqlDataAdapter("FindDuplicate", sqlConnection);
                    dataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                    dataAdapter.SelectCommand.Parameters.AddWithValue("@First_Name", textBoxFirstName.Text.Trim());
                    dataAdapter.SelectCommand.Parameters.AddWithValue("@Last_Name", textBoxLastName.Text.Trim());

                    DataTable dataTable = new DataTable();

                    dataAdapter.Fill(dataTable);
                    if (dataTable.Rows.Count == 0)
                    {
                        AddOrUpdate("Add_Member", sqlConnection);
                    }
                    else
                    {
                        DialogResult result = MessageBox.Show($"PhoneBook contains member {textBoxFirstName.Text.Trim()} {textBoxLastName.Text.Trim()}. " +
                        $"Do you want to add this record anyway ?"
                        , "Possible duplicate.", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            AddOrUpdate("Add_Member", sqlConnection);
                        }
                        else
                        { }
                    }
                }
                else
                {
                    MessageBox.Show($"Phone number {maskedTextPhoneNumber.Text.Trim()} already exists in PhoneBook. " +
                    "Please verify introduced number.",
                    "Possible duplicate number.", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        private void buttonUpDate_Click(object sender, EventArgs e)
        {

            SqlConnection sqlConnection = new SqlConnection(sqlconnectionstring);

            try
            {
                sqlConnection.Open();
                AddOrUpdate("UpdatePhoneBook", sqlConnection);
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

        private void textBoxSearchByNumber_TextChanged(object sender, EventArgs e)
        {
            SearchByValue("SearchByNumber", "@searchValueNum",
             textBoxSearchByNumber.Text.Trim());
        }

        private void textBoxSearchByName_TextChanged(object sender, EventArgs e)
        {
            SearchByValue("SearchByNames", "@searchValueName", textBoxSearchByName.Text.Trim());
        }

        private void dataGridViewPhoneBook_DoubleClick(object sender, EventArgs e)
        {
            //Below Condition exist to avoid NullReferenceException, in case dataGridViewPhoneBook would be empty.
            if (!Object.ReferenceEquals(dataGridViewPhoneBook.CurrentRow, null))
            {
                buttonUpDate.Enabled = true;
                buttonDelete.Enabled = true;

                PhonebookID = int.Parse(dataGridViewPhoneBook.CurrentRow.Cells[0].Value.ToString());
                textBoxFirstName.Text = dataGridViewPhoneBook.CurrentRow.Cells[1].Value.ToString();
                textBoxLastName.Text = dataGridViewPhoneBook.CurrentRow.Cells[2].Value.ToString();
                maskedTextPhoneNumber.Text = dataGridViewPhoneBook.CurrentRow.Cells[3].Value.ToString();
                comboBoxSex.SelectedItem = dataGridViewPhoneBook.CurrentRow.Cells[4].Value.ToString();
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            DialogResult result = new DialogResult();
            result = MessageBox.Show($"Are you sure to delete {textBoxFirstName.Text} {textBoxLastName.Text} ?"
                , "Are you sure?!", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                SqlConnection sqlConnection = new SqlConnection(sqlconnectionstring);

                try
                {
                    sqlConnection.Open();
                    SqlCommand command = new SqlCommand("DeleteFromPhB", sqlConnection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PhonebookID", PhonebookID);
                    command.ExecuteNonQuery();

                    MessageBox.Show($"{textBoxFirstName.Text} {textBoxLastName.Text} was deleted successfully.",
                        "Success", MessageBoxButtons.OK);
                    Clear();
                    CurrentGridView();
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

        /// <summary>
        /// Disable ButtonDelete after choice record from dataGridViewPhoneBook and change at least one of his item.
        /// </summary>
        /// <param name= "textcontrol">Text from control matching to particular cell in dataGridViewPhoneBook.</param>
        /// <param name="cellIndex">Index of Cell in dataGridViewPhoneBook matching to particular text from control. </param>
        void InactiveDeleteButton(string textcontrol, int cellIndex)
        {
            //Below Condition exist to avoid NullReferenceException, in case dataGridViewPhoneBook would be empty.
            if (!Object.ReferenceEquals(dataGridViewPhoneBook.CurrentRow, null))
            {
                if (textcontrol != dataGridViewPhoneBook.CurrentRow.Cells[cellIndex].Value.ToString().Trim())
                {
                    buttonDelete.Enabled = false;
                }

            }

        }

        private void textBoxFirstName_TextChanged(object sender, EventArgs e)
        {
            InactiveDeleteButton(textBoxFirstName.Text.Trim(), 1);
        }

        private void textBoxLastName_TextChanged(object sender, EventArgs e)
        {
            InactiveDeleteButton(textBoxLastName.Text.Trim(), 2);
        }

        private void comboBoxSex_SelectedValueChanged(object sender, EventArgs e)
        {
            InactiveDeleteButton(comboBoxSex.Text.Trim(), 4);
        }

        private void maskedTextPhoneNumber_TextChanged(object sender, EventArgs e)
        {
            InactiveDeleteButton(maskedTextPhoneNumber.Text.Trim(), 3);
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            Clear();
        }

        public void buttonResetAll_Click(object sender, EventArgs e)
        {

            DialogResult result = new DialogResult();
            result = MessageBox.Show("Are you sure to reset entire PhoneBook? "
                , "Warning!!", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);


            if (result == DialogResult.Yes)
            {
                AccesWindowBox windowBox = new AccesWindowBox();
                windowBox.ShowDialog();
            }
            CurrentGridView();
        }

        private void dataGridViewPhoneBook_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            //If dataGridViewPhoneBook is empty, buttonUpDate should be disabled.
            if (dataGridViewPhoneBook.Rows.Count == 0)
            {
                buttonUpDate.Enabled = false;
            }

        }
    }
}
