using Exception = System.Exception;


namespace Cell.Runtime {
  public abstract class RelAutoBase {
    public abstract void WriteState(DataWriter writer);
    public abstract void LoadState(System.IO.TextReader reader);
    public abstract bool FullCheck();
  }

  public abstract class RelAutoUpdaterBase {
    public Exception lastException;
  }
}
