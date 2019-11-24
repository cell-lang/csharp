namespace Cell.Runtime {
  public class Index {
    const int Empty = -1;

    public int[] hashtable;
    public int[] buckets;

    public Index(int size) {
      hashtable = new int[size/2];
      buckets   = new int[size];
      Array.Fill(hashtable, Empty);
      Array.Fill(buckets, Empty); //## IS THIS NECESSARY?
    }

    public void Clear() {
      Array.Fill(hashtable, Empty);
      Array.Fill(buckets, Empty); //## IS THIS NECESSARY?
    }

    public void Insert(int index, int hashcode) {
      Debug.Assert(buckets[index] == Empty);
      Debug.Assert(index < buckets.Length);

      int hashIdx = Miscellanea.UnsignedRemaider(hashcode, hashtable.Length);
      int head = hashtable[hashIdx];
      hashtable[hashIdx] = index;
      buckets[index] = head;
    }

    public void Delete(int index, int hashcode) {
      int hashIdx = Miscellanea.UnsignedRemaider(hashcode, hashtable.Length);
      int head = hashtable[hashIdx];
      Debug.Assert(head != Empty);

      if (head == index) {
        hashtable[hashIdx] = buckets[index];
        buckets[index] = Empty;
        return;
      }

      int curr = head;
      for ( ; ; ) {
        int next = buckets[curr];
        Debug.Assert(next != Empty);
        if (next == index) {
          buckets[curr] = buckets[next];
          buckets[next] = Empty;
          return;
        }
        curr = next;
      }
    }

    public int Head(int hashcode) {
      return hashtable[Miscellanea.UnsignedRemaider(hashcode, hashtable.Length)];
    }

    public int Next(int index) {
      return buckets[index];
    }

    public void Dump() {
      // System.out.Print("hashtable =");
      // if (hashtable != null)
      //   for (int i=0 ; i < hashtable.Length ; i++)
      //     System.out.Print(" " + (hashtable[i] == Empty ? "-" : Integer.ToString(hashtable[i])));
      // else
      //   System.out.Print(" null");
      // System.out.Println("");

      // System.out.Print("buckets   =");
      // if (hashtable != null)
      //   for (int i=0 ; i < buckets.Length ; i++)
      //     System.out.Print(" " + (buckets[i] == Empty ? "-" : Integer.ToString(buckets[i])));
      // else
      //   System.out.Print(" null");
      // System.out.Println("");
      // System.out.Println("");
    }
  }
}
