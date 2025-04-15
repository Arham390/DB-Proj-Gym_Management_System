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
    public partial class OwnerRegsitration : Form
    {
        private string ConnectionString = GlobalSettings.GetConnectionString();

        public OwnerRegsitration()
        {
            InitializeComponent();
        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string firstName = textBox1.Text;
            string email = textBox4.Text;
            string password = textBox3.Text;
            string gender = comboBox1.Text;
            string gymName = textBox2.Text; // Get the selected gym name
            string contact = textBox5.Text;

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
                        command.Parameters.AddWithValue("@FullName",firstName);
                        command.Parameters.AddWithValue("@Gender", gender);
                        command.Parameters.AddWithValue("@UserType", "Owner"); // Assuming the user is registering as a member
                        command.Parameters.AddWithValue("@Contact", contact);

                        // Execute the command and get the generated UserID
                        int userID = Convert.ToInt32(command.ExecuteScalar());

                        

                            // Insert into Owners table
                            string memberQuery = @"INSERT INTO Owners (OwnerID, GymName, Approved) 
                                           VALUES (@MemberID, @GymID,0)";
                            using (SqlCommand memberCommand = new SqlCommand(memberQuery, connection))
                            {
                                // Retrieve the mID based on membership type
                                
                                    

                                    // Insert data into Members table
                                    memberCommand.Parameters.AddWithValue("@MemberID", userID);
                                    memberCommand.Parameters.AddWithValue("@GymID", gymName);
                                    memberCommand.ExecuteNonQuery();
                                
                            }
                        
                        MessageBox.Show("Registration successful!");
                        using (GymOwnerLogin Form = new GymOwnerLogin())
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

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void OwnerRegsitration_Load(object sender, EventArgs e)
        {

        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (GymOwnerLogin Form = new GymOwnerLogin())
            {
                this.Hide();
                Form.ShowDialog();
            }
        }
    }
}
