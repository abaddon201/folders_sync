using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;

namespace folders_sync {
  class TransferPipe {
    Stack mInputStack = new Stack ();
    Stack mOutputStack = new Stack ();

    AutoResetEvent autoEvent = new AutoResetEvent (false);

    public void put (Command o) {
      lock (mInputStack) {
        if (o == null) {
          Logger.putError ("Empty command in queue");
        }
        mInputStack.Push (o);
        autoEvent.Set ();
      }
    }

    /// <summary>
    /// Return items count in both queues
    /// </summary>
    /// <returns></returns>
    public int check () {
      int len = 0;
      lock (mInputStack) {
        len += mInputStack.Count;
      }
      lock (mOutputStack) {
        len += mOutputStack.Count;
      }
      return len;
    }

    public Command get () {
      /*            lock (mOutputStack)
                        {*/
      if (mOutputStack.Count == 0) {
        //empty, must load all
        while (mInputStack.Count == 0) {
          autoEvent.WaitOne ();
        }
        lock (mInputStack) {
          int count = mInputStack.Count;
          for (int i = 0; i < count; ++i) {
            mOutputStack.Push (mInputStack.Pop ());
          }
        }
      }
      return (Command)mOutputStack.Pop ();
      //}
    }

  }
}
