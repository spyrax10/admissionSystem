using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing;

namespace QRScan
{
    public partial class Scanner : Form
    {
        int move = 0;
        int left = 5;
        string empcs = @"Data Source=D8672B6A3F8B574\LOCAL;Initial Catalog=facialDB;Integrated Security=True";

        FilterInfoCollection filterInfoCollection;
        VideoCaptureDevice FinalFrame;
        string imgLoc = "";
        public Scanner()
        {
            InitializeComponent();
        }
        
        #region Regular Emp Attendance, with complete Sched
        
        //For regular Emp Attendance Morning Time In
        public void mornInRegEmp()
        {
            SqlConnection mornIncon = new SqlConnection(empcs);
            SqlConnection chkcon = new SqlConnection(empcs);
            SqlConnection dispcon = new SqlConnection(empcs);

            string empId = tBId.Text;
            string date = DateTime.Now.ToShortDateString();
            string evtCode = lblEvtCode.Text;
            string time = DateTime.Now.ToShortTimeString();

            string mornOut = "00:00";
            string aftIn = "00:00";
            string aftOut = "00:00";
            string eveIn = "00:00";
            string eveOut = "00:00";
            string totHours = "0";
            string mornStat = "---";
            string aftStat = "---";
            string eveStat = "---";

            try
            {
                dispcon.Open();
                SqlCommand cmd = dispcon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from empTB where empId = '" + tBId.Text + "'";
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    string first = (dr["Firstname"].ToString());
                    string mid = (dr["Midname"].ToString());
                    string last = (dr["Lastname"].ToString());
                    string dept = (dr["Dept"].ToString());

                    tBFirst.Text = first;
                    tbMid.Text = mid;
                    tBLast.Text = last;

                    byte[] img = (byte[])(dr["empImg"]);
                    if (img == null)
                    {
                        pBImg.Image = null;
                    }
                    else
                    {
                        MemoryStream ms = new MemoryStream(img);
                        pBImg.Image = Image.FromStream(ms);
                    }

                    chkcon.Open();
                    SqlCommand cmd2 = chkcon.CreateCommand();
                    cmd2.CommandType = CommandType.Text;
                    cmd2.CommandText = "Select * from evtEmpTB where evtCode = '" + evtCode + "' " +
                        "and evtDate = '" + date + "' " +
                        "and mornTimeIn != '' " +
                        "and empId = '" + empId + "'";
                    SqlDataReader dr2 = cmd2.ExecuteReader();

                    if (dr2.Read())
                    {
                        lblStat.Text = "ID Already Logged IN!";
                        SoundPlayer play = new SoundPlayer(Properties.Resources.what_are_you_doing);
                        play.Play();

                        mornInTimer.Stop();
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.kali;
                        empRegMornIn();
                    }
                    else
                    {
                        mornIncon.Open();
                        SqlCommand cmd3 = mornIncon.CreateCommand();
                        cmd3.CommandType = CommandType.Text;
                        cmd3.CommandText = "Insert into evtEmpTB Values ('" + evtCode + "', " +
                            "'" + date + "', " +
                            "'" + empId + "', " +
                            "'" + first + "', " +
                            "'" + mid + "', " +
                            "'" + last + "', " +
                            "'" + dept + "', " +
                            "'" + time + "', " +
                            "'" + mornOut + "', " +
                            "'" + aftIn + "', " +
                            "'" + aftOut + "', " +
                            "'" + eveIn + "', " +
                            "'" + eveOut + "', " +
                            "'" + mornStat + "', " +
                            "'" + aftStat + "', " +
                            "'" + eveStat + "', " +
                            "'" + totHours + "')";
                        cmd3.ExecuteNonQuery();
                        mornIncon.Close();
                        lblStat.Text = "RECORDED!";
                        SoundPlayer play = new SoundPlayer(Properties.Resources.yeah);
                        play.Play();

                        tBId.Text = "";
                        
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.kali;
                        tBFirst.Text = "";
                        tbMid.Text = "";
                        tBLast.Text = "";
                        pBImg.Image = Properties.Resources.kali;
                        empRegMornIn();
                    }
                    chkcon.Close();
                }
                else
                {
                    lblStat.Text = "ID NOT FOUND!";
                    SoundPlayer play = new SoundPlayer(Properties.Resources.who_are_you);
                    play.Play();

                    FinalFrame.Stop();
                    pBQR.Image = Properties.Resources.kali;
                    empRegMornIn();
                }
                dispcon.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // For regular Emp Attendance Morning Time Out
        public void mornOutRegEmp()
        {
            SqlConnection upcon = new SqlConnection(empcs);
            SqlConnection chkcon = new SqlConnection(empcs);
            SqlConnection dispcon = new SqlConnection(empcs);

            string empId = tBId.Text;
            string date = DateTime.Now.ToShortDateString();
            string evtCode = lblEvtCode.Text;
            string time = DateTime.Now.ToShortTimeString();

            string mornOut = "00:00";

            try
            {
                dispcon.Open();
                SqlCommand cmd = dispcon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from empTB where empId = '" + tBId.Text + "'";
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    string first = (dr["Firstname"].ToString());
                    string mid = (dr["Midname"].ToString());
                    string last = (dr["Lastname"].ToString());
                    string dept = (dr["Dept"].ToString());

                    chkcon.Open();
                    SqlCommand cmd2 = chkcon.CreateCommand();
                    cmd2.CommandType = CommandType.Text;
                    cmd2.CommandText = "Select * from evtEmpTB where evtCode = '" + evtCode + "' " +
                        "and evtDate = '" + date + "' " +
                        "and mornTimeOut != '" + mornOut + "' " +
                        "and empId = '" + empId + "'";
                    SqlDataReader dr2 = cmd2.ExecuteReader();

                    if (dr2.Read())
                    {
                        lblStat.Text = "ID Already Logged IN!";
                       
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.kali;
                        capCam();
                    }
                    else
                    {
                        upcon.Open();
                        SqlCommand cmd3 = upcon.CreateCommand();
                        cmd3.CommandType = CommandType.Text;
                        cmd3.CommandText = "Update eveEmpTB set mornTimeOut = '" + time + "' " +
                            "where evtCode = '" + evtCode + "' " +
                            "and evtDate = '" + date + "' " +
                            "and empId = '" + empId + "'";
                        cmd3.ExecuteNonQuery();
                        upcon.Close();

                        tBId.Text = "";
                        lblStat.Text = "SUCCESS!";
                       
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.kali;
                       
                    }
                    chkcon.Close();
                }
                else
                {
                    lblStat.Text = "ID NOT FOUND!";
                    
                    FinalFrame.Stop();
                    pBQR.Image = Properties.Resources.kali;
                   
                }
                dispcon.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // For regular Emp Attendance Afternoon Time In
        public void aftInRegEmp()
        {
            SqlConnection upAftcon = new SqlConnection(empcs);
            SqlConnection chkcon = new SqlConnection(empcs);
            SqlConnection dispcon = new SqlConnection(empcs);

            string empId = tBId.Text;
            string date = DateTime.Now.ToShortDateString();
            string evtCode = lblEvtCode.Text;
            string time = DateTime.Now.ToShortTimeString();

            string aftIn = "00:00";

            try
            {
                dispcon.Open();
                SqlCommand cmd = dispcon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from empTB where empId = '" + tBId.Text + "'";
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    string first = (dr["Firstname"].ToString());
                    string mid = (dr["Midname"].ToString());
                    string last = (dr["Lastname"].ToString());
                    string dept = (dr["Dept"].ToString());

                    chkcon.Open();
                    SqlCommand cmd2 = chkcon.CreateCommand();
                    cmd2.CommandType = CommandType.Text;
                    cmd2.CommandText = "Select * from evtEmpTB where evtCode = '" + evtCode + "' " +
                        "and evtDate = '" + date + "' " +
                        "and aftTimeIn != '" + aftIn + "' " +
                        "and empId = '" + empId + "'";
                    SqlDataReader dr2 = cmd2.ExecuteReader();

                    if (dr2.Read())
                    {
                        lblStat.Text = "ID Already Logged IN!";
                        
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.kali;
                        
                    }
                    else
                    {
                        upAftcon.Open();
                        SqlCommand cmd3 = upAftcon.CreateCommand();
                        cmd3.CommandType = CommandType.Text;
                        cmd3.CommandText = "Update eveEmpTB set aftTimeIn = '" + time + "' " +
                            "where evtCode = '" + evtCode + "' " +
                            "and evtDate = '" + date + "' " +
                            "and empId = '" + empId + "'";
                        cmd3.ExecuteNonQuery();
                        upAftcon.Close();

                        tBId.Text = "";
                        lblStat.Text = "SUCCESS!";
                        
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.kali;
                       
                    }
                    chkcon.Close();
                }
                else
                {
                    lblStat.Text = "ID NOT FOUND!";
                   
                    FinalFrame.Stop();
                    pBQR.Image = Properties.Resources.kali;
                    
                }
                dispcon.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // For regular Emp Attendance Afternoon Time out
        public void aftOutRegEmp()
        {
            SqlConnection upAftcon = new SqlConnection(empcs);
            SqlConnection chkcon = new SqlConnection(empcs);
            SqlConnection dispcon = new SqlConnection(empcs);

            string empId = tBId.Text;
            string date = DateTime.Now.ToShortDateString();
            string evtCode = lblEvtCode.Text;
            string time = DateTime.Now.ToShortTimeString();

            string aftOut = "00:00";

            try
            {
                dispcon.Open();
                SqlCommand cmd = dispcon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from empTB where empId = '" + tBId.Text + "'";
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    string first = (dr["Firstname"].ToString());
                    string mid = (dr["Midname"].ToString());
                    string last = (dr["Lastname"].ToString());
                    string dept = (dr["Dept"].ToString());

                    chkcon.Open();
                    SqlCommand cmd2 = chkcon.CreateCommand();
                    cmd2.CommandType = CommandType.Text;
                    cmd2.CommandText = "Select * from evtEmpTB where evtCode = '" + evtCode + "' " +
                        "and evtDate = '" + date + "' " +
                        "and aftTimeOut != '" + aftOut + "' " +
                        "and empId = '" + empId + "'";
                    SqlDataReader dr2 = cmd2.ExecuteReader();

                    if (dr2.Read())
                    {
                        lblStat.Text = "ID Already Logged IN!";
                       
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.kali;
                        
                    }
                    else
                    {
                        upAftcon.Open();
                        SqlCommand cmd3 = upAftcon.CreateCommand();
                        cmd3.CommandType = CommandType.Text;
                        cmd3.CommandText = "Update eveEmpTB set aftTimeOut = '" + time + "' " +
                            "where evtCode = '" + evtCode + "' " +
                            "and evtDate = '" + date + "' " +
                            "and empId = '" + empId + "'";
                        cmd3.ExecuteNonQuery();
                        upAftcon.Close();

                        tBId.Text = "";
                        lblStat.Text = "SUCCESS!";
                        
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.kali;
                        
                    }
                    chkcon.Close();
                }
                else
                {
                    lblStat.Text = "ID NOT FOUND!";
                    
                    FinalFrame.Stop();
                    pBQR.Image = Properties.Resources.kali;
                    
                }
                dispcon.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void eveInRegEmp()
        {
            SqlConnection upAftcon = new SqlConnection(empcs);
            SqlConnection chkcon = new SqlConnection(empcs);
            SqlConnection dispcon = new SqlConnection(empcs);

            string empId = tBId.Text;
            string date = DateTime.Now.ToShortDateString();
            string evtCode = lblEvtCode.Text;
            string time = DateTime.Now.ToShortTimeString();

            string eveIn = "00:00";

            try
            {
                dispcon.Open();
                SqlCommand cmd = dispcon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from empTB where empId = '" + tBId.Text + "'";
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    string first = (dr["Firstname"].ToString());
                    string mid = (dr["Midname"].ToString());
                    string last = (dr["Lastname"].ToString());
                    string dept = (dr["Dept"].ToString());

                    chkcon.Open();
                    SqlCommand cmd2 = chkcon.CreateCommand();
                    cmd2.CommandType = CommandType.Text;
                    cmd2.CommandText = "Select * from evtEmpTB where evtCode = '" + evtCode + "' " +
                        "and evtDate = '" + date + "' " +
                        "and eveTimeIn != '" + eveIn + "' " +
                        "and empId = '" + empId + "'";
                    SqlDataReader dr2 = cmd2.ExecuteReader();

                    if (dr2.Read())
                    {
                        lblStat.Text = "ID Already Logged IN!";
                       
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.kali;
                        
                    }
                    else
                    {
                        upAftcon.Open();
                        SqlCommand cmd3 = upAftcon.CreateCommand();
                        cmd3.CommandType = CommandType.Text;
                        cmd3.CommandText = "Update eveEmpTB set eveTimeIn = '" + time + "' " +
                            "where evtCode = '" + evtCode + "' " +
                            "and evtDate = '" + date + "' " +
                            "and empId = '" + empId + "'";
                        cmd3.ExecuteNonQuery();
                        upAftcon.Close();

                        tBId.Text = "";
                        lblStat.Text = "SUCCESS!";
                       
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.kali;
                        
                    }
                    chkcon.Close();
                }
                else
                {
                    lblStat.Text = "ID NOT FOUND!";
                   
                    FinalFrame.Stop();
                    pBQR.Image = Properties.Resources.kali;
                    
                }
                dispcon.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void eveOutRegEmp()
        {
            SqlConnection upAftcon = new SqlConnection(empcs);
            SqlConnection chkcon = new SqlConnection(empcs);
            SqlConnection dispcon = new SqlConnection(empcs);

            string empId = tBId.Text;
            string date = DateTime.Now.ToShortDateString();
            string evtCode = lblEvtCode.Text;
            string time = DateTime.Now.ToShortTimeString();

            string eveOut = "00:00";

            try
            {
                dispcon.Open();
                SqlCommand cmd = dispcon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from empTB where empId = '" + tBId.Text + "'";
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    string first = (dr["Firstname"].ToString());
                    string mid = (dr["Midname"].ToString());
                    string last = (dr["Lastname"].ToString());
                    string dept = (dr["Dept"].ToString());

                    chkcon.Open();
                    SqlCommand cmd2 = chkcon.CreateCommand();
                    cmd2.CommandType = CommandType.Text;
                    cmd2.CommandText = "Select * from evtEmpTB where evtCode = '" + evtCode + "' " +
                        "and evtDate = '" + date + "' " +
                        "and eveTimeOut != '" + eveOut + "' " +
                        "and empId = '" + empId + "'";
                    SqlDataReader dr2 = cmd2.ExecuteReader();

                    if (dr2.Read())
                    {
                        lblStat.Text = "ID Already Logged IN!";
                       
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.kali;
                       
                    }
                    else
                    {
                        upAftcon.Open();
                        SqlCommand cmd3 = upAftcon.CreateCommand();
                        cmd3.CommandType = CommandType.Text;
                        cmd3.CommandText = "Update eveEmpTB set eveTimeOut = '" + time + "' " +
                            "where evtCode = '" + evtCode + "' " +
                            "and evtDate = '" + date + "' " +
                            "and empId = '" + empId + "'";
                        cmd3.ExecuteNonQuery();
                        upAftcon.Close();

                        tBId.Text = "";
                        lblStat.Text = "SUCCESS!";
                       
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.kali;
                        
                    }
                    chkcon.Close();
                }
                else
                {
                    lblStat.Text = "ID NOT FOUND!";
                    
                    FinalFrame.Stop();
                    pBQR.Image = Properties.Resources.kali;
                   
                }
                dispcon.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        public void showOut()
        {
            SqlConnection con = new SqlConnection(empcs);
            string time = DateTime.Now.ToShortTimeString();
            string date = DateTime.Now.ToShortDateString();
            string code = lblEvtCode.Text;
            try
            {
                con.Open();
                SqlCommand cmd = con.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from eventTB where evtCode = '" + code + "' " +
                    "and eventDate = '" + date + "'";
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    string mornOut = (dr["MorningOut"].ToString());
                    string aftOut = (dr["AftOut"].ToString());
                    string eveOut = (dr["EveOut"].ToString());

                    if (mornOut == time)
                    {
                        btnMornOut.Enabled = true;
                        btnMornIn.Enabled = false;
                    }
                    if (aftOut == time)
                    {
                        btnAftOut.Enabled = true;
                        btnMornIn.Enabled = false;
                    }
                    if (eveOut == time)
                    {
                        btnEveOut.Enabled = true;
                        btnMornIn.Enabled = false;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void verPass()
        {
            SqlConnection vercon = new SqlConnection(empcs);
            string pass = tBPassLock.Text;
            string user = tBUser.Text;

            try
            {
                if (pass == "")
                {
                    tBPassLock.Visible = false;
                }
                else
                {
                    vercon.Open();
                    SqlCommand cmd = vercon.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "Select * from setTB where username = '" + user + "' " +
                        "and password = '" + pass + "'";
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        tBPassLock.Visible = true;
                        if (paneSet.Enabled == true)
                        {
                            paneSet.Enabled = false;
                            btnLock.Image = Properties.Resources.Lock_Unlock_icon;
                            tBPassLock.Text = "";
                            tBPassLock.Visible = false;
                        }
                        else
                        {
                            paneSet.Enabled = true;
                            btnLock.Image = Properties.Resources.Lock_Lock_icon;
                            tBPassLock.Text = "";
                            tBPassLock.Visible = false;
                        }
                    }
                    else
                    {
                        tBPassLock.Visible = false;
                    }
                }
              
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void empRegMornIn()
        {
            capCam();
            mornInTimer.Enabled = true;
            mornInTimer.Start();
           // MessageBox.Show("Started!", " Started", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public void capCam()
        {
            try
            {
                tBId.Text = "";
                FinalFrame = new VideoCaptureDevice(filterInfoCollection[cBCamera.SelectedIndex].MonikerString);
                FinalFrame.NewFrame += new NewFrameEventHandler(FinalFrame_NewFrame);
                FinalFrame.Start();
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FinalFrame_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
           
            pBQR.Image = (Bitmap)eventArgs.Frame.Clone();
        }

        public void cbEvt()
        {
            SqlConnection evtcon = new SqlConnection(empcs);
            string name = cBEvt.Text;
            disbutt();
            try
            {
                evtcon.Open();
                SqlCommand cmd = evtcon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from eventTB where eventName = '" + name + "'";
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    lblEvtCode.Text = (dr["evtCode"].ToString());
                    tBEvtAtt.Text = (dr["Attendee"].ToString());
                    tBMornIn.Text = (dr["MorningIn"].ToString());
                    tBMornOut.Text = (dr["MorningOut"].ToString());
                    tBAftIn.Text = (dr["AftIn"].ToString());
                    tBAftOut.Text = (dr["AftOut"].ToString());
                    tBEveIn.Text = (dr["EveIn"].ToString());
                    tBEveOut.Text = (dr["EveOut"].ToString());

                    if (tBMornIn.Text != "NONE")
                    {
                        btnMornIn.Enabled = true;       
                    }
                    if (tBAftIn.Text != "NONE" && tBMornIn.Text == "NONE")
                    {
                        btnAftIn.Enabled = true;
                    }
                    if(tBEveIn.Text != "NONE" && tBMornIn.Text == "NONE" && tBAftIn.Text == "NONE")
                    {
                        btnEveIn.Enabled = true;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void loadEvt()
        {
            SqlConnection evtcon = new SqlConnection(empcs);
            string date = DateTime.Now.ToShortDateString();

            try
            {
                evtcon.Open();
                SqlCommand cmd = evtcon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from eventTB order by evtCode DESC";
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    string name = (dr["eventName"].ToString());
                    
                    if (!cBEvt.Items.Contains(name))
                    {
                        cBEvt.Items.Add(name);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void loadCam()
        {
            try
            {
                filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                foreach (FilterInfo Device in filterInfoCollection)
                {
                    cBCamera.Items.Add(Device.Name);
                }
                cBCamera.SelectedIndex = 0;
                FinalFrame = new VideoCaptureDevice();
                
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
                    cmd.CommandText = "Select * from setTB where " +
                        "username = '" + username + "' " +
                        "and password = '" + pass + "'";
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        string user = (dr["username"].ToString());

                        MessageBox.Show("Welcome, " + user + "!", " Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        paneHome.Visible = true;
                        gBLog.Visible = false;
                    }
                    else
                    {
                        MessageBox.Show("Incorrect Combination!", " Invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    logcon.Close();
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void disbutt()
        {
            btnMornIn.Enabled = false;
            btnMornOut.Enabled = false;
            btnAftIn.Enabled = false;
            btnAftOut.Enabled = false;
            btnEveIn.Enabled = false;
            btnEveOut.Enabled = false;
            
        }
        private void Scanner_Load(object sender, EventArgs e)
        {
            paneHome.Visible = false;
            gBLog.Visible = false;
            paneLogo.Visible = true;
            lblDate.Text = DateTime.Now.ToLongDateString();
            loadCam();
            loadEvt();
            disbutt();
            tBPassLock.Visible = false;
            timer1.Start();
            timer2.Start();
            timer3.Start();
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
                    gBLog.Visible = true;
                    tBUser.Focus();
                    paneLogo.Visible = false;
                }
            }
            catch (Exception f)
            {
                MessageBox.Show(f.Message);
            }
        }

        private void btnLog_Click(object sender, EventArgs e)
        {
            logID();
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            lblTime.Text = DateTime.Now.ToLongTimeString();

            if (lblEvtCode.Text != "0000000")
            {
                showOut();
            }
        }

        private void cBEvt_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cBEvt.Text != null)
            {
                cbEvt();
            }
        }

        private void cBEvt_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }


        private void Scanner_FormClosing(object sender, FormClosingEventArgs e)
        {
            FinalFrame.Stop();
            Application.Exit();
        }

        private void btnMornIn_Click(object sender, EventArgs e)
        {
            //if (lblEvtCode.Text != "0000000")
            //{
            //    if (tBEvtAtt.Text == "ALL EMPLOYEES")
            //    {
                   
            //    }
            //    if (tBEvtAtt.Text == "ALL STUDENTS")
            //    {

            //    }
            //    if (tBEvtAtt.Text == "GENERAL")
            //    {

            //    }
                
            //}
            //else
            //{

            //}
            empRegMornIn();

        }

        private void mornInTimer_Tick(object sender, EventArgs e)
        {
            try
            {

                if (pBQR.Image != null)
                {
                    BarcodeReader Reader = new BarcodeReader();
                    Result result = Reader.Decode((Bitmap)pBQR.Image);

                    if (result != null)
                    {
                        string decoded = result.ToString().Trim();
                        tBId.Text = decoded;
                        lblStat.Text = "QR Decoded!";       
                        mornInTimer.Stop();
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.kali;
                        mornInRegEmp();             
                    }
                    else
                    {
                        lblStat.Text = "Verifying...";
                        pBImg.Image = Properties.Resources.download;
                    }
                }

            }
            catch (Exception f)
            {
                MessageBox.Show(f.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLock_Click(object sender, EventArgs e)
        {
            tBPassLock.Visible = true;
            tBPassLock.Focus();
        }

        private void tBPassLock_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                verPass();
            }
        }

        private void btnWrap_Click(object sender, EventArgs e)
        {
            SoundPlayer play = new SoundPlayer(Properties.Resources.ding);
            play.Play();
        }
    }
}
