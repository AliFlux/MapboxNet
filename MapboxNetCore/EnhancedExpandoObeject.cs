using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Text;

namespace MapboxNetCore
{
    public class EnhancedExpandoObeject : DynamicObject, IDictionary<string, object>
    {
        public Dictionary<string,object> Properties = new Dictionary<string, object>();

        public ICollection<string> Keys => Properties.Keys;

        public ICollection<object> Values => Properties.Values;

        public int Count => Properties.Count;

        public bool IsReadOnly => false;

        public EnhancedExpandoObeject()
        {

        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;

            // first check the Properties collection for member
            if (Properties.ContainsKey(binder.Name))
            {
                result = Properties[binder.Name];
                return true;
            }

            // failed to retrieve a property
            result = null;
            return false;
        }
        
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            // no match - set or add to dictionary
            Properties[binder.Name] = value;
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = null;
            return false;
        }

        public void Add(string key, object value)
        {
            Properties.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return Properties.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return Properties.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return Properties.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            Properties.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            Properties.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return Properties.ContainsKey(item.Key) && Properties.ContainsValue(item.Value);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return Properties.Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return Properties.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Properties.GetEnumerator();
        }

        public object this[string key]
        {
            get
            {
                if (Properties.ContainsKey(key))
                {
                    return Properties[key];
                }
                return null;
            }
            set
            {
                if (Properties.ContainsKey(key))
                {
                    Properties[key] = value;
                }
            }
        }
        
    }
}
