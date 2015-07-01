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

        //TODO: grafikusan megjeleníteni pie charton a cuccot!
        //-pie chart szeleteire klikkelve subdirbe lehessen menni
        //-jobb klikk parent dir-be
        //-kurzor fölött tooltipben az éppen aktuális pie-rész adatai
        //-legend

        private void Form1_Load(object sender, EventArgs e)
        {            
            //string path = @"D:\Dokumentumok";
            //DSDir root = new DSDir(path);
            //System.Diagnostics.Debug.WriteLine(DDirHelper.printDDir(root));
        }

        DSDir currentDirectory;
        DSDir rootDirectory;
        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            rootDirectory = new DSDir(fbd.SelectedPath);
            currentDirectory = rootDirectory;
            System.Diagnostics.Debug.WriteLine(DSDirHelper.printDSDir(currentDirectory));

            PieChartDrawer.DrawChart(panel1, currentDirectory);
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            DSDir dirundercursor = PieChartDrawer.GetDirUnderCursor(panel1, currentDirectory, e.Location);
            if (dirundercursor != null)
                this.Text = dirundercursor.path + " - " + DSDirHelper.getrepr(dirundercursor.size);
            else
                this.Text = "";
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (currentDirectory != null)
                PieChartDrawer.DrawChart(panel1, currentDirectory);
        }

        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                DSDir dirundercursor = PieChartDrawer.GetDirUnderCursor(panel1, currentDirectory, e.Location);
                if (dirundercursor != null)
                {
                    currentDirectory = dirundercursor;
                    PieChartDrawer.DrawChart(panel1, currentDirectory);
                }
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (currentDirectory == rootDirectory)
                    return;

                currentDirectory = currentDirectory.parent;
                PieChartDrawer.DrawChart(panel1, currentDirectory);
            }
        }
    }
}
