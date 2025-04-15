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

    public partial class OwnerMemberManagement : Form
    {
        private string ConnectionString = GlobalSettings.GetConnectionString();
        private DataTable memTab=new DataTable();
        private int uID;
         private int selectID;

        public OwnerMemberManagement(int userID)
        {
            InitializeComponent();
            uID = userID;
            filltable();
        }
        private void filltable()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = @"SELECT M.MemberID, U.FullName AS MemberName ,U.Email, U.Gender, U.Contact,M.Height, M.Weight, 
       MS.type AS MembershipType, MS.duration AS MembershipDuration, MS.cost AS MembershipCost, M.JoinDate
FROM Members M
INNER JOIN Users U ON M.MemberID = U.UserID
INNER JOIN Membership MS ON M.mID = MS.mID
WHERE M.GymID = @gymID;
";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@gymID", uID);
                        SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                        dataAdapter.Fill(memTab);

                        dataGridView2.DataSource = memTab;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void label2_Click(object sender, EventArgs e)
        {

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
            OwnerInterface form=new OwnerInterface(uID);
            form.Show();
            this.Hide();

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            DataView memTab1 = memTab.DefaultView;
            memTab1.RowFilter = "MemberName Like '%" + textBox1.Text + "%'";
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
                        string query = @"UPDATE Members SET GymID = NULL WHERE MEMBERID=@MemID";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@MemID", selectID);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
                memTab.Clear(); 
                filltable();
                MessageBox.Show("Membership Revoked");
            }
            else
            {
                MessageBox.Show("No Member Selected");
            }
        }

        private void OwnerMemberManagement_Load(object sender, EventArgs e)
        {

        }
    }
    }

