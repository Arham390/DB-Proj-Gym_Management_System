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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace Deliverable_2
{
    public partial class MemberNoGym : Form
    {
        private int userID;
        private string ConnectionString = GlobalSettings.GetConnectionString();
        public MemberNoGym(int userID)
        {
            InitializeComponent();
            this.userID = userID;
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void MemberNoGym_Load(object sender, EventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                // Open the connection
                connection.Open();

                // Create a SqlCommand to execute your SQL query
                string query = "SELECT GymName FROM Owners Where Approved=1"; // Change the query according to your table and column names
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Create a DataTable to hold the results of the query
                    DataTable dataTable = new DataTable();

                    // Use a SqlDataAdapter to fill the DataTable with the results of the query
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }

                    // Set the DataSource of your ComboBox to the DataTable
                    comboBox3.DataSource = dataTable;

                    // Set the DisplayMember property of your ComboBox to the column you want to display
                    comboBox3.DisplayMember = "GymName"; // Change "type" to the appropriate column name
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form1 form = new Form1();
            form.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string gymName = comboBox3.Text;
            string MemberShip = comboBox2.Text;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    string gymIDQuery = "SELECT OwnerID FROM Owners WHERE GymName = @GymName";
                    using (SqlCommand gymIDCommand = new SqlCommand(gymIDQuery, connection))
                    {
                        gymIDCommand.Parameters.AddWithValue("@GymName", gymName);
                        int gymID = Convert.ToInt32(gymIDCommand.ExecuteScalar());

                        string memberQuery = "UPDATE Members SET GymID = @NewGymID, mID = @NewMembershipID WHERE MemberID = @MemberID";
                        using (SqlCommand memberCommand = new SqlCommand(memberQuery, connection))
                        {
                            memberCommand.Parameters.AddWithValue("@NewGymID", gymID); // Add this line to define the parameter
                            memberCommand.Parameters.AddWithValue("@MemberID", userID);

                            string membershipIDQuery = "SELECT mID FROM Membership WHERE type = @MembershipType";
                            using (SqlCommand membershipIDCommand = new SqlCommand(membershipIDQuery, connection))
                            {
                                membershipIDCommand.Parameters.AddWithValue("@MembershipType", comboBox2.Text);

                                int membershipID = Convert.ToInt32(membershipIDCommand.ExecuteScalar());

                                memberCommand.Parameters.AddWithValue("@NewMembershipID", membershipID);

                                memberCommand.ExecuteNonQuery();
                            }
                        }
                    }

                    MessageBox.Show("Gym Updated!");

                    using (MemberInterface Form = new MemberInterface(userID))
                    {
                        this.Hide();
                        Form.ShowDialog();
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
