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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Deliverable_2
{
    public partial class MemberRegistration : Form
    {
        private string ConnectionString = GlobalSettings.GetConnectionString();

        public MemberRegistration()
        {
            InitializeComponent();
        }
        private void label1_Click(object sender, EventArgs e)
        {
            // Add your logic here
        }

        private void label2_Click(object sender, EventArgs e)
        {
            // Add your logic here
        }

        private void label3_Click(object sender, EventArgs e)
        {
            // Add your logic here
        }

        private void label4_Click(object sender, EventArgs e)
        {
            // Add your logic here
        }

        private void label5_Click(object sender, EventArgs e)
        {
            // Add your logic here
        }

        private void label6_Click(object sender, EventArgs e)
        {
            // Add your logic here
        }

        private void label7_Click(object sender, EventArgs e)
        {
            // Add your logic here
        }

        private void label8_Click(object sender, EventArgs e)
        {
            // Add your logic here
        }

        private void label9_Click(object sender, EventArgs e)
        {
            // Add your logic here
        }

        private void label10_Click(object sender, EventArgs e)
        {
            // Add your logic here
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Add your logic here
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            // Add your logic here
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            // Add your logic here
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            // Add your logic here
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            // Add your logic here
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            // Add your logic here
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            // Add your logic here
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Add your logic here
        }

        

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Add your logic here
        }



        private void button1_Click(object sender, EventArgs e)
        {
            // Retrieve input values from text boxes and combo boxes
            string firstName = textBox1.Text;
            string lastName = textBox2.Text;
            string email = textBox4.Text;
            string password = textBox3.Text;
            string gender = comboBox1.Text;
            string gymName = comboBox3.Text; // Get the selected gym name
            int height = Convert.ToInt32(textBox7.Text);
            int weight = Convert.ToInt32(textBox6.Text);
            string contact = textBox5.Text;
            string membershipType = comboBox2.Text;

            // Insert data into the database
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = @"INSERT INTO Users (PasswordHash, Email, FullName, Gender, UserType, Contact) 
                             VALUES (@Password, @Email, @FullName, @Gender, @UserType, @Contact);
                             SELECT SCOPE_IDENTITY();"; // Retrieve the generated UserID
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Password", password); // Remember to hash the password before storing it
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@FullName", firstName + " " + lastName);
                        command.Parameters.AddWithValue("@Gender", gender);
                        command.Parameters.AddWithValue("@UserType", "Member"); // Assuming the user is registering as a member
                        command.Parameters.AddWithValue("@Contact", contact);

                        // Execute the command and get the generated UserID
                        int userID = Convert.ToInt32(command.ExecuteScalar());

                        // Retrieve GymID based on GymName
                        string gymIDQuery = "SELECT OwnerID FROM Owners WHERE GymName = @GymName";
                        using (SqlCommand gymIDCommand = new SqlCommand(gymIDQuery, connection))
                        {
                            gymIDCommand.Parameters.AddWithValue("@GymName", gymName);
                            int gymID = Convert.ToInt32(gymIDCommand.ExecuteScalar());

                            // Insert into Members table
                            string memberQuery = @"INSERT INTO Members (MemberID, GymID, mID, JoinDate, Height, Weight) 
                                           VALUES (@MemberID, @GymID, @MembershipID, @JoinDate, @Height, @Weight)";
                            using (SqlCommand memberCommand = new SqlCommand(memberQuery, connection))
                            {
                                // Retrieve the mID based on membership type
                                string membershipIDQuery = "SELECT mID FROM Membership WHERE type = @MembershipType";
                                using (SqlCommand membershipIDCommand = new SqlCommand(membershipIDQuery, connection))
                                {
                                    membershipIDCommand.Parameters.AddWithValue("@MembershipType", membershipType);
                                    int membershipID = Convert.ToInt32(membershipIDCommand.ExecuteScalar());

                                    // Insert data into Members table
                                    memberCommand.Parameters.AddWithValue("@MemberID", userID);
                                    memberCommand.Parameters.AddWithValue("@GymID", gymID);
                                    memberCommand.Parameters.AddWithValue("@MembershipID", membershipID);
                                    memberCommand.Parameters.AddWithValue("@JoinDate", DateTime.Now); // Current date
                                    memberCommand.Parameters.AddWithValue("@Height", height);
                                    memberCommand.Parameters.AddWithValue("@Weight", weight);

                                    memberCommand.ExecuteNonQuery();
                                }
                            }
                        }
                        MessageBox.Show("Registration successful!");
                        using (Form2 Form = new Form2())
                        {
                            this.Hide();
                            Form.ShowDialog();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void MemberRegistration_Load(object sender, EventArgs e)
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
            using (Form2 Form = new Form2())
            {
                this.Hide();
                Form.ShowDialog();
            }
        }
    }
}
