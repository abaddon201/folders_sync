using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace folders_sync {
  class File {
    ulong md5_1;
    ulong md5_2;

    public ulong size { get; set; }

    public string name { get; set; }

    public string path { get; set; }

    public string root;

    public string full_path { get { return root + path + System.IO.Path.DirectorySeparatorChar + name; } }

    public FSOps.FileAttributes attrs { get; set; }

    public DateTime creationTime { get; set; }

    public DateTime modTime { get; set; }

    public void setMd5 (ulong md1, ulong md2) {
      md5_1 = md1;
      md5_2 = md2;
    }

    public FileStream open () {
      if (FSOps.FileExists (root + path + System.IO.Path.DirectorySeparatorChar + name)) {
        return FSOps.FileOpenRead (root + path + System.IO.Path.DirectorySeparatorChar + name);
      }
      return null;
    }

    public void calcMd5 () {
      if (FSOps.FileExists (root + path + System.IO.Path.DirectorySeparatorChar + name)) {
        using (FileStream fs = FSOps.FileOpenRead (root + path + System.IO.Path.DirectorySeparatorChar + name)) {
          MD5 md5 = new MD5CryptoServiceProvider ();
          fs.Position = 0;
          byte[] checkSum = md5.ComputeHash (fs);
          fs.Close ();
          md5_1 = BitConverter.ToUInt64 (checkSum, 0);
          md5_2 = BitConverter.ToUInt64 (checkSum, 8);
          //string result = BitConverter.ToString(checkSum).Replace("-", String.Empty);
        }
      }
    }

    public bool checkMd5 (File f) {
      return ((md5_1 == f.md5_1) && (md5_2 == f.md5_2));
    }

    public bool checkMd5 (ulong md1, ulong md2) {
      return ((md5_1 == md1) && (md5_2 == md2));
    }

    public void readInfo () {
      if (FSOps.FileExists (path)) {
        FSOps.FileInfo fi = new FSOps.FileInfo (path);
        creationTime = fi.CreationTimeUtc;
        modTime = fi.LastWriteTimeUtc;
        attrs = fi.Attributes;
        size = (ulong)fi.Length;
      } else if (FSOps.DirectoryExists (path)) {
        FSOps.FileInfo fi = new FSOps.FileInfo (path);
        creationTime = fi.CreationTimeUtc;
        modTime = fi.LastWriteTimeUtc;
        attrs = fi.Attributes;
      }
      name = FSOps.GetFileName (path);
      path = FSOps.GetDirectoryName (path);
    }

    public bool isEqualByAttrs (File f) {
      if (FSOps.FileExists (f.full_path)) {
        return ((size == f.size) &&
        (attrs.Equal (f.attrs)) &&
        (creationTime == f.creationTime) &&
        (modTime == f.modTime));
      } else if (FSOps.DirectoryExists (f.full_path)) {
        return (
          (attrs.Equal (f.attrs)) &&
          (creationTime == f.creationTime) &&
          (modTime == f.modTime));
      } else {
        return false;
      }
    }

    public void printInfo () {
      Logger.putFileInfo (name, size, creationTime);
      //Console.WriteLine("{0}: {1} {2}", name, size, creationTime);
    }

    /*        public File(string path, string name)
                {
                    this.path = path;
                    this.name = name;
                    readInfo();
                }*/

    public File (string path, string root) {
      this.path = path;
      this.root = root;
      readInfo ();
      removeParent ();
    }

    public File () {
    }

    public void removeParent () {
      if (root != null) {
        int index = path.IndexOf (root);
        path = (index < 0)
                    ? path
                    : path.Remove (index, root.Length);
      }

    }
    /*        public void removeParent(string parent)
                {
                    int index = path.IndexOf(parent);
                    path = (index < 0)
                        ? path
                        : path.Remove(index, parent.Length);

                }
        */
    public static string removeParent (string path, string parent) {
      int index = path.IndexOf (parent);
      return (index < 0)
                ? path
                : path.Remove (index, parent.Length);
    }
  }
}
