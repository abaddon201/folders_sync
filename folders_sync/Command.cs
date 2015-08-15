using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace folders_sync
{
	enum ECommand
	{
		NOP,
		CHECK_FILE,
		CHECK_FOLDER,
		GET_MD5,
		SAME_MD5,
		BEGIN_MD5,
		COPY_FILE,
		COPY_FOLDER,
		COPY_FOLDER_ATTRS,
		FIX_FOLDER_ATTRS,
		BEGIN_FOLDER,
		END_FOLDER,
		NOCOMPARE_FOLDER,
		END_STAGE,

		REMOVE_ITEM,
		REMOVED_ITEM,

		ITEM_DONE
	}

	class Command
	{
		public File file;
		public File file2;
		public ECommand command;

		public Command (ECommand command)
		{
			this.command = command;
		}

		public static Command create (ECommand command)
		{
			Command c = new Command (command);
			switch (command) {
			case ECommand.NOP:
				return c;
			case ECommand.END_STAGE:
				return c;
			default:
				Logger.putError ("Unknown command");
				return null;
			}
		}

		public static Command create (ECommand command, File file, File file2 = null)
		{
			Command c = new Command (command);
			switch (command) {
			case ECommand.SAME_MD5:
				c.file = file;
				return c;
			case ECommand.COPY_FILE:
				c.file = file;
				c.file2 = file2;
				return c;
			case ECommand.COPY_FOLDER:
				c.file = file;
				c.file2 = file2;
				return c;
			case ECommand.GET_MD5:
				c.file = file;
				c.file2 = file2;
				return c;
			default:
				Logger.putError ("Unknown command");
				return null;
			}
		}

		public static Command create (ECommand command, string path, string root = null)
		{
			Command c = new Command (command);
			switch (command) {
			case ECommand.BEGIN_FOLDER:
				c.file = new File ();
				c.file.path = File.removeParent (path, root);
				c.file.root = root;
				return c;
			case ECommand.NOCOMPARE_FOLDER:
				c.file = new File ();
				c.file.path = File.removeParent (path, root);
				c.file.root = root;
				return c;
			case ECommand.END_FOLDER:
				c.file = new File ();
				c.file.path = path;
				return c;
			case ECommand.CHECK_FILE:
				c.file = new File (path, root);
				return c;
			case ECommand.CHECK_FOLDER:
				c.file = new File (path, root);
				return c;
			case ECommand.REMOVE_ITEM:
				c.file = new File (path, root);
				return c;
			default:
				Logger.putError ("Unknown command");
				return null;
			}
		}
	}
}
