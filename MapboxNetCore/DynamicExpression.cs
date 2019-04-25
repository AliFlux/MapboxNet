using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Text.RegularExpressions;

namespace MapboxNetCore
{
    public class DynamicExpression : DynamicObject
    {
        public string PropertyGetKeyword { get; set; } = "_";

        public Action<DynamicExpression, object> PropertySet;
        public Func<DynamicExpression, object> PropertyGet;
        public Func<DynamicExpression, object[], object> MethodCall;

        public Func<string, string> TransformToken = (string a) => a;

        public string Expression
        {
            get
            {
                return string.Join(".", tokens);
            }
        }

        List<string> tokens = new List<string>();

        public DynamicExpression(params string[] tokens)
        {
            this.tokens.AddRange(tokens);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder.Name == PropertyGetKeyword)
            {
                result = PropertyGet(this);
                return true;
            }

            this.tokens.Add(TransformToken(binder.Name));
            result = this;
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            this.tokens.Add(TransformToken(binder.Name));
            PropertySet?.Invoke(this, value);
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            this.tokens.Add(TransformToken(binder.Name));
            result = MethodCall(this, args);
            return true;
        }
    }
}
