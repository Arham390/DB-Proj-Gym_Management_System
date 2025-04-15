using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Deliverable_2
{
    public partial class AdminRequests : Form
    {
        int userID;
        private string ConnectionString = GlobalSettings.GetConnectionString();

        public AdminRequests(int userID)
        {
            InitializeComponent();
            this.userID = userID;
            PopulateDataGridView();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        private void PopulateDataGridView()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = @"
                SELECT 
                    G.OwnerID,
                    U.FullName AS OwnerName,
                    G.GymName,
                    U.Contact,
                    U.Gender,
                    CASE
                        WHEN G.Approved = 0 THEN 'Pending'
                    END AS Status
                FROM 
                    Owners G
                INNER JOIN 
                    Users U ON G.OwnerID = U.UserID
                WHERE 
                    G.Approved = 0";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        dataGridView2.DataSource = dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            AdminInterface form = new AdminInterface(userID);
            form.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0)
            {
                // Get the selected OwnerID
                int ownerId = Convert.ToInt32(dataGridView2.SelectedRows[0].Cells["OwnerID"].Value);

                // Update the approval status to 1 (approved)
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();
                        string query = "UPDATE Owners SET Approved = 1 WHERE OwnerID = @OwnerId";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@OwnerId", ownerId);
                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Request approved successfully.");
                                // Refresh the DataGridView
                                PopulateDataGridView();
                            }
                            else
                            {
                                MessageBox.Show("Failed to approve the request.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Please select a row to approve.");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            
                // Check if a row is selected
                if (dataGridView2.SelectedRows.Count > 0)
                {
                    // Get the selected OwnerID
                    int ownerId = Convert.ToInt32(dataGridView2.SelectedRows[0].Cells["OwnerID"].Value);

                    try
                    {
                        using (SqlConnection connection = new SqlConnection(ConnectionString))
                        {
                            connection.Open();

                            // Begin a transaction
                            SqlTransaction transaction = connection.BeginTransaction();

                            // Delete the owner and gym entry from the Owners table
                            string deleteOwnerQuery = "DELETE FROM Owners WHERE OwnerID = @OwnerId";
                            using (SqlCommand command = new SqlCommand(deleteOwnerQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@OwnerId", ownerId);
                                int rowsAffected = command.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    // Delete the owner from the Users table
                                    string deleteUserQuery = "DELETE FROM Users WHERE UserID = @UserId";
                                    using (SqlCommand deleteUserCommand = new SqlCommand(deleteUserQuery, connection, transaction))
                                    {
                                        deleteUserCommand.Parameters.AddWithValue("@UserId", ownerId);
                                        int usersRowsAffected = deleteUserCommand.ExecuteNonQuery();

                                        if (usersRowsAffected > 0)
                                        {
                                            // If both deletions are successful, commit the transaction
                                            transaction.Commit();
                                            MessageBox.Show("Request denied and owner deleted successfully.");
                                            // Refresh the DataGridView
                                            PopulateDataGridView();
                                        }
                                        else
                                        {
                                            // Rollback the transaction if deleting the owner from the Users table fails
                                            transaction.Rollback();
                                            MessageBox.Show("Failed to delete owner from the Users table.");
                                        }
                                    }
                                }
                                else
                                {
                                    // If deleting the owner from the Owners table fails, rollback the transaction
                                    transaction.Rollback();
                                    MessageBox.Show("Failed to deny the request.");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a row to deny.");
                }
            

        }

        private void AdminRequests_Load(object sender, EventArgs e)
        {

        }
    }
}
