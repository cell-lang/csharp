using System;
using System.Collections.Generic;


namespace Cell.Runtime {
  public class Debug {
    public static bool debugMode = false;
    public static bool debugFlag = false;

    private static int      stackDepth = 0;
    private static string[] fnNamesStack = new string[100];
    private static Obj[][]  argsStack = new Obj[100][];

    private static List<Obj> filedObjs = new List<Obj>();

    ////////////////////////////////////////////////////////////////////////////

    public static void PushCallInfo(string fnName, Obj[] args) {
      if (stackDepth < 100) {
        fnNamesStack[stackDepth] = fnName;
        argsStack[stackDepth]    = args;
      }
      stackDepth++;
    }

    public static void PopCallInfo() {
      stackDepth--;
    }

    ////////////////////////////////////////////////////////////////////////////

    public static void Assert(bool cond) {
      Assert(cond, null);
    }

    public static void Assert(bool cond, string message) {
      if (!cond) {
        ErrWriteLine("Assertion failed" + (message != null ? ": " + message : ""));
        if (stackDepth > 0)
          PrintCallStack();
        else
          ErrWriteLine(Environment.StackTrace);
        IO.Exit(1);
      }
    }

    ////////////////////////////////////////////////////////////////////////////

    public static void Trace(string msg) {
      if (debugMode)
        ErrWriteLine(msg);
    }

    public static void Trace(string msg, string varName, Obj obj) {
      if (debugMode) {
        ErrWriteLine(msg);
        DumpVar(varName, obj);
      }
    }

    public static void Trace(string msg, string var1Name, Obj obj1, string var2Name, Obj obj2) {
      if (debugMode) {
        ErrWriteLine(msg);
        DumpVar(var1Name, obj1);
        DumpVar(var2Name, obj2);
      }
    }

    ////////////////////////////////////////////////////////////////////////////

    public static void OnSoftFailure(bool insideTransaction) {
      if (debugMode & !insideTransaction)
        PrintCallStack();
    }

    public static void OnImplFailure(string msg) {
      if (msg != null)
        ErrWriteLine(msg + "\n");
      PrintCallStack();
    }

    public static void OnHardFailure() {
      PrintCallStack();
    }

    public static void OnInternalFailure(Obj obj) {
      ErrWriteLine("Internal error!\n");
      // PrintCallStack();
      if (obj != null) {
        // DumpVar("this", obj);
        ErrWrite(string.Format("this.GetClass().GetSimpleName() = {0}\n", obj.GetType().FullName));
      }
      ErrWriteLine(Environment.StackTrace);
      IO.Exit(1);
    }

    ////////////////////////////////////////////////////////////////////////////

    public static void PrintAssertionFailedMsg(string file, int line, string text) {
      if (text == null)
        ErrWrite(string.Format("\nAssertion failed. File: {0}, line: {1}\n\n\n", file, line));
      else
        ErrWrite(string.Format("\nAssertion failed: {0}\nFile: {1}, line: {2}\n\n\n", text, file, line));
    }

    public static void PrintFailReachedMsg(string file, int line) {
      ErrWrite(string.Format("\nFail statement reached. File: {0}, line: {1}\n\n\n", file, line));
    }

    public static void DumpVar(string name, Obj obj) {
      try {
        string str = PrintedObjOrFilename(obj, true);
        ErrWrite(string.Format("{0} = {1}\n\n", name, str));
      }
      catch (Exception) {

      }
    }

    ////////////////////////////////////////////////////////////////////////////

    public static void PrintCallStack() {
      if (stackDepth > 0) {
        ErrWriteLine("\nCall stack:\n");
        int size = stackDepth <= fnNamesStack.Length ? stackDepth : fnNamesStack.Length;
        for (int i=0 ; i < size ; i++)
          ErrWriteLine("  " + fnNamesStack[i]);
        string outFnName = "debug" + IO.DirectorySeparator() + "stack-trace.txt";
        string outNativeFnName = "debug" + IO.DirectorySeparator() + "dotnet-stack-trace.txt";
        ErrWriteLine("\nNow trying to write a full dump of the stack to " + outFnName);
        try {
          DataWriter writer = IO.FileDataWriter(outFnName);
          for (int i=0 ; i < size ; i++)
            PrintStackFrame(i, writer);
          writer.Write('\n');
          writer.Flush();

          writer = IO.FileDataWriter(outNativeFnName);
          writer.Write(IO.StackTrace());
          writer.Write('\n');
          writer.Flush();
        }
        catch (Exception) {
          ErrWriteLine(
            string.Format(
              "Could not write a dump of the stack to {0}. Did you create the \"debug\" directory?",
              outFnName
            )
          );
        }
      }
    }

    static void PrintStackFrame(int frameIdx, DataWriter writer) {
      Obj[] args = argsStack[frameIdx];
      writer.Write(fnNamesStack[frameIdx] + "(");
      if (args != null) {
        writer.Write("\n");
        for (int i=0 ; i < args.Length ; i++)
          PrintIndentedArg(args[i], i == args.Length - 1, writer);
      }
      writer.Write(")\n\n");
      writer.Flush();
    }

    static void PrintIndentedArg(Obj arg, bool isLast, DataWriter writer) {
      string str = arg.IsBlankObj() ? "<closure>" : PrintedObjOrFilename(arg, false);
      for (int i=0 ; i < str.Length ; i++) {
        if (i == 0 || str[i] == '\n')
          writer.Write("  ");
        writer.Write(str[i]);
      }
      if (!isLast)
        writer.Write(',');
      writer.Write("\n");
      writer.Flush();
    }

    ////////////////////////////////////////////////////////////////////////////

    static string PrintedObjOrFilename(Obj obj, bool addPath) {
      string path = addPath ? "debug" + IO.DirectorySeparator() : "";

      for (int i=0 ; i < filedObjs.Count ; i++)
        if (filedObjs[i].IsEq(obj))
          return string.Format("<{0}obj-{1}.txt>", path, i);

      if (ObjPrinter.PrintSizeFits(obj, 200))
        return obj.ToString();

      int idx = filedObjs.Count;
      string outFnName = string.Format("debug{0}obj-{1}.txt", IO.DirectorySeparator(), idx);
      ObjPrinter.Print(obj, IO.FileDataWriter(outFnName), 100);

      filedObjs.Add(obj);

      return string.Format("<{0}obj-{1}.txt>", path, idx);
    }

    ////////////////////////////////////////////////////////////////////////////

    private static void ErrWrite(string msg) {
      IO.StdErrWrite(msg);
    }

    private static void ErrWriteLine(string msg) {
      IO.StdErrWrite(msg);
      IO.StdErrWrite("\n");
    }

    ////////////////////////////////////////////////////////////////////////////

    public static void OnProcessEnd() {

    }
  }
}
