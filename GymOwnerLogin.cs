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
    public partial class GymOwnerLogin : Form
    {
        private string ConnectionString = GlobalSettings.GetConnectionString();

        public GymOwnerLogin()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
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
                    string query = "SELECT U.UserID, O.Approved FROM Users U INNER JOIN Owners O ON U.UserID = O.OwnerID WHERE U.Email = @Email AND U.PasswordHash = @Password"; // Select the UserID and Approved from the Owner table
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Password", password); // Remember to hash the password before storing it

                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            int userId = reader.GetInt32(0); // Get the UserID
                            int approved = reader.GetInt32(1); // Get the Approved value

                            if (approved == 1)
                            {
                                MessageBox.Show("Login successful!");
                                using (OwnerInterface Form = new OwnerInterface(userId))
                                {
                                    this.Hide();
                                    Form.ShowDialog();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Your registration is pending approval.");
                                using (PendingApprovalForm Form = new PendingApprovalForm())
                                {
                                    this.Hide();
                                    Form.ShowDialog();
                                }
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

        private void label5_Click(object sender, EventArgs e)
        {
            using (OwnerRegsitration Form = new OwnerRegsitration())
            {
                this.Hide();
                Form.ShowDialog();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (Form1 Form = new Form1())
            {
                this.Hide();
                Form.ShowDialog();
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

       

        
    }
}
