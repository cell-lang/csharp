using System;
using System.IO;


namespace Cell.Runtime {
  public class ParsingException : Exception {
    public int line;
    public int col;
    public string text;

    public ParsingException(int line, int col) {
      this.line = line;
      this.col = col;
    }

    public override string ToString() {
      if (text != null) {
        if (OnlyOneLine())
          return string.Format("Parsing error at line {0}, column {1}:\n  {2}", line, col, text);
        else
          return string.Format("Parsing error at line {0}, column {1}:\n\n{2}\n", line, col, text);
      }
      else
        return string.Format("Parsing error at line {0}, column {1}", line, col);
    }

    private bool OnlyOneLine() {
      if (text.Length > 120)
        return false;
      for (int i=0 ; i < text.Length ; i++)
        if (text[i] == '\n')
          return false;
      return true;
    }
  }


  public sealed class CharStream {
    internal const int EOF = -1;

    private TextReader reader;

    private int line = 0;
    private int col = 0;

    const int BUFFER_SIZE = 4096;

    char[] buffer = new char[BUFFER_SIZE];
    int offset = 0;
    int count = 0;

    public CharStream(TextReader reader) {
      this.reader = reader;
    }

    public int Read() {
      if (count == 0) {
        Fill();
        if (count == 0)
          return EOF;
      }

      char ch = buffer[offset++];
      count--;

      if (ch == '\n') {
        line++;
        col = 0;
      }
      else
        col++;

      return ch;
    }

    public int Peek(int idx) {
      if (idx >= count) {
        Fill();
        if (idx >= count)
          return EOF;
      }
      return buffer[offset + idx];
    }

    public int Line() {
      return line;
    }

    public int Column() {
      return col;
    }

    public ParsingException Fail() {
      throw new ParsingException(line + 1, col);
    }

    //////////////////////////////////////////////////////////////////////////////

    private void Fill() {
      if (count == 0) {
        offset = 0;
        count = reader.Read(buffer, 0, BUFFER_SIZE);
        if (count == -1)
          count = 0;
      }
      else {
        if (offset != 0)
          for (int i=0 ; i < count ; i++)
            buffer[i] = buffer[offset+i];
        offset = 0;
        int read = reader.Read(buffer, count, BUFFER_SIZE - count);
        if (read != -1)
          count += read;
      }
    }
  }
}
