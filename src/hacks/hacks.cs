using System.Collections.Generic;
using Cell.Runtime;


namespace Cell.Compiler {
  static class Hacks {
    private static Dictionary<Obj, Obj> attachments =
      new Dictionary<Obj, Obj>(new IdentityEqualityComparer<Obj>());

    static public void Attach(Obj target, Obj attachment) {
      attachments[target] = attachment;
    }

    static public Obj Fetch(Obj target) {
      if (attachments.ContainsKey(target))
        return Builder.CreateTaggedObj(SymbObj.JustSymbId, attachments[target]);
      else
        return SymbObj.Get(SymbObj.NothingSymbId);
    }

    //////////////////////////////////////////////////////////////////////////////

    private static Dictionary<Obj, Obj> cachedSourceFileLocation =
      new Dictionary<Obj, Obj>(new IdentityEqualityComparer<Obj>());

    static public void SetSourceFileLocation(Obj ast, Obj value) {
      cachedSourceFileLocation[ast] = value;
    }

    static public Obj GetSourceFileLocation(Obj ast) {
      if (cachedSourceFileLocation.ContainsKey(ast))
        return cachedSourceFileLocation[ast];
      else
        return null;
    }
  }
}
