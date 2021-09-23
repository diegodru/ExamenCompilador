namespace DotNetWeb.Core.Expressions
{
  public class HtmlNode : Expression
  {

    public HtmlNode(Token token) : base (token, null) {}

    public Expression InnerNode { get; private set; }

    public override string Generate()
    {
      return $"<{Token.Lexeme}>\n\t{InnerNode.Generate()}\n</{Token.Lexeme}";
    }
  }
}
