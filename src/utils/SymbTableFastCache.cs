namespace Cell.Runtime {
  public sealed class SymbTableFastCache {
    private const int SIZE = 4096;

    private static ulong[]  encSymbs1     = new ulong[SIZE];
    private static ushort[] encSymbsIdxs1 = new ushort[SIZE];

    private static ulong[]  encSymbs2     = new ulong[2 * SIZE];
    private static ushort[] encSymbsIdxs2 = new ushort[SIZE];

    private static ulong[]  encSymbs3;
    private static ushort[] encSymbsIdxs3;

    private static ulong[][] encSymbs;
    private static ushort[]  encSymbsIdxs;


    public static ushort EncToIdx(ulong encWord) {
      uint hashcode = Hashing.Hashcode64(encWord);
      uint idx = hashcode % SIZE;

      ulong storedEnc = encSymbs1[idx];
      if (storedEnc == encWord)
        return encSymbsIdxs1[idx];

      byte[] bytes = Decode(encWord);
      ushort symbIdx = SymbObj.BytesToIdx(bytes);

      if (storedEnc == 0) {
        encSymbs1[idx] = encWord;
        encSymbsIdxs1[idx] = symbIdx;
      }

      return symbIdx;
    }

    public static ushort EncToIdx(ulong encWord1, ulong encWord2) {
      uint hashcode1 = Hashing.Hashcode64(encWord1);
      uint hashcode2 = Hashing.Hashcode64(encWord2);
      uint idx = (31 * hashcode1 + hashcode2) % SIZE;

      ulong storedEnc1 = encSymbs2[2 * idx];
      ulong storedEnc2 = encSymbs2[2 * idx + 1];

      if (storedEnc1 == encWord1 & storedEnc2 == encWord2)
        return encSymbsIdxs2[idx];

      byte[] bytes = Decode(encWord1, encWord2);
      ushort symbIdx = SymbObj.BytesToIdx(bytes);

      if (storedEnc1 == 0) {
        encSymbs2[2 * idx] = encWord1;
        encSymbs2[2 * idx + 1] = encWord2;
        encSymbsIdxs2[idx] = symbIdx;
      }

      return symbIdx;
    }

    public static ushort EncToIdx(ulong encWord1, ulong encWord2, ulong encWord3) {
      uint hashcode1 = Hashing.Hashcode64(encWord1);
      uint hashcode2 = Hashing.Hashcode64(encWord2);
      uint hashcode3 = Hashing.Hashcode64(encWord3);
      uint idx = (31 * 31 * hashcode1 + 31 * hashcode2 + hashcode3) % SIZE;

      if (encSymbs3 == null) {
        encSymbs3     = new ulong[3 * SIZE];
        encSymbsIdxs3 = new  ushort[SIZE];
      }

      ulong storedEnc1 = encSymbs3[3 * idx];
      ulong storedEnc2 = encSymbs3[3 * idx + 1];
      ulong storedEnc3 = encSymbs3[3 * idx + 2];

      if (storedEnc1 == encWord1 & storedEnc2 == encWord2 & storedEnc3 == encWord3)
        return encSymbsIdxs3[idx];

      byte[] bytes = Decode(encWord1, encWord2, encWord3);
      ushort symbIdx = SymbObj.BytesToIdx(bytes);

      if (storedEnc1 == 0) {
        encSymbs3[3 * idx] = encWord1;
        encSymbs3[3 * idx + 1] = encWord2;
        encSymbs3[3 * idx + 2] = encWord3;
        encSymbsIdxs3[idx] = symbIdx;
      }

      return symbIdx;
    }

    public static ushort EncToIdx(ulong[] encWords, int count) {
      uint hashcode = Hashing.Hashcode64(encWords[0]);
      for (int i=1 ; i < count ; i++)
        hashcode = 31 * hashcode + Hashing.Hashcode64(encWords[i]);
      uint idx = hashcode % SIZE;

      if (encSymbs == null) {
        encSymbs = new ulong[SIZE][];
        encSymbsIdxs = new ushort[SIZE];
      }

      ulong[] storedEncs = encSymbs[idx];
      if (storedEncs != null && storedEncs.Length == count && Array.IsPrefix(storedEncs, encWords))
        return encSymbsIdxs[idx];

      byte[] bytes = Decode(encWords, count);
      ushort symbIdx = SymbObj.BytesToIdx(bytes);

      if (storedEncs == null) {
        encSymbs[idx] = Array.Take(encWords, count);
        encSymbsIdxs[idx] = symbIdx;
      }

      return symbIdx;
    }

    //////////////////////////////////////////////////////////////////////////////

    //  0         Empty
    //  1 - 26    Letter
    // 27 - 36    Digit
    // 37         Underscore (followed by a digit)
    // 38 - 63    Underscore + letter

    public static byte ENCODED_UNDERSCORE = 37;

    public static byte EncodedLetter(int ch) {
      return (byte) (ch - 'a' + 1);
    }

    public static byte EncodedDigit(int ch) {
      return (byte) (ch - '0' + 27);
    }

    public static byte EncodedUnderscoredLetter(int ch) {
      return (byte) (ch - 'a' + 38);
    }

    public static byte[] Decode(ulong encWord) {
      int size = Size(encWord);
      byte[] bytes = new byte[size];
      int idx = Decode(encWord, bytes, size-1);
      Debug.Assert(idx == -1);
      return bytes;
    }

    public static byte[] Decode(ulong encWord1, ulong encWord2) {
      int size = Size(encWord1, encWord2);
      byte[] bytes = new byte[size];
      int idx = Decode(encWord2, bytes, size-1);
      idx = Decode(encWord1, bytes, idx);
      Debug.Assert(idx == -1);
      return bytes;
    }

    public static byte[] Decode(ulong encWord1, ulong encWord2, ulong encWord3) {
      int size = Size(encWord1, encWord2, encWord3);
      byte[] bytes = new byte[size];
      int idx = Decode(encWord3, bytes, size-1);
      idx = Decode(encWord2, bytes, idx);
      idx = Decode(encWord1, bytes, idx);
      Debug.Assert(idx == -1);
      return bytes;
    }

    public static byte[] Decode(ulong[] encWords, int count) {
      int size = Size(encWords, count);
      byte[] bytes = new byte[size];
      int idx = size - 1;
      for (int i = count - 1 ; i >= 0 ; i--)
        idx = Decode(encWords[i], bytes, idx);
      Debug.Assert(idx == -1);
      return bytes;
    }

    //////////////////////////////////////////////////////////////////////////////

    private static int Size(ulong word) {
      int size = 0;
      while (word != 0) {
        int code = (int) (word & 0x3F);
        size += code >= 38 ? 2 : 1;
        word = word >> 6;
      }
      Debug.Assert(size > 0);
      return size;
    }

    private static int Size(ulong word1, ulong word2) {
      return Size(word1) + Size(word2);
    }

    private static int Size(ulong word1, ulong word2, ulong word3) {
      return Size(word1) + Size(word2) + Size(word3);
    }

    private static int Size(ulong[] words, int count) {
      int size = 0;
      for (int i=0 ; i < count ; i++)
        size += Size(words[i]);
      return size;
    }

    private static int Decode(ulong word, byte[] bytes, int idx) {
      while (word != 0) {
        int code = (int) (word & 0x3F);
        Debug.Assert(code != 0);
        if (code <= 26) {
          bytes[idx--] = (byte) (code - 1 + 'a');
        }
        else if (code <= 36) {
          bytes[idx--] = (byte) (code - 27 + '0');
        }
        else if (code == 37) {
          bytes[idx--] = (byte) '_';
        }
        else {
          bytes[idx--] = (byte) (code - 38 + 'a');
          bytes[idx--] = (byte) '_';
        }
        word = word >> 6;
      }
      return idx;
    }
  }
}
