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
    public partial class TrainerInterface : Form
    {
        private int userID;
        public TrainerInterface(int uid)
        {
            InitializeComponent();
            userID = uid;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            TrainerDietPlan Form = new TrainerDietPlan(userID);
            {
                Form.Show();
                this.Hide();
            }
        }

        private void TrainerInterface_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            WorkoutPlanTrainer Form=new WorkoutPlanTrainer(userID);
            {
                Form.Show();
                this.Hide();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Form1 form = new Form1();
            form.Show();
            this.Hide();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            TrainerFeedback form = new TrainerFeedback(userID);
            form.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TrainerAppointment form = new TrainerAppointment(userID);
            form.Show();
            this.Hide();
        }
    }
}
