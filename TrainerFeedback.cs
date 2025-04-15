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
    public partial class TrainerFeedback : Form
    {

        private int userID;
        private string ConnectionString = GlobalSettings.GetConnectionString();
        public TrainerFeedback(int uID)
        {
            InitializeComponent();
            userID = uID;
            DisplayAverageRatings();
        }

        private void TrainerFeedbacl_Load(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = @"SELECT
                        U1.FullName AS MemberName,
                        U2.FullName AS TrainerName,
                        O.GymName,
                        TR.Feedback,
                        TR.Rating
                    FROM
                        TrainerRatings TR
                    INNER JOIN
                        Members M ON TR.MemberID = M.MemberID
                    INNER JOIN
                        Trainers T ON TR.TrainerID = T.TrainerID
                    INNER JOIN
                        Owners O ON M.GymID = O.OwnerID
                    INNER JOIN
                        Users U1 ON M.MemberID = U1.UserID
                    INNER JOIN
                        Users U2 ON T.TrainerID = U2.UserID
                    WHERE
                        T.TrainerID = @TrainerID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TrainerID", userID); // Assuming userID contains the current trainer's ID
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        dataGridView2.DataSource = dataTable;

                        // Set AutoSizeMode property after populating dataGridView2
                        dataGridView2.Columns["Feedback"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
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

        }

        private void button2_Click(object sender, EventArgs e)
        {
            TrainerInterface form=new TrainerInterface(userID);
            form.Show();
            this.Hide();
        }
        private void DisplayAverageRatings()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = @"SELECT
        O.GymName,
        U.FullName AS TrainerName,
        AVG(TR.Rating) AS AverageRating
    FROM
        TrainerRatings TR
    INNER JOIN
        Members M ON TR.MemberID = M.MemberID
    INNER JOIN
        Trainers T ON TR.TrainerID = T.TrainerID
    INNER JOIN
        Owners O ON M.GymID = O.OwnerID
    INNER JOIN
        Users U ON T.TrainerID = U.UserID
    WHERE
        T.TrainerID = @trainerID
    GROUP BY
        O.GymName,
        U.FullName";


                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@trainerID", userID); // Assuming userID contains the current trainer's ID
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

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
