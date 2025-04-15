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
    public partial class TrainerLogin : Form
    {
        private string ConnectionString = GlobalSettings.GetConnectionString();

        public TrainerLogin()
        {
            InitializeComponent();
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

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

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void TrainerLogin_Load(object sender, EventArgs e)
        {

        }

        private void label5_Click_1(object sender, EventArgs e)
        {
            using (TrainerRegistration Form = new TrainerRegistration())
            {
                this.Hide();
                Form.ShowDialog();
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            string email = textBox1.Text;
            string password = textBox2.Text;

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = @"
                SELECT 
                    UserID
                FROM 
                    Users
                WHERE 
                    Email = @Email AND PasswordHash = @Password";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Password", password);

                        object result = command.ExecuteScalar();

                        if (result != null)
                        {
                            int userId = (int)result;

                            query = @"
                        SELECT 
                            COUNT(*) 
                        FROM 
                            WorksAt
                        WHERE 
                            Trainer = @TrainerID";
                            using (SqlCommand employmentCheckCommand = new SqlCommand(query, connection))
                            {
                                employmentCheckCommand.Parameters.AddWithValue("@TrainerID", userId);

                                int employmentStatus = (int)employmentCheckCommand.ExecuteScalar();

                                if (employmentStatus > 0)
                                {
                                    MessageBox.Show("Login successful! Employment status: Employed");

                                    using (TrainerInterface Form = new TrainerInterface(userId))
                                    {
                                        this.Hide();
                                        Form.ShowDialog();
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Login successful! Employment status: Unemployed");
                                    using (TrainerInterface Form = new TrainerInterface(userId))
                                    {
                                        this.Hide();
                                        Form.ShowDialog();
                                    }
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
