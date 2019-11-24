namespace Cell.Runtime {
  public class SeqIter {
    Obj[] objs;
    int next;
    int last;

    public SeqIter(Obj[] objs, int next, int last) {
      this.objs = objs;
      this.next = next;
      this.last = last;
    }

    public Obj Get() {
      Debug.Assert(next <= last);
      return objs[next];
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
