using DotNetWeb.Core;
using DotNetWeb.Core.Expressions;
using DotNetWeb.Core.Interfaces;
using DotNetWeb.Core.Statements;
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
            Init();
            Template();
        }

        private Expression Template()
        {
            var expression = Tag();
            InnerTemplate();
            return expression;
        }
        
        private void InnerTemplate()
        {
            if (this.lookAhead.TokenType == TokenType.LessThan)
            {
                Template();
            }
        }
        private Expression Tag()
        {
            Match(TokenType.LessThan);
            var token = lookAhead;
            Match(TokenType.Identifier);
            Match(TokenType.GreaterThan);
            var nodo = new HtmlNode(token, Stmts());
            Match(TokenType.LessThan);
            Match(TokenType.Slash);
            Match(TokenType.Identifier);
            Match(TokenType.GreaterThan);
            return nodo;
        }

        private Expression Stmts()
        {
            if (this.lookAhead.TokenType == TokenType.OpenBrace)
            {
                var expression = Stmt();
                Stmts();
                return expression;
            }
            return null;
        }

        private Expression Stmt()
        {
            Match(TokenType.OpenBrace);
            switch (this.lookAhead.TokenType)
            {
                case TokenType.OpenBrace:
                    Match(TokenType.OpenBrace);
                    var expression = Eq();
                    Match(TokenType.CloseBrace);
                    Match(TokenType.CloseBrace);
                    return expression;
                case TokenType.Percentage:
                    return IfStmt();
                    break;
                case TokenType.Hyphen:
                    ForeachStatement();
                    return null;
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

        private Expression IfStmt()
        {
            Match(TokenType.Percentage);
            Match(TokenType.IfKeyword);
            var expression = Eq();
            Match(TokenType.Percentage);
            Match(TokenType.CloseBrace);
            var stmt = new IfStatement(expression as TypedExpression, Template() as TypedExpression).Interpret();
            Match(TokenType.OpenBrace);
            Match(TokenType.Percentage);
            Match(TokenType.EndIfKeyword);
            Match(TokenType.Percentage);
            Match(TokenType.CloseBrace);
            return stmt;
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
                case TokenType.IntConstant:{
                    var constant = new Constant(lookAhead, Type.Int);
                    Match(TokenType.IntConstant);
                    return constant;
                                           }
                case TokenType.FloatConstant:{
                    var constant = new Constant(lookAhead, Type.Float);
                    Match(TokenType.FloatConstant);
                    return constant;
                                             }
                case TokenType.StringConstant:{
                    var constant = new Constant(lookAhead, Type.String);
                    Match(TokenType.StringConstant);
                    return constant;
                                              }
                case TokenType.OpenBracket:{
                    Match(TokenType.OpenBracket);
                    ExprList();
                    Match(TokenType.CloseBracket);
                    return null;

                                           }
                default:
                    Match(TokenType.Identifier);
                    return null;
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
                case TokenType.FloatKeyword:{
                    Match(TokenType.FloatKeyword);
                    var token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    var id = new Id(token, Type.Float);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                                            }
                case TokenType.StringKeyword:{
                    Match(TokenType.StringKeyword);
                    var token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    var id = new Id(token, Type.String);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                                             }
                    break;
                case TokenType.IntKeyword: {
                    Match(TokenType.IntKeyword);
                    var token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    var id = new Id(token, Type.Int);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                                           }
                case TokenType.FloatListKeyword:{ 
                    Match(TokenType.FloatListKeyword);
                    var token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    var id = new Id(token, Type.FloatList);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                                                }
                case TokenType.IntListKeyword: {
                    Match(TokenType.IntListKeyword);
                    var token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    var id = new Id(token, Type.IntList);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                                               }
                case TokenType.StringListKeyword: {
                    Match(TokenType.StringListKeyword);
                    var token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    var id = new Id(token, Type.StringList);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                                                  }
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
