using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Deliverable_2
{
    public partial class MemberInterface : Form
    {
        private string ConnectionString = GlobalSettings.GetConnectionString();

        private int userID;

        // Modify the constructor to accept the user ID as a parameter
        public MemberInterface(int userID)
        {
            InitializeComponent();
            // Assign the passed user ID to the private field
            this.userID = userID;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Check if the member has a workout plan
            int workoutID = GetWorkoutIDForUser(userID);

            if (workoutID != -1)
            {
                using (CurrentWorkoutPlan Form = new CurrentWorkoutPlan(workoutID, userID))
                {
                    this.Hide();
                    Form.ShowDialog();
                }
            }
            else
            {
                using (WorkoutCreate Form = new WorkoutCreate(userID,true,this))
                {
                    this.Hide();
                    Form.ShowDialog();
                }
            }
        }

        // Method to get the workoutID for the given userID
        private int GetWorkoutIDForUser(int userID)
        {
            int workoutID = -1; // Default value if no workout plan is found

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT workoutID FROM Members WHERE MemberID = @UserID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userID);
                        object result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            workoutID = Convert.ToInt32(result);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

            return workoutID;
        }

        private void MemberInterface_Load(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            using (BookAppointment Form = new BookAppointment(userID))
            {
                this.Hide();
                Form.ShowDialog();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            using (MemberFeedback Form = new MemberFeedback(userID))
            {
                this.Hide();
                Form.ShowDialog();
            }
        }
        private int getDietPlanID()
        {
            int dietID = -1; // Default value if no workout plan is found

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT dietID FROM Members WHERE MemberID = @UserID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userID);
                        object result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            dietID = Convert.ToInt32(result);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            return dietID;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            int dietID = getDietPlanID();

            if (dietID != -1)
            {
                using (CurrentDietPlan Form = new CurrentDietPlan(dietID,userID))
                {
                    this.Hide();
                    Form.ShowDialog();
                }
            }
            else
            {

                using (DietPlan Form = new DietPlan(userID,true,this))
                {
                    this.Hide();
                    Form.ShowDialog();
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (DietPlanSelection Form = new DietPlanSelection(userID))
            {
                this.Hide();
                Form.ShowDialog();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (WorkoutPlanSelect Form = new WorkoutPlanSelect(userID))
            {
                this.Hide();
                Form.ShowDialog();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Form1 form = new Form1();
            form.Show();
            this.Hide();
        }
    }
}
