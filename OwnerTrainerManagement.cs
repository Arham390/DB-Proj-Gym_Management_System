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
    public partial class OwnerTrainerManagement : Form
    {
        private string ConnectionString = GlobalSettings.GetConnectionString();
        private DataTable trainerDataTable = new DataTable();
        int userID;
        int selectedTrainerID;
        public OwnerTrainerManagement(int u)
        {
            InitializeComponent();
            userID = u;
            PopulateDataGridView();

        }
        private void PopulateDataGridView()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = @"SELECT 
                                T.TrainerID, 
                                U.FullName AS TrainerName, 
                                AVG(TR.Rating) AS AvgRating,
                                T.Weight,
                                T.Height,
                                T.Experience,
                                T.Spec as Specialization
                            FROM 
                                Trainers T
                            INNER JOIN 
                                WorksAt WA ON T.TrainerID = WA.Trainer
                            INNER JOIN 
                                Owners G ON WA.GymID = G.OwnerID
                            INNER JOIN 
                                Users U ON T.TrainerID = U.UserID
                            LEFT JOIN 
                                TrainerRatings TR ON T.TrainerID = TR.TrainerID
                            WHERE 
                                G.OwnerID = @OwnerID
                            GROUP BY 
                                T.TrainerID, U.FullName, T.Weight, T.Height, T.Experience, T.Spec
                            ORDER BY 
                                AvgRating " + sortOrder;
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OwnerID", userID);
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(trainerDataTable);
                        dataGridView2.DataSource = trainerDataTable;
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

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            DataView view = trainerDataTable.DefaultView;
            view.RowFilter = string.Format("TrainerName LIKE '%{0}%'", textBox1.Text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (selectedTrainerID > 0)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();
                        string query = "DELETE FROM WorksAt WHERE Trainer = @TrainerID AND GymID = @OwnerID";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@TrainerID", selectedTrainerID);
                            command.Parameters.AddWithValue("@OwnerID", userID);
                            command.ExecuteNonQuery();
                        }
                        trainerDataTable.Clear();
                        PopulateDataGridView();
                        MessageBox.Show("Trainer fired successfully.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("No trainer selected.");
            }
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                selectedTrainerID = Convert.ToInt32(dataGridView2.Rows[e.RowIndex].Cells["TrainerID"].Value);
            }
        }


        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        

        

        private void label11_Click(object sender, EventArgs e)
        {

        }
        string sortOrder = "ASC";

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedSortOption = comboBox2.SelectedItem.ToString();
            sortOrder = selectedSortOption == "Ascending" ? "ASC" : "DESC";
            trainerDataTable.Clear();
            PopulateDataGridView(); // Re-populate the DataGridView after changing the sort order
        }
    }
}
