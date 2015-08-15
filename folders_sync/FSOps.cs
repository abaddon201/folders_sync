using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace folders_sync
{
	class FSOps
	{
		public class FileAttributes
		{
			System.IO.FileAttributes sys_attrs;
			Delimon.Win32.IO.FileAttributes delim_attrs;

			public object attrs {
				get {
					if (isLinux) {
						return sys_attrs;
					} else {
						return delim_attrs;
					}
				}
				set {
					if (isLinux) {
						sys_attrs = (System.IO.FileAttributes)value;
					} else {
						delim_attrs = (Delimon.Win32.IO.FileAttributes)value;
					}
				}
			}

			public bool Equal (FileAttributes at2)
			{
				if (isLinux) {
					return sys_attrs == at2.sys_attrs;
				} else {
					return delim_attrs == at2.delim_attrs;
				}

			}

			public FileAttributes (object attrs)
			{
				if (isLinux) {
					sys_attrs = (System.IO.FileAttributes)attrs;
				} else {
					delim_attrs = (Delimon.Win32.IO.FileAttributes)attrs;
				}
			}
		}

		public class FileInfo
		{
			System.IO.FileInfo sys_fi;
			Delimon.Win32.IO.FileInfo delim_fi;

			public object fileInfo {
				get {
					if (isLinux) {
						return sys_fi;
					} else {
						return delim_fi;
					}
				}
				set {
					if (isLinux) {
						sys_fi = (System.IO.FileInfo)value;
					} else {
						delim_fi = (Delimon.Win32.IO.FileInfo)value;
					}
				}
			}

			public DateTime CreationTimeUtc {
				get {
					if (isLinux) {
						return sys_fi.CreationTimeUtc;
					} else {
						return delim_fi.CreationTimeUtc;
					}
				}
			}

			public DateTime LastWriteTimeUtc {
				get {
					if (isLinux) {
						return sys_fi.LastWriteTimeUtc;
					} else {
						return delim_fi.LastWriteTimeUtc;
					}
				}
			}

			public FileAttributes Attributes {
				get {
					if (isLinux) {
						return new FileAttributes (sys_fi.Attributes);
					} else {
						return new FileAttributes (delim_fi.Attributes);
					}
				}
			}

			public string Name {
				get {
					if (isLinux) {
						return sys_fi.Name;
					} else {
						return delim_fi.Name;
					}
				}
			}

			public string DirectoryName {
				get {
					if (isLinux) {
						return sys_fi.DirectoryName;
					} else {
						return delim_fi.DirectoryName;
					}
				}
			}

			public long Length {
				get {
					if (isLinux) {
						return sys_fi.Length;
					} else {
						return delim_fi.Length;
					}
				}
			}

			public FileInfo (string path)
			{
				if (isLinux) {
					sys_fi = new System.IO.FileInfo (path);
				} else {
					delim_fi = new Delimon.Win32.IO.FileInfo (path);
				}
			}
		}

		public class DirectoryInfo
		{
			System.IO.DirectoryInfo sys_fi;
			Delimon.Win32.IO.DirectoryInfo delim_fi;

			public object directoryInfo {
				get {
					if (isLinux) {
						return sys_fi;
					} else {
						return delim_fi;
					}
				}
				set {
					if (isLinux) {
						sys_fi = (System.IO.DirectoryInfo)value;
					} else {
						delim_fi = (Delimon.Win32.IO.DirectoryInfo)value;
					}
				}
			}

			public DateTime CreationTimeUtc {
				get {
					if (isLinux) {
						return sys_fi.CreationTimeUtc;
					} else {
						return delim_fi.CreationTimeUtc;
					}
				}
			}

			public DateTime LastWriteTimeUtc {
				get {
					if (isLinux) {
						return sys_fi.LastWriteTimeUtc;
					} else {
						return delim_fi.LastWriteTimeUtc;
					}
				}
			}

			public FileAttributes Attributes {
				get {
					if (isLinux) {
						return new FileAttributes (sys_fi.Attributes);
					} else {
						return new FileAttributes (delim_fi.Attributes);
					}
				}
			}

			public string Name {
				get {
					if (isLinux) {
						return sys_fi.Name;
					} else {
						return delim_fi.Name;
					}
				}
			}

			public string DirectoryName {
				get {
					if (isLinux) {
						return sys_fi.Parent.FullName;
					} else {
						return delim_fi.Parent.FullName;
					}
				}
			}

			public DirectoryInfo (string path)
			{
				if (isLinux) {
					sys_fi = new System.IO.DirectoryInfo (path);
				} else {
					delim_fi = new Delimon.Win32.IO.DirectoryInfo (path);
				}
			}
		}


		private static bool isLinux;

		public static void IsLinux ()
		{
			int p = (int)Environment.OSVersion.Platform;
			isLinux = (p == 4) || (p == 6) || (p == 128);
		}

		public static bool DirectoryExists (string path)
		{
			if (isLinux) {
				return System.IO.Directory.Exists (path);
			} else {
				return Delimon.Win32.IO.Directory.Exists (path);
			}
		}

		public static void CreateDirectory (string path)
		{
			if (isLinux) {
				System.IO.Directory.CreateDirectory (path);
			} else {
				Delimon.Win32.IO.Directory.CreateDirectory (path);
			}
		}

		public static bool FileExists (string path)
		{
			if (isLinux) {
				return System.IO.File.Exists (path);
			} else {
				return Delimon.Win32.IO.File.Exists (path);
			}
		}

		public static string GetFileName (string path)
		{
			if (isLinux) {
				return System.IO.Path.GetFileName (path);
			} else {
				return Delimon.Win32.IO.Path.GetFileName (path);
			}
		}

		public static string GetDirectoryName (string path)
		{
			if (isLinux) {
				return System.IO.Path.GetDirectoryName (path);
			} else {
				return Delimon.Win32.IO.Path.GetDirectoryName (path);
			}
		}

		public static System.IO.FileStream FileOpenRead (string path)
		{
			if (isLinux) {
				return System.IO.File.OpenRead (path);
			} else {
				return Delimon.Win32.IO.File.OpenRead (path);
			}
		}

		public static string[] GetDirectories (string path)
		{
			if (isLinux) {
				return System.IO.Directory.GetDirectories (path);
			} else {
				return Delimon.Win32.IO.Directory.GetDirectories (path);
			}
		}

		public static string[] GetFiles (string path)
		{
			if (isLinux) {
				return System.IO.Directory.GetFiles (path);
			} else {
				return Delimon.Win32.IO.Directory.GetFiles (path);
			}
		}

		public static bool IsDirectory (string path)
		{
			if (isLinux) {
				System.IO.FileAttributes attr = System.IO.File.GetAttributes (path);

				//detect whether its a directory or file
				if ((attr & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory)
					return true;
				else
					return false;
			} else {
				Delimon.Win32.IO.FileAttributes attr = Delimon.Win32.IO.File.GetAttributes (path);

				//detect whether its a directory or file
				if ((attr & Delimon.Win32.IO.FileAttributes.Directory) == Delimon.Win32.IO.FileAttributes.Directory)
					return true;
				else
					return false;
			}
		}

		public static bool DeleteDirectory (string path)
		{
			if (isLinux) {
				System.IO.Directory.Delete (path, true);
				return true;
			} else {
				return Delimon.Win32.IO.Directory.Delete (path, true);
			}
		}

		public static bool DeleteFile (string path)
		{
			if (isLinux) {
				System.IO.File.Delete (path);
				return true;
			} else {
				return Delimon.Win32.IO.File.Delete (path);
			}
		}

		public static void FileCopy (string src, string dst, bool overwrite, bool gentle=false)
		{
			if (isLinux) {
				bool was_ro = false;
				System.IO.FileInfo fi3 = null;
				if (!gentle) {
					fi3 = new System.IO.FileInfo (dst);
					if ((fi3.Attributes & System.IO.FileAttributes.ReadOnly) == System.IO.FileAttributes.ReadOnly) {
						was_ro = true;
						fi3.Attributes &= ~System.IO.FileAttributes.ReadOnly;
					}
				}
				System.IO.File.Copy (src, dst, overwrite);
				if (was_ro) {
					fi3.Attributes = fi3.Attributes | System.IO.FileAttributes.ReadOnly;
				}
			} else {
				bool was_ro = false;
				Delimon.Win32.IO.FileInfo fi3 = null;
				if (!gentle) {
					fi3 = new Delimon.Win32.IO.FileInfo (dst);
					if ((fi3.Attributes & Delimon.Win32.IO.FileAttributes.ReadOnly) == Delimon.Win32.IO.FileAttributes.ReadOnly) {
						was_ro = true;
						fi3.Attributes &= ~Delimon.Win32.IO.FileAttributes.ReadOnly;
					}
				}
				Delimon.Win32.IO.File.Copy (src, dst, overwrite);
				if (was_ro) {
					fi3.Attributes = fi3.Attributes | Delimon.Win32.IO.FileAttributes.ReadOnly;
				}
			}
		}

		public static void FileSetCreationTimeUtc (string path, DateTime time, bool gentle=false)
		{
			if (isLinux) {
				bool was_ro = false;
				System.IO.FileInfo fi3 = null;
				if (!gentle) {
					fi3 = new System.IO.FileInfo (path);
					if ((fi3.Attributes & System.IO.FileAttributes.ReadOnly) == System.IO.FileAttributes.ReadOnly) {
						was_ro = true;
						fi3.Attributes &= ~System.IO.FileAttributes.ReadOnly;
					}
				}
				System.IO.File.SetCreationTimeUtc (path, time);
				if (was_ro) {
					fi3.Attributes = fi3.Attributes | System.IO.FileAttributes.ReadOnly;
				}
			} else {
				bool was_ro = false;
				Delimon.Win32.IO.FileInfo fi3 = null;
				if (!gentle) {
					fi3 = new Delimon.Win32.IO.FileInfo (path);
					if ((fi3.Attributes & Delimon.Win32.IO.FileAttributes.ReadOnly) == Delimon.Win32.IO.FileAttributes.ReadOnly) {
						was_ro = true;
						fi3.Attributes &= ~Delimon.Win32.IO.FileAttributes.ReadOnly;
					}
				}
				Delimon.Win32.IO.File.SetCreationTimeUtc (path, time);
				if (was_ro) {
					fi3.Attributes = fi3.Attributes | Delimon.Win32.IO.FileAttributes.ReadOnly;
				}
			}
		}

		public static void FileSetLastWriteTimeUtc (string path, DateTime time, bool gentle=false)
		{
			if (isLinux) {
				bool was_ro = false;
				System.IO.FileInfo fi3 = null;
				if (!gentle) {
					fi3 = new System.IO.FileInfo (path);
					if ((fi3.Attributes & System.IO.FileAttributes.ReadOnly) == System.IO.FileAttributes.ReadOnly) {
						was_ro = true;
						fi3.Attributes &= ~System.IO.FileAttributes.ReadOnly;
					}
				}
				System.IO.File.SetLastWriteTimeUtc (path, time);
				if (was_ro) {
					fi3.Attributes = fi3.Attributes | System.IO.FileAttributes.ReadOnly;
				}
			} else {
				bool was_ro = false;
				Delimon.Win32.IO.FileInfo fi3 = null;
				if (!gentle) {
					fi3 = new Delimon.Win32.IO.FileInfo (path);
					if ((fi3.Attributes & Delimon.Win32.IO.FileAttributes.ReadOnly) == Delimon.Win32.IO.FileAttributes.ReadOnly) {
						was_ro = true;
						fi3.Attributes &= ~Delimon.Win32.IO.FileAttributes.ReadOnly;
					}
				}
				Delimon.Win32.IO.File.SetLastWriteTimeUtc (path, time);
				if (was_ro) {
					fi3.Attributes = fi3.Attributes | Delimon.Win32.IO.FileAttributes.ReadOnly;
				}
			}
		}

		public static void FileSetAttributes (string path, FileAttributes attrs)
		{
			if (isLinux) {
				System.IO.File.SetAttributes (path, (System.IO.FileAttributes)attrs.attrs);
			} else {
				Delimon.Win32.IO.File.SetAttributes (path, (Delimon.Win32.IO.FileAttributes)attrs.attrs);
			}
		}

		public static void DirectorySetCreationTimeUtc (string path, DateTime time)
		{
			if (isLinux) {
				System.IO.Directory.SetCreationTimeUtc (path, time);
			} else {
				Delimon.Win32.IO.Directory.SetCreationTimeUtc (path, time);
			}
		}

		public static void DirectorySetLastWriteTimeUtc (string path, DateTime time)
		{
			if (isLinux) {
				System.IO.Directory.SetLastWriteTimeUtc (path, time);
			} else {
				Delimon.Win32.IO.Directory.SetLastWriteTimeUtc (path, time);
			}
		}



	}
}
