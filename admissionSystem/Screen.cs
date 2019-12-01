using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace admissionSystem
{
    public partial class Screen : Form
    {
        int move = 0;
        int left = 5;
        public Screen()
        {
            InitializeComponent();
        }

        private void Screen_Load(object sender, EventArgs e)
        {
            gBLog.Visible = false;
            gBId.Visible = false;
            gBForgot.Visible = false;
            gBCreate.Visible = false;
            btnShut.Visible = false;
            lblWel.Visible = false;
            lblName.Visible = false;
            timer1.Start();
            timer2.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            paneSlide.Left += 2;

            if (paneSlide.Left > 340)
            {
                paneSlide.Left = 0;
            }
            if (paneSlide.Left < 0)
            {
                move = 2;
            }

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            try
            {
                left--;

                if (left == 0)
                {
                    timer1.Stop();
                    timer2.Stop();
                    paneLogo.Visible = false;
                    gBId.Visible = true;
                    btnShut.Visible = true;
                    tBId.Focus();
                }
            }
            catch (Exception f)
            {
                MessageBox.Show(f.Message);
            }
        }

        private void btnShut_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnIDVer_Click(object sender, EventArgs e)
        {
            if (tBId.Text == "emanCute")
            {
                this.Visible = false;
                Main ma = new Main();
                ma.ShowDialog();    
            }
        }
    }
}
