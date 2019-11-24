namespace Cell.Runtime {
  public class BinRelIter {
    Obj[] col1;
    Obj[] col2;
    int[] idxs;
    int next;
    int last;

    public static BinRelIter emptyIter =
      new BinRelIter(Array.emptyObjArray, Array.emptyObjArray, 0, -1);

    public BinRelIter(Obj[] col1, Obj[] col2, int[] idxs, int next, int last) {
      Debug.Assert(col1.Length == col2.Length);
      Debug.Assert(idxs == null || col1.Length == idxs.Length);
      Debug.Assert(next >= 0);
      Debug.Assert(last >= -1 & last < col1.Length);
      this.col1 = col1;
      this.col2 = col2;
      this.idxs = idxs;
      this.next = next;
      this.last = last;
    }

    public BinRelIter(Obj[] col1, Obj[] col2, int next, int last) : this(col1, col2, null, next, last) {

    }

    public BinRelIter(Obj[] col1, Obj[] col2) : this(col1, col2, null, 0, col1.Length-1) {

    }

    public Obj Get1() {
      return col1[idxs == null ? next : idxs[next]];
    }

    public Obj Get2() {
      return col2[idxs == null ? next : idxs[next]];
    }

    public void Next() {
      Debug.Assert(next <= last);
      next++;
    }

    public bool Done() {
      return next > last;
    }
  }
}
