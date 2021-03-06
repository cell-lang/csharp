namespace Cell.Runtime {
  public abstract class OptTagRecObj : Obj {
    uint hcode = Hashing.NULL_HASHCODE;
    Obj innerObj;


    public override Obj GetInnerObj() {
      if (innerObj == null)
        innerObj = Builder.CreateRecord(GetFieldIds(), GetValues());
      return innerObj;
    }

    //////////////////////////////////////////////////////////////////////////////

    public override uint Hashcode() {
      if (hcode == Hashing.NULL_HASHCODE) {
        ushort[] fieldIds = GetFieldIds();
        ulong code = 0;
        for (int i=0 ; i < fieldIds.Length ; i++)
          code += Hashing.Hashcode(SymbObj.Get(fieldIds[i]).Hashcode(), LookupField(fieldIds[i]).Hashcode());
        hcode = Hashing.Hashcode(SymbObj.Get(GetTagId()).Hashcode(), Hashing.Hashcode64(code));
        if (hcode == Hashing.NULL_HASHCODE)
          hcode++;
      }
      return hcode;
    }

    public override TypeCode GetTypeCode() {
      return TypeCode.TAGGED_VALUE;
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.OptTagRecObj(this);
    }

    //////////////////////////////////////////////////////////////////////////////

    protected Obj[] GetValues() {
      ushort[] fieldIds = GetFieldIds();
      int len = fieldIds.Length;
      Obj[] values = new Obj[len];
      for (int i=0 ; i < len ; i++)
        values[i] = LookupField(fieldIds[i]);
      return values;
    }

    protected abstract int CountFields();
    protected abstract ushort[] GetFieldIds();
  }
}
