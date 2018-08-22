// Do learn to insert your names and a brief description of
// what the program is supposed to do!

// This is a skeleton program for developing a parser for C declarations
// P.D. Terry, Rhodes University, 2015

using Library;
using System;
using System.Text;

class Token
{
    public int kind;
    public string val;

    public Token(int kind, string val)
    {
        this.kind = kind;
        this.val = val;
    } // constructor
} // Token

class Declarations
{
    // +++++++++++++++++++++++++ File Handling and Error handlers ++++++++++++++++++++

    static InFile input;
    static OutFile output;

    static string NewFileName(string oldFileName, string ext)
    {
        // Creates new file name by changing extension of oldFileName to ext
        int i = oldFileName.LastIndexOf('.');
        if (i < 0) return oldFileName + ext; else return oldFileName.Substring(0, i) + ext;
    } // NewFileName

    static void ReportError(string errorMessage)
    {
        // Displays errorMessage on standard output and on reflected output
        Console.WriteLine(errorMessage);
        output.WriteLine(errorMessage);
    } // ReportError

    static void Abort(string errorMessage)
    {
        // Abandons parsing after issuing error message
        ReportError(errorMessage);
        output.Close();
        System.Environment.Exit(1);
    } // Abort

    // +++++++++++++++++++++++  token kinds enumeration +++++++++++++++++++++++++

    const int
      noSym = 0,
      EOFSym = 1,
      intSym = 2,
      charSym = 3,
      boolSym = 4,
      voidSym = 5,
      numSym = 6,
      identSym = 7,
      lparenSym = 8,
      rparenSym = 9,
      lbrackSym = 10,
      rbrackSym = 11,
      pointerSym = 12,
      commaSym = 13,
      semicolonSym = 14;
    // and others like this

    // +++++++++++++++++++++++++++++ Character Handler ++++++++++++++++++++++++++

    const char EOF = '\0';
    static bool atEndOfFile = false;

    // Declaring ch as a global variable is done for expediency - global variables
    // are not always a good thing

    static char ch;    // look ahead character for scanner

    static void GetChar()
    {
        // Obtains next character ch from input, or CHR(0) if EOF reached
        // Reflect ch to output
        if (atEndOfFile) ch = EOF;
        else
        {
            ch = input.ReadChar();
            atEndOfFile = ch == EOF;
            if (!atEndOfFile) output.Write(ch);
        }
    } // GetChar

    // +++++++++++++++++++++++++++++++ Scanner ++++++++++++++++++++++++++++++++++

    // Declaring sym as a global variable is done for expediency - global variables
    // are not always a good thing
    static void IgnoreComments()
    {
        //if '/',check next is * or / so we know its a comment
        if (ch == '/')
        {
            GetChar();
            if (ch == '/')
            {
                while (ch != '\n')
                {
                    //keep going till line end,then comment is finished
                    GetChar();
                    if (ch == EOF)
                    {
                        break;
                    }
                }
            }
            else if (ch == '*')
            {
                while (true)
                {
                    GetChar();
                    if (ch == EOF)
                    {
                        break;
                    }
                    if (ch == '*')
                    {
                        GetChar();
                        if (ch == '/')
                        {
                            //keep going till we find */,then comment ends
                            break;
                        }
                    }
                }
            }
            GetChar();
        }
    }
    static Token sym;

    static void GetSym()
    {
        int symKind = noSym;
        string symLex = "";
        // Scans for next sym from input
        //get starting character

        while (ch <= ' ' && ch > EOF)
        {
            GetChar();
            IgnoreComments();
        }

        //use character and see if it is a single character token
        switch (ch)
        {
            case EOF:
                symKind = EOFSym;
                break;

            case '(':
                symKind = lparenSym;
                break;

            case ')':
                symKind = rparenSym;
                break;

            case '[':
                symKind = lbrackSym;
                break;

            case ']':
                symKind = rbrackSym;
                break;

            case '*':
                symKind = pointerSym;
                break;

            case ',':
                symKind = commaSym;
                break;

            case ';':
                symKind = semicolonSym;
                break;
        }
        string s = "()[]*,;";
        //if its not a single character token
        if (symKind == noSym)
        {
            //keep getting characters,building a symbol string
            while (!(ch <= ' ' && ch > EOF))
            {
                if (ch == EOF || s.Contains(ch.ToString()))
                {
                    //finished building symbol string
                    break;
                }
                symLex += ch.ToString();
                GetChar();
                IgnoreComments();
            }
            //check if typeSym
            switch (symLex)
            {
                case "int":
                    symKind = intSym;
                    break;

                case "char":
                    symKind = charSym;
                    break;

                case "bool":
                    symKind = boolSym;
                    break;

                case "void":
                    symKind = voidSym;
                    break;
            }
            //check if numSym
            if (symKind == noSym)
            {
                symKind = numSym;
                foreach (var item in symLex)
                {
                    if (!(Char.IsDigit(item)))
                    {
                        symKind = noSym;
                        break;
                    }
                }
            }
            char c = symLex[0];
            //check if identSym
            if (symKind == noSym && Char.IsLetter(c))
            {
                symKind = identSym;
                foreach (var item in symLex)
                {
                    if (!(Char.IsLetterOrDigit(item)) && item != '_')
                    {
                        symKind = noSym;
                        break;
                    }
                }
            }
        }
        else
        {
            //if single character symbol,store it
            symLex = ch.ToString();
            GetChar();
        }
        sym = new Token(symKind, symLex);
    } // GetSym

    //++++ Commented out for the moment

    // +++++++++++++++++++++++++++++++ Parser +++++++++++++++++++++++++++++++++++

    static void Accept(int wantedSym, string errorMessage)
    {
        //Checks that lookahead token is wantedSym
        if (sym.kind == wantedSym) GetSym(); else Abort(errorMessage);
    } // Accept

    static void Accept(IntSet allowedSet, string errorMessage)
    {
        //Checks that lookahead token is in allowedSet
        if (allowedSet.Contains(sym.kind)) GetSym(); else Abort(errorMessage);
    } // Accept

    //Parsing Starts Here
	//go through all input lines,until EOF,then accept EOF
    static void CDecls()
    {
		
        while (sym.kind != EOFSym)
        {
            DecList();
        }
        Accept(EOFSym, "Error,EOF expected");
    }
		//single line in file
    static void DecList()
    {
        Type();
        OneDecl();
        while (sym.kind == commaSym)
        {
            Accept(commaSym, "Error,comma expected");
            OneDecl();
        }
        Accept(semicolonSym, "Error,semi-colon expected");
    }
	//line expected to start with a type
    static void Type()
    {
        switch (sym.kind)
        {
            case intSym:
                Accept(intSym, "Error,int expected");
                break;

            case voidSym:
                Accept(voidSym, "Error,void expected");
                break;

            case boolSym:
                Accept(boolSym, "Error,bool expected");
                break;

            case charSym:
                Accept(charSym, "Error,char expected");
                break;

            default:
                Abort("Error,type expected");
                break;
        }
    }
    static void OneDecl()
    {
        if (sym.kind == pointerSym)
        {
            Accept(pointerSym, "Error,pointer expected");
            OneDecl();
        }
        else
        {
            Direct();
        }
    }

    static void Direct()
    {
        if (sym.kind == identSym)
        {
            Accept(identSym, "Error,ident expected");
        }
        else
        {
            Accept(lparenSym, "Error,lparen expected");
            OneDecl();
            Accept(rparenSym, "Error,rparen expected");
        }
        Suffix();
    }

    static void Suffix()
    {
        if (sym.kind == lbrackSym)
        {
            Array();
            while (sym.kind == lbrackSym)
            {
                Array();
            }
        }
        if (sym.kind == lparenSym)
        {
            Params();
        }
    }
    static void Params()
    {
        Accept(lparenSym, "Error,lparen expected");

        if (sym.kind == intSym ||
         sym.kind == voidSym ||
         sym.kind == boolSym ||
         sym.kind == charSym)
        {
            OneParam();
            while (sym.kind == commaSym)
            {
                Accept(commaSym, "Error,comma expected");
                OneParam();
            }
        }

        Accept(rparenSym, "Error,rparen expected");
    }
    static void OneParam()
    {
        Type();
        if (sym.kind == pointerSym ||
            sym.kind == identSym ||
            sym.kind == lparenSym)
        {
            OneDecl();
        }
    }
    static void Array()
    {
        Accept(lbrackSym, "Error,lbrack expected");
        if (sym.kind == numSym)
        {
            Accept(numSym, "Error,number expected");
        }
        Accept(rbrackSym, "Error,rbrack expected");
    }

    //++++++

    // +++++++++++++++++++++ Main driver function +++++++++++++++++++++++++++++++

    public static void Main(string[] args)
    {
        // Open input and output files from command line arguments
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: Declarations FileName");
            System.Environment.Exit(1);
        }
        input = new InFile(args[0]);
        output = new OutFile(NewFileName(args[0], ".out"));

        GetChar();                                  // Lookahead character

        //  To test the scanner we can use a loop like the following:

        //do
        //{
        //    GetSym();                                 // Lookahead symbol
        //    OutFile.StdOut.Write(sym.kind, 3);
        //    OutFile.StdOut.WriteLine(" " + sym.val);  // See what we got
        //} while (sym.kind != EOFSym);

        ////  After the scanner is debugged we shall substitute this code:

        GetSym();                                   // Lookahead symbol
        CDecls();                                   // Start to parse from the goal symbol
                                                    // if we get back here everything must have been satisfactory
        Console.WriteLine("Parsed correctly");

        output.Close();
    } // Main
} // Declarations