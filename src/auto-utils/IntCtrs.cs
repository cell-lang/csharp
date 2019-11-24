using System.Collections.Generic;


namespace Cell.Runtime {
  class IntCtrs {
    IntMap map = new IntMap();

    public void Increment(int id) {
      int counter = map.HasKey(id) ? map.Get(id) : 0;
      map.Set(id, counter + 1);
    }

    public bool TryDecrement(int id) {
      if (map.HasKey(id)) {
        int counter = map.Get(id) - 1;
        if (counter > 0)
          map.Set(id, counter);
        else
          map.Reset(id);
        return true;
      }
      else
        return false;
    }

    public void Set(int id, int counter) {
      if (counter != 0)
        map.Set(id, counter);
      else
        map.Reset(id);
    }

    public int Get(int id) {
      return map.HasKey(id) ? map.Get(id) : 0;
    }

    //////////////////////////////////////////////////////////////////////////////

    //## REPLACE WITH IntIntMap ONCE DELETION AND UPDATE HAVE BEEN IMPLEMENTED
    private class IntMap {
      Dictionary<int, int> map = new Dictionary<int, int>();

      public void Set(int key, int value) {
        map[key] = value;
      }

      public void Reset(int key) {
        map.Remove(key);
      }

      public int Get(int key) {
        return map[key];
      }

      public bool HasKey(int key) {
        return map.ContainsKey(key);
      }
    }
  }
}
