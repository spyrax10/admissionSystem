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
        string empcs = @"Data Source=LOCALHOST192\SQL2019;Initial Catalog=facialDB;Integrated Security=True";

        public static string id;
        public static string userId;
        public Screen()
        {
            InitializeComponent();
        }
        public void logID()
        {
            SqlConnection logcon = new SqlConnection(empcs);
            
            string username = tBUser.Text;
            string pass = tBPass.Text;
            string empId = tBId.Text;

            try
            {
                if (username == "" || pass == "")
                {
                    MessageBox.Show("Missing Fields!", " Incomplete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    logcon.Open();
                    SqlCommand cmd = logcon.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "Select * from setTB where empId = '" + empId + "' " +
                        "and username = '" + username + "' " +
                        "and password = '" + pass + "'";
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        string user = (dr["username"].ToString());

                        MessageBox.Show("Welcome, " + user + "!", " Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Main ma = new Main();
                        id = empId;
                        userId = user;
                        ma.Show();
                        this.Visible = false;
                    }
                    else
                    {
                        MessageBox.Show("Incorrect Combination!", " Invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    logcon.Close();
                }
               
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void creID()
        {
            SqlConnection crecon = new SqlConnection(empcs);
            SqlConnection chkcon = new SqlConnection(empcs);

            string username = tBCreID.Text;
            string pass = tBCrePass.Text;
            string empId = tBId.Text;

            try
            {
                chkcon.Open();
                SqlCommand cmd2 = chkcon.CreateCommand();
                cmd2.CommandType = CommandType.Text;
                cmd2.CommandText = "Select * from setTB where username = '" + username + "'";
                SqlDataReader dr = cmd2.ExecuteReader();

                if (dr.Read())
                {
                    MessageBox.Show("Username is already taken!", " Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else
                {
                    if (username == "" || pass == "")
                    {
                        MessageBox.Show("Missing Fields!", " Incomplete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        crecon.Open();
                        SqlCommand cmd = crecon.CreateCommand();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "Insert into setTB Values ('" + empId + "', " +
                            "'" + username + "', " +
                            "'" + pass + "')";
                        cmd.ExecuteNonQuery();
                        crecon.Close();
                        if (MessageBox.Show("Credentials successfully created!, Login Now?", " Success", 
                            MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                        {
                            gBLog.Visible = true;
                            gBLog.BringToFront();
                            tBUser.Focus();
                        }
                        else
                        {
                            gBId.Visible = true;
                            gBId.BringToFront();
                            tBCreID.Focus();
                        }
                    }
                   
                }
                chkcon.Close();
               
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
                cmd.CommandText = "Select * from empTB where empId = '" + empId + "' and Pos = 'Admin'";
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
                        tBUser.Focus();
                    }
                    else
                    {
                        MessageBox.Show("Create your LogIn Credentials", " Create", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        gBCreate.Visible = true;
                        gBCreate.BringToFront();
                        tBCreID.Focus();
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

            if (paneSlide.Left > 355)
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

        private void btnCre_Click(object sender, EventArgs e)
        {
            creID();
        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            logID();
        }
    }
}
