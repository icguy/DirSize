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
        //pie chart szeleteire klikkelve subdirbe lehessen menni
        //jobb klikk parent dir-be
        // kurzor fölött tooltipben az éppen aktuális pie-rész adatai

        private void Form1_Load(object sender, EventArgs e)
        {            
            //string path = @"D:\Dokumentumok";
            //DSDir root = new DSDir(path);
            //System.Diagnostics.Debug.WriteLine(DDirHelper.printDDir(root));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            DSDir dsdir = new DSDir(fbd.SelectedPath);
            PieChartDrawer.DrawChart(panel1.CreateGraphics(), 100, 100, 100, dsdir);
        }
    }
}
