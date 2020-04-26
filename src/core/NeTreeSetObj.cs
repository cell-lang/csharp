namespace Cell.Runtime {
  public class NeTreeSetObj : Obj {
    Node rootNode;
    NeSetObj packed;

    //////////////////////////////////////////////////////////////////////////////

    private NeTreeSetObj(Node rootNode) {
      data = BinRelObjData((uint) rootNode.Size());
      extraData = NeSetObjExtraData();
      this.rootNode = rootNode;
    }

    public NeTreeSetObj(Obj elt) {
      data = SetObjData(1);
      extraData = NeSetObjExtraData();
      rootNode = new StdNode(elt, elt.Hashcode());
    }

    public NeTreeSetObj(Obj[] elts, uint[] hashcodes, int first, int count) {
      data = SetObjData((uint) count);
      extraData = NeSetObjExtraData();
      rootNode = NewNode(elts, hashcodes, first, count);
    }

    //////////////////////////////////////////////////////////////////////////////

    public override Obj Insert(Obj obj) {
      if (rootNode != null) {
        Node newRoot = rootNode.Insert(obj, obj.Hashcode());
        return newRoot != rootNode ? new NeTreeSetObj(newRoot) : this;
      }
      else
        return packed.Insert(obj);
    }

    public override Obj Remove(Obj obj) {
      if (rootNode != null) {
        Node newRoot = rootNode.Remove(obj, obj.Hashcode());
        if (newRoot == null)
          return EmptyRelObj.singleton;
        else if (newRoot == rootNode)
          return this;
        else
          return new NeTreeSetObj(newRoot);
      }
      else
        return packed.Remove(obj);
    }

    //////////////////////////////////////////////////////////////////////////////

    public override bool Contains(Obj obj) {
      return rootNode != null ? rootNode.Contains(obj, obj.Hashcode()) : packed.Contains(obj);
    }

    public override SetIter GetSetIter() {
      return Packed().GetSetIter();
    }

    public override Obj[] GetObjArray(Obj[] buffer) {
      return Packed().GetObjArray(buffer);
    }

    public override SeqObj InternalSort() {
      return Packed().InternalSort();
    }

    public override Obj RandElem() {
      return rootNode != null ? rootNode.LeftMostElt() : packed.RandElem();
    }

    //////////////////////////////////////////////////////////////////////////////

    public override int InternalOrder(Obj other) {
      return Packed().InternalOrder(other);
    }

    public override uint Hashcode() {
      return Packed().Hashcode();
    }

    public override TypeCode GetTypeCode() {
      return TypeCode.NE_SET;
    }

    public override void Visit(ObjVisitor visitor) {
      visitor.NeSetObj(Packed());
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    public NeSetObj Packed() {
      if (rootNode != null) {
        Debug.Assert(packed == null);
        int size = GetSize();
        Obj[] elts = new Obj[size];
        int count = rootNode.Traverse(elts, 0);
        Debug.Assert(count == size);
        // packed = new NeSetObj(elts);
        packed = (NeSetObj) Builder.CreateSet(elts); //## BAD BAD BAD
        rootNode = null;
      }
      return packed;
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    private interface Node {
      int Size();
      bool Contains(Obj obj, uint hashcode);

      StdNode Insert(Obj obj, uint hashcode);
      Node Remove(Obj obj, uint hashcode);
      Obj LeftMostElt();

      Node Merge(Node right);
      int Traverse(Obj[] elts, int offset);
      string[] ToStrings();
    }


    private static Node NewNode(Obj[] elts, uint[] hashcodes, int first, int count) {
      Debug.Assert(count > 0);
      if (count > 1)
        return new ArraysNode(elts, hashcodes, first, count);
      else
        return new StdNode(elts[first], hashcodes[first]);
    }

    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    private sealed class StdNode : Node {
      internal Obj elt;
      internal Node left, right;
      internal int size;
      internal uint hashcode;


      internal StdNode(Obj elt, uint hashcode, Node left, Node right) {
        this.elt = elt;
        this.hashcode = hashcode;
        this.size = 1 + (left != null ? left.Size() : 0) + (right != null ? right.Size() : 0);
        this.left = left;
        this.right = right;

        if (left != null) {
          if (left is StdNode) {
            StdNode node = (StdNode) left;
            Debug.Assert(
              node.hashcode < hashcode || (node.hashcode == hashcode && node.elt.QuickOrder(elt) < 0)
            );
          }
          else {
            ArraysNode node = (ArraysNode) left;
            int last = node.first + node.count - 1;
            Debug.Assert(
              node.hashcodes[last] < hashcode ||
              (node.hashcodes[last] == hashcode && node.elts[last].QuickOrder(elt) < 0)
            );
          }
        }

        if (right != null) {
          if (right is StdNode) {
            StdNode node = (StdNode) right;
            Debug.Assert(
              hashcode < node.hashcode || (node.hashcode == hashcode && elt.QuickOrder(node.elt) < 0)
            );
          }
          else {
            ArraysNode node = (ArraysNode) right;
            int first = node.first;
            Debug.Assert(
              hashcode < node.hashcodes[first] ||
              (node.hashcodes[first] == hashcode && elt.QuickOrder(node.elts[first]) < 0)
            );
          }
        }
      }

      internal StdNode(Obj elt, uint hashcode) {
        this.elt = elt;
        this.size = 1;
        this.hashcode = hashcode;
      }

      ////////////////////////////////////////////////////////////////////////////

      public int Size() {
        return size;
      }

      public bool Contains(Obj obj, uint objHash) {
        int ord = Order(obj, objHash);

        if (ord > 0) // search elt < node elt, searching the left node
          return left != null && left.Contains(obj, objHash);
        else if (ord < 0) // node elt < search elt, searching the right node
          return right != null && right.Contains(obj, objHash);
        else
          return true;
      }

      public Obj LeftMostElt() {
        return left != null ? left.LeftMostElt() : elt;
      }

      ////////////////////////////////////////////////////////////////////////////

      public StdNode Insert(Obj obj, uint objHash) {
        int ord = Order(obj, objHash);

        if (ord > 0) {
          Node node = left != null ? left.Insert(obj, objHash) : new StdNode(obj, objHash);
          return new StdNode(elt, hashcode, node, right);
        }
        else if (ord < 0) {
          Node node = right != null ? right.Insert(obj, objHash) : new StdNode(obj, objHash);
          return new StdNode(elt, hashcode, left, node);
        }
        else {
          return new StdNode(elt, hashcode, left, right);
        }
      }

      public Node Remove(Obj obj, uint objHash) {
        int ord = Order(obj, objHash);

        if (ord > 0) {
          // search elt < node elt
          Node node = left != null ? left.Remove(obj, objHash) : null;
          return node != left ? new StdNode(elt, hashcode, node, right) : this;
        }
        else if (ord < 0) {
          // node elt < search elt
          Node node = right != null ? right.Remove(obj, objHash) : null;
          return node != right ? new StdNode(elt, hashcode, left, node) : this;
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

      private int Order(Obj obj, uint objHash) {
        return objHash < hashcode ? 1 : (objHash > hashcode ? -1 : elt.QuickOrder(obj));
      }

      public Node Merge(Node other) {
        return new StdNode(elt, hashcode, left, right != null ? right.Merge(other) : other);
      }

      ////////////////////////////////////////////////////////////////////////////

      public int Traverse(Obj[] elts, int offset) {
        if (left != null)
          offset = left.Traverse(elts, offset);
        elts[offset++] = elt;
        if (right != null)
          offset = right.Traverse(elts, offset);
        return offset;
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
        strs[0] = string.Format("StdNode: elt = {0}, hashcode = {1}", elt, hashcode);
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
      internal Obj[] elts;
      internal uint[] hashcodes;
      internal int first;
      internal int count;


      internal ArraysNode(Obj[] elts, uint[] hashcodes, int first, int count) {
        Debug.Assert(count >= 2);
        this.elts = elts;
        this.hashcodes = hashcodes;
        this.first = first;
        this.count = count;
      }

      ////////////////////////////////////////////////////////////////////////////

      public int Size() {
        return count;
      }

      public bool Contains(Obj obj, uint hashcode) {
        return EltIdx(obj, hashcode) >= 0;
      }

      public Obj LeftMostElt() {
        return elts[first];
      }

      ////////////////////////////////////////////////////////////////////////////

      public int Traverse(Obj[] elts, int offset) {
        for (int i=0 ; i < count ; i++)
          elts[offset + i] = this.elts[first + i];
        return offset + count;
      }

      ////////////////////////////////////////////////////////////////////////////

      public StdNode Insert(Obj elt, uint hashcode) {
        Node right = null, left = null;

        int lastIdx = first + count - 1;
        int idx = EltIdx(elt, hashcode);

        if (idx >= 0) {
          // <elt> was found
          int leftCount = idx - first;
          if (leftCount > 1)
            left = NewNode(elts, hashcodes, first, leftCount);
          else if (leftCount == 1)
            left = new StdNode(elts[first], hashcodes[first]);

          int rightCount = lastIdx - idx;
          if (rightCount > 1)
            right = NewNode(elts, hashcodes, idx + 1, rightCount);
          else if (rightCount == 1)
            right = new StdNode(elts[lastIdx], hashcodes[lastIdx]);
        }
        else {
          // <elt> was not found
          int insIdx = -idx - 1;

          int leftCount = insIdx - first;
          if (leftCount > 1)
            left = NewNode(elts, hashcodes, first, leftCount);
          else if (leftCount == 1)
            left = new StdNode(elts[first], hashcodes[first]);

          int rightCount = lastIdx - insIdx + 1;
          if (rightCount > 1)
            right = NewNode(elts, hashcodes, insIdx, rightCount);
          else if (rightCount == 1)
            right = new StdNode(elts[insIdx], hashcodes[insIdx]);
        }

        return new StdNode(elt, hashcode, left, right);
      }

      ////////////////////////////////////////////////////////////////////////////

      public Node Remove(Obj elt, uint hashcode) {
        int idx = EltIdx(elt, hashcode);
        if (idx < 0)
          return this;

        int lastIdx = first + count - 1;

        int countl = idx - first;
        int countr = lastIdx - idx;

        if (countl > 1) {
          if (countr > 1) {
            if (countl > countr) { // countl > countr >= 2  =>  countl >= 3
              Node left = NewNode(elts, hashcodes, first, countl-1);
              Node right = NewNode(elts, hashcodes, idx+1, countr);
              return new StdNode(elts[idx-1], hashcodes[idx-1], left, right);
            }
            else { // countr >= countl >= 2
              Node left = NewNode(elts, hashcodes, first, countl);
              Node right = NewNode(elts, hashcodes, idx+2, countr-1);
              return new StdNode(elts[idx+1], hashcodes[idx+1], left, right);
            }
          }
          else if (countr == 1) {
            Node left = NewNode(elts, hashcodes, first, countl);
            return new StdNode(elts[lastIdx], hashcodes[lastIdx], left, null);
          }
          else {
            return NewNode(elts, hashcodes, first, countl);
          }
        }
        else if (countl == 1) {
          if (countr > 1) {
            Node right = NewNode(elts, hashcodes, idx+1, countr);
            return new StdNode(elts[first], hashcodes[first], null, right);
          }
          else if (countr == 1) {
            Obj[] remElts = new Obj[2];
            uint[] remHashcodes = new uint[2];
            remElts[0] = elts[first];
            remElts[1] = elts[lastIdx];
            remHashcodes[0] = hashcodes[first];
            remHashcodes[1] = hashcodes[lastIdx];
            return NewNode(remElts, remHashcodes, 0, 2);
          }
          else {
            return new StdNode(elts[first], hashcodes[first]);
          }
        }
        else {
          if (countr > 1) {
            return NewNode(elts, hashcodes, idx+1, countr);
          }
          else if (countr == 1) {
            return new StdNode(elts[lastIdx], hashcodes[lastIdx]);
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
          return new StdNode(node.elt, node.hashcode, newLeft, node.right);
        }
        else {
          ArraysNode node = (ArraysNode) aNode;
          if (count > node.count) { // count > node.count >= 2  =>  count >= 3
            Node left = NewNode(elts, hashcodes, first, count-1);
            int idx = first + count - 1;
            return new StdNode(elts[idx], hashcodes[idx], left, node);

          }
          else { // node.count >= count >= 2  =>  count >= 2
            Node right = NewNode(node.elts, node.hashcodes, node.first+1, node.count-1);
            return new StdNode(node.elts[node.first], node.hashcodes[node.first], this, right);
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
          strs[i+1] = string.Format("{0,13}:  {1,-40}", hashcodes[first+i], elts[first+i]);
        return strs;
      }

      ////////////////////////////////////////////////////////////////////////

      private bool LowerThan(Obj elt, uint hashcode, int idx) {
        return hashcode < hashcodes[idx] || (hashcode == hashcodes[idx] && elt.QuickOrder(elts[idx]) < 0);
      }

      private bool GreaterThan(Obj elt, uint hashcode, int idx) {
        return hashcode > hashcodes[idx] || (hashcode == hashcodes[idx] && elt.QuickOrder(elts[idx]) > 0);
      }

      private int EltIdx(Obj elt, uint hashcode) {
        int res = _eltIdx(elt, hashcode);
        int end = first + count;
        int last = end - 1;
        if (res >= 0) {
          Debug.Assert(res >= first & res <= last);
          Debug.Assert(elt.IsEq(elts[res]));
          Debug.Assert(res == first || GreaterThan(elt, hashcode, res-1)); // elts[res-1] < elt
          Debug.Assert(res == last  || LowerThan(elt, hashcode, res+1));   // elt < elts[res+1]
        }
        else {
          int insIdx = -res - 1;
          Debug.Assert(insIdx >= first & insIdx <= end);
          Debug.Assert(insIdx == first || GreaterThan(elt, hashcode, insIdx-1)); // elts[insIdx-1] < elt
          Debug.Assert(insIdx == end   || LowerThan(elt, hashcode, insIdx));     // elt < elts[insIdx]
        }
        return res;
      }

      private int _eltIdx(Obj elt, uint hashcode) {
        int end = first + count;
        int idx = Array.AnyIndexOrEncodedInsertionPointIntoSortedArray(hashcodes, first, end, hashcode);
        if (idx < 0)
          return idx;

        int ord = elt.QuickOrder(elts[idx]);
        if (ord == 0)
          return idx;

        while (idx > first && hashcodes[idx-1] == hashcode)
          idx--;

        while (idx < end && hashcodes[idx] == hashcode) {
          ord = elt.QuickOrder(elts[idx]);
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
