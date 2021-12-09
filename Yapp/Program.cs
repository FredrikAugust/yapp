﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Yapp
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("> ");

                var line = Console.ReadLine();
                if (line == null) continue;

                var parser = new Parser();
                var syntaxTree = parser.Parse(line);
                
                Console.WriteLine(syntaxTree);
            }
        }
    }

    enum TokenType
    {
        END_OF_FILE,
        NUMBER,
        PLUS,
        MINUS,
        MULTIPLY,
        DIVIDE,
        PRINT,
    }

    class Token
    {
        public TokenType Type { get; }
        public int Index { get; }
        public string Lexeme { get; }
        
        public Token(string lexeme, int index)
        {
            this.Type = LexemeToTokenType(lexeme);
            this.Index = index;
            this.Lexeme = lexeme;
        }

        public override string ToString()
        {
            return $"Token {this.Type.ToString()} (index { this.Index.ToString()}): {this.Lexeme}";
        }

        private TokenType LexemeToTokenType(string lexeme)
        {
            if (int.TryParse(lexeme, out _))
                return TokenType.NUMBER;
            
            switch (lexeme)
            {
                case "+":
                    return TokenType.PLUS;
                case "-":
                    return TokenType.MINUS;
                case "*":
                    return TokenType.MULTIPLY;
                case "/":
                    return TokenType.DIVIDE;
                case "print":
                    return TokenType.PRINT;
                case "EOF":
                    return TokenType.END_OF_FILE;
                default:
                    throw new Exception("Could not find TokenType " + lexeme);
            }
        }
    }

    class SyntaxTree
    {
        public TreeNode Root;
        public SyntaxTree(TreeNode root)
        {
            this.Root = root;
        }

        public override string ToString()
        {
            return $"SyntaxTree: {this.Root}";
        }
    }

    abstract class TreeNode
    {
        public Token Token { get; protected set; }

        public virtual IEnumerable<TreeNode> GetChildren() => ImmutableList<TreeNode>.Empty;
    }

    abstract class Expression : TreeNode
    {
    }

    class BinaryExpression : Expression
    {
        public BinaryExpression(Expression left, Token operatorToken, Expression right)
        {
            this.Left = left;
            this.Token = operatorToken;
            this.Right = right;
        }

        public Expression Left { get; }
        public Expression Right { get; }

        public override IEnumerable<TreeNode> GetChildren()
        {
            yield return this.Left;
            yield return this.Right;
        }

        public override string ToString()
        {
            return $"Binary expression: {this.Left} {this.Token} {this.Right}";
        }
    }

    class PrimaryExpression : Expression
    {
        public PrimaryExpression(Token token)
        {
            this.Token = token;
        }

        public override string ToString()
        {
            return $"Primary expression: {this.Token}";
        }
    }

    class Parser
    {
        private List<Token> tokens = new List<Token>();
        private int index = 0;
        private Token CurrentToken
        {
            get
            {
                if (index >= tokens.Count) return new Token("EOF", index);
                return tokens[index];
            }
        }

        public SyntaxTree Parse(string line)
        {
            this.tokens = Tokenize(line);

            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }
            
            var expression = ParseTerm();

            return new SyntaxTree(expression);
        }

        private List<Token> Tokenize(string line)
        {
            string[] lexemes = line.Split(' ');
            return lexemes.Select((lexeme, i) => new Token(lexeme, i)).ToList();
        }

        private Token NextToken()
        {
            var current = this.CurrentToken;
            this.index++;
            return current;
        }

        private Expression ParseTerm()
        {
            var left = ParseFactor();

            while (this.CurrentToken.Type == TokenType.PLUS || this.CurrentToken.Type == TokenType.MINUS)
            {
                var operatorToken = NextToken();
                var right = ParseFactor();
                left = new BinaryExpression(left, operatorToken, right);
            }

            return left;
        }

        private Expression ParseFactor()
        {
            var left = ParsePrimaryExpression();

            while (this.CurrentToken.Type == TokenType.MULTIPLY ||
                   this.CurrentToken.Type == TokenType.DIVIDE)
            {
                var operatorToken = NextToken();
                var right = ParsePrimaryExpression();
                left = new BinaryExpression(left, operatorToken, right);
            }

            return left;
        }

        private Expression ParsePrimaryExpression()
        {
            // Is next token guaranteed to be a primary expression? 
            return new PrimaryExpression(NextToken());
        }

    }
}