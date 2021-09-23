namespace DotNetWeb.Core.Expressions
{
  public abstract class BinaryOperator : Expression 
  {
    public BinaryOperator(Token token, 
        TypedExpression leftExp,
        TypedExpression rightExp,
        Type type) : base (token, type)
    {
      left = leftExp;
      right = rightExp;
    }
    public TypedExpression left { get; }
    public TypedExpression right { get; }
  }
}
