using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NodeDescriptor
{
    /// <summary>
    /// The node descriptor format class.
    /// </summary>
    public class NDF
    {
        #region Constants
        internal const int VERSION = 1;
        internal const string MAGIC = "NDF";
        #endregion

        #region Fields
        private Dictionary<string, NDFNode> nodes = null;
        private NDFImporter importer = null;
        private string path = null;
        #endregion

        #region Properties        
        /// <summary>
        /// Gets the nodes.
        /// </summary>
        /// <value>The nodes.</value>
        public Dictionary<string, NDFNode> Nodes {
            get {
                return nodes;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Deserializes the NDF data from binary form.
        /// </summary>
        /// <param name="stream">The stream.</param>
        private void DeserializeBinary(Stream stream) {
            // reader
            BinaryReader reader = new BinaryReader(stream);

            // header
            if (Encoding.ASCII.GetString(reader.ReadBytes(3)) != MAGIC)
                throw new InvalidDataException("The NDF magic is invalid");

            if (reader.ReadInt32() != VERSION)
                throw new InvalidDataException("The NDF version is invalid");
        }

        /// <summary>
        /// Deserializes the NDF data from ASCII form.
        /// </summary>
        /// <param name="stream">The stream.</param>
        private void DeserializeASCII(Stream stream) {
            StreamReader reader = new StreamReader(stream);

            // get importer
            NDFImporter ppImporter = (this.importer == null) ? new NDFEmptyImporter() : null;

            if (this.importer == null && path != null)
                ppImporter = new NDFFileImporter(Path.GetDirectoryName(Path.GetFullPath(path)));

            // source
            string source = (path == null) ? "[Stream]" : Path.GetFileName(path);

            // preprocess
            NDFPreprocessor preprocessor = new NDFPreprocessor(reader.ReadToEnd(), ppImporter, source);
            preprocessor.Preprocess();

            // tokenize
            NDFTokenizer tokenizer = new NDFTokenizer(preprocessor.Output, source);
            tokenizer.Tokenize();

            // parse
            NDFParser parser = new NDFParser(tokenizer.Output, source);
            parser.Parse();

            // output
            foreach (NDFNode node in parser.Output)
                nodes.Add(node.Key, node);
        }

        /// <summary>
        /// Serializes the NDF data into binary form.
        /// </summary>
        /// <param name="stream">The stream.</param>
        private void SerializeBinary(Stream stream) {
            // writer
            BinaryWriter writer = new BinaryWriter(stream);

            // header
            writer.Write(Encoding.ASCII.GetBytes(MAGIC));
            writer.Write(VERSION);

            // commands
            writer.Write((short)0);

            // nodes
            writer.Write(nodes.Count);

            // flush
            writer.Flush();

            // serialize nodes
            foreach (KeyValuePair<string, NDFNode> kv in nodes) {
                // type
                if (kv.Value is NDFKeyValueNode)
                    writer.Write((byte)NDFNodeType.KeyValue);
                else if (kv.Value is NDFObjectNode)
                    writer.Write((byte)NDFNodeType.Object);
                else
                    throw new NotImplementedException("Node not implemented for binary serialization");

                // length
                long lengthPos = stream.Position;
                writer.Write(0);

                // serialize
                kv.Value.Serialize(writer);

                // store positon
                long oldPos = stream.Position;

                // calculate size
                int length = (int)(oldPos - lengthPos);
                
                // seek and write length
                stream.Seek(lengthPos, SeekOrigin.Begin);
                writer.Write(length);

                // seek to old
                stream.Seek(oldPos, SeekOrigin.Begin);

                // flush
                writer.Flush();
            }
        }

        /// <summary>
        /// Serializes the NDF data into ASCII form.
        /// </summary>
        /// <param name="stream">The stream.</param>
        private void SerializeASCII(Stream stream) {
            // writer
            StreamWriter writer = new StreamWriter(stream);

            // version header
            writer.WriteLine("#version " + VERSION);

            // serialize nodes
            foreach (KeyValuePair<string, NDFNode> kv in nodes) {
                if (kv.Value is NDFKeyValueNode)
                    writer.Write("val ");

                kv.Value.Serialize(writer);
                writer.Write(';');
            }

            // flush
            writer.Flush();
        }

        /// <summary>
        /// Loads the NDF from the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="mode">The mode.</param>
        public void Import(string path, NDFMode mode) {
            this.path = path;

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                Import(fs, mode);
            }
        }

        /// <summary>
        /// Loads the NDF from the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="mode">The mode.</param>
        public void Import(Stream stream, NDFMode mode) {
            if (mode == NDFMode.ASCII)
                DeserializeASCII(stream);
            else
                //DeserializeBinary(stream);
                throw new NotImplementedException("Binary deserialization not implemented");
        }

        /// <summary>
        /// Saves the NDF to the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="mode">The mode.</param>
        public void Export(string path, NDFMode mode) {
            this.path = path;

            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write)) {
                Export(fs, mode);
            }
        }

        /// <summary>
        /// Saves the NDF to the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="mode">The mode.</param>
        public void Export(Stream stream, NDFMode mode) {
            if (mode == NDFMode.ASCII)
                SerializeASCII(stream);
            else
                //SerializeBinary(stream);
                throw new NotImplementedException("Binary deserialization not implemented");
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="NDF"/> class.
        /// </summary>
        public NDF() {
            this.nodes = new Dictionary<string, NDFNode>();
        }
        #endregion
    }

    /// <summary>
    /// The mode of serialization/deserialization.
    /// </summary>
    public enum NDFMode
    {
        /// <summary>
        /// The binary format.
        /// </summary>
        Binary,

        /// <summary>
        /// The ASCII format.
        /// </summary>
        ASCII
    }

    /// <summary>
    /// The type of node, used in the binary format.
    /// </summary>
    enum NDFNodeType : byte
    {
        KeyValue = 0,
        Object = 1
    }

    /// <summary>
    /// The type of value, used in the binary format.
    /// </summary>
    enum NDFValueType : byte
    {
        String,
        Double,
        Float,
        Byte,
        SByte,
        Short,
        UShort,
        Int,
        UInt,
        Long,
        ULong,
        Decimal,
        Bool
    }
}
