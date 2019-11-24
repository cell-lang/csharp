namespace Cell.Runtime {
  // unary_rel_1(x) -> unary_rel_2(x);

  public class ForeignKeyCheckerUU : UnaryTableUpdater.DeleteChecker {
    UnaryTableUpdater source;
    UnaryTableUpdater target;

    public ForeignKeyCheckerUU(UnaryTableUpdater source, UnaryTableUpdater target) {
      Debug.Assert(source.store == target.store);
      this.source = source;
      this.target = target;
    }

    public void Check() {
      int count = source.insertCount;
      if (count > 0) {
        int[] inserts = source.insertList;
        for (int i=0 ; i < count ; i++)
          if (!target.Contains(inserts[i]))
            throw ForeignKeyViolation(inserts[i]);
      }

      target.CheckDeletedKeys(this);
    }

    public void WasDeleted(int surr) {
      if (source.Contains(surr))
        throw ForeignKeyViolation(surr);
    }

    //////////////////////////////////////////////////////////////////////////////

    private ForeignKeyViolationException ForeignKeyViolation(int surr) {
      return ForeignKeyViolationException.UnaryUnary(source.relvarName, target.relvarName, source.store.SurrToValue(surr));
    }
  }
}
