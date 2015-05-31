using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NodeDescriptor
{
    class NDFPreprocessor
    {
        #region Fields
        private string input = "";
        private string output = "";
        private bool preprocessed = false;
        private int version = NDF.VERSION;
        private string source = "[Unknown]";
        private NDFImporter importer = new NDFEmptyImporter();
        #endregion

        #region Properties
        /// <summary>
        /// Gets the input.
        /// </summary>
        /// <value>The input.</value>
        public string Input {
            get {
                return input;
            }
        }

        /// <summary>
        /// Gets the output.
        /// </summary>
        /// <value>The output.</value>
        public string Output {
            get {
                // check if preprocessed
                if (!preprocessed)
                    throw new InvalidOperationException("The input has not been preprocessed");

                return output;
            }
        }

        /// <summary>
        /// Gets the version the file identifies as, if none is included then the version will be specified as default.
        /// </summary>
        /// <value>The version.</value>
        public int Version {
            get {
                return version;
            }
        }

        /// <summary>
        /// Gets or sets the importer.
        /// </summary>
        /// <value>The importer.</value>
        public NDFImporter Importer {
            get {
                return importer;
            }
            set {
                this.importer = value;
            }
        }
        #endregion

        #region Methods        
        /// <summary>
        /// Splits the string accomodating for quotes
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        private string[] SplitQuotes(string str) {
            int s = 0;
            string token = "";
            List<string> strs = new List<string>();
            
            // loop
            for (int i = 0; i < str.Length; i++) {
                char c = str[i];

                if (s == 0) {
                    if (char.IsWhiteSpace(c)) {
                        if (token != "") {
                            strs.Add(token);
                            token = "";
                        }
                    } else if (c == '"') {
                        token = "";
                        s = 1;
                    } else
                        token += c;
                } else if (s == 1) {
                    if (c == '"') {
                        strs.Add(token);
                        token = "";
                    } else if (c == '\\')
                        s = 2;
                    else
                        token += c;
                } else if (s == 2) {
                    if (c == 'n')
                        token += '\n';
                    else if (c == 't')
                        token += '\t';
                    else
                        token += c;

                    s = 1;
                }
            }

            // add last
            if (token != "")
                strs.Add(token);

            // to array
            return strs.ToArray();
        }

        /// <summary>
        /// Executes the specified preprocessor command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="line">The line.</param>
        /// <param name="builder">The builder.</param>
        /// <exception cref="Exception">
        /// Expected preprocessor command after '#' on line  + source + : + line
        /// or
        /// Invalid preprocessor import, expected path on line  + source + : + line
        /// or
        /// Error occured on line  + source + : + line
        /// or
        /// Error ' + argsStr + ' occured in  + source + : + line
        /// </exception>
        private void Execute(string command, int line, StringBuilder builder) {
            // split
            string[] cmd = SplitQuotes(command);

            if (cmd.Length == 0)
                throw new Exception("Expected preprocessor command after '#' in " + source + ":" + line);

            // get arguments
            string[] args = new string[cmd.Length - 1];
            Array.Copy(cmd, 1, args, 0, args.Length);
            string argsStr = string.Join(" ", args);

            // process command
            string cmdName = cmd[0].ToLower();

            if (cmdName == "import") {
                // check length
                if (args.Length == 0)
                    throw new Exception("Invalid preprocessor import, expected path in " + source + ":" + line);

                // import
                NDFPreprocessor preprocessor = new NDFPreprocessor(importer.Import(argsStr), importer, Path.GetFileName(argsStr));
                preprocessor.Preprocess();

                // check version
                if (preprocessor.version != version)
                    throw new Exception("Imported file '" + Path.GetFileName(argsStr) + " of invalid version in " + source + ":" + line);

                // append
                builder.AppendLine(preprocessor.Output);
            } else if (cmdName == "error") {
                // check length
                if (args.Length == 0)
                    throw new Exception("Error occured in " + source + ":" + line);

                // throw
                throw new Exception("Error '" + argsStr + "' occured in " + source + ":" + line);
            } else if (cmdName == "version") {
                // check length
                if (args.Length == 0)
                    throw new Exception("Invalid version command in " + source + ":" + line);

                // parse
                int version = -1;

                if (!int.TryParse(argsStr, out version))
                    throw new Exception("Malformed version command in " + source + ":" + line);

                // check if supported
                if (NDF.VERSION < version)
                    throw new Exception("File '" + source + "' is of a higher version than supported");

                // set
                this.version = version;
            }
        }

        /// <summary>
        /// Preprocesses the input.
        /// </summary>
        public void Preprocess() {
            // builder
            StringBuilder builder = new StringBuilder();

            // other values
            bool gotChars = false;
            int line = 1;

            // state
            NDFPreprocessorState state = NDFPreprocessorState.Start;
            string command = "";
            Dictionary<int, string> commands = new Dictionary<int, string>();

            // loop
            for (int i = 0; i < input.Length; i++) {
                char c = input[i];
                char pc = (i + 1 != input.Length) ? input[i + 1] : '\0';

                // lines
                if (c == '\n') {
                    gotChars = false;
                    line++;
                }

                if (c == '\r')
                    continue;

                // state machine
                if (state == NDFPreprocessorState.Start) {
                    if (c == '#') {
                        if (!gotChars)
                            state = NDFPreprocessorState.Command;
                        else
                            builder.Append('#');
                    } else {
                        builder.Append(c);
                    }
                } else if (state == NDFPreprocessorState.Command) {
                    if (c == '\n') {
                        Execute(command, line, builder);
                        command = "";
                        state = NDFPreprocessorState.Start;
                    } else {
                        command += c;
                    }
                }

                // set chars
                if (!Char.IsWhiteSpace(c))
                    gotChars = true;
            }

            // last command
            if (command != "")
                Execute(command, line, builder);

            // output
            output = builder.ToString();

            // preprocessed
            preprocessed = true;
        }
        #endregion

        #region Constructors                
        /// <summary>
        /// Initializes a new instance of the <see cref="NDFPreprocessor"/> class.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="importer">The importer.</param>
        /// <param name="source">The source.</param>
        public NDFPreprocessor(string input, NDFImporter importer, string source) {
            this.input = input;
            this.importer = importer;
            this.source = source;
        }
        #endregion
    }

    enum NDFPreprocessorState
    {
        Start,
        Command
    }
}
