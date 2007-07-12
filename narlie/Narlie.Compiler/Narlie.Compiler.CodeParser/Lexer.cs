using System;
using System.IO;
using System.Text;

namespace Narlie.Compiler.CodeParser
{
    public delegate void HaveTokenHandler(Token token);

    public class Lexer
    {
        private StreamReader reader;

        private char peek = ' ';
        private int current_line = 1;
        private int current_column = 1;
        private int token_start_line;
        private int token_start_column;
        
        private static string [] reserved_words = new string [] { 
            "define", "nil", "true", "false", "if", "for", "while"
        };
        
        private static string [] virtual_functions = new string [] {
            "using", "let", "set", "setf"
        };
        
        public static string [] VirtualFunctions {
            get { return virtual_functions; }
        }
        
        public Lexer(string input) : this()
        {
            SetInput(input);
        }

        public Lexer(Stream stream) : this()
        {
            SetInput(stream);
        }

        public Lexer(StreamReader reader) : this()
        {
            SetInput(reader);
        }
        
        public Lexer()
        {
            Reset();
        }
        
        public void Reset()
        {
            peek = ' ';
            current_line = 1;
            current_column = 1;
            token_start_line = 0;
            token_start_column = 0;
        }
        
        public void SetInput(StreamReader reader)
        {
            this.reader = reader;
        }
        
        public void SetInput(Stream stream)
        {
            SetInput(new StreamReader(stream));
        }
        
        public void SetInput(string input)
        {
            SetInput(new MemoryStream(Encoding.UTF8.GetBytes(input)));
        }
        
        private void ReadChar()
        {
            peek = (char)reader.Read();
            current_column++;
        }
        
        private void UnexpectedCharacter(char ch)
        {
            throw new LexerException(String.Format("Unexpected character '{0}' at [{1}:{2}]", 
                ch, current_line, current_column - 1));
        }
        
        private void EnsureTerminationChar(char ch)
        {
            if(Char.IsWhiteSpace(ch) || ch == Char.MinValue 
                || ch == Char.MaxValue || ch == '(' || ch == ')') {
                return;
            }
            
            UnexpectedCharacter(ch);
        }

        private int LexInt()
        {
            int value = 0;
            
            do {
                value = 10 * value + (peek - '0');
                ReadChar();
            } while(Char.IsDigit(peek));
        
            return value;
        }

        private double LexFraction()
        {
            double fraction = 0;
            double d = 10;
            
            while(true) {
                ReadChar();

                if(!Char.IsDigit(peek)) {
                    break;
                }

                fraction += (peek - '0') / d;
                d *= 10;
            }
            
            return fraction;
        }

        private string LexString()
        {
            StringBuilder buffer = new StringBuilder();
            
            while(true) {
                ReadChar();

                if(peek == '\\') {
                    ReadChar();
                    if(peek == '"' || peek == '\\') {
                        buffer.Append(peek);
                    } else {
                        UnexpectedCharacter(peek);
                    }
                } else if(peek == '"') {
                    break;
                } else {
                    buffer.Append(peek);
                }
            }

            return buffer.ToString();
        }

        private bool ValidIdCharacter(char ch)
        {
            if(Char.IsLetterOrDigit(ch)) {
                return true;
            }

            switch(ch) {
                case '&': case '|': case '>': case '<': case '=': case '!':
                case '*': case '+': case '-': case '/': case '%': case '^':
                case '_': case '.':
                    return true;
                default:
                    return false;
            }
        }

        private string LexId()
        {
            StringBuilder builder = new StringBuilder();

            do {
                builder.Append(peek);
                ReadChar();
            } while(ValidIdCharacter(peek));

            return builder.ToString();
        }
        
        private bool IsReservedWord(string word)
        {
            foreach(string rword in reserved_words) {
                if(rword == word.ToLower()) {
                    return true;
                }
            }
            
            foreach(string rword in virtual_functions) {
                if(rword == word.ToLower()) {
                    return true;
                }
            }
            
            return false;
        }
        
        private Token ReadReservedWord(string token)
        {
            switch(token) {
                case "define": return new DefineToken();
                case "nil": return new NilToken(); 
                case "true": return new BooleanToken(true); 
                case "false": return new BooleanToken(false);
                case "if": return new KeywordToken(Tag.If);
                case "for": return new KeywordToken(Tag.For);
                case "while": return new KeywordToken(Tag.While);
            }
            
            foreach(string vf in virtual_functions) {
                if(vf == token) {
                    return new IdToken(token);
                }
            }
            
            return null;
        }

        public Token Scan()
        {
            Token token = InnerScan();
            token.SourceLine = token_start_line;
            token.SourceColumn = token_start_column;
            return token;
        }

        private Token InnerScan()
        {
            for(; ; ReadChar()) {
                if(Char.IsWhiteSpace(peek) && peek != '\n') {
                    continue;
                } else if(peek == '\n') {
                    current_line++;
                    current_column = 0;
                } else if(peek == ';') {
                    do {
                        ReadChar();
                    } while(peek != '\n' && peek != Char.MinValue 
                        && peek != Char.MaxValue);

                    if(peek == '\n') {
                        current_line++;
                        current_column = 0;
                    }
                } else {
                    break;
                }
            }

            token_start_column = current_column;
            token_start_line = current_line;
        
            if(Char.IsDigit(peek) || peek == '.') {
                bool have_float = false;
                int intval = peek != '.' ? LexInt() : 0;
                double floatval = 0;
            
                if(peek == '.') {
                    floatval = intval + LexFraction();
                    have_float = true;
                } else if(Char.ToLower(peek) != 'e') {
                    EnsureTerminationChar(peek);
                    return new IntegerToken(intval);
                }
                                    
                if(Char.ToLower(peek) == 'e') {
                    ReadChar();
                    int exp = (int)Math.Pow(10, LexInt());
                    if(have_float) {
                        floatval *= exp;
                    } else {
                        intval *= exp;
                    }
                }

                EnsureTerminationChar(peek);

                return have_float ? 
                    (Token)new RealToken(floatval) : 
                    (Token)new IntegerToken(intval);
            } else if(peek == '#') {
                ReadChar();
                Token bool_token = null;
                if(peek == 't') {
                    bool_token = new BooleanToken(true);
                } else if(peek == 'f') {
                    bool_token = new BooleanToken(false);
                } else {
                    UnexpectedCharacter(peek);
                }
                ReadChar();
                EnsureTerminationChar(peek);
                return bool_token;
            } else if(peek == '"') {
                string str = LexString();
                ReadChar();
                EnsureTerminationChar(peek);
                return new StringToken(str);
            } else if(ValidIdCharacter(peek)) {
                string str = LexId();
                Token token = null;
                if(IsReservedWord(str)) {
                    token = ReadReservedWord(str.ToLower());
                } else {
                    token = new IdToken(str);
                }
                EnsureTerminationChar(peek);
                return token;
            } else if(peek == '(') {
                ReadChar();
                return new ScopeToken(ScopeAction.Push);
            } else if(peek == ')') {
                ReadChar();
                return new ScopeToken(ScopeAction.Pop);
            } else if(peek == Char.MinValue || peek == Char.MaxValue) {
                return new Token(peek);
            }
            
            UnexpectedCharacter(peek);
            return null;
        }
    }
}
