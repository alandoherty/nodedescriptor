using NodeDescriptor.Nodes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NodeDescriptor
{
    /// <summary>
    /// The base NDF node.
    /// </summary>
    public abstract class NDFNode
    {
        #region Fields        
        /// <summary>
        /// The key.
        /// </summary>
        protected string key;

        /// <summary>
        /// The value.
        /// </summary>
        protected object val;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <value>The key.</value>
        public string Key {
            get {
                return key;
            }
            set {
                this.key = value;
            }
        }
        #endregion

        #region Methods        
        /// <summary>
        /// Serializes the node in ASCII form.
        /// </summary>
        /// <param name="writer">The writer.</param>
        internal virtual void Serialize(StreamWriter writer) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Serializes the node in binary form.
        /// </summary>
        /// <param name="writer">The writer.</param>
        internal virtual void Serialize(BinaryWriter writer) {
            throw new NotImplementedException();
        }
        #endregion
    }
}
