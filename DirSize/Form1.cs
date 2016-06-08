//#define DEBUG_

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Configuration;

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
        //-ahol a kurzor áll, azt a mappát highlightolni a pie charton és a legenden
        //-appsettings nem működ.

        private void Form1_Load(object sender, EventArgs e)
        {
            //string path = @"D:\Dokumentumok";
            //DSDir root = new DSDir(path);
            //System.Diagnostics.Debug.WriteLine(DDirHelper.printDDir(root));
            Config_ = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        private DSDir CurrentDirectory_;
        private DSDir RootDirectory_;
        private PieChartDrawer ChartDrawer_;
        private Configuration Config_;

        private void button1_Click(object sender, EventArgs e)
        {
#if !DEBUG_
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            string initfolder = Config_.AppSettings.Settings["initialfolder"].Value;
            if (initfolder != null)
                fbd.SelectedPath = initfolder;

            if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            Config_.AppSettings.Settings["initialfolder"].Value = fbd.SelectedPath;
            Config_.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            RootDirectory_ = new DSDir(fbd.SelectedPath);
#else
            RootDirectory_ = new DSDir("D:/Asztal/temp");
#endif
            ChartDrawer_ = new PieChartDrawer(RootDirectory_);
            CurrentDirectory_ = RootDirectory_;
            System.Diagnostics.Debug.WriteLine(DSDirHelper.PrintDSDir(CurrentDirectory_));

            ChartDrawer_.DrawChart(panel1);
            ChartDrawer_.DrawLegend(panel2);
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (CurrentDirectory_ == null)
                return;

            DSDir dirundercursor = ChartDrawer_.GetDirUnderCursor(panel1, e.Location);
            if (dirundercursor != null)
                this.Text = dirundercursor.path + " - " + DSDirHelper.SizeToString(dirundercursor.size);
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
                    RefreshCurrentDir(dirundercursor);
                    ChartDrawer_.DrawChart(panel1);
                    ChartDrawer_.DrawLegend(panel2);
                }
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (CurrentDirectory_ == RootDirectory_)
                    return;

                RefreshCurrentDir(CurrentDirectory_.parent);
                ChartDrawer_.DrawChart(panel1);
                ChartDrawer_.DrawLegend(panel2);
            }
        }

        private void RefreshCurrentDir(DSDir newDir)
        {
            CurrentDirectory_ = newDir;
            ChartDrawer_.CurrentDirectory = CurrentDirectory_;
        }
    }
}
