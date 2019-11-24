namespace Cell.Runtime {
  public sealed class ObjColumn : ColumnBase {
    private const int INIT_SIZE = 256;

    //////////////////////////////////////////////////////////////////////////////

    Obj[] column = new Obj[INIT_SIZE];

    //////////////////////////////////////////////////////////////////////////////

    public ObjColumn(SurrObjMapper mapper) : base(mapper) {
      this.mapper = mapper;
    }

    public bool Contains1(int idx) {
      return idx < column.Length && column[idx] != null;
    }

    public Obj Lookup(int idx) {
      Obj value = column[idx];
      if (value == null)
        throw ErrorHandler.SoftFail();
      return value;
    }

    public Iter GetIter() {
      return Iter.NewIter(column, count);
    }

    //////////////////////////////////////////////////////////////////////////////

    public void Insert(int idx, Obj value) {
      if (idx >= column.Length)
        column = Array.Extend(column, Array.Capacity(column.Length, idx+1));
      Obj currValue = column[idx];
      if (currValue == null) {
        count++;
        column[idx] = value;
      }
      else if (!value.IsEq(currValue))
        throw ErrorHandler.SoftFail();
    }

    public void Update(int idx, Obj value) {
      if (idx >= column.Length)
        column = Array.Extend(column, Array.Capacity(column.Length, idx+1));
      if (column[idx] == null)
        count++;
      column[idx] = value;
    }

    public void Delete(int idx) {
      if (idx < column.Length && column[idx] != null) {
        column[idx] = null;
        count--;
      }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class Iter {
      private Obj[] column;
      private int left; // Includes current value
      private int idx;

      private static Iter emptyIter = new Iter();

      private Iter() {
        column = new Obj[0];
        left = 0;
        idx = 0;
      }

      private Iter(Obj[] column, int count) {
        Debug.Assert(count > 0);
        this.column = column;
        this.left = count;
        this.idx = 0;
        while (column[idx] == null)
          idx++;
      }

      public static Iter NewIter(Obj[] column, int count) {
        return count != 0 ? new Iter(column, count) : emptyIter;
      }

      public bool Done() {
        return left <= 0;
      }

      public int GetIdx() {
        return idx;
      }

      public Obj GetValue() {
        return column[idx];
      }

      public void Next() {
        if (--left > 0)
          do
            idx++;
          while (column[idx] == null);
      }
    }
  }
}
