using DotNetWeb.Core.Expressions;
using System;

namespace DotNetWeb.Core.Statements
{
  public class AssignationStatement : Statement
  {
    public AssignationStatement(Id id, TypedExpression expression)
    {
      Id = id;
      Expression = expression;
    }

    public Id Id { get; }
    public TypedExpression Expression { get; }

    public override string Generate()
    {
      return string.Empty;
    }

    public override Expression Interpret()
    {
      EnvironmentManager.UpdateVariable(Id.Token.Lexeme, Expression.Evaluate());
      return null;
    }

    public override void ValidateSemantic()
    {
      if(Id.GetExpressionType() != Expression.GetExpressionType())
      {
        throw new ApplicationException($"El tipo {Id.GetExpressionType()} no es asignable a un tipo {Expression.GetExpressionType()}");
      }
    }
  }
}
