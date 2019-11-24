namespace Cell.Runtime {
  public sealed class FloatColumn : ColumnBase {
    private const int INIT_SIZE = 256;

    // private const long NULL_BIT_MASK = 0x7FF8000000000000L;
    // private const long NULL_BIT_MASK = 0x7FFFFFFFFFFFFFFFL;
    private const long NULL_BIT_MASK = 0x7FFA3E90779F7D08L; // Random NaN

    private readonly double NULL = Miscellanea.LongBitsToDoubleBits(NULL_BIT_MASK);

    //////////////////////////////////////////////////////////////////////////////

    double[] column = new double[INIT_SIZE];

    //////////////////////////////////////////////////////////////////////////////

    public FloatColumn(SurrObjMapper mapper) : base(mapper) {
      Debug.Assert(FloatObj.IsNaN(NULL));
      Array.Fill(column, NULL);
      this.mapper = mapper;
    }

    public bool Contains1(int idx) {
      return idx < column.Length && !IsNull(column[idx]);
    }

    public double Lookup(int idx) {
      double value = column[idx];
      if (IsNull(value))
        throw ErrorHandler.SoftFail();
      return value;
    }

    public Iter GetIter() {
      return Iter.NewIter(column, count);
    }

    //////////////////////////////////////////////////////////////////////////////

    public void Insert(int index, double value) {
      if (index >= column.Length)
        column = Array.Extend(column, Array.Capacity(column.Length, index+1), NULL);
      double currValue = column[index];
      if (!IsNull(currValue)) {
        if (value != currValue)
          throw ErrorHandler.SoftFail();
      }
      else {
        column[index] = FloatObj.IsNaN(value) ? FloatObj.NaN : value;
        count++;
      }
    }

    public void Update(int index, double value) {
      if (index >= column.Length)
        column = Array.Extend(column, Array.Capacity(column.Length, index+1), NULL);
      if (IsNull(column[index]))
        count++;
      column[index] = FloatObj.IsNaN(value) ? FloatObj.NaN : value;
    }

    public void Delete(int index) {
      if (index < column.Length && !IsNull(column[index])) {
        column[index] = NULL;
        count--;
      }
    }

    public void Clear() {
      count = 0;
      if (column.Length != INIT_SIZE)
        column = new double[INIT_SIZE];
      Array.Fill(column, NULL);
    }

    //////////////////////////////////////////////////////////////////////////////

    private static bool IsNull(double value) {
      return Miscellanea.DoubleBitsToLongBits(value) == NULL_BIT_MASK;
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public class Iter {
      private double[] column;
      private int left; // Includes current value
      private int idx;

      private static Iter emptyIter = new Iter();

      private Iter() {
        column = new double[0];
        left = 0;
        idx = 0;
      }

      private Iter(double[] column, int count) {
        Debug.Assert(count > 0);
        this.column = column;
        this.left = count;
        this.idx = 0;
        while (IsNull(column[idx]))
          idx++;
      }

      public static Iter NewIter(double[] column, int count) {
        return count != 0 ? new Iter(column, count) : emptyIter;
      }

      public bool Done() {
        return left <= 0;
      }

      public int GetIdx() {
        return idx;
      }

      public double GetValue() {
        return column[idx];
      }

      public void Next() {
        if (--left > 0)
          do
            idx++;
          while (IsNull(column[idx]));
      }
    }
  }
}
