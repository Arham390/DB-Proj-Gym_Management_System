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
    public partial class AdminLogin : Form
    {
        private  string ConnectionString = GlobalSettings.GetConnectionString();

        public AdminLogin()
        {
            InitializeComponent();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string email = textBox1.Text;
            string password = textBox2.Text;

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT UserID FROM Users WHERE Email = @Email AND PasswordHash = @Password"; // Select only the UserID
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Password", password); // Remember to hash the password before storing it

                        object result = command.ExecuteScalar(); // ExecuteScalar returns the first column of the first row

                        if (result != null) // Check if a UserID was returned
                        {
                            int userId = (int)result; // Cast the result to int
                            MessageBox.Show("Login successful!");

                            // Pass the user ID to MemberInterface when creating an instance
                            using (AdminInterface Form = new AdminInterface(userId))
                            {
                                this.Hide();
                                Form.ShowDialog();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Invalid email or password. Please try again.");
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

        private void button2_Click(object sender, EventArgs e)
        {
            using (Form1 Form = new Form1())
            {
                this.Hide();
                Form.ShowDialog();
            }
        }
    }
}
