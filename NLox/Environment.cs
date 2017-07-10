using System.Collections.Generic;

namespace NLox
{
    public class Environment
    {
        private readonly Environment enclosing;
        private readonly Dictionary<string, object> values = new Dictionary<string, object>();

        public Environment() { }

        public Environment(Environment enclosing) => this.enclosing = enclosing;

        public void assign(Token name, object value)
        {
            if (values.ContainsKey(name.lexeme))
            {
                values[name.lexeme] = value;
                return;
            }

            if (enclosing != null)
            {
                enclosing.assign(name, value);
                return;
            }

            throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
            
        }

        public void define(string name, object value)
        {
            values[name] = value;
        }

        public object get(Token name)
        {
            if (values.TryGetValue(name.lexeme, out var val))
                return val;

            if (enclosing != null) return enclosing.get(name);

            throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
        }
    }
}
