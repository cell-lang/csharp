using System;
using System.IO;


namespace Cell.Runtime {
  public delegate Obj SurrObjMapper(int surr);


  public static class AutoMisc {
    public static bool Load(Obj fname, RelAutoBase automaton, RelAutoUpdaterBase updater) {
      try {
        using (Stream stream = new FileStream(fname.GetString(), FileMode.Open)) {
          automaton.LoadState(new StreamReader(stream));
        }
      }
      catch (Exception e) {
        updater.lastException = e;
        return false;
      }

      if (!automaton.FullCheck()) {
        updater.lastException = new Exception("Invalid state");
        return false;
      }

      return true;
    }
  }


  public class InvalidArgumentTypeException : Exception {
    int idx;
    Obj obj;
    string type_user_repr;


    public InvalidArgumentTypeException(int idx, Obj obj, string type_user_repr) {
      this.idx = idx;
      this.obj = obj;
      this.type_user_repr = type_user_repr;
    }

    public override string ToString() {
      return string.Format(
        "The value provided for argument {0} does not belong to its expected type ({1}):\n\n{2}\n",
        idx + 1, type_user_repr, obj.ToString()
      );
    }
  }
}
