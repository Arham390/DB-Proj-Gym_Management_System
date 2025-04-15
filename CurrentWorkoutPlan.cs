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
    public partial class CurrentWorkoutPlan : Form
    {
        private string ConnectionString = GlobalSettings.GetConnectionString();

        private int workID;
        private int userID;
        public CurrentWorkoutPlan(int workID,int userID)
        {
            InitializeComponent();
            this.workID = workID;
            this.userID = userID;
            FILLTables(workID);
        }
        private void FILLTables(int workID)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM WORKOUTPLANS WHERE PLANID=@planID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@planID", workID);
                        DataTable dataTable = new DataTable();
                        SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                        dataAdapter.Fill(dataTable);

                        // Assuming dataGridView2 is correctly bound to the DataGridView control in your form
                        dataGridView1.DataSource = dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT r.Day1 AS Day, e.ExerciseName AS ExerciseName, e.TargetMuscle AS TargetMuscle, e.Machine AS Machine, r.sets1 AS Sets, r.Reps, r.Interval " +
                                    "FROM ROUTINE r " +
                                    "INNER JOIN Exercise e ON r.ExerciseID = e.ExerciseID " +
                                    "INNER JOIN WorkoutPlans wp ON r.PlanID = wp.PlanID " +
                                    "WHERE r.PlanID = @PlanID"; using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PlanID", workID);
                        DataTable dataTable = new DataTable();
                        SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                        dataAdapter.Fill(dataTable);

                        // Assuming dataGridView2 is correctly bound to the DataGridView control in your form
                        dataGridView2.DataSource = dataTable;
                    }
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

        private void button1_Click(object sender, EventArgs e)
        {
            MemberInterface memberInterfaceForm = new MemberInterface(userID);
            memberInterfaceForm.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "UPDATE Members SET workoutID = NULL WHERE MemberID = @UserID;";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userID);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions here
                MessageBox.Show("An error occurred: " + ex.Message);
            }
            dataGridView1.DataSource = null;
            // Clear dataGridView2
            dataGridView2.DataSource = null;
        }
    }
}
