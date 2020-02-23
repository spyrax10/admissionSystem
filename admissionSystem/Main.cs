using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using QRCoder;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace admissionSystem
{
    public partial class Main : Form
    {
        string empcs = @"Data Source=LOCALHOST192\SQL2019;Initial Catalog=facialDB;Integrated Security=True";

        string imgLoc = "";
        SqlDataAdapter adapt;

        string loadId = Screen.id;
        
        public Main()
        {
            InitializeComponent();
            paneSide.Height = btnHome.Height;
            paneSide.Top = btnHome.Top;
        }
        public void upSet()
        {
            SqlConnection con = new SqlConnection(empcs);
            SqlConnection chk = new SqlConnection(empcs);

            string user = tbUser.Text;
            string pass = tbPass.Text;
            string id = lblId.Text;

            try
            {
                if (tbUser.Text == "" || tbPass.Text == "")
                {
                    MessageBox.Show("Missing Fields!", " Incomplete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    chk.Open();
                    SqlCommand chkcmd = chk.CreateCommand();
                    chkcmd.CommandType = CommandType.Text;
                    chkcmd.CommandText = "Select * from setTB where username = '" + user + "' "+
                        "and empId != '" + id + "'";
                    SqlDataReader dr = chkcmd.ExecuteReader();

                    if (dr.Read())
                    {
                        MessageBox.Show("Username already taken!", " Invalid", MessageBoxButtons.OK, MessageBoxIcon.Warning);            
                    }
                    else
                    {
                        con.Open();
                        SqlCommand cmd = con.CreateCommand();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "Update setTB set username = '" + user + "', " +
                            "password = '" + pass + "' where empId = '" + id + "'";
                        cmd.ExecuteNonQuery();
                        con.Close();
                        paneSet.Visible = false;
                        MessageBox.Show("Successfully Updated!", " Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                    }
                    chk.Close();
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void delEvt()
        {
            SqlConnection delcon = new SqlConnection(empcs);

            string code = gVEvent.CurrentRow.Cells[0].Value.ToString();
            try
            {
                delcon.Open();
                SqlCommand cmd = delcon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Delete from eventTB where evtCode = '" + code + "'";
                cmd.ExecuteNonQuery();
                delcon.Close();
                clrEvt();
                dispEvt();
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void cbMainEvt()
        {
            SqlConnection con = new SqlConnection(empcs);
            string name = cBEvt.Text;
            
            try
            {

                con.Open();
                SqlCommand cmd = con.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from eventTB where eventName = '" + name + "'";
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    tBAtt.Text = (dr["Attendee"].ToString());
                    lblCode.Text = (dr["evtCode"].ToString());     
                }
                con.Close();
                
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void loadEvt()
        {
            SqlConnection con = new SqlConnection(empcs);
            try
            {
                con.Open();
                SqlCommand cmd = con.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from eventTB";
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    string evtName = (dr["eventName"].ToString());
                    string att = "Regular Employee LogIn";

                    if (!cBEvt.Items.Contains(evtName))
                    {
                        cBEvt.Items.Add(evtName);
                    }
                    if (!cBEvt.Items.Contains(att))
                    {
                        cBEvt.Items.Add(att);
                    }
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void evtLog()
        {
            SqlConnection con = new SqlConnection(empcs);

            string code = lblCode.Text;
            try
            {
                con.Open();
                DataTable dt = new DataTable();
                adapt = new SqlDataAdapter("Select * ", con);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void clrEvt()
        {
            foreach(Control c in gBEvent.Controls)
            {
                if (c is TextBox)
                {
                    c.Text = "";
                }
                if (c is DateTimePicker)
                {
                    c.Text = DateTime.Now.ToShortDateString();
                }
                if (c is ComboBox)
                {
                    c.Text = null;
                    c.Text = "";
                }
            }
            btnEvtAdd.Text = "ADD";
        }
        public void clrStud()
        {
            foreach (Control c in gBStud.Controls)
            {
                if (c is TextBox)
                {
                    c.Text = "";
                }
                if (c is ComboBox)
                {
                    c.Text = null;
                }
            }
            btnStudPH.Text = "UPLOAD";
            btnStudAdd.Text = "ADD";
            dispStud();
            pBStud.Image = Properties.Resources.stud;
            pBStudQR.Image = null;
            lblRegStud.Text = "00000";
        }
        public void clrEmp()
        {
            foreach (Control c in gBEmp.Controls)
            {
                if (c is TextBox)
                {
                    c.Text = "";
                }
                if (c is ComboBox)
                {
                    c.Text = null;
                }
            }
            btnEmpPH.Text = "UPLOAD";
            btnEmpAdd.Text = "ADD";
            dispEmp();
            pBEmp.Image = Properties.Resources.download;
            pBEmpQR.Image = null;
            lblRegEmp.Text = "00000";
        }
        public void empQR()
        {
            try
            {
                QRCodeGenerator qr = new QRCodeGenerator();
                QRCodeData data = qr.CreateQrCode(tBEmpId.Text, QRCodeGenerator.ECCLevel.Q);
                QRCode code = new QRCode(data);
                pBEmpQR.Image = code.GetGraphic(5);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void loadCre()
        {
            SqlConnection  loadcon = new SqlConnection(empcs);

            string user = Screen.userId;
            try
            {
                loadcon.Open();
                SqlCommand cmd = loadcon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from setTB where username = '" + user + "'";
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    tbUser.Text = (dr["username"].ToString());
                    tbPass.Text = (dr["password"].ToString());
                    lblId.Text = (dr["empId"].ToString());
                }
                loadcon.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void studQR()
        {
            try
            {
                QRCodeGenerator qr = new QRCodeGenerator();
                QRCodeData data = qr.CreateQrCode(tBStudId.Text, QRCodeGenerator.ECCLevel.Q);
                QRCode code = new QRCode(data);
                pBStudQR.Image = code.GetGraphic(5);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
        public void upStudPH()
        {
            SqlConnection phcon = new SqlConnection(empcs);
            SqlConnection phcon2 = new SqlConnection(empcs);

            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.tif;...";
                dlg.Title = "Select a Photo";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    imgLoc = dlg.FileName.ToString();
                    pBStud.ImageLocation = imgLoc;

                    if (MessageBox.Show("Save?",
                    " Verify",
                     MessageBoxButtons.YesNo,
                     MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        byte[] img = null;
                        FileStream fs = new FileStream(imgLoc, FileMode.Open, FileAccess.Read);
                        BinaryReader br = new BinaryReader(fs);
                        img = br.ReadBytes((int)fs.Length);

                        phcon.Open();
                        SqlCommand cmd = phcon.CreateCommand();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "UPDATE studTB SET studImg=@img WHERE studId ='" + tBStudId.Text + "'";
                        cmd.Parameters.Add(new SqlParameter("@img", img));
                        cmd.ExecuteNonQuery();
                        phcon.Close();
                        MessageBox.Show("Done!", " Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        phcon2.Open();

                        SqlCommand cmd2 = phcon2.CreateCommand();
                        cmd2.CommandType = CommandType.Text;
                        cmd2.CommandText = "Select * from studTB where studId = '" + tBStudId.Text + "'";

                        SqlDataReader dr = cmd2.ExecuteReader();

                        if (dr.Read())
                        {
                            byte[] img = (byte[])(dr["studImg"]);
                            if (img == null)
                            {
                                pBStud.Image = null;
                            }
                            else
                            {
                                MemoryStream ms = new MemoryStream(img);
                                pBStud.Image = Image.FromStream(ms);
                            }
                        }
                        phcon2.Close();
                    }

                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void upEmpPH()
        {
            SqlConnection phcon = new SqlConnection(empcs);
            SqlConnection phcon2 = new SqlConnection(empcs);

            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.tif;...";
                dlg.Title = "Select a Photo";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    imgLoc = dlg.FileName.ToString();
                    pBEmp.ImageLocation = imgLoc;

                    if (MessageBox.Show("Save?",
                    " Verify",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        byte[] img = null;
                        FileStream fs = new FileStream(imgLoc, FileMode.Open, FileAccess.Read);
                        BinaryReader br = new BinaryReader(fs);
                        img = br.ReadBytes((int)fs.Length);

                        phcon.Open();
                        SqlCommand cmd = phcon.CreateCommand();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "UPDATE empTB SET empImg=@img WHERE empId ='" + tBEmpId.Text + "'";
                        cmd.Parameters.Add(new SqlParameter("@img", img));
                        cmd.ExecuteNonQuery();
                        phcon.Close();
                        MessageBox.Show("Done!", " Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        phcon2.Open();

                        SqlCommand cmd2 = phcon2.CreateCommand();
                        cmd2.CommandType = CommandType.Text;
                        cmd2.CommandText = "Select * from empTB where empId = '" + tBEmpId.Text + "'";

                        SqlDataReader dr = cmd2.ExecuteReader();

                        if (dr.Read())
                        {
                            byte[] img = (byte[])(dr["empImg"]);
                            if (img == null)
                            {
                                pBEmp.Image = null;
                            }
                            else
                            {
                                MemoryStream ms = new MemoryStream(img);
                                pBEmp.Image = Image.FromStream(ms);
                            }
                        }
                        phcon2.Close();
                    }

                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
      
        public void dispEvt()
        {
            SqlConnection evtcon = new SqlConnection(empcs);

            try
            {
                evtcon.Open();
                DataTable dt = new DataTable();
                adapt = new SqlDataAdapter("Select evtCode as Code, " +
                    "eventName as Name, " +
                    "Attendee, " +
                    "eventDate as Date, " +
                    "eventStat as Status from eventTB order by evtCode DESC", evtcon);
                adapt.Fill(dt);
                gVEvent.DataSource = dt;
                evtcon.Close();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void dispStud()
        {
            SqlConnection empcon = new SqlConnection(empcs);

            try
            {
                empcon.Open();
                DataTable dt = new DataTable();
                adapt = new SqlDataAdapter("Select studId as StudId, Lastname, Firstname, Midname, Year, Dept as Department, Course from studTB", empcon);
                adapt.Fill(dt);
                gVStud.DataSource = dt;
                empcon.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void dispEmp()
        {
            SqlConnection empcon = new SqlConnection(empcs);

            try
            {
                empcon.Open();
                DataTable dt = new DataTable();
                adapt = new SqlDataAdapter("Select empId as EmpId, Lastname, Firstname, Midname, Dept as Department, Pos as Position from empTB", empcon);
                adapt.Fill(dt);
                gVEmp.DataSource = dt;
                empcon.Close();
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void cpEvt()
        {
            SqlConnection evtcon = new SqlConnection(empcs);
            string code = gVEvent.CurrentRow.Cells[0].Value.ToString();

            try
            {
                evtcon.Open();
                SqlCommand cmd = evtcon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from eventTB where evtCode = '" + code + "'";
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    tBEventCode.Text = "";
                    tBEventName.Text = (dr["eventName"].ToString());
                    tBEventAtt.Text = (dr["Attendee"].ToString());
                    dTEventDate.Text = (dr["eventDate"].ToString());
                    tBEventMorIn.Text = (dr["MorningIn"].ToString());
                    tBEventMorOut.Text = (dr["MorningOut"].ToString());
                    tBEventAftIn.Text = (dr["AftIn"].ToString());
                    tBEventAftOut.Text = (dr["AftOut"].ToString());
                    tBEventEveIn.Text = (dr["EveIn"].ToString());
                    tBEventEveOut.Text = (dr["EveOut"].ToString());

                    btnEvtAdd.Text = "ADD";
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void cellEvt()
        {
            SqlConnection evtcon = new SqlConnection(empcs);
            string code = gVEvent.CurrentRow.Cells[0].Value.ToString();

            try
            {
                evtcon.Open();
                SqlCommand cmd = evtcon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from eventTB where evtCode = '" + code + "'";
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    tBEventCode.Text = (dr["evtCode"].ToString());
                    tBEventName.Text = (dr["eventName"].ToString());
                    tBEventAtt.Text = (dr["Attendee"].ToString());
                    dTEventDate.Text = (dr["eventDate"].ToString());
                    tBEventMorIn.Text = (dr["MorningIn"].ToString());
                    tBEventMorOut.Text = (dr["MorningOut"].ToString());
                    tBEventAftIn.Text = (dr["AftIn"].ToString());
                    tBEventAftOut.Text = (dr["AftOut"].ToString());
                    tBEventEveIn.Text = (dr["EveIn"].ToString());
                    tBEventEveOut.Text = (dr["EveOut"].ToString());

                    btnEvtAdd.Text = "UPDATE";
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void cellStud()
        {
            SqlConnection empcon = new SqlConnection(empcs);

            string studId = gVStud.CurrentRow.Cells[0].Value.ToString();
            try
            {
                empcon.Open();
                SqlCommand cmd = empcon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from studTB where studId = '" + studId + "' ";
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    lblRegStud.Text = (dr["regCode"].ToString());
                    tBStudId.Text = (dr["studId"].ToString());
                    tBStudFirst.Text = (dr["Firstname"].ToString());
                    tBStudMid.Text = (dr["Midname"].ToString());
                    tBStudLast.Text = (dr["Lastname"].ToString());
                    cbStudSex.Text = (dr["Sex"].ToString());
                    tBStudYr.Text = (dr["Year"].ToString());
                    tBStudDept.Text = (dr["Dept"].ToString());
                    tBStudCour.Text = (dr["Course"].ToString());

                    byte[] img = (byte[])(dr["studImg"]);
                    if (img == null)
                    {
                        pBStud.Image = null;
                    }
                    else
                    {
                        MemoryStream ms = new MemoryStream(img);
                        pBStud.Image = Image.FromStream(ms);
                    }
                    btnStudAdd.Text = "UPDATE";
                    btnStudPH.Text = "UPDATE";
                    studQR();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void cellEmp()
        {
            SqlConnection empcon = new SqlConnection(empcs);

            string empId = gVEmp.CurrentRow.Cells[0].Value.ToString();
            try
            {
                empcon.Open();
                SqlCommand cmd = empcon.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "Select * from empTB where empId = '" + empId + "' ";
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    lblRegEmp.Text = (dr["regCode"].ToString());
                    tBEmpId.Text = (dr["empId"].ToString());
                    tBEmpFirst.Text = (dr["Firstname"].ToString());
                    tBEmpMid.Text = (dr["Midname"].ToString());
                    tBEmpLast.Text = (dr["Lastname"].ToString());
                    tBEmpDept.Text = (dr["Dept"].ToString());
                    tBEmpPos.Text = (dr["Pos"].ToString());

                    byte[] img = (byte[])(dr["empImg"]);
                    if (img == null)
                    {
                        pBEmp.Image = null;
                    }
                    else
                    {
                        MemoryStream ms = new MemoryStream(img);
                        pBEmp.Image = Image.FromStream(ms);
                    }
                    btnEmpAdd.Text = "UPDATE";
                    btnEmpPH.Text = "UPDATE";
                    empQR();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void upEvt()
        {
            SqlConnection upcon = new SqlConnection(empcs);

            string code = tBEventCode.Text;
            string name = tBEventName.Text;
            string att = tBEventAtt.Text;
            string date = dTEventDate.Text;
            string mornIn = tBEventMorIn.Text;
            string mornOut = tBEventMorOut.Text;
            string aftIn = tBEventAftIn.Text;
            string aftOut = tBEventAftOut.Text;
            string eveIn = tBEventEveIn.Text;
            string eveOut = tBEventEveOut.Text;

            try
            {
                if (name == "" || att == "" || date == "" || 
                    mornIn == "" || mornOut == "" || 
                    aftIn == "" || aftOut == "" || eveIn == "" || eveOut == "")
                {
                    MessageBox.Show("Missing Fields!", " Incomplete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                
                else
                {
                    upcon.Open();
                    SqlCommand cmd = upcon.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "Update eventTB SET eventName = '" + name + "', " +
                        "Attendee = '" + att + "', " +
                        "eventDate = '" + date + "', " +
                        "MorningIn = '" + mornIn + "', " +
                        "MorningOut = '" + mornOut + "', " +
                        "AftIn = '" + aftIn + "', " +
                        "AftOut = '" + aftOut + "', " +
                        "EveIn = '" + eveIn + "', " +
                        "EveOut = '" + eveOut + "' where evtCode = '" + code + "'";
                    cmd.ExecuteNonQuery();
                    upcon.Close();
                    MessageBox.Show("Event Successfully Updated!", " Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    clrEvt();
                    dispEvt();
                }     
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void upStud()
        {
            SqlConnection upcon = new SqlConnection(empcs);

            string code = lblRegStud.Text;
            string studId = tBStudId.Text;
            string first = tBStudFirst.Text;
            string mid = tBStudMid.Text;
            string last = tBStudLast.Text;
            string sex = cbStudSex.Text;
            string yr = tBStudYr.Text;
            string dept = tBStudDept.Text;
            string cour = tBStudCour.Text;

            try
            {
                if (studId == "" || first == "" || mid == "" || last == "" 
                    || dept == "" || cour == "" || sex == "" || yr == "")
                {
                    MessageBox.Show("Missing Fields!", " Incomplete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    upcon.Open();
                    SqlCommand cmd = upcon.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "Update studTB set studId = '" + studId + "', " +
                        "Firstname = '" + first + "', " +
                        "Midname = '" + mid + "', " +
                        "Lastname = '" + last + "', " +
                        "Sex = '" + sex + "', " +
                        "Year = '" + yr + "', " +
                        "Dept = '" + dept + "', " +
                        "Course = '" + cour + "' where regCode = '" + code + "'";
                    cmd.ExecuteNonQuery();
                    upcon.Close();
                    MessageBox.Show("Student Successfully Updated!", " Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    clrStud();
                    dispStud();
                }

            }
            catch (Exception e)
            {
                if (e.Message == "Empty path name is not legal.")
                {
                    MessageBox.Show("Please Upload your photo!", " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        public void upEmp()
        {
            SqlConnection upcon = new SqlConnection(empcs);

            string code = lblRegEmp.Text;
            string empId = tBEmpId.Text;
            string first = tBEmpFirst.Text;
            string mid = tBEmpMid.Text;
            string last = tBEmpLast.Text;
            string dept = tBEmpDept.Text;
            string pos = tBEmpPos.Text;

            try
            {
                if (empId == "" || first == "" || mid == "" || last == "" || dept == "" || pos == "")
                {
                    MessageBox.Show("Missing Fields!", " Incomplete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    upcon.Open();
                    SqlCommand cmd = upcon.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "Update empTB set empId = '" + empId + "', " +
                        "Firstname = '" + first + "', " +
                        "Midname = '" + mid + "', " +
                        "Lastname = '" + last + "', " +
                        "Dept = '" + dept + "', " +
                        "Pos = '" + pos + "' where regCode = '" + code + "'";
                    cmd.ExecuteNonQuery();
                    upcon.Close();
                    MessageBox.Show("Employee Successfully Updated!", " Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    clrEmp();
                    dispEmp();
                }
               
            }
            catch (Exception e)
            {
                if (e.Message == "Empty path name is not legal.")
                {
                    MessageBox.Show("Please Upload your photo!", " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        public void newEvt()
        {
            SqlConnection crecon = new SqlConnection(empcs);
            SqlConnection chkcon = new SqlConnection(empcs);

            string pre = "EVT";
            string name = tBEventName.Text;
            string att = tBEventAtt.Text;
            string date = dTEventDate.Text;
            string mornIn = tBEventMorIn.Text;
            string mornOut = tBEventMorOut.Text;
            string aftIn = tBEventAftIn.Text;
            string aftOut = tBEventAftOut.Text;
            string eveIn = tBEventEveIn.Text;
            string eveOut = tBEventEveOut.Text;
            string stat = "AVAIL";

            try
            {
                if (name == "" || att == "" || date == "" || 
                    mornIn == "" || mornOut == "" || 
                    aftIn == "" || aftOut == "" || 
                    eveIn == "" || eveOut == "")
                {
                    MessageBox.Show("Missing Fields!", " Incomplete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    chkcon.Open();
                    SqlCommand cmd = chkcon.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "Select * from eventTB where eventName = '" + name + "' " +
                        "and Attendee = '" + att + "' and eventDate = '" + date + "'";
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        MessageBox.Show("Event Already Added!", " Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        tBEventCode.Text = (dr["evtCode"].ToString());
                        tBEventName.Text = (dr["eventName"].ToString());
                        tBEventAtt.Text = (dr["Attendee"].ToString());
                        dTEventDate.Text = (dr["eventDate"].ToString());
                        tBEventMorIn.Text = (dr["MorningIn"].ToString());
                        tBEventMorOut.Text = (dr["MorningOut"].ToString());
                        tBEventAftIn.Text = (dr["AftIn"].ToString());
                        tBEventAftOut.Text = (dr["AftOut"].ToString());
                        tBEventEveIn.Text = (dr["EveIn"].ToString());
                        tBEventEveOut.Text = (dr["EveOut"].ToString());

                        btnEvtAdd.Text = "UPDATE";
                    }
                    else
                    {
                        crecon.Open();
                        SqlCommand cmd2 = crecon.CreateCommand();
                        cmd2.CommandType = CommandType.Text;
                        cmd2.CommandText = "Insert into eventTB Values ('" + pre + "', " +
                            "'" + name + "', " +
                            "'" + att + "', " +
                            "'" + date + "', " +
                            "'" + mornIn + "', " +
                            "'" + mornOut + "', " +
                            "'" + aftIn + "', " +
                            "'" + aftOut + "', " +
                            "'" + eveIn + "', " +
                            "'" + eveOut + "', " +
                            "'" + stat + "')";
                        cmd2.ExecuteNonQuery();

                        cmd2.CommandText = "Select Top 1 * from eventTB Order by evtCode DESC";
                        SqlDataReader dr2 = cmd2.ExecuteReader();

                        if (dr2.Read())
                        {
                            tBEventCode.Text = (dr2["evtCode"].ToString());
                        }
                        crecon.Close();
                        
                        MessageBox.Show("Event Successfully Added!.", " Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        dispEvt();
                        btnEvtAdd.Text = "UPDATE";
                    }
                    chkcon.Close();

                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void newStud()
        {
            SqlConnection crecon = new SqlConnection(empcs);
            SqlConnection chkcon = new SqlConnection(empcs);

            string studId = tBStudId.Text;
            string first = tBStudFirst.Text;
            string mid = tBStudMid.Text;
            string last = tBStudLast.Text;
            string sex = cbStudSex.Text;
            string yr = tBStudYr.Text;
            string dept = tBStudDept.Text;
            string pos = tBStudCour.Text;
            string pre = "S";

            try
            {
                if (studId == "" || first == "" || mid == "" || last == ""
                     || dept == "" || pos == "" || sex == "" || yr == "")
                {
                    MessageBox.Show("Missing Fields!", " Incomplete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (pBEmp.Image == Properties.Resources.download)
                {
                    MessageBox.Show("Pleae Upload your photo!", " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    chkcon.Open();
                    SqlCommand cmd = chkcon.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "Select * from studTB where Firstname = '" + first + "' " +
                        "and Midname = '" + mid + "' " +
                        "and Lastname = '" + last + "' " +
                        "and studId = '" + studId + "'";
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        MessageBox.Show("Student Already Added!", " Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        lblRegStud.Text = (dr["regCode"].ToString());
                        tBStudId.Text = (dr["empId"].ToString());
                        tBStudFirst.Text = (dr["Firstname"].ToString());
                        tBStudMid.Text = (dr["Midname"].ToString());
                        tBStudLast.Text = (dr["Lastname"].ToString());
                        cbStudSex.Text = (dr["Sex"].ToString());
                        tBStudYr.Text = (dr["Year"].ToString());
                        tBStudDept.Text = (dr["Dept"].ToString());
                        tBStudCour.Text = (dr["Course"].ToString());

                        byte[] img = (byte[])(dr["empImg"]);
                        if (img == null)
                        {
                            pBStud.Image = null;
                        }
                        else
                        {
                            MemoryStream ms = new MemoryStream(img);
                            pBStud.Image = Image.FromStream(ms);
                        }
                        btnStudAdd.Text = "UPDATE";
                        btnStudPH.Text = "UPDATE";
                        studQR();
                    }
                    else
                    {
                        byte[] img = null;
                        FileStream fs = new FileStream(imgLoc, FileMode.Open, FileAccess.Read);
                        BinaryReader br = new BinaryReader(fs);
                        img = br.ReadBytes((int)fs.Length);

                        crecon.Open();
                        SqlCommand cmd2 = crecon.CreateCommand();
                        cmd2.CommandType = CommandType.Text;
                        cmd2.CommandText = "Insert into studTB Values('" + pre + "'," +
                            "'" + studId + "', " +
                            "'" + first + "', " +
                            "'" + mid + "', " +
                            "'" + last + "', " +
                            "'" + sex + "', " +
                            "'" + yr + "', " +
                            "'" + dept + "', " +
                            "'" + pos + "', @img)";
                        cmd2.Parameters.Add(new SqlParameter("@img", img));
                        cmd2.ExecuteNonQuery();

                        cmd2.CommandText = "Select regCode from studTB where studId = '" + studId + "'";
                        SqlDataReader dr2 = cmd2.ExecuteReader();

                        if (dr2.Read())
                        {
                            lblRegStud.Text = (dr2["regCode"].ToString());
                        }
                        
                        crecon.Close();
                        MessageBox.Show("Student Successfully Added!.", " Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        dispStud();
                        studQR();
                        btnStudAdd.Text = "UPDATE";
                        btnStudPH.Text = "UPDATE";
                    }
                    chkcon.Close();
                }

            }
            catch (Exception e)
            {
                if (e.Message == "Empty path name is not legal.")
                {
                    MessageBox.Show("Pleae Upload your photo!", " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }
        public void newEmp()
        {
            SqlConnection crecon = new SqlConnection(empcs);
            SqlConnection chkcon = new SqlConnection(empcs);

            string empId = tBEmpId.Text;
            string first = tBEmpFirst.Text;          
            string mid = tBEmpMid.Text;                
            string last = tBEmpLast.Text;
            string dept = tBEmpDept.Text;
            string pos = tBEmpPos.Text;
            string pre = "E";
            

            try
            {
                if (empId == "" ||first == "" || mid == "" || last == "" || dept == "" || pos == "")
                {
                    MessageBox.Show("Missing Fields!", " Incomplete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (pBEmp.Image == Properties.Resources.download)
                {
                    MessageBox.Show("Pleae Upload your photo!", " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    chkcon.Open();
                    SqlCommand cmd = chkcon.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "Select * from empTB where Firstname = '" + first + "' " +
                        "and Midname = '" + mid + "' " +
                        "and Lastname = '" + last + "' " +
                        "and empId = '" + empId + "'";
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        MessageBox.Show("Employee Already Added!", " Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        lblRegEmp.Text = (dr["regCode"].ToString());
                        tBEmpId.Text = (dr["empId"].ToString());
                        tBEmpFirst.Text = (dr["Firstname"].ToString());
                        tBEmpMid.Text = (dr["Midname"].ToString());
                        tBEmpLast.Text = (dr["Lastname"].ToString());
                        tBEmpDept.Text = (dr["Dept"].ToString());
                        tBEmpPos.Text = (dr["Pos"].ToString());

                        byte[] img = (byte[])(dr["empImg"]);
                        if (img == null)
                        {
                            pBEmp.Image = null;
                        }
                        else
                        {
                            MemoryStream ms = new MemoryStream(img);
                            pBEmp.Image = Image.FromStream(ms);
                        }
                        btnEmpAdd.Text = "UPDATE";
                        btnEmpPH.Text = "UPDATE";
                        empQR();
                    }
                    else
                    {
                        byte[] img = null;
                        FileStream fs = new FileStream(imgLoc, FileMode.Open, FileAccess.Read);
                        BinaryReader br = new BinaryReader(fs);
                        img = br.ReadBytes((int)fs.Length);

                        crecon.Open();
                        SqlCommand cmd2 = crecon.CreateCommand();
                        cmd2.CommandType = CommandType.Text;
                        cmd2.CommandText = "Insert into empTB Values('" + pre + "', " +
                            "'" + empId + "', " +
                            "'" + first + "', " +
                            "'" + mid + "', " +
                            "'" + last + "', " +
                            "'" + dept + "', " +
                            "'" + pos + "', @img)";
                        cmd2.Parameters.Add(new SqlParameter("@img", img));
                        cmd2.ExecuteNonQuery();

                        cmd2.CommandText = "Select regCode from empTB where empId = '" + empId + "'";
                        SqlDataReader dr2 = cmd2.ExecuteReader();

                        if (dr2.Read())
                        {
                            lblRegEmp.Text = (dr2["regCode"].ToString());
                        }

                        crecon.Close();
                        MessageBox.Show("Employee Successfully Added!", " Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        dispEmp();
                        empQR();
                        btnEmpAdd.Text = "UPDATE";
                        btnEmpPH.Text = "UPDATE";
                    }
                    chkcon.Close();
                }
                
            }
            catch (Exception e)
            {
                if (e.Message == "Empty path name is not legal.")
                {
                    MessageBox.Show("Pleae Upload your photo!", " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
               
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            paneSet.Visible = false;
            tabControl1.TabPages.Clear();
            timer1.Start();
            lblDate.Text = DateTime.Now.ToLongDateString();
            this.TopMost = true;
            this.BringToFront();
            loadCre();
            loadEvt();
            tabControl1.TabPages.Insert(0, tabHome);
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            paneSide.Height = btnHome.Height;
            paneSide.Top = btnHome.Top;
            tabControl1.TabPages.Clear();
            tabControl1.TabPages.Insert(0, tabHome);
        }

        private void btnAddEmp_Click(object sender, EventArgs e)
        {
            paneSide.Height = btnAddEmp.Height;
            paneSide.Top = btnAddEmp.Top;
            tabControl1.TabPages.Clear();
            clrEmp();
            dispEmp();
            tabControl1.TabPages.Insert(1, tabAddEmp);
        }

        private void btnAddStud_Click(object sender, EventArgs e)
        {
            paneSide.Height = btnAddStud.Height;
            paneSide.Top = btnAddStud.Top;
            tabControl1.TabPages.Clear();
            clrStud();
            dispStud();
            tabControl1.TabPages.Insert(2, tabStud);
        }
        private void btnAttdLog_Click(object sender, EventArgs e)
        {
            paneSide.Height = btnAttdLog.Height;
            paneSide.Top = btnAttdLog.Top;
            tabControl1.TabPages.Clear();

            tabControl1.TabPages.Insert(3, tabLog);
        }
        private void btnAddEvent_Click(object sender, EventArgs e)
        {
            paneSide.Height = btnAddEvent.Height;
            paneSide.Top = btnAddEvent.Top;
            tabControl1.TabPages.Clear();
            clrEvt();
            dispEvt();
            tabControl1.TabPages.Insert(4, tabEvent);
        }
 
        private void timer1_Tick(object sender, EventArgs e)
        {
            lblTime.Text = DateTime.Now.ToLongTimeString();
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            if (paneSet.Visible == false)
            {
                paneSet.Visible = true;
                paneSet.BringToFront();
            }
            else
            {
                paneSet.Visible = false;
                paneSet.SendToBack();
            }
        }
    

        private void bntEmpRe_Click(object sender, EventArgs e)
        {
            clrEmp();
        }

        private void btnEmpAdd_Click(object sender, EventArgs e)
        {
            if (btnEmpAdd.Text == "ADD")
            {
                newEmp();
            }
            else
            {
                upEmp();
            }
        }

        private void gVEmp_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (gVEmp.Rows.Count > 0)
            {
                cellEmp();
            }
            else
            {
                MessageBox.Show("Data Empty!", " Empty", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEmpPH_Click(object sender, EventArgs e)
        {
            if (btnEmpPH.Text == "UPLOAD")
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.tif;...";
                dlg.Title = "Select a Photo";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    imgLoc = dlg.FileName.ToString();
                    pBEmp.ImageLocation = imgLoc;
                }
            }
            else
            {
                if (tBEmpId.Text == "")
                {
                    MessageBox.Show("Employee ID can't be empty!", " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    tBEmpId.Focus();
                }
                else
                {
                    upEmpPH();
                }            
            }    
        }
        private void tBEmpDept_Leave(object sender, EventArgs e)
        {
            var tb = (TextBox)sender;
            if (tb.Text.Length > 0)
            {
                tb.Text = Char.ToUpper(tb.Text[0]) + tb.Text.Substring(1);
            }
        }

        private void btnEmpGen_Click(object sender, EventArgs e)
        {
            if (pBEmpQR.Image == null)
            {
                MessageBox.Show("QR Code Empty!", " Empty", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (tBEmpId.Text == "")
            {
                MessageBox.Show("Employee ID Empty!", " Empty", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                try
                {
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Filter = "PNG Image(*.png)|*.png|JPG Image(*.jpg)|*.jpg|BMP Image(*.bmp)|*.bmp";
                    sfd.FileName = tBEmpId.Text;
                    ImageFormat format = ImageFormat.Png;
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        string ext = System.IO.Path.GetExtension(sfd.FileName);
                        pBEmpQR.Image.Save(sfd.FileName, format);
                        MessageBox.Show("Image Saved!", " Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception f)
                {
                     MessageBox.Show(f.Message, " Empty", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
               
            }
        }

        private void btnStudAdd_Click(object sender, EventArgs e)
        {
            if (btnStudAdd.Text == "ADD")
            {
                newStud();
            }
            else
            {
                upStud();
            }
        }

        private void btnStudGen_Click(object sender, EventArgs e)
        {
            if (pBStudQR.Image == null)
            {
                MessageBox.Show("QR Code Empty!", " Empty", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (tBStudId.Text == "")
            {
                MessageBox.Show("Student ID Empty!", " Empty", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                try
                {
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Filter = "PNG Image(*.png)|*.png|JPG Image(*.jpg)|*.jpg|BMP Image(*.bmp)|*.bmp";
                    sfd.FileName = tBStudId.Text;
                    ImageFormat format = ImageFormat.Png;
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        string ext = System.IO.Path.GetExtension(sfd.FileName);
                        pBStudQR.Image.Save(sfd.FileName, format);
                        MessageBox.Show("Image Saved!", " Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception f)
                {
                    MessageBox.Show(f.Message, " Empty", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }

        private void btnStudPH_Click(object sender, EventArgs e)
        {
            if (btnStudPH.Text == "UPLOAD")
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.tif;...";
                dlg.Title = "Select a Photo";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    imgLoc = dlg.FileName.ToString();
                    pBStud.ImageLocation = imgLoc;
                }
            }
            else
            {
                if (tBStudId.Text == "")
                {
                    MessageBox.Show("Student ID can't be empty!", " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    tBStudId.Focus();
                }
                else
                {
                    upStudPH();
                }
            }
        }

        private void gVStud_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (gVStud.Rows.Count > 0)
            {
                cellStud();
            }
            else
            {
                MessageBox.Show("Data Empty!", " Empty", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void gVEvent_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (gVEvent.Rows.Count > 0)
            {
                cellEvt();
            }
            else
            {
                MessageBox.Show("Data Empty!", " Empty", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEventRe_Click(object sender, EventArgs e)
        {
            clrEvt();
        }

        private void btnEvtAdd_Click(object sender, EventArgs e)
        {
            if (btnEvtAdd.Text == "ADD")
            {
                newEvt();
            }
            else
            {
                upEvt();
            }
        }


        private void btnStudRe_Click(object sender, EventArgs e)
        {
            clrStud();
        }

        private void tBEventCode_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void btnCPEvt_Click(object sender, EventArgs e)
        {
            if (gVEvent.Rows.Count > 0)
            {
                cpEvt();
            }
            else
            {
                MessageBox.Show("Data Empty!", " Empty", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cBEvt_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cBEvt.Text != "")
            {
                tBAtt.Text = "";
                if (cBEvt.Text == "Regular Employee LogIn")
                {
                    lblCode.Text = "REG0001";
                    tBAtt.Text = "ALL EMPLOYEES";
                }
                else
                {
                    cbMainEvt();
                }
            }
        }

        private void btnDelEvt_Click(object sender, EventArgs e)
        {
            if (gVEvent.Rows.Count > 0)
            {
                if (MessageBox.Show ("Are you sure?", " Verify", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    delEvt();
                }
            }
        }

        private void btnSetSave_Click(object sender, EventArgs e)
        {
            upSet();
        }
    }
}
