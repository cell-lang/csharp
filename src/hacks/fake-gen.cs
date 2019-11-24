using Cell.Runtime;


namespace Cell.Generated {
  public class Generated {
    public static readonly string[] embeddedSymbols = {
      "false",
      "true",
      "void",
      "string",
      "date",
      "time",
      "nothing",
      "just",
      "success",
      "failure"
    };

    public static void Main(string[] args) {
      System.Console.WriteLine("Hello world!");
    }

    ////////////////////////////////////////////////////////////////////////////

    static Obj ConvertGenericTaggedValue(Obj tag, Obj obj) {
      return Builder.CreateTaggedObj(tag.GetSymbId(), obj);
    }


    public sealed class Parser : Cell.Runtime.Parser {
      public Parser(TokenStream tokens) : base(tokens) {

      }

      protected override Obj CreateTaggedObj(ushort tagId, Obj obj) {
        return ConvertGenericTaggedValue(SymbObj.Get(tagId), obj);
      }
    }
  }
}