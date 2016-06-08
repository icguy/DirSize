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
        private Color[] StandardColors_;
        public Color[] StandardColors
        {
            get { return StandardColors_; }
            set
            {
                StandardColors_ = value;
                UpdateLegendMarkers();
            }
        }

        private Color OtherColor_;
        public Color OtherColor
        {
            get { return OtherColor_; }
            set
            {
                OtherColor_ = value;
                UpdateLegendMarkers();
            }
        }

        private Color FilesColor_;
        public Color FilesColor
        {
            get { return FilesColor_; }
            set
            {
                FilesColor_ = value;
                UpdateLegendMarkers();
            }
        }

        //pie chart dimensions
        public int ChartBorder { get; set; }
        public Color ChartBorderColor { get; set; }
        public float ChartSizeRatio { get; set; }

        //legend dimensions
        private int SquareSize_;
        public int SquareSize
        {
            get { return SquareSize_; }
            set
            {
                SquareSize_ = value;
                UpdateLegendMarkers();
            }
        }
        
        private Dictionary<Color, Bitmap> LegendMarkers_;
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

            if (directory == null)
                throw new ArgumentNullException();

            ColorMap_ = new Dictionary<DSDir, Color>();

            UpdateCurrentDirectory(directory);
        }

        private void UpdateLegendMarkers()
        {
            if (SquareSize <= 0)
                return;

            LegendMarkers_ = new Dictionary<Color, Bitmap>();
            foreach (var color in StandardColors.Concat(new List<Color>() { OtherColor, FilesColor }))
            {
                Bitmap bmp = new Bitmap(SquareSize, SquareSize);
                Graphics gfx = Graphics.FromImage(bmp);
                gfx.FillRectangle(new SolidBrush(color), new Rectangle(0, 0, SquareSize, SquareSize));
                gfx.DrawRectangle(new Pen(Color.Black), new Rectangle(0, 0, SquareSize, SquareSize));
                LegendMarkers_[color] = bmp;
            }
        }

        private void UpdateCurrentDirectory(DSDir newDir)
        {
            if (CurrentDirectory_ == newDir)
                return;

            CurrentDirectory_ = newDir;
            CurrentDirectory_.Subdirs.Sort(new DSDirComparer());
            UpdateColorMap();
        }

        private void UpdateColorMap()
        {
            ColorMap_.Clear();
            List<DSDir> subdirs = CurrentDirectory_.Subdirs;
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
            List<DSDir> subdirs = CurrentDirectory_.Subdirs;
            long allSubdirsSize = 0;
            foreach (DSDir subdir in subdirs)
            {
                allSubdirsSize += subdir.Size;
            }
            long filesSize = CurrentDirectory_.Size - allSubdirsSize;

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
                float sweepAngle = 360f * subdirs[i].Size / CurrentDirectory_.Size;
                gfx.FillPie(new SolidBrush(ColorMap_[subdirs[i]]), targetRect, startAngle, sweepAngle);
                startAngle += sweepAngle;
            }

            //files
            gfx.FillPie(new SolidBrush(FilesColor), targetRect, startAngle, 360 - startAngle);
        }

        public DSDir GetDirUnderCursor(Control control, Point location)
        {
            if (CurrentDirectory_.Subdirs.Count == 0)
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
            for (int i = 0; i < CurrentDirectory_.Subdirs.Count; i++)
            {
                start += CurrentDirectory_.Subdirs[i].Size;
                if (1.0 * start / CurrentDirectory_.Size > ratio)
                    return CurrentDirectory_.Subdirs[i];
            }
            return null;
        }

        public void DrawLegend(DataGridView grid)
        {
            grid.Rows.Clear();

            List<DSDir> subdirs = CurrentDirectory_.Subdirs;
            List<DSDirGridRow> rows = new List<DSDirGridRow>();
            foreach (var subdir in subdirs)
            {                
                string dir = subdir.Path.Split(new char[]{'\\', '/'}).LastOrDefault();
                string size = DSDirHelper.SizeToString(subdir.Size);
                var newrow = new DSDirGridRow()
                    {
                        LegendImage = LegendMarkers_[ColorMap_[subdir]],
                        Path = dir,
                        Size = size
                    };                                
                rows.Add(newrow);
            }

            if (CurrentDirectory_.FilesSize > 0)
            {
                var filesRow = new DSDirGridRow()
                {
                    LegendImage = LegendMarkers_[FilesColor],
                    Path = "(files)",
                    Size = DSDirHelper.SizeToString(CurrentDirectory_.FilesSize)
                };
                rows.Add(filesRow);
            }

            var source = new BindingSource();
            source.DataSource = rows;
            grid.DataSource = source;

            grid.AutoResizeColumns();
            foreach (DataGridViewRow row in grid.Rows)
            {                
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Style.BackColor = SystemColors.Control;
                }
            }


            grid.ClearSelection();
        }

        public struct DSDirGridRow
        {
            public Image LegendImage { get; set; }
            public string Path { get; set; }
            public string Size { get; set; }
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
                long d = x.Size - y.Size;
                if (Ascending)
                    return Math.Sign(d);
                return -Math.Sign(d);
            }
        }
    }
}
