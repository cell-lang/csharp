namespace Cell.Runtime {
  public interface ObjVisitor {
    void ArrayObj(ArrayObj obj);
    void ArraySliceObj(ArraySliceObj obj);
    void BlankObj(BlankObj obj);
    void EmptyRelObj(EmptyRelObj obj);
    void EmptySeqObj(EmptySeqObj obj);
    void FloatArrayObj(FloatArrayObj obj);
    void FloatArraySliceObj(FloatArraySliceObj obj);
    void FloatObj(FloatObj obj);
    void IntArrayObj(IntArrayObj obj);
    void IntArraySliceObj(IntArraySliceObj obj);
    void SignedByteArrayObj(SignedByteArrayObj obj);
    void SignedByteArraySliceObj(SignedByteArraySliceObj obj);
    void UnsignedByteArrayObj(UnsignedByteArrayObj obj);
    void UnsignedByteArraySliceObj(UnsignedByteArraySliceObj obj);
    void ShortArrayObj(ShortArrayObj obj);
    void ShortArraySliceObj(ShortArraySliceObj obj);
    void Int32ArrayObj(Int32ArrayObj obj);
    void Int32ArraySliceObj(Int32ArraySliceObj obj);
    void IntObj(IntObj obj);
    void NeBinRelObj(NeBinRelObj obj);
    void NeSetObj(NeSetObj obj);
    void NeTernRelObj(NeTernRelObj obj);
    void NullObj(NullObj obj);
    void SymbObj(SymbObj obj);
    void TaggedIntObj(TaggedIntObj obj);
    void TaggedObj(TaggedObj obj);
    void OptTagRecObj(OptTagRecObj obj);
  }
}
