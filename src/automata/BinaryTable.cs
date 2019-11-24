namespace Cell.Runtime {
  public class BinaryTable {
    OneWayBinTable table1 = new OneWayBinTable();
    OneWayBinTable table2 = new OneWayBinTable();

    public SurrObjMapper mapper1, mapper2;


    public BinaryTable(SurrObjMapper mapper1, SurrObjMapper mapper2) {
      this.mapper1 = mapper1;
      this.mapper2 = mapper2;
      Check();
    }

    public void Check() {
      // table1.Check();
      // table2.Check();
    }

    public int Size() {
      return table1.count;
    }

    public bool Contains(int surr1, int surr2) {
      return table1.Contains(surr1, surr2);
    }

    public bool Contains1(int surr1) {
      return table1.ContainsKey(surr1);
    }

    public bool Contains2(int surr2) {
      if (table2.count == 0 & table1.count > 0)
        table2.InitReverse(table1);
      return table2.ContainsKey(surr2);
    }

    public int Count1(int surr1) {
      return table1.Count(surr1);
    }

    public int Count2(int surr2) {
      if (table2.count == 0 & table1.count > 0)
        table2.InitReverse(table1);
      return table2.Count(surr2);
    }

    public int[] Restrict1(int surr) {
      return table1.Restrict(surr);
    }

    public int[] Restrict2(int surr) {
      if (table2.count == 0 & table1.count > 0)
        table2.InitReverse(table1);
      return table2.Restrict(surr);
    }

    public int Lookup1(int surr) {
      return table1.Lookup(surr);
    }

    public int Lookup2(int surr) {
      if (table2.count == 0 & table1.count > 0)
        table2.InitReverse(table1);
      return table2.Lookup(surr);
    }

    public Iter GetIter() {
      return new Iter(table1.Copy(), false);
    }

    public Iter GetIter1(int surr1) {
      return new Iter(Restrict1(surr1), true);
    }

    public Iter GetIter2(int surr2) {
      return new Iter(Restrict2(surr2), true);
    }

    public bool Insert(int surr1, int surr2) {
      bool wasNew = table1.Insert(surr1, surr2);
      if (wasNew && table2.count > 0)
        table2.InsertUnique(surr2, surr1);
      Check();
      return wasNew;
    }

    public void Clear() {
      table1 = new OneWayBinTable();
      table2 = new OneWayBinTable();
      Check();
    }

    public bool Delete(int surr1, int surr2) {
      bool wasThere = table1.Delete(surr1, surr2);
      if (wasThere & table2.count > 0)
        table2.Delete(surr2, surr1);
      Check();
      return wasThere;
    }

    public void Delete1(int surr1, int[] surrs2) {
      int count = table1.Count(surr1);
      table1.DeleteByKey(surr1, surrs2);
      if (table2.count != 0)
        for (int i=0 ; i < count ; i++)
          table2.Delete(surrs2[i], surr1);
    }

    public void Delete2(int surr2, int[] surrs1) {
      if (table2.count == 0 & table1.count > 0)
        table2.InitReverse(table1);
      int count = table2.Count(surr2);
      table2.DeleteByKey(surr2, surrs1);
      for (int i=0 ; i < count ; i++)
        table1.Delete(surrs1[i], surr2);
    }

    public Obj Copy(bool flipped) {
      return Copy(new BinaryTable[] {this}, flipped);
    }

    //////////////////////////////////////////////////////////////////////////////

    public bool Col1IsKey() {
      return table1.IsMap();
    }

    public bool Col2IsKey() {
      if (table2.count == 0 & table1.count > 0)
        table2.InitReverse(table1);
      return table2.IsMap();
    }

    //////////////////////////////////////////////////////////////////////////////

    public static Obj Copy(BinaryTable[] tables, bool flipped) {
      int count = 0;
      for (int i=0 ; i < tables.Length ; i++)
        count += tables[i].Size();

      if (count == 0)
        return EmptyRelObj.singleton;

      Obj[] objs1 = new Obj[count];
      Obj[] objs2 = new Obj[count];

      int[] buffer = new int[32];

      int next = 0;
      for (int iT=0 ; iT < tables.Length ; iT++) {
        BinaryTable table = tables[iT];
        OneWayBinTable oneWayTable = table.table1;

        SurrObjMapper mapper1 = table.mapper1;
        SurrObjMapper mapper2 = table.mapper2;

        int len = oneWayTable.column.Length;
        for (int iS=0 ; iS < len ; iS++) {
          int count1 = oneWayTable.Count(iS);
          if (count1 != 0) {
            if (count1 > buffer.Length)
              buffer = new int[Array.Capacity(buffer.Length, count1)];
            Obj obj1 = mapper1(iS);
            int _count1 = oneWayTable.Restrict(iS, buffer);
            Debug.Assert(_count1 == count1);
            for (int i=0 ; i < count1 ; i++) {
              objs1[next] = obj1;
              objs2[next++] = mapper2(buffer[i]);
            }
          }
        }
      }
      Debug.Assert(next == count);

      return Builder.CreateBinRel(flipped ? objs2 : objs1, flipped ? objs1 : objs2); //## THIS COULD BE MADE MORE EFFICIENT
    }

    //////////////////////////////////////////////////////////////////////////////

    public class Iter {
      int[] entries;
      bool singleCol;
      int next;
      int end;

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
