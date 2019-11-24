namespace Cell.Runtime {
  // unary_rel(x) -> binary_rel(_, x);

  public sealed class ForeignKeyCheckerUB2 {
    private UnaryTableUpdater source;
    private BinaryTableUpdater target;

    private int[] counter = new int[1];
    private int[] intBuff = new int[256];


    public ForeignKeyCheckerUB2(UnaryTableUpdater source, BinaryTableUpdater target) {
      Debug.Assert(source.store == target.store2);

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
      //## METHODS OF source MUST NOT BE CALLED WHILE WE'RE ITERATING OVER source.insertList
      for (int i=0 ; i < source.insertCount ; i++) {
        int elt = source.insertList[i];
        if (!target.Contains2(elt))
          throw InsertionForeignKeyViolationException(elt);
      }
    }

    private void CheckTargetClear() {
      Debug.Assert(target.WasCleared());

      if (source.clear)
        return;

      UnaryTable.Iter it = source.table.GetIter();
      while (!it.Done()) {
        int elt = it.Get();
        if (source.Contains(elt) && !target.Contains2(elt))
            throw DeletionForeignKeyViolationException(target.AnyDeletedArg1(elt), elt);
        it.Next();
      }
    }

    private void CheckTargetDeletes() {
      int[] buffer = target.Deletes2(intBuff, counter);
      int count = counter[0];
      for (int i=0 ; i < count ; i++) {
        int elt = buffer[i];
        if (source.Contains(elt) && !target.Contains2(elt))
          throw DeletionForeignKeyViolationException(target.AnyDeletedArg1(elt), elt);
      }
    }

    //////////////////////////////////////////////////////////////////////////////

    private ForeignKeyViolationException InsertionForeignKeyViolationException(int surr) {
      Obj obj = source.store.SurrToValue(surr);
      return ForeignKeyViolationException.UnaryBinary(source.relvarName, 2, target.relvarName, obj);
    }

    private ForeignKeyViolationException DeletionForeignKeyViolationException(int surr1, int surr2) {
      Obj obj1 = target.store1.SurrToValue(surr1);
      Obj obj2 = source.store.SurrToValue(surr2);
      Obj[] tuple = new Obj[] {obj1, obj2};
      return ForeignKeyViolationException.UnaryBinary(source.relvarName, 2, target.relvarName, tuple);
    }
  }
}
