namespace Cell.Runtime {
  // bin_rel(a, _) -> unary_rel(a);

  public class ForeignKeyCheckerBU1 : UnaryTableUpdater.DeleteChecker {
    BinaryTableUpdater source;
    UnaryTableUpdater target;

    private int[] counter = new int[1];
    private long[] buffer = new long[256];


    public ForeignKeyCheckerBU1(BinaryTableUpdater source, UnaryTableUpdater target) {
      Debug.Assert(source.store1 == target.store);

      this.source = source;
      this.target = target;
    }

    public void Check() {
      if (source.HasInsertions()) {
        long[] buffer = source.Insertions(this.buffer, counter);
        int count = counter[0];
        for (int i=0 ; i < count ; i++) {
          long entry = buffer[i];
          int arg1 = BinaryTableUpdater.Arg1(entry);
          if (!target.Contains(arg1))
            throw InsertionForeignKeyViolation(arg1, BinaryTableUpdater.Arg2(entry));
        }
      }

      target.CheckDeletedKeys(this);
    }

    public void WasDeleted(int arg1) {
      // arg1 is guaranteed to have been deleted and not reinserted
      if (source.Contains1(arg1))
        throw DeletionForeignKeyViolation(arg1);
    }

    //////////////////////////////////////////////////////////////////////////////

    private ForeignKeyViolationException InsertionForeignKeyViolation(int surr1, int surr2) {
      Obj[] tuple = new Obj[] {source.store1.SurrToValue(surr1), source.store2.SurrToValue(surr2)};
      return ForeignKeyViolationException.BinaryUnary(source.relvarName, 1, target.relvarName, tuple);
    }

    private ForeignKeyViolationException DeletionForeignKeyViolation(int surr1) {
      int surr2 = source.table.Restrict1(surr1)[0]; //## BAD: VERY INEFFICIENT
      Obj obj1 = source.store1.SurrToValue(surr1);
      Obj[] tuple = new Obj[] {obj1, source.store2.SurrToValue(surr2)};
      return ForeignKeyViolationException.BinaryUnary(source.relvarName, 1, target.relvarName, tuple, obj1);
    }
  }
}
