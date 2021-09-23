using System;
using System.Collections.Generic;
namespace DotNetWeb.Core.Expressions
{
  public class ArithmeticOperator : TypedBinaryOperator
  {
    private readonly Dictionary<(Type, Type), Type> _typeRules;

    public ArithmeticOperator(Token token,
       TypedExpression left,
       TypedExpression right) : base(token, left, right, null)
    {
      _typeRules = new Dictionary<(Type, Type), Type>
      {
        { (Type.Float, Type.Float), Type.Float },
        { (Type.Int, Type.Int), Type.Int },
        { (Type.String, Type.String), Type.String },
        { (Type.Int, Type.Float), Type.Float },
        { (Type.Float, Type.Int), Type.Float },
        { (Type.String, Type.Int), Type.String },
        { (Type.String, Type.Float), Type.String }
      };
    }
    public override dynamic Evaluate()
    {
      return Token.TokenType switch
      {
        TokenType.Plus => left.Evaluate() + right.Evaluate(),
        TokenType.Hyphen => left.Evaluate() - right.Evaluate(),
        TokenType.Asterisk => left.Evaluate() * right.Evaluate(),
        TokenType.Slash => left.Evaluate() / right.Evaluate(),
        _ => throw new NotImplementedException() // <----------- Throw not implmented
      };
    }
    
    public override string Generate()
    {
        return Evaluate().ToString();
    }
    

    public override Type GetExpressionType()
    {
      if(_typeRules.TryGetValue((left.GetExpressionType(), right.GetExpressionType()), out var resultType))
          {
            return resultType;
          }
      throw new ApplicationException($"");
    }
  
  }
}
