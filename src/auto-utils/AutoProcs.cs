using Exception = System.Exception;


namespace Cell.Runtime {
  public class AutoProcs {
    public static Obj Error_P(RelAutoBase automaton, RelAutoUpdaterBase updater, object env) {
      Exception e = updater.lastException;
      string msg = "";
      if (e != null) {
        if (e is KeyViolationException || e is ForeignKeyViolationException) {
          msg = e.ToString();
        }
        else {
          DataWriter writer = IO.StringDataWriter();
          writer.Write(e.ToString());
          writer.Flush();
          msg = writer.Output();
        }
      }
      return Conversions.StringToObj(msg);
    }

    public static Obj Save_P(Obj fname, RelAutoBase automaton, RelAutoUpdaterBase updater, object env) {
      try {
        DataWriter writer = IO.FileDataWriter(fname.GetString());
        automaton.WriteState(writer);
        return SymbObj.Get(true);
      }
      catch (Exception e) {
        updater.lastException = e;
        return SymbObj.Get(false);
      }
    }
  }
}
