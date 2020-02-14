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

namespace admissionSystem
{
    public partial class Screen : Form
    {
        int move = 0;
        int left = 5;
        string empcs = @"Data Source=D8672B6A3F8B574\LOCAL;Initial Catalog=facialDB;Integrated Security=True";
        public Screen()
        {
            InitializeComponent();
        }
        public void verID()
        {
            SqlConnection vercon = new SqlConnection(empcs);
            SqlConnection chkcon = new SqlConnection(empcs);
            string empId = tBId.Text;

            try
            {
                vercon.Open();
                SqlCommand cmd = vercon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from empTB where empId = '" + empId + "' and empStat = 'Admin'";
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    chkcon.Open();
                    SqlCommand cmd2 = chkcon.CreateCommand();
                    cmd2.CommandType = CommandType.Text;
                    cmd2.CommandText = "Select * from setTB where empId = '" + empId + "'";
                    SqlDataReader dr2 = cmd2.ExecuteReader();
    
                    if (dr2.Read())
                    {
                        MessageBox.Show("Please LogIn to your Credentials", " Login", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        gBLog.Visible = true;
                        gBLog.BringToFront();
                    }
                    else
                    {
                        MessageBox.Show("Create your LogIn Credentials", " Create", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        gBCreate.Visible = true;
                        gBCreate.BringToFront();

                    }
                    chkcon.Close();

                }
                else
                {
                    MessageBox.Show("ID not found!", " Invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                vercon.Close();

            }

            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Screen_Load(object sender, EventArgs e)
        {
            gBLog.Visible = false;
            gBId.Visible = false;
            gBForgot.Visible = false;
            gBCreate.Visible = false;
            btnShut.Visible = false;
            
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
            
        }

        private void tBId_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (tBId.Text == "emanCute")
                {
                    Main ma = new Main();
                    ma.Show();
                    this.Visible = false;
                }
                else
                {
                    verID();
                }
               
            }
        }
    }
}
