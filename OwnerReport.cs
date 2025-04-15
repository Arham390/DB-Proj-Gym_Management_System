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
    public partial class OwnerReport : Form
    {
        private string connectionString=GlobalSettings.GetConnectionString();
        int userID;
        public OwnerReport(int u)
        {
            InitializeComponent();
            userID= u;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void OwnerReport_Load(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT FullName, Email, Contact, m.JoinDate, m.Weight, m.Height, ms.type as Type, ms.duration as Duration, ms.cost as Cost
                        FROM users
                        INNER JOIN Members m ON m.MemberID = users.UserID
                        INNER JOIN Owners o ON m.GymID = o.OwnerID
                        INNER JOIN Membership ms ON m.mID = ms.mID
                        WHERE m.JoinDate >= DATEADD(MONTH, -3, GETDATE()) and o.OwnerID = @userID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userID", userID);
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        dataGridView1.DataSource = dataTable;
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
            OwnerInterface form = new OwnerInterface(userID);
            form.Show();
            this.Hide();
        }
    }
}
