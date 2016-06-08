using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.IO;

namespace DirSize
{
    class PieChartDrawer
    {
        //colors
        public Color[] StandardColors { get; set; }
        public Color OtherColor { get; set; }
        public Color FilesColor { get; set; }

        //pie chart dimensions
        public int ChartBorder { get; set; }
        public Color ChartBorderColor { get; set; }
        public float ChartSizeRatio { get; set; }

        //legend dimensions
        public int SquareSize { get; set; }
        public int HalfBorder { get; set; }
        public int FontSize { get; set; }

        private Dictionary<DSDir, Color> ColorMap_;

        private DSDir CurrentDirectory_ = null;
        public DSDir CurrentDirectory
        {
            get { return CurrentDirectory_; }
            set { UpdateCurrentDirectory(value); }
        }

        public PieChartDrawer(DSDir directory)
        {
            //colors
            StandardColors = new Color[] { Color.Red, Color.Green, Color.Blue, Color.Purple, Color.Cyan, Color.Yellow };
            OtherColor = Color.Black;
            FilesColor = Color.Gray;

            //pie chart dimensions
            ChartBorder = 5;
            ChartBorderColor = Color.LightGray;
            ChartSizeRatio = 0.8f;

            //legend dimensions
            SquareSize = 15;
            HalfBorder = 5;
            FontSize = 10;

            if (directory == null)
                throw new ArgumentNullException();

            this.ColorMap_ = new Dictionary<DSDir, Color>();

            UpdateCurrentDirectory(directory);
        }

        private void UpdateCurrentDirectory(DSDir newDir)
        {
            if (CurrentDirectory_ == newDir)
                return;

            CurrentDirectory_ = newDir;
            CurrentDirectory_.subdirs.Sort(new DSDirComparer());
            UpdateColorMap();
        }

        private void UpdateColorMap()
        {
            ColorMap_.Clear();
            List<DSDir> subdirs = CurrentDirectory_.subdirs;
            for (int i = 0; i < subdirs.Count; i++)
            {
                if (i < StandardColors.Length)
                    ColorMap_[subdirs[i]] = StandardColors[i];
                else
                    ColorMap_[subdirs[i]] = OtherColor;
            }
        }

        public void DrawChart(Control control)
        {
            Graphics gfx = control.CreateGraphics();
            gfx.Clear(control.BackColor);
            int minDimension = control.Width > control.Height ? control.Height : control.Width;
            int radius = (int)((minDimension - 2 * ChartBorder) * ChartSizeRatio / 2);
            int x = control.Width / 2, y = control.Height / 2;

            //sizes to be drawn
            List<DSDir> subdirs = CurrentDirectory_.subdirs;
            long allSubdirsSize = 0;
            foreach (DSDir subdir in subdirs)
            {
                allSubdirsSize += subdir.size;
            }
            long filesSize = CurrentDirectory_.size - allSubdirsSize;

            //drawing background
            gfx.FillRectangle(
                new HatchBrush(HatchStyle.WideDownwardDiagonal, Color.LightGray, Color.White),
                new Rectangle(Point.Empty, control.Size));
            gfx.FillEllipse(
                new SolidBrush(ChartBorderColor),
                new Rectangle(
                    x - radius - ChartBorder,
                    y - radius - ChartBorder,
                    2 * (radius + ChartBorder),
                    2 * (radius + ChartBorder)));

            //drawing subdirs
            Rectangle targetRect = new Rectangle(x - radius, y - radius, 2 * radius, 2 * radius);
            float startAngle = 0;
            for (int i = 0; i < subdirs.Count; i++)
            {
                float sweepAngle = 360f * subdirs[i].size / CurrentDirectory_.size;
                gfx.FillPie(new SolidBrush(ColorMap_[subdirs[i]]), targetRect, startAngle, sweepAngle);
                startAngle += sweepAngle;
            }

            //files
            gfx.FillPie(new SolidBrush(FilesColor), targetRect, startAngle, 360 - startAngle);
        }

        public DSDir GetDirUnderCursor(Control control, Point location)
        {
            if (CurrentDirectory_.subdirs.Count == 0)
                return null;

            int cx = control.Width / 2, cy = control.Height / 2;
            int dx = location.X - cx, dy = location.Y - cy;
            double phi = Math.Atan2(dy, dx);
            if (phi < 0)
                phi += Math.PI * 2;
            double r = Math.Sqrt(dx * dx + dy * dy);
            int minDimension = control.Width > control.Height ? control.Height : control.Width;
            int radius = (int)((minDimension - 2 * ChartBorder) * ChartSizeRatio / 2);
            if (r > radius)
                return null;

            double ratio = phi / (Math.PI * 2);
            long start = 0;
            for (int i = 0; i < CurrentDirectory_.subdirs.Count; i++)
            {
                start += CurrentDirectory_.subdirs[i].size;
                if (1.0 * start / CurrentDirectory_.size > ratio)
                    return CurrentDirectory_.subdirs[i];
            }
            return null;
        }

        public void DrawLegend(Control control)
        {
            Graphics gfx = control.CreateGraphics();
            gfx.Clear(control.BackColor);
            
            int ox = 0, oy = 0; //origin

            List<DSDir> subdirs = CurrentDirectory_.subdirs;

            int lineHeight = SquareSize + 2 * HalfBorder;
            //drawing subdirs
            int i = 0;
            for (; i < subdirs.Count; i++)
            {
                string dir = subdirs[i].path.Split(new char[]{'\\', '/'}).LastOrDefault();
                DrawLegendLine(ox, oy + i * lineHeight, ColorMap_[subdirs[i]], dir, gfx);
            }

            //files
            DrawLegendLine(ox, oy + i * lineHeight, FilesColor, "Files", gfx);
        }

        private void DrawLegendLine(int ox, int oy, Color color, string text, Graphics gfx)
        {
            int stringOffset = FontSize / 5;
            gfx.FillRectangle(new SolidBrush(color), new Rectangle(ox + HalfBorder, oy + HalfBorder, SquareSize, SquareSize));
            gfx.DrawRectangle(new Pen(Color.Black), new Rectangle(ox + HalfBorder, oy + HalfBorder, SquareSize, SquareSize));
            gfx.DrawString(text, new Font("Sergoe UI", FontSize), new SolidBrush(Color.Black), new Point(ox + HalfBorder * 2 + SquareSize, oy + (SquareSize + 2 * HalfBorder - FontSize) / 2 - stringOffset));
        }

        public class DSDirComparer : IComparer<DSDir>
        {
            bool Ascending;
            public DSDirComparer(bool ascending = false)
            {
                this.Ascending = ascending;
            }

            public int Compare(DSDir x, DSDir y)
            {
                long d = x.size - y.size;
                if (Ascending)
                    return Math.Sign(d);
                return -Math.Sign(d);
            }
        }
    }
}
