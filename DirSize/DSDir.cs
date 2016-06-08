using System;
using System.IO;
using System.Collections.Generic;

namespace DirSize
{
    static class DSDirHelper
    {
        static public string PrintDSDir(DSDir dir)
        {
            string outstr = "";
            List<string> print_inner = PrintDDir_inner(dir);
            foreach (var line in print_inner)
            {
                outstr = outstr + line + "\n";
            }
            return outstr;
        }

        public const string Delim = "\t";

        static List<string> PrintDDir_inner(DSDir dir)
        {
            List<string> retval = new List<string>();
            retval.Add("<folder " + dir.path + " - " + SizeToString(dir.size)+ ">");
            foreach (var d in dir.subdirs)
            {
                List<string> dlist = PrintDDir_inner(d);
                foreach (var line in dlist)
                {
                    retval.Add(Delim + line);
                }
            }
            retval.Add("</folder>");
            return retval;
        }

        public static string SizeToString(long size)
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

    class DSDir
    {
        public string path { get { return basepath + shortpath; }} //basepath + shortpath = path
        public string basepath { get; private set; }
        public string shortpath { get; private set; }
        public DSDir parent { get; private set; }
        public List<DSDir> subdirs { get; private set; }
        public List<DSFile> files { get; private set; }
        public long size { get { return FilesSize + SubdirectoriesSize; } }
        public long FilesSize { get; private set; }
        public long SubdirectoriesSize { get; private set; }
        
        public DSDir(string path, DSDir parent = null)
        {
            System.Diagnostics.Debug.WriteLine("scanning: " + path);
            subdirs = new List<DSDir>();
            files = new List<DSFile>();
            this.parent = parent;
            if (parent == null)
            {
                this.basepath = path;
                this.shortpath = "";
            }
            else
            {
                this.basepath = parent.basepath;
                this.shortpath = path.Substring(this.basepath.Length);
            }

            string[] subdirs_list = Directory.GetDirectories(path);
            string[] files_list = Directory.GetFiles(path);
            foreach (var f in files_list)
            {
                this.files.Add(new DSFile(f));
            }
            foreach (var sd in subdirs_list)
            {
                this.subdirs.Add(new DSDir(sd, this));
            }

            FilesSize = 0;
            foreach (var f in files)
            {
                FilesSize += f.size;
            }
            SubdirectoriesSize = 0;
            foreach (var sd in subdirs)
            {
                SubdirectoriesSize += sd.size;
            }
        }
    }

    class DSFile
    {
        public string path;
        public long size;

        public DSFile(string path)
        {
            this.path = path;
            this.size = new FileInfo(path).Length;
        }
    }
}