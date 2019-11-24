using System.Collections.Generic;


namespace Cell.Runtime {
  public class TokenStreamProcessor {
    private TokenStream tokens;
    private int line = -1;
    private int col = -1;

    protected TokenStreamProcessor(TokenStream tokens) {
      this.tokens = tokens;
    }

    public void CheckEof() {
      if (!tokens.Eof())
        Fail();
    }

    public TokenType PeekType() {
      return tokens.PeekType();
    }

    protected ParsingException Fail() {
      return tokens.Fail();
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public void Bookmark() {
      line = tokens.Line();
      col = tokens.Column();
    }

    public ParsingException FailAtBookmark() {
      Debug.Assert(line != -1 & col != -1);
      throw new ParsingException(line + 1, col + 1);
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public long ReadLong() {
      return tokens.ReadLong();
    }

    public double ReadDouble() {
      return tokens.ReadDouble();
    }

    public ushort ReadSymbol() {
      return tokens.ReadSymbol();
    }

    public Obj ReadString() {
      return tokens.ReadString();
    }

    public Obj ReadLiteral() {
      return tokens.ReadLiteral();
    }

    public ushort TryReadingLabel() {
      return tokens.TryReadingLabel();
    }

    public bool NextIsCloseBracket() {
      return tokens.NextIs(']');
    }

    public void ConsumeArrow() {
      tokens.Consume('-', '>');
    }

    public void ConsumeCloseBracket() {
      tokens.Consume(']');
    }

    public void ConsumeClosePar() {
      tokens.Consume(')');
    }

    public void ConsumeColon() {
      tokens.Consume(':');
    }

    public void ConsumeComma() {
      tokens.Consume(',');
    }

    public void ConsumeOpenBracket() {
      tokens.Consume('[');
    }

    public void ConsumeOpenPar() {
      tokens.Consume('(');
    }

    public void ConsumeSemicolon() {
      tokens.Consume(';');
    }

    public bool TryConsumingSemicolon() {
      return tokens.TryConsuming(';');
    }

    public bool TryConsumingArrow() {
      return tokens.TryConsuming('-', '>');
    }

    public bool TryConsumingComma() {
      return tokens.TryConsuming(',');
    }

    public bool TryConsumingOpenBracket() {
      return tokens.TryConsuming('[');
    }

    protected bool TryConsumingOpenPar() {
      return tokens.TryConsuming('(');
    }

    protected bool TryConsumingClosePar() {
      return tokens.TryConsuming(')');
    }

    protected bool TryConsumingCloseBracket() {
      return tokens.TryConsuming(']');
    }
  }

  ////////////////////////////////////////////////////////////////////////////////

  public abstract class Parser : TokenStreamProcessor {
    public Parser(TokenStream tokens) : base(tokens) {

    }

    public void SkipValue() {
      ParseObj(); //## IMPLEMENT FOR REAL
    }

    public Obj ParseObj() {
      TokenType type = PeekType();

      switch (type) {
        case TokenType.Comma:
        case TokenType.Colon:
        case TokenType.Semicolon:
        case TokenType.Arrow:
        case TokenType.ClosePar:
        case TokenType.CloseBracket:
          throw Fail();

        case TokenType.Int:
          return IntObj.Get(ReadLong());

        case TokenType.Float:
          return new FloatObj(ReadDouble());

        case TokenType.Symbol:
          return ParseSymbOrTaggedObj();

        case TokenType.OpenPar:
          ConsumeOpenPar();
          return ParseSeqOrRecord(PeekType());

        case TokenType.OpenBracket:
          return ParseUnordColl();

        case TokenType.String:
          return ReadString();

        case TokenType.Literal:
          return ReadLiteral();

        default:
          throw ErrorHandler.InternalFail(); // Unreachable code
      }
    }

    ////////////////////////////////////////////////////////////////////////////////

    // The opening parenthesis must have already been consumed
    Obj ParseSeqOrRecord(TokenType firstTokenType) {
      if (firstTokenType == TokenType.Symbol) {
        ushort labelId = TryReadingLabel();
        if (labelId != SymbObj.InvalidSymbId)
          return ParseRec(labelId);
      }
      else if (firstTokenType == TokenType.ClosePar) {
        ConsumeClosePar();
        return EmptySeqObj.singleton;
      }

      return ParseNeSeq();
    }

    ////////////////////////////////////////////////////////////////////////////////

    // The opening parenthesis must have already been consumed
    Obj ParseNeSeq() {
      List<Obj> elts = new List<Obj>();

      do {
        elts.Add(ParseObj());
      } while (TryConsumingComma());

      ConsumeClosePar();
      return Builder.CreateSeq(elts.ToArray());
    }

    ////////////////////////////////////////////////////////////////////////////////

    // The opening parenthesis and the first label including
    // its trailing colon must have already been consumed
    Obj ParseRec(ushort firstLabelId) {
      ushort[] labels = new ushort[8];
      Obj[] values = new Obj[8];

      labels[0] = firstLabelId;
      values[0] = ParseObj();

      int i = 1;
      while (TryConsumingComma()) {
        if (i >= labels.Length) {
          labels = Array.Extend(labels, 2 * labels.Length);
          values = Array.Extend(values, 2 * values.Length);
        }
        ushort labelId = TryReadingLabel();
        if (labelId == SymbObj.InvalidSymbId)
          throw Fail();
        //## BAD BAD BAD: WITH A LARGE RECORD...
        for (int j=0 ; j < i ; j++)
          if (labels[j] == labelId)
            throw Fail();
        labels[i] = labelId;
        values[i++] = ParseObj();
      }
      ConsumeClosePar();

      Obj[] labelObjs = new Obj[i];
      for (int j=0 ; j < i ; j++)
        labelObjs[j] = SymbObj.Get(labels[j]);

      //## IT WOULD BE BETTER TO CREATE A RecordObj, BUT THE LABELS WOULD NEED TO BE SORTED FIRST
      return Builder.CreateMap(labelObjs, values, i);
    }

    ////////////////////////////////////////////////////////////////////////////////

    Obj ParseSymbOrTaggedObj() {
      ushort symbId = ReadSymbol();

      if (!TryConsumingOpenPar())
        return SymbObj.Get(symbId);

      TokenType type = PeekType();

      Obj firstValue = null;

      if (type == TokenType.Int) {
        long value = ReadLong();
        if (TryConsumingClosePar())
          return Builder.CreateTaggedIntObj(symbId, value);
        // Here we've consumed the opening parenthesis and the integer
        // Since the opening parenthesis was not follow by a label,
        // we're dealing with a sequence, possibly a sequence of integers
        //## OPTIMIZE FOR SEQUENCES OF INTEGERS
        firstValue = IntObj.Get(value);
      }
      else if (type == TokenType.Symbol) {
        ushort labelId = TryReadingLabel();
        if (labelId != SymbObj.InvalidSymbId)
          return CreateTaggedObj(symbId, ParseRec(labelId));
        firstValue = ParseObj();
      }
      else {
        firstValue = ParseObj();
      }

      if (TryConsumingClosePar())
        return CreateTaggedObj(symbId, firstValue);

      Obj[] elts = new Obj[16];
      elts[0] = firstValue;

      int i = 1;
      while (TryConsumingComma()) {
        if (i >= elts.Length)
          elts = Array.Extend(elts, 2 * elts.Length);
        elts[i++] = ParseObj();
      }
      ConsumeClosePar();

      return CreateTaggedObj(symbId, Builder.CreateSeq(elts, i));
    }

    protected abstract Obj CreateTaggedObj(ushort tagId, Obj obj);

    ////////////////////////////////////////////////////////////////////////////////

    Obj ParseUnordColl() {
      ConsumeOpenBracket();

      if (TryConsumingCloseBracket())
        return EmptyRelObj.singleton;

      List<Obj> objs = new List<Obj>();
      do {
        objs.Add(ParseObj());
      } while (TryConsumingComma());

      if (TryConsumingCloseBracket())
        return Builder.CreateSet(objs.ToArray());

      int len = objs.Count;

      if (len == 1) {
        List<Obj> values = new List<Obj>();
        ConsumeArrow();
        values.Add(ParseObj());
        while (TryConsumingComma()) {
          objs.Add(ParseObj());
          ConsumeArrow();
          values.Add(ParseObj());
        }
        ConsumeCloseBracket();
        return Builder.CreateBinRel(objs.ToArray(), values.ToArray()); // Here we create a binary relation rather than a map
      }

      if (len == 2) {
        List<Obj> col1 = new List<Obj>();
        List<Obj> col2 = new List<Obj>();
        col1.Add(objs[0]);
        col2.Add(objs[1]);
        while (!TryConsumingCloseBracket()) {
          ConsumeSemicolon();
          col1.Add(ParseObj());
          ConsumeComma();
          col2.Add(ParseObj());
        }
        return Builder.CreateBinRel(col1.ToArray(), col2.ToArray());
      }

      if (len == 3) {
        List<Obj> col1 = new List<Obj>();
        List<Obj> col2 = new List<Obj>();
        List<Obj> col3 = new List<Obj>();
        col1.Add(objs[0]);
        col2.Add(objs[1]);
        col3.Add(objs[2]);
        while (!TryConsumingCloseBracket()) {
          ConsumeSemicolon();
          col1.Add(ParseObj());
          ConsumeComma();
          col2.Add(ParseObj());
          ConsumeComma();
          col3.Add(ParseObj());
        }
        return Builder.CreateTernRel(col1.ToArray(), col2.ToArray(), col3.ToArray());
      }

      throw Fail();
    }
  }
}
