using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;

namespace folders_sync
{
    class SenderThread
    {
        string sourceFolder;

        public static void traverseFolder(object root)
        {
            // Data structure to hold names of subfolders to be 
            // examined for files.
            Stack<string> dirs = new Stack<string>(20);
            string root_ = (string)root;

            if (!FSOps.DirectoryExists(root_))
            {
                throw new ArgumentException();
            }
            dirs.Push(root_);

            while (dirs.Count > 0)
            {
                string currentDir = dirs.Pop();
                ReceiverThread.queue.put(Command.create(ECommand.BEGIN_FOLDER, currentDir, root_));
				Logger.putDoneCommand(ECommand.BEGIN_FOLDER, currentDir);

                string[] subDirs;
                string[] files = null;
                try
                {
                    subDirs = FSOps.GetDirectories(currentDir);
                    files = FSOps.GetFiles(currentDir);
                }
                // An UnauthorizedAccessException exception will be thrown if we do not have 
                // discovery permission on a folder or file. It may or may not be acceptable  
                // to ignore the exception and continue enumerating the remaining files and  
                // folders. It is also possible (but unlikely) that a DirectoryNotFound exception  
                // will be raised. This will happen if currentDir has been deleted by 
                // another application or thread after our call to Directory.Exists. The  
                // choice of which exceptions to catch depends entirely on the specific task  
                // you are intending to perform and also on how much you know with certainty  
                // about the systems on which this code will run. 
                catch (UnauthorizedAccessException e)
                {
                    Logger.putError(e.Message);
                    continue;
                }
                catch (System.IO.DirectoryNotFoundException e)
                {
                    Logger.putError(e.Message);
                    continue;
                }

			foreach(string f in files) {
				if (f.EndsWith(".no_compare")) {
					ReceiverThread.queue.put(Command.create(ECommand.NOCOMPARE_FOLDER, currentDir, root_));
				}
				}
				// Perform the required action on each file here. 
                // Modify this block to perform your required task. 
                foreach (string file in files)
                {
                    try
                    {
                        ReceiverThread.queue.put(Command.create(ECommand.CHECK_FILE, file, root_));
                        Logger.putPendingCommand(ECommand.CHECK_FILE, file);
                    }
                    catch (System.IO.FileNotFoundException e)
                    {
                        // If file was deleted by a separate application 
                        //  or thread since the call to TraverseTree() 
                        // then just continue.
                        Logger.putError(e.Message);
                        continue;
                    }
                }

                // Push the subdirectories onto the stack for traversal. 
                // This could also be done before handing the files. 
                foreach (string str in subDirs)
                {
                    dirs.Push(str);
                    ReceiverThread.queue.put(Command.create(ECommand.CHECK_FOLDER, str, root_));
                    Logger.putPendingCommand(ECommand.CHECK_FOLDER, str);
                }
                ReceiverThread.queue.put(Command.create(ECommand.END_FOLDER, currentDir, root_));
            }
            Logger.putDoneCommand(ECommand.END_STAGE, null);
        }

        Thread thr;
        public void start(string path)
        {
            sourceFolder = path;
            thr = new Thread(traverseFolder);
            thr.Start(path);
        }

        public void join()
        {
            thr.Join();
        }
    }
}
