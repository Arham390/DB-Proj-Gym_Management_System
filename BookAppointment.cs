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
    public partial class BookAppointment : Form
    {
        private string ConnectionString = GlobalSettings.GetConnectionString();
       private  int userID;
        public BookAppointment(int userID)
        {
            InitializeComponent();
            this.userID = userID;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            MemberInterface form=new MemberInterface(userID);
            form.Show();
            this.Hide();
        }
        private int GetTrainerID(string trainerName)
        {
            int trainerID = 0;

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                // Open the connection
                connection.Open();

                // Create a SqlCommand to execute the query
                string query = "SELECT T.TrainerID FROM Trainers T INNER JOIN Users U ON T.TrainerID = U.UserID WHERE U.FullName = @FullName";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add parameter to the query
                    command.Parameters.AddWithValue("@FullName", trainerName);

                    // Execute the query and retrieve TrainerID
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        trainerID = Convert.ToInt32(result);
                    }
                }
            }

            return trainerID;
        }
       

        private void BookAppointment_Load(object sender, EventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                // Open the connection
                connection.Open();

                // Create a SqlCommand to execute your SQL query
                string query = "SELECT TrainerID,U.FullName AS TrainerName, T.Height, T.Weight, T.Spec, T.Experience FROM Trainers T INNER JOIN worksat W ON T.TrainerID = W.trainer INNER JOIN Owners O ON W.gymid = O.OwnerID INNER JOIN Members M ON M.GymID = O.OwnerID INNER JOIN Users U ON T.TrainerID = U.UserID WHERE M.MemberID = @userID;"; // Change the query according to your table and column names
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add parameters to your query
                    command.Parameters.AddWithValue("@userID", userID); // Assuming userID is defined elsewhere

                    // Create a DataTable to hold the results of the query
                    DataTable dataTable = new DataTable();

                    // Use a SqlDataAdapter to fill the DataTable with the results of the query
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }

                    // Set the DataSource of your ComboBox to the DataTable
                    comboBox2.DataSource = dataTable;

                    // Set the DisplayMember property of your ComboBox to the appropriate column name
                    comboBox2.DisplayMember = "TrainerName"; // Change "TrainerName" to the appropriate column name
                }
            }
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {

            // Retrieve selected trainer name
            string trainerName = comboBox2.Text;

            // Retrieve selected date and time
            DateTime sessionDate = dateTimePicker2.Value.Date; // Get date part only
            DateTime sessionTime = dateTimePicker1.Value; // Get time part only

            // Retrieve TrainerID based on the selected trainer name
            int trainerID = GetTrainerID(trainerName);

            // Check if all required fields are filled
            if (trainerID != 0 && sessionDate != null && sessionTime != null)
            {
                // Define your SQL query to insert appointment into PersonalTrainingSessions table
                string query = "INSERT INTO PersonalTrainingSessions (TrainerID, MemberID, Status1, SessionDate, SessionTime) " +
                               "VALUES (@TrainerID, @MemberID, @Status1, @SessionDate, @SessionTime)";

                // Establish connection to the database
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    try
                    {
                        // Open the connection
                        connection.Open();

                        // Create a SqlCommand to execute the query
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            // Add parameters to the query
                            command.Parameters.AddWithValue("@TrainerID", trainerID);
                            command.Parameters.AddWithValue("@MemberID", userID);
                            command.Parameters.AddWithValue("@Status1", 0);
                            command.Parameters.AddWithValue("@SessionDate", sessionDate);
                            command.Parameters.AddWithValue("@SessionTime", sessionTime);

                            // Execute the query
                            int rowsAffected = command.ExecuteNonQuery();

                            // Check if the query was executed successfully
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Appointment saved successfully!");
                                MemberInterface FORM = new MemberInterface(userID);
                                FORM.Show();
                                this.Hide();
                            }
                            else
                            {
                                MessageBox.Show("Failed to save appointment. Please try again.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An error occurred: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please fill in all the fields before saving.");
            }
        }
    }
}
