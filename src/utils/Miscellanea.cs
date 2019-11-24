using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;


namespace Cell.Runtime {
  public class Miscellanea {
    // str1 <  str2 -> -1
    // str1 == str2 ->  0
    // str1 >  str2 ->  1
    public static int Order(string str1, string str2) {
      return string.Compare(str1, str2);
    }

    public static string AsciiString(byte[] bytes, int len) {
      char[] chs = new char[len];
      for (int i=0 ; i < len ; i++)
        chs[i] = (char) bytes[i];
      return new string(chs);
    }

    public static string UnicodeString(long[] codePoints) {
      if (CanBeTurnedDirectlyIntoUTF16(codePoints)) {
        int len = codePoints.Length;
        char[] chs = new char[len];
        for (int i=0 ; i < len ; i++)
          chs[i] = (char) codePoints[i];
        return new string(chs);
      }
      else {
        StringBuilder sb = new StringBuilder();
        for (int i=0 ; i < codePoints.Length ; i++) {
          long cp = codePoints[i];
          if (cp < 0 | cp > 1114111)
            throw ErrorHandler.InternalFail();
          sb.Append(Char.ConvertFromUtf32((int) codePoints[i]));
        }
        return sb.ToString();
      }
    }

    public static bool CanBeTurnedDirectlyIntoUTF16(long[] codePoints) {
      int len = codePoints.Length;
      for (int i=0 ; i < len ; i++)
        if (!IsBMPCodePoint(codePoints[i]))
          return false;
      return true;
    }

    public static bool IsBMPCodePoint(int cp) {
      return IsBMPCodePoint((ulong) cp);
    }

    public static bool IsBMPCodePoint(long cp) {
      return IsBMPCodePoint((ulong) cp);
    }

    public static bool IsBMPCodePoint(ulong cp) {
      return cp < 0xD800 | (cp >= 0xDFFF & cp <= 0xFFFF);
    }

    // public static string DecodeUTF8(byte[] bytes, int index, int count) {
    //   return Encoding.UTF8.GetString(bytes, index, count);
    // }

    public static double LongBitsToDoubleBits(long value) {
      return BitConverter.Int64BitsToDouble(value);
    }

    public static long DoubleBitsToLongBits(double value) {
      return BitConverter.DoubleToInt64Bits(value);
    }

    public static double ULongBitsToDoubleBits(ulong value) {
      return BitConverter.Int64BitsToDouble((long) value);
    }

    public static ulong DoubleBitsToULongBits(double value) {
      return (ulong) BitConverter.DoubleToInt64Bits(value);
    }

    public static int[] CodePoints(string str) {
      int len = str.Length;
      int[] codePoints = new int[len];
      int count = 0;
      for (int i=0 ; i < len ; i++) {
        Char ch = str[i];
        if (Char.IsHighSurrogate(ch)) {
          i++;
          if (i < len) {
            Char ch2 = str[i];
            if (Char.IsLowSurrogate(ch2))
              codePoints[count++] = Char.ConvertToUtf32(ch, ch2);
            else
              throw new Exception("Invalid string: " + str);
          }
          else
            throw new Exception("Invalid string: " + str);
        }
        else
          codePoints[count++] = Convert.ToInt32(ch);
      }
      return count == len ? codePoints : Array.Take(codePoints, count);
    }

    public static Obj StrToObj(string str) {
      int len = str.Length;
      int[] chars = new int[len];
      int count = 0;
      for (int i=0 ; i < len ; i++) {
        Char ch = str[i];
        if (Char.IsHighSurrogate(ch)) {
          i++;
          if (i < len) {
            Char ch2 = str[i];
            if (Char.IsLowSurrogate(ch2))
              chars[count++] = Char.ConvertToUtf32(ch, ch2);
            else
              throw new Exception("Invalid string: " + str);
          }
          else
            throw new Exception("Invalid string: " + str);
        }
        else
          chars[count++] = Convert.ToInt32(ch);
      }
      return Builder.CreateTaggedObj(SymbObj.StringSymbId, Builder.CreateSeq(chars, count));
    }

    ////////////////////////////////////////////////////////////////////////////

    static Random random = new Random(0);

    public static long RandNat(long max) {
      return random.Next(max > Int32.MaxValue ? Int32.MaxValue : (int) max);
    }

    static int nextUniqueNat = 0;
    public static long UniqueNat() {
      return nextUniqueNat++;
    }

    ////////////////////////////////////////////////////////////////////////////

    public static int CastLongToInt(long x) {
      if (x == (int) x)
        return (int) x;
      else
        throw ErrorHandler.SoftFail();
    }

    ////////////////////////////////////////////////////////////////////////////

    public static long Pack(int low, int high) {
      long slot = (((long) low) & 0xFFFFFFFFL) | (((long) high) << 32);
      Debug.Assert(Low(slot) == low & High(slot) == high);
      return slot;
    }

    public static int Low(long slot) {
      return (int) (slot & 0xFFFFFFFFL);
    }

    public static int High(long slot) {
      return (int) UnsignedLeftShift64(slot, 32);
    }

    ////////////////////////////////////////////////////////////////////////////

    public static int Extend(int size, int minSize) {
      while (size < minSize)
        size *= 2;
      return size;
    }

    ////////////////////////////////////////////////////////////////////////////

    public static bool IsHexDigit(byte b) {
      char ch = (char) b;
      return ('0' <= ch & ch <= '9') | ('a' <= ch & ch <= 'f') | ('A' <= ch & ch <= 'F');
    }

    public static int HexDigitValue(byte b) {
      char ch = (char) b;
      return ch - (ch >= '0' & ch <= '9' ? '0' : (ch >= 'a' & ch <= 'f' ? 'a' : 'A'));
    }

    ////////////////////////////////////////////////////////////////////////////

    public static long UnsignedLeftShift64(long value, int shift) {
      return (long) (((ulong) value) >> shift);
    }

    public static int UnsignedLeftShift32(int value, int shift) {
      return (int) (((uint) value) >> shift);
    }

    public static int UnsignedRemaider(int value, int modulo) {
      return (int) (((ulong) value) % ((ulong) modulo));
    }

    public static bool BitIsSet64(long value, int bit) {
      return (UnsignedLeftShift64(value, bit) & 1) != 0;
    }

    public static bool BitIsSet32(int value, int bit) {
      return (UnsignedLeftShift32(value, bit) & 1) != 0;
    }
  }

  //////////////////////////////////////////////////////////////////////////////
  //////////////////////////////////////////////////////////////////////////////

  public sealed class IdentityEqualityComparer<T> : IEqualityComparer<T> where T : class {
    public int GetHashCode(T value) {
      return RuntimeHelpers.GetHashCode(value);
    }

    public bool Equals(T left, T right) {
      return ReferenceEquals(left, right);
    }
  }
}
