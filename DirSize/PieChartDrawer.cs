using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DirSize
{
    static class PieChartDrawer
    {
        public static Color[] StandardColors = { Color.Red, Color.Green, Color.Blue, Color.Purple, Color.Cyan, Color.Yellow};
        public static Color OtherColor = Color.Black;
        public static Color FilesColor = Color.Gray;
        public static int ChartBorder = 5;
        public static Color ChartBorderColor = Color.LightGray;
        public static float ChartSizeRatio = 0.8f;

        public static void DrawChart(Control control, DSDir directory)
        {
            Graphics gfx = control.CreateGraphics();
            int minDimension = control.Width > control.Height ? control.Height : control.Width;
            int radius = (int)((minDimension - 2 * ChartBorder) * ChartSizeRatio / 2);
            int x = control.Width / 2, y = control.Height / 2;

            //sizes to be drawn
            List<DSDir> subdirs = directory.subdirs;
            long allSubdirsSize = 0;
            foreach (DSDir subdir in subdirs)
            {
                allSubdirsSize += subdir.size;
            }
            long filesSize = directory.size - allSubdirsSize;

            subdirs.Sort(new DSDirComparer());
            
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
            Rectangle targetRect = new Rectangle(x-radius, y-radius, 2*radius, 2*radius);
            long remainingSubdirsSize = allSubdirsSize;
            int i = 0;
            float startAngle = 0;
            for (; i < subdirs.Count && i < StandardColors.Length; i++)
            {
                float sweepAngle = 360f * subdirs[i].size / directory.size;
                gfx.FillPie(new SolidBrush(StandardColors[i]), targetRect, startAngle, sweepAngle);
                startAngle += sweepAngle;
                remainingSubdirsSize -= subdirs[i].size;
            }

            //if having "other" directories
            if (i < subdirs.Count)
            {
                float sweepAngle = 360f * remainingSubdirsSize / directory.size;
                gfx.FillPie(new SolidBrush(OtherColor), targetRect, startAngle, sweepAngle);
                startAngle += sweepAngle;
            }

            //files
            gfx.FillPie(new SolidBrush(FilesColor), targetRect, startAngle, 360 - startAngle);
        }

        public static DSDir GetDirUnderCursor(Control control, DSDir directory, Point location)
        {
            if (directory == null || directory.subdirs.Count == 0)
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
            for (int i = 0; i < directory.subdirs.Count; i++)
            {
                start += directory.subdirs[i].size;
                if (1.0 * start / directory.size > ratio)
                    return directory.subdirs[i];
            }
            return null;
        }
        
        public static int squaresize = 15;
        public static int halfborder = 5;
        public static void DrawLegend(Control control, DSDir directory)
        {
            Graphics gfx = control.CreateGraphics();
            int ox = 0, oy = 0; //origin

            List<DSDir> subdirs = directory.subdirs;
            subdirs.Sort(new DSDirComparer());

            int lineHeight= squaresize + 2*halfborder;
            //drawing subdirs
            int i = 0;
            for (; i < subdirs.Count && i < StandardColors.Length; i++)
            {
                DrawLegendLine(ox, oy + i * lineHeight, StandardColors[i], subdirs[i].path, gfx);
            }

            //if having "other" directories
            for (; i < subdirs.Count; i++ )
            {
                DrawLegendLine(ox, oy + i * lineHeight, OtherColor, subdirs[i].path, gfx);
            }

            //files
            DrawLegendLine(ox, oy + i * lineHeight, FilesColor, "Files", gfx);
        }

        public static int FontSize = 10;
        static void DrawLegendLine(int ox, int oy, Color color, string text, Graphics gfx)
        {
            int stringOffset = FontSize / 5;
            gfx.FillRectangle(new SolidBrush(color), new Rectangle(ox + halfborder, oy + halfborder, squaresize, squaresize));
            gfx.DrawRectangle(new Pen(Color.Black), new Rectangle(ox + halfborder, oy + halfborder, squaresize, squaresize));
            gfx.DrawString(text, new Font("Sergoe UI", FontSize), new SolidBrush(Color.Black), new Point(ox + halfborder * 2 + squaresize, oy + (squaresize + 2 * halfborder - FontSize) / 2 - stringOffset));
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
                if(Ascending)                    
                    return Math.Sign(d);
                return -Math.Sign(d);
            }
        }
    }
}
