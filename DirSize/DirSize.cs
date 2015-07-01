using System;
using System.IO;
using System.Collections.Generic;

namespace DirSize
{
    static class DDirHelper
    {
        static public string printDDir(DDir dir)
        {
            string outstr = "";
            List<string> print_inner = printDDir_inner(dir);
            foreach (var line in print_inner)
            {
                outstr = outstr + line + "\n";
            }
            return outstr;
        }

        const string delim = "\t";

        static List<string> printDDir_inner(DDir dir)
        {
            List<string> retval = new List<string>();
            retval.Add("<folder " + dir.path + " - " + getrepr(dir.size)+ ">");
            foreach (var d in dir.subdirs)
            {
                List<string> dlist = printDDir_inner(d);
                foreach (var line in dlist)
                {
                    retval.Add(delim + line);
                }
            }
            retval.Add("</folder>");
            return retval;
        }

        static string getrepr(long size)
        {
            double sz = size;
            int order = 0;
            while (sz > 1024)
            {
                sz /= 1024;
                order++;
            }
            string orderstr = "";
            switch (order)
            {
                case 0:
                    orderstr = "B";
                    break;
                case 1:
                    orderstr = "KB";
                    break;
                case 2:
                    orderstr = "MB";
                    break;
                case 3:
                    orderstr = "GB";
                    break;
            }
            return sz.ToString("0.00") + " " + orderstr;
        }
    }

    class DDir
    {
        public string path;
        public List<DDir> subdirs;
        public List<FFile> files;
        public long size;

        public DDir(string path)
        {
            System.Diagnostics.Debug.WriteLine("scanning: " + path);
            subdirs = new List<DDir>();
            files = new List<FFile>();
            this.path = path;

            string[] subdirs_list = Directory.GetDirectories(path);
            string[] files_list = Directory.GetFiles(path);
            foreach (var f in files_list)
            {
                this.files.Add(new FFile(f));
            }
            foreach (var sd in subdirs_list)
            {
                this.subdirs.Add(new DDir(sd));
            }

            size = 0;
            foreach (var f in files)
            {
                size += f.size;
            }
            foreach (var sd in subdirs)
            {
                size += sd.size;
            }
        }
    }

    class FFile
    {
        public string path;
        public long size;

        public FFile(string path)
        {
            this.path = path;
            this.size = new FileInfo(path).Length;
        }
    }
}