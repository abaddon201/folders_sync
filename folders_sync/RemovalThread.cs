using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace folders_sync {
  class RemovalThread {
    Thread thr;

    public static TransferPipe queue { get; set; }

    public static bool stopped = false;
    private static bool should_stop = false;

    private void removal () {
      while (true) {
        if ((should_stop) && (queue.check () == 0)) {
          stopped = true;
          break;
        }

        Command cmd = queue.get ();
        switch (cmd.command) {

        case ECommand.REMOVE_ITEM:
          if (!Program.test_run) {
            if (FSOps.DirectoryExists (cmd.file.full_path) || FSOps.FileExists (cmd.file.full_path)) {
              if (FSOps.IsDirectory (cmd.file.full_path)) {
                FSOps.DeleteDirectory (cmd.file.full_path);
              } else {
                FSOps.DeleteFile (cmd.file.full_path);
              }
            }
          }
          Logger.putDoneCommand (ECommand.REMOVE_ITEM, cmd.file.full_path);
          break;
        case ECommand.NOP:
          break;
        default:
          Logger.putError ("Wrond command in Removal thread");
          break;
        }
      }
    }

    public void start () {
      thr = new Thread (removal);
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
