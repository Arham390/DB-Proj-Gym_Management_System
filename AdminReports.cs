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
    public partial class AdminReports : Form
    {
        private int userID;
       string ConnectionString=GlobalSettings.GetConnectionString();
        public AdminReports(int a)
        {
            InitializeComponent();
            userID = a;
            PopulateComboBoxes();
            PopulateReportTypes();
            PopulateTrainerComboBox();
        }
        private void PopulateComboBoxes()
        {
            PopulateGymComboBox();
            PopulateDietPlanComboBox();
            PopulateMachineComboBox();
        }
        private void PopulateReportTypes()
        {
            comboBoxReportType.Items.Add("Members of Specific Gym Trained by Specific Trainer");
            comboBoxReportType.Items.Add("Members of Specific Gym Following Specific Diet Plan");
            comboBoxReportType.Items.Add("Members Across Gyms of Specific Trainer Following Specific Diet Plan");
            comboBoxReportType.Items.Add("Count of Members Using Specific Machines in a Gym on a Given Day");
            comboBoxReportType.Items.Add("Diet Plans with Less Than 500 Calorie Meals as Breakfast");
            comboBoxReportType.Items.Add("Diet Plans with Total Carbohydrate Intake Less Than 300 Grams");
            comboBoxReportType.Items.Add("Workout Plans That Don’t Require Using a Specific Machine");
            comboBoxReportType.Items.Add("Diet Plans Without Peanuts as Allergens");
            comboBoxReportType.Items.Add("Comparison of Total Members in Multiple Gyms in the Past 6 Months");
            comboBoxReportType.Items.Add("Details of members who have had session with specific trainer");
            comboBoxReportType.Items.Add("List of diet plans with meals that contain at least 20 grams of protein per serving.");
            comboBoxReportType.Items.Add("List of workout plans that involve exercises targeting Chest, Back, or Legs, sorted by plan name.");
            comboBoxReportType.Items.Add("List of diet plans that have meals with Nuts as allergen, sorted by diet plan name.");
            comboBoxReportType.Items.Add("List of workout plans that involve exercises with a specific machine, sorted by plan name.");
            comboBoxReportType.Items.Add("Details of members who have followed a specific diet plan in the last 3 months, sorted by join date.");
            comboBoxReportType.Items.Add("Find the total number of a specific membership type acquired by members of a specific gym");
            comboBoxReportType.Items.Add("Query to find routines with plan on weekends");
            comboBoxReportType.Items.Add("List of diet plans that have meals with the highest calorie count, sorted by plan name.");
            comboBoxReportType.Items.Add("Total Revenue of Gyms");
            comboBoxReportType.SelectedIndex = 0;
        }


        private void PopulateDietPlanComboBox()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT diet_plan_name FROM diet";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            comboBoxDietPlan.Items.Add(reader["diet_plan_name"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void PopulateTrainerComboBox()
        {
            try
            {
                comboBoxTrainer.Items.Clear();
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT DISTINCT u.FullName FROM Trainers t JOIN Users u ON t.TrainerID = u.UserID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            comboBoxTrainer.Items.Add(reader["FullName"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        private void comboBoxGym_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Clear existing items in trainer combo box
            comboBoxTrainer.Items.Clear();

            // Populate trainer combo box based on selected gym name
            string selectedGymName = comboBoxGym.Text;
            int gymID = GetGymID(selectedGymName);

            if (gymID != -1)
            {
                PopulateTrainerComboBox(gymID);
            }
        }

        private int GetGymID(string gymName)
        {
            int gymID = -1;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT OwnerID FROM Owners WHERE GymName = @GymName";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@GymName", gymName);
                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            gymID = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            return gymID;
        }
        private void PopulateMachineComboBox()
        {
            comboBoxMachine.Items.Clear();

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT DISTINCT Machine FROM Exercise";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            comboBoxMachine.Items.Add(reader["Machine"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void PopulateTrainerComboBox(int gymID)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT u.FullName FROM Trainers t JOIN Users u ON t.TrainerID = u.UserID WHERE t.TrainerID IN (SELECT trainer FROM worksat WHERE gymid = @GymID)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@GymID", gymID);
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            comboBoxTrainer.Items.Add(reader["FullName"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void PopulateGymComboBox()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT GymName FROM Owners Where Approved=1";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            comboBoxGym.Items.Add(reader["GymName"].ToString());
                        }
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
        private string ConstructGymTrainerQuery()
        {
            string gymName = comboBoxGym.SelectedItem.ToString();
            string trainerName = comboBoxTrainer.SelectedItem.ToString();
            int gymID = GetGymID(gymName);
            int trainerID = GetTrainerID(trainerName, gymID);

            return $"SELECT FullName, Email, Contact FROM Users INNER JOIN Members m ON m.MemberID = Users.UserID INNER JOIN worksat w ON m.GymID = w.gymid INNER JOIN Trainers t ON w.trainer = t.TrainerID WHERE t.TrainerID = {trainerID} AND m.GymID = {gymID};";
        }

        private string ConstructGymDietPlanQuery()
        {
            string gymName = comboBoxGym.SelectedItem.ToString();
            string dietPlan = comboBoxDietPlan.SelectedItem.ToString();

            return $"SELECT FullName, Email, Contact FROM Users INNER JOIN Members m ON m.MemberID = Users.UserID INNER JOIN Owners o ON m.GymID = o.OwnerID INNER JOIN diet d ON m.dietID = d.dietID WHERE o.GymName = '{gymName}' AND d.diet_plan_name = '{dietPlan}';";
        }

        private string ConstructTrainerDietPlanQuery()
        {
            string trainerName = comboBoxTrainer.SelectedItem.ToString();
            string dietPlanName = comboBoxDietPlan.SelectedItem.ToString();

            return $"SELECT u.FullName AS MemberFullName, u.Email AS MemberEmail, u.Contact AS MemberContact, o.GymName FROM Members m INNER JOIN Users u ON m.MemberID = u.UserID INNER JOIN Owners o ON m.GymID = o.OwnerID INNER JOIN worksat w ON o.OwnerID = w.gymid INNER JOIN Trainers t ON w.trainer = t.TrainerID INNER JOIN diet d ON m.dietID = d.dietID WHERE t.TrainerID = (SELECT UserID FROM Users WHERE FullName = '{trainerName}') AND d.diet_plan_name = '{dietPlanName}';";
        }


        private string ConstructMachineCountQuery()
        {
            string machineName = comboBoxMachine.SelectedItem.ToString();
            string day = comboBoxDay.Text; // Assuming you have a TextBox for day input
            int gymID = GetGymID(comboBoxGym.Text);

            return $"SELECT COUNT(*) AS MEMBERCOUNT FROM members m JOIN WorkoutPlans wp ON m.MemberID = wp.CreatorID JOIN ROUTINE r ON wp.PlanID = r.PlanID JOIN Exercise e ON r.ExerciseID = e.ExerciseID WHERE m.GymID = {gymID} AND r.Day1 = '{day}' AND e.Machine = '{machineName}' GROUP BY e.Machine;";
        }

        private string ConstructWorkoutPlansQuery()
        {
            string machineName = comboBoxMachine.SelectedItem.ToString();

            return $"SELECT p.PlanName FROM WorkoutPlans p WHERE NOT EXISTS (SELECT 1 FROM ROUTINE r JOIN Exercise e ON r.ExerciseID = e.ExerciseID WHERE p.PlanID = r.PlanID AND e.Machine = '{machineName}') GROUP BY p.PlanName;";
        }


        private void ExecuteQuery(string query)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    dataGridView2.DataSource = dataTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error executing query: " + ex.Message);
            }
        }

       

        private int GetTrainerID(string trainerName, int gymID)
        {
            int trainerID = -1;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT t.TrainerID FROM Trainers t JOIN Users u ON t.TrainerID = u.UserID WHERE u.FullName = @FullName AND t.TrainerID IN (SELECT trainer FROM worksat WHERE gymid = @GymID)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FullName", trainerName);
                        command.Parameters.AddWithValue("@GymID", gymID);
                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            trainerID = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            return trainerID;
        }
        private void AdminReports_Load(object sender, EventArgs e)
        {

        }

        private void buttonGenerateReport_Click(object sender, EventArgs e)
        {
            string selectedReport = comboBoxReportType.SelectedItem.ToString();

            string query = "";

            switch (selectedReport)
            {
                case "Members of Specific Gym Trained by Specific Trainer":
                    query = ConstructGymTrainerQuery();
                    break;

                case "Members of Specific Gym Following Specific Diet Plan":
                    query = ConstructGymDietPlanQuery();
                    break;

                case "Members Across Gyms of Specific Trainer Following Specific Diet Plan":
                    query = ConstructTrainerDietPlanQuery();
                    break;

                case "Count of Members Using Specific Machines in a Gym on a Given Day":
                    query = ConstructMachineCountQuery();
                    break;

                case "Diet Plans with Less Than 500 Calorie Meals as Breakfast":
                    query = "SELECT d.diet_plan_name FROM diet d INNER JOIN meal m ON d.dietID = m.dietplanID WHERE m.meal_name = 'Breakfast' GROUP BY d.diet_plan_name HAVING SUM(calories) < 500;";
                    break;

                case "Diet Plans with Total Carbohydrate Intake Less Than 300 Grams":
                    query = "SELECT d.diet_plan_name FROM diet d INNER JOIN meal m ON d.dietID = m.dietplanID GROUP BY d.diet_plan_name HAVING SUM(Carbs) < 300;";
                    break;

                case "Workout Plans That Don’t Require Using a Specific Machine":
                    query = ConstructWorkoutPlansQuery();
                    break;

                case "Diet Plans Without Peanuts as Allergens":
                    query = "SELECT d.diet_plan_name FROM diet d WHERE d.dietID NOT IN (SELECT dietplanID FROM meal m WHERE m.allergens LIKE '%peanut%');";
                    break;

                case "Comparison of Total Members in Multiple Gyms in the Past 6 Months":
                    query = "SELECT o.GymName, COUNT(m.MemberID) AS TotalMembers FROM Owners o INNER JOIN Members m ON m.GymID = o.OwnerID WHERE m.JoinDate >= DATEADD(MONTH, -6, GETDATE()) GROUP BY o.GymName ORDER BY TotalMembers DESC;";
                    break;

                case "Details of members who have had session with specific trainer":
                    query = ConstructSpecificTrainerSessionQuery();
                    break;

                case "List of diet plans with meals that contain at least 20 grams of protein per serving.":
                    query = "SELECT DISTINCT d.diet_plan_name, d.CreatorID FROM diet d JOIN meal m ON d.dietID = m.dietplanID WHERE m.Protein >= 20;";
                    break;

                case "List of workout plans that involve exercises targeting Chest, Back, or Legs, sorted by plan name.":
                    query = "SELECT Distinct wp.PlanName, wp.CreatorID FROM WorkoutPlans wp JOIN ROUTINE r ON wp.PlanID = r.PlanID JOIN Exercise e ON r.ExerciseID = e.ExerciseID WHERE e.TargetMuscle IN ('Chest', 'Back', 'Legs') ORDER BY wp.PlanName;";
                    break;

                case "List of diet plans that have meals with a Nuts as allergen, sorted by diet plan name.":
                    query = "SELECT DISTINCT d.diet_plan_name, d.CreatorID FROM diet d JOIN meal m ON d.dietID = m.dietplanID WHERE m.allergens NOT LIKE '%Nuts%' ORDER BY d.diet_plan_name;";
                    break;

                case "List of workout plans that involve exercises with a specific machine, sorted by plan name.":
                    query = ConstructMachineWorkoutPlansQuery();
                    break;

                case "Details of members who have followed a specific diet plan in the last 3 months, sorted by join date.":
                    query = ConstructSpecificDietPlanQuery();
                    break;

                case "Find the total number of a specific membership type acquired by members of a specific gym":
                    query = ConstructMembershipTypeQuery();
                    break;

                case "Query to find routines with plan on weekends":
                    query = "SELECT DISTINCT WorkoutPlans.PlanID, WorkoutPlans.PlanName, WorkoutPlans.CreatorID FROM WorkoutPlans JOIN ROUTINE ON WorkoutPlans.PlanID = ROUTINE.PlanID WHERE ROUTINE.Day1 IN ('Saturday', 'Sunday');";
                    break;

                case "Total Revenue of Gyms":
                    query = "SELECT SUM(m.cost), Owners.GymName FROM Owners JOIN Members ON Members.GymID = Owners.OwnerID JOIN Membership m ON m.mID = Members.mID GROUP BY Owners.GymName;";
                    break;

                case "List of diet plans that have meals with the highest calorie count, sorted by plan name.":
                    query = "SELECT d.diet_plan_name, d.CreatorID, MAX(m.calories) AS MaxCalories FROM diet d JOIN meal m ON d.dietID = m.dietplanID GROUP BY d.diet_plan_name, d.CreatorID ORDER BY MaxCalories DESC;";
                    break;

                default:
                    MessageBox.Show("Selected report not supported.");
                    return;
            }

            ExecuteQuery(query);
        }

        private string ConstructSpecificTrainerSessionQuery()
        {
            string trainerName = comboBoxTrainer.SelectedItem.ToString();

            return $"SELECT u.FullName, u.Email, u.Contact FROM Users u JOIN Members m on m.MemberID = u.UserID JOIN PersonalTrainingSessions pts ON m.MemberID = pts.MemberID JOIN Trainers t ON pts.TrainerID = t.TrainerID WHERE U.FullName Like '{trainerName}';";
        }

        private string ConstructMachineWorkoutPlansQuery()
        {
            string machineName = comboBoxMachine.SelectedItem.ToString();

            return $"SELECT DISTINCT wp.PlanName, wp.CreatorID FROM WorkoutPlans wp JOIN ROUTINE r ON wp.PlanID = r.PlanID JOIN Exercise e ON r.ExerciseID = e.ExerciseID WHERE e.Machine = '{machineName}' ORDER BY wp.PlanName;";
        }

        private string ConstructSpecificDietPlanQuery()
        {
            string dietPlan = comboBoxDietPlan.SelectedItem.ToString();

            return $"SELECT u.FullName, u.Email, u.Contact FROM Users u JOIN Members m on m.MemberID = u.UserID JOIN diet d ON m.dietID = d.dietID WHERE d.diet_plan_name = '{dietPlan}' AND m.JoinDate >= DATEADD(MONTH, -3, GETDATE()) ORDER BY m.JoinDate;";
        }

        private string ConstructMembershipTypeQuery()
        {
            string membershipType = comboBoxMembership.SelectedItem.ToString();
            string gymName = comboBoxGym.SelectedItem.ToString();

            return $"SELECT m.mID,ms.type as Type, COUNT(*) AS Membership FROM Members m JOIN Owners on m.GymID = Owners.OwnerID Join Membership ms on ms.mID=m.mID WHERE ms.type Like '{membershipType}' AND GymID = {GetGymID(gymName)} GROUP BY m.mID,ms.type";
        }


        private void comboBoxReportType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Reset all combo boxes
            comboBoxGym.Visible = false;
            comboBoxTrainer.Visible = false;
            comboBoxDietPlan.Visible = false;
            comboBoxMachine.Visible = false;
            comboBoxDay.Visible = false;
            comboBoxMembership.Visible = false;
            Gym.Visible = false;
            Trainer.Visible = false;
            Diet.Visible = false;
            Machine.Visible = false;
            Day.Visible = false;
            Membership.Visible = false;

            // Show the necessary combo boxes based on the selected report type
            switch (comboBoxReportType.SelectedItem.ToString())
            {
                case "Members of Specific Gym Trained by Specific Trainer":
                    comboBoxGym.Visible = true;
                    comboBoxTrainer.Visible = true;
                    Gym.Visible = true;
                    Trainer.Visible = true;
                    break;
                case "Members of Specific Gym Following Specific Diet Plan":
                    comboBoxGym.Visible = true;
                    comboBoxDietPlan.Visible = true;
                    Gym.Visible = true;
                    Diet.Visible = true;
                    break;
                case "Members Across Gyms of Specific Trainer Following Specific Diet Plan":
                    comboBoxTrainer.Visible = true;
                    comboBoxDietPlan.Visible = true;
                    Trainer.Visible = true;
                    Diet.Visible = true;
                    PopulateTrainerComboBox();
                    break;
                case "Count of Members Using Specific Machines in a Gym on a Given Day":
                    comboBoxGym.Visible = true;
                    comboBoxMachine.Visible = true;
                    comboBoxDay.Visible = true;
                    Day.Visible = true;
                    Gym.Visible = true;
                    Machine.Visible = true;
                    break;
                case "Workout Plans That Don’t Require Using a Specific Machine":
                    comboBoxMachine.Visible = true; // Show the combo box for selecting the machine
                    Machine.Visible = true;
                    break;
                case "Details of members who have had session with specific trainer":
                    comboBoxTrainer.Visible = true; // Show the combo box for selecting the trainer
                    Trainer.Visible = true;
                    break;
                case "List of diet plans with meals that contain at least 20 grams of protein per serving.":
                    break;
                case "List of workout plans that involve exercises targeting Chest, Back, or Legs, sorted by plan name.":
                    break;
                case "List of diet plans that have meals with Nuts as allergen, sorted by diet plan name.":
                    break;
                case "List of workout plans that involve exercises with a specific machine, sorted by plan name.":
                    comboBoxMachine.Visible = true; // Show the combo box for selecting the machine
                    Machine.Visible = true;
                    break;
                case "Details of members who have followed a specific diet plan in the last 3 months, sorted by join date.":
                    comboBoxDietPlan.Visible = true; // Show the combo box for selecting the diet plan
                    Diet.Visible = true;
                    break;
                case "Find the total number of a specific membership type acquired by members of a specific gym":
                    comboBoxGym.Visible = true; // Show the combo box for selecting the gym
                    comboBoxMembership.Visible = true; // Show the combo box for selecting the membership type
                    Membership.Visible = true;
                    Gym.Visible = true;
                    break;
                default:
                    break;
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            AdminInterface form = new AdminInterface(userID);
            form.Show();
            this.Hide();
        }

        private void comboBoxMembership_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
