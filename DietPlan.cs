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
    public partial class DietPlan : Form
    {
        private string ConnectionString = GlobalSettings.GetConnectionString();
        int UserID;
        private Dictionary<string, List<Meal>> dietPlan = new Dictionary<string, List<Meal>>();
        private bool up;
        private Form callingForm;
        private class Meal
        {
            public string MealName { get; set; }
            public decimal Protein { get; set; }
            public decimal Carbs { get; set; }
            public decimal Fiber { get; set; }
            public decimal Fat { get; set; }
            public decimal Calories { get; set; }
            public string Allergens { get; set; }
        }

        public DietPlan(int userID,bool a,Form b)
        {
            InitializeComponent();
            UserID = userID;
            up = a;
            callingForm = b;
        }




        

        private void ClearInputFields()
        {
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            textBox6.Text = "";
        }

        

        private void SaveDietPlanToDatabase(string dietPlanName, Dictionary<string, List<Meal>> dietPlan)
{
    using (SqlConnection connection = new SqlConnection(ConnectionString))
    {
        connection.Open();

        // Insert diet plan into diet table
        SqlCommand dietPlanCommand = new SqlCommand("INSERT INTO diet (diet_plan_name, CreatorID) VALUES (@DietPlanName, @CreatorID);SELECT SCOPE_IDENTITY();", connection);
        dietPlanCommand.Parameters.AddWithValue("@DietPlanName", dietPlanName);
        dietPlanCommand.Parameters.AddWithValue("@CreatorID", UserID);
                object result = dietPlanCommand.ExecuteScalar();
                int dietID = 0;
                if (result != null && result != DBNull.Value)
                {
                    dietID = Convert.ToInt32(result);
                    if (up)
                    {
                        string updateMemberQuery = "UPDATE Members SET dietID = @dietID WHERE MemberID = @UserID;";
                        using (SqlCommand updateCommand = new SqlCommand(updateMemberQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@dietID", dietID);
                            updateCommand.Parameters.AddWithValue("@UserID", UserID);
                            updateCommand.ExecuteNonQuery();
                        }
                    }
                }
                // Insert meals into meal table
                foreach (var kvp in dietPlan)
        {
            string day = kvp.Key;
            List<Meal> meals = kvp.Value;

            foreach (Meal m in meals)
            {
                SqlCommand command = new SqlCommand("INSERT INTO meal (meal_name, Protein, Carbs, Fiber, Fat, calories, allergens, Day1, dietplanID) VALUES (@MealName, @Protein, @Carbs, @Fiber, @Fat, @Calories, @Allergens, @Day , (SELECT MAX(dietID) FROM diet))", connection);
                command.Parameters.AddWithValue("@MealName", m.MealName);
                command.Parameters.AddWithValue("@Protein", m.Protein);
                command.Parameters.AddWithValue("@Carbs", m.Carbs);
                command.Parameters.AddWithValue("@Fiber", m.Fiber);
                command.Parameters.AddWithValue("@Fat", m.Fat);
                command.Parameters.AddWithValue("@Calories", m.Calories);
                command.Parameters.AddWithValue("@Allergens", m.Allergens);
                command.Parameters.AddWithValue("@Day", day);
                command.ExecuteNonQuery();
            }
        }
    }
}



        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }
        private void DietPlan_Load(object sender, EventArgs e)
        {

        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string day = comboBox1.SelectedItem?.ToString();
            string mealName = comboBox3.SelectedItem?.ToString();

            // Check if day and meal name are selected
            if (string.IsNullOrEmpty(day) || string.IsNullOrEmpty(mealName))
            {
                MessageBox.Show("Please select a day and meal type.");
                return;
            }

            // Check if a meal type is already added for the selected day
            if (dietPlan.ContainsKey(day) && dietPlan[day].Exists(m => m.MealName == mealName))
            {
                MessageBox.Show("A meal type for this day already exists.");
                return;
            }

            // Create Meal object
            Meal meal = new Meal
            {
                MealName = mealName,
                Protein = Convert.ToDecimal(textBox2.Text),
                Carbs = Convert.ToDecimal(textBox3.Text),
                Fiber = Convert.ToDecimal(textBox4.Text),
                Fat = Convert.ToDecimal(textBox5.Text),
                Calories = Convert.ToDecimal(textBox6.Text),
                Allergens = comboBox2.SelectedItem?.ToString() ?? "None"
            };

            if (!dietPlan.ContainsKey(day))
            {
                dietPlan.Add(day, new List<Meal>());
            }
            dietPlan[day].Add(meal);

            ClearInputFields();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string dietPlanName = textBox1.Text;

            // Check if there are any meals added to the diet plan
            if (dietPlan.Count == 0)
            {
                MessageBox.Show("Please add at least one meal to the diet plan.");
                return;
            }

            // Save diet plan to the database
            SaveDietPlanToDatabase(dietPlanName, dietPlan);
            if (callingForm is MemberInterface)
            {
                using (MemberInterface Form = new MemberInterface(UserID))
                {
                    this.Hide();
                    Form.ShowDialog();
                }
            }
            else if (callingForm is TrainerDietPlan)
            {
                using (TrainerDietPlan Form = new TrainerDietPlan(UserID))
                {
                    this.Hide();
                    Form.ShowDialog();
                }
            }
            else if (callingForm is WorkoutPlanSelect)
            {
                using (DietPlanSelection Form = new DietPlanSelection(UserID))
                {
                    this.Hide();
                    Form.ShowDialog();
                }
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (callingForm is MemberInterface)
            {
                using (MemberInterface Form = new MemberInterface(UserID))
                {
                    this.Hide();
                    Form.ShowDialog();
                }
            }
            else if (callingForm is TrainerDietPlan)
            {
                using (TrainerDietPlan Form = new TrainerDietPlan(UserID))
                {
                    this.Hide();
                    Form.ShowDialog();
                }
            }
            else if (callingForm is DietPlanSelection)
            {
                using (DietPlanSelection Form = new DietPlanSelection(UserID))
                {
                    this.Hide();
                    Form.ShowDialog();
                }
            }
        }
    }
}
