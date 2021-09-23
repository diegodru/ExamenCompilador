namespace DotNetWeb.Core.Expressions
{
  public class ArgumentExpression : BinaryOperator
  {
    public ArgumentExpression(Token token,
        TypedExpression left,
        TypedExpression right,
        Type type) : base(token, left, right, type)
    {
    }

    public override string Generate()
    {
      if(right != null)
        return $"{left.Generate()} {Token.Lexeme} {right.Generate()}";
      return left.Generate();
    }
  }
}
