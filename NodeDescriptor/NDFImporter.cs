using System;
using System.IO;

namespace NodeDescriptor
{
    public interface NDFImporter
    {
        /// <summary>
        /// Imports the NDF file data from the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        string Import(string path);
    }

    public class NDFEmptyImporter : NDFImporter
    {
        #region Fields
        #endregion

        #region Properties
        #endregion

        #region Methods                
        /// <summary>
        /// Imports the NDF from the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public string Import(string path) {
            return "#error The host does not support importing for " + path;
        }
        #endregion
    }

    public class NDFFileImporter : NDFImporter
    {
        #region Fields
        private string rootPath = Environment.CurrentDirectory;
        private bool escapable = false;
        #endregion

        #region Properties             
        /// <summary>
        /// Gets or sets the the root path.
        /// </summary>
        /// <value>The root path.</value>
        public string Root {
            get {
                return rootPath;
            }
            set {
                // remove trailing slashes
                if (rootPath[rootPath.Length - 1] == '/' || rootPath[rootPath.Length - 1] == '\\')
                    this.rootPath = value.Substring(0, this.rootPath.Length - 1);
                else
                    this.rootPath = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether an import path can be relative and reach directories underneath the root.
        /// </summary>
        /// <value>
        ///   <c>true</c> if escapable; otherwise, <c>false</c>.
        /// </value>
        public bool Escapable {
            get {
                return escapable;
            }
            set {
                this.escapable = value;
            }
        }
        #endregion

        #region Methods        
        /// <summary>
        /// Imports the NDF file data from the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// Import path  + path +  violates escape rule
        /// or
        /// Import path  + path +  does not exist
        /// </exception>
        public string Import(string path) {
            // build path
            string finalPath = Path.GetFullPath(Path.Combine(rootPath, path));

            // check for escapaing
            if (!escapable) {
                if (!finalPath.StartsWith(rootPath, StringComparison.InvariantCultureIgnoreCase))
                    throw new Exception("Import path " + path + " violates escape rule");
            }

            // check exists
            if (!File.Exists(finalPath))
                throw new Exception("Import path " + path + " does not exist");

            return File.ReadAllText(finalPath);
        }
        #endregion

         #region Constructors        
        /// <summary>
        /// Initializes a new instance of the <see cref="NDFFileImporter"/> class.
        /// </summary>
        public NDFFileImporter() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NDFFileImporter"/> class.
        /// </summary>
        /// <param name="rootPath">The root path.</param>
        public NDFFileImporter(string rootPath) {
            this.rootPath = rootPath;
        }
        #endregion
    }

    public class NDFFuncImporter : NDFImporter {
        #region Fields
        private Func<string, string> func = null;
        #endregion

        #region Properties        
        /// <summary>
        /// Gets or sets the function to execute on import.
        /// </summary>
        /// <value>The function.</value>
        public Func<string, string> Function {
            get {
                return func;
            }
            set {
                this.func = value;
            }
        }
        #endregion

        #region Methods        
        /// <summary>
        /// Imports the NDF file data from the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">The func handler has not been setup for importer</exception>
        public string Import(string path) {
            if (this.func == null)
                throw new InvalidOperationException("The function handler has not been setup for importer");
            else
                return this.func(path);
        }
        #endregion

        #region Constructors        
        /// <summary>
        /// Initializes a new instance of the <see cref="NDFFuncImporter"/> class.
        /// </summary>
        /// <param name="func">The function.</param>
        public NDFFuncImporter(Func<string, string> func) {
            this.func = func;
        }
        #endregion
    }
}
