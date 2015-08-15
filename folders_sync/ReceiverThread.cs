using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace folders_sync
{
	class ReceiverThread
	{
		public sealed class ReverseComparer<T> : IComparer<T>
		{
			private readonly IComparer<T> inner;

			public ReverseComparer () : this(null)
			{
			}

			public ReverseComparer (IComparer<T> inner)
			{
				this.inner = inner ?? Comparer<T>.Default;
			}

			int IComparer<T>.Compare (T x, T y)
			{
				return inner.Compare (y, x);
			}
		}

		private static bool should_stop = false;
		public static bool stopped = false;

		public static TransferPipe queue { get; set; }

		string targetFolder;
		private string cur_path;
		private List<string> current_folder_files_source = new List<string> ();
		private List<string> current_folder_files_target = new List<string> ();
		private List<string> no_compare = new List<string> ();
		private SortedDictionary<string, FoldersPair> parsedFolders = new SortedDictionary<string, FoldersPair> (new ReverseComparer<string> ());

		private void getTargetFilesAndFolders (string root)
		{
			current_folder_files_target.Clear ();
			if (!FSOps.DirectoryExists (root)) {
				return;
			}

			string[] subDirs;
			string[] files = null;
			try {
				subDirs = FSOps.GetDirectories (root);
				files = FSOps.GetFiles (root);
			} catch (UnauthorizedAccessException e) {
				Logger.putError (e.Message);
				//FIXME: must react more shitly, or can loose data
				return;
			} catch (System.IO.DirectoryNotFoundException e) {
				Logger.putError (e.Message);
				//FIXME: must react more shitly, or can loose data
				return;
			}

			foreach (string file in files) {
				current_folder_files_target.Add (File.removeParent (file, root));
			}

			foreach (string str in subDirs) {
				current_folder_files_target.Add (File.removeParent (str, root));
			}
		}

		private void checkFilesForRemoval ()
		{
			getTargetFilesAndFolders (cur_path);
			foreach (string name in current_folder_files_target) {
				if (!current_folder_files_source.Contains (name)) {
					//must remove
					Logger.putPendingCommand (ECommand.REMOVE_ITEM, name);
					RemovalThread.queue.put (Command.create (ECommand.REMOVE_ITEM, cur_path + name));

				}
			}
		}

		private void receive ()
		{
			while (true) {
				if (should_stop && (queue.check () == 0)) {
					RemovalThread.shouldStop ();
					stopped = true;
					break;
				}
				Command command = (Command)queue.get ();
				switch (command.command) {
				case ECommand.SAME_MD5:
                        //copy attrs
					if (!Program.test_run) {
						FSOps.FileSetCreationTimeUtc (targetFolder + command.file.path + System.IO.Path.DirectorySeparatorChar + command.file.name, command.file.creationTime);
						FSOps.FileSetLastWriteTimeUtc (targetFolder + command.file.path + System.IO.Path.DirectorySeparatorChar + command.file.name, command.file.modTime);
						FSOps.FileSetAttributes (targetFolder + command.file.path + System.IO.Path.DirectorySeparatorChar + command.file.name, command.file.attrs);
					}
					Logger.putDoneCommand (ECommand.SAME_MD5, targetFolder + command.file.path + System.IO.Path.DirectorySeparatorChar + command.file.name);
					Logger.putDoneCommand (ECommand.ITEM_DONE, targetFolder + command.file.path + System.IO.Path.DirectorySeparatorChar + command.file.name);
					break;
				case ECommand.CHECK_FILE:
					File f = command.file;
					current_folder_files_source.Add (System.IO.Path.DirectorySeparatorChar + f.name);

                        //it's a file
					string relative_path = f.path + System.IO.Path.DirectorySeparatorChar + f.name;
					string full_path = targetFolder + relative_path;
					File f2 = new File (full_path, targetFolder);
					if (FSOps.FileExists (full_path)) {
						if (f.isEqualByAttrs (f2)) {
							if (Program.fastMode || no_compare.Contains (f.path)) {
								Logger.putDoneCommand (ECommand.ITEM_DONE, targetFolder + command.file.path + System.IO.Path.DirectorySeparatorChar + command.file.name);
							} else {
								Logger.putPendingCommand (ECommand.GET_MD5, relative_path);
								Md5Thread.queue.put (Command.create (ECommand.GET_MD5, f, f2));
							}
						} else {
							if (f.size == f2.size) {
								if (!no_compare.Contains (f.path)) {
									Logger.putPendingCommand (ECommand.GET_MD5, relative_path);
									Md5Thread.queue.put (Command.create (ECommand.GET_MD5, f, f2));
								} else {
									if (!Program.test_run) {
										FSOps.FileSetCreationTimeUtc (targetFolder + command.file.path + System.IO.Path.DirectorySeparatorChar + command.file.name, command.file.creationTime);
										FSOps.FileSetLastWriteTimeUtc (targetFolder + command.file.path + System.IO.Path.DirectorySeparatorChar + command.file.name, command.file.modTime);
										FSOps.FileSetAttributes (targetFolder + command.file.path + System.IO.Path.DirectorySeparatorChar + command.file.name, command.file.attrs);
									}
									Logger.putDoneCommand (ECommand.ITEM_DONE, targetFolder + command.file.path + System.IO.Path.DirectorySeparatorChar + command.file.name);
								}

							} else {
								CopyThread.queue.put (Command.create (ECommand.COPY_FILE, f, f2));
								Logger.putPendingCommand (ECommand.COPY_FILE, f.name, f.size);
							}
						}
					} else {
						CopyThread.queue.put (Command.create (ECommand.COPY_FILE, f, f2));
						Logger.putPendingCommand (ECommand.COPY_FILE, f.name, f.size);
					}
					Logger.putDoneCommand (ECommand.CHECK_FILE, f.name);
					break;
				case ECommand.CHECK_FOLDER:
					current_folder_files_source.Add (System.IO.Path.DirectorySeparatorChar + command.file.name);

					string relative_path2 = command.file.path + System.IO.Path.DirectorySeparatorChar + command.file.name;
					string full_path2 = targetFolder + relative_path2;
					File f4 = new File (full_path2, targetFolder);
					Logger.putDoneCommand (ECommand.CHECK_FOLDER, full_path2);
					if (!FSOps.DirectoryExists (full_path2)) {
						CopyThread.queue.put (Command.create (ECommand.COPY_FOLDER, command.file, f4));
						Logger.putPendingCommand (ECommand.COPY_FOLDER, full_path2);
					} else {
						if (!command.file.isEqualByAttrs (f4)) {
							if (!Program.test_run) {
								FSOps.DirectorySetCreationTimeUtc (full_path2, command.file.creationTime);
								FSOps.DirectorySetLastWriteTimeUtc (full_path2, command.file.modTime);
							}
							Logger.putDoneCommand (ECommand.COPY_FOLDER_ATTRS, full_path2);
						} else {
							Logger.putDoneCommand (ECommand.ITEM_DONE, full_path2);
						}
					}
					break;
				case ECommand.BEGIN_FOLDER:
					cur_path = targetFolder + command.file.path;
					parsedFolders.Add (command.file.path, new FoldersPair (command.file.root + command.file.path, cur_path));
					break;
				case ECommand.NOCOMPARE_FOLDER:
					no_compare.Add (command.file.path);
					break;
				case ECommand.END_FOLDER:
					checkFilesForRemoval ();
					current_folder_files_source.Clear ();
					break;
				}
			}
			// FIXME: final stage check all folders for attrs. Must know src and dst path
			foreach (KeyValuePair<string, FoldersPair> de in parsedFolders) {
				if (de.Key != "") {
					bool fix = false;
					/*                    FSOps.DirectoryInfo fi_src = new FSOps.DirectoryInfo(de.Value.Source);
                                        FSOps.DirectoryInfo fi_tgt = new FSOps.DirectoryInfo(de.Value.Target);*/
					FSOps.DirectoryInfo fi_src = new FSOps.DirectoryInfo (de.Value.Source);
					FSOps.DirectoryInfo fi_tgt = new FSOps.DirectoryInfo (de.Value.Target);
					if (!fi_src.Attributes.Equal (fi_tgt.Attributes)) {
						//must fix attrs
						if (!Program.test_run) {
							FSOps.FileSetAttributes (de.Value.Target, fi_src.Attributes);
						}
						fix = true;
					}
					if (fi_src.CreationTimeUtc != fi_tgt.CreationTimeUtc) {
						//must fix attrs
						if (!Program.test_run) {
							FSOps.DirectorySetCreationTimeUtc (de.Value.Target, fi_src.CreationTimeUtc);
						}
						fix = true;
					}
					if (fi_src.LastWriteTimeUtc != fi_tgt.LastWriteTimeUtc) {
						//must fix attrs
						if (!Program.test_run) {
							FSOps.DirectorySetLastWriteTimeUtc (de.Value.Target, fi_src.LastWriteTimeUtc);
						}
						fix = true;
					}
					if (fix) {
						Logger.putDoneCommand (ECommand.FIX_FOLDER_ATTRS, de.Value.Target);
					}
				}
			}
		}

		Thread thr;

		public void start (string path)
		{
			targetFolder = path;

			thr = new Thread (receive);
			thr.Start ();
		}

		public static void shouldStop ()
		{
			should_stop = true;
			queue.put (Command.create (ECommand.NOP));
		}

		public void join ()
		{
			thr.Join ();
		}

	}
}
