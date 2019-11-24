using System.Collections.Generic;


namespace Cell.Runtime {
  public class ObjPrinter : ObjVisitor {
    private DataWriter writer;
    private HashSet<Obj> multilineObjs;


    private ObjPrinter(DataWriter writer, HashSet<Obj> multilineObjs) {
      this.writer = writer;
      this.multilineObjs = multilineObjs;
    }

    public static void Print(Obj obj) {
      DataWriter writer = IO.StdOutDataWriter();
      Print(obj, writer, 90);
      writer.Write('\n');
      writer.Flush();
    }

    public static void Print(Obj obj, DataWriter writer, int maxLineLen) {
      HashSet<Obj> multilineObjs = Multiliner.MultilineObjs(obj, maxLineLen);
      ObjPrinter printer = new ObjPrinter(writer, multilineObjs);
      obj.Visit(printer);
      // writer.Flush();
    }

    public static void Print(Obj obj, DataWriter writer) {
      // ObjPrinter printer = new ObjPrinter(writer, new HashSet<Obj>(new IdentityEqualityComparer<Obj>()));
      ObjPrinter printer = new ObjPrinter(writer, new HashSet<Obj>());
      obj.Visit(printer);
      // writer.Flush();
    }

    public static bool PrintSizeFits(Obj obj, int maxSize) {
      return !Multiliner.IsMultiline(obj, maxSize);
    }

    ////////////////////////////////////////////////////////////////////////////

    private bool IsMultiline(Obj obj) {
      return multilineObjs.Contains(obj);
    }

    ////////////////////////////////////////////////////////////////////////////

    public void ArrayObj(ArrayObj obj) {
      NeSeqObj(obj);
    }

    public void ArraySliceObj(ArraySliceObj obj) {
      NeSeqObj(obj);
    }

    private void NeSeqObj(NeSeqObj obj) {
      int len = obj.GetSize();

      writer.Write('(');

      if (IsMultiline(obj)) {
        writer.Indent();

        // If we are on a fresh line, we start writing the first element
        // after the opening bracket, with just a space in between
        // Otherwise we start on the next line
        if (writer.IsNewLine())
          writer.Write(' ');
        else
          writer.NewLine();

        for (int i=0 ; i < len ; i++) {
          if (i > 0) {
            writer.Write(',');
            writer.NewLine();
          }
          obj.GetObjAt(i).Visit(this);
        }

        writer.UnindentedNewLine();
      }
      else {
        for (int i=0 ; i < len ; i++) {
          if (i > 0)
            writer.Write(", ");
          obj.GetObjAt(i).Visit(this);
        }
      }

      writer.Write(')');
    }

    public void BlankObj(BlankObj obj) {
      writer.Write("Blank");
    }

    public void EmptyRelObj(EmptyRelObj obj) {
      writer.Write("[]");
    }

    public void EmptySeqObj(EmptySeqObj obj) {
      writer.Write("()");
    }

    public void FloatArrayObj(FloatArrayObj obj) {
      NeFloatSeqObj(obj);
    }

    public void FloatArraySliceObj(FloatArraySliceObj obj) {
      NeFloatSeqObj(obj);
    }

    private void NeFloatSeqObj(NeFloatSeqObj obj) {
      int len = obj.GetSize();

      writer.Write('(');

      if (IsMultiline(obj)) {
        writer.Indent();

        // If we are on a fresh line, we start writing the first element
        // after the opening bracket, with just a space in between
        // Otherwise we start on the next line
        if (writer.IsNewLine())
          writer.Write(' ');
        else
          writer.NewLine();

        for (int i=0 ; i < len ; i++) {
          if (i > 0) {
            writer.Write(',');
            writer.NewLine();
          }
          writer.Write(obj.GetDoubleAt(i));
        }

        writer.UnindentedNewLine();
      }
      else {
        for (int i=0 ; i < len ; i++) {
          if (i > 0)
            writer.Write(", ");
          writer.Write(obj.GetDoubleAt(i));
        }
      }

      writer.Write(')');
    }

    public void FloatObj(FloatObj obj) {
      writer.Write(obj.GetDouble());
    }

    public void IntArrayObj(IntArrayObj obj) {
      NeIntSeqObj(obj);
    }

    public void IntArraySliceObj(IntArraySliceObj obj) {
      NeIntSeqObj(obj);
    }

    public void SignedByteArrayObj(SignedByteArrayObj obj) {
      NeIntSeqObj(obj);
    }

    public void SignedByteArraySliceObj(SignedByteArraySliceObj obj) {
      NeIntSeqObj(obj);
    }

    public void UnsignedByteArrayObj(UnsignedByteArrayObj obj) {
      NeIntSeqObj(obj);
    }

    public void UnsignedByteArraySliceObj(UnsignedByteArraySliceObj obj) {
      NeIntSeqObj(obj);
    }

    public void ShortArrayObj(ShortArrayObj obj) {
      NeIntSeqObj(obj);
    }

    public void ShortArraySliceObj(ShortArraySliceObj obj) {
      NeIntSeqObj(obj);
    }

    public void Int32ArrayObj(Int32ArrayObj obj) {
      NeIntSeqObj(obj);
    }

    public void Int32ArraySliceObj(Int32ArraySliceObj obj) {
      NeIntSeqObj(obj);
    }

    private void NeIntSeqObj(NeIntSeqObj obj) {
      int len = obj.GetSize();

      writer.Write('(');

      if (IsMultiline(obj)) {
        writer.Indent();

        // If we are on a fresh line, we start writing the first element
        // after the opening bracket, with just a space in between
        // Otherwise we start on the next line
        if (writer.IsNewLine())
          writer.Write(' ');
        else
          writer.NewLine();

        for (int i=0 ; i < len ; i++) {
          if (i > 0) {
            writer.Write(',');
            writer.NewLine();
          }
          writer.Write(obj.GetLongAt(i));
        }

        writer.UnindentedNewLine();
      }
      else {
        for (int i=0 ; i < len ; i++) {
          if (i > 0)
            writer.Write(", ");
          writer.Write(obj.GetLongAt(i));
        }
      }

      writer.Write(')');
    }

    public void IntObj(IntObj obj) {
      writer.Write(obj.GetLong());
    }

    public void NeBinRelObj(NeBinRelObj obj) {
      if (!obj.IsNeMap())
        StandardBinRelObj(obj);
      else if (obj.IsNeRecord())
        Record(obj);
      else
        Map(obj);
    }

    private void Record(NeBinRelObj obj) {
      Obj[] values = obj.Col2();
      int len = values.Length;

      ushort[] labels;
      if (obj is RecordObj) {
        RecordObj recObj = (RecordObj) obj;
        labels = recObj.fieldIds;
      }
      else {
        labels = new ushort[len];
        Obj[] fields = obj.Col1();
        for (int i=0 ; i < len ; i++)
          labels[i] = fields[i].GetSymbId();
      }

      writer.Write('(');

      if (IsMultiline(obj)) {
        int maxLabelLen = 0;
        for (int i=0 ; i < len ; i++) {
          int labelLen = Cell.Runtime.SymbObj.IdxToStr(labels[i]).Length;
          if (labelLen > maxLabelLen)
            maxLabelLen = labelLen;
        }

        int[] idxs = SortedIdxs(labels, values);

        writer.Indent();
        if (writer.IsNewLine())
          writer.Write(' ');
        else
          writer.NewLine();

        for (int i=0 ; i < len ; i++) {
          int idx = idxs[i];
          string label = Cell.Runtime.SymbObj.IdxToStr(labels[idx]);
          writer.Write(label);
          writer.Write(':');

          Obj value = values[idx];
          if (IsMultiline(value)) {
            writer.IndentedNewLine();
            value.Visit(this);
            writer.Unindent();
          }
          else {
            writer.WriteSpaces(maxLabelLen - label.Length + 1);
            value.Visit(this);
          }

          if (i < len - 1)
            writer.Write(',');
          else
            writer.Unindent();
          writer.NewLine();
        }
      }
      else {
        for (int i=0 ; i < len ; i++) {
          if (i > 0)
            writer.Write(", ");
          writer.Write(Cell.Runtime.SymbObj.IdxToStr(labels[i]));
          writer.Write(": ");
          values[i].Visit(this);
        }
      }

      writer.Write(')');
    }

    private int[] SortedIdxs(ushort[] labels, Obj[] values) {
      int count = labels.Length;
      int[] idxs = new int[count];
      int lowIdx = 0;
      int highIdx = count - 1;
      for (int i=0 ; i < count ; i++)
        if (IsMultiline(values[i]))
          idxs[highIdx--] = i;
        else
          idxs[lowIdx++] = i;
      Debug.Assert(lowIdx == highIdx + 1);
      return idxs;
    }

    private void Map(NeBinRelObj obj) {
      Obj[] col1 = obj.Col1();
      Obj[] col2 = obj.Col2();
      int len = col1.Length;

      writer.Write('[');

      if (IsMultiline(obj)) {
        writer.Indent();
        if (writer.IsNewLine())
          writer.Write(' ');
        else
          writer.NewLine();

        for (int i=0 ; i < len ; i++) {
          Obj arg1 = col1[i];
          Obj arg2 = col2[i];

          arg1.Visit(this);
          writer.Write(" ->");

          if (IsMultiline(arg2)) {
            writer.IndentedNewLine();
            arg2.Visit(this);
            writer.Unindent();
          }
          else {
            writer.Write(' ');
            arg2.Visit(this);
          }

          if (i < len - 1)
            writer.Write(',');
          else
            writer.Unindent();
          writer.NewLine();
        }
      }
      else {
        for (int i=0 ; i < len ; i++) {
          if (i > 0)
            writer.Write(", ");

          col1[i].Visit(this);
          writer.Write(" -> ");
          col2[i].Visit(this);
        }
      }

      writer.Write(']');
    }

    private void StandardBinRelObj(NeBinRelObj obj) {
      Obj[] col1 = obj.Col1();
      Obj[] col2 = obj.Col2();
      int len = col1.Length;

      writer.Write('[');

      if (IsMultiline(obj)) {
        writer.Indent();
        if (writer.IsNewLine())
          writer.Write(' ');
        else
          writer.NewLine();

        for (int i=0 ; i < len ; i++) {
          Obj arg1 = col1[i];
          Obj arg2 = col2[i];

          bool multiline = IsMultiline(arg1) || IsMultiline(arg2);

          arg1.Visit(this);

          writer.Write(",");
          if (multiline)
            writer.NewLine();
          else
            writer.Write(' ');

          arg2.Visit(this);

          if (i == 0 | i < len - 1)
            writer.Write(";");
          writer.NewLine();
        }

        writer.UnindentedNewLine();
      }
      else {
        for (int i=0 ; i < len ; i++) {
          if (i > 0)
            writer.Write("; ");
          col1[i].Visit(this);
          writer.Write(", ");
          col2[i].Visit(this);
        }

        if (len == 1)
          writer.Write(';');
      }

      writer.Write(']');
    }

    public void NeSetObj(NeSetObj obj) {
      Obj[] elts = obj.Elts();
      int len = elts.Length;

      writer.Write('[');

      if (IsMultiline(obj)) {
        writer.Indent();

        // If we are on a fresh line, we start writing the first element
        // after the opening bracket, with just a space in between
        // Otherwise we start on the next line
        if (writer.IsNewLine())
          writer.Write(' ');
        else
          writer.NewLine();

        for (int i=0 ; i < len ; i++) {
          if (i > 0) {
            writer.Write(',');
            writer.NewLine();
          }
          elts[i].Visit(this);
        }

        writer.UnindentedNewLine();
      }
      else {
        for (int i=0 ; i < len ; i++) {
          if (i > 0)
            writer.Write(", ");
          elts[i].Visit(this);
        }
      }

      writer.Write(']');
    }

    public void NeTernRelObj(NeTernRelObj obj) {
      Obj[] col1 = obj.Col1();
      Obj[] col2 = obj.Col2();
      Obj[] col3 = obj.Col3();
      int len = col1.Length;

      writer.Write('[');

      if (IsMultiline(obj)) {
        writer.Indent();
        if (writer.IsNewLine())
          writer.Write(' ');
        else
          writer.NewLine();

        for (int i=0 ; i < len ; i++) {
          Obj arg1 = col1[i];
          Obj arg2 = col2[i];
          Obj arg3 = col3[i];

          bool multiline = IsMultiline(arg1) || IsMultiline(arg2) || IsMultiline(arg3);

          arg1.Visit(this);
          writer.Write(",");
          if (multiline)
            writer.NewLine();
          else
            writer.Write(' ');

          arg2.Visit(this);
          writer.Write(",");
          if (multiline)
            writer.NewLine();
          else
            writer.Write(' ');

          arg3.Visit(this);

          if (i == 0 | i < len - 1)
            writer.Write(";");
          writer.NewLine();
        }

        writer.UnindentedNewLine();
      }
      else {
        for (int i=0 ; i < len ; i++) {
          if (i > 0)
            writer.Write("; ");
          col1[i].Visit(this);
          writer.Write(", ");
          col2[i].Visit(this);
          writer.Write(", ");
          col3[i].Visit(this);
        }

        if (len == 1)
          writer.Write(';');
      }

      writer.Write(']');
    }

    public void NullObj(NullObj obj) {
      writer.Write("Null");
    }

    public void SymbObj(SymbObj obj) {
      writer.Write(obj.stringRepr);
    }

    public void TaggedIntObj(TaggedIntObj obj) {
      TaggedIntObj(obj.GetTagId(), obj.GetInnerLong());
    }

    public void TaggedIntObj(ushort tag, long value) {
      if (tag == Cell.Runtime.SymbObj.DateSymbId & IsPrintableDate(value)) {
        int[] yearMonthDay = DateTimeUtils.GetYearMonthDay((int) value);
        string str = string.Format("`{0}-{1:D2}-{2:D2}`", yearMonthDay[0], yearMonthDay[1], yearMonthDay[2]);
        writer.Write(str);
        return;
      }

      if (tag == Cell.Runtime.SymbObj.TimeSymbId) {
        int days;
        long dayNsecs;

        if (value >= 0) {
          days = (int) (value / 86400000000000L);
          dayNsecs = value % 86400000000000L;
        }
        else {
          long revDayNsecs = value % 86400000000000L;
          if (revDayNsecs == 0) {
            days = (int) (value / 86400000000000L);
            dayNsecs = 0;
          }
          else {
            days = (int) (value / 86400000000000L) - 1;
            dayNsecs = 86400000000000L + revDayNsecs;
          }
        }

        if (IsPrintableDate(days)) {
          int[] yearMonthDay = DateTimeUtils.GetYearMonthDay((int) days);
          long secs = dayNsecs / 1000000000;
          long nanosecs = dayNsecs % 1000000000;
          string nsecsStr = "";
          if (nanosecs != 0) {
            nsecsStr = ".";
            int div = 100000000;
            while (div > 0 & nanosecs > 0) {
              long digit = '0' + nanosecs / div;
              nsecsStr += digit.ToString();
              nanosecs %= div;
              div /= 10;
            }
          }
          string str = string.Format(
            "`{0}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}{6}`",
            yearMonthDay[0], yearMonthDay[1], yearMonthDay[2],
            secs / 3600, (secs / 60) % 60, secs % 60, nsecsStr
          );
          writer.Write(str);
          return;
        }
      }

      writer.Write(Cell.Runtime.SymbObj.IdxToStr(tag));
      writer.Write("(");
      writer.Write(value);
      writer.Write(")");
    }

    private static bool IsPrintableDate(long days) {
      // The date has to be between `1582-10-15` and `9999-12-31`
      return days >= -141427 & days <= 2932896;
    }

    public void TaggedObj(TaggedObj tagObj) {
      ushort tag = tagObj.GetTagId();
      Obj obj = tagObj.GetInnerObj();
      if (IsSyntacticSugaredString(tagObj))
        SyntacticSugaredString(obj);
      else if (obj.IsInt())
        TaggedIntObj(tag, obj.GetLong());
      else
        StandardTaggedObj(tag, obj);
    }

    private bool IsSyntacticSugaredString(TaggedObj tagObj) {
      Obj obj = tagObj.GetInnerObj();
      if (tagObj.GetTagId() != Cell.Runtime.SymbObj.StringSymbId | !obj.IsIntSeq())
        return false;
      int len = obj.GetSize();
      for (int i=0 ; i < len ; i++)
        if (!Miscellanea.IsBMPCodePoint(obj.GetLongAt(i)))
          return false;
      return true;
    }

    private void SyntacticSugaredString(Obj chars) {
      writer.Write('"');
      int len = chars.GetSize();
      for (int i=0 ; i < len ; i++) {
        int code = (char) chars.GetLongAt(i);
        if (code == '\n')
          writer.Write("\\n");
        else if (code == '\t')
          writer.Write("\\t");
        else if (code == '\\')
          writer.Write("\\\\");
        else if (code == '"')
          writer.Write("\\\"");
        else if (code >= 32 & code <= 126)
          writer.Write((char) code);
        else {
          writer.Write('\\');
          for (int j=0 ; j < 4 ; j++) {
            int hexDigit = (code >> (12 - 4 * j)) % 16;
            char ch = (char) (hexDigit < 10 ? '0' + hexDigit : 'a' - 10 + hexDigit);
            writer.Write(ch);
          }
        }
      }
      writer.Write('"');
    }

    private void StandardTaggedObj(ushort tagId, Obj obj) {
      writer.Write(Cell.Runtime.SymbObj.IdxToStr(tagId));

      if (!obj.IsNeRecord() && (!obj.IsNeSeq() || obj.GetSize() == 1)) {
        writer.Write('(');

        if (IsMultiline(obj)) {
          writer.IndentedNewLine();
          obj.Visit(this);
          writer.UnindentedNewLine();
        }
        else
          obj.Visit(this);

        writer.Write(')');
      }
      else
        obj.Visit(this);
    }


    public void OptTagRecObj(OptTagRecObj obj) {
      StandardTaggedObj(obj.GetTagId(), obj.GetInnerObj()); //## IMPLEMENT FOR REAL
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    private class Multiliner : ObjVisitor {
      private int lastVisitedObjSize = -1;
      private bool done = false;
      private bool bailEarly;
      private int maxLineLen;
      private HashSet<Obj> multilineObjs;


      private Multiliner(Obj obj, bool bailEarly, int maxLineLen, HashSet<Obj> multilineObjs) {
        this.bailEarly = bailEarly;
        this.maxLineLen = maxLineLen;
        this.multilineObjs = multilineObjs;
        ObjSize(obj);
      }

      public static HashSet<Obj> MultilineObjs(Obj obj, int maxLineLen) {
        HashSet<Obj> multilineObjs = new HashSet<Obj>(new IdentityEqualityComparer<Obj>());
        Multiliner multiliner = new Multiliner(obj, false, maxLineLen, multilineObjs);
        return multilineObjs;
      }

      public static bool IsMultiline(Obj obj, int maxLineLen) {
        Multiliner multiliner = new Multiliner(obj, true, maxLineLen, null);
        return multiliner.done;
      }

      ////////////////////////////////////////////////////////////////////////////

      private void ConsumeSize(Obj obj, int size) {
        lastVisitedObjSize = size;
      }

      private int ObjSize(Obj obj) {
        if (done)
          return 0;
        obj.Visit(this);
        Debug.Assert(lastVisitedObjSize != -1);
        int size = lastVisitedObjSize;
        lastVisitedObjSize = -1;
        if (size > maxLineLen) {
          if (multilineObjs != null)
            multilineObjs.Add(obj);
          if (bailEarly)
            done = true;
        }
        return size;
      }

      ////////////////////////////////////////////////////////////////////////////

      public void ArrayObj(ArrayObj obj) {
        NeSeqObj(obj);
      }

      public void ArraySliceObj(ArraySliceObj obj) {
        NeSeqObj(obj);
      }

      private void NeSeqObj(NeSeqObj obj) {
        int len = obj.GetSize();
        int size = 2 * len;
        for (int i=0 ; i < len ; i++)
          size += ObjSize(obj.GetObjAt(i));
        ConsumeSize(obj, size);
      }

      public void BlankObj(BlankObj obj) {
        ConsumeSize(obj, 5);
      }

      public void EmptyRelObj(EmptyRelObj obj) {
        ConsumeSize(obj, 2);
      }

      public void EmptySeqObj(EmptySeqObj obj) {
        ConsumeSize(obj, 2);
      }

      public void FloatArrayObj(FloatArrayObj obj) {
        NeFloatSeqObj(obj);
      }

      public void FloatArraySliceObj(FloatArraySliceObj obj) {
        NeFloatSeqObj(obj);
      }

      private void NeFloatSeqObj(NeFloatSeqObj obj) {
        int len = obj.GetSize();
        int size = 2 * len;
        for (int i=0 ; i < len ; i++)
          size += DoublePrintSize(obj.GetDoubleAt(i));
        ConsumeSize(obj, size);
      }

      public void FloatObj(FloatObj obj) {
        ConsumeSize(obj, DoublePrintSize(obj.GetDouble()));
      }

      private int DoublePrintSize(double value) {
        return value.ToString().Length;
      }

      public void IntArrayObj(IntArrayObj obj) {
        NeIntSeqObj(obj);
      }

      public void IntArraySliceObj(IntArraySliceObj obj) {
        NeIntSeqObj(obj);
      }

      public void SignedByteArrayObj(SignedByteArrayObj obj) {
        NeIntSeqObj(obj);
      }

      public void SignedByteArraySliceObj(SignedByteArraySliceObj obj) {
        NeIntSeqObj(obj);
      }

      public void UnsignedByteArrayObj(UnsignedByteArrayObj obj) {
        NeIntSeqObj(obj);
      }

      public void UnsignedByteArraySliceObj(UnsignedByteArraySliceObj obj) {
        NeIntSeqObj(obj);
      }

      public void ShortArrayObj(ShortArrayObj obj) {
        NeIntSeqObj(obj);
      }

      public void ShortArraySliceObj(ShortArraySliceObj obj) {
        NeIntSeqObj(obj);
      }

      public void Int32ArrayObj(Int32ArrayObj obj) {
        NeIntSeqObj(obj);
      }

      public void Int32ArraySliceObj(Int32ArraySliceObj obj) {
        NeIntSeqObj(obj);
      }

      private void NeIntSeqObj(NeIntSeqObj obj) {
        int len = obj.GetSize();
        int size = 2 * len;
        for (int i=0 ; i < len; i++)
          size += LongPrintSize(obj.GetLongAt(i));
        ConsumeSize(obj, size);
      }

      public void IntObj(IntObj obj) {
        ConsumeSize(obj, LongPrintSize(obj.GetLong()));
      }

      private int LongPrintSize(long value) {
        return value.ToString().Length;
      }

      public void NeBinRelObj(NeBinRelObj obj) {
        Obj[] col1 = obj.Col1();
        Obj[] col2 = obj.Col2();
        bool isMap = obj.IsNeMap();
        bool isRec = obj.IsNeRecord();
        int len = col1.Length;
        int size = (2 + (isMap & !isRec ? 4 : 2)) * len + ((!isMap & len == 1) ? 1 : 0);
        for (int i=0 ; i < len ; i++)
          size += ObjSize(col1[i]) + ObjSize(col2[i]);
        ConsumeSize(obj, size);
      }

      public void NeSetObj(NeSetObj obj) {
        Obj[] elts = obj.Elts();
        int len = elts.Length;
        int size = 2 * len;
        for (int i=0 ; i < len ; i++)
          size += ObjSize(elts[i]);
        ConsumeSize(obj, size);
      }

      public void NeTernRelObj(NeTernRelObj obj) {
        Obj[] col1 = obj.Col1();
        Obj[] col2 = obj.Col2();
        Obj[] col3 = obj.Col3();
        int len = col1.Length;
        int size = 6 * len + (len == 1 ? 1 : 0);
        for (int i=0 ; i < len ; i++)
          size += ObjSize(col1[i]) + ObjSize(col2[i]) + ObjSize(col3[i]);
        ConsumeSize(obj, size);
      }

      public void NullObj(NullObj obj) {
        ConsumeSize(obj, 4);
      }

      public void SymbObj(SymbObj obj) {
        ConsumeSize(obj, obj.stringRepr.Length);
      }

      public void TaggedIntObj(TaggedIntObj obj) {
        ConsumeSize(obj, 2 + Cell.Runtime.SymbObj.Get(obj.GetTagId()).stringRepr.Length + LongPrintSize(obj.GetInnerLong()));
      }

      public void TaggedObj(TaggedObj obj) {
        int tagSize = Cell.Runtime.SymbObj.Get(obj.GetTagId()).stringRepr.Length;
        ConsumeSize(obj, 2 + tagSize + ObjSize(obj.GetInnerObj()));
      }

      public void OptTagRecObj(OptTagRecObj obj) {
        int tagSize = Cell.Runtime.SymbObj.Get(obj.GetTagId()).stringRepr.Length;
        ConsumeSize(obj, 2 + tagSize + ObjSize(obj.GetInnerObj()));
      }
    }
  }
}
