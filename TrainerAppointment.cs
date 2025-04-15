using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Deliverable_2
{
    public partial class TrainerAppointment : Form
    {
        private int userID;
        private string ConnectionString = GlobalSettings.GetConnectionString();

        public TrainerAppointment(int u)
        {
            InitializeComponent();
            userID = u;
            PopulateDataGridView();
        }


        private void PopulateDataGridView()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = @"SELECT 
                                PT.SessionID,
                                U.FullName AS MemberName, 
                                M.GymID, 
                                O.GymName, 
                                PT.SessionTime, 
                                PT.SessionDate, 
                                CASE 
                                    WHEN PT.Status1 = 0 THEN 'Pending'
                                    WHEN PT.Status1 = 1 THEN 'Done'
                                END AS Status
                            FROM 
                                PersonalTrainingSessions PT
                            INNER JOIN 
                                Members M ON PT.MemberID = M.MemberID
                            INNER JOIN 
                                Owners O ON M.GymID = O.OwnerID
                            INNER JOIN 
                                Users U ON M.MemberID = U.UserID
                            INNER JOIN 
                                Trainers T ON PT.TrainerID = T.TrainerID
                            WHERE 
                                T.TrainerID = @TrainerID AND
                                (M.GymID IS NOT NULL AND M.GymID <> '') OR M.GymID IS NULL";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TrainerID", userID); // Assuming userID contains the current trainer's ID
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        dataGridView1.DataSource = dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }




        private void ApplyFilter()
        {
            string filter = comboBox2.SelectedItem.ToString();
            DataView dv = ((DataTable)dataGridView1.DataSource).DefaultView;
            switch (filter)
            {
                case "All":
                    dv.RowFilter = "";
                    break;
                case "Pending":
                    dv.RowFilter = "Status = 'Pending'";
                    break;
                case "Done":
                    dv.RowFilter = "Status = 'Done'";
                    break;
                default:
                    break;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Update status to Done (1)
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedIndex = dataGridView1.SelectedRows[0].Index;
                string status = dataGridView1.Rows[selectedIndex].Cells["Status"].Value.ToString();
                if (status == "Pending")
                {
                    int sessionID = GetSessionID(selectedIndex);
                    UpdateSessionStatus(sessionID, 1); // Update status to Done
                    PopulateDataGridView();
                }
                else
                {
                    MessageBox.Show("Session status is already Done.");
                }
            }
            else
            {
                MessageBox.Show("Please select a session to update.");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Delete the selected training session
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedIndex = dataGridView1.SelectedRows[0].Index;
                int sessionID = GetSessionID(selectedIndex);
                DeleteSession(sessionID);
                PopulateDataGridView();
            }
            else
            {
                MessageBox.Show("Please select a session to delete.");
            }
        }

        private int GetSessionID(int rowIndex)
        {
            return Convert.ToInt32(dataGridView1.Rows[rowIndex].Cells["SessionID"].Value);
        }

        private void UpdateSessionStatus(int sessionID, int newStatus)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "UPDATE PersonalTrainingSessions SET Status1 = @NewStatus WHERE SessionID = @SessionID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@NewStatus", newStatus);
                        command.Parameters.AddWithValue("@SessionID", sessionID);
                        command.ExecuteNonQuery();
                    }
                    PopulateDataGridView();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        
        private void DeleteSession(int sessionID)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    // Check if the session status is Pending (0) before deleting
                    string checkStatusQuery = "SELECT Status1 FROM PersonalTrainingSessions WHERE SessionID = @SessionID";
                    using (SqlCommand checkStatusCommand = new SqlCommand(checkStatusQuery, connection))
                    {
                        checkStatusCommand.Parameters.AddWithValue("@SessionID", sessionID);
                        int status = (int)checkStatusCommand.ExecuteScalar();
                        if (status == 0) // Only delete if the status is Pending
                        {
                            string deleteQuery = "DELETE FROM PersonalTrainingSessions WHERE SessionID = @SessionID";
                            using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                            {
                                command.Parameters.AddWithValue("@SessionID", sessionID);
                                command.ExecuteNonQuery();
                            }
                            // Update the DataGridView after the deletion
                            PopulateDataGridView();
                        }
                        else
                        {
                            MessageBox.Show("Cannot delete a session that is already completed.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void TrainerAppointment_Load(object sender, EventArgs e)
        {

        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TrainerInterface form = new TrainerInterface(userID);
            form.Show();
            this.Hide();
        }
    }
}
