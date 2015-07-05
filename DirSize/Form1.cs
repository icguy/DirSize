#define DEBUG

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace DirSize
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //TODO:
        //-kurzor fölött tooltipben az éppen aktuális pie-rész adatai
        //-legend

        private void Form1_Load(object sender, EventArgs e)
        {
            //string path = @"D:\Dokumentumok";
            //DSDir root = new DSDir(path);
            //System.Diagnostics.Debug.WriteLine(DDirHelper.printDDir(root));
        }

        DSDir CurrentDirectory_;
        DSDir RootDirectory_;
        PieChartDrawer ChartDrawer_;

        private void button1_Click(object sender, EventArgs e)
        {
#if !DEBUG
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            rootDirectory = new DSDir(fbd.SelectedPath);
#else
            RootDirectory_ = new DSDir("D:/Asztal/temp");
#endif
            ChartDrawer_ = new PieChartDrawer(RootDirectory_);
            CurrentDirectory_ = RootDirectory_;
            System.Diagnostics.Debug.WriteLine(DSDirHelper.printDSDir(CurrentDirectory_));

            ChartDrawer_.DrawChart(panel1);
            ChartDrawer_.DrawLegend(panel2);
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (CurrentDirectory_ == null)
                return;

            DSDir dirundercursor = ChartDrawer_.GetDirUnderCursor(panel1, e.Location);
            if (dirundercursor != null)
                this.Text = dirundercursor.path + " - " + DSDirHelper.getrepr(dirundercursor.size);
            else
                this.Text = "";
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (CurrentDirectory_ != null)
            {
                ChartDrawer_.DrawChart(panel1);
                ChartDrawer_.DrawLegend(panel2);
            }
        }

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            if (CurrentDirectory_ == null)
                return;

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                DSDir dirundercursor = ChartDrawer_.GetDirUnderCursor(panel1, e.Location);
                if (dirundercursor != null)
                {
                    CurrentDirectory_ = dirundercursor;
                    ChartDrawer_.DrawChart(panel1);
                    ChartDrawer_.DrawLegend(panel2);
                }
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (CurrentDirectory_ == RootDirectory_)
                    return;

                CurrentDirectory_ = CurrentDirectory_.parent;
                ChartDrawer_.DrawChart(panel1);
                ChartDrawer_.DrawLegend(panel2);
            }
        }
    }
}
