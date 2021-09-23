using DotNetWeb.Core.Expressions;

namespace DotNetWeb.Core
{
    public class Symbol
    {
        public Symbol(Id id, dynamic value)
        {
            Id = id;
            Value = value;
        }

        public Symbol(Id id, Expression attributes)
        {
            Attributes = attributes;
            Id = id;
        }

        public Id Id { get; }
        public dynamic Value { get; set; }
        public Expression Attributes { get; }
    }
}
