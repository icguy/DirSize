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
        private void Form1_Load(object sender, EventArgs e)
        {
            string path = @"D:\Dokumentumok";
            DDir root = new DDir(path);
            System.Diagnostics.Debug.WriteLine(DDirHelper.printDDir(root));
        }
    }
}
