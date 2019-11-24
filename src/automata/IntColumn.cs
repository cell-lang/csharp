using System.Collections.Generic;


namespace Cell.Runtime {
  public sealed class IntColumn : ColumnBase {
    private const int INIT_SIZE = 256;

    // private const long NULL = -9223372036854775808L;
    private const long NULL = -5091454680840284659L; // Random null value

    //////////////////////////////////////////////////////////////////////////////

    long[] column = new long[INIT_SIZE];
    HashSet<int> collisions = new HashSet<int>();

    //////////////////////////////////////////////////////////////////////////////

    public IntColumn(SurrObjMapper mapper) : base(mapper) {
      Array.Fill(column, NULL);
      this.mapper = mapper;
    }

    public bool Contains1(int idx) {
      return idx < column.Length && !IsNull(idx);
    }

    public long Lookup(int idx) {
      long value = column[idx];
      if (IsNull(idx, value))
        throw ErrorHandler.SoftFail();
      return value;
    }

    public Iter GetIter() {
      return new Iter(this);
    }

    //////////////////////////////////////////////////////////////////////////////

    public void Insert(int idx, long value) {
      if (idx >= column.Length)
        column = Array.Extend(column, Array.Capacity(column.Length, idx+1), NULL);

      long currValue = column[idx];
      if (IsNull(idx, currValue)) {
        count++;
        column[idx] = value;
        if (value == NULL)
          collisions.Add(idx);
      }
      else {
        // The value is already set, so we need to fail if the new value is different from the existing one
        if (value != currValue)
          throw ErrorHandler.SoftFail();
      }
    }

    public void Update(int idx, long value) {
      if (idx >= column.Length)
        column = Array.Extend(column, Array.Capacity(column.Length, idx+1), NULL);

      long currValue = column[idx];
      if (currValue != NULL) {
        // There is an existing value, and it's not NULL
        column[idx] = value;
        if (value == NULL)
          collisions.Add(idx);
      }
      else if (collisions.Contains(idx)) {
        // The existing value happens to be NULL
        if (value != NULL) {
          column[idx] = value;
          collisions.Remove(idx);
        }
      }
      else {
        // No existing value
        count++;
        column[idx] = value;
        if (value == NULL)
          collisions.Add(idx);
      }
    }

    public void Delete(int idx) {
      if (idx < column.Length) {
        long value = column[idx];
        if (value != NULL) {
          column[idx] = NULL;
          count--;
        }
        else if (collisions.Contains(idx)) {
          collisions.Remove(idx);
          count--;
        }
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    private bool IsNull(int idx) {
      return IsNull(idx, column[idx]);
    }

    private bool IsNull(int idx, long value) {
      return value == NULL && !collisions.Contains(idx);
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public class Iter {
      IntColumn column;
      private int left; // Includes current value
      private int idx;


      public Iter(IntColumn column) {
        this.column = column;
        this.left = column.count;
        this.idx = 0;
        if (left > 0)
          while (column.IsNull(idx))
            idx++;
      }

      public bool Done() {
        return left <= 0;
      }

      public int GetIdx() {
        return idx;
      }

      public long GetValue() {
        return column.column[idx];
      }

      public void Next() {
        if (--left > 0)
          do
            idx++;
          while (column.IsNull(idx));
      }
    }
  }
}
