using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NodeDescriptor.Nodes
{
    /// <summary>
    /// The object node.
    /// </summary>
    public class NDFObjectNode : NDFNode
    {
        #region Fields
        private string type;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        public string Type
        {
            get
            {
                return type;
            }
            set
            {
                this.type = value;
            }
        }

        /// <summary>
        /// Gets or sets the table.
        /// </summary>
        /// <value>The table.</value>
        public Dictionary<string, NDFKeyValueNode> Table
        {
            get
            {
                return (Dictionary<string, NDFKeyValueNode>)val;
            }
            set
            {
                this.val = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine("@" + key + " of type " + type.ToString());
            output.AppendLine("[");

            foreach (KeyValuePair<string, NDFKeyValueNode> kv in (Dictionary<string, NDFKeyValueNode>)val)
            {
                output.AppendLine("\t" + kv.Value.ToString());
            }

            output.AppendLine("]");
            return output.ToString();
        }

        /// <summary>
        /// Serializes the node in binary form.
        /// </summary>
        /// <param name="writer">The writer.</param>
        internal override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
        }

        /// <summary>
        /// Serializes the node in ASCII form.
        /// </summary>
        /// <param name="writer">The writer.</param>
        internal override void Serialize(StreamWriter writer)
        {
            // header
            writer.Write("object " + key + " : " + type + " {");

            // kv pairs
            foreach (KeyValuePair<string, NDFKeyValueNode> kv in (Dictionary<string, NDFKeyValueNode>)val)
            {
                writer.Write("val ");
                kv.Value.Serialize(writer);
                writer.Write(";");
            }

            // footer
            writer.Write("}");
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="NDFObjectNode"/> class.
        /// </summary>
        public NDFObjectNode()
        {
            this.val = new Dictionary<string, object>();
        }
        #endregion
    }
}
