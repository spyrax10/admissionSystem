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
        string imgLoc = "";
        string empcs = @"Data Source=LOCALHOST192\SQL2019;Initial Catalog=facialDB;Integrated Security=True";

        FilterInfoCollection filterInfoCollection;
        VideoCaptureDevice FinalFrame;

        public Scanner()
        {
            InitializeComponent();
        }
        public void wrap()
        {
            SqlConnection wcon = new SqlConnection(empcs);

            string code = lblEvtCode.Text;

            try
            {
                wcon.Open();
                SqlCommand cmd = wcon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Update attendTB set mornStat = CASE WHEN mornTimeIn != '00:00' and mornTimeOut != '00:00' THEN 'PRESENT' ELSE 'INC' END, " +
                    "aftStat = CASE WHEN aftTimeIn != '00:00' and aftTimeOut != '00:00' THEN 'PRESENT' ELSE 'INC' END, " +
                    "eveStat = CASE WHEN eveTimeIn != '00:00' and eveTimeOut != '00:00' THEN 'PRESENT' ELSE 'INC' END " +
                    "where evtCode = '" + code + "'";
                cmd.ExecuteNonQuery();
                wcon.Close();

                tBWPass.Visible = false;
                tBWPass.Text = "";

                eveOutTimer.Stop();
                mornOutTimer.Stop();
                aftOutTimer.Stop();
                FinalFrame.Stop();
                pBQR.Image = Properties.Resources.abc;
                Application.Exit();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Complete Attendance Schedule
        
        public void logEveOut()
        {
            SqlConnection empmOut = new SqlConnection(empcs);
            SqlConnection studmOut = new SqlConnection(empcs);
            SqlConnection empcon = new SqlConnection(empcs);
            SqlConnection studcon = new SqlConnection(empcs);
            SqlConnection empchk = new SqlConnection(empcs);
            SqlConnection studchk = new SqlConnection(empcs);
            SqlConnection econ = new SqlConnection(empcs);
            SqlConnection scon = new SqlConnection(empcs);
            SqlConnection vercon = new SqlConnection(empcs);

            string id = tBId.Text;
            string time = DateTime.Now.ToShortTimeString();
            string date = DateTime.Now.ToShortDateString();
            string code = lblEvtCode.Text;
            string att = tBEvtAtt.Text;

            string mStat = "---";
            string aStat = "---";
            string eStat = "---";

            try
            {
                empcon.Open();
                SqlCommand cmd = empcon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from empTB where empId = '" + id + "'";
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    string efirst = (dr["Firstname"].ToString());
                    string emid = (dr["Midname"].ToString());
                    string elast = (dr["Lastname"].ToString());

                    tBFirst.Text = efirst;
                    tbMid.Text = emid;
                    tBLast.Text = elast;

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


                    empchk.Open();
                    SqlCommand echk = empchk.CreateCommand();
                    echk.CommandType = CommandType.Text;
                    echk.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                        "and evtDate = '" + date + "' and Id = '" + id + "' " +
                        "and eveTimeIn != ''";
                    SqlDataReader edr = echk.ExecuteReader();

                    if (edr.Read())
                    {
                        econ.Open();
                        SqlCommand ecmd = econ.CreateCommand();
                        ecmd.CommandType = CommandType.Text;
                        ecmd.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                            "and evtDate = '" + date + "' and Id = '" + id + "' " +
                            "and eveTimeOut = '00:00'";
                        SqlDataReader dr2 = ecmd.ExecuteReader();

                        if (dr2.Read())
                        {
                            if (att == "ALL EMPLOYEES" || att == "GENERAL")
                            {
                                empmOut.Open();
                                SqlCommand eout = empmOut.CreateCommand();
                                eout.CommandText = "Update attendTB set eveTimeOut = '" + time + "' " +
                                    "where evtCode = '" + code + "' and evtDate = '" + date + "' " +
                                    "and Id = '" + id + "'";
                                eout.ExecuteNonQuery();
                                empmOut.Close();

                                lblStat.Text = "ID RECORDED!";
                                tBId.Text = "";
                                eveOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                eveOut();
                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to logout!";
                                tBId.Text = "";
                                eveOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                eveOut();
                            }

                        }
                        else
                        {
                            if (att == "ALL EMPLOYEES" || att == "GENERAL")
                            {
                                lblStat.Text = "ID HAS ALREADY LOGGED OUT!";
                                tBId.Text = "";
                                eveOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                eveOut();
                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to logout!";
                                tBId.Text = "";
                                eveOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                eveOut();
                            }
                          
                        }
                        econ.Close();
                    }
                    else
                    {
                        if (att == "ALL EMPLOYEES" || att == "GENERAL")
                        {
                            lblStat.Text = "ID HAS NOT LOGGED IN!";
                            tBId.Text = "";
                            eveOutTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            eveOut();
                        }
                        else
                        {
                            lblStat.Text = "You are not allowed to login!";
                            tBId.Text = "";
                            eveOutTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            eveOut();
                        }
                       
                    }
                    empchk.Close();
                }
                else
                {
                    studcon.Open();
                    SqlCommand cmd2 = studcon.CreateCommand();
                    cmd2.CommandType = CommandType.Text;
                    cmd2.CommandText = "Select * from studTB where studId = '" + id + "'";
                    SqlDataReader dr2 = cmd2.ExecuteReader();

                    if (dr2.Read())
                    {
                        string sfirst = (dr2["Firstname"].ToString());
                        string smid = (dr2["Midname"].ToString());
                        string slast = (dr2["Lastname"].ToString());
                        string syear = (dr2["Year"].ToString());
                        string sdept = (dr2["Dept"].ToString());
                        string scourse = (dr2["Course"].ToString());
                        string stype = "STUDENT";
                        string smornIn = "00:00";
                        string saftIn = "00:00";
                        string smornOut = "00:00";
                        string seveIn = "00:00";
                        string saftOut = "00:00";
                        string stotHrs = "0";

                        tBFirst.Text = sfirst;
                        tbMid.Text = smid;
                        tBLast.Text = slast;

                        byte[] img = (byte[])(dr2["studImg"]);
                        if (img == null)
                        {
                            pBImg.Image = null;
                        }

                        else
                        {
                            MemoryStream ms = new MemoryStream(img);
                            pBImg.Image = Image.FromStream(ms);
                        }

                        studchk.Open();
                        SqlCommand schk = studchk.CreateCommand();
                        schk.CommandType = CommandType.Text;
                        schk.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                            "and evtDate = '" + date + "' and Id = '" + id + "' " +
                            "and mornTimeIn = '' and mornTimeOut = '' and aftTimeIn = '' and eveTimeIn = ''";
                        SqlDataReader sdr = schk.ExecuteReader();

                        if (sdr.Read())
                        {
                            if (att != "ALL EMPLOYESS" || att == "GENERAL")
                            {
                                scon.Open();
                                SqlCommand scmd = scon.CreateCommand();
                                scmd.CommandType = CommandType.Text;
                                scmd.CommandText = "Insert into attendTB values ('" + code + "', " +
                                      "'" + date + "', " +
                                      "'" + stype + "', " +
                                      "'" + id + "', " +
                                      "'" + sfirst + "', " +
                                      "'" + smid + "', " +
                                      "'" + slast + "', " +
                                      "'" + sdept + "', " +
                                      "'" + scourse + "', " +
                                      "'" + syear + "', " +
                                      "'" + smornIn + "', " +
                                      "'" + smornOut + "', " +
                                      "'" + saftIn + "', " +
                                      "'" + saftOut + "', " +
                                      "'" + seveIn + "', " +
                                      "'" + time + "', " +
                                      "'" + mStat + "', " +
                                      "'" + aStat + "', " +
                                      "'" + eStat + "', " +
                                      "'" + stotHrs + "')";
                                scmd.ExecuteNonQuery();
                                scon.Close();

                                lblStat.Text = "ID RECORDED!";
                                tBId.Text = "";
                                eveOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                eveOut();

                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to logout!";
                                tBId.Text = "";
                                eveOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                eveOut();
                            }
                           
                        }
                        else
                        {

                            vercon.Open();
                            SqlCommand vercmd = vercon.CreateCommand();
                            vercmd.CommandType = CommandType.Text;
                            vercmd.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                            "and evtDate = '" + date + "' and Id = '" + id + "' " +
                            "and eveTimeOut != '00:00' and eveTimeIn != '00:00'";
                            SqlDataReader vdr = vercmd.ExecuteReader();

                            if (vdr.Read())
                            {
                                if (att != "ALL EMPLOYEES" || att == "GENERAL")
                                {
                                    lblStat.Text = "ID HAS ALREADY LOGGED OUT!";
                                    tBId.Text = "";
                                    eveOutTimer.Stop();
                                    FinalFrame.Stop();
                                    pBQR.Image = Properties.Resources.abc;
                                    eveOut();
                                }
                                else
                                {
                                    lblStat.Text = "You are not allowed to logout!";
                                    tBId.Text = "";
                                    eveOutTimer.Stop();
                                    FinalFrame.Stop();
                                    pBQR.Image = Properties.Resources.abc;
                                    eveOut();
                                }
                             
                            }
                            else
                            {

                                if (att != "ALL EMPLOYEES" || att == "GENERAL")
                                {
                                    studmOut.Open();
                                    SqlCommand sout = studmOut.CreateCommand();
                                    sout.CommandType = CommandType.Text;
                                    sout.CommandText = "Update attendTB set eveTimeOut = '" + time + "' " +
                                        "where evtCode = '" + code + "' and evtDate = '" + date + "' " +
                                        "and Id = '" + id + "'";
                                    sout.ExecuteNonQuery();
                                    studmOut.Close();

                                    lblStat.Text = "ID RECORDED!";
                                    tBId.Text = "";
                                    eveOutTimer.Stop();
                                    FinalFrame.Stop();
                                    pBQR.Image = Properties.Resources.abc;
                                    eveOut();
                                }
                                else
                                {
                                    lblStat.Text = "You are not allowed to logout!";
                                    tBId.Text = "";
                                    eveOutTimer.Stop();
                                    FinalFrame.Stop();
                                    pBQR.Image = Properties.Resources.abc;
                                    eveOut();
                                }
                            }

                        }
                        studchk.Close();

                    }
                    else
                    {
                        lblStat.Text = "ID NOT FOUND!";
                        tBId.Text = "";
                        eveOutTimer.Stop();
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.abc;
                        eveOut();
                    }
                    studcon.Close();
                }
                empcon.Close();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }        
        public void logEveIn()
        {
            SqlConnection empmOut = new SqlConnection(empcs);
            SqlConnection studmOut = new SqlConnection(empcs);
            SqlConnection empcon = new SqlConnection(empcs);
            SqlConnection studcon = new SqlConnection(empcs);
            SqlConnection empchk = new SqlConnection(empcs);
            SqlConnection studchk = new SqlConnection(empcs);
            SqlConnection econ = new SqlConnection(empcs);
            SqlConnection scon = new SqlConnection(empcs);
            SqlConnection vercon = new SqlConnection(empcs);
            SqlConnection newemp = new SqlConnection(empcs);

            string id = tBId.Text;
            string time = DateTime.Now.ToShortTimeString();
            string date = DateTime.Now.ToShortDateString();
            string code = lblEvtCode.Text;
            string att = tBEvtAtt.Text;

            string mStat = "---";
            string aStat = "---";
            string eStat = "---";

            try
            {
                empcon.Open();
                SqlCommand cmd = empcon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from empTB where empId = '" + id + "'";
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    string efirst = (dr["Firstname"].ToString());
                    string emid = (dr["Midname"].ToString());
                    string elast = (dr["Lastname"].ToString());
                    string edept = (dr["Dept"].ToString());
                    string ecourse = "NA";
                    string eyear = "NA";
                    string etype = "EMPLOYEE";
                    string emornOut = "00:00";
                    string emornIn = "00:00";
                    string eaftIn = "00:00";
                    string eeveIn = "00:00";
                    string eeveOut = "00:00";
                    string etotHrs = "0";

                    tBFirst.Text = efirst;
                    tbMid.Text = emid;
                    tBLast.Text = elast;

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
                    empchk.Open();
                    SqlCommand echk = empchk.CreateCommand();
                    echk.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                        "and evtDate = '" + date + "' and Id = '" + id + "' " +
                        "and mornTimeIn = '' and mornTimeOut = '' and aftTimeIn = '' and aftTimeOut = ''";
                    SqlDataReader edr = echk.ExecuteReader();

                    if (edr.Read())
                    {
                        if (att == "ALL EMPLOYEES" || att == "GENERAL")
                        {
                            //Insert
                            newemp.Open();
                            SqlCommand ncmd = newemp.CreateCommand();
                            ncmd.CommandType = CommandType.Text;
                            ncmd.CommandText = "Insert into attendTB values ('" + code + "', " +
                                   "'" + date + "', " +
                                   "'" + etype + "', " +
                                   "'" + id + "', " +
                                   "'" + efirst + "', " +
                                   "'" + emid + "', " +
                                   "'" + elast + "', " +
                                   "'" + edept + "', " +
                                   "'" + ecourse + "', " +
                                   "'" + eyear + "', " +
                                   "'" + emornIn + "', " +
                                   "'" + emornOut + "', " +
                                   "'" + eaftIn + "', " +
                                   "'" + time + "', " +
                                   "'" + eeveIn + "', " +
                                   "'" + eeveOut + "', " +
                                   "'" + mStat + "', " +
                                   "'" + aStat + "', " +
                                   "'" + eStat + "', " +
                                   "'" + etotHrs + "')";
                            ncmd.ExecuteNonQuery();

                            lblStat.Text = "ID RECORDED!";
                            tBId.Text = "";
                            eveInTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            eveIn();
                        }
                        else
                        {
                            lblStat.Text = "You are not allowed to login!";
                            tBId.Text = "";
                            eveInTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            eveIn();
                        }
                        
                    }
                    else
                    {

                        econ.Open();
                        SqlCommand ecmd = econ.CreateCommand();
                        ecmd.CommandType = CommandType.Text;
                        ecmd.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                            "and evtDate = '" + date + "' and Id = '" + id + "' " +
                            "and eveTimeIn = '00:00'";
                        SqlDataReader dr2 = ecmd.ExecuteReader();

                        if (dr2.Read())
                        {
                            if (att == "ALL EMPLOYEES" || att == "GENERAL")
                            {
                                empmOut.Open();
                                SqlCommand eout = empmOut.CreateCommand();
                                eout.CommandText = "Update attendTB set eveTimeIn = '" + time + "' " +
                                    "where evtCode = '" + code + "' and evtDate = '" + date + "' " +
                                    "and Id = '" + id + "'";
                                eout.ExecuteNonQuery();
                                empmOut.Close();

                                lblStat.Text = "ID RECORDED!";
                                tBId.Text = "";
                                eveInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                eveIn();
                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to login!";
                                tBId.Text = "";
                                eveInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                eveIn();
                            }

                        }
                        else
                        {
                            if (att == "ALL EMPLOYEES" || att == "GENERAL")
                            {
                                lblStat.Text = "ID HAS ALREADY LOGGED IN!";
                                tBId.Text = "";
                                eveInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                eveIn();
                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to login!";
                                tBId.Text = "";
                                eveInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                eveIn();
                            }
                            
                        }
                        econ.Close();
                    }
                    empchk.Close();
                }
                else
                {
                    studcon.Open();
                    SqlCommand cmd2 = studcon.CreateCommand();
                    cmd2.CommandType = CommandType.Text;
                    cmd2.CommandText = "Select * from studTB where studId = '" + id + "'";
                    SqlDataReader dr2 = cmd2.ExecuteReader();

                    if (dr2.Read())
                    {
                        string sfirst = (dr2["Firstname"].ToString());
                        string smid = (dr2["Midname"].ToString());
                        string slast = (dr2["Lastname"].ToString());
                        string syear = (dr2["Year"].ToString());
                        string sdept = (dr2["Dept"].ToString());
                        string scourse = (dr2["Course"].ToString());
                        string stype = "STUDENT";
                        string smornIn = "00:00";
                        string smornOut = "00:00";
                        string saftOut = "00:00";
                        string saftIn = "00:00";
                        string seveOut = "00:00";
                        string stotHrs = "0";

                        tBFirst.Text = sfirst;
                        tbMid.Text = smid;
                        tBLast.Text = slast;

                        byte[] img = (byte[])(dr2["studImg"]);
                        if (img == null)
                        {
                            pBImg.Image = null;
                        }

                        else
                        {
                            MemoryStream ms = new MemoryStream(img);
                            pBImg.Image = Image.FromStream(ms);
                        }

                        studchk.Open();
                        SqlCommand schk = studchk.CreateCommand();
                        schk.CommandType = CommandType.Text;
                        schk.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                            "and evtDate = '" + date + "' and Id = '" + id + "' " +
                             "and mornTimeIn = '' and mornTimeOut = '' and aftTimeIn = '' and aftTimeOut = ''";
                        SqlDataReader sdr = schk.ExecuteReader();

                        if (sdr.Read())
                        {
                            if (att != "ALL EMPLOYEES" || att == "GENERAL")
                            {
                                scon.Open();
                                SqlCommand scmd = scon.CreateCommand();
                                scmd.CommandType = CommandType.Text;
                                scmd.CommandText = "Insert into attendTB values ('" + code + "', " +
                                      "'" + date + "', " +
                                      "'" + stype + "', " +
                                      "'" + id + "', " +
                                      "'" + sfirst + "', " +
                                      "'" + smid + "', " +
                                      "'" + slast + "', " +
                                      "'" + sdept + "', " +
                                      "'" + scourse + "', " +
                                      "'" + syear + "', " +
                                      "'" + smornIn + "', " +
                                      "'" + smornOut + "', " +
                                      "'" + saftIn + "', " +
                                      "'" + saftOut + "', " +
                                      "'" + time + "', " +
                                      "'" + seveOut + "', " +
                                      "'" + mStat + "', " +
                                      "'" + aStat + "', " +
                                      "'" + eStat + "', " +
                                      "'" + stotHrs + "')";
                                scmd.ExecuteNonQuery();
                                scon.Close();

                                lblStat.Text = "ID RECORDED!";
                                tBId.Text = "";
                                eveInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                eveIn();

                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to login!";
                                tBId.Text = "";
                                eveInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                eveIn();
                            }
                           
                        }
                        else
                        {

                            vercon.Open();
                            SqlCommand vercmd = vercon.CreateCommand();
                            vercmd.CommandType = CommandType.Text;
                            vercmd.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                            "and evtDate = '" + date + "' and Id = '" + id + "' " +
                            "and eveTimeIn = '00:00'";
                            SqlDataReader vdr = vercmd.ExecuteReader();

                            if (vdr.Read())
                            {
                                if (att != "ALL EMPLOYEES" || att == "GENERAL")
                                {
                                    studmOut.Open();
                                    SqlCommand sout = studmOut.CreateCommand();
                                    sout.CommandType = CommandType.Text;
                                    sout.CommandText = "Update attendTB set eveTimein = '" + time + "' " +
                                        "where evtCode = '" + code + "' and evtDate = '" + date + "' " +
                                        "and Id = '" + id + "'";
                                    sout.ExecuteNonQuery();
                                    studmOut.Close();

                                    lblStat.Text = "ID RECORDED!";
                                    tBId.Text = "";
                                    eveInTimer.Stop();
                                    FinalFrame.Stop();
                                    pBQR.Image = Properties.Resources.abc;
                                    eveIn();
                                }
                                else
                                {
                                    lblStat.Text = "You are not allowed to login!";
                                    tBId.Text = "";
                                    eveInTimer.Stop();
                                    FinalFrame.Stop();
                                    pBQR.Image = Properties.Resources.abc;
                                    eveIn();
                                }
                            }
                            else
                            {
                                if (att != "ALL EMPLOYEES" || att == "GENERAL")
                                {
                                    lblStat.Text = "ID HAS ALREADY LOGGED IN!";
                                    tBId.Text = "";
                                    eveInTimer.Stop();
                                    FinalFrame.Stop();
                                    pBQR.Image = Properties.Resources.abc;
                                    eveIn();
                                }
                                else
                                {
                                    lblStat.Text = "You are not allowed to login!";
                                    tBId.Text = "";
                                    eveInTimer.Stop();
                                    FinalFrame.Stop();
                                    pBQR.Image = Properties.Resources.abc;
                                    eveIn();
                                }
                               

                            }
                        }
                        studchk.Close();

                    }
                    else
                    {
                        lblStat.Text = "ID NOT FOUND!";
                        tBId.Text = "";
                        eveInTimer.Stop();
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.abc;
                        eveIn();
                    }
                    studcon.Close();
                }
                empcon.Close();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }        
        public void logAftOut()
        {
            SqlConnection empmOut = new SqlConnection(empcs);
            SqlConnection studmOut = new SqlConnection(empcs);
            SqlConnection empcon = new SqlConnection(empcs);
            SqlConnection studcon = new SqlConnection(empcs);
            SqlConnection empchk = new SqlConnection(empcs);
            SqlConnection studchk = new SqlConnection(empcs);
            SqlConnection econ = new SqlConnection(empcs);
            SqlConnection scon = new SqlConnection(empcs);
            SqlConnection vercon = new SqlConnection(empcs);

            string id = tBId.Text;
            string time = DateTime.Now.ToShortTimeString();
            string date = DateTime.Now.ToShortDateString();
            string code = lblEvtCode.Text;
            string att = tBEvtAtt.Text;

            string mStat = "---";
            string aStat = "---";
            string eStat = "---";

            try
            {
                empcon.Open();
                SqlCommand cmd = empcon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from empTB where empId = '" + id + "'";
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    string efirst = (dr["Firstname"].ToString());
                    string emid = (dr["Midname"].ToString());
                    string elast = (dr["Lastname"].ToString());

                    tBFirst.Text = efirst;
                    tbMid.Text = emid;
                    tBLast.Text = elast;

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


                    empchk.Open();
                    SqlCommand echk = empchk.CreateCommand();
                    echk.CommandType = CommandType.Text;
                    echk.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                        "and evtDate = '" + date + "' and Id = '" + id + "' " +
                        "and aftTimeIn != ''";
                    SqlDataReader edr = echk.ExecuteReader();

                    if (edr.Read())
                    {
                        econ.Open();
                        SqlCommand ecmd = econ.CreateCommand();
                        ecmd.CommandType = CommandType.Text;
                        ecmd.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                            "and evtDate = '" + date + "' and Id = '" + id + "' " +
                            "and aftTimeOut = '00:00'";
                        SqlDataReader dr2 = ecmd.ExecuteReader();

                        if (dr2.Read())
                        {
                            if (att == "ALL EMPLOYEES" || att == "GENERAL")
                            {
                                empmOut.Open();
                                SqlCommand eout = empmOut.CreateCommand();
                                eout.CommandText = "Update attendTB set aftTimeOut = '" + time + "' " +
                                    "where evtCode = '" + code + "' and evtDate = '" + date + "' " +
                                    "and Id = '" + id + "'";
                                eout.ExecuteNonQuery();
                                empmOut.Close();

                                lblStat.Text = "ID RECORDED!";
                                tBId.Text = "";
                                aftOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                aftOut();
                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to logout!";
                                tBId.Text = "";
                                aftOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                aftOut();
                            }

                        }
                        else
                        {
                            lblStat.Text = "ID HAS ALREADY LOGGED OUT!";
                            tBId.Text = "";
                            aftOutTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            aftOut();
                        }
                        econ.Close();
                    }
                    else
                    { 
                        if (att == "ALL EMPLOYEES" || att == "GENERAL")
                        {
                            lblStat.Text = "ID HAS NOT LOGGED IN!";
                            tBId.Text = "";
                            aftOutTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            aftOut();
                        }
                        else
                        {
                            lblStat.Text = "You are not allowed to logout!";
                            tBId.Text = "";
                            aftOutTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            aftOut();
                        }
                    }
                    empchk.Close();
                }
                else
                {
                    studcon.Open();
                    SqlCommand cmd2 = studcon.CreateCommand();
                    cmd2.CommandType = CommandType.Text;
                    cmd2.CommandText = "Select * from studTB where studId = '" + id + "'";
                    SqlDataReader dr2 = cmd2.ExecuteReader();

                    if (dr2.Read())
                    {
                        string sfirst = (dr2["Firstname"].ToString());
                        string smid = (dr2["Midname"].ToString());
                        string slast = (dr2["Lastname"].ToString());
                        string syear = (dr2["Year"].ToString());
                        string sdept = (dr2["Dept"].ToString());
                        string scourse = (dr2["Course"].ToString());
                        string stype = "STUDENT";
                        string smornIn = "00:00";
                        string saftIn = "00:00";
                        string smornOut = "00:00";
                        string seveIn = "00:00";
                        string seveOut = "00:00";
                        string stotHrs = "0";

                        tBFirst.Text = sfirst;
                        tbMid.Text = smid;
                        tBLast.Text = slast;

                        byte[] img = (byte[])(dr2["studImg"]);
                        if (img == null)
                        {
                            pBImg.Image = null;
                        }

                        else
                        {
                            MemoryStream ms = new MemoryStream(img);
                            pBImg.Image = Image.FromStream(ms);
                        }

                        studchk.Open();
                        SqlCommand schk = studchk.CreateCommand();
                        schk.CommandType = CommandType.Text;
                        schk.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                            "and evtDate = '" + date + "' and Id = '" + id + "' " +
                            "and mornTimeIn = '' and mornTimeOut = '' and aftTimeIn = ''";
                        SqlDataReader sdr = schk.ExecuteReader();

                        if (sdr.Read())
                        {
                            if (att != "ALL EMPLOYEES" || att == "GENERAL")
                            {
                                scon.Open();
                                SqlCommand scmd = scon.CreateCommand();
                                scmd.CommandType = CommandType.Text;
                                scmd.CommandText = "Insert into attendTB values ('" + code + "', " +
                                      "'" + date + "', " +
                                      "'" + stype + "', " +
                                      "'" + id + "', " +
                                      "'" + sfirst + "', " +
                                      "'" + smid + "', " +
                                      "'" + slast + "', " +
                                      "'" + sdept + "', " +
                                      "'" + scourse + "', " +
                                      "'" + syear + "', " +
                                      "'" + smornIn + "', " +
                                      "'" + smornOut + "', " +
                                      "'" + saftIn + "', " +
                                      "'" + time + "', " +
                                      "'" + seveIn + "', " +
                                      "'" + seveOut + "', " +
                                      "'" + mStat + "', " +
                                      "'" + aStat + "', " +
                                      "'" + eStat + "', " +
                                      "'" + stotHrs + "')";
                                scmd.ExecuteNonQuery();
                                scon.Close();

                                lblStat.Text = "ID RECORDED!";
                                tBId.Text = "";
                                aftOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                aftOut();
                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to logout!";
                                tBId.Text = "";
                                aftOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                aftOut();

                            }
                           

                        }
                        else
                        {

                            vercon.Open();
                            SqlCommand vercmd = vercon.CreateCommand();
                            vercmd.CommandType = CommandType.Text;
                            vercmd.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                            "and evtDate = '" + date + "' and Id = '" + id + "' " +
                            "and aftTimeOut != '00:00' and aftTimeIn != '00:00'";
                            SqlDataReader vdr = vercmd.ExecuteReader();

                            if (vdr.Read())
                            {
                                lblStat.Text = "ID HAS ALREADY LOGGED OUT!";
                                tBId.Text = "";
                                aftOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                aftOut();
                            }
                            else
                            {

                                if (att != "ALL EMPLOYEES" || att == "GENERAL")
                                {
                                    studmOut.Open();
                                    SqlCommand sout = studmOut.CreateCommand();
                                    sout.CommandType = CommandType.Text;
                                    sout.CommandText = "Update attendTB set aftTimeOut = '" + time + "' " +
                                        "where evtCode = '" + code + "' and evtDate = '" + date + "' " +
                                        "and Id = '" + id + "'";
                                    sout.ExecuteNonQuery();
                                    studmOut.Close();

                                    lblStat.Text = "ID RECORDED!";
                                    tBId.Text = "";
                                    aftOutTimer.Stop();
                                    FinalFrame.Stop();
                                    pBQR.Image = Properties.Resources.abc;
                                    aftOut();
                                }
                                else
                                {
                                    lblStat.Text = "You are not allowed to logout!";
                                    tBId.Text = "";
                                    aftOutTimer.Stop();
                                    FinalFrame.Stop();
                                    pBQR.Image = Properties.Resources.abc;
                                    aftOut();
                                }
                            }

                        }
                        studchk.Close();

                    }
                    else
                    {
                        lblStat.Text = "ID NOT FOUND!";
                        tBId.Text = "";
                        aftOutTimer.Stop();
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.abc;
                        aftOut();
                    }
                    studcon.Close();
                }
                empcon.Close();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void logAftIn()
        {
            SqlConnection empmOut = new SqlConnection(empcs);
            SqlConnection studmOut = new SqlConnection(empcs);
            SqlConnection empcon = new SqlConnection(empcs);
            SqlConnection studcon = new SqlConnection(empcs);
            SqlConnection empchk = new SqlConnection(empcs);
            SqlConnection studchk = new SqlConnection(empcs);
            SqlConnection econ = new SqlConnection(empcs);
            SqlConnection scon = new SqlConnection(empcs);
            SqlConnection vercon = new SqlConnection(empcs);
            SqlConnection newemp = new SqlConnection(empcs);

            string id = tBId.Text;
            string time = DateTime.Now.ToShortTimeString();
            string date = DateTime.Now.ToShortDateString();
            string code = lblEvtCode.Text;
            string att = tBEvtAtt.Text;

            string mStat = "---";
            string aStat = "---";
            string eStat = "---";

            try
            {
                empcon.Open();
                SqlCommand cmd = empcon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from empTB where empId = '" + id + "'";
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    string efirst = (dr["Firstname"].ToString());
                    string emid = (dr["Midname"].ToString());
                    string elast = (dr["Lastname"].ToString());
                    string edept = (dr["Dept"].ToString());
                    string ecourse = "NA";
                    string eyear = "NA";
                    string etype = "EMPLOYEE";
                    string emornOut = "00:00";
                    string emornIn = "00:00";
                    string eaftOut = "00:00";
                    string eeveIn = "00:00";
                    string eeveOut = "00:00";
                    string etotHrs = "0";

                    tBFirst.Text = efirst;
                    tbMid.Text = emid;
                    tBLast.Text = elast;

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
                    empchk.Open();
                    SqlCommand echk = empchk.CreateCommand();
                    echk.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                        "and evtDate = '" + date + "' and Id = '" + id + "' " +
                        "and mornTimeIn = '' and mornTimeOut = '' and aftTimeIn = ''";
                    SqlDataReader edr = echk.ExecuteReader();

                    if (edr.Read())
                    {
                        if (att == "ALL EMPLOYEES" || att == "GENERAL")
                        {
                            //Insert
                            newemp.Open();
                            SqlCommand ncmd = newemp.CreateCommand();
                            ncmd.CommandType = CommandType.Text;
                            ncmd.CommandText = "Insert into attendTB values ('" + code + "', " +
                                   "'" + date + "', " +
                                   "'" + etype + "', " +
                                   "'" + id + "', " +
                                   "'" + efirst + "', " +
                                   "'" + emid + "', " +
                                   "'" + elast + "', " +
                                   "'" + edept + "', " +
                                   "'" + ecourse + "', " +
                                   "'" + eyear + "', " +
                                   "'" + emornIn + "', " +
                                   "'" + emornOut + "', " +
                                   "'" + time + "', " +
                                   "'" + eaftOut + "', " +
                                   "'" + eeveIn + "', " +
                                   "'" + eeveOut + "', " +
                                   "'" + mStat + "', " +
                                   "'" + aStat + "', " +
                                   "'" + eStat + "', " +
                                   "'" + etotHrs + "')";
                            ncmd.ExecuteNonQuery();

                            lblStat.Text = "ID RECORDED!";
                            tBId.Text = "";
                            aftInTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            aftIn();
                        }
                        else
                        {
                            lblStat.Text = "You are not allowed to login!";
                            tBId.Text = "";
                            aftInTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            aftIn();
                        }
                        
                    }
                    else
                    {

                        econ.Open();
                        SqlCommand ecmd = econ.CreateCommand();
                        ecmd.CommandType = CommandType.Text;
                        ecmd.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                            "and evtDate = '" + date + "' and Id = '" + id + "' " +
                            "and aftTimeIn = '00:00'";
                        SqlDataReader dr2 = ecmd.ExecuteReader();

                        if (dr2.Read())
                        {
                            if (att == "ALL EMPLOYEES" || att == "GENERAL")
                            {
                                empmOut.Open();
                                SqlCommand eout = empmOut.CreateCommand();
                                eout.CommandText = "Update attendTB set aftTimeIn = '" + time + "' " +
                                    "where evtCode = '" + code + "' and evtDate = '" + date + "' " +
                                    "and Id = '" + id + "'";
                                eout.ExecuteNonQuery();
                                empmOut.Close();

                                lblStat.Text = "ID RECORDED!";
                                tBId.Text = "";
                                aftInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                aftIn();
                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to login!";
                                tBId.Text = "";
                                aftInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                aftIn();
                            }

                        }
                        else
                        {
                            if (att == "ALL EMPLOYEE" || att == "GENERAL")
                            {
                                lblStat.Text = "ID HAS ALREADY LOGGED IN!";
                                tBId.Text = "";
                                aftInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                aftIn();
                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to login!";
                                tBId.Text = "";
                                aftInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                aftIn();
                            }
                           
                        }
                        econ.Close();
                    }
                    empchk.Close();
                }
                else
                {
                    studcon.Open();
                    SqlCommand cmd2 = studcon.CreateCommand();
                    cmd2.CommandType = CommandType.Text;
                    cmd2.CommandText = "Select * from studTB where studId = '" + id + "'";
                    SqlDataReader dr2 = cmd2.ExecuteReader();

                    if (dr2.Read())
                    {
                        string sfirst = (dr2["Firstname"].ToString());
                        string smid = (dr2["Midname"].ToString());
                        string slast = (dr2["Lastname"].ToString());
                        string syear = (dr2["Year"].ToString());
                        string sdept = (dr2["Dept"].ToString());
                        string scourse = (dr2["Course"].ToString());
                        string stype = "STUDENT";
                        string smornIn = "00:00";
                        string smornOut = "00:00";
                        string saftOut = "00:00";
                        string seveIn = "00:00";
                        string seveOut = "00:00";
                        string stotHrs = "0";

                        tBFirst.Text = sfirst;
                        tbMid.Text = smid;
                        tBLast.Text = slast;

                        byte[] img = (byte[])(dr2["studImg"]);
                        if (img == null)
                        {
                            pBImg.Image = null;
                        }

                        else
                        {
                            MemoryStream ms = new MemoryStream(img);
                            pBImg.Image = Image.FromStream(ms);
                        }

                        studchk.Open();
                        SqlCommand schk = studchk.CreateCommand();
                        schk.CommandType = CommandType.Text;
                        schk.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                            "and evtDate = '" + date + "' and Id = '" + id + "' " +
                             "and mornTimeIn = '' and mornTimeOut = '' and aftTimeIn = ''";
                        SqlDataReader sdr = schk.ExecuteReader();

                        if (sdr.Read())
                        {
                            if (att != "ALL EMPLOYEES" || att == "GENERAL")
                            {
                                scon.Open();
                                SqlCommand scmd = scon.CreateCommand();
                                scmd.CommandType = CommandType.Text;
                                scmd.CommandText = "Insert into attendTB values ('" + code + "', " +
                                      "'" + date + "', " +
                                      "'" + stype + "', " +
                                      "'" + id + "', " +
                                      "'" + sfirst + "', " +
                                      "'" + smid + "', " +
                                      "'" + slast + "', " +
                                      "'" + sdept + "', " +
                                      "'" + scourse + "', " +
                                      "'" + syear + "', " +
                                      "'" + smornIn + "', " +
                                      "'" + smornOut + "', " +
                                      "'" + time + "', " +
                                      "'" + saftOut + "', " +
                                      "'" + seveIn + "', " +
                                      "'" + seveOut + "', " +
                                      "'" + mStat + "', " +
                                      "'" + aStat + "', " +
                                      "'" + eStat + "', " +
                                      "'" + stotHrs + "')";
                                scmd.ExecuteNonQuery();
                                scon.Close();

                                lblStat.Text = "ID RECORDED!";
                                tBId.Text = "";
                                aftInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                aftIn();

                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to login!";
                                tBId.Text = "";
                                aftInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                aftIn();

                            }

                        }
                        else
                        {

                            vercon.Open();
                            SqlCommand vercmd = vercon.CreateCommand();
                            vercmd.CommandType = CommandType.Text;
                            vercmd.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                            "and evtDate = '" + date + "' and Id = '" + id + "' " +
                            "and aftTimeIn = '00:00'";
                            SqlDataReader vdr = vercmd.ExecuteReader();

                            if (vdr.Read())
                            {
                                if (att != "ALL EMPLOYEES" || att == "GENERAL")
                                {
                                    studmOut.Open();
                                    SqlCommand sout = studmOut.CreateCommand();
                                    sout.CommandType = CommandType.Text;
                                    sout.CommandText = "Update attendTB set aftTimein = '" + time + "' " +
                                        "where evtCode = '" + code + "' and evtDate = '" + date + "' " +
                                        "and Id = '" + id + "'";
                                    sout.ExecuteNonQuery();
                                    studmOut.Close();

                                    lblStat.Text = "ID RECORDED!";
                                    tBId.Text = "";
                                    aftInTimer.Stop();
                                    FinalFrame.Stop();
                                    pBQR.Image = Properties.Resources.abc;
                                    aftIn();
                                }
                                else
                                {
                                    lblStat.Text = "You are not allowed to login!";
                                    tBId.Text = "";
                                    aftInTimer.Stop();
                                    FinalFrame.Stop();
                                    pBQR.Image = Properties.Resources.abc;
                                    aftIn();
                                }
                            }
                            else
                            {
                                if (att != "ALL EMPLOYEES" || att == "GENERAL")
                                {
                                    lblStat.Text = "ID HAS ALREADY LOGGED IN!";
                                    tBId.Text = "";
                                    aftInTimer.Stop();
                                    FinalFrame.Stop();
                                    pBQR.Image = Properties.Resources.abc;
                                    aftIn();
                                }
                                else
                                {
                                    lblStat.Text = "You are not allowed to log In!";
                                    tBId.Text = "";
                                    aftInTimer.Stop();
                                    FinalFrame.Stop();
                                    pBQR.Image = Properties.Resources.abc;
                                    aftIn();
                                }
                               

                            }
                        }
                        studchk.Close();

                    }
                    else
                    {
                        lblStat.Text = "ID NOT FOUND!";
                        tBId.Text = "";
                        aftInTimer.Stop();
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.abc;
                        aftIn();
                    }
                    studcon.Close();
                }
                empcon.Close();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }         
        public void logMornOut()
        {
            SqlConnection empmOut = new SqlConnection(empcs);
            SqlConnection studmOut = new SqlConnection(empcs);
            SqlConnection empcon = new SqlConnection(empcs);
            SqlConnection studcon = new SqlConnection(empcs);
            SqlConnection empchk = new SqlConnection(empcs);
            SqlConnection studchk = new SqlConnection(empcs);
            SqlConnection econ = new SqlConnection(empcs);
            SqlConnection scon = new SqlConnection(empcs);
            SqlConnection vercon = new SqlConnection(empcs);

            string id = tBId.Text;
            string time = DateTime.Now.ToShortTimeString();
            string date = DateTime.Now.ToShortDateString();
            string code = lblEvtCode.Text;
            string att = tBEvtAtt.Text;

            string mStat = "---";
            string aStat = "---";
            string eStat = "---";

            try
            {
                empcon.Open();
                SqlCommand cmd = empcon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from empTB where empId = '" + id + "'";
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    string efirst = (dr["Firstname"].ToString());
                    string emid = (dr["Midname"].ToString());
                    string elast = (dr["Lastname"].ToString());

                    tBFirst.Text = efirst;
                    tbMid.Text = emid;
                    tBLast.Text = elast;

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


                    empchk.Open();
                    SqlCommand echk = empchk.CreateCommand();
                    echk.CommandType = CommandType.Text;
                    echk.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                        "and evtDate = '" + date + "' and Id = '" + id + "' " +
                        "and mornTimeIn != ''";
                    SqlDataReader edr = echk.ExecuteReader();

                    if (edr.Read())
                    {
                        econ.Open();
                        SqlCommand ecmd = econ.CreateCommand();
                        ecmd.CommandType = CommandType.Text;
                        ecmd.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                            "and evtDate = '" + date + "' and Id = '" + id + "' " +
                            "and mornTimeOut = '00:00'";
                        SqlDataReader dr2 = ecmd.ExecuteReader();

                        if (dr2.Read())
                        {
                            if (att == "ALL EMPLOYEES" || att == "GENERAL")
                            {
                                empmOut.Open();
                                SqlCommand eout = empmOut.CreateCommand();
                                eout.CommandText = "Update attendTB set mornTimeOut = '" + time + "' " +
                                    "where evtCode = '" + code + "' and evtDate = '" + date + "' " +
                                    "and Id = '" + id + "'";
                                eout.ExecuteNonQuery();
                                empmOut.Close();

                                lblStat.Text = "ID RECORDED!";
                                tBId.Text = "";
                                mornOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                morningOut();
                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to logout!";
                                tBId.Text = "";
                                mornOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                morningOut();
                            }

                        }
                        else
                        {
                            if (att == "ALL EMPLOYEES" || att == "GENERAL")
                            {
                                lblStat.Text = "ID HAS ALREADY LOGGED OUT!";
                                tBId.Text = "";
                                mornOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                morningOut();
                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to logout!";
                                tBId.Text = "";
                                mornOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                morningOut();
                            }

                        }
                        econ.Close();
                    }
                    else
                    {
                        if (att == "ALL EMPLOYEES" || att == "GENERAL")
                        {
                            lblStat.Text = "ID HAS NOT LOGGED IN!";
                            tBId.Text = "";
                            mornOutTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            morningOut();
                        }
                        else
                        {
                            lblStat.Text = "You are not allowed to logout!";
                            tBId.Text = "";
                            mornOutTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            morningOut();

                        }
                    }
                    empchk.Close();
                }
                else
                {
                    studcon.Open();
                    SqlCommand cmd2 = studcon.CreateCommand();
                    cmd2.CommandType = CommandType.Text;
                    cmd2.CommandText = "Select * from studTB where studId = '" + id + "'";
                    SqlDataReader dr2 = cmd2.ExecuteReader();

                    if (dr2.Read())
                    {
                        string sfirst = (dr2["Firstname"].ToString());
                        string smid = (dr2["Midname"].ToString());
                        string slast = (dr2["Lastname"].ToString());
                        string syear = (dr2["Year"].ToString());
                        string sdept = (dr2["Dept"].ToString());
                        string scourse = (dr2["Course"].ToString());
                        string stype = "STUDENT";
                        string smornIn = "00:00";
                        string saftIn = "00:00";
                        string saftOut = "00:00";
                        string seveIn = "00:00";
                        string seveOut = "00:00";
                        string stotHrs = "0";

                        tBFirst.Text = sfirst;
                        tbMid.Text = smid;
                        tBLast.Text = slast;

                        byte[] img = (byte[])(dr2["studImg"]);
                        if (img == null)
                        {
                            pBImg.Image = null;
                        }

                        else
                        {
                            MemoryStream ms = new MemoryStream(img);
                            pBImg.Image = Image.FromStream(ms);
                        }

                        studchk.Open();
                        SqlCommand schk = studchk.CreateCommand();
                        schk.CommandType = CommandType.Text;
                        schk.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                            "and evtDate = '" + date + "' and Id = '" + id + "' " +
                            "and mornTimeIn = '' and mornTimeOut = ''";
                        SqlDataReader sdr = schk.ExecuteReader();

                        if (sdr.Read())
                        {
                            if (att != "ALL EMPLOYEES" || att == "GENERAL")
                            {
                                scon.Open();
                                SqlCommand scmd = scon.CreateCommand();
                                scmd.CommandType = CommandType.Text;
                                scmd.CommandText = "Insert into attendTB values ('" + code + "', " +
                                      "'" + date + "', " +
                                      "'" + stype + "', " +
                                      "'" + id + "', " +
                                      "'" + sfirst + "', " +
                                      "'" + smid + "', " +
                                      "'" + slast + "', " +
                                      "'" + sdept + "', " +
                                      "'" + scourse + "', " +
                                      "'" + syear + "', " +
                                      "'" + smornIn + "', " +
                                      "'" + time + "', " +
                                      "'" + saftIn + "', " +
                                      "'" + saftOut + "', " +
                                      "'" + seveIn + "', " +
                                      "'" + seveOut + "', " +
                                      "'" + mStat + "', " +
                                      "'" + aStat + "', " +
                                      "'" + eStat + "', " +
                                      "'" + stotHrs + "')";
                                scmd.ExecuteNonQuery();
                                scon.Close();

                                lblStat.Text = "ID RECORDED!";
                                tBId.Text = "";
                                mornOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                morningOut();
                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to logout!";
                                tBId.Text = "";
                                mornOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                morningOut();
                            }
                           

                        }
                        else
                        {

                            vercon.Open();
                            SqlCommand vercmd = vercon.CreateCommand();
                            vercmd.CommandType = CommandType.Text;
                            vercmd.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                            "and evtDate = '" + date + "' and Id = '" + id + "' " +
                            "and mornTimeOut != '00:00' and mornTimeIn != ''";
                            SqlDataReader vdr = vercmd.ExecuteReader();

                            if (vdr.Read())
                            {
                                lblStat.Text = "ID HAS ALREADY LOGGED OUT!";
                                tBId.Text = "";
                                mornOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                morningOut();
                            }
                            else
                            {

                                if (att != "ALL EMPLOYEES" || att == "GENERAL")
                                {
                                    studmOut.Open();
                                    SqlCommand sout = studmOut.CreateCommand();
                                    sout.CommandType = CommandType.Text;
                                    sout.CommandText = "Update attendTB set mornTimeOut = '" + time + "' " +
                                        "where evtCode = '" + code + "' and evtDate = '" + date + "' " +
                                        "and Id = '" + id + "' and mornTimeIn != ''";
                                    sout.ExecuteNonQuery();
                                    studmOut.Close();

                                    lblStat.Text = "ID RECORDED!";
                                    tBId.Text = "";
                                    mornOutTimer.Stop();
                                    FinalFrame.Stop();
                                    pBQR.Image = Properties.Resources.abc;
                                    morningOut();
                                }
                                else
                                {
                                    lblStat.Text = "You are not allowed to logout!";
                                    tBId.Text = "";
                                    mornOutTimer.Stop();
                                    FinalFrame.Stop();
                                    pBQR.Image = Properties.Resources.abc;
                                    morningOut();
                                }
                            }

                        }
                        studchk.Close();

                    }
                    else
                    {
                        lblStat.Text = "ID NOT FOUND!";
                        tBId.Text = "";
                        mornOutTimer.Stop();
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.abc;
                        morningOut();
                    }
                    studcon.Close();
                }
                empcon.Close();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        } 
        public void logMornIn()
        {
            SqlConnection empcon = new SqlConnection(empcs);
            SqlConnection studcon = new SqlConnection(empcs);
            SqlConnection studins = new SqlConnection(empcs);
            SqlConnection empins = new SqlConnection(empcs);
            SqlConnection empchk = new SqlConnection(empcs);
            SqlConnection studchk = new SqlConnection(empcs);


            string id = tBId.Text;
            string time = DateTime.Now.ToShortTimeString();
            string date = DateTime.Now.ToShortDateString();
            string code = lblEvtCode.Text;
            string att = tBEvtAtt.Text;

            string mStat = "---";
            string aStat = "---";
            string eStat = "---";

            try
            {
                empcon.Open();
                SqlCommand cmd = empcon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from empTB where empId = '" + id + "'";
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    string efirst = (dr["Firstname"].ToString());
                    string emid = (dr["Midname"].ToString());
                    string elast = (dr["Lastname"].ToString());
                    string edept = (dr["Dept"].ToString());
                    string ecourse = "NA";
                    string eyear = "NA";
                    string etype = "EMPLOYEE";
                    string emornOut = "00:00";
                    string eaftIn = "00:00";
                    string eaftOut = "00:00";
                    string eeveIn = "00:00";
                    string eeveOut = "00:00";
                    string etotHrs = "0";

                    tBFirst.Text = efirst;
                    tbMid.Text = emid;
                    tBLast.Text = elast;

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

                    empchk.Open();
                    SqlCommand echk = empchk.CreateCommand();
                    echk.CommandType = CommandType.Text;
                    echk.CommandText = "Select * from attendTB where Id = '" + id + "' " +
                        "and evtCode = '" + code + "' and evtDate = '" + date + "'";
                    SqlDataReader edr = echk.ExecuteReader();

                    if (edr.Read())
                    {
                        if (att == "ALL EMPLOYEES" || att == "GENERAL")
                        {
                            lblStat.Text = "ID ALREADY LOGGED IN";
                            tBId.Text = "";
                            mornInTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            morningIn();
                        }
                        else
                        {
                            lblStat.Text = "You are not allowed to login!";
                            tBId.Text = "";
                            mornInTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            morningIn();
                        }
                        
                    }
                    else
                    {

                        if (att == "ALL EMPLOYESS" || att == "GENERAL")
                        {
                            empins.Open();
                            SqlCommand ecmd = empins.CreateCommand();
                            ecmd.CommandType = CommandType.Text;
                            ecmd.CommandText = "Insert into attendTB values ('" + code + "', " +
                                "'" + date + "', " +
                                "'" + etype + "', " +
                                "'" + id + "', " +
                                "'" + efirst + "', " +
                                "'" + emid + "', " +
                                "'" + elast + "', " +
                                "'" + edept + "', " +
                                "'" + ecourse + "', " +
                                "'" + eyear + "', " +
                                "'" + time + "', " +
                                "'" + emornOut + "', " +
                                "'" + eaftIn + "', " +
                                "'" + eaftOut + "', " +
                                "'" + eeveIn + "', " +
                                "'" + eeveOut + "', " +
                                "'" + mStat + "', " +
                                "'" + aStat + "', " +
                                "'" + eStat + "', " +
                                "'" + etotHrs + "')";
                            ecmd.ExecuteNonQuery();
                            empins.Close();

                            lblStat.Text = "ID RECORDED";
                            tBId.Text = "";
                            mornInTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            morningIn();
                        }
                        else
                        {
                            lblStat.Text = "You are not allowed to login!";
                            tBId.Text = "";
                            mornInTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            morningIn();
                        }
                    }
                    empchk.Close();
                }
                else
                {
                    studcon.Open();
                    SqlCommand cmd2 = studcon.CreateCommand();
                    cmd2.CommandType = CommandType.Text;
                    cmd2.CommandText = "Select * from studTB where studId = '" + id + "'";
                    SqlDataReader dr2 = cmd2.ExecuteReader();

                    if (dr2.Read())
                    {
                        string sfirst = (dr2["Firstname"].ToString());
                        string smid = (dr2["Midname"].ToString());
                        string slast = (dr2["Lastname"].ToString());
                        string syear = (dr2["Year"].ToString());
                        string sdept = (dr2["Dept"].ToString());
                        string scourse = (dr2["Course"].ToString());
                        string stype = "STUDENT";
                        string smornOut = "00:00";
                        string saftIn = "00:00";
                        string saftOut = "00:00";
                        string seveIn = "00:00";
                        string seveOut = "00:00";
                        string stotHrs = "0";

                        tBFirst.Text = sfirst;
                        tbMid.Text = smid;
                        tBLast.Text = slast;

                        byte[] img = (byte[])(dr2["studImg"]);
                        if (img == null)
                        {
                            pBImg.Image = null;
                        }

                        else
                        {
                            MemoryStream ms = new MemoryStream(img);
                            pBImg.Image = Image.FromStream(ms);
                        }

                        studchk.Open();
                        SqlCommand schk = studchk.CreateCommand();
                        schk.CommandType = CommandType.Text;
                        schk.CommandText = "Select * from attendTB where Id = '" + id + "' " +
                            "and evtCode = '" + code + "' and evtDate = '" + date + "'";
                        SqlDataReader sdr = schk.ExecuteReader();

                        if (sdr.Read())
                        {
                            if (att != "ALL EMPLOYEES" || att == "GENERAL")
                            {
                                lblStat.Text = "ID ALREADY LOGGED IN";
                                mornInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                morningIn();
                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to login!";
                                mornInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                morningIn();
                            }
                           
                        }
                        else
                        {
                            if (att != "ALL EMPLOYEES" || att == "GENERAL")
                            {
                                studins.Open();
                                SqlCommand scmd = studins.CreateCommand();
                                scmd.CommandType = CommandType.Text;
                                scmd.CommandText = "Insert into attendTB values ('" + code + "', " +
                                 "'" + date + "', " +
                                 "'" + stype + "', " +
                                 "'" + id + "', " +
                                 "'" + sfirst + "', " +
                                 "'" + smid + "', " +
                                 "'" + slast + "', " +
                                 "'" + sdept + "', " +
                                 "'" + scourse + "', " +
                                 "'" + syear + "', " +
                                 "'" + time + "', " +
                                 "'" + smornOut + "', " +
                                 "'" + saftIn + "', " +
                                 "'" + saftOut + "', " +
                                 "'" + seveIn + "', " +
                                 "'" + seveOut + "', " +
                                 "'" + mStat + "', " +
                                 "'" + aStat + "', " +
                                 "'" + eStat + "', " +
                                 "'" + stotHrs + "')";
                                scmd.ExecuteNonQuery();
                                studins.Close();

                                lblStat.Text = "ID RECORDED";
                                tBId.Text = "";
                                mornInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                morningIn();
                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to login!";
                                tBId.Text = "";
                                mornInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                morningIn();
                            }
                        }
                        studchk.Close();
                    }
                    else
                    {
                        lblStat.Text = "ID NOT FOUND!";
                        tBId.Text = "";
                        mornInTimer.Stop();
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.abc;
                        morningIn();
                    }
                    studcon.Close();
                }
                empcon.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + " MornIn", " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
       
        
        
        public void eveOut()
        {
            capCam();
            eveOutTimer.Enabled = true;
            eveOutTimer.Start();
        }
        public void eveIn()
        {
            capCam();
            eveInTimer.Enabled = true;
            eveInTimer.Start();
        }
        public void aftOut()
        {
            capCam();
            aftOutTimer.Enabled = true;
            aftOutTimer.Start();
        }
        public void aftIn()
        {
            capCam();
            aftInTimer.Enabled = true;
            aftInTimer.Start();

        }
        public void morningIn()
        {
            capCam();
            mornInTimer.Enabled = true;
            mornInTimer.Start();

        }
        public void morningOut()
        {
            capCam();
            mornOutTimer.Enabled = true;
            mornOutTimer.Start();
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
                        pBQR.Image = Properties.Resources.abc;
                        logMornIn();

                    }
                    else
                    {
                        lblStat.Text = "Verifying...";

                    }
                }

            }
            catch (Exception f)
            {
                MessageBox.Show(f.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnMornOut_Click(object sender, EventArgs e)
        {
            try
            {
                if (lblEvtCode.Text != "0000000")
                {
                    if (tBMornIn.Text != "NONE" && tBAftIn.Text != "NONE" && tBEveIn.Text != "NONE")
                    {
                        lblStat.Text = "Starting...";
                        string ntime = DateTime.Now.ToShortTimeString();
                        DateTime mIn = DateTime.Parse(tBMornIn.Text);
                        DateTime mOut = DateTime.Parse(tBMornOut.Text);
                        DateTime aIn = DateTime.Parse(tBAftIn.Text);
                        DateTime time = DateTime.Parse(ntime);

                        if (time < aIn && time >= mOut)
                        {
                            mornInTimer.Stop();
                            FinalFrame.Stop();
                            morningOut();
                        }
                        else
                        {
                            lblStat.Text = "TIME NOT MEET!";
                        }
                    }
                    else if (tBMornIn.Text != "NONE" && tBAftIn.Text == "NONE" && tBEveIn.Text == "NONE")
                    {
                        lblStat.Text = "Starting...";
                        string ntime = DateTime.Now.ToShortTimeString();
                        DateTime mOut = DateTime.Parse(tBMornOut.Text);    
                        DateTime time = DateTime.Parse(ntime);

                        if (time >= mOut)
                        {
                            sinMornInTimer.Stop();
                            FinalFrame.Stop();
                            sinMornOut();
                        }
                        else
                        {
                            lblStat.Text = "TIME NOT MEET!";
                        }
                    }
                    else
                    {
                        lblStat.Text = "ERROR!";
                    }
                }
                else
                {
                    lblStat.Text = "EVENT EMPTY!";
                }
            }
            catch (Exception f)
            {
                MessageBox.Show(f.Message);
            }
        }
        private void mornOutTimer_Tick(object sender, EventArgs e)
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
                        mornOutTimer.Stop();
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.abc;
                        logMornOut();

                    }
                    else
                    {
                        lblStat.Text = "Verifying...";

                    }
                }

            }
            catch (Exception f)
            {
                MessageBox.Show(f.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void btnAftIn_Click(object sender, EventArgs e)
        {
            try
            {
                if (lblEvtCode.Text != "0000000")
                {
                    if (tBMornIn.Text != "NONE" && tBAftIn.Text != "NONE" && tBEveIn.Text != "NONE")
                    {
                        lblStat.Text = "Starting...";
                        string ntime = DateTime.Now.ToShortTimeString();
                        DateTime mIn = DateTime.Parse(tBAftIn.Text);
                        DateTime mOut = DateTime.Parse(tBAftOut.Text);
                        DateTime time = DateTime.Parse(ntime);

                        if (time >= mIn && time < mOut)
                        {
                            mornOutTimer.Stop();
                            FinalFrame.Stop();
                            aftIn();
                        }
                        else
                        {
                            lblStat.Text = "TIME NOT MEET!";
                        }
                    }
                    else if (tBAftIn.Text != "NONE" && tBMornIn.Text == "NONE" && tBEveIn.Text == "NONE")
                    {
                        lblStat.Text = "Starting...";
                        string ntime = DateTime.Now.ToShortTimeString();
                        DateTime mIn = DateTime.Parse(tBAftIn.Text);
                        DateTime mOut = DateTime.Parse(tBAftOut.Text);
                        DateTime time = DateTime.Parse(ntime);

                        if (time >= mIn && time < mOut)
                        {
                            sinMornOutTimer.Stop();
                            FinalFrame.Stop();
                            sinAftIn();
                        }
                        else
                        {
                            lblStat.Text = "TIME NOT MEET!";
                        }
                    }
                    else
                    {
                        lblStat.Text = "ERROR!";
                    }
                }
                else
                {
                    lblStat.Text = "EVENT EMPTY!";
                }
            }
            catch (Exception f)
            {
                MessageBox.Show(f.Message);
            }

        }
        private void aftInTimer_Tick(object sender, EventArgs e)
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
                        aftInTimer.Stop();
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.kali;
                        logAftIn();
                    }
                    else
                    {
                        lblStat.Text = "Verifying...";

                    }
                }

            }
            catch (Exception f)
            {
                MessageBox.Show(f.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void btnAftOut_Click(object sender, EventArgs e)
        {
            try
            {
                if (lblEvtCode.Text != "0000000")
                {
                    if (tBMornIn.Text != "NONE" && tBAftIn.Text != "NONE" && tBEveIn.Text != "NONE")
                    {
                        lblStat.Text = "Starting...";
                        string ntime = DateTime.Now.ToShortTimeString();
                        DateTime mIn = DateTime.Parse(tBMornIn.Text);
                        DateTime mOut = DateTime.Parse(tBAftOut.Text);
                        DateTime aIn = DateTime.Parse(tBEveIn.Text);
                        DateTime time = DateTime.Parse(ntime);

                        if (time < aIn && time >= mOut)
                        {
                            aftInTimer.Stop();
                            FinalFrame.Stop();
                            aftOut();
                        }
                        else
                        {
                            lblStat.Text = "TIME NOT MEET!";
                        }
                    }
                    else if (tBAftIn.Text != "NONE" && tBMornIn.Text == "NONE" && tBEveIn.Text == "NONE")
                    {
                        lblStat.Text = "Starting...";
                        string ntime = DateTime.Now.ToShortTimeString();        
                        DateTime mOut = DateTime.Parse(tBAftOut.Text);
                        DateTime time = DateTime.Parse(ntime);

                        if (time >= mOut)
                        {
                            sinAftInTimer.Stop();
                            FinalFrame.Stop();
                            sinAftOut();
                        }
                        else
                        {
                            lblStat.Text = "TIME NOT MEET!";
                        }
                    }
                    else
                    {
                        lblStat.Text = "ERROR!";
                    }
                }
                else
                {
                    lblStat.Text = "EVENT EMPTY!";
                }
            }
            catch (Exception f)
            {
                lblStat.Text = "ERROR!";
            }
        }
        private void aftOutTimer_Tick(object sender, EventArgs e)
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
                        aftOutTimer.Stop();
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.abc;
                        logAftOut();
                    }
                    else
                    {
                        lblStat.Text = "Verifying...";

                    }
                }

            }
            catch (Exception f)
            {
                MessageBox.Show(f.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void eveInTimer_Tick(object sender, EventArgs e)
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
                        aftInTimer.Stop();
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.abc;
                        logEveIn();
                    }
                    else
                    {
                        lblStat.Text = "Verifying...";

                    }
                }

            }
            catch (Exception f)
            {
                MessageBox.Show(f.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnEveIn_Click(object sender, EventArgs e)
        {
            try
            {
                if (lblEvtCode.Text != "0000000")
                {
                    if (tBMornIn.Text != "NONE" && tBAftIn.Text != "NONE" && tBEveIn.Text != "NONE")
                    {
                        lblStat.Text = "Starting...";
                        string ntime = DateTime.Now.ToShortTimeString();
                        DateTime mIn = DateTime.Parse(tBEveIn.Text);
                        DateTime mOut = DateTime.Parse(tBEveOut.Text);
                        DateTime time = DateTime.Parse(ntime);

                        if (time >= mIn && time < mOut)
                        {
                            aftInTimer.Stop();
                            FinalFrame.Stop();
                            eveIn();
                        }
                        else
                        {
                            lblStat.Text = "TIME NOT MEET!";
                        }
                    }
                    else if (tBMornIn.Text == "NONE" && tBAftIn.Text == "NONE" && tBEveIn.Text != "NONE")
                    {
                        lblStat.Text = "Starting...";
                        string ntime = DateTime.Now.ToShortTimeString();
                        DateTime mIn = DateTime.Parse(tBEveIn.Text); 
                        DateTime time = DateTime.Parse(ntime);

                        if (time >= mIn)
                        {
                            sinEveInTimer.Stop();
                            FinalFrame.Stop();
                            sinEveIn();
                        }
                        else
                        {
                            lblStat.Text = "TIME NOT MEET!";
                        }
                    }
                    else
                    {
                        lblStat.Text = "ERROR!";
                    }
                }
                else
                {
                    lblStat.Text = "EVENT EMPTY!";
                }
            }
            catch (Exception f)
            {
                lblStat.Text = "ERROR!";
            }

        }
        private void eveOutTimer_Tick(object sender, EventArgs e)
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
                        eveOutTimer.Stop();
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.abc;
                        logEveOut();
                    }
                    else
                    {
                        lblStat.Text = "Verifying...";

                    }
                }

            }
            catch (Exception f)
            {
                MessageBox.Show(f.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnEveOut_Click(object sender, EventArgs e)
        {
            try
            {
                if (lblEvtCode.Text != "0000000")
                {
                    if (tBMornIn.Text != "NONE" && tBAftIn.Text != "NONE" && tBEveIn.Text != "NONE")
                    {
                        lblStat.Text = "Starting...";
                        string ntime = DateTime.Now.ToShortTimeString();

                        DateTime eIn = DateTime.Parse(tBEveIn.Text);
                        DateTime eOut = DateTime.Parse(tBEveOut.Text);

                        DateTime time = DateTime.Parse(ntime);

                        if (time >= eOut)
                        {
                            eveInTimer.Stop();
                            FinalFrame.Stop();
                            eveOut();
                        }
                        else
                        {
                            lblStat.Text = "TIME NOT MEET!";
                        }
                    }
                    else if (tBEveIn.Text != "NONE" && tBMornIn.Text == "NONE" && tBAftIn.Text == "NONE")
                    {
                        lblStat.Text = "Starting...";
                        string ntime = DateTime.Now.ToShortTimeString();
                        DateTime mOut = DateTime.Parse(tBEveOut.Text);
                        DateTime time = DateTime.Parse(ntime);

                        if (time >= mOut)
                        {
                            sinEveInTimer.Stop();
                            FinalFrame.Stop();
                            sinEveOut();
                        }
                        else
                        {
                            lblStat.Text = "TIME NOT MEET!";
                        }
                    }
                    else
                    {
                        lblStat.Text = "ERROR!";
                    }
                }
                else
                {
                    lblStat.Text = "EVENT EMPTY!";
                }
            }
            catch (Exception f)
            {
                lblStat.Text = "ERROR!";
            }
        }

        #endregion

        #region Single Schedule Attendance
        public void sinEveIn()
        {
            capCam();
            sinEveInTimer.Enabled = true;
            sinEveInTimer.Start();
        } 
        public void sinAftIn()
        {
            capCam();
            sinAftInTimer.Enabled = true;
            sinAftInTimer.Start();
        }
        public void sinMornIn()
        {
            capCam();
            sinMornInTimer.Enabled = true;
            sinMornInTimer.Start();
        }
        public void sinMornOut()
        {
            capCam();
            sinMornOutTimer.Enabled = true;
            sinMornOutTimer.Start();
        }
        public void sinEveOut()
        {
            capCam();
            sinEveOutTimer.Enabled = true;
            sinEveOutTimer.Start();
        }
        public void sinAftOut()
        {
            capCam();
            sinAftOutTimer.Enabled = true;
            sinAftOutTimer.Start();
        }
        public void snLogEveOut()
        {
            SqlConnection empmOut = new SqlConnection(empcs);
            SqlConnection empcon = new SqlConnection(empcs);
            SqlConnection studcon = new SqlConnection(empcs);
            SqlConnection empchk = new SqlConnection(empcs);
            SqlConnection studchk = new SqlConnection(empcs);
            SqlConnection econ = new SqlConnection(empcs);
            SqlConnection scon = new SqlConnection(empcs);
            SqlConnection vercon = new SqlConnection(empcs);

            string id = tBId.Text;
            string time = DateTime.Now.ToShortTimeString();
            string date = DateTime.Now.ToShortDateString();
            string code = lblEvtCode.Text;
            string att = tBEvtAtt.Text;

            try
            {
                empcon.Open();
                SqlCommand cmd = empcon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from empTB where empId = '" + id + "'";
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    string efirst = (dr["Firstname"].ToString());
                    string emid = (dr["Midname"].ToString());
                    string elast = (dr["Lastname"].ToString());

                    tBFirst.Text = efirst;
                    tbMid.Text = emid;
                    tBLast.Text = elast;

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


                    empchk.Open();
                    SqlCommand echk = empchk.CreateCommand();
                    echk.CommandType = CommandType.Text;
                    echk.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                        "and evtDate = '" + date + "' and Id = '" + id + "' " +
                        "and eveTimeIn != ''";
                    SqlDataReader edr = echk.ExecuteReader();

                    if (edr.Read())
                    {
                        econ.Open();
                        SqlCommand ecmd = econ.CreateCommand();
                        ecmd.CommandType = CommandType.Text;
                        ecmd.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                            "and evtDate = '" + date + "' and Id = '" + id + "' " +
                            "and eveTimeOut = '00:00'";
                        SqlDataReader dr2 = ecmd.ExecuteReader();

                        if (dr2.Read())
                        {
                            if (att == "ALL EMPLOYEES" || att == "GENERAL")
                            {
                                empmOut.Open();
                                SqlCommand eout = empmOut.CreateCommand();
                                eout.CommandText = "Update attendTB set eveTimeOut = '" + time + "' " +
                                    "where evtCode = '" + code + "' and evtDate = '" + date + "' " +
                                    "and Id = '" + id + "'";
                                eout.ExecuteNonQuery();
                                empmOut.Close();

                                lblStat.Text = "ID RECORDED!";
                                tBId.Text = "";
                                sinEveOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                sinEveOut();
                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to logout!";
                                tBId.Text = "";
                                sinEveOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                sinEveOut();
                            }

                        }
                        else
                        {
                            if (att == "ALL EMPLOYEES" || att == "GENERAL")
                            {
                                lblStat.Text = "ID HAS ALREADY LOGGED OUT!";
                                tBId.Text = "";
                                sinEveOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                sinEveOut();
                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to logout!";
                                tBId.Text = "";
                                sinEveOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                sinEveOut();
                            }

                        }
                        econ.Close();
                    }
                    else
                    {
                        if (att == "ALL EMPLOYEES" || att == "GENERAL")
                        {
                            lblStat.Text = "ID HAS NOT LOGGED IN!";
                            tBId.Text = "";
                            sinEveOutTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            sinEveOut();
                        }
                        else
                        {
                            lblStat.Text = "You are not allowed to logout!";
                            tBId.Text = "";
                            sinEveOutTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            sinEveOut();

                        }
                    }
                    empchk.Close();
                }
                else
                {
                    studcon.Open();
                    SqlCommand cmd2 = studcon.CreateCommand();
                    cmd2.CommandType = CommandType.Text;
                    cmd2.CommandText = "Select * from studTB where studId = '" + id + "' ";
                    SqlDataReader dr2 = cmd2.ExecuteReader();

                    if (dr2.Read())
                    {
                        string sfirst = (dr2["Firstname"].ToString());
                        string smid = (dr2["Midname"].ToString());
                        string slast = (dr2["Lastname"].ToString());

                        tBFirst.Text = sfirst;
                        tbMid.Text = smid;
                        tBLast.Text = slast;

                        byte[] img = (byte[])(dr2["studImg"]);
                        if (img == null)
                        {
                            pBImg.Image = null;
                        }

                        else
                        {
                            MemoryStream ms = new MemoryStream(img);
                            pBImg.Image = Image.FromStream(ms);
                        }


                        if (att != "ALL EMPLOYEE" || att == "GENERAL")
                        {
                            studchk.Open();
                            SqlCommand schk = studchk.CreateCommand();
                            schk.CommandType = CommandType.Text;
                            schk.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                                 "and evtDate = '" + date + "' and Id = '" + id + "' " +
                                 "and eveTimeIn != ''";
                            SqlDataReader sdr = schk.ExecuteReader();

                            if (sdr.Read())
                            {
                                vercon.Open();
                                SqlCommand vercmd = vercon.CreateCommand();
                                vercmd.CommandType = CommandType.Text;
                                vercmd.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                                      "and evtDate = '" + date + "' and Id = '" + id + "' " +
                                       "and eveTimeOut = '00:00'";
                                SqlDataReader verDr = vercmd.ExecuteReader();

                                if (verDr.Read())
                                {
                                    scon.Open();
                                    SqlCommand scmd = scon.CreateCommand();
                                    scmd.CommandType = CommandType.Text;
                                    scmd.CommandText = "Update attendTB set eveTimeOut = '" + time + "' " +
                                        "where evtCode = '" + code + "' and evtDate = '" + date + "' " +
                                        "and Id = '" + id + "'";
                                    scmd.ExecuteNonQuery();
                                    scon.Close();

                                    lblStat.Text = "ID RECORDED!";
                                    tBId.Text = "";
                                    sinEveOutTimer.Stop();
                                    FinalFrame.Stop();
                                    pBQR.Image = Properties.Resources.abc;
                                    sinEveOut();
                                }
                                else
                                {
                                    lblStat.Text = "ID ALREADY LOGGED OUT!";
                                    tBId.Text = "";
                                    sinEveOutTimer.Stop();
                                    FinalFrame.Stop();
                                    pBQR.Image = Properties.Resources.abc;
                                    sinEveOut();

                                }
                                vercon.Close();
                               
                            }
                            else
                            {
                                lblStat.Text = "ID NOT LOGGED IN!";
                                tBId.Text = "";
                                sinEveOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                sinEveOut();
                            }
                            studchk.Close();
                        }
                        else
                        {
                            lblStat.Text = "You are not allowed to logout!";
                            tBId.Text = "";
                            sinEveOutTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            sinEveOut();
                        }
                    }
                    else
                    {
                        lblStat.Text = "ID NOT FOUND!";
                        tBId.Text = "";
                        sinEveOutTimer.Stop();
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.abc;
                        sinEveOut();
                    }
                    studcon.Close();
                }
                empcon.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void snLogAftOut()
        {
            SqlConnection empmOut = new SqlConnection(empcs);
            SqlConnection empcon = new SqlConnection(empcs);
            SqlConnection studcon = new SqlConnection(empcs);
            SqlConnection empchk = new SqlConnection(empcs);
            SqlConnection studchk = new SqlConnection(empcs);
            SqlConnection econ = new SqlConnection(empcs);
            SqlConnection scon = new SqlConnection(empcs);
            SqlConnection vercon = new SqlConnection(empcs);

            string id = tBId.Text;
            string time = DateTime.Now.ToShortTimeString();
            string date = DateTime.Now.ToShortDateString();
            string code = lblEvtCode.Text;
            string att = tBEvtAtt.Text;

            try
            {
                empcon.Open();
                SqlCommand cmd = empcon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from empTB where empId = '" + id + "'";
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    string efirst = (dr["Firstname"].ToString());
                    string emid = (dr["Midname"].ToString());
                    string elast = (dr["Lastname"].ToString());

                    tBFirst.Text = efirst;
                    tbMid.Text = emid;
                    tBLast.Text = elast;

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


                    empchk.Open();
                    SqlCommand echk = empchk.CreateCommand();
                    echk.CommandType = CommandType.Text;
                    echk.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                        "and evtDate = '" + date + "' and Id = '" + id + "' " +
                        "and aftTimeIn != ''";
                    SqlDataReader edr = echk.ExecuteReader();

                    if (edr.Read())
                    {
                        econ.Open();
                        SqlCommand ecmd = econ.CreateCommand();
                        ecmd.CommandType = CommandType.Text;
                        ecmd.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                            "and evtDate = '" + date + "' and Id = '" + id + "' " +
                            "and aftTimeOut = '00:00'";
                        SqlDataReader dr2 = ecmd.ExecuteReader();

                        if (dr2.Read())
                        {
                            if (att == "ALL EMPLOYEES" || att == "GENERAL")
                            {
                                empmOut.Open();
                                SqlCommand eout = empmOut.CreateCommand();
                                eout.CommandText = "Update attendTB set aftTimeOut = '" + time + "' " +
                                    "where evtCode = '" + code + "' and evtDate = '" + date + "' " +
                                    "and Id = '" + id + "'";
                                eout.ExecuteNonQuery();
                                empmOut.Close();

                                lblStat.Text = "ID RECORDED!";
                                tBId.Text = "";
                                sinAftOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                sinAftOut();
                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to logout!";
                                tBId.Text = "";
                                sinAftOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                sinAftOut();
                            }

                        }
                        else
                        {
                            if (att == "ALL EMPLOYEES" || att == "GENERAL")
                            {
                                lblStat.Text = "ID HAS ALREADY LOGGED OUT!";
                                tBId.Text = "";
                                sinAftOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                sinAftOut();
                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to logout!";
                                tBId.Text = "";
                                sinAftOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                sinAftOut();
                            }

                        }
                        econ.Close();
                    }
                    else
                    {
                        if (att == "ALL EMPLOYEES" || att == "GENERAL")
                        {
                            lblStat.Text = "ID HAS NOT LOGGED IN!";
                            tBId.Text = "";
                            sinAftOutTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            sinAftOut();
                        }
                        else
                        {
                            lblStat.Text = "You are not allowed to logout!";
                            tBId.Text = "";
                            sinAftOutTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            sinAftOut();

                        }
                    }
                    empchk.Close();
                }
                else
                {
                    studcon.Open();
                    SqlCommand cmd2 = studcon.CreateCommand();
                    cmd2.CommandType = CommandType.Text;
                    cmd2.CommandText = "Select * from studTB where studId = '" + id + "' ";
                    SqlDataReader dr2 = cmd2.ExecuteReader();

                    if (dr2.Read())
                    {
                        string sfirst = (dr2["Firstname"].ToString());
                        string smid = (dr2["Midname"].ToString());
                        string slast = (dr2["Lastname"].ToString());

                        tBFirst.Text = sfirst;
                        tbMid.Text = smid;
                        tBLast.Text = slast;

                        byte[] img = (byte[])(dr2["studImg"]);
                        if (img == null)
                        {
                            pBImg.Image = null;
                        }

                        else
                        {
                            MemoryStream ms = new MemoryStream(img);
                            pBImg.Image = Image.FromStream(ms);
                        }


                        if (att != "ALL EMPLOYEE" || att == "GENERAL")
                        {
                            studchk.Open();
                            SqlCommand schk = studchk.CreateCommand();
                            schk.CommandType = CommandType.Text;
                            schk.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                                 "and evtDate = '" + date + "' and Id = '" + id + "' " +
                                 "and aftTimeIn != ''";
                            SqlDataReader sdr = schk.ExecuteReader();

                            if (sdr.Read())
                            {
                                vercon.Open();
                                SqlCommand vercmd = vercon.CreateCommand();
                                vercmd.CommandType = CommandType.Text;
                                vercmd.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                                      "and evtDate = '" + date + "' and Id = '" + id + "' " +
                                       "and aftTimeOut = '00:00'";
                                SqlDataReader verDr = vercmd.ExecuteReader();

                                if (verDr.Read())
                                {
                                    scon.Open();
                                    SqlCommand scmd = scon.CreateCommand();
                                    scmd.CommandType = CommandType.Text;
                                    scmd.CommandText = "Update attendTB set aftTimeOut = '" + time + "' " +
                                        "where evtCode = '" + code + "' and evtDate = '" + date + "' " +
                                        "and Id = '" + id + "'";
                                    scmd.ExecuteNonQuery();
                                    scon.Close();

                                    lblStat.Text = "ID RECORDED!";
                                    tBId.Text = "";
                                    sinAftOutTimer.Stop();
                                    FinalFrame.Stop();
                                    pBQR.Image = Properties.Resources.abc;
                                    sinAftOut();
                                }
                                else
                                {
                                    lblStat.Text = "ID ALREADY LOGGED OUT!";
                                    tBId.Text = "";
                                    sinAftOutTimer.Stop();
                                    FinalFrame.Stop();
                                    pBQR.Image = Properties.Resources.abc;
                                    sinAftOut();

                                }
                                vercon.Close();

                            }
                            else
                            {
                                lblStat.Text = "ID NOT LOGGED IN!";
                                tBId.Text = "";
                                sinAftOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                sinAftOut();
                            }
                            studchk.Close();
                        }
                        else
                        {
                            lblStat.Text = "You are not allowed to logout!";
                            tBId.Text = "";
                            sinAftOutTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            sinAftOut();
                        }
                    }
                    else
                    {
                        lblStat.Text = "ID NOT FOUND!";
                        tBId.Text = "";
                        sinAftOutTimer.Stop();
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.abc;
                        sinAftOut();
                    }
                    studcon.Close();
                }
                empcon.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        public void snLogMornOut()
        {
            SqlConnection empmOut = new SqlConnection(empcs);
            SqlConnection empcon = new SqlConnection(empcs);
            SqlConnection studcon = new SqlConnection(empcs);
            SqlConnection empchk = new SqlConnection(empcs);
            SqlConnection studchk = new SqlConnection(empcs);
            SqlConnection econ = new SqlConnection(empcs);
            SqlConnection scon = new SqlConnection(empcs);
            SqlConnection vercon = new SqlConnection(empcs);

            string id = tBId.Text;
            string time = DateTime.Now.ToShortTimeString();
            string date = DateTime.Now.ToShortDateString();
            string code = lblEvtCode.Text;
            string att = tBEvtAtt.Text;

            try
            {
                empcon.Open();
                SqlCommand cmd = empcon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from empTB where empId = '" + id + "'";
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    string efirst = (dr["Firstname"].ToString());
                    string emid = (dr["Midname"].ToString());
                    string elast = (dr["Lastname"].ToString());

                    tBFirst.Text = efirst;
                    tbMid.Text = emid;
                    tBLast.Text = elast;

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


                    empchk.Open();
                    SqlCommand echk = empchk.CreateCommand();
                    echk.CommandType = CommandType.Text;
                    echk.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                        "and evtDate = '" + date + "' and Id = '" + id + "' " +
                        "and mornTimeIn != ''";
                    SqlDataReader edr = echk.ExecuteReader();

                    if (edr.Read())
                    {
                        econ.Open();
                        SqlCommand ecmd = econ.CreateCommand();
                        ecmd.CommandType = CommandType.Text;
                        ecmd.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                            "and evtDate = '" + date + "' and Id = '" + id + "' " +
                            "and mornTimeOut = '00:00'";
                        SqlDataReader dr2 = ecmd.ExecuteReader();

                        if (dr2.Read())
                        {
                            if (att == "ALL EMPLOYEES" || att == "GENERAL")
                            {
                                empmOut.Open();
                                SqlCommand eout = empmOut.CreateCommand();
                                eout.CommandText = "Update attendTB set mornTimeOut = '" + time + "' " +
                                    "where evtCode = '" + code + "' and evtDate = '" + date + "' " +
                                    "and Id = '" + id + "'";
                                eout.ExecuteNonQuery();
                                empmOut.Close();

                                lblStat.Text = "ID RECORDED!";
                                tBId.Text = "";
                                sinMornOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                sinMornOut();
                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to logout!";
                                tBId.Text = "";
                                sinMornOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                sinMornOut();
                            }

                        }
                        else
                        {
                            if (att == "ALL EMPLOYEES" || att == "GENERAL")
                            {
                                lblStat.Text = "ID HAS ALREADY LOGGED OUT!";
                                tBId.Text = "";
                                sinMornOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                sinMornOut();
                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to logout!";
                                tBId.Text = "";
                                sinMornOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                sinMornOut();
                            }

                        }
                        econ.Close();
                    }
                    else
                    {
                        if (att == "ALL EMPLOYEES" || att == "GENERAL")
                        {
                            lblStat.Text = "ID HAS NOT LOGGED IN!";
                            tBId.Text = "";
                            sinMornOutTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            sinMornOut();
                        }
                        else
                        {
                            lblStat.Text = "You are not allowed to logout!";
                            tBId.Text = "";
                            sinMornOutTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            sinMornOut();

                        }
                    }
                    empchk.Close();
                }
                else
                {
                    studcon.Open();
                    SqlCommand cmd2 = studcon.CreateCommand();
                    cmd2.CommandType = CommandType.Text;
                    cmd2.CommandText = "Select * from studTB where studId = '" + id + "' ";
                    SqlDataReader dr2 = cmd2.ExecuteReader();

                    if (dr2.Read())
                    {
                        string sfirst = (dr2["Firstname"].ToString());
                        string smid = (dr2["Midname"].ToString());
                        string slast = (dr2["Lastname"].ToString());

                        tBFirst.Text = sfirst;
                        tbMid.Text = smid;
                        tBLast.Text = slast;

                        byte[] img = (byte[])(dr2["studImg"]);
                        if (img == null)
                        {
                            pBImg.Image = null;
                        }

                        else
                        {
                            MemoryStream ms = new MemoryStream(img);
                            pBImg.Image = Image.FromStream(ms);
                        }


                        if (att != "ALL EMPLOYEE" || att == "GENERAL")
                        {
                            studchk.Open();
                            SqlCommand schk = studchk.CreateCommand();
                            schk.CommandType = CommandType.Text;
                            schk.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                                 "and evtDate = '" + date + "' and Id = '" + id + "' " +
                                 "and mornTimeIn != ''";
                            SqlDataReader sdr = schk.ExecuteReader();

                            if (sdr.Read())
                            {
                                vercon.Open();
                                SqlCommand vercmd = vercon.CreateCommand();
                                vercmd.CommandType = CommandType.Text;
                                vercmd.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                                      "and evtDate = '" + date + "' and Id = '" + id + "' " +
                                       "and mornTimeOut = '00:00'";
                                SqlDataReader verDr = vercmd.ExecuteReader();

                                if (verDr.Read())
                                {
                                    scon.Open();
                                    SqlCommand scmd = scon.CreateCommand();
                                    scmd.CommandType = CommandType.Text;
                                    scmd.CommandText = "Update attendTB set mornTimeOut = '" + time + "' " +
                                        "where evtCode = '" + code + "' and evtDate = '" + date + "' " +
                                        "and Id = '" + id + "'";
                                    scmd.ExecuteNonQuery();
                                    scon.Close();

                                    lblStat.Text = "ID RECORDED!";
                                    tBId.Text = "";
                                    sinMornOutTimer.Stop();
                                    FinalFrame.Stop();
                                    pBQR.Image = Properties.Resources.abc;
                                    sinMornOut();
                                }
                                else
                                {
                                    lblStat.Text = "ID ALREADY LOGGED OUT!";
                                    tBId.Text = "";
                                    sinMornOutTimer.Stop();
                                    FinalFrame.Stop();
                                    pBQR.Image = Properties.Resources.abc;
                                    sinMornOut();

                                }
                                vercon.Close();

                            }
                            else
                            {
                                lblStat.Text = "ID NOT LOGGED IN!";
                                tBId.Text = "";
                                sinMornOutTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                sinMornOut();
                            }
                            studchk.Close();
                        }
                        else
                        {
                            lblStat.Text = "You are not allowed to logout!";
                            tBId.Text = "";
                            sinMornOutTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            sinMornOut();
                        }
                    }
                    else
                    {
                        lblStat.Text = "ID NOT FOUND!";
                        tBId.Text = "";
                        sinMornOutTimer.Stop();
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.abc;
                        sinMornOut();
                    }
                    studcon.Close();
                }
                empcon.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        public void snLogEveIn()
        {
            SqlConnection empcon = new SqlConnection(empcs);
            SqlConnection studcon = new SqlConnection(empcs);
            SqlConnection empchk = new SqlConnection(empcs);
            SqlConnection studchk = new SqlConnection(empcs);
            SqlConnection empIns = new SqlConnection(empcs);
            SqlConnection studIns = new SqlConnection(empcs);

            string id = tBId.Text;
            string code = lblEvtCode.Text;
            string att = tBEvtAtt.Text;
            string date = DateTime.Now.ToShortDateString();
            string time = DateTime.Now.ToShortTimeString();

            string mStat = "---";
            string aStat = "---";
            string eStat = "---";

            try
            {
                empcon.Open();
                SqlCommand cmd = empcon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from empTB where empId = '" + id + "'";
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    string efirst = (dr["Firstname"].ToString());
                    string emid = (dr["Midname"].ToString());
                    string elast = (dr["Lastname"].ToString());
                    string edept = (dr["Dept"].ToString());
                    string ecourse = "NA";
                    string eyear = "NA";
                    string etype = "EMPLOYEE";
                    string emornOut = "00:00";
                    string emornIn = "00:00";
                    string eaftOut = "00:00";
                    string eaftIn = "00:00";
                    string eeveOut = "00:00";
                    string etotHrs = "0";

                    tBFirst.Text = efirst;
                    tbMid.Text = emid;
                    tBLast.Text = elast;

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

                    empchk.Open();
                    SqlCommand chkcmd = empchk.CreateCommand();
                    chkcmd.CommandType = CommandType.Text;
                    chkcmd.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                        "and evtDate = '" + date + "' and Id = '" + id + "'";
                    SqlDataReader empDr = chkcmd.ExecuteReader();

                    if (empDr.Read())
                    {
                        if (att == "ALL EMPLOYEES" || att == "GENERAL")
                        {
                            lblStat.Text = "ID ALREADY LOGGED IN!";
                            tBId.Text = "";
                            sinEveInTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            sinEveIn();

                        }
                        else
                        {
                            lblStat.Text = "You are not allowed to login!";
                            tBId.Text = "";
                            sinEveInTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            sinEveIn();
                        }
                    }
                    else
                    {
                        if (att == "ALL EMPLOYEES" || att == "GENERAL")
                        {
                            empIns.Open();
                            SqlCommand eins = empIns.CreateCommand();
                            eins.CommandType = CommandType.Text;
                            eins.CommandText = "Insert into attendTB values ('" + code + "', " +
                                "'" + date + "', " +
                                "'" + etype + "', " +
                                "'" + id + "', " +
                                "'" + efirst + "', " +
                                "'" + emid + "', " +
                                "'" + elast + "', " +
                                "'" + edept + "', " +
                                "'" + ecourse + "', " +
                                "'" + eyear + "', " +
                                "'" + emornIn + "', " +
                                "'" + emornOut + "', " +
                                "'" + eaftIn + "', " +
                                "'" + eaftOut + "', " +
                                "'" + time + "', " +
                                "'" + eeveOut + "', " +
                                "'" + mStat + "', " +
                                "'" + aStat + "', " +
                                "'" + eStat + "', " +
                                "'" + etotHrs + "')";
                            eins.ExecuteNonQuery();
                            empIns.Close();

                            lblStat.Text = "ID RECORDED!";
                            tBId.Text = "";
                            sinEveInTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            sinEveIn();
                        }
                        else
                        {
                            lblStat.Text = "You are not allowed to login!";
                            tBId.Text = "";
                            sinEveInTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            sinEveIn();
                        }
                    }
                    empchk.Close();
                }
                else
                {
                    studcon.Open();
                    SqlCommand cmd2 = studcon.CreateCommand();
                    cmd2.CommandType = CommandType.Text;
                    cmd2.CommandText = "Select * from studTB where studId = '" + id + "'";
                    SqlDataReader dr2 = cmd2.ExecuteReader();

                    if (dr2.Read())
                    {
                        string sfirst = (dr2["Firstname"].ToString());
                        string smid = (dr2["Midname"].ToString());
                        string slast = (dr2["Lastname"].ToString());
                        string syear = (dr2["Year"].ToString());
                        string sdept = (dr2["Dept"].ToString());
                        string scourse = (dr2["Course"].ToString());
                        string stype = "STUDENT";
                        string smornOut = "00:00";
                        string smornIn = "00:00";
                        string saftOut = "00:00";
                        string saftIn = "00:00";
                        string seveOut = "00:00";
                        string stotHrs = "0";

                        tBFirst.Text = sfirst;
                        tbMid.Text = smid;
                        tBLast.Text = slast;

                        byte[] img = (byte[])(dr2["studImg"]);
                        if (img == null)
                        {
                            pBImg.Image = null;
                        }

                        else
                        {
                            MemoryStream ms = new MemoryStream(img);
                            pBImg.Image = Image.FromStream(ms);
                        }

                        studchk.Open();
                        SqlCommand scmd = studchk.CreateCommand();
                        scmd.CommandType = CommandType.Text;
                        scmd.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                        "and evtDate = '" + date + "' and Id = '" + id + "'";
                        SqlDataReader sdr = scmd.ExecuteReader();

                        if (sdr.Read())
                        {
                            if (att != "ALL EMPLOYEES" || att != "GENERAL")
                            {
                                lblStat.Text = "ID ALREADY LOGGED IN!";
                                tBId.Text = "";
                                sinEveInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                sinEveIn();
                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to login!";
                                tBId.Text = "";
                                sinEveInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                sinEveIn();
                            }
                        }
                        else
                        {
                            if (att != "ALL EMPLOYEES" || att != "GENERAL")
                            {
                                studIns.Open();
                                SqlCommand sins = studIns.CreateCommand();
                                sins.CommandType = CommandType.Text;
                                sins.CommandText = "Insert into attendTB values ('" + code + "', " +
                                 "'" + date + "', " +
                                 "'" + stype + "', " +
                                 "'" + id + "', " +
                                 "'" + sfirst + "', " +
                                 "'" + smid + "', " +
                                 "'" + slast + "', " +
                                 "'" + sdept + "', " +
                                 "'" + scourse + "', " +
                                 "'" + syear + "', " +
                                 "'" + smornIn + "', " +
                                 "'" + smornOut + "', " +
                                 "'" + saftIn + "', " +
                                 "'" + saftOut + "', " +
                                 "'" + time + "', " +
                                 "'" + seveOut + "', " +
                                 "'" + mStat + "', " +
                                 "'" + aStat + "', " +
                                 "'" + eStat + "', " +
                                 "'" + stotHrs + "')";
                                sins.ExecuteNonQuery();
                                studIns.Close();

                                lblStat.Text = "ID RECORDED!";
                                tBId.Text = "";
                                sinEveInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                sinEveIn();

                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to login!";
                                tBId.Text = "";
                                sinEveInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                sinEveIn();
                            }
                        }
                        studchk.Close();
                    }
                    else
                    {
                        lblStat.Text = "ID NOT FOUND!";
                        tBId.Text = "";
                        sinEveInTimer.Stop();
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.abc;
                        sinEveIn();
                    }
                    studcon.Close();
                }
                empcon.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        public void snLogAftIn()
        {
            SqlConnection empcon = new SqlConnection(empcs);
            SqlConnection studcon = new SqlConnection(empcs);
            SqlConnection empchk = new SqlConnection(empcs);
            SqlConnection studchk = new SqlConnection(empcs);
            SqlConnection empIns = new SqlConnection(empcs);
            SqlConnection studIns = new SqlConnection(empcs);

            string id = tBId.Text;
            string code = lblEvtCode.Text;
            string att = tBEvtAtt.Text;
            string date = DateTime.Now.ToShortDateString();
            string time = DateTime.Now.ToShortTimeString();

            string mStat = "---";
            string aStat = "---";
            string eStat = "---";

            try
            {
                empcon.Open();
                SqlCommand cmd = empcon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from empTB where empId = '" + id + "'";
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    string efirst = (dr["Firstname"].ToString());
                    string emid = (dr["Midname"].ToString());
                    string elast = (dr["Lastname"].ToString());
                    string edept = (dr["Dept"].ToString());
                    string ecourse = "NA";
                    string eyear = "NA";
                    string etype = "EMPLOYEE";
                    string emornOut = "00:00";
                    string emornIn = "00:00";
                    string eaftOut = "00:00";
                    string eeveIn = "00:00";
                    string eeveOut = "00:00";
                    string etotHrs = "0";

                    tBFirst.Text = efirst;
                    tbMid.Text = emid;
                    tBLast.Text = elast;

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

                    empchk.Open();
                    SqlCommand chkcmd = empchk.CreateCommand();
                    chkcmd.CommandType = CommandType.Text;
                    chkcmd.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                        "and evtDate = '" + date + "' and Id = '" + id + "'";
                    SqlDataReader empDr = chkcmd.ExecuteReader();

                    if (empDr.Read())
                    {
                        if (att == "ALL EMPLOYEES" || att == "GENERAL")
                        {
                            lblStat.Text = "ID ALREADY LOGGED IN!";
                            tBId.Text = "";
                            sinAftInTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            sinAftIn();

                        }
                        else
                        {
                            lblStat.Text = "You are not allowed to login!";
                            tBId.Text = "";
                            sinAftInTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            sinAftIn();
                        }
                    }
                    else
                    {
                        if (att == "ALL EMPLOYEES" || att == "GENERAL")
                        {
                            empIns.Open();
                            SqlCommand eins = empIns.CreateCommand();
                            eins.CommandType = CommandType.Text;
                            eins.CommandText = "Insert into attendTB values ('" + code + "', " +
                                "'" + date + "', " +
                                "'" + etype + "', " +
                                "'" + id + "', " +
                                "'" + efirst + "', " +
                                "'" + emid + "', " +
                                "'" + elast + "', " +
                                "'" + edept + "', " +
                                "'" + ecourse + "', " +
                                "'" + eyear + "', " +
                                "'" + emornIn + "', " +
                                "'" + emornOut + "', " +
                                "'" + time + "', " +
                                "'" + eaftOut + "', " +
                                "'" + eeveIn + "', " +
                                "'" + eeveOut + "', " +
                                "'" + mStat + "', " +
                                "'" + aStat + "', " +
                                "'" + eStat + "', " +
                                "'" + etotHrs + "')";
                            eins.ExecuteNonQuery();
                            empIns.Close();

                            lblStat.Text = "ID RECORDED!";
                            tBId.Text = "";
                            sinAftInTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            sinAftIn();
                        }
                        else
                        {
                            lblStat.Text = "You are not allowed to login!";
                            tBId.Text = "";
                            sinAftInTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            sinAftIn();
                        }
                    }
                    empchk.Close();
                }
                else
                {
                    studcon.Open();
                    SqlCommand cmd2 = studcon.CreateCommand();
                    cmd2.CommandType = CommandType.Text;
                    cmd2.CommandText = "Select * from studTB where studId = '" + id + "'";
                    SqlDataReader dr2 = cmd2.ExecuteReader();

                    if (dr2.Read())
                    {
                        string sfirst = (dr2["Firstname"].ToString());
                        string smid = (dr2["Midname"].ToString());
                        string slast = (dr2["Lastname"].ToString());
                        string syear = (dr2["Year"].ToString());
                        string sdept = (dr2["Dept"].ToString());
                        string scourse = (dr2["Course"].ToString());
                        string stype = "STUDENT";
                        string smornOut = "00:00";
                        string smornIn = "00:00";
                        string saftOut = "00:00";
                        string seveIn = "00:00";
                        string seveOut = "00:00";
                        string stotHrs = "0";

                        tBFirst.Text = sfirst;
                        tbMid.Text = smid;
                        tBLast.Text = slast;

                        byte[] img = (byte[])(dr2["studImg"]);
                        if (img == null)
                        {
                            pBImg.Image = null;
                        }

                        else
                        {
                            MemoryStream ms = new MemoryStream(img);
                            pBImg.Image = Image.FromStream(ms);
                        }

                        studchk.Open();
                        SqlCommand scmd = studchk.CreateCommand();
                        scmd.CommandType = CommandType.Text;
                        scmd.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                        "and evtDate = '" + date + "' and Id = '" + id + "'";
                        SqlDataReader sdr = scmd.ExecuteReader();

                        if (sdr.Read())
                        {
                            if (att != "ALL EMPLOYEES" || att != "GENERAL")
                            {
                                lblStat.Text = "ID ALREADY LOGGED IN!";
                                tBId.Text = "";
                                sinAftInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                sinAftIn();
                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to login!";
                                tBId.Text = "";
                                sinAftInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                sinAftIn();
                            }
                        }
                        else
                        {
                            if (att != "ALL EMPLOYEES" || att != "GENERAL")
                            {
                                studIns.Open();
                                SqlCommand sins = studIns.CreateCommand();
                                sins.CommandType = CommandType.Text;
                                sins.CommandText = "Insert into attendTB values ('" + code + "', " +
                                 "'" + date + "', " +
                                 "'" + stype + "', " +
                                 "'" + id + "', " +
                                 "'" + sfirst + "', " +
                                 "'" + smid + "', " +
                                 "'" + slast + "', " +
                                 "'" + sdept + "', " +
                                 "'" + scourse + "', " +
                                 "'" + syear + "', " +
                                 "'" + smornIn + "', " +
                                 "'" + smornOut + "', " +
                                 "'" + time + "', " +
                                 "'" + saftOut + "', " +
                                 "'" + seveIn + "', " +
                                 "'" + seveOut + "', " +
                                 "'" + mStat + "', " +
                                 "'" + aStat + "', " +
                                 "'" + eStat + "', " +
                                 "'" + stotHrs + "')";
                                sins.ExecuteNonQuery();
                                studIns.Close();

                                lblStat.Text = "ID RECORDED!";
                                tBId.Text = "";
                                sinAftInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                sinAftIn();

                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to login!";
                                tBId.Text = "";
                                sinAftInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                sinAftIn();
                            }
                        }
                        studchk.Close();
                    }
                    else
                    {
                        lblStat.Text = "ID NOT FOUND!";
                        tBId.Text = "";
                        sinAftInTimer.Stop();
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.abc;
                        sinAftIn();
                    }
                    studcon.Close();
                }
                empcon.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        public void snLogMornIn()
        {
            SqlConnection empcon = new SqlConnection(empcs);
            SqlConnection studcon = new SqlConnection(empcs);
            SqlConnection empchk = new SqlConnection(empcs);
            SqlConnection studchk = new SqlConnection(empcs);
            SqlConnection empIns = new SqlConnection(empcs);
            SqlConnection studIns = new SqlConnection(empcs);

            string id = tBId.Text;
            string code = lblEvtCode.Text;
            string att = tBEvtAtt.Text;
            string date = DateTime.Now.ToShortDateString();
            string time = DateTime.Now.ToShortTimeString();

            string mStat = "---";
            string aStat = "---";
            string eStat = "---";

            try
            {
                empcon.Open();
                SqlCommand cmd = empcon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from empTB where empId = '" + id + "'";
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    string efirst = (dr["Firstname"].ToString());
                    string emid = (dr["Midname"].ToString());
                    string elast = (dr["Lastname"].ToString());
                    string edept = (dr["Dept"].ToString());
                    string ecourse = "NA";
                    string eyear = "NA";
                    string etype = "EMPLOYEE";
                    string emornOut = "00:00";
                    string eaftIn = "00:00";
                    string eaftOut = "00:00";
                    string eeveIn = "00:00";
                    string eeveOut = "00:00";
                    string etotHrs = "0";

                    tBFirst.Text = efirst;
                    tbMid.Text = emid;
                    tBLast.Text = elast;

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

                    empchk.Open();
                    SqlCommand chkcmd = empchk.CreateCommand();
                    chkcmd.CommandType = CommandType.Text;
                    chkcmd.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                        "and evtDate = '" + date + "' and Id = '" + id + "'";
                    SqlDataReader empDr = chkcmd.ExecuteReader();

                    if (empDr.Read())
                    {
                        if (att == "ALL EMPLOYEES" || att == "GENERAL")
                        {
                            lblStat.Text = "ID ALREADY LOGGED IN!";
                            tBId.Text = "";
                            sinMornInTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            sinMornIn();

                        }
                        else
                        {
                            lblStat.Text = "You are not allowed to login!";
                            tBId.Text = "";
                            sinMornInTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            sinMornIn();
                        }
                    }
                    else
                    {
                        if (att == "ALL EMPLOYEES" || att == "GENERAL")
                        {
                            empIns.Open();
                            SqlCommand eins = empIns.CreateCommand();
                            eins.CommandType = CommandType.Text;
                            eins.CommandText = "Insert into attendTB values ('" + code + "', " +
                                "'" + date + "', " +
                                "'" + etype + "', " +
                                "'" + id + "', " +
                                "'" + efirst + "', " +
                                "'" + emid + "', " +
                                "'" + elast + "', " +
                                "'" + edept + "', " +
                                "'" + ecourse + "', " +
                                "'" + eyear + "', " +
                                "'" + time + "', " +
                                "'" + emornOut + "', " +
                                "'" + eaftIn + "', " +
                                "'" + eaftOut + "', " +
                                "'" + eeveIn + "', " +
                                "'" + eeveOut + "', " +
                                "'" + mStat + "', " +
                                "'" + aStat + "', " +
                                "'" + eStat + "', " +
                                "'" + etotHrs + "')";
                            eins.ExecuteNonQuery();
                            empIns.Close();

                            lblStat.Text = "ID RECORDED!";
                            tBId.Text = "";
                            sinMornInTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            sinMornIn();
                        }
                        else
                        {
                            lblStat.Text = "You are not allowed to login!";
                            tBId.Text = "";
                            sinMornInTimer.Stop();
                            FinalFrame.Stop();
                            pBQR.Image = Properties.Resources.abc;
                            sinMornIn();
                        }
                    }
                    empchk.Close();
                }
                else
                {
                    studcon.Open();
                    SqlCommand cmd2 = studcon.CreateCommand();
                    cmd2.CommandType = CommandType.Text;
                    cmd2.CommandText = "Select * from studTB where studId = '" + id + "'";
                    SqlDataReader dr2 = cmd2.ExecuteReader();

                    if (dr2.Read())
                    {
                        string sfirst = (dr2["Firstname"].ToString());
                        string smid = (dr2["Midname"].ToString());
                        string slast = (dr2["Lastname"].ToString());
                        string syear = (dr2["Year"].ToString());
                        string sdept = (dr2["Dept"].ToString());
                        string scourse = (dr2["Course"].ToString());
                        string stype = "STUDENT";
                        string smornOut = "00:00";
                        string saftIn = "00:00";
                        string saftOut = "00:00";
                        string seveIn = "00:00";
                        string seveOut = "00:00";
                        string stotHrs = "0";

                        tBFirst.Text = sfirst;
                        tbMid.Text = smid;
                        tBLast.Text = slast;

                        byte[] img = (byte[])(dr2["studImg"]);
                        if (img == null)
                        {
                            pBImg.Image = null;
                        }

                        else
                        {
                            MemoryStream ms = new MemoryStream(img);
                            pBImg.Image = Image.FromStream(ms);
                        }

                        studchk.Open();
                        SqlCommand scmd = studchk.CreateCommand();
                        scmd.CommandType = CommandType.Text;
                        scmd.CommandText = "Select * from attendTB where evtCode = '" + code + "' " +
                        "and evtDate = '" + date + "' and Id = '" + id + "'";
                        SqlDataReader sdr = scmd.ExecuteReader();

                        if (sdr.Read())
                        {
                            if (att != "ALL EMPLOYEES" || att != "GENERAL")
                            {
                                lblStat.Text = "ID ALREADY LOGGED IN!";
                                tBId.Text = "";
                                sinMornInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                sinMornIn();
                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to login!";
                                tBId.Text = "";
                                sinMornInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                sinMornIn();
                            }
                        }
                        else
                        {
                            if (att != "ALL EMPLOYEES" || att != "GENERAL")
                            {
                                studIns.Open();
                                SqlCommand sins = studIns.CreateCommand();
                                sins.CommandType = CommandType.Text;
                                sins.CommandText = "Insert into attendTB values ('" + code + "', " +
                                 "'" + date + "', " +
                                 "'" + stype + "', " +
                                 "'" + id + "', " +
                                 "'" + sfirst + "', " +
                                 "'" + smid + "', " +
                                 "'" + slast + "', " +
                                 "'" + sdept + "', " +
                                 "'" + scourse + "', " +
                                 "'" + syear + "', " +
                                 "'" + time + "', " +
                                 "'" + smornOut + "', " +
                                 "'" + saftIn + "', " +
                                 "'" + saftOut + "', " +
                                 "'" + seveIn + "', " +
                                 "'" + seveOut + "', " +
                                 "'" + mStat + "', " +
                                 "'" + aStat + "', " +
                                 "'" + eStat + "', " +
                                 "'" + stotHrs + "')";
                                sins.ExecuteNonQuery();
                                studIns.Close();

                                lblStat.Text = "ID RECORDED!";
                                tBId.Text = "";
                                sinMornInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                sinMornIn();

                            }
                            else
                            {
                                lblStat.Text = "You are not allowed to login!";
                                tBId.Text = "";
                                sinMornInTimer.Stop();
                                FinalFrame.Stop();
                                pBQR.Image = Properties.Resources.abc;
                                sinMornIn();
                            }
                        }
                        studchk.Close();
                    }
                    else
                    {
                        lblStat.Text = "ID NOT FOUND!";
                        tBId.Text = "";
                        sinMornInTimer.Stop();
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.abc;
                        sinMornIn();
                    }
                    studcon.Close();
                }
                empcon.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Misc
        private void btnMornIn_Click(object sender, EventArgs e)
        {
            try
            {
                if (lblEvtCode.Text != "0000000")
                {
                    if (tBMornIn.Text != "NONE" && tBAftIn.Text != "NONE" && tBEveIn.Text != "NONE")
                    {
                        lblStat.Text = "Starting...";
                        string ntime = DateTime.Now.ToShortTimeString();
                        DateTime mIn = DateTime.Parse(tBMornIn.Text);
                        DateTime mOut = DateTime.Parse(tBMornOut.Text);
                        DateTime time = DateTime.Parse(ntime);

                        if (time >= mIn && time < mOut)
                        {
                            FinalFrame.Stop();
                            morningIn();
                        }
                        else
                        {
                            lblStat.Text = "TIME NOT MEET!";
                        }
                    }
                    else if (tBMornIn.Text != "NONE" && tBAftIn.Text == "NONE" && tBEveIn.Text == "NONE")
                    {
                        lblStat.Text = "Starting...";
                        string ntime = DateTime.Now.ToShortTimeString();
                        DateTime mIn = DateTime.Parse(tBMornIn.Text);
                        DateTime mOut = DateTime.Parse(tBMornOut.Text);
                        DateTime time = DateTime.Parse(ntime);

                        if (time >= mIn && time < mOut)
                        {
                            //FinalFrame.Stop();
                            sinMornIn();
                        }
                        else
                        {
                            lblStat.Text = "TIME NOT MEET!";
                        }
                    }
                    else
                    {
                        lblStat.Text = "ERROR!";
                    }
                }
                else
                {
                    lblStat.Text = "EVENT EMPTY!";
                }
            }
            catch (Exception f)
            {
                lblStat.Text = "ERROR!";
            }

        }
        public void wrapPass()
        {
            SqlConnection vercon = new SqlConnection(empcs);
            string pass = tBWPass.Text;
            string user = tBUser.Text;

            try
            {
                if (pass == "")
                {
                    tBWPass.Visible = false;
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
                        if (MessageBox.Show("Are you sure to wrap the attendance?", " Verify", 
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            wrap();
                        }
                        else
                        {
                            tBWPass.Visible = false;
                            tBWPass.Text = "";
                        }
                    }
                    else
                    {
                        tBWPass.Visible = false;
                        tBWPass.Text = "";
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
        public void capCam()
        {
            try
            {
                tBId.Text = "";
                FinalFrame = new VideoCaptureDevice(filterInfoCollection[cBCamera.SelectedIndex].MonikerString);
                FinalFrame.NewFrame += new NewFrameEventHandler(FinalFrame_NewFrame);
                FinalFrame.Start();
            }
            catch (Exception e)
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
                    if (tBEveIn.Text != "NONE" && tBMornIn.Text == "NONE" && tBAftIn.Text == "NONE")
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
                cmd.CommandText = "Select * from eventTB where eventDate = '" + date + "' order by evtCode DESC";
                SqlDataReader dr = cmd.ExecuteReader();
                string att = "Regular Employee LogIn";

                while (dr.Read())
                {
                    string name = (dr["eventName"].ToString());
                   

                    if (!cBEvt.Items.Contains(name))
                    {
                        cBEvt.Items.Add(name);
                    }
                }
                if (!cBEvt.Items.Contains(att))
                {
                    cBEvt.Items.Add(att);
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
            btnMornOut.Enabled = false;
            btnAftOut.Enabled = false;
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
            tBWPass.Visible = false;
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
            string time = DateTime.Now.ToShortTimeString();

            try
            {

                string mOut = tBMornOut.Text;
                string aOut = tBAftOut.Text;
                string eOut = tBEveOut.Text;

                string mIn = tBMornIn.Text;
                string aIn = tBAftIn.Text;
                string eIn = tBAftIn.Text;


                if (lblEvtCode.Text != "0000000")
                {
                    if (time == mOut)
                    {
                        btnMornIn.Enabled = false;
                        btnMornOut.Enabled = true;
                    }
                    if (time == aOut)
                    {
                        btnMornIn.Enabled = false;
                        btnMornOut.Enabled = false;
                        btnAftIn.Enabled = false;
                        btnAftOut.Enabled = true;
                    }
                    if (time == eOut)
                    {
                        btnMornIn.Enabled = false;
                        btnMornOut.Enabled = false;
                        btnAftIn.Enabled = false;
                        btnAftOut.Enabled = false;
                        btnEveIn.Enabled = false;
                        btnEveOut.Enabled = true;
                    }
                    //// Morning Out
                    //if (time == mOut && mOut != "NONE" && aIn != "NONE" && eIn != "NONE")
                    //{
                    //    btnMornIn.Enabled = false;
                    //    btnMornOut.Enabled = true;
                    //}
                    //if (time == mOut && mOut != "NONE" && aIn == "NONE" && eIn == "NONE")
                    //{
                    //    btnMornIn.Enabled = false;
                    //    btnMornOut.Enabled = true;
                    //}
                    //// Afternoon Out
                    //if (time == aOut && aOut != "NONE" && mIn != "NONE" && aIn != "NONE" && eIn != "NONE")
                    //{
                    //    btnMornIn.Enabled = false;
                    //    btnMornOut.Enabled = false;
                    //    btnAftIn.Enabled = false;
                    //    btnAftOut.Enabled = true;
                    //}
                    //if (time == aOut && aOut != "NONE" && mIn == "NONE"  && eIn == "NONE" && aIn != "NONE")
                    //{
                    //    btnMornIn.Enabled = false;
                    //    btnMornOut.Enabled = false;
                    //    btnAftIn.Enabled = false;
                    //    btnAftOut.Enabled = true;
                    //}
                    //// Evening Out
                    //if (time == eOut && eOut != "NONE" && mIn != "NONE" && aIn != "NONE" && eIn != "NONE")
                    //{
                    //    btnMornIn.Enabled = false;
                    //    btnMornOut.Enabled = false;
                    //    btnAftIn.Enabled = false;
                    //    btnAftOut.Enabled = false;
                    //    btnEveIn.Enabled = false;
                    //    btnEveOut.Enabled = true;
                    //}
                }
            }
            catch (Exception f)
            {
                lblStat.Text = f.Message;
            }
        }
        private void cBEvt_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cBEvt.Text != null)
            {
                if (cBEvt.Text == "Regular Employee LogIn")
                {
                    lblEvtCode.Text = "REG0001";
                    tBEvtAtt.Text = "ALL EMPLOYEES";
                    tBMornIn.Text = "8:00 AM";
                    tBMornOut.Text = "12:00 PM";
                    tBAftIn.Text = "1:00 PM";
                    tBAftOut.Text = "5:00 PM";
                    tBEveIn.Text = "6:00 PM";
                    tBEveOut.Text = "9:00 PM";
                    disbutt();
                    btnMornIn.Enabled = true;
                }
                else
                {
                    cbEvt();
                }
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
            if (btnMornOut.Enabled == true ||
                btnAftIn.Enabled == true ||
                btnEveOut.Enabled == true)
            {
                tBWPass.Visible = true;
                tBWPass.Focus();
            }
        }
        private void tBWPass_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                wrapPass();
            }
        }
        #endregion

        private void sinMornInTimer_Tick(object sender, EventArgs e)
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
                        sinMornInTimer.Stop();
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.abc;
                        snLogMornIn();

                    }
                    else
                    {
                        lblStat.Text = "Verifying...";

                    }
                }

            }
            catch (Exception f)
            {
                MessageBox.Show(f.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void sinMornOutTimer_Tick(object sender, EventArgs e)
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
                        sinMornOutTimer.Stop();
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.abc;
                        snLogMornOut();

                    }
                    else
                    {
                        lblStat.Text = "Verifying...";

                    }
                }

            }
            catch (Exception f)
            {
                MessageBox.Show(f.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void sinAftInTimer_Tick(object sender, EventArgs e)
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
                        sinAftInTimer.Stop();
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.abc;
                        snLogAftIn();
                    }
                    else
                    {
                        lblStat.Text = "Verifying...";

                    }
                }

            }
            catch (Exception f)
            {
                MessageBox.Show(f.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void sinAftOutTimer_Tick(object sender, EventArgs e)
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
                        sinAftOutTimer.Stop();
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.abc;
                        snLogAftOut();

                    }
                    else
                    {
                        lblStat.Text = "Verifying...";

                    }
                }

            }
            catch (Exception f)
            {
                MessageBox.Show(f.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void sinEveInTimer_Tick(object sender, EventArgs e)
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
                        sinEveInTimer.Stop();
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.abc;
                        snLogEveIn();
                    }
                    else
                    {
                        lblStat.Text = "Verifying...";

                    }
                }

            }
            catch (Exception f)
            {
                MessageBox.Show(f.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void sinEveOutTimer_Tick(object sender, EventArgs e)
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
                        sinEveOutTimer.Stop();
                        FinalFrame.Stop();
                        pBQR.Image = Properties.Resources.abc;
                        snLogEveOut();

                    }
                    else
                    {
                        lblStat.Text = "Verifying...";

                    }
                }

            }
            catch (Exception f)
            {
                MessageBox.Show(f.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
