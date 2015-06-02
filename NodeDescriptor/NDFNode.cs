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

    /// <summary>
    /// The key value node.
    /// </summary>
    public class NDFKeyValueNode : NDFNode
    {
        #region Fields
        private static Dictionary<Type, NDFValueType> valueTypes = new Dictionary<Type, NDFValueType>();
        #endregion

        #region Properties
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public object Value {
            get {
                return val;
            }
            set {
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
        public override string ToString() {
            string valStr = val.ToString();

            if (val is Array)
                valStr = "[" + string.Join(",", (object[])val) + "]";

            return "@" + key + " -> " + valStr;
        }

        /// <summary>
        /// Serializes the node in binary form.
        /// </summary>
        /// <param name="writer">The writer.</param>
        internal override void Serialize(BinaryWriter writer) {
            // key
            byte[] keyData = Encoding.UTF8.GetBytes(key);
            if (keyData.Length > short.MaxValue)
                throw new Exception("The key is too large for binary serialization");

            writer.Write((short)keyData.Length);
            writer.Write(keyData);

            // value
            SerializeValue(writer, val);
        }

        /// <summary>
        /// Serializes a value in binary format.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="val">The value.</param>
        /// <exception cref="NotImplementedException">The type  + val.GetType() +  cannot be serialized</exception>
        private void SerializeValue(BinaryWriter writer, object val) {
            TypeCode typeCode = Type.GetTypeCode(val.GetType());

            if (typeof(IList).IsAssignableFrom(val.GetType()))
                SerializeArray(writer, (object[])val);
            //else if (typeof(IDictionary).IsAssignableFrom(val.GetType()))
            //    SerializeTable(writer, (Dictionary<string, NDFKeyValueNode>)val);
            else if ((int)typeCode >= 5 && (int)typeCode <= 15) {
                writer.Write(val.ToString());
            }  else if (val is string)
                SerializeString(writer, (string)val);
            else if (val is bool)
                writer.Write(val.ToString().ToLower());
            else
                throw new NotImplementedException("The type " + val.GetType() + " cannot be serialized");
        }

        /// <summary>
        /// Serializes a string in binary format.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="str">The string.</param>
        private void SerializeString(BinaryWriter writer, string str) {
            byte[] strData = Encoding.UTF8.GetBytes(str);
            if (strData.Length > short.MaxValue)
                throw new Exception("The key is too large for binary serialization");

            writer.Write(strData.Length);
            writer.Write(strData);
        }

        /// <summary>
        /// Serializes the array.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="arr">The arr.</param>
        private void SerializeArray(BinaryWriter writer, object[] arr) {

        }

        /// <summary>
        /// Serializes the node in ASCII form.
        /// </summary>
        /// <param name="writer">The writer.</param>
        internal override void Serialize(StreamWriter writer) {
            writer.Write(key + " = ");
            SerializeValue(writer, val);
        }

        /// <summary>
        /// Serializes a value in ASCII format.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="val">The value.</param>
        /// <exception cref="NotImplementedException">The type  + val.GetType() +  cannot be serialized</exception>
        private void SerializeValue(StreamWriter writer, object val) {
            TypeCode typeCode = Type.GetTypeCode(val.GetType());

            if (typeof(IList).IsAssignableFrom(val.GetType()))
                SerializeArray(writer, (object[])val);
            else if (typeof(IDictionary).IsAssignableFrom(val.GetType()))
                SerializeTable(writer, (Dictionary<string, NDFKeyValueNode>)val);
            else if ((int)typeCode >= 5 && (int)typeCode <= 15)
                writer.Write(val.ToString());
            else if (val is string)
                SerializeString(writer, (string)val);
            else if (val is bool)
                writer.Write(val.ToString().ToLower());
            else
                throw new NotImplementedException("The type " + val.GetType() + " cannot be serialized");
        }

        /// <summary>
        /// Serializes an array in ASCII format.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="arr">The arr.</param>
        private void SerializeArray(StreamWriter writer, object[] arr) {
            writer.Write('[');

            bool first = true;
            foreach (object o in arr) {
                if (first)
                    first = false;
                else
                    writer.Write(',');

                SerializeValue(writer, o);
            }
            
            writer.Write(']');
        }

        /// <summary>
        /// Serializes a table in ASCII format.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="table">The table.</param>
        private void SerializeTable(StreamWriter writer, Dictionary<string, NDFKeyValueNode> table) {
            writer.Write('{');

            bool first = true;
            foreach (KeyValuePair<string, NDFKeyValueNode> kv in table) {
                if (first)
                    first = false;
                else
                    writer.Write(',');

                SerializeString(writer, kv.Key);
                writer.Write(" = ");
                SerializeValue(writer, kv.Value.val);
            }

            writer.Write('}');
        }

        /// <summary>
        /// Serializes a string in ASCII format.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="str">The string.</param>
        private void SerializeString(StreamWriter writer, string str) {
            writer.Write('"');

            // builder
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < str.Length; i++) {
                if (str[i] == '"')
                    builder.Append("\\\"");
                else if (str[i] == '\n')
                    builder.Append("\\n");
                else if (str[i] == '\r')
                    builder.Append("\\r");
                else if (str[i] == '\t')
                    builder.Append("\\t");
                else
                    builder.Append(str[i]);
            }

            // write builder
            writer.Write(builder.ToString());

            writer.Write('"');
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="NDFKeyValueNode"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="val">The value.</param>
        public NDFKeyValueNode(string key, object val) {
            this.key = key;
            this.val = val;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NDFKeyValueNode"/> class.
        /// </summary>
        public NDFKeyValueNode()
        { }

        static NDFKeyValueNode() {
            valueTypes = new Dictionary<Type, NDFValueType>() {
                {typeof(decimal), NDFValueType.Decimal},
                {typeof(bool), NDFValueType.Bool},
                {typeof(byte), NDFValueType.Byte},
                {typeof(double), NDFValueType.Double},
                {typeof(float), NDFValueType.Float},
                {typeof(int), NDFValueType.Int},
                {typeof(long), NDFValueType.Long},
                {typeof(short), NDFValueType.Short},
                {typeof(string), NDFValueType.String},
                {typeof(sbyte), NDFValueType.SByte},
                {typeof(uint), NDFValueType.UInt},
                {typeof(ulong), NDFValueType.ULong},
                {typeof(ushort), NDFValueType.UShort}
            };
        }
        #endregion
    }

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
        public string Type {
            get {
                return type;
            }
            set {
                this.type = value;
            }
        }

        /// <summary>
        /// Gets or sets the table.
        /// </summary>
        /// <value>The table.</value>
        public Dictionary<string, NDFKeyValueNode> Table {
            get {
                return (Dictionary<string, NDFKeyValueNode>)val;
            }
            set {
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
        public override string ToString() {
            StringBuilder output = new StringBuilder();
            output.AppendLine( "@" + key + " of type " + type.ToString());
            output.AppendLine("[");
            
            foreach(KeyValuePair<string, NDFKeyValueNode> kv in (Dictionary<string, NDFKeyValueNode>)val) {
                output.AppendLine("\t" + kv.Value.ToString());
            }

            output.AppendLine("]");
            return output.ToString();
        }

        /// <summary>
        /// Serializes the node in binary form.
        /// </summary>
        /// <param name="writer">The writer.</param>
        internal override void Serialize(BinaryWriter writer) {
            base.Serialize(writer);
        }

        /// <summary>
        /// Serializes the node in ASCII form.
        /// </summary>
        /// <param name="writer">The writer.</param>
        internal override void Serialize(StreamWriter writer) {
            // header
            writer.Write("object " + key + " : " + type + " {");

            // kv pairs
            foreach (KeyValuePair<string, NDFKeyValueNode> kv in (Dictionary<string, NDFKeyValueNode>)val) {
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
