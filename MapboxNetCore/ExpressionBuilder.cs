using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace MapboxNetCore
{
    [JsonConverter(typeof(ExpressionBuilderConverter))]
    public class ExpressionBuilder : DynamicObject
    {
        public Func<string, string> TransformToken = (string a) => a;
        public Func<string, object> Execute = (string a) => a;
        
        public int TokenCount { get; private set; } = 0;
        public string Expression { get; private set; }
        public bool Consumed { get; private set; } = false;

        //public bool QuickExecute { get; private set; } = false;
        public string ExecuteKey { get; set; } = "";

        public ExpressionBuilder(params string[] tokens)
        {
            foreach (var token in tokens)
                append(token);
        }

        void append(string token)
        {
            if (TokenCount > 0)
                Expression += "." + token;
            else
                Expression = token;

            TokenCount++;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var token = TransformToken(binder.Name);
            append(token);

            result = this;
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var token = TransformToken(binder.Name);

            Expression = "(" + Expression + " = " + serializeObject(value) + ")";
            TokenCount++;

            Execute(Expression);

            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (ExecuteKey == "")
            {
                var token = TransformToken(binder.Name);

                var serializedArgs = args.Select(arg => serializeObject(arg));

                var call = token + "(" + string.Join(", ", serializedArgs) + ")";
                append(call);

                result = Execute(Expression);

                return true;
            }
            else if (ExecuteKey == binder.Name)
            {
                result = Execute(Expression);
                return true;
            }
            else// if (binder.Name != ExecuteKey)
            {
                var token = TransformToken(binder.Name);

                var serializedArgs = args.Select(arg => serializeObject(arg));

                var call = token + "(" + string.Join(", ", serializedArgs) + ")";
                append(call);

                result = this;
                return true;
            }
        }

        static string serializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static ExpressionBuilder operator +(ExpressionBuilder c1, object c2)
        {
            c1.Expression = "(" + c1.Expression + " + " + serializeObject(c2) + ")";
            c1.TokenCount++;
            return c1;
        }

        public static ExpressionBuilder operator +(object c2, ExpressionBuilder c1)
        {
            c1.Expression = "(" + serializeObject(c2) + " + " + c1.Expression + ")";
            c1.TokenCount++;
            return c1;
        }

        public static ExpressionBuilder operator -(ExpressionBuilder c1, object c2)
        {
            c1.Expression = "(" + c1.Expression + " - " + serializeObject(c2) + ")";
            c1.TokenCount++;
            return c1;
        }

        public static ExpressionBuilder operator -(object c2, ExpressionBuilder c1)
        {
            c1.Expression = "(" + serializeObject(c2) + " - " + c1.Expression + ")";
            c1.TokenCount++;
            return c1;
        }

        public static ExpressionBuilder operator *(ExpressionBuilder c1, object c2)
        {
            c1.Expression = "(" + c1.Expression + " * " + serializeObject(c2) + ")";
            c1.TokenCount++;
            return c1;
        }

        public static ExpressionBuilder operator *(object c2, ExpressionBuilder c1)
        {
            c1.Expression = "(" + serializeObject(c2) + " * " + c1.Expression + ")";
            c1.TokenCount++;
            return c1;
        }

        public static ExpressionBuilder operator /(ExpressionBuilder c1, object c2)
        {
            c1.Expression = "(" + c1.Expression + " / " + serializeObject(c2) + ")";
            c1.TokenCount++;
            return c1;
        }

        public static ExpressionBuilder operator /(object c2, ExpressionBuilder c1)
        {
            c1.Expression = "(" + serializeObject(c2) + " / " + c1.Expression + ")";
            c1.TokenCount++;
            return c1;
        }

        public static ExpressionBuilder operator %(ExpressionBuilder c1, object c2)
        {
            c1.Expression = "(" + c1.Expression + " % " + serializeObject(c2) + ")";
            c1.TokenCount++;
            return c1;
        }

        public static ExpressionBuilder operator %(object c2, ExpressionBuilder c1)
        {
            c1.Expression = "(" + serializeObject(c2) + " % " + c1.Expression + ")";
            c1.TokenCount++;
            return c1;
        }


        class ExpressionBuilderConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var expressionBuilder = (value as ExpressionBuilder);
                writer.WriteValue(expressionBuilder.Expression);
                expressionBuilder.Consumed = true;
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override bool CanRead
            {
                get { return false; }
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(ExpressionBuilder);
            }
        }
    }
}
