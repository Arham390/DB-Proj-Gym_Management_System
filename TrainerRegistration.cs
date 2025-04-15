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
    public partial class TrainerRegistration : Form
    {
        private string ConnectionString = GlobalSettings.GetConnectionString();
        public TrainerRegistration()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void TrainerRegistration_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Retrieve input values from text boxes and combo boxes
            string firstName = textBox1.Text;
            string lastName = textBox2.Text;
            string email = textBox4.Text;
            string password = textBox3.Text;
            string gender = comboBox1.Text;
            string exp = comboBox3.Text; // Get the selected gym name
            int height = Convert.ToInt32(textBox7.Text);
            int weight = Convert.ToInt32(textBox6.Text);
            string contact = textBox5.Text;
            string spec = comboBox2.Text;

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
                        command.Parameters.AddWithValue("@UserType", "Trainer"); // Assuming the user is registering as a member
                        command.Parameters.AddWithValue("@Contact", contact);

                        // Execute the command and get the generated UserID
                        int userID = Convert.ToInt32(command.ExecuteScalar());

                        

                            // Insert into Members table
                            string memberQuery = @"INSERT INTO Trainers (TrainerID,Height, Weight,Spec,Experience) 
                                           VALUES (@MemberID,@Height, @Weight,@spec,@exp)";
                            using (SqlCommand memberCommand = new SqlCommand(memberQuery, connection))
                            {
                                // Retrieve the mID based on membership type
                                
                                    

                                    // Insert data into Members table
                                    memberCommand.Parameters.AddWithValue("@MemberID", userID);
                                    memberCommand.Parameters.AddWithValue("@spec", spec);
                                    memberCommand.Parameters.AddWithValue("@exp", exp);
                                    memberCommand.Parameters.AddWithValue("@Height", height);
                                    memberCommand.Parameters.AddWithValue("@Weight", weight);

                                    memberCommand.ExecuteNonQuery();
                                
                            }
                        
                        MessageBox.Show("Registration successful!");
                        using (TrainerLogin Form = new TrainerLogin())
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

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (TrainerLogin Form = new TrainerLogin())
            {
                this.Hide();
                Form.ShowDialog();
            }
        }
    }
}
