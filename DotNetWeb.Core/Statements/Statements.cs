using DotNetWeb.Core.Interfaces;
using DotNetWeb.Core.Expressions;

namespace DotNetWeb.Core.Statements
{
  public abstract class Statement : ISemanticValidation
  {
    public abstract Expression Interpret();

    public abstract void ValidateSemantic();

    public abstract string Generate();

    public virtual string GetCodeInit()
    {
      var code = string.Empty;
      return code;
    }
  }
}
