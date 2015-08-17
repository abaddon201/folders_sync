using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace folders_sync {
  class CopyThread {
    #region copy_folders

    public static void CopyDirectory (string source, string target) {
      var stack = new Stack<FoldersPair> ();
      stack.Push (new FoldersPair (source, target));

      while (stack.Count > 0) {
        var folders = stack.Pop ();
        if (!FSOps.DirectoryExists (folders.Target)) {
          FSOps.CreateDirectory (folders.Target);
        }
        /*foreach (var file in Delimon.Win32.IO.Directory.GetFiles(folders.Source, "*.*"))
                {
                    queue.put(Command.create(ECommand.COPY_FILE, new File(file, folders.Source), new File(Delimon.Win32.IO.Path.Combine(folders.Target, Delimon.Win32.IO.Path.GetFileName(file)), folders.Target)));
                    Logger.putPendingCommand(ECommand.COPY_FILE, file);
                    //Delimon.Win32.IO.File.Copy(file, Delimon.Win32.IO.Path.Combine(folders.Target, Delimon.Win32.IO.Path.GetFileName(file)), true);
                }

                foreach (var folder in Delimon.Win32.IO.Directory.GetDirectories(folders.Source))
                {
                    stack.Push(new Folders(folder, Delimon.Win32.IO.Path.Combine(folders.Target, Delimon.Win32.IO.Path.GetFileName(folder))));
                }*/
      }
    }

    #endregion

    public static TransferPipe queue { get; set; }

    public static bool stopped = false;
    private static bool should_stop = false;

    private void receive_comands () {
      while (true) {
        if (should_stop && (queue.check () == 0)) {
          stopped = true;
          break;
        }
        Command cmd = queue.get ();
        switch (cmd.command) {
        case ECommand.COPY_FILE:
          if (!Program.test_run) {
            FSOps.FileCopy (cmd.file.full_path, cmd.file2.full_path, true);
            FSOps.FileSetCreationTimeUtc (cmd.file2.full_path, cmd.file.creationTime);
            FSOps.FileSetLastWriteTimeUtc (cmd.file2.full_path, cmd.file.modTime);
            FSOps.FileSetAttributes (cmd.file2.full_path, cmd.file.attrs);

          }
          Logger.putDoneCommand (ECommand.COPY_FILE, cmd.file2.full_path, cmd.file.size);
          break;
        case ECommand.COPY_FOLDER:
          if (!Program.test_run) {
            CopyDirectory (cmd.file.full_path, cmd.file2.full_path);
            FSOps.DirectorySetCreationTimeUtc (cmd.file2.full_path, cmd.file.creationTime);
            FSOps.DirectorySetLastWriteTimeUtc (cmd.file2.full_path, cmd.file.modTime);
          }
          Logger.putDoneCommand (ECommand.COPY_FOLDER, cmd.file2.full_path);
          break;
        case ECommand.NOP:
          break;
        default:
          Logger.putError ("Wrond command in Copy thread");
          break;
        }
      }
    }

    Thread thr;

    public void start () {
      thr = new Thread (receive_comands);
      thr.Start ();
    }

    public static void shouldStop () {
      should_stop = true;
      queue.put (Command.create (ECommand.NOP));
    }

    public void join () {
      thr.Join ();
    }

  }
}
