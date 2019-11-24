using Exception = System.Exception;


namespace Cell.Runtime {
  public class KeyViolationException : Exception {
    public static int[] key_1 = new int[] {1};
    public static int[] key_2 = new int[] {2};
    public static int[] key_3 = new int[] {3};
    public static int[] key_12 = new int[] {1, 2};
    public static int[] key_13 = new int[] {1, 3};
    public static int[] key_23 = new int[] {2, 3};

    string relvarName;
    int[] key;
    Obj[] tuple1, tuple2;
    bool betweenNew;

    internal KeyViolationException(string relvarName, int[] key, Obj[] tuple1, Obj[] tuple2, bool betweenNew) {
      this.relvarName = relvarName;
      this.key = key;
      this.tuple1 = tuple1;
      this.tuple2 = tuple2;
      this.betweenNew = betweenNew;
    }

    public override string ToString() {
      bool isComposite = key.Length > 1;
      DataWriter writer = IO.StringDataWriter();
      writer.Write("Key violation: relation variable: " + relvarName + ", column");
      if (isComposite)
        writer.Write("s");
      writer.Write(":");
      for (int i=0 ; i < key.Length ; i++) {
        writer.Write(" ");
        writer.Write(key[i].ToString());
      }
      writer.Write(betweenNew ?
        "\nAttempt to insert conflicting tuples:\n" :
        "\nAttempt to insert tuple that conflicts with existing one:\n"
      );
      writer.Write("  (");
      for (int i=0 ; i < tuple1.Length ; i++) {
        if (i > 0)
          writer.Write(", ");
        ObjPrinter.Print(tuple1[i], writer);
      }
      writer.Write(")\n  (");
      for (int i=0 ; i < tuple2.Length ; i++) {
        if (i > 0)
          writer.Write(", ");
        ObjPrinter.Print(tuple2[i], writer);
      }
      writer.Write(")\n");
      return writer.Output();
    }
  }
}
