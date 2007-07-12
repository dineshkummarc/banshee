using System;

namespace Narlie.Compiler.CodeParser
{
    public enum ScopeAction {
        Push,
        Pop
    }

    public class ScopeToken : Token
    {
        private ScopeAction action;

        public ScopeToken(ScopeAction action) : base(Tag.Scope)
        {
            this.action = action;
        }
        
        public override string ToString()
        {
            return action == ScopeAction.Push ? "(" : ")";
        }

        public ScopeAction Action {
            get { return action; }
        }
    }
}
