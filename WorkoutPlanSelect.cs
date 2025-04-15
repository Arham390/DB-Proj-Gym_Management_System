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
    public partial class WorkoutPlanSelect : Form
    {
        private string ConnectionString = GlobalSettings.GetConnectionString();
        private int userID;
        private DataTable originalDataTable = new DataTable();

        public WorkoutPlanSelect(int userID)
        {
            InitializeComponent();
            this.userID = userID;
            FILLTables();
        }




        private void FILLTables()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM WorkoutPlans";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        DataTable dataTable = new DataTable();
                        SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                        dataAdapter.Fill(originalDataTable);

                        dataGridView1.DataSource = originalDataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

        }
        private void PopulateRoutines(int planID)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                 string query = "SELECT r.Day1 AS Day, e.ExerciseName AS ExerciseName, e.TargetMuscle AS TargetMuscle, e.Machine AS Machine, r.sets1 AS Sets, r.Reps, r.Interval " +
                "FROM ROUTINE r " +
                "INNER JOIN Exercise e ON r.ExerciseID = e.ExerciseID " +
                "INNER JOIN WorkoutPlans wp ON r.PlanID = wp.PlanID " +
                "WHERE r.PlanID = @PlanID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@PlanID", planID);
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




        private void WorkoutPlanSelect_Load(object sender, EventArgs e)
        {

        }


        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // Assuming "PlanID" is the correct column name, change it if necessary
                int selectedPlanID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value);

                PopulateRoutines(selectedPlanID);
            }
        }

        private void workoutPlansBindingSource_CurrentChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void rOUTINEBindingSource_CurrentChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedPlanID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value);
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        string query = "UPDATE MEMBERS SET WORKOUTID = @planid WHERE MEMBERID = @userID";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@planid", selectedPlanID);
                            command.Parameters.AddWithValue("@userID", userID);
                            command.ExecuteNonQuery();
                        }
                    }
                    MessageBox.Show("Workout plan updated.");
                    // Assuming "MemberInterface" is the name of the form you want to show
                    MemberInterface memberInterfaceForm = new MemberInterface(userID);
                    memberInterfaceForm.Show();
                    this.Hide();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            MemberInterface memberInterfaceForm = new MemberInterface(userID);
            memberInterfaceForm.Show();
            this.Hide();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            WorkoutCreate Form = new WorkoutCreate(userID, false,this);
            Form.Show();
            this.Hide();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            DataView a = originalDataTable.DefaultView;
            a.RowFilter = "PlanName Like '%" + textBox1.Text + "%'";
        }
    }
}

