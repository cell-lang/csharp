using System;
using System.IO;


namespace Cell.Runtime {
  public class IO {
    public static string DirectorySeparator() {
      return new string(Path.DirectorySeparatorChar, 1);
    }

    public static byte[] ReadAllBytes(string fname) {
      return File.ReadAllBytes(fname);
    }

    public static void WriteAllBytes(string fname, byte[] bytes) {
      File.WriteAllBytes(fname, bytes);
    }

    public static void AppendAllBytes(string fname, byte[] bytes) {
      using (var stream = new FileStream(fname, FileMode.Append, FileAccess.Write, FileShare.None))
        using (var writer = new BinaryWriter(stream))
          writer.Write(bytes);
    }

    public static void StdOutWrite(string str) {
      Console.Write(str);
    }

    public static void StdErrWrite(string str) {
      Console.Error.Write(str);
    }

    public static int StdInRead(int eof, int errCode) {
      try {
        int ch = Console.Read();
        return ch != -1 ? ch : eof;
      }
      catch (Exception) {
        return errCode;
      }
    }

    private static readonly System.DateTime Jan1st1970 = new System.DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static long UnixTimeMs() {
      return (long) (System.DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
    }

    public static Exception Exit(int code) {
      Environment.Exit(code);
      throw new Exception();
    }

    public static string StackTrace() {
      return Environment.StackTrace;
    }

    ////////////////////////////////////////////////////////////////////////////

    public static DataWriter StringDataWriter() {
      return new DataWriter(new StringWriter());
    }

    public static DataWriter FileDataWriter(string filename) {
      return new DataWriter(new StreamWriter(filename));
    }

    public static DataWriter StdOutDataWriter() {
      return new DataWriter(Console.Out);
    }

    public static DataWriter DataWriter(Stream stream) {
      // if (!(stream is BufferedStream))
      //   stream = new BufferedStream(stream);
      return new DataWriter(new StreamWriter(stream));
    }
  }

  //////////////////////////////////////////////////////////////////////////////
  //////////////////////////////////////////////////////////////////////////////

  public class DataWriter {
    protected TextWriter writer;
    private bool newLine = true;
    private int indentLevel = 0;


    public DataWriter(TextWriter writer) {
      this.writer = writer;
    }

    public bool IsNewLine() {
      return newLine;
    }

    public string Output() {
      if (writer is StringWriter)
        return writer.ToString();
      else
        throw new NotSupportedException();
    }

    public void Indent() {
      indentLevel++;
    }

    public void Unindent() {
      indentLevel--;
    }

    public void Flush() {
      writer.Flush();
    }

    public void NewLine() {
      writer.Write('\n');
      WriteSpaces(2 * indentLevel);
      newLine = true;
    }

    public void IndentedNewLine() {
      Indent();
      NewLine();
    }

    public void UnindentedNewLine() {
      Unindent();
      NewLine();
    }

    public void Write(char ch) {
      writer.Write(ch);
      newLine = false;
    }

    public void Write(string str) {
      writer.Write(str);
      newLine = false;
    }

    public void Write(long value) {
      writer.Write(value);
      newLine = false;
    }

    public void Write(double value) {
      string str = value.ToString();
      writer.Write(str);
      newLine = false;
      for (int i=0 ; i < str.Length ; i++) {
        char ch = str[i];
        if (ch == '.' | ch == 'e')
          return;
      }
      writer.Write(".0");
    }

    private const int SPACES_BLOCK_SIZE = 20;
    private static readonly char[] spaces = Array.Repeat(' ', SPACES_BLOCK_SIZE);

    public void WriteSpaces(int count) {
      while (count >= SPACES_BLOCK_SIZE) {
        writer.Write(spaces);
        count -= SPACES_BLOCK_SIZE;
      }
      writer.Write(spaces, 0, count);
    }
  }
}