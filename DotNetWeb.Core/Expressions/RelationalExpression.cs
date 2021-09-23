using System;
using System.Collections.Generic;

namespace DotNetWeb.Core.Expressions
{
  public class RelationalExpression : TypedBinaryOperator
  {
    private readonly Dictionary<(Type, Type), Type> _typeRules;

    public RelationalExpression(Token token,
       TypedExpression left,
       TypedExpression right) : base(token, left, right, null)
    {
      _typeRules = new Dictionary<(Type, Type), Type>
      {
        //pendiente
        { (Type.Float, Type.Float), Type.Float },
        { (Type.Int, Type.Int), Type.Int },
        { (Type.String, Type.String), Type.String },
        { (Type.Int, Type.Float), Type.Float },
        { (Type.Float, Type.Int), Type.Float },
      };
    }
    public override dynamic Evaluate()
    {
      return Token.TokenType switch
      {
        TokenType.GreaterThan => left.Evaluate() > right.Evaluate(),
        TokenType.LessThan => left.Evaluate() < right.Evaluate(),
        TokenType.Equal => left.Evaluate() == right.Evaluate(),
        TokenType.NotEqual => left.Evaluate() != right.Evaluate(),
        _ => throw new NotImplementedException() // <----------- Throw not implmented
      };
    }

    public override string Generate()
    {
      return Evaluate().ToString();
    }

    public override Type GetExpressionType()
    {
      if(_typeRules.TryGetValue((left.GetExpressionType(), right.GetExpressionType()),
            out var resultType))
          {
            return resultType;
          }
      throw new ApplicationException($"");
    }
  }
}
