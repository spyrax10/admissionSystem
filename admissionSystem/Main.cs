using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace admissionSystem
{
    public partial class Main : Form
    {
        string empcs = @"Data Source=D8672B6A3F8B574\LOCAL;Initial Catalog=facialDB;Integrated Security=True";

        string imgLoc = "";
        public Main()
        {
            InitializeComponent();
            paneSide.Height = btnHome.Height;
            paneSide.Top = btnHome.Top;
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        public void newEmp()
        {
            SqlConnection crecon = new SqlConnection(empcs);
            SqlConnection chkcon = new SqlConnection(empcs);

            string first = tBEmpAddFirst.Text;      string cfirst = tBEmpAddConFirst.Text;      string count = cBEmpAddCoun.Text;
            string mid = tBEmpAddMid.Text;          string cmid = tBEmpAddConMid.Text;          string pro = cBEmpAddPro.Text;
            string last = tBEmpAddLast.Text;        string clast = tBEmpAddConLast.Text;        string mun = cBEmpAddMun.Text;
            string sex = cBEmpAddSex.Text;          string cadd = tBEmpAddConAdd.Text;          string bar = cBEmpAddBar.Text;
            string stat = cbEmpAddStat.Text;        string cmob = tBEmpAddConMob.Text;          string hou = tBEmpAddPur.Text;          
            string rel = cBEmpAddRel.Text;          string crel = tbEmpAddConRel.Text;          string dept = tBEmpAddDept.Text;
            string dob = dTEmpAdd.Text;                                                         string pos = tBEmpAddPos.Text;
            string doMun = tBEmpAddMun.Text;        string mob = tBEmpAddConMob.Text;           string estat = tBEmpAddStat.Text;
            string doPro = tBEmpAddPro.Text;        string email = tBEmpAddEmail.Text;

            try
            {
                if (first == "" || mid  == "" || last == "" || sex == "" || stat == "" || rel == "" || 
                    doMun == "" || doPro == "" || cfirst == "" || cmid == "" || clast == "" || cadd == "" ||
                    cmob == "" || crel  == "" || count == "" || pro  == "" || mun == "" || bar == "" ||
                    hou ==  "" || dept == "" || pos == "" || estat == "")
                {
                    MessageBox.Show("Missing Fields!", " Incomplete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (pBEmpAdd.Image == Properties.Resources.download)
                {
                    MessageBox.Show("Pleae Upload your photo!", " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    chkcon.Open();
                    SqlCommand cmd = chkcon.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "Select * from empTB where Firstname = '" + first + "' " +
                        "and Middlename = '" + mid + "' " +
                        "and Lastname = '" + last + "'";
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        tBEmpAddId.Text = (dr["empId"].ToString());

                        tBEmpAddFirst.Text = (dr["Firstname"].ToString());
                        tBEmpAddMid.Text = (dr["Middlename"].ToString());
                        tBEmpAddLast.Text = (dr["Lastname"].ToString());
                        dTEmpAdd.Text = (dr["DOB"].ToString());
                        tBEmpAddPro.Text = (dr["DOPro"].ToString());
                        tBEmpAddMun.Text = (dr["DOMun"].ToString());
                        cBEmpAddSex.Text = (dr["Gender"].ToString());
                        cbEmpAddStat.Text = (dr["Stat"].ToString());
                        cBEmpAddRel.Text = (dr["Rel"].ToString());
                        tBEmpAddPur.Text = (dr["HouNo"].ToString());
                        cBEmpAddBar.Text = (dr["Brgy"].ToString());
                        cBEmpAddMun.Text = (dr["Municipality"].ToString());
                        cBEmpAddPro.Text = (dr["Province"].ToString());
                        cBEmpAddCoun.Text = (dr["Country"].ToString());
                        tBEmpAddMob.Text = (dr["Mobile"].ToString());
                        tBEmpAddEmail.Text = (dr["Email"].ToString());
                        tBEmpAddDept.Text = (dr["Department"].ToString());
                        tBEmpAddPos.Text = (dr["Pos"].ToString());
                        tBEmpAddStat.Text = (dr["EStat"].ToString());
                        tBEmpAddConFirst.Text = (dr["CFirst"].ToString());
                        tBEmpAddConMid.Text = (dr["CMid"].ToString());
                        tBEmpAddConLast.Text = (dr["CLast"].ToString());
                        tBEmpAddConMob.Text = (dr["CMob"].ToString());
                        tBEmpAddConAdd.Text = (dr["CAdd"].ToString());
                        tbEmpAddConRel.Text = (dr["CRel"].ToString());



                        byte[] img = (byte[])(dr["empImg"]);
                        if (img == null)
                        {
                            pBEmpAdd.Image = null;
                        }

                        else
                        {
                            MemoryStream ms = new MemoryStream(img);
                            pBEmpAdd.Image = Image.FromStream(ms);
                        }

                    }
                    else
                    {
                        crecon.Open();
                        SqlCommand cmd2 = crecon.CreateCommand();
                        cmd2.CommandType = CommandType.Text;
                        cmd2.CommandText = "Insert into empInfo Values('" + first + "', " +
                            "'" + mid + "', " +
                            "'" + last + "', " +
                            "'" + dob + "', " +
                            "'" + doPro + "', " +
                            "'" + doMun + "', " +
                            "'" + sex + "', " +
                            "'" + stat + "'," +
                            "'" + rel + "', " +
                            "'" + hou + "', " +
                            "'" + bar +"', " +
                            "'" + mun + "', " +
                            "'" + pro + "', " +
                            "'" + count + "', " +
                            "'" + mob + "', " +
                            "'" + email + "', " +
                            "'" + dept + "', " +
                            "'" + pos + "', " +
                            "'" + estat + "', " +
                            "'" + cfirst + "', " +
                            "'" + cmid + "', " +
                            "'" + clast + "', " +
                            "'" + cmob + "', " +
                            "'" + cadd + "', " +
                            "'" + crel + "')";
                        cmd2.ExecuteNonQuery();
                        crecon.Close();
                    }
                    chkcon.Close();
                }
                

            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, " Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            paneSet.Visible = false;
            tabControl1.TabPages.Clear();
            timer1.Start();
            lblDate.Text = DateTime.Now.ToLongDateString();
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
            tabControl1.TabPages.Insert(1, tabAddEmp);
        }

        private void btnEmpInfo_Click(object sender, EventArgs e)
        {
            paneSide.Height = btnEmpInfo.Height;
            paneSide.Top = btnEmpInfo.Top;
            tabControl1.TabPages.Clear();
            tabControl1.TabPages.Insert(2, tabEmpInfo);
        }

        private void btnAddStud_Click(object sender, EventArgs e)
        {
            paneSide.Height = btnAddStud.Height;
            paneSide.Top = btnAddStud.Top;
        }

        private void btnStudInfo_Click(object sender, EventArgs e)
        {
            paneSide.Height = btnStudInfo.Height;
            paneSide.Top = btnStudInfo.Top;
        }

        private void btnAdInfo_Click(object sender, EventArgs e)
        {
            paneSide.Height = btnAdInfo.Height;
            paneSide.Top = btnAdInfo.Top;
        }

        private void btnOut_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            Screen sc = new Screen();
            sc.ShowDialog();
            
        }

        private void btnEmpAddUp_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.tif;...";
            dlg.Title = "Select a Photo";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                imgLoc = dlg.FileName.ToString();
                pBEmpAdd.ImageLocation = imgLoc;
            }
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
        }

        private void btnSetX_Click(object sender, EventArgs e)
        {
            paneSet.Visible = false;
        }

        private void paneSet_Leave(object sender, EventArgs e)
        {
            paneSet.Visible = false;
        }

        private void paneSet_Enter(object sender, EventArgs e)
        {
            if (!paneSet.Focus())
            {
                paneSet.Visible = false;
            }
        }

        private void btnSPrint_Click(object sender, EventArgs e)
        {

        }
    }
}
