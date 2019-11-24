namespace Cell.Runtime {
  // bin_rel(a, _) -> unary_rel(a);

  public class ForeignKeyCheckerICU : UnaryTableUpdater.DeleteChecker {
    IntColumnUpdater source;
    UnaryTableUpdater target;

    public ForeignKeyCheckerICU(IntColumnUpdater source, UnaryTableUpdater target) {
      Debug.Assert(source.store == target.store);
      this.source = source;
      this.target = target;
    }

    public void Check() {
      // Checking that every new entry satisfies the foreign key
      int count = source.insertCount;
      if (count > 0) {
        int[] idxs = source.insertIdxs;
        for (int i=0 ; i < count ; i++)
          if (!target.Contains(idxs[i]))
            throw ForeignKeyViolation(idxs[i], source.insertValues[i]);
      }

      count = source.updateCount;
      if (count > 0) {
        int[] idxs = source.updateIdxs;
        for (int i=0 ; i < count ; i++)
          if (!target.Contains(idxs[i]))
            throw ForeignKeyViolation(idxs[i], source.updateValues[i]);
      }

      // Checking that no entries were invalidated by a deletion on the target table
      target.CheckDeletedKeys(this);
    }

    public void WasDeleted(int surr) {
      if (source.Contains1(surr))
        throw ForeignKeyViolation(surr);
    }

    //////////////////////////////////////////////////////////////////////////////

    private ForeignKeyViolationException ForeignKeyViolation(int keySurr, long value) {
      Obj[] tuple = new Obj[] {source.store.SurrToValue(keySurr), IntObj.Get(value)};
      return ForeignKeyViolationException.BinaryUnary(source.relvarName, 1, target.relvarName, tuple);
    }

    private ForeignKeyViolationException ForeignKeyViolation(int keySurr) {
      Obj key = source.store.SurrToValue(keySurr);
      Obj[] fromTuple = new Obj[] {key, IntObj.Get(source.Lookup(keySurr))};
      return ForeignKeyViolationException.BinaryUnary(source.relvarName, 1, target.relvarName, fromTuple, key);
    }
  }
}
