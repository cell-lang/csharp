using System;


namespace Cell.Runtime {
  public class RecordObj : NeBinRelObj {
    internal ushort[] fieldIds;


    //## HERE I SHOULD BE PASSING LABEL OBJECTS AS WELL...
    public RecordObj(ushort[] fieldIds, Obj[] values) {
      Debug.Assert(fieldIds.Length > 0);
      for (int i=1 ; i < fieldIds.Length ; i++)
        Debug.Assert(SymbObj.CompSymbs(fieldIds[i-1], fieldIds[i]) == 1);

      data = BinRelObjData((uint) fieldIds.Length);
      extraData = NeBinRelObjExtraData();

      this.fieldIds = fieldIds;
      col2 = values;
      isMap = true;
    }

    //////////////////////////////////////////////////////////////////////////////

    public override Obj SetKeyValue(Obj key, Obj value) {
      BuildCol1();
      return base.SetKeyValue(key, value);
    }

    public override Obj DropKey(Obj key) {
      if (!Contains1(key))
        return this;
      BuildCol1();
      return base.DropKey(key);
    }

    //////////////////////////////////////////////////////////////////////////////

    public override bool IsNeMap() {
      return true;
    }

    public override bool IsNeRecord() {
      return true;
    }

    //////////////////////////////////////////////////////////////////////////////

    public override bool Contains1(Obj obj) {
      return obj.IsSymb() && HasField(obj.GetSymbId());
    }

    public override bool Contains2(Obj obj) {
      for (int i=0 ; i < col2.Length ; i++)
        if (col2[i].IsEq(obj))
          return true;
      return false;
    }

    public override bool Contains(Obj obj1, Obj obj2) {
      if (!obj1.IsSymb())
        return false;
      ushort keyId = obj1.GetSymbId();
      int idx = GetFieldIdx(obj1.GetSymbId());
      return idx != -1 && col2[idx].IsEq(obj2);
    }

    public override bool HasField(ushort fieldId) {
      return GetFieldIdx(fieldId) != -1;
    }

    public override BinRelIter GetBinRelIter() {
      BuildCol1();
      return base.GetBinRelIter();
    }

    public override BinRelIter GetBinRelIterByCol1(Obj obj) {
      BuildCol1();
      return base.GetBinRelIterByCol1(obj);
    }

    public override BinRelIter GetBinRelIterByCol2(Obj obj) {
      BuildCol1();
      return base.GetBinRelIterByCol2(obj);
    }

    public override Obj Lookup(Obj key) {
      if (key.IsSymb()) {
        int idx = GetFieldIdx(key.GetSymbId());
        if (idx != -1)
          return col2[idx];
      }
      throw ErrorHandler.SoftFail("Key not found:", "collection", this, "key", key);
    }

    public override Obj LookupField(ushort fieldId) {
      return col2[GetFieldIdx(fieldId)];
    }

    //////////////////////////////////////////////////////////////////////////////

    public override int InternalOrder(Obj other) {
      Debug.Assert(GetSize() == other.GetSize());

      Obj[] col, otherCol;
      int size = GetSize();
      NeBinRelObj otherRel = (NeBinRelObj) other;

      if (other is RecordObj) {
        RecordObj otherRecord = (RecordObj) other;

        ushort[] otherFieldIds = otherRecord.fieldIds;
        if (fieldIds != otherFieldIds)
          for (int i=0 ; i < size ; i++) {
            int res = SymbObj.CompSymbs(fieldIds[i], otherFieldIds[i]);
            if (res != 0)
              return res;
          }
      }
      else {
        BuildCol1();

        col = col1;
        otherCol = otherRel.Col1();
        for (int i=0 ; i < size ; i++) {
          int ord = col[i].QuickOrder(otherCol[i]);
          if (ord != 0)
            return ord;
        }
      }

      col = col2;
      otherCol = otherRel.Col2();
      for (int i=0 ; i < size ; i++) {
        int ord = col[i].QuickOrder(otherCol[i]);
        if (ord != 0)
          return ord;
      }

      return 0;
    }

    public override uint Hashcode() {
      if (hcode == Hashing.NULL_HASHCODE) {
        long hcode = 0;
        for (int i=0 ; i < fieldIds.Length ; i++)
          hcode += Hashing.Hashcode(SymbObj.Hashcode(fieldIds[i]), col2[i].Hashcode());
        hcode = Hashing.Hashcode64(hcode);
        if (hcode == Hashing.NULL_HASHCODE)
          hcode++;
      }
      return hcode;
    }

    //////////////////////////////////////////////////////////////////////////////

    internal override Obj[] Col1() {
      BuildCol1();
      return col1;
    }

    //////////////////////////////////////////////////////////////////////////////

    private int GetFieldIdx(ushort fieldId) {
      int len = fieldIds.Length;
      for (int i=0 ; i < len ; i++)
        if (fieldIds[i] == fieldId)
          return i;
      return -1;
    }

    private void BuildCol1() {
      if (col1 == null) {
        int len = fieldIds.Length;
        col1 = new Obj[len];
        hashcodes1 = new uint[len];
        for (int i=0 ; i < len ; i++) {
          Obj symbObj = SymbObj.Get(fieldIds[i]);
          col1[i] = symbObj;
          hashcodes1[i] = symbObj.Hashcode();
        }
      }
    }
  }
}
