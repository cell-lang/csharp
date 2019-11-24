namespace Cell.Runtime {
  // tern_rel(a | b, _) -> bin_rel(a | b)

  public class ForeignKeyCheckerSTSB : SymBinaryTableUpdater.DeleteChecker {
    Sym12TernaryTableUpdater source;
    SymBinaryTableUpdater target;

    public ForeignKeyCheckerSTSB(Sym12TernaryTableUpdater source, SymBinaryTableUpdater target) {
      Debug.Assert(source.store12 == target.store);
      this.source = source;
      this.target = target;
    }

    public void Check() {
      int count = source.insertCount;
      if (count > 0) {
        int[] inserts = source.insertList;
        for (int i=0 ; i < count ; i++)
          if (!target.Contains(inserts[3*i], inserts[3*i+1]))
            throw ForeignKeyViolation(inserts[3*i], inserts[3*i+1], inserts[3*i+2]);
      }

      target.CheckDeletes(this);
    }

    public void CheckDelete(int surr1, int surr2, SymBinaryTableUpdater target) {
      if (source.Contains12(surr1, surr2) && !target.Contains(surr1, surr2))
        throw ForeignKeyViolation(surr1, surr2, source.LookupAny12(surr1, surr2));
    }

    //////////////////////////////////////////////////////////////////////////////

    private ForeignKeyViolationException ForeignKeyViolation(int surr1, int surr2, int surr3) {
      Obj arg1 = source.store12.SurrToValue(surr1);
      Obj arg2 = source.store12.SurrToValue(surr2);
      Obj arg3 = source.store3.SurrToValue(surr3);
      return ForeignKeyViolationException.SymTernarySymBinary(source.relvarName, target.relvarName, arg1, arg2, arg3);
    }
  }
}
