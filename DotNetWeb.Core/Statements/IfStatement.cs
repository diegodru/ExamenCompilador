using DotNetWeb.Core.Expressions;
using DotNetWeb.Core.Interfaces;
using System;

namespace DotNetWeb.Core.Statements
{
  public class IfStatement : Statement
  {
    public IfStatement(TypedExpression expression, TypedExpression result)
    {
      Expression = expression;
      Result = result;
    }

    public TypedExpression Expression { get; }
    public TypedExpression Result { get; }

    public override string Generate()
    {
      if(Expression.Evaluate())
        return Result.Generate();
      return string.Empty;
    }

    public override void Interpret()
    {
      if (Expression.Evaluate())
      {
        Generate();
      }
    }

    public override void ValidateSemantic() // <-------- implementar
    {

    }

  }
}
