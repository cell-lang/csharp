namespace Cell.Runtime {
  public abstract class ColumnBase {
    protected int count = 0;

    internal SurrObjMapper mapper;


    protected ColumnBase(SurrObjMapper mapper) {
      this.mapper = mapper;
    }

    //////////////////////////////////////////////////////////////////////////////

    public int Size() {
      return count;
    }

    //////////////////////////////////////////////////////////////////////////////

    public static Obj Copy(ColumnBase[] columns, bool flipCols) {
      int totalSize = 0;
      for (int i=0 ; i < columns.Length ; i++)
        totalSize += columns[i].count;

      if (totalSize == 0)
        return EmptyRelObj.singleton;

      Obj[] objs1 = new Obj[totalSize];
      Obj[] objs2 = new Obj[totalSize];

      int next = 0;
      for (int i=0 ; i < columns.Length ; i++) {
        ColumnBase col = columns[i];
        if (col is IntColumn) {
          IntColumn intCol = (IntColumn) col;
          IntColumn.Iter it = intCol.GetIter();
          while (!it.Done()) {
            objs1[next] = intCol.mapper(it.GetIdx());
            objs2[next] = IntObj.Get(it.GetValue());
            next++;
            it.Next();
          }

        }
        else if (col is FloatColumn) {
          FloatColumn floatCol = (FloatColumn) col;
          FloatColumn.Iter it = floatCol.GetIter();
          while (!it.Done()) {
            objs1[next] = floatCol.mapper(it.GetIdx());
            objs2[next] = new FloatObj(it.GetValue());
            next++;
            it.Next();
          }
        }
        else {
          ObjColumn objCol = (ObjColumn) col;
          ObjColumn.Iter it = objCol.GetIter();
          while (!it.Done()) {
            objs1[next] = objCol.mapper(it.GetIdx());
            objs2[next] = it.GetValue();
            next++;
            it.Next();
          }
        }
      }
      Debug.Assert(next == totalSize);

      if (flipCols) {
        Obj[] tmp = objs1;
        objs1 = objs2;
        objs2 = tmp;
      }

      return Builder.CreateBinRel(objs1, objs2);
    }
  }
}
