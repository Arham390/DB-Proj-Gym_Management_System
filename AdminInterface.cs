using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Deliverable_2
{
    public partial class AdminInterface : Form
    {
        private int userID;
        public AdminInterface(int userID)
        {
            InitializeComponent();
            this.userID = userID;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AdminReports form = new AdminReports(userID);
            form.Show();
            this.Hide();
        }

        private void AdminInterface_Load(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            Form1 form = new Form1();
            form.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AdminRequests form = new AdminRequests(userID);
            form.Show();
            this.Hide();
        }
    }
}
