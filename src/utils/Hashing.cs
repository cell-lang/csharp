namespace Cell.Runtime {
  public class Hashing {
    public const uint NULL_HASHCODE = 2147483648; // 2 ^ 31

    public static uint Hashcode(uint n) {
      return (uint) n;
    }

    public static uint Hashcode64(long n) {
      return Hashcode64((ulong) n);
    }

    public static uint Hashcode64(ulong n) {
      return Hashcode((uint) n, (uint) (n >> 32));
    }

    public static uint Hashcode(uint n1, uint n2) {
      return Hash6432shift(n1, n2);
    }

    public static uint Hashcode(uint n1, uint n2, uint n3) {
      return JenkinsHash(n1, n2, n3);
    }

    //////////////////////////////////////////////////////////////////////////////

    public static int Hashcode(int n) {
      return (int) Hashcode((uint) n);
    }

    public static int Hashcode(int n1, int n2) {
      return (int) Hashcode((uint) n1, (uint) n2);
    }

    public static int Hashcode(int n1, int n2, int n3) {
      return (int) Hashcode((uint) n1, (uint) n2, (uint) n3);
    }

    //////////////////////////////////////////////////////////////////////////////

    private static uint JenkinsHash(uint a, uint b, uint c) {
      a = a - b;  a = a - c;  a = a ^ (c >> 13);
      b = b - c;  b = b - a;  b = b ^ (a << 8);
      c = c - a;  c = c - b;  c = c ^ (b >> 13);
      a = a - b;  a = a - c;  a = a ^ (c >> 12);
      b = b - c;  b = b - a;  b = b ^ (a << 16);
      c = c - a;  c = c - b;  c = c ^ (b >> 5);
      a = a - b;  a = a - c;  a = a ^ (c >> 3);
      b = b - c;  b = b - a;  b = b ^ (a << 10);
      c = c - a;  c = c - b;  c = c ^ (b >> 15);
      return c;
    }

    private static uint Hash6432shift(uint a, uint b) {
      ulong key = ((ulong) a) | (((ulong) b) << 32);
      key = (~key) + (key << 18); // key = (key << 18) - key - 1;
      key = key ^ (key >> 31);
      key = key * 21; // key = (key + (key << 2)) + (key << 4);
      key = key ^ (key >> 11);
      key = key + (key << 6);
      key = key ^ (key >> 22);
      return (uint) key;
    }

    private static ulong Murmur64(ulong h) {
      h ^= h >> 33;
      h *= 0xff51afd7ed558ccdL;
      h ^= h >> 33;
      h *= 0xc4ceb9fe1a85ec53L;
      h ^= h >> 33;
      return h;
    }

    private static uint Murmur64to32(uint a, uint b) {
      return (uint) Murmur64(((ulong) a) | (((ulong) b) << 32));
    }

    private static uint Wang32hash(uint key) {
      key = ~key + (key << 15); // key = (key << 15) - key - 1;
      key = key ^ (key >> 12);
      key = key + (key << 2);
      key = key ^ (key >> 4);
      key = key * 2057; // key = (key + (key << 3)) + (key << 11);
      key = key ^ (key >> 16);
      return key;
    }
  }
}
