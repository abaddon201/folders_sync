using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace folders_sync
{
    class Program
    {
        public static bool test_run = false;
		public static bool fastMode = false;

        static void Main(string[] args)
        {
            Logger.startTime = DateTime.Now;
            FSOps.IsLinux();

			if (args.Count() < 2) {
				Console.WriteLine("Usage: <exe> from to");
				return;
			}

			if (!FSOps.DirectoryExists(args[0])) {
				Console.WriteLine("Source not found");
				return;
			}

			if (args.Count () > 2) {
				//have additional option
				if (args[2]=="--fast") {
					fastMode=true;
				}
			}
			Logger l = new Logger();
			
			l.start(args[0], args[1]);

			if (!FSOps.DirectoryExists (args [1])) {
				Logger.putMessage("Created target folder");
				FSOps.CreateDirectory(args[1]);
			}

            SenderThread st = new SenderThread();

            ReceiverThread rt = new ReceiverThread();
            ReceiverThread.queue = new TransferPipe();

            RemovalThread remt = new RemovalThread();
            RemovalThread.queue = new TransferPipe();

            CopyThread ct = new CopyThread();
            CopyThread.queue = new TransferPipe();

            Md5Thread md5t = new Md5Thread();
            Md5Thread.queue = new TransferPipe();

            remt.start();
            ct.start();
            md5t.start();
            rt.start(args[1]);
            st.start(args[0]);
            st.join();
            rt.join();
            remt.join();
            ct.join();
            l.finish();
            l.join();
            Logger.putRecord(new Logger.LogRecord("Job finished", ""));
            Logger.printStatisticsScreen();

            /*while (!Console.KeyAvailable)
            {
                // Do something
            }*/
        }
    }
}
