using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DirSize
{
    static class PieChartDrawer
    {
        public static Color[] StandardColors = { Color.Red, Color.Green, Color.Blue, Color.Purple, Color.Cyan, Color.Yellow};
        public static Color OtherColor = Color.Black;
        public static Color FilesColor = Color.LightGray;
        public static int ChartBorder = 3;

        public static void DrawChart(Graphics gfx, int x, int y, int radius, DSDir directory)
        {
            List<DSDir> subdirs = directory.subdirs;
            long allSubdirsSize = 0;
            foreach (DSDir subdir in subdirs)
            {
                allSubdirsSize += subdir.size;
            }
            long filesSize = directory.size - allSubdirsSize;

            subdirs.Sort(new DSDirComparer());
            
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

            gfx.FillPie(new SolidBrush(FilesColor), targetRect, startAngle, 360 - startAngle);
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
