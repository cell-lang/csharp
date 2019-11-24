namespace Cell.Runtime {
  // tern_rel(a, b, _) -> binary_rel(a, b)

  public class ForeignKeyCheckerTB {
    TernaryTableUpdater source;
    BinaryTableUpdater target;

    int[] counter = new int[1];
    long[] longBuff = new long[256];


    public ForeignKeyCheckerTB(TernaryTableUpdater source, BinaryTableUpdater target) {
      Debug.Assert(source.store1 == target.store1);
      Debug.Assert(source.store2 == target.store2);

      this.source = source;
      this.target = target;
    }

    public void Check() {
      if (source.insertCount > 0)
        CheckSourceInsertions();

      if (target.WasCleared())
        CheckTargetClear();
      else if (target.HasPartialDeletes())
        CheckTargetDeletes();
    }

    //////////////////////////////////////////////////////////////////////////////

    private void CheckSourceInsertions() {
      int count = source.insertCount;
      int[] inserts = source.insertList;
      for (int i=0 ; i < count ; i++) {
        int offset = 3 * i;
        int arg1 = inserts[offset];
        int arg2 = inserts[offset + 1];
        if (!target.Contains(arg1, arg2))
          throw InsertionForeignKeyViolation(arg1, arg2, inserts[offset+2]);
      }
    }

    private void CheckTargetClear() {
      Debug.Assert(target.WasCleared());

      TernaryTable.Iter123 it = source.table.GetIter();
      while (!it.Done()) {
        int arg1 = it.Get1();
        int arg2 = it.Get2();
        if (source.Contains12(arg1, arg2) && !target.Contains(arg1, arg2))
          throw DeletionForeignKeyViolation(arg1, arg2);
        it.Next();
      }
    }

    private void CheckTargetDeletes() {
      long[] buffer = target.Deletes(longBuff, counter);
      int count = counter[0];
      for (int i=0 ; i < count ; i++) {
        long entry = buffer[i];
        int arg1 = BinaryTableUpdater.Arg1(entry);
        int arg2 = BinaryTableUpdater.Arg2(entry);
        if (source.Contains12(arg1, arg2) && !target.Contains(arg1, arg2))
          throw DeletionForeignKeyViolation(arg1, arg2);
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    private ForeignKeyViolationException InsertionForeignKeyViolation(int surr1, int surr2, int surr3) {
      Obj obj1 = source.store1.SurrToValue(surr1);
      Obj obj2 = source.store2.SurrToValue(surr2);
      Obj obj3 = source.store3.SurrToValue(surr3);
      Obj[] tuple = new Obj[] {obj1, obj2, obj3};
      return ForeignKeyViolationException.TernaryBinary(source.relvarName, target.relvarName, tuple);
    }

    private ForeignKeyViolationException DeletionForeignKeyViolation(int surr1, int surr2) {
      Obj obj1 = source.store1.SurrToValue(surr1);
      Obj obj2 = source.store2.SurrToValue(surr2);
      Obj obj3 = source.store3.SurrToValue(source.LookupAny12(surr1, surr2));
      Obj[] fromTuple = new Obj[] {obj1, obj2, obj3};
      Obj[] toTuple = new Obj[] {obj1, obj2};
      return ForeignKeyViolationException.TernaryBinary(source.relvarName, target.relvarName, fromTuple, toTuple);
    }
  }
}
