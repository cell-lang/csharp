namespace Cell.Runtime {
  public class TableWriter {
    public static void Write(DataWriter writer, int fieldSymbId, UnaryTable[] tables, int indentation, bool indentFirstLine, bool writeSeparator) {
      string baseWs = new string(Array.Repeat(' ', indentation));
      string entryWs = new string(Array.Repeat(' ', indentation + 2));

      int count = 0;
      for (int i=0 ; i < tables.Length ; i++)
        count += tables[i].Size();

      if (indentFirstLine)
        writer.Write(baseWs);
      writer.Write(SymbObj.IdxToStr(fieldSymbId));
      writer.Write(": [");

      if (count > 0) {
        writer.Write("\n");

        int written = 0;
        for (int i=0 ; i < tables.Length ; i++) {
          UnaryTable table = tables[i];
          SurrObjMapper mapper = table.mapper;
          UnaryTable.Iter it = table.GetIter();
          while (!it.Done()) {
            writer.Write(entryWs);
            Obj obj = mapper(it.Get());
            ObjPrinter.Print(obj, writer);
            written++;
            writer.Write(written < count ? ",\n" : "\n");
            it.Next();
          }
        }
        Debug.Assert(written == count);

        writer.Write(baseWs);
      }

      writer.Write(writeSeparator ? "],\n" : "]\n");
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public static void Write(DataWriter writer, int fieldSymbId, BinaryTable[] tables, bool flipCols, int indentation, bool indentFirstLine, bool writeSeparator) {
      string baseWs = new string(Array.Repeat(' ', indentation));
      string entryWs = new string(Array.Repeat(' ', indentation + 2));

      int count = 0;
      for (int i=0 ; i < tables.Length ; i++)
        count += tables[i].Size();

      if (indentFirstLine)
        writer.Write(baseWs);
      writer.Write(SymbObj.IdxToStr(fieldSymbId));
      writer.Write(": [");

      if (count > 0) {
        writer.Write("\n");

        int written = 0;
        for (int i=0 ; i < tables.Length ; i++) {
          BinaryTable table = tables[i];
          SurrObjMapper mapper1 = table.mapper1;
          SurrObjMapper mapper2 = table.mapper2;
          BinaryTable.Iter it = table.GetIter();
          while (!it.Done()) {
            writer.Write(entryWs);
            Obj obj1 = mapper1(it.Get1());
            Obj obj2 = mapper2(it.Get2());
            if (flipCols) {
              Obj tmp = obj1;
              obj1 = obj2;
              obj2 = tmp;
            }
            ObjPrinter.Print(obj1, writer);
            writer.Write(", ");
            ObjPrinter.Print(obj2, writer);
            written++;
            writer.Write(written < count || count == 1 ? ";\n" : "\n");
            it.Next();
          }
        }
        Debug.Assert(written == count);

        writer.Write(baseWs);
      }

      writer.Write(writeSeparator ? "],\n" : "]\n");
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public static void Write(DataWriter writer, int fieldSymbId, ColumnBase[] columns, bool flipCols, int indentation, bool indentFirstLine, bool writeSeparator) {
      string baseWs = new string(Array.Repeat(' ', indentation));
      string entryWs = new string(Array.Repeat(' ', indentation + 2));

      int count = 0;
      for (int i=0 ; i < columns.Length ; i++)
        count += columns[i].Size();

      if (indentFirstLine)
        writer.Write(baseWs);
      writer.Write(SymbObj.IdxToStr(fieldSymbId));
      writer.Write(": [");

      if (count > 0) {
        writer.Write("\n");

        int written = 0;
        for (int i=0 ; i < columns.Length ; i++) {
          ColumnBase col = columns[i];
          SurrObjMapper mapper = col.mapper;

          if (col is IntColumn) {
            IntColumn intCol = (IntColumn) col;
            IntColumn.Iter it = intCol.GetIter();
            while (!it.Done()) {
              writer.Write(entryWs);
              Obj key = mapper(it.GetIdx());
              long value = it.GetValue();
              if (flipCols) {
                writer.Write(value);
                writer.Write(", ");
                ObjPrinter.Print(key, writer);
                written++;
                writer.Write(written == 1 | written < count ? ";\n" : "\n");
              }
              else {
                ObjPrinter.Print(key, writer);
                writer.Write(" -> ");
                writer.Write(value);
                written++;
                writer.Write(written < count ? ",\n" : "\n");
              }
              it.Next();
            }
          }
          else if (col is FloatColumn) {
            FloatColumn floatCol = (FloatColumn) col;
            FloatColumn.Iter it = floatCol.GetIter();
            while (!it.Done()) {
              writer.Write(entryWs);
              Obj key = mapper(it.GetIdx());
              double value = it.GetValue();
              if (flipCols) {
                writer.Write(value);
                writer.Write(", ");
                ObjPrinter.Print(key, writer);
                written++;
                writer.Write(written == 1 | written < count ? ";\n" : "\n");
              }
              else {
                ObjPrinter.Print(key, writer);
                writer.Write(" -> ");
                writer.Write(value);
                written++;
                writer.Write(written < count ? ",\n" : "\n");
              }
              it.Next();
            }
          }
          else {
            ObjColumn objCol = (ObjColumn) col;
            ObjColumn.Iter it = objCol.GetIter();
            while (!it.Done()) {
              writer.Write(entryWs);
              Obj key = mapper(it.GetIdx());
              Obj value = it.GetValue();
              if (flipCols) {
                ObjPrinter.Print(value, writer);
                writer.Write(", ");
                ObjPrinter.Print(key, writer);
                written++;
                writer.Write(written == 1 | written < count ? ";\n" : "\n");
              }
              else {
                ObjPrinter.Print(key, writer);
                writer.Write(" -> ");
                ObjPrinter.Print(value, writer);
                written++;
                writer.Write(written < count ? ",\n" : "\n");
              }
              it.Next();
            }
          }
        }
        Debug.Assert(written == count);

        writer.Write(baseWs);
      }

      writer.Write(writeSeparator ? "],\n" : "]\n");
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////


    public static void Write(DataWriter writer, int fieldSymbId, TernaryTable[] tables, int col1, int col2, int col3, int indentation, bool indentFirstLine, bool writeSeparator) {
      string baseWs = new string(Array.Repeat(' ', indentation));
      string entryWs = new string(Array.Repeat(' ', indentation + 2));

      int count = 0;
      for (int i=0 ; i < tables.Length ; i++)
        count += tables[i].Size();

      if (indentFirstLine)
        writer.Write(baseWs);
      writer.Write(SymbObj.IdxToStr(fieldSymbId));
      writer.Write(": [");

      if (count > 0) {
        writer.Write("\n");

        int written = 0;
        for (int i=0 ; i < tables.Length ; i++) {
          TernaryTable table = tables[i];
          SurrObjMapper mapper1 = table.mapper1;
          SurrObjMapper mapper2 = table.mapper2;
          SurrObjMapper mapper3 = table.mapper3;
          TernaryTable.Iter it = table.GetIter();
          while (!it.Done()) {
            writer.Write(entryWs);
            Obj obj1 = mapper1(it.Get1());
            Obj obj2 = mapper2(it.Get2());
            Obj obj3 = mapper3(it.Get3());

            if (col1 == 0)
              ObjPrinter.Print(obj1, writer);
            else if (col1 == 1)
              ObjPrinter.Print(obj2, writer);
            else
              ObjPrinter.Print(obj3, writer);

            writer.Write(", ");

            if (col2 == 0)
              ObjPrinter.Print(obj1, writer);
            else if (col2 == 1)
              ObjPrinter.Print(obj2, writer);
            else
              ObjPrinter.Print(obj3, writer);

            writer.Write(", ");

            if (col3 == 0)
              ObjPrinter.Print(obj1, writer);
            else if (col3 == 1)
              ObjPrinter.Print(obj2, writer);
            else
              ObjPrinter.Print(obj3, writer);

            written++;
            writer.Write(written < count || count == 1 ? ";\n" : "\n");
            it.Next();
          }
        }
        Debug.Assert(written == count);

        writer.Write(baseWs);
      }

      writer.Write(writeSeparator ? "],\n" : "]\n");
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public static void Write(DataWriter writer, int fieldSymbId, SymBinaryTable[] tables, int indentation, bool indentFirstLine, bool writeSeparator) {
      string baseWs = new string(Array.Repeat(' ', indentation));
      string entryWs = new string(Array.Repeat(' ', indentation + 2));

      int count = 0;
      for (int i=0 ; i < tables.Length ; i++)
        count += tables[i].Size();

      if (indentFirstLine)
        writer.Write(baseWs);
      writer.Write(SymbObj.IdxToStr(fieldSymbId));
      writer.Write(": [");

      if (count > 0) {
        writer.Write("\n");

        int written = 0;
        for (int i=0 ; i < tables.Length ; i++) {
          SymBinaryTable table = tables[i];
          SurrObjMapper mapper = table.mapper;
          SymBinaryTable.Iter it = table.GetIter();
          while (!it.Done()) {
            writer.Write(entryWs);
            Obj obj1 = mapper(it.Get1());
            Obj obj2 = mapper(it.Get2());
            ObjPrinter.Print(obj1, writer);
            writer.Write(", ");
            ObjPrinter.Print(obj2, writer);
            written++;
            writer.Write(written < count || count == 1 ? ";\n" : "\n");
            it.Next();
          }
        }
        Debug.Assert(written == count);

        writer.Write(baseWs);
      }

      writer.Write(writeSeparator ? "],\n" : "]\n");
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public static void Write(DataWriter writer, int fieldSymbId, Sym12TernaryTable[] tables, int indentation, bool indentFirstLine, bool writeSeparator) {
      string baseWs = new string(Array.Repeat(' ', indentation));
      string entryWs = new string(Array.Repeat(' ', indentation + 2));

      int count = 0;
      for (int i=0 ; i < tables.Length ; i++)
        count += tables[i].Size();

      if (indentFirstLine)
        writer.Write(baseWs);
      writer.Write(SymbObj.IdxToStr(fieldSymbId));
      writer.Write(": [");

      if (count > 0) {
        writer.Write("\n");

        int written = 0;
        for (int i=0 ; i < tables.Length ; i++) {
          Sym12TernaryTable table = tables[i];
          SurrObjMapper mapper12 = table.mapper12;
          SurrObjMapper mapper3 = table.mapper3;
          Sym12TernaryTable.Iter it = table.GetIter();
          while (!it.Done()) {
            writer.Write(entryWs);
            Obj obj1 = mapper12(it.Get1());
            Obj obj2 = mapper12(it.Get2());
            Obj obj3 = mapper3(it.Get3());
            ObjPrinter.Print(obj1, writer);
            writer.Write(", ");
            ObjPrinter.Print(obj2, writer);
            writer.Write(", ");
            ObjPrinter.Print(obj3, writer);
            written++;
            writer.Write(written < count || count == 1 ? ";\n" : "\n");
            it.Next();
          }
        }
        Debug.Assert(written == count);

        writer.Write(baseWs);
      }

      writer.Write(writeSeparator ? "],\n" : "]\n");
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    // public static void Write(DataWriter writer, int fieldSymbId, AssocTable[] tables, int indentation, bool indentFirstLine, bool writeSeparator) {
    //   throw new RuntimeException();
    // }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    // public static void Write(DataWriter writer, int fieldSymbId, SlaveTernTable[] tables, int indentation, bool indentFirstLine, bool writeSeparator) {
    //   throw new RuntimeException();
    // }
  }
}
