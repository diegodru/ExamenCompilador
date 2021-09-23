using DotNetWeb.Core.Interfaces;

namespace DotNetWeb.Core.Statements
{
  public abstract class Statement : ISemanticValidation, IStatementEvaluate
  {
    public abstract void Interpret();

    public abstract void ValidateSemantic();

    public abstract string Generate();

    public virtual string GetCodeInit()
    {
      var code = string.Empty;
      return code;
    }
  }
}
