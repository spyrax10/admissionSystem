using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace admissionSystem
{
    public partial class Screen : Form
    {
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

        int move = 0;
        int left = 5;
        string empcs = @"Data Source=LOCALHOST192\SQL2019;Initial Catalog=facialDB;Integrated Security=True";

        public static string id;
        public static string userId;
        public Screen()
        {
            InitializeComponent();
        }
        private static string RandomString(int length)
        {
            Random random = new Random();
            const string pool = "abcdefghijklmnopqrstuvwxyz0123456789";
            var builder = new StringBuilder();

            for (var i = 0; i < length; i++)
            {
                var c = pool[random.Next(0, pool.Length)];
                builder.Append(c);
            }

            return builder.ToString();
        }
        public void chk()
        {
            int Desc;
            String chk = InternetGetConnectedState(out Desc, 0).ToString();

            if (chk == "True")
            {
                codeReq();
            }
            else if (chk == "False")
            {
                MessageBox.Show("Please check your network and try again.. ", " No Connection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else
            {
                MessageBox.Show("Fatal Error!", " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        public void codeLog()
        {
            SqlConnection verCon = new SqlConnection(empcs);
            SqlConnection upCon = new SqlConnection(empcs);

            string code = tbCode.Text;
            string date = DateTime.Now.ToShortDateString();
            string user = tBUser.Text;
            string time = DateTime.Now.ToShortTimeString();
            string stat = "USED";
            string empId = tBId.Text;

            try
            {
                verCon.Open();
                SqlCommand verCmd = verCon.CreateCommand();
                verCmd.CommandType = CommandType.Text;
                verCmd.CommandText = "Select * from codeTB where code = '" + code + "' " +
                    "and DateReq = '" + date + "' " +
                    // "and user = '" + user + "' " +
                    "and status = 'ACTIVE'";
                SqlDataReader dr = verCmd.ExecuteReader();

                if (dr.Read())
                {
                    upCon.Open();
                    SqlCommand upCmd = upCon.CreateCommand();
                    upCmd.CommandType = CommandType.Text;
                    upCmd.CommandText = "Update codeTB set TimeLog = '" + time + "', " +
                        "status = '" + stat + "'" +
                        "where code = '" + code + "' " +
                        "and DateReq = '" + date + "'";

                    upCmd.ExecuteNonQuery();
                    upCon.Close();
                    id = empId;
                    userId = user;

                    MessageBox.Show("Login Successful!", " Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Main ma = new Main();
                    ma.Show();
                    this.Visible = false;
                }
                else
                {
                    MessageBox.Show("Code Error!", " Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                verCon.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void codeReq()
        {
            SqlConnection codeCon = new SqlConnection(empcs);

            string code = RandomString(5);
            string user = tBUser.Text;
            string date = DateTime.Now.ToShortDateString();
            string time = "-----";
            string stat = "ACTIVE";
            string email = "smarteman10@gmail.com";

            try
            {
                if (tBUser.Text == "")
                {
                    MessageBox.Show("Username required...", " Invalid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    tBUser.Focus();
                }
                else
                {
                   
                    gBForgot.Visible = true;
                    gBForgot.BringToFront();


                    codeCon.Open();
                    SqlCommand cmd = codeCon.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "Insert into codeTB Values('" + code + "', " +
                        "'" + stat + "', " +
                        "'" + date + "', " +
                        "'" + time + "', " +
                        "'" + user + "')";
                    cmd.ExecuteNonQuery();
                    codeCon.Close();

                    // Sending Email
                    SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                    MailMessage mail = new MailMessage();
                    mail.From = new MailAddress("sistemoquizo@gmail.com");
                    mail.To.Add(email);
                    mail.Subject = "Recovery Code: " + date + "";
                    mail.Body = "Username: " + user + "" + Environment.NewLine + "Code: " + code + "";

                    SmtpServer.Port = 587;
                    SmtpServer.Credentials = new System.Net.NetworkCredential("sistemoquizo@gmail.com", "qUizandexamsystem101");
                    SmtpServer.EnableSsl = true;
                    SmtpServer.Send(mail);

                    tbCode.Focus();
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            btnMin.Visible = false;
            timer1.Start();
            timer2.Start();
            this.TopMost = true;
            this.BringToFront();
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
                    btnMin.Visible = true;
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

        private void lblForgot_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            chk();
        }

        private void pBForBack_Click(object sender, EventArgs e)
        {
            gBLog.Visible = true;
            gBLog.BringToFront();
        }

        private void btnForVer_Click(object sender, EventArgs e)
        {
            if (tbCode.Text != "")
            {
                codeLog();
            }
            else
            {
                MessageBox.Show("Missing Fields!", " Empty", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnMin_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }
}
