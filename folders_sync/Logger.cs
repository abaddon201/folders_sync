using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;

namespace folders_sync {
  class Logger {
    public class LogRecord {
      public string cmd;
      public string param;

      /// <summary>
      /// Returns a list of strings no larger than the max length sent in.
      /// </summary>
      /// <remarks>useful function used to wrap string text for reporting.</remarks>
      /// <param name="text">Text to be wrapped into of List of Strings</param>
      /// <param name="maxLength">Max length you want each line to be.</param>
      /// <returns>List of Strings</returns>
      public static List<String> Wrap (string text, int maxLength) {
        // Return empty list of strings if the text was empty
        if (text.Length == 0)
          return new List<string> ();
        var words = text.Split (' ');
        var lines = new List<string> ();
        var currentLine = "";

        foreach (var currentWord in words) {
          if ((currentLine.Length > maxLength) ||
          ((currentLine.Length + currentWord.Length) > maxLength)) {
            lines.Add (currentLine);
            currentLine = "";
          }

          if (currentLine.Length > 0)
            currentLine += " " + currentWord;
          else
            currentLine += currentWord;
        }

        if (currentLine.Length > 0)
          lines.Add (currentLine);

        return lines;
      }

      public LogRecord (string cmd, string param) {
        this.cmd = cmd;
        this.param = param;
      }

      public void print () {
        Console.Write (cmd);
        if (param != "") {
          List<String> ppp = Wrap (param, Logger.console_width - 22);
          foreach (String str in ppp) {
            Console.CursorLeft = 21;
            Console.WriteLine (str);
          }
        } else {
          Console.WriteLine ("");
        }
      }
    }

    public static int console_width;
    public static int console_height;
    static int items_checked;
    static int items_found;
    static int items_removed;
    static int items_for_removal;
    static int items_copied;
    static int items_for_copy;
    static int items_done;
//FIXME: get too many items done:)
    static ulong items_size_total;
    static ulong items_size_done;
    static int items_md5_done;
    static int items_for_md5;
    static string item_md5_file;
    static string sourceFolder;
    static string targetFolder;
    private static bool stop = false;
    private static List<LogRecord> log_strings = new List<LogRecord> ();
    private static List<string> error_strings = new List<string> ();

    public static void putError (string error) {
      lock (error_strings) {
        error_strings.Add (error);
      }
    }

    public static void putRecord (LogRecord lr) {
      lock (log_strings) {
        log_strings.Add (lr);
      }
    }

    public static void putMessage (string msg) {
      lock (log_strings) {
        log_strings.Add (new LogRecord ("", msg));
      }
    }

    private static void checkIfDone () {
      if (sender_stage_done && (items_done == items_found)) {
        //if we haven't any queued commands, we should stop all threads
        /*if ((Md5Thread.queue.check () == 0) &&
					(CopyThread.queue.check () == 0) &&
					(ReceiverThread.queue.check () == 0)) {*/
        Md5Thread.shouldStop ();
        CopyThread.shouldStop ();
        ReceiverThread.shouldStop ();
        //}
        log_strings.Add (new LogRecord ("Waiting threads...", ""));
        printStatisticsScreen ();
      }
    }

    static string cwd = "";
    object command_lock = new object ();
    static bool sender_stage_done = false;

    public static void putDoneCommand (ECommand cmd, string param, ulong size = 0) {
      //lock (command_lock) {
      switch (cmd) {
      case ECommand.BEGIN_FOLDER:
        cwd = param;
        break;
      case ECommand.END_STAGE:
        sender_stage_done = true;
        lock (log_strings) {
          log_strings.Add (new LogRecord ("Source parsed:", ""));
        }
        break;
      case ECommand.ITEM_DONE:
        items_done++;
        checkIfDone ();
        break;
      case ECommand.GET_MD5:
        items_md5_done++;
        break;
      case ECommand.SAME_MD5:
        items_copied++;
        lock (log_strings) {
          log_strings.Add (new LogRecord ("File attrs copied:", param));
        }
        break;
      case ECommand.COPY_FILE:
        items_copied++;
        items_done++;
        items_size_done += size;
        lock (log_strings) {
          log_strings.Add (new LogRecord ("File copied:", param));
        }
        checkIfDone ();
        break;
      case ECommand.COPY_FOLDER:
        items_copied++;
        items_done++;
        lock (log_strings) {
          log_strings.Add (new LogRecord ("Folder copied:", param));
        }
        checkIfDone ();
        break;
      case ECommand.COPY_FOLDER_ATTRS:
        items_done++;
        lock (log_strings) {
          log_strings.Add (new LogRecord ("Folder attrs copy:", param));
        }
        checkIfDone ();
        break;
      case ECommand.FIX_FOLDER_ATTRS:
        lock (log_strings) {
          log_strings.Add (new LogRecord ("Fixed folder attrs:", param));
        }
        break;
      case ECommand.CHECK_FILE:
      case ECommand.CHECK_FOLDER:
        items_checked++;
        break;
      case ECommand.REMOVE_ITEM:
        items_removed++;
        lock (log_strings) {
          log_strings.Add (new LogRecord ("Item removed:", param));
        }
        break;
      default:
        lock (error_strings) {
          error_strings.Add ("Wrong record in DoneCommand");
        }
        break;
      }
      //}
    }

    public static void putPendingCommand (ECommand cmd, string param, ulong size = 0) {
      //lock (command_lock) {
      switch (cmd) {
      case ECommand.BEGIN_MD5:
        item_md5_file = param;
        break;
      case ECommand.GET_MD5:
        items_for_md5++;
        break;
      case ECommand.SAME_MD5:
        items_for_copy++;
        lock (log_strings) {
          log_strings.Add (new LogRecord ("File for copy attrs:", param));
        }
        break;
      case ECommand.COPY_FILE:
        items_for_copy++;
        items_size_total += size;
        lock (log_strings) {
          log_strings.Add (new LogRecord ("File for copy:", param));
        }
        break;
      case ECommand.COPY_FOLDER:
        items_for_copy++;
        lock (log_strings) {
          log_strings.Add (new LogRecord ("Folder for copy:", param));
        }
        break;
      case ECommand.CHECK_FILE:
      case ECommand.CHECK_FOLDER:
        items_found++;
        break;
      case ECommand.REMOVE_ITEM:
        items_for_removal++;
        lock (log_strings) {
          log_strings.Add (new LogRecord ("Item for remove:", param));
        }
        break;
      default:
        lock (error_strings) {
          error_strings.Add ("Wrong record in PendingCommand");
        }
        break;
      }
      //}
    }

    public static void putFileInfo (string name, ulong size, DateTime creation_time) {
      log_strings.Add (new LogRecord ("Info", name + ": " + size + " " + creation_time));
    }

    public static DateTime startTime;

    public static void printStatisticsScreen () {
      int row = 0;
      Console.Clear ();
      Console.SetCursorPosition (0, 0);
      Console.WriteLine ("Sync from '{0}' to '{1}'", sourceFolder, targetFolder);
      Console.WriteLine ("Statistics:");
      Console.WriteLine ("Items done: {0}/{1}", items_done, items_found);
      Console.WriteLine ("Items checked: {0}/{1}", items_checked, items_found);
      Console.WriteLine ("Items removed: {0}/{1}", items_removed, items_for_removal);
      TimeSpan dt = DateTime.Now - startTime;
      ulong size_to_finish = items_size_total - items_size_done;
      double speed = (double)items_size_done / dt.Ticks;
      TimeSpan eta = new TimeSpan ((long)((double)(items_checked - items_done) * dt.Ticks / items_done));
      if (speed > 0.1) {
        eta += new TimeSpan ((long)(size_to_finish / speed));
      }
      Console.WriteLine ("Items md5: {0}/{1}  File: {2}", items_md5_done, items_for_md5, item_md5_file);
      Console.WriteLine ("Items copied: {0}/{1}: {2}/{3} bytes", items_copied, items_for_copy, items_size_done, items_size_total);
      Console.WriteLine ("Elapsed: {0:dd\\.hh\\:mm\\:ss} ETA: {1:dd\\.hh\\:mm\\:ss}", dt, eta);
      //Console.WriteLine ("Current dir: {0}", cwd);
      Console.WriteLine ("");
      Console.WriteLine ("Errors:");
      row = 10;
      Console.ForegroundColor = ConsoleColor.Red;
      lock (error_strings) {
        foreach (string error in error_strings) {
          Console.WriteLine (error);
          row++;
        }
      }
      Console.ForegroundColor = ConsoleColor.Gray;
      Console.WriteLine ("");
      Console.WriteLine ("Logs:");
      row += 7;
      int count = console_height - row;
      int start = log_strings.Count - count;
      start = (start > 0) ? start : 0;
      count = (count > log_strings.Count) ? log_strings.Count : count;
      lock (log_strings) {
        for (int i = 0; i < count; i++) {
          LogRecord lr = log_strings [i + start];

          /*foreach (LogRecord lr in log_strings)
                    {*/
          lr.print ();
        }
      }
    }

    public static void printStatistics () {
      while (!stop) {
        printStatisticsScreen ();
        Thread.Sleep (500);
      }
    }

    Thread thr;

    public void start (string source, string target) {
      console_width = Console.LargestWindowWidth;
      console_height = Console.LargestWindowHeight;
      Console.SetWindowSize (console_width, console_height);

      stop = false;
      sourceFolder = source;
      targetFolder = target;

      thr = new Thread (printStatistics);
      thr.Start ();
    }

    public void finish () {
      stop = true;
    }

    public void join () {
      thr.Join ();
    }

  }
}
