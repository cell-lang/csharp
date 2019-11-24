namespace Cell.Runtime {
  // bin_rel(a | b) -> ternary_rel(a | b, _)

  public class ForeignKeyCheckerSBST : Sym12TernaryTableUpdater.DeleteChecker {
    SymBinaryTableUpdater source;
    Sym12TernaryTableUpdater target;

    public ForeignKeyCheckerSBST(SymBinaryTableUpdater source, Sym12TernaryTableUpdater target) {
      Debug.Assert(source.store == target.store12);
      this.source = source;
      this.target = target;
    }

    public void Check() {
      // Checking that every new entry satisfies the foreign key
      int count = source.insertCount;
      if (count > 0) {
        int[] inserts = source.insertList;
        for (int i=0 ; i < count ; i++)
          if (!target.Contains12(inserts[2*i], inserts[2*i+1]))
            throw ForeignKeyViolation(inserts[2*i], inserts[2*i+1]);
      }

      // Checking that no entries were invalidated by a deletion on the target table
      target.CheckDeletes(this);
    }

    public void CheckDelete(int surr1, int surr2, int surr3, Sym12TernaryTableUpdater target) {
      if (source.Contains(surr1, surr2) && !target.Contains12(surr1, surr2))
        throw ForeignKeyViolation(surr1, surr2, surr3);
    }

    //////////////////////////////////////////////////////////////////////////////

    private ForeignKeyViolationException ForeignKeyViolation(int surr1, int surr2) {
      Obj arg1 = source.store.SurrToValue(surr1);
      Obj arg2 = source.store.SurrToValue(surr2);
      return ForeignKeyViolationException.SymBinarySymTernary(source.relvarName, target.relvarName, arg1, arg2);
    }

    private ForeignKeyViolationException ForeignKeyViolation(int surr1, int surr2, int surr3) {
      Obj arg1 = source.store.SurrToValue(surr1);
      Obj arg2 = source.store.SurrToValue(surr2);
      Obj arg3 = target.store3.SurrToValue(surr3);
      return ForeignKeyViolationException.SymBinarySymTernary(source.relvarName, target.relvarName, arg1, arg2, arg3);
    }
  }
}
