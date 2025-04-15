using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Deliverable_2
{
    public partial class WorkoutCreate : Form
    {
        private string ConnectionString = GlobalSettings.GetConnectionString();

        private int  userID,dietID;
        private List<Routine> routines = new List<Routine>();
        private bool up;
        private Form callingForm;
        public WorkoutCreate(int uid,bool update, Form callingForm)
        {
            InitializeComponent();
            userID = uid;
            update = up;
            this.callingForm = callingForm;
        }

        private void WorkoutCreate_Load(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("SELECT ExerciseName FROM Exercise", connection);
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        comboBox2.Items.Add(reader["ExerciseName"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading exercise names: " + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string planName = textBox1.Text;

            // Check if there are any routines added to the workout plan
            if (routines.Count == 0)
            {
                MessageBox.Show("Please add at least one routine to the workout plan.");
                return;
            }

            // Insert into diet table and get the dietID
            int dietID = InsertDietPlan(planName);

            // Insert routines into ROUTINE table
            foreach (Routine routine in routines)
            {
                InsertRoutine(dietID, routine);
            }

            if (up)
            {
                UpdateMemberWorkoutID(dietID);
            }

            // Clear input fields after saving plan
            ClearPlanInputFields();

            if (callingForm is MemberInterface || callingForm is WorkoutPlanSelect)
            {
                using (MemberInterface Form = new MemberInterface(userID))
                {
                    this.Hide();
                    Form.ShowDialog();
                }
            }
            else if (callingForm is WorkoutPlanTrainer)
            {
                using (WorkoutPlanTrainer Form = new WorkoutPlanTrainer(userID))
                {
                    this.Hide();
                    Form.ShowDialog();
                }
            }
        }

        private void UpdateMemberWorkoutID(int workoutID)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("UPDATE Members SET workoutID = @WorkoutID WHERE MemberID = @UserID", connection);
                    command.Parameters.AddWithValue("@WorkoutID", workoutID);
                    command.Parameters.AddWithValue("@UserID", userID);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating workoutID for member: " + ex.Message);
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            string day = comboBox1.SelectedItem.ToString();
            string exerciseName = comboBox2.SelectedItem.ToString();
            int sets = int.Parse(textBox3.Text);
            int reps = int.Parse(textBox2.Text);
            int interval = int.Parse(textBox4.Text);

            // Create a new Routine object
            Routine routine = new Routine(day, exerciseName, sets, reps, interval);

            // Add the routine to the list
            routines.Add(routine);

            // Clear input fields after saving exercise
            ClearExerciseInputFields();
        }

        private int GetExerciseID(string exerciseName)
        {
            int exerciseID = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("SELECT ExerciseID FROM Exercise WHERE ExerciseName = @ExerciseName", connection);
                    command.Parameters.AddWithValue("@ExerciseName", exerciseName);
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        exerciseID = Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving ExerciseID: " + ex.Message);
            }

            // Add debug statement to check the exerciseName
            Console.WriteLine("ExerciseName: " + exerciseName);

            return exerciseID;
        }


        private void InsertRoutine(int dietID, Routine routine)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("INSERT INTO ROUTINE (PlanID, Day1, sets1, Reps, Interval, ExerciseID) VALUES (@PlanID, @Day, @Sets, @Reps, @Interval, @ExerciseID)", connection);
                command.Parameters.AddWithValue("@PlanID", dietID);
                command.Parameters.AddWithValue("@Day", routine.Day);
                command.Parameters.AddWithValue("@Sets", routine.Sets);
                command.Parameters.AddWithValue("@Reps", routine.Reps);
                command.Parameters.AddWithValue("@Interval", routine.Interval);

                // Get the ExerciseID based on exercise name
                int exerciseID = GetExerciseID(routine.ExerciseName);
                command.Parameters.AddWithValue("@ExerciseID", exerciseID);

                command.ExecuteNonQuery();
            }
        }


        private int InsertDietPlan(string planName)
        {
            int insertedDietID = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("INSERT INTO WORKOUTPLANS (PlanName, CreatorID) VALUES (@PlanName, @CreatorID); SELECT SCOPE_IDENTITY();", connection);
                command.Parameters.AddWithValue("@PlanName", planName);
                command.Parameters.AddWithValue("@CreatorID", userID);
                object result = command.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    insertedDietID = Convert.ToInt32(result);
                }
            }
            return insertedDietID;
        }

        public class Routine
        {
            public string Day { get; set; }
            public string ExerciseName { get; set; }
            public int Sets { get; set; }
            public int Reps { get; set; }
            public int Interval { get; set; }

            public Routine(string day, string exerciseName, int sets, int reps, int interval)
            {
                Day = day;
                ExerciseName = exerciseName;
                Sets = sets;
                Reps = reps;
                Interval = interval;
            }
        }

        private void ClearExerciseInputFields()
        {
            comboBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = -1;
            textBox4.Clear();
            textBox2.Clear();
            textBox3.Clear();
        }

        private void ClearPlanInputFields()
        {
            textBox1.Clear();
        }
        private void label3_Click(object sender, EventArgs e)
        {

        }
        private void label2_Click(object sender, EventArgs e)
        {

        }
        private void label6_Click(object sender, EventArgs e)
        {

        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (callingForm is MemberInterface)
            {
                using (MemberInterface Form = new MemberInterface(userID))
                {
                    this.Hide();
                    Form.ShowDialog();
                }
            }
            else if (callingForm is WorkoutPlanTrainer)
            {
                using (WorkoutPlanTrainer Form = new WorkoutPlanTrainer(userID))
                {
                    this.Hide();
                    Form.ShowDialog();
                }
            }
            else if (callingForm is WorkoutPlanSelect)
            {
                using (WorkoutPlanSelect Form = new WorkoutPlanSelect(userID))
                {
                    this.Hide();
                    Form.ShowDialog();
                }
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
