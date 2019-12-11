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
    public partial class Main : Form
    {
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

        private void Main_Load(object sender, EventArgs e)
        {
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
    }
}
