namespace Cell.Runtime {
  // bin_rel(_, b) -> unary_rel(b);

  public class ForeignKeyCheckerBU2 : UnaryTableUpdater.DeleteChecker {
    BinaryTableUpdater source;
    UnaryTableUpdater target;

    private int[] counter = new int[1];
    private long[] buffer = new long[256];


    public ForeignKeyCheckerBU2(BinaryTableUpdater source, UnaryTableUpdater target) {
      this.source = source;
      this.target = target;
    }

    public void Check() {
      if (source.HasInsertions()) {
        long[] buffer = source.Insertions(this.buffer, counter);
        int count = counter[0];
        for (int i=0 ; i < count ; i++) {
          long entry = buffer[i];
          int arg2 = BinaryTableUpdater.Arg2(entry);
          if (!target.Contains(arg2))
            throw InsertionForeignKeyViolation(BinaryTableUpdater.Arg1(entry), arg2);
        }
      }

      target.CheckDeletedKeys(this);
    }

    public void WasDeleted(int arg2) {
      // arg2 is guaranteed to have been deleted and not reinserted
      if (source.Contains2(arg2))
        throw DeletionForeignKeyViolation(arg2);
    }

    //////////////////////////////////////////////////////////////////////////////

    private ForeignKeyViolationException InsertionForeignKeyViolation(int surr1, int surr2) {
      Obj[] tuple = new Obj[] {source.store1.SurrToValue(surr1), source.store2.SurrToValue(surr2)};
      return ForeignKeyViolationException.BinaryUnary(source.relvarName, 2, target.relvarName, tuple);
    }

    private ForeignKeyViolationException DeletionForeignKeyViolation(int surr2) {
      int surr1 = source.table.Restrict2(surr2)[0]; //## BAD: VERY INEFFICIENT
      Obj obj2 = source.store2.SurrToValue(surr2);
      Obj[] tuple = new Obj[] {source.store1.SurrToValue(surr1), obj2};
      return ForeignKeyViolationException.BinaryUnary(source.relvarName, 1, target.relvarName, tuple, obj2);
    }
  }
}
