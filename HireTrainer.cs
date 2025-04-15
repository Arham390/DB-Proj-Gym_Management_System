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
    public partial class HireTrainer : Form
    {
        private string ConnectionString = GlobalSettings.GetConnectionString();

        private int userID;
        private int selectID;
        public HireTrainer(int uID)
        {
            InitializeComponent();
            userID = uID;
            Update();
            selectID = -1;
        }

        private void HireTrainer_Load(object sender, EventArgs e)
        {
            
        }
        private void Update() 
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = @"SELECT Users.UserID AS ID, Users.FullName AS Name, Users.Contact, Trainers.Spec AS Specialization, Trainers.Experience 
FROM Trainers 
JOIN Users ON Users.UserID = Trainers.TrainerID 
LEFT JOIN worksat ON worksat.trainer = Trainers.TrainerID AND worksat.gymid = @userID
WHERE worksat.trainer IS NULL";
;
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userID", userID);
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
        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0)
            {
                selectID = Convert.ToInt32(dataGridView2.SelectedRows[0].Cells[0].Value);

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            OwnerInterface form = new OwnerInterface(userID);
            form.Show();
            this.Hide();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (selectID > 0)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();
                        string query = "Insert into Worksat Values(@TrainerID,@OwnerID)";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@TrainerID", selectID);
                            command.Parameters.AddWithValue("@OwnerID", userID);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
                dataGridView2.DataSource = null;
                Update();
                MessageBox.Show("Trainer Hired");
            }
            else
            {
                MessageBox.Show("No Trainer Selected "+selectID);

            }
        }
    }
}
