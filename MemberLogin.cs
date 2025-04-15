using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Deliverable_2
{
    public partial class Form2 : Form
    {
        private string ConnectionString = GlobalSettings.GetConnectionString();

        public Form2()
        {
            InitializeComponent();
        }

        private void label2_Click(object sender, EventArgs e)
        {
            // Add your logic here
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Add your logic here
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            // Add your logic here
        }

        private void label5_Click(object sender, EventArgs e)
        {
            using (MemberRegistration Form = new MemberRegistration())
            {
                this.Hide();
                Form.ShowDialog();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Add your logic here
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
                    string query = "SELECT UserID, GymID FROM Members M INNER JOIN Users U ON M.MemberID = U.UserID WHERE U.Email = @Email AND U.PasswordHash = @Password"; // Select UserID and GymID
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Password", password); // Remember to hash the password before storing it

                        SqlDataReader reader = command.ExecuteReader();

                        if (reader.Read()) // Check if a record was returned
                        {
                            int userId = reader.GetInt32(0); // Get UserID from the first column
                            int gymId = reader.IsDBNull(1) ? -1 : reader.GetInt32(1); // Get GymID from the second column, if NULL set to -1

                            // Determine which form to open based on GymID
                            if (gymId == -1) // If GymID is NULL (-1)
                            {
                                MessageBox.Show("Login successful! Opening form for members without a gym.");
                                using (MemberNoGym Form = new MemberNoGym(userId))
                                {
                                    this.Hide();
                                    Form.ShowDialog();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Login successful! Opening form for members with a gym.");
                                using (MemberInterface Form = new MemberInterface(userId))
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
