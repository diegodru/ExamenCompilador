using DotNetWeb.Core;
using DotNetWeb.Core.Expressions;
using DotNetWeb.Core.Interfaces;
using System;

namespace DotNetWeb.Parser
{
    public class Parser : IParser
    {
        private readonly IScanner scanner;
        private Token lookAhead;
        public Parser(IScanner scanner)
        {
            this.scanner = scanner;
            this.Move();
        }
        public void Parse()
        {
            Program();
        }

        private void Program()
        {
            var init = Init();
            Template();
        }

        private void Template()
        {
            Tag();
            InnerTemplate();
        }
        
        private void InnerTemplate()
        {
            if (this.lookAhead.TokenType == TokenType.LessThan)
            {
                Template();
            }
        }
        private void Tag()
        {
            Match(TokenType.LessThan);
            Match(TokenType.Identifier);
            Match(TokenType.GreaterThan);
            Stmts();
            Match(TokenType.LessThan);
            Match(TokenType.Slash);
            Match(TokenType.Identifier);
            Match(TokenType.GreaterThan);
        }

        private void Stmts()
        {
            if (this.lookAhead.TokenType == TokenType.OpenBrace)
            {
                Stmt();
                Stmts();
            }
        }

        private Statement Stmt()
        {
            Match(TokenType.OpenBrace);
            switch (this.lookAhead.TokenType)
            {
                case TokenType.OpenBrace:
                    Match(TokenType.OpenBrace);
                    Eq();
                    Match(TokenType.CloseBrace);
                    Match(TokenType.CloseBrace);
                    break;
                case TokenType.Percentage:
                    IfStmt();
                    break;
                case TokenType.Hyphen:
                    ForeachStatement();
                    break;
                default:
                    throw new ApplicationException("Unrecognized statement");
            }
        }

        private void ForeachStatement()
        {
            Match(TokenType.Hyphen);
            Match(TokenType.Percentage);
            Match(TokenType.ForEeachKeyword);
            Match(TokenType.Identifier);
            Match(TokenType.InKeyword);
            Match(TokenType.Identifier);
            Match(TokenType.Percentage);
            Match(TokenType.CloseBrace);
            Template();
            Match(TokenType.OpenBrace);
            Match(TokenType.Percentage);
            Match(TokenType.EndForEachKeyword);
            Match(TokenType.Percentage);
            Match(TokenType.CloseBrace);
        }

        private void IfStmt()
        {
            Match(TokenType.Percentage);
            Match(TokenType.IfKeyword);
            Eq();
            Match(TokenType.Percentage);
            Match(TokenType.CloseBrace);
            Template();
            Match(TokenType.OpenBrace);
            Match(TokenType.Percentage);
            Match(TokenType.EndIfKeyword);
            Match(TokenType.Percentage);
            Match(TokenType.CloseBrace);
        }

        private Expression Eq()
        {
            var expression = Rel();
            while (this.lookAhead.TokenType == TokenType.Equal || this.lookAhead.TokenType == TokenType.NotEqual)
            {
              var token = lookAhead;
              Move();
              expression = new RelationalExpression(token,
                  expression as TypedExpression,
                  Rel() as TypedExpression
                  );
            }
            return expression;
        }

        private Expression Rel()
        {
            var expression = Expr();
            if (this.lookAhead.TokenType == TokenType.LessThan
                || this.lookAhead.TokenType == TokenType.GreaterThan)
            {
              var token = lookAhead;
              Move();
              expression = new RelationalExpression(token,
                  expression as TypedExpression,
                  Expr() as TypedExpression
                  );
            }
            return expression;
        }

        private Expression Expr()
        {
            var expression = Term();
            while (this.lookAhead.TokenType == TokenType.Plus || this.lookAhead.TokenType == TokenType.Hyphen)
            {
              var token = lookAhead;
              Move();
              expression = new ArithmeticOperator(token,
                  expression as TypedExpression,
                  Term() as TypedExpression
                  );
            }
            return expression;
        }

        private Expression Term()
        {
            var expression = Factor();
            while (this.lookAhead.TokenType == TokenType.Asterisk || this.lookAhead.TokenType == TokenType.Slash)
            {
              var token = lookAhead;
              Move();
              expression = new ArithmeticOperator(token,
                  expression as TypedExpression,
                  Factor() as TypedExpression
                  );
            }
            return expression;
        }

        private Expression Factor()
        {
            switch (this.lookAhead.TokenType)
            {
                case TokenType.LeftParens:
                    {
                      Match(TokenType.LeftParens);
                      var expression = Eq();
                      Match(TokenType.RightParens);
                      return expression;
                    }
                case TokenType.IntConstant:
                    var constant = new Constant(lookAhead, Type.Int);
                    Match(TokenType.IntConstant);
                    return constant;
                case TokenType.FloatConstant:
                    var constant = new Constant(lookAhead, Type.Float);
                    Match(TokenType.FloatConstant);
                    return constant;
                case TokenType.StringConstant:
                    var constant = new Constant(lookAhead, Type.String);
                    Match(TokenType.StringConstant);
                    return constant;
                case TokenType.OpenBracket:
                    Match(TokenType.OpenBracket);
                    ExprList();
                    Match(TokenType.CloseBracket);
                    break;
                default:
                    Match(TokenType.Identifier);
                    break;
            }
        }

        private void ExprList()
        {
            Eq();
            if (this.lookAhead.TokenType != TokenType.Comma)
            {
                return;
            }
            Match(TokenType.Comma);
            ExprList();
        }

        private void Init()
        {
            Match(TokenType.OpenBrace);
            Match(TokenType.Percentage);
            Match(TokenType.InitKeyword);
            EnvironmentManager.PushContext();
            Code();
            Match(TokenType.Percentage);
            Match(TokenType.CloseBrace);
        }

        private void Code()
        {
            Decls();
            Assignations();
        }

        private void Assignations()
        {
            if (this.lookAhead.TokenType == TokenType.Identifier)
            {
                Assignation();
                Assignations();
            }
        }

        private void Assignation()
        {
            var token = lookAhead;
            Match(TokenType.Identifier);
            Match(TokenType.Assignation);
            EnvironmentManager.UpdateVariable(token, Eq().Evaluate());
            Match(TokenType.SemiColon);
        }

        private void Decls()
        {
            Decl();
            InnerDecls();
        }

        private void InnerDecls()
        {
            if (this.LookAheadIsType())
            {
                Decls();
            }
        }

        private void Decl()
        {
            switch (this.lookAhead.TokenType)
            {
                case TokenType.FloatKeyword:
                    Match(TokenType.FloatKeyword);
                    var token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    var id = new Id(token, Type.Float);
                    EnvironmentManager.AddVariable(token.Lexemen, id);
                    break;
                case TokenType.StringKeyword:
                    Match(TokenType.StringKeyword);
                    var token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    var id = new Id(token, Type.String);
                    EnvironmentManager.AddVariable(token.Lexemen, id);
                    break;
                case TokenType.IntKeyword:
                    Match(TokenType.IntKeyword);
                    var token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    var id = new Id(token, Type.Int);
                    EnvironmentManager.AddVariable(token.Lexemen, id);
                    break;
                case TokenType.FloatListKeyword:
                    Match(TokenType.FloatListKeyword);
                    var token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    var id = new Id(token, Type.FloatList);
                    EnvironmentManager.AddVariable(token.Lexemen, id);
                    break;
                case TokenType.IntListKeyword:
                    Match(TokenType.IntListKeyword);
                    var token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    var id = new Id(token, Type.IntList);
                    EnvironmentManager.AddVariable(token.Lexemen, id);
                    break;
                case TokenType.StringListKeyword:
                    Match(TokenType.StringListKeyword);
                    var token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    var id = new Id(token, Type.StringList);
                    EnvironmentManager.AddVariable(token.Lexemen, id);
                    break;
                default:
                    throw new ApplicationException($"Unsupported type {this.lookAhead.Lexeme}");
            }
        }

        private void Move()
        {
            this.lookAhead = this.scanner.GetNextToken();
        }

        private void Match(TokenType tokenType)
        {
            if (this.lookAhead.TokenType != tokenType)
            {
                throw new ApplicationException($"Syntax error! expected token {tokenType} but found {this.lookAhead.TokenType}. Line: {this.lookAhead.Line}, Column: {this.lookAhead.Column}");
            }
            this.Move();
        }

        private bool LookAheadIsType()
        {
            return this.lookAhead.TokenType == TokenType.IntKeyword ||
                this.lookAhead.TokenType == TokenType.StringKeyword ||
                this.lookAhead.TokenType == TokenType.FloatKeyword ||
                this.lookAhead.TokenType == TokenType.IntListKeyword ||
                this.lookAhead.TokenType == TokenType.FloatListKeyword ||
                this.lookAhead.TokenType == TokenType.StringListKeyword;

        }
    }
}
