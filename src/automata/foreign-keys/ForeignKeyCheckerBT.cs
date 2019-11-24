namespace Cell.Runtime {
  // bin_rel(a, b) -> ternary_rel(a, b, _)

  public class ForeignKeyCheckerBT : TernaryTableUpdater.DeleteChecker {
    BinaryTableUpdater source;
    TernaryTableUpdater target;

    private int[] counter = new int[0];
    private long[] buffer = new long[256];


    public ForeignKeyCheckerBT(BinaryTableUpdater source, TernaryTableUpdater target) {
      Debug.Assert(source.store1 == target.store1 & source.store2 == target.store2);

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
          int arg2 = BinaryTableUpdater.Arg2(entry);
          if (!target.Contains12(arg1, arg2))
            throw InsertionForeignKeyViolation(arg1, arg2);
        }
      }

      target.CheckDeletes123(this);
    }

    //////////////////////////////////////////////////////////////////////////////

    public void MightHaveBeenDeleted(int arg1, int arg2, int arg3) {
      if (source.Contains(arg1, arg2) && !target.Contains12(arg1, arg2))
        throw DeletionForeignKeyViolation(arg1, arg2, arg3);
    }

    //////////////////////////////////////////////////////////////////////////////

    private ForeignKeyViolationException InsertionForeignKeyViolation(int surr1, int surr2) {
      Obj obj1 = source.store1.SurrToValue(surr1);
      Obj obj2 = source.store2.SurrToValue(surr2);
      return ForeignKeyViolationException.BinaryTernary(source.relvarName, target.relvarName, obj1, obj2);
    }

    private ForeignKeyViolationException DeletionForeignKeyViolation(int surr1, int surr2, int surr3) {
      Obj obj1 = source.store1.SurrToValue(surr1);
      Obj obj2 = source.store2.SurrToValue(surr2);
      Obj obj3 = target.store3.SurrToValue(surr3);
      return ForeignKeyViolationException.BinaryTernary(source.relvarName, target.relvarName, obj1, obj2, obj3);
    }
  }
}
