using System;


namespace Cell.Runtime {
  public abstract class Obj {
    public uint extraData;
    public ulong data;


    public bool IsBlankObj() {
      return extraData == BlankObjExtraData();
    }

    public bool IsNullObj() {
      return extraData == NullObjExtraData();
    }

    public bool IsSymb() {
      return extraData == SymbObjExtraData();
    }

    public bool IsBool() {
      if (IsSymb()) {
        uint symbId = GetSymbId();
        return symbId == SymbObj.FalseSymbId | symbId == SymbObj.TrueSymbId;
      }
      else
        return false;
    }

    public bool IsInt() {
      return extraData == IntObjExtraData();
    }

    public bool IsFloat() {
      return extraData == FloatObjExtraData();
    }

    public bool IsSeq() {
      return IsEmptySeq() | IsNeSeq();
    }

    public bool IsEmptySeq() {
      return extraData == EmptySeqObjExtraData();
    }

    public bool IsNeSeq() {
      return extraData == NeSeqObjExtraData();
    }

    public bool IsEmptyRel() {
      return extraData == EmptyRelObjExtraData();
    }

    public bool IsSet() {
      return IsNeSet() | IsEmptyRel();
    }

    public bool IsNeSet() {
      return extraData == NeSetObjExtraData();
    }

    public bool IsBinRel() {
      return IsNeBinRel() | IsEmptyRel();
    }

    public bool IsNeBinRel() {
      return extraData == NeBinRelObjExtraData();
    }

    public bool IsTernRel() {
      return IsNeTernRel() | IsEmptyRel();
    }

    public bool IsNeTernRel() {
      return extraData == NeTernRelObjExtraData();
    }

    public bool IsTagged() {
      return extraData == tagIntObjId | extraData >= refTagObjId;
    }

    public bool IsTaggedInt() {
      return (extraData == tagIntObjId || (extraData == refTagObjId && GetInnerObj().IsInt()));
    }

    //////////////////////////////////////////////////////////////////////////////

    public bool IsSymb(int id) {
      return IsSymb() && GetSymbId() == id;
    }

    public bool IsInt(long n) {
      return IsInt() && GetLong() == n;
    }

    public bool IsFloat(double x) {
      return IsFloat() && GetDouble() == x;
    }

    public bool IsTaggedInt(int tagId) {
      return IsTaggedInt() && GetTagId() == tagId;
    }

    public bool IsTaggedInt(int tagId, long value) {
      return IsTaggedInt() && (GetTagId() == tagId & GetInnerLong() == value);
    }

    //////////////////////////////////////////////////////////////////////////////

    public ushort GetSymbId() {
      return (ushort) data;
    }

    public bool GetBool() {
      return GetSymbId() == SymbObj.TrueSymbId;
    }

    public long GetLong() {
      return (long) data;
    }

    public double GetDouble() {
      return Miscellanea.ULongBitsToDoubleBits(data);
    }

    public int GetSize() {
      return (int) data;
    }

    public ushort GetTagId() {
      return (ushort) (data & 0xFFFF);
    }

    public SymbObj GetTag() {
      return SymbObj.Get(GetTagId());
    }

    //////////////////////////////////////////////////////////////////////////////

    public bool IsEq(Obj other) {
      if (this == other)
        return true;

      if (data != other.data)
        return false;

      uint otherExtraData = other.extraData;
      if (extraData != otherExtraData)
        return false;

      if (IsInlineObj())
        return true;

      return InternalOrder(other) == 0;
    }

    // this <  other -> -1
    // this == other ->  0
    // this >  other ->  1
    public int QuickOrder(Obj other) {
      if (this == other)
        return 0;

      ulong otherData = other.data;
      if (data != otherData)
        return data < otherData ? -1 : 1;

      uint otherExtraData = other.extraData;
      if (extraData != otherExtraData)
        return extraData < otherExtraData ? -1 : 1;

      if (IsInlineObj())
        return 0;

      return InternalOrder(other);
    }

    //////////////////////////////////////////////////////////////////////////////

    // Called only when data == other.data and extraData == other.extraData
    public abstract int InternalOrder(Obj other);

    public abstract uint Hashcode();

    //////////////////////////////////////////////////////////////////////////////

    protected static ulong SymbObjData(ushort id) {
      return id;
    }

    protected static ulong BoolObjData(bool value) {
      return value ? SymbObj.TrueSymbId : SymbObj.FalseSymbId;
    }

    protected static ulong IntObjData(long value) {
      return (ulong) value;
    }

    protected static ulong FloatObjData(double value) {
      return Miscellanea.DoubleBitsToULongBits(value);
    }

    // 32 bit 0 padding - 32 bit size/length
    private static ulong CollObjData(uint size) {
      return size;
    }

    protected static ulong SeqObjData(uint length) {
      return CollObjData(length);
    }

    protected static ulong SetObjData(uint size) {
      return CollObjData(size);
    }

    protected static ulong BinRelObjData(uint size) {
      return CollObjData(size);
    }

    protected static ulong TernRelObjData(uint size) {
      return CollObjData(size);
    }

    // 48 bit 0 padding - 16 bit tag id
    protected static ulong TagObjData(ushort tag) {
      return tag;
    }

    // 48 bit value - 16 bit tag id
    protected static ulong TagIntObjData(ushort tag, long value) {
      return ((ulong) (value << 16)) | tag;
    }

    // 32 bit 0 padding - 16 bit optional field mask - 16 bit tag id
    public static ulong OptTagRecObjData(ushort tag, ushort optFieldsMask) {
      return (((ulong) optFieldsMask) << 16) | tag;
    }

    //////////////////////////////////////////////////////////////////////////////

    private const uint blankObjId           = 0;
    private const uint nullObjId            = 1;
    private const uint symbObjId            = 2;
    private const uint intObjId             = 3;
    private const uint floatObjId           = 4;
    private const uint emptySeqObjId        = 5;
    private const uint emptyRelObjId        = 6;
    private const uint tagIntObjId          = 7;

    private const uint neSeqObjId           = 16;
    private const uint neSetObjId           = 17;
    private const uint neBinRelObjId        = 18;
    private const uint neTernRelObjId       = 19;
    private const uint refTagObjId          = 20;

    private const uint optTagRecObjBaseId   = 21;

    bool IsInlineObj() {
      return extraData < 16;
    }

    protected static uint BlankObjExtraData()          {return blankObjId;         }
    protected static uint NullObjExtraData()           {return nullObjId;          }
    protected static uint SymbObjExtraData()           {return symbObjId;          }
    protected static uint IntObjExtraData()            {return intObjId;           }
    protected static uint FloatObjExtraData()          {return floatObjId;         }
    protected static uint EmptySeqObjExtraData()       {return emptySeqObjId;      }
    protected static uint EmptyRelObjExtraData()       {return emptyRelObjId;      }
    protected static uint TagIntObjExtraData()         {return tagIntObjId;        }

    protected static uint NeSeqObjExtraData()          {return neSeqObjId;         }
    protected static uint NeSetObjExtraData()          {return neSetObjId;         }
    protected static uint NeBinRelObjExtraData()       {return neBinRelObjId;      }
    protected static uint NeTernRelObjExtraData()      {return neTernRelObjId;     }
    protected static uint RefTagObjExtraData()         {return refTagObjId;        }

    protected static uint OptTagRecObjExtraData(uint idx) {
      return idx + optTagRecObjBaseId;
    }

    //////////////////////////////////////////////////////////////////////////////

    public enum TypeCode {
      SYMBOL, INTEGER, FLOAT, EMPTY_SEQ, EMPTY_REL, NE_SEQ, NE_SET, NE_BIN_REL, NE_TERN_REL, TAGGED_VALUE
    };

    public abstract TypeCode GetTypeCode();

    //////////////////////////////////////////////////////////////////////////////

    public abstract void Visit(ObjVisitor visitor);

    //////////////////////////////////////////////////////////////////////////////
    ///////////////////////////// Sequence operations ////////////////////////////

    public virtual bool    IsNeIntSeq()                     {return false;}
    public virtual bool    IsNeFloatSeq()                   {return false;}

    public virtual Obj     GetObjAt(long idx)               {throw ErrorHandler.InternalFail(this);}
    public virtual bool    GetBoolAt(long idx)              {throw ErrorHandler.InternalFail(this);}
    public virtual long    GetLongAt(long idx)              {throw ErrorHandler.InternalFail(this);}
    public virtual double  GetDoubleAt(long idx)            {throw ErrorHandler.InternalFail(this);}

    public virtual SeqObj GetSlice(long first, long count)  {throw ErrorHandler.InternalFail(this);}

    public virtual bool[]   GetBoolArray(bool[] buffer)     {throw ErrorHandler.InternalFail(this);}
    public virtual long[]   GetLongArray(long[] buffer)     {throw ErrorHandler.InternalFail(this);}
    public virtual double[] GetDoubleArray(double[] buffer) {throw ErrorHandler.InternalFail(this);}
    public virtual Obj[]    GetObjArray(Obj[] buffer)       {throw ErrorHandler.InternalFail(this);}

    public virtual byte[]   GetByteArray()                  {throw ErrorHandler.InternalFail(this);}

    public virtual SeqIter GetSeqIter()                     {throw ErrorHandler.InternalFail(this);}

    public virtual SeqObj Reverse()                         {throw ErrorHandler.InternalFail(this);}
    public virtual SeqObj Concat(Obj seq)                   {throw ErrorHandler.InternalFail(this);}

    public virtual NeSeqObj Append(Obj obj)                 {throw ErrorHandler.InternalFail(this);}
    public virtual NeSeqObj Append(bool value)              {throw ErrorHandler.InternalFail(this);}
    public virtual NeSeqObj Append(long value)              {throw ErrorHandler.InternalFail(this);}
    public virtual NeSeqObj Append(double value)            {throw ErrorHandler.InternalFail(this);}

    // Copy-on-write update
    public virtual NeSeqObj UpdatedAt(long idx, Obj value)  {throw ErrorHandler.InternalFail(this);}

    //////////////////////////////////////////////////////////////////////////////
    /////////////////////////////// Set operations ///////////////////////////////

    public virtual bool Contains(Obj obj)     {throw ErrorHandler.InternalFail(this);}

    public virtual SetIter GetSetIter()       {throw ErrorHandler.InternalFail(this);}
    public virtual SeqObj  InternalSort()     {throw ErrorHandler.InternalFail(this);}
    public virtual Obj     RandElem()         {throw ErrorHandler.InternalFail(this);}

    public virtual Obj     Insert(Obj obj)    {throw ErrorHandler.InternalFail(this);}
    public virtual Obj     Remove(Obj obj)    {throw ErrorHandler.InternalFail(this);}

    //////////////////////////////////////////////////////////////////////////////
    ///////////////////////// Binary relation operations /////////////////////////

    public virtual bool IsNeMap()                           {return false;}
    public virtual bool IsNeRecord()                        {return false;}

    public virtual bool Contains1(Obj val)                  {throw ErrorHandler.InternalFail(this);}
    public virtual bool Contains2(Obj val)                  {throw ErrorHandler.InternalFail(this);}
    public virtual bool Contains(Obj val1, Obj val2)        {throw ErrorHandler.InternalFail(this);}

    public virtual bool HasField(ushort fieldId)            {throw ErrorHandler.InternalFail(this);}

    public virtual Obj Lookup(Obj key)                      {throw ErrorHandler.InternalFail(this);}
    public virtual Obj LookupField(ushort fieldId)          {throw ErrorHandler.InternalFail(this);}

    public virtual BinRelIter GetBinRelIter()               {throw ErrorHandler.InternalFail(this);}

    public virtual BinRelIter GetBinRelIterByCol1(Obj obj)  {throw ErrorHandler.InternalFail(this);}
    public virtual BinRelIter GetBinRelIterByCol2(Obj obj)  {throw ErrorHandler.InternalFail(this);}

    public virtual Obj SetKeyValue(Obj key, Obj value)      {throw ErrorHandler.InternalFail(this);}
    public virtual Obj DropKey(Obj key)                     {throw ErrorHandler.InternalFail(this);}

    //////////////////////////////////////////////////////////////////////////////
    ///////////////////////// Ternary relation operations ////////////////////////

    // public virtual bool Contains1(Obj val)                                {throw ErrorHandler.InternalFail(this);}
    // public virtual bool Contains2(Obj val)                                {throw ErrorHandler.InternalFail(this);}
    public virtual bool Contains3(Obj val)                                {throw ErrorHandler.InternalFail(this);}
    public virtual bool Contains12(Obj val1, Obj val2)                    {throw ErrorHandler.InternalFail(this);}
    public virtual bool Contains13(Obj val1, Obj val3)                    {throw ErrorHandler.InternalFail(this);}
    public virtual bool Contains23(Obj val2, Obj val3)                    {throw ErrorHandler.InternalFail(this);}
    public virtual bool Contains(Obj val1, Obj val2, Obj val3)            {throw ErrorHandler.InternalFail(this);}

    public virtual TernRelIter GetTernRelIter()                           {throw ErrorHandler.InternalFail(this);}

    public virtual TernRelIter GetTernRelIterByCol1(Obj val)              {throw ErrorHandler.InternalFail(this);}
    public virtual TernRelIter GetTernRelIterByCol2(Obj val)              {throw ErrorHandler.InternalFail(this);}
    public virtual TernRelIter GetTernRelIterByCol3(Obj val)              {throw ErrorHandler.InternalFail(this);}

    public virtual TernRelIter GetTernRelIterByCol12(Obj val1, Obj val2)  {throw ErrorHandler.InternalFail(this);}
    public virtual TernRelIter GetTernRelIterByCol13(Obj val1, Obj val3)  {throw ErrorHandler.InternalFail(this);}
    public virtual TernRelIter GetTernRelIterByCol23(Obj val2, Obj val3)  {throw ErrorHandler.InternalFail(this);}

    //////////////////////////////////////////////////////////////////////////////
    /////////////////////////// Tagged obj operations ///////////////////////////

    public virtual Obj  GetInnerObj()  {throw ErrorHandler.InternalFail(this);}
    public virtual long GetInnerLong() {throw ErrorHandler.InternalFail(this);}

    public virtual bool IsSyntacticSugaredString() {return false;}
    public virtual string GetString() {throw ErrorHandler.InternalFail(this);}

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public virtual Obj PackForString() {
      return this;
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public bool[] GetBoolArray() {
      return GetBoolArray(null);
    }

    public long[] GetLongArray() {
      return GetLongArray(null);
    }

    public double[] GetDoubleArray() {
      return GetDoubleArray(null);
    }

    public Obj[] GetObjArray() {
      return GetObjArray(null);
    }

    public int SignedHashcode() {
      return (int) Hashcode();
    }

    public bool IsIntSeq() {
      return IsEmptySeq() || IsNeIntSeq();
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public override string ToString() {
      DataWriter writer = IO.StringDataWriter();
      ObjPrinter.Print(this, writer, 90);
      return writer.Output();
    }

    public Obj Printed() {
      return Miscellanea.StrToObj(ToString());
    }
  }
}
