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
            retval.Add("<folder " + dir.Path + " - " + SizeToString(dir.Size)+ ">");
            foreach (var d in dir.Subdirs)
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
        public string Path { get { return Basepath + Shortpath; }} //basepath + shortpath = path
        public string Basepath { get; private set; }
        public string Shortpath { get; private set; }
        public DSDir Parent { get; private set; }
        public List<DSDir> Subdirs { get; private set; }
        public List<DSFile> Files { get; private set; }
        public long Size { get { return FilesSize + SubdirectoriesSize; } }
        public long FilesSize { get; private set; }
        public long SubdirectoriesSize { get; private set; }
        
        public DSDir(string path, DSDir parent = null)
        {
            System.Diagnostics.Debug.WriteLine("scanning: " + path);
            Subdirs = new List<DSDir>();
            Files = new List<DSFile>();
            this.Parent = parent;
            if (parent == null)
            {
                this.Basepath = path;
                this.Shortpath = "";
            }
            else
            {
                this.Basepath = parent.Basepath;
                this.Shortpath = path.Substring(this.Basepath.Length);
            }

            string[] subdirs_list = Directory.GetDirectories(path);
            string[] files_list = Directory.GetFiles(path);
            foreach (var f in files_list)
            {
                this.Files.Add(new DSFile(f));
            }
            foreach (var sd in subdirs_list)
            {
                this.Subdirs.Add(new DSDir(sd, this));
            }

            FilesSize = 0;
            foreach (var f in Files)
            {
                FilesSize += f.size;
            }
            SubdirectoriesSize = 0;
            foreach (var sd in Subdirs)
            {
                SubdirectoriesSize += sd.Size;
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