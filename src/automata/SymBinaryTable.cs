namespace Cell.Runtime {
  public class SymBinaryTable {
    OneWayBinTable table = new OneWayBinTable();
    public SurrObjMapper mapper;

    int eqCount = 0;


    public SymBinaryTable(SurrObjMapper mapper) {
      this.mapper = mapper;
      Check();
    }

    public void Check() {
      // table.Check();
    }

    public int Size() {
      return (table.count + eqCount) / 2;
    }

    public bool Contains(int surr1, int surr2) {
      return table.Contains(surr1, surr2);
    }

    public bool Contains(int surr) {
      return table.ContainsKey(surr);
    }

    public int Count(int surr12) {
      return table.Count(surr12);
    }

    public int[] Restrict(int surr) {
      return table.Restrict(surr);
    }

    public int Lookup(int surr) {
      return table.Lookup(surr);
    }

    public Iter GetIter() {
      return new Iter(RawCopy(), false);
    }

    public Iter GetIter(int surr) {
      return new Iter(table.Restrict(surr), true);
    }

    public void Insert(int surr1, int surr2) {
      table.Insert(surr1, surr2);
      if (surr1 != surr2)
        table.Insert(surr2, surr1);
      else
        eqCount++;
      Check();
    }

    public void Clear() {
      table = new OneWayBinTable();
      eqCount = 0;
      Check();
    }

    public void Delete(int surr1, int surr2) {
      table.Delete(surr1, surr2);
      if (surr1 != surr2)
        table.Delete(surr2, surr1);
      else
        eqCount--;
      Check();
    }

    public Obj Copy() {
      return Copy(new SymBinaryTable[] {this});
    }

    public int[] RawCopy() {
      return table.CopySym(eqCount);
    }

    //////////////////////////////////////////////////////////////////////////////

    public static Obj Copy(SymBinaryTable[] tables) {
      int size = 0;
      for (int i=0 ; i < tables.Length ; i++)
        size += tables[i].Size();

      if (size == 0)
        return EmptyRelObj.singleton;

      Obj[] objs1 = new Obj[size];
      Obj[] objs2 = new Obj[size];

      int[] buffer = new int[32];

      int next = 0;

      for (int iT=0 ; iT < tables.Length ; iT++) {
        SymBinaryTable table = tables[iT];
        OneWayBinTable oneWayTable = table.table;
        SurrObjMapper mapper = table.mapper;

        int len = oneWayTable.column.Length;
        for (int iS=0 ; iS < len ; iS++) {
          int count1 = oneWayTable.Count(iS);
          if (count1 != 0) {
            if (count1 > buffer.Length)
              buffer = new int[Array.Capacity(buffer.Length, count1)];
            Obj obj1 = mapper(iS);
            int _count1 = oneWayTable.Restrict(iS, buffer);
            Debug.Assert(_count1 == count1);
            for (int i=0 ; i < count1 ; i++) {
              int surr2 = buffer[i];
              if (iS <= surr2) {
                objs1[next] = obj1;
                objs2[next++] = mapper(surr2);
              }
            }
          }
        }
      }
      Debug.Assert(next == size);

      return Builder.CreateBinRel(objs1, objs2, size); //## THIS COULD BE MADE MORE EFFICIENT
    }

    ////////////////////////////////////////////////////////////////////////////

    public class Iter {
      int[] entries;
      int next;
      int end;
      bool singleCol;

      public Iter(int[] entries, bool singleCol) {
        this.entries = entries;
        this.singleCol = singleCol;
        next = 0;
        end = entries.Length;
      }

      public bool Done() {
        return next >= end;
      }

      public int Get1() {
        return entries[next];
      }

      public int Get2() {
        Debug.Assert(!singleCol);
        return entries[next+1];
      }

      public void Next() {
        next += singleCol ? 1 : 2;
      }
    }
  }
}
