using System;


namespace Cell.Runtime {
  public enum TokenType {
    Comma,
    Colon,
    Semicolon,
    Arrow,
    OpenPar,
    ClosePar,
    OpenBracket,
    CloseBracket,
    Int,
    Float,
    Symbol,
    String,
    Literal,
    Whatever
  };


  public interface TokenStream {
    long ReadLong();
    double ReadDouble();
    ushort ReadSymbol();
    Obj ReadString();
    Obj ReadLiteral();

    ushort TryReadingLabel();

    TokenType PeekType();

    bool NextIs(char ch);

    void Consume(char ch);
    void Consume(char ch1, char ch2);

    bool TryConsuming(char ch);
    bool TryConsuming(char ch1, char ch2);

    bool Eof();

    int Line();
    int Column();

    ParsingException Fail();
  }


  public class CharStreamProcessor {
    private CharStream src;
    private int currChar;
    private int offset = 0;


    protected CharStreamProcessor(CharStream src) {
      this.src = src;
      currChar = src.Read();
    }

    protected int Read() {
      int result = currChar;
      currChar = src.Read();
      offset++;
      return result;
    }

    protected void Skip() {
      currChar = src.Read();
      offset++;
    }

    protected void Skip(int count) {
      for (int i=0 ; i < count ; i++)
        currChar = src.Read();
      offset += count;
    }

    protected int Peek() {
      return currChar;
    }

    protected int Peek(int idx) {
      return idx == 0 ? currChar : src.Peek(idx - 1);
    }

    protected int Offset() {
      return offset;
    }

    //////////////////////////////////////////////////////////////////////////////

    public int Line() {
      return src.Line();
    }

    public int Column() {
      return src.Column();
    }

    public ParsingException Fail() {
      return src.Fail();
    }
  }

  ////////////////////////////////////////////////////////////////////////////////
  ////////////////////////////////////////////////////////////////////////////////

  public sealed class Tokenizer : CharStreamProcessor, TokenStream {
    const int BUFFER_SIZE = 256;
    byte[] buffer = new byte[BUFFER_SIZE]; // For reading symbols, it's not part of the state of the class


    public Tokenizer(CharStream src) : base(src) {

    }

    public TokenType PeekType() {
      ConsumeWhiteSpace();

      if (NextIsDigit())
        return NumberType(1);

      if (NextIsLower())
        return TokenType.Symbol;

      switch (Peek()) {
        case '"':     return TokenType.String;
        case ',':     return TokenType.Comma;
        case ':':     return TokenType.Colon;
        case ';':     return TokenType.Semicolon;
        case '(':     return TokenType.OpenPar;
        case ')':     return TokenType.ClosePar;
        case '[':     return TokenType.OpenBracket;
        case ']':     return TokenType.CloseBracket;
        case '`':     return TokenType.Literal;
        case '-':     int ch = Peek(1);
                      if (ch == '>')
                        return TokenType.Arrow;
                      if (IsDigit(ch))
                        return NumberType(2);
                      throw Fail();
      }

      throw Fail();
    }

    public long ReadLong() {
      bool negate = TryConsuming('-');
      long natVal = ReadNat();
      return negate ? -natVal : natVal;
    }

    public double ReadDouble() {
      bool negate = TryConsuming('-');
      double value = ReadNat();

      if (TryConsuming('.')) {
        int start = Offset();
        long decIntVal = ReadNat();
        value += ((double) decIntVal) / Math.Pow(10, Offset() - start);
      }

      if (TryConsuming('e')) {
        bool negExp = TryConsuming('-');
        CheckNextIsDigit();
        long expValue = ReadNat();
        value *= Math.Pow(10, negExp ? -expValue : expValue);
      }

      Check(!NextIsLower());
      return negate ? -value : value;
    }

    public ushort ReadSymbol() {
      Debug.Assert(NextIsLower());

      // long encWord1 = SymbTableFastCache.EncodedLetter(Read());
      // for (int i=0 ; i < 9 ; i++) {
      //   if (NextIsLower()) {
      //     int code =  SymbTableFastCache.EncodedLetter(Read());
      //     encWord1 = (encWord1 << 6) + code;
      //   }
      //   else if (NextIs('_')) {
      //     Skip();
      //     if (NextIsLower()) {
      //       int code = SymbTableFastCache.EncodedUnderscoredLetter(Read());
      //       encWord1 = (encWord1 << 6) + code;
      //     }
      //     else if (NextIsDigit()) {
      //       encWord1 = (encWord1 << 6) + SymbTableFastCache.ENCODED_UNDERSCORE;
      //     }
      //     else
      //       throw Fail();
      //   }
      //   else if (NextIsDigit()) {
      //     int code = SymbTableFastCache.EncodedDigit(Read());
      //     encWord1 = (encWord1 << 6) + code;
      //   }
      //   else {
      //     return SymbTableFastCache.EncToIdx(encWord1);
      //   }
      // }

      // if (!NextIsAlphaNum() & !NextIs('_'))
      //   return SymbTableFastCache.EncToIdx(encWord1);

      // long encWord2 = 0;
      // for (int i=0 ; i < 10 ; i++) {
      //   if (NextIsLower()) {
      //     int code =  SymbTableFastCache.EncodedLetter(Read());
      //     encWord2 = (encWord2 << 6) + code;
      //   }
      //   else if (NextIs('_')) {
      //     Skip();
      //     if (NextIsLower()) {
      //       int code = SymbTableFastCache.EncodedUnderscoredLetter(Read());
      //       encWord2 = (encWord2 << 6) + code;
      //     }
      //     else if (NextIsDigit()) {
      //       encWord2 = (encWord2 << 6) + SymbTableFastCache.ENCODED_UNDERSCORE;
      //     }
      //     else
      //       throw Fail();
      //   }
      //   else if (NextIsDigit()) {
      //     int code = SymbTableFastCache.EncodedDigit(Read());
      //     encWord2 = (encWord2 << 6) + code;
      //   }
      //   else {
      //     return SymbTableFastCache.EncToIdx(encWord1, encWord2);
      //   }
      // }

      // if (!NextIsAlphaNum() & !NextIs('_'))
      //   return SymbTableFastCache.EncToIdx(encWord1, encWord2);


      ulong encWord1 = ReadEncSymbWord();
      if (!NextIsAlphaNum() & !NextIs('_'))
        return SymbTableFastCache.EncToIdx(encWord1);

      ulong encWord2 = ReadEncSymbWord();
      if (!NextIsAlphaNum() & !NextIs('_'))
        return SymbTableFastCache.EncToIdx(encWord1, encWord2);

      ulong encWord3 = ReadEncSymbWord();
      if (!NextIsAlphaNum() & !NextIs('_'))
        return SymbTableFastCache.EncToIdx(encWord1, encWord2, encWord3);

      ulong[] encWords = new ulong[8];
      encWords[0] = encWord1;
      encWords[1] = encWord2;
      encWords[2] = encWord3;

      for (int i=3 ; i < 64 ; i++) {
        if (i >= encWords.Length)
          encWords = Array.Extend(encWords, 2 * encWords.Length);
        encWords[i] = ReadEncSymbWord();
        if (!NextIsAlphaNum() & !NextIs('_'))
          return SymbTableFastCache.EncToIdx(encWords, i+1);
      }

      // The symbol was too long, we give up
      throw Fail();
    }

    private ulong ReadEncSymbWord() {
      ulong encWord = 0;
      for (int i=0 ; i < 10 ; i++) {
        if (NextIsLower()) {
          byte code =  SymbTableFastCache.EncodedLetter(Read());
          encWord = (encWord << 6) + code;
        }
        else if (NextIs('_')) {
          Skip();
          if (NextIsLower()) {
            byte code = SymbTableFastCache.EncodedUnderscoredLetter(Read());
            encWord = (encWord << 6) + code;
          }
          else if (NextIsDigit()) {
            encWord = (encWord << 6) + SymbTableFastCache.ENCODED_UNDERSCORE;
          }
          else
            throw Fail();
        }
        else if (NextIsDigit()) {
          byte code = SymbTableFastCache.EncodedDigit(Read());
          encWord = (encWord << 6) + code;
        }
        else {
          return encWord;
        }
      }
      return encWord;
    }

    public Obj ReadString() {
      Debug.Assert(NextIs('"'));

      int len = 0;
      char[] chars = new char[32];

      Read();
      for ( ; ; ) {
        int ch = Read();
        Check(Miscellanea.IsBMPCodePoint(ch));

        if (ch == '\\') {
          ch = Read();
          if (ch == '\\' | ch == '"') {
            // Nothing to do here
          }
          else if (ch == 'n') {
            ch = '\n';
          }
          else if (ch == 't') {
            ch = '\t';
          }
          else {
            Check(IsHex(ch)); //## THIS ACTUALLY FAILS ONE CHARACTER AHEAD
            int d3 = HexDigitValue(ch);
            int d2 = HexDigitValue(ReadHex());
            int d1 = HexDigitValue(ReadHex());
            int d0 = HexDigitValue(ReadHex());
            ch = (char) (4096 * d3 + 256 * d2 + 16 * d1 + d0);
          }
        }
        else if (ch == '"')
          break;

        if (len >= chars.Length)
          chars = Array.Extend(chars, 2 * chars.Length);
        chars[len++] = (char) ch;
      }

      return Builder.CreateString(chars, len);
    }

    public Obj ReadLiteral() {
      Debug.Assert(NextIs('`'));

      Read();
      int ch1 = Read();
      int ch2 = Read();
      if (ch1 == '\\') {
        Read('`');
        if (ch2 == 'n')
          return IntObj.Get('\n');
        else if (ch2 == '`')
          return IntObj.Get('`');
        else if (ch2 == 't')
          return IntObj.Get('\t');
        else if (ch2 == '\\')
          return IntObj.Get('\\');
        else
          throw Fail();
      }
      else if (ch2 == '`') {
        return IntObj.Get(ch1);
      }
      else {
        Check(IsDigit(ch1) & IsDigit(ch2));

        int year = 1000 * (ch1 - '0') + 100 * (ch2 - '0') + 10 * ReadDigit() + ReadDigit();
        Read('-');
        int month = 10 * ReadDigit() + ReadDigit();
        Read('-');
        int day = 10 * ReadDigit() + ReadDigit();

        Check(DateTimeUtils.IsValidDate(year, month, day));

        int daysSinceEpoc = DateTimeUtils.DaysSinceEpoc(year, month, day);

        if (TryReading('`'))
          return Builder.CreateTaggedIntObj(SymbObj.DateSymbId, daysSinceEpoc);

        Read(' ');
        int hours = 10 * ReadDigit() + ReadDigit();
        Check(hours >= 0 & hours < 24);
        Read(':');
        int minutes = 10 * ReadDigit() + ReadDigit();
        Check(minutes >= 0 & minutes < 60);
        Read(':');
        int seconds = 10 * ReadDigit() + ReadDigit();
        Check(seconds >= 0 & minutes < 60);

        int nanosecs = 0;
        if (TryReading('.')) {
          int pow10 = 100000000;
          for (int i=0 ; i < 9 && NextIsDigit() ; i++) {
            nanosecs = nanosecs + pow10 * ReadDigit();
            pow10 /= 10;
          }
        }

        Read('`');

        long dayTimeNs = 1000000000L * (60L * (60L * hours + minutes) + seconds) + nanosecs;
        Check(DateTimeUtils.IsWithinRange(daysSinceEpoc, dayTimeNs));

        long epocTimeNs = DateTimeUtils.EpocTimeNs(daysSinceEpoc, dayTimeNs);
        return Builder.CreateTaggedIntObj(SymbObj.TimeSymbId, epocTimeNs);
      }
    }

    public ushort TryReadingLabel() {
      ConsumeWhiteSpace();

      if (!NextIsAlphaNum())
        return SymbObj.InvalidSymbId;

      buffer[0] = (byte) Peek();
      for (int i=1 ; i < BUFFER_SIZE ; i++) {
        int ch = Peek(i);

        if (IsAlphaNum(ch)) {
          buffer[i] = (byte) ch;
        }
        else if (ch == '_') {
          buffer[i++] = (byte) ch;
          ch = Peek(i);
          if (IsAlphaNum(ch))
            buffer[i] = (byte) ch;
          else
            throw Fail();
        }
        else if (ch == ':') {
          Skip(i+1);
          return SymbObj.BytesToIdx(buffer, i);
        }
        else {
          return SymbObj.InvalidSymbId;
        }
      }

      // The label was too long, we give up
      throw Fail();
    }

    public bool NextIs(char ch) {
      ConsumeWhiteSpace();
      return Peek() == ch;
    }

    public void Consume(char ch) {
      ConsumeWhiteSpace();
      Check(Peek() == ch);
      Skip(1);
    }

    public void Consume(char ch1, char ch2) {
      ConsumeWhiteSpace();
      Check(Peek() == ch1);
      Skip(1);
      Check(Peek() == ch2);
      Skip(1);
    }

    public bool TryConsuming(char ch) {
      ConsumeWhiteSpace();
      if (Peek() == ch) {
        Skip(1);
        return true;
      }
      else
        return false;
    }

    public bool TryConsuming(char ch1, char ch2) {
      ConsumeWhiteSpace();
      if (Peek() == ch1 && Peek(1) == ch2) {
        Skip(2);
        return true;
      }
      return false;
    }

    public bool Eof() {
      ConsumeWhiteSpace();
      return Peek() == CharStream.EOF;
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    private TokenType NumberType(int idx) {
      while (IsDigit(Peek(idx)))
        idx++;
      int ch = Peek(idx);
      return ch == '.' | ch == 'e' ? TokenType.Float : TokenType.Int;
    }

    private long ReadNat() {
      int count = 0;
      long value = 0;
      while (NextIsDigit()) {
        int digit = Read() - '0';
        if (++count == 19) {
          Check(value < 922337203685477580L | (value == 922337203685477580L & digit <= 7));
          Check(!NextIsDigit());
        }
        value = 10 * value + digit;
      }
      return value;
    }

    private void ConsumeWhiteSpace() {
      while (IsWhiteSpace(Peek()))
        Skip(1);
    }

    private void Read(char ch) {
      Check(Read() == ch);
    }

    private int ReadHex() {
      Check(NextIsHex());
      return Read();
    }

    private int ReadDigit() {
      int ch = Read();
      Check(IsDigit(ch));
      return ch - '0';
    }

    private bool TryReading(char ch) {
      if (Peek() == ch) {
        Read();
        return true;
      }
      else
        return false;
    }

    private bool NextIsDigit() {
      return IsDigit(Peek());
    }

    private bool NextIsHex() {
      return IsHex(Peek());
    }

    private bool NextIsLower() {
      return IsLower(Peek());
    }

    private bool NextIsAlphaNum() {
      return IsAlphaNum(Peek());
    }

    private void CheckNextIs(char ch) {
      Check(Peek() == ch);
    }

    private void CheckNextIsDigit() {
      Check(IsDigit(Peek()));
    }

    private void CheckNextIsHex() {
      Check(IsHex(Peek()));
    }

    private void CheckNextIsAlphaNum() {
      Check(IsAlphaNum(Peek()));
    }

    private void CheckNextIsPrintable() {
      Check(IsPrintable(Peek()));
    }

    private void Check(bool cond) {
      if (!cond)
        Fail();
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    private static bool IsDigit(int ch) {
      return ch >= '0' & ch <= '9';
    }

    private static bool IsHex(int ch) {
      return (ch >= '0' & ch <= '9') | (ch >= 'a' & ch <= 'f');
    }

    private static bool IsLower(int ch) {
      return ch >= 'a' & ch <= 'z';
    }

    private static bool IsAlphaNum(int ch) {
      return IsDigit(ch) | IsLower(ch);
    }

    private static bool IsPrintable(int ch) {
      return ch >= ' ' & ch <= '~';
    }

    private static bool IsWhiteSpace(int ch) {
      return ch == ' ' | ch == '\t' | ch == '\n' | ch == '\r';
    }

    private static int HexDigitValue(int ch) {
      return ch - (IsDigit(ch) ? '0' : 'a');
    }
  }
}
