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
    public partial class DietPlanSelection : Form
    {
        private DataTable originalDataTable=new DataTable();

        int userID;
        private string ConnectionString = GlobalSettings.GetConnectionString();

        public DietPlanSelection(int userID)
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
                    string query = "SELECT * FROM DIET";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        DataTable dataTable = new DataTable();
                        SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                        dataAdapter.Fill(originalDataTable);

                        dataGridView3.DataSource = originalDataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            
        }
        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }



        private void DietPlanSelection_Load(object sender, EventArgs e)
        {

        }

        private void DietPlanSelection_Load_1(object sender, EventArgs e)
        {

        }
        private void PopulateRoutines(int planID)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT DAY1 AS DAY,MEAL_NAME AS MEAL_TYPE,PROTEIN,CARBS,FIBER,FAT,CALORIES AS CALORIES,ALLERGENS AS ALLERGENS FROM MEAL WHERE dietplanID=@dietID ";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@dietID", planID);
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

        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
                if (dataGridView3.SelectedRows.Count > 0)
                {
                    // Assuming "PlanID" is the correct column name, change it if necessary
                    int selectedPlanID = Convert.ToInt32(dataGridView3.SelectedRows[0].Cells[0].Value);

                    PopulateRoutines(selectedPlanID);
                }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MemberInterface memberInterfaceForm = new MemberInterface(userID);
            memberInterfaceForm.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView3.SelectedRows.Count > 0)
            {
                int selectedPlanID = Convert.ToInt32(dataGridView3.SelectedRows[0].Cells[0].Value);
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        string query = "UPDATE MEMBERS SET DIETID = @planid WHERE MEMBERID = @userID";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@planid", selectedPlanID);
                            command.Parameters.AddWithValue("@userID", userID);
                            command.ExecuteNonQuery();
                        }
                    }
                    MessageBox.Show("Diet plan updated.");
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
            DietPlan memberInterfaceForm = new DietPlan(userID,false,this);
            memberInterfaceForm.Show();
            this.Hide();
        }
        
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            DataView a = originalDataTable.DefaultView;
            a.RowFilter = "diet_plan_name Like '%" + textBox1.Text + "%'";
        }
    }
}
