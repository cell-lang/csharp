namespace Cell.Runtime {
  public class NeTreeMapObj : Obj {
    Node rootNode;
    NeBinRelObj packed;

    //////////////////////////////////////////////////////////////////////////////

    private NeTreeMapObj(Node rootNode) {
      data = BinRelObjData((uint) rootNode.Size());
      extraData = NeBinRelObjExtraData();
      this.rootNode = rootNode;
    }

    public NeTreeMapObj(Obj key, Obj value) {
      data = BinRelObjData(1);
      extraData = NeBinRelObjExtraData();
      rootNode = new StdNode(key, value, key.Hashcode());
    }

    public NeTreeMapObj(Obj[] keys, Obj[] values, uint[] hashcodes, int first, int count) {
      data = BinRelObjData((uint) count);
      extraData = NeBinRelObjExtraData();
      rootNode = NewNode(keys, values, hashcodes, first, count);
    }

    //////////////////////////////////////////////////////////////////////////////

    public override Obj SetKeyValue(Obj key, Obj value) {
      if (rootNode != null)
        return new NeTreeMapObj(rootNode.Insert(key, value, key.Hashcode()));
      else
        return packed.SetKeyValue(key, value);
    }

    public override Obj DropKey(Obj key) {
      if (rootNode != null) {
        Node newRoot = rootNode.Remove(key, key.Hashcode());
        if (newRoot == rootNode)
          return this;
        if (newRoot != null)
          return new NeTreeMapObj(newRoot);
        else
          return EmptyRelObj.singleton;
      }
      else
        return packed.DropKey(key);
    }

    //////////////////////////////////////////////////////////////////////////////

    public override bool IsNeMap() {
      return true;
    }

    public override bool IsNeRecord() {
      return rootNode != null ? rootNode.KeysAreSymbols() : packed.IsNeRecord();
    }

    //////////////////////////////////////////////////////////////////////////////

    public override bool Contains1(Obj key) {
      return rootNode != null ? rootNode.Lookup(key, key.Hashcode()) != null : packed.Contains1(key);
    }

    public override bool Contains2(Obj obj) {
      return Packed().Contains2(obj);
    }

    public override bool Contains(Obj key, Obj value) {
      if (rootNode != null) {
        Obj currValue = rootNode.Lookup(key, key.Hashcode());
        return currValue != null && currValue.IsEq(value);
      }
      else
        return packed.Contains(key, value);
    }

    //////////////////////////////////////////////////////////////////////////////

    public override bool HasField(ushort fieldId) {
      return Contains1(SymbObj.Get(fieldId));
    }

    //////////////////////////////////////////////////////////////////////////////

    public override BinRelIter GetBinRelIter() {
      return Packed().GetBinRelIter();
    }

    public override BinRelIter GetBinRelIterByCol1(Obj key) {
      if (rootNode != null) {
        Obj value = rootNode.Lookup(key, key.Hashcode());
        if (value != null)
          return new BinRelIter(new Obj[] {key}, new Obj[] {value});
        else
          return new BinRelIter(Array.emptyObjArray, Array.emptyObjArray);
      }
      else
        return packed.GetBinRelIterByCol1(key);
    }

    public override BinRelIter GetBinRelIterByCol2(Obj obj) {
      return Packed().GetBinRelIterByCol2(obj);
    }

    //////////////////////////////////////////////////////////////////////////////

    public override Obj Lookup(Obj key) {
      if (rootNode != null) {
        Obj value = rootNode.Lookup(key, key.Hashcode());
        if (value == null)
          throw ErrorHandler.SoftFail("Key not found:", "collection", this, "key", key);
        return value;
      }
      else
        return packed.Lookup(key);
    }

    public override Obj LookupField(ushort fieldId) {
      return Lookup(SymbObj.Get(fieldId));
    }

    //////////////////////////////////////////////////////////////////////////////

    public override int InternalOrder(Obj other) {
      return Packed().InternalOrder(other);
    }

    public override uint Hashcode() {
      return Packed().Hashcode();
    }

    public override TypeCode GetTypeCode() {
      return TypeCode.NE_BIN_REL;
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.NeBinRelObj(Packed());
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public NeBinRelObj Packed() {
      if (rootNode != null) {
        Debug.Assert(packed == null);
        int size = GetSize();
        Obj[] keys = new Obj[size];
        Obj[] values = new Obj[size];
        int count = rootNode.Traverse(keys, values, 0);
        Debug.Assert(count == size);
        // packed = new NeBinRelObj(keys, values, true);
        packed = (NeBinRelObj) Builder.CreateMap(keys, values); //## BAD BAD BAD
        rootNode = null;
      }
      return packed;
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    private interface Node {
      int Size();
      Obj Lookup(Obj key, uint hashcode);

      StdNode Insert(Obj key, Obj value, uint hashcode);
      Node Remove(Obj key, uint hashcode);

      Node Merge(Node right);

      int Traverse(Obj[] keys, Obj[] values, int offset);
      bool KeysAreSymbols();

      string[] ToStrings();
    }


    private static Node NewNode(Obj[] keys, Obj[] values, uint[] hashcodes, int first, int count) {
      Debug.Assert(count > 0);
      if (count > 1)
        return new ArraysNode(keys, values, hashcodes, first, count);
      else
        return new StdNode(keys[first], values[first], hashcodes[first]);
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    private sealed class StdNode : Node {
      internal Obj key;
      internal Obj value;
      internal Node left, right;
      internal int size;
      internal uint hashcode;


      internal StdNode(Obj key, Obj value, uint hashcode, Node left, Node right) {
        this.key = key;
        this.value = value;
        this.hashcode = hashcode;
        this.size = 1 + (left != null ? left.Size() : 0) + (right != null ? right.Size() : 0);
        this.left = left;
        this.right = right;

        if (left != null) {
          if (left is StdNode) {
            StdNode node = (StdNode) left;
            Debug.Assert(
              node.hashcode < hashcode || (node.hashcode == hashcode && node.key.QuickOrder(key) < 0)
            );
          }
          else {
            ArraysNode node = (ArraysNode) left;
            int last = node.first + node.count - 1;
            Debug.Assert(
              node.hashcodes[last] < hashcode ||
              (node.hashcodes[last] == hashcode && node.keys[last].QuickOrder(key) < 0)
            );
          }
        }

        if (right != null) {
          if (right is StdNode) {
            StdNode node = (StdNode) right;
            Debug.Assert(
              hashcode < node.hashcode || (node.hashcode == hashcode && key.QuickOrder(node.key) < 0)
            );
          }
          else {
            ArraysNode node = (ArraysNode) right;
            int first = node.first;
            Debug.Assert(
              hashcode < node.hashcodes[first] ||
              (node.hashcodes[first] == hashcode && key.QuickOrder(node.keys[first]) < 0)
            );
          }
        }
      }

      internal StdNode(Obj key, Obj value, uint hashcode) {
        this.key = key;
        this.value = value;
        this.size = 1;
        this.hashcode = hashcode;
      }

      ////////////////////////////////////////////////////////////////////////////

      public int Size() {
        return size;
      }

      public Obj Lookup(Obj aKey, uint aHashcode) {
        int ord = Order(aKey, aHashcode);

        if (ord > 0) // search key < node key, searching the left node
          return left != null ? left.Lookup(aKey, aHashcode) : null;
        else if (ord < 0) // node key < search key, searching the right node
          return right != null ? right.Lookup(aKey, aHashcode) : null;
        else
          return value;
      }

      ////////////////////////////////////////////////////////////////////////////

      public StdNode Insert(Obj aKey, Obj aValue, uint aHashcode) {
        int ord = Order(aKey, aHashcode);

        if (ord > 0) {
          Node node = left != null ? left.Insert(aKey, aValue, aHashcode) : new StdNode(aKey, aValue, aHashcode);
          return new StdNode(key, value, hashcode, node, right);
        }
        else if (ord < 0) {
          Node node = right != null ? right.Insert(aKey, aValue, aHashcode) : new StdNode(aKey, aValue, aHashcode);
          return new StdNode(key, value, hashcode, left, node);
        }
        else {
          return new StdNode(key, aValue, hashcode, left, right);
        }
      }

      public Node Remove(Obj aKey, uint aHashcode) {
        int ord = Order(aKey, aHashcode);

        if (ord > 0) {
          // search key < node key
          Node node = left != null ? left.Remove(aKey, aHashcode) : null;
          return node != left ? new StdNode(key, value, hashcode, node, right) : this;
        }
        else if (ord < 0) {
          // node key < search key
          Node node = right != null ? right.Remove(aKey, aHashcode) : null;
          return node != right ? new StdNode(key, value, hashcode, left, node) : this;
        }
        else {
          if (left == null)
            return right;
          else if (right == null)
            return left;
          else
            return left.Merge(right);
        }
      }

      ////////////////////////////////////////////////////////////////////////////

      private int Order(Obj aKey, uint aHashcode) {
        return aHashcode < hashcode ? 1 : (aHashcode > hashcode ? -1 : key.QuickOrder(aKey));
      }

      public Node Merge(Node other) {
        return new StdNode(key, value, hashcode, left, right != null ? right.Merge(other) : other);
      }

      ////////////////////////////////////////////////////////////////////////////

      public int Traverse(Obj[] keys, Obj[] values, int offset) {
        if (left != null)
          offset = left.Traverse(keys, values, offset);
        keys[offset] = key;
        values[offset++] = value;
        if (right != null)
          offset = right.Traverse(keys, values, offset);
        return offset;
      }

      public bool KeysAreSymbols() {
        return (left == null || left.KeysAreSymbols()) && key.IsSymb() && (right == null || right.KeysAreSymbols());
      }

      ////////////////////////////////////////////////////////////////////////////

      public override string ToString() {
        string[] strs = ToStrings();
        string str = "";
        for (int i=0 ; i < strs.Length ; i++) {
          if (i > 0)
            str += "\n";
          str += strs[i];
        }
        return str;
      }

      public string[] ToStrings() {
        string[] leftStrs = left != null ? left.ToStrings() : new string[] {"null"};
        string[] rightStrs = right != null ? right.ToStrings() : new string[] {"null"};
        int count = 1 + leftStrs.Length + rightStrs.Length;
        string[] strs = new string[count];
        strs[0] = string.Format("StdNode: key = {0}, value = {1}, hashcode = {2}", key, value, hashcode);
        for (int i=0 ; i < leftStrs.Length ; i++)
          strs[i+1] = "  " + leftStrs[i];
        for (int i=0 ; i < rightStrs.Length ; i++)
          strs[i+1+leftStrs.Length] = "  " + rightStrs[i];
        return strs;
      }
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    private sealed class ArraysNode : Node {
      internal Obj[] keys;
      internal Obj[] values;
      internal uint[] hashcodes;
      internal int first;
      internal int count;


      internal ArraysNode(Obj[] keys, Obj[] values, uint[] hashcodes, int first, int count) {
        Debug.Assert(count >= 2);
        this.keys = keys;
        this.values = values;
        this.hashcodes = hashcodes;
        this.first = first;
        this.count = count;
      }

      ////////////////////////////////////////////////////////////////////////////

      public int Size() {
        return count;
      }

      public Obj Lookup(Obj key, uint hashcode) {
        int idx = KeyIdx(key, hashcode);
        return idx >= 0 ? values[idx] : null;
      }

      ////////////////////////////////////////////////////////////////////////////

      public int Traverse(Obj[] keys, Obj[] values, int offset) {
        for (int i=0 ; i < count ; i++) {
          keys[offset + i] = this.keys[first + i];
          values[offset + i] = this.values[first + i];
        }
        return offset + count;
      }

      public bool KeysAreSymbols() {
        for (int i=0 ; i < count ; i++)
          if (!keys[first+i].IsSymb())
            return false;
        return true;
      }

      ////////////////////////////////////////////////////////////////////////////

      public StdNode Insert(Obj key, Obj value, uint hashcode) {
        Node right = null, left = null;

        int lastIdx = first + count - 1;
        int idx = KeyIdx(key, hashcode);

        if (idx >= 0) {
          // <key> was found
          int leftCount = idx - first;
          if (leftCount > 1)
            left = NewNode(keys, values, hashcodes, first, leftCount);
          else if (leftCount == 1)
            left = new StdNode(keys[first], values[first], hashcodes[first]);

          int rightCount = lastIdx - idx;
          if (rightCount > 1)
            right = NewNode(keys, values, hashcodes, idx + 1, rightCount);
          else if (rightCount == 1)
            right = new StdNode(keys[lastIdx], values[lastIdx], hashcodes[lastIdx]);
        }
        else {
          // <key> was not found
          int insIdx = -idx - 1;

          int leftCount = insIdx - first;
          if (leftCount > 1)
            left = NewNode(keys, values, hashcodes, first, leftCount);
          else if (leftCount == 1)
            left = new StdNode(keys[first], values[first], hashcodes[first]);

          int rightCount = lastIdx - insIdx + 1;
          if (rightCount > 1)
            right = NewNode(keys, values, hashcodes, insIdx, rightCount);
          else if (rightCount == 1)
            right = new StdNode(keys[insIdx], values[insIdx], hashcodes[insIdx]);
        }

        return new StdNode(key, value, hashcode, left, right);
      }

      ////////////////////////////////////////////////////////////////////////////

      public Node Remove(Obj key, uint hashcode) {
        int idx = KeyIdx(key, hashcode);
        if (idx < 0)
          return this;

        int lastIdx = first + count - 1;

        int countl = idx - first;
        int countr = lastIdx - idx;

        if (countl > 1) {
          if (countr > 1) {
            if (countl > countr) { // countl > countr >= 2  =>  countl >= 3
              Node left = NewNode(keys, values, hashcodes, first, countl-1);
              Node right = NewNode(keys, values, hashcodes, idx+1, countr);
              return new StdNode(keys[idx-1], values[idx-1], hashcodes[idx-1], left, right);
            }
            else { // countr >= countl >= 2
              Node left = NewNode(keys, values, hashcodes, first, countl);
              Node right = NewNode(keys, values, hashcodes, idx+2, countr-1);
              return new StdNode(keys[idx+1], values[idx+1], hashcodes[idx+1], left, right);
            }
          }
          else if (countr == 1) {
            Node left = NewNode(keys, values, hashcodes, first, countl);
            return new StdNode(keys[lastIdx], values[lastIdx], hashcodes[lastIdx], left, null);
          }
          else {
            return NewNode(keys, values, hashcodes, first, countl);
          }
        }
        else if (countl == 1) {
          if (countr > 1) {
            Node right = NewNode(keys, values, hashcodes, idx+1, countr);
            return new StdNode(keys[first], values[first], hashcodes[first], null, right);
          }
          else if (countr == 1) {
            Obj[] remKeys = new Obj[2];
            Obj[] remValues = new Obj[2];
            uint[] remHashcodes = new uint[2];
            remKeys[0] = keys[first];
            remKeys[1] = keys[lastIdx];
            remValues[0] = values[first];
            remValues[1] = values[lastIdx];
            remHashcodes[0] = hashcodes[first];
            remHashcodes[1] = hashcodes[lastIdx];
            return NewNode(remKeys, remValues, remHashcodes, 0, 2);
          }
          else {
            return new StdNode(keys[first], values[first], hashcodes[first]);
          }
        }
        else {
          if (countr > 1) {
            return NewNode(keys, values, hashcodes, idx+1, countr);
          }
          else if (countr == 1) {
            return new StdNode(keys[lastIdx], values[lastIdx], hashcodes[lastIdx]);
          }
          else {
            return null;
          }
        }
      }

      ////////////////////////////////////////////////////////////////////////////

      public Node Merge(Node aNode) {
        if (aNode is StdNode) {
          StdNode node = (StdNode) aNode;
          Node newLeft = node.left != null ? Merge(node.left) : this;
          return new StdNode(node.key, node.value, node.hashcode, newLeft, node.right);
        }
        else {
          ArraysNode node = (ArraysNode) aNode;
          if (count > node.count) { // count > node.count >= 2  =>  count >= 3
            Node left = NewNode(keys, values, hashcodes, first, count-1);
            int idx = first + count - 1;
            return new StdNode(keys[idx], values[idx], hashcodes[idx], left, node);

          }
          else { // node.count >= count >= 2  =>  count >= 2
            Node right = NewNode(node.keys, node.values, node.hashcodes, node.first+1, node.count-1);
            return new StdNode(node.keys[node.first], node.values[node.first], node.hashcodes[node.first], this, right);
          }
        }
      }

      ////////////////////////////////////////////////////////////////////////

      public override string ToString() {
        string[] strs = ToStrings();
        string str = "";
        for (int i=0 ; i < strs.Length ; i++) {
          if (i > 0)
            str += "\n";
          str += strs[i];
        }
        return str;
      }

      public string[] ToStrings() {
        string[] strs = new string[count+1];
        strs[0] = "ArraysNode";
        for (int i=0 ; i < count ; i++)
          strs[i+1] = string.Format("{0, 13}:  {1,-40} -> {2}", hashcodes[first+i], keys[first+i], values[first+i]);
        return strs;
      }

      ////////////////////////////////////////////////////////////////////////

      private bool LowerThan(Obj key, uint hashcode, int idx) {
        return hashcode < hashcodes[idx] || (hashcode == hashcodes[idx] && key.QuickOrder(keys[idx]) < 0);
      }

      private bool GreaterThan(Obj key, uint hashcode, int idx) {
        return hashcode > hashcodes[idx] || (hashcode == hashcodes[idx] && key.QuickOrder(keys[idx]) > 0);
      }

      private int KeyIdx(Obj key, uint hashcode) {
        int res = _keyIdx(key, hashcode);
        int end = first + count;
        int last = end - 1;
        if (res >= 0) {
          Debug.Assert(res >= first & res <= last);
          Debug.Assert(key.IsEq(keys[res]));
          Debug.Assert(res == first || GreaterThan(key, hashcode, res-1)); // keys[res-1] < key
          Debug.Assert(res == last  || LowerThan(key, hashcode, res+1));   // key < keys[res+1]
        }
        else {
          int insIdx = -res - 1;
          Debug.Assert(insIdx >= first & insIdx <= end);
          Debug.Assert(insIdx == first || GreaterThan(key, hashcode, insIdx-1)); // keys[insIdx-1] < key
          Debug.Assert(insIdx == end   || LowerThan(key, hashcode, insIdx));     // key < keys[insIdx]
        }
        return res;
      }

      private int _keyIdx(Obj key, uint hashcode) {
        int end = first + count;
        int idx = Array.AnyIndexOrEncodedInsertionPointIntoSortedArray(hashcodes, first, end, hashcode);
        if (idx < 0)
          return idx;

        int ord = key.QuickOrder(keys[idx]);
        if (ord == 0)
          return idx;

        while (idx > first && hashcodes[idx-1] == hashcode)
          idx--;

        while (idx < end && hashcodes[idx] == hashcode) {
          ord = key.QuickOrder(keys[idx]);
          if (ord == 0)
            return idx;
          else if (ord > 0)
            idx++;
          else
            break;
        }

        return -idx - 1;
      }
    }
  }
}
