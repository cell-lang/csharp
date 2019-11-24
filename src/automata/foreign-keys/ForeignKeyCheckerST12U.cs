namespace Cell.Runtime {
  // tern_rel(a | b, _) -> unary_rel(a), unary_rel(b);

  public class ForeignKeyCheckerST12U : UnaryTableUpdater.DeleteChecker {
    Sym12TernaryTableUpdater source;
    UnaryTableUpdater target;

    public ForeignKeyCheckerST12U(Sym12TernaryTableUpdater source, UnaryTableUpdater target) {
      Debug.Assert(source.store12 == target.store);
      this.source = source;
      this.target = target;
    }

    public void Check() {
      // Checking that every new entry satisfies the foreign key
      int count = source.insertCount;
      if (count > 0) {
        int[] inserts = source.insertList;
        for (int i=0 ; i < count ; i++)
          if (!target.Contains(inserts[3*i]) | !target.Contains(inserts[3*i+1]))
            throw ForeignKeyViolation(inserts[3*i], inserts[3*i+1], inserts[3*i+2]);
      }

      // Checking that no entries were invalidates by a deletion on the target table
      target.CheckDeletedKeys(this);
    }

    public void WasDeleted(int surr12) {
      if (source.contains_1_2(surr12))
        throw ForeignKeyViolation(surr12);
    }

    //////////////////////////////////////////////////////////////////////////////

    private ForeignKeyViolationException ForeignKeyViolation(int arg1Surr, int arg2Surr, int arg3Surr) {
      Obj[] tuple = new Obj[] {
        source.store12.SurrToValue(arg1Surr),
        source.store12.SurrToValue(arg2Surr),
        source.store3.SurrToValue(arg3Surr)
      };
      return ForeignKeyViolationException.SymTernary12Unary(source.relvarName, target.relvarName, tuple);
    }

    private ForeignKeyViolationException ForeignKeyViolation(int arg12Surr) {
      Sym12TernaryTable.Iter it = source.table.getIter_1_2(arg12Surr);
      Obj arg1 = source.store12.SurrToValue(arg12Surr);
      Obj arg2 = source.store12.SurrToValue(it.Get1());
      Obj arg3 = source.store3.SurrToValue(it.Get2());
      Obj[] tuple = new Obj[] {arg1, arg2, arg3};
      return ForeignKeyViolationException.SymTernary12Unary(source.relvarName, target.relvarName, tuple, arg1);
    }
  }
}
