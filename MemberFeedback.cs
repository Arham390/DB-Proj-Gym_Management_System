using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Deliverable_2
{
    public partial class MemberFeedback : Form
    {
        private string ConnectionString = GlobalSettings.GetConnectionString();
        int userID;
        public MemberFeedback(int userID)
        {
            InitializeComponent();
            this.userID = userID;
        }
        private int GetGymID()
        {
            int gymID = 0;

            // Define your SQL query to retrieve GymID based on MemberID
            string query = "SELECT GymID FROM Members WHERE MemberID = @MemberID";

            // Establish connection to the database
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                // Open the connection
                connection.Open();

                // Create a SqlCommand to execute the query
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Add parameter to the query
                    command.Parameters.AddWithValue("@MemberID", userID);

                    // Execute the query and retrieve GymID
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        gymID = Convert.ToInt32(result);
                    }
                }
            }

            return gymID;
        }

        private void MemberFeedback_Load(object sender, EventArgs e)
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


        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            MemberInterface form = new MemberInterface(userID);
            form.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Retrieve selected trainer name
            string trainerName = comboBox2.Text;

            // Retrieve selected rating
            int rating = Convert.ToInt32(comboBox1.SelectedItem);

            int gymID = GetGymID();
            // Retrieve feedback text
            string feedback = richTextBox1.Text;
            // Retrieve the TrainerID based on the selected trainer name
            int trainerID = GetTrainerID(trainerName);
            // Check if all required fields are filled
            if (trainerName != null && rating != 0 && !string.IsNullOrEmpty(feedback))
            {
                // Define your SQL query to insert feedback into the TrainerRatings table
                string query = "INSERT INTO TrainerRatings (TrainerID, MemberID, GymID, Rating, Feedback) VALUES (@TrainerID, @MemberID, @GymID, @Rating, @Feedback)";

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
                            command.Parameters.AddWithValue("@GymID", gymID); // You need to define gymID
                            command.Parameters.AddWithValue("@Rating", rating);
                            command.Parameters.AddWithValue("@Feedback", feedback);

                            // Execute the query
                            int rowsAffected = command.ExecuteNonQuery();

                            // Check if the query was executed successfully
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Feedback saved successfully!");
                                MemberInterface form = new MemberInterface(userID);
                                form.Show();
                                this.Hide();
                            }
                            else
                            {
                                MessageBox.Show("Failed to save feedback. Please try again.");
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


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
