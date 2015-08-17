using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace folders_sync {
  class Md5Thread {
    public static TransferPipe queue { get; set; }

    public static bool stopped = false;
    private static bool should_stop = false;

    private void receive_comands () {
      while (true) {
        if ((should_stop) && (queue.check () == 0)) {
          stopped = true;
          break;
        }
        Command cmd = queue.get ();
        switch (cmd.command) {
        case ECommand.GET_MD5:
          bool is_equal = true;
					/*cmd.file.calcMd5();
                        cmd.file2.calcMd5();
                        is_equal=cmd.file.checkMd5(cmd.file2);
                        */
          Logger.putPendingCommand (ECommand.BEGIN_MD5, cmd.file2.full_path);
          FileStream fs1 = cmd.file.open ();
          FileStream fs2 = cmd.file2.open ();
          if ((fs2 == null) || (fs1 == null)) {
            Logger.putError ("One of comparison file are missed: " + cmd.file2.full_path);
            is_equal = false;
            if (fs1 != null) {
              fs1.Dispose ();
            }
            if (fs2 != null) {
              fs2.Dispose ();
            }
          } else {
            int res = 0, res2;
            byte[] buff1 = new byte[1024 * 1024];
            byte[] buff2 = new byte[1024 * 1024];
            do {
              res = fs1.Read (buff1, 0, 1024 * 1024);
              res2 = fs2.Read (buff2, 0, 1024 * 1024);
              if (res != res2) {
                is_equal = false;
                break;
              }
              for (int i = 0; i < res; ++i) {
                if (buff1 [i] != buff2 [i]) {
                  is_equal = false;
                  break;
                }
              }
            } while (is_equal && (res != 0));
            buff1 = null;
            buff2 = null;
            fs1.Dispose ();
            fs2.Dispose ();
          }
          if (!is_equal) {
            CopyThread.queue.put (Command.create (ECommand.COPY_FILE, cmd.file, cmd.file2));
            Logger.putPendingCommand (ECommand.COPY_FILE, cmd.file.name, cmd.file.size);
          } else {
            if (!cmd.file.isEqualByAttrs (cmd.file2)) {
              ReceiverThread.queue.put (Command.create (ECommand.SAME_MD5, cmd.file));
              Logger.putPendingCommand (ECommand.SAME_MD5, cmd.file.name);
            } else {
              Logger.putDoneCommand (ECommand.ITEM_DONE, cmd.file.name);
            }
          }
          Logger.putDoneCommand (ECommand.GET_MD5, cmd.file.name);
          break;
        case ECommand.NOP:
          break;
        default:
          Logger.putError ("Wrond command in Md5 thread");
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
