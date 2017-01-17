using NodeDescriptor.Nodes;
using NodeDescriptor.Serializers;
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
        private static Dictionary<Type, NDFSerializer> Serializers = null;

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
            internal set {
                this.nodes = value;
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

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <value>The path.</value>
        public string Path {
            get {
                return path;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Deserializes the NDF data from binary form.
        /// </summary>
        /// <param name="stream">The stream.</param>
        private void DeserializeBinary(Stream stream) {
            Serializers[typeof(NDFBinarySerializer)].Deserialize(this, stream);
        }

        /// <summary>
        /// Deserializes the NDF data from ASCII form.
        /// </summary>
        /// <param name="stream">The stream.</param>
        private void DeserializeASCII(Stream stream) {
            Serializers[typeof(NDFASCIISerializer)].Deserialize(this, stream);
        }

        /// <summary>
        /// Serializes the NDF data into binary form.
        /// </summary>
        /// <param name="stream">The stream.</param>
        private void SerializeBinary(Stream stream) {
            Serializers[typeof(NDFBinarySerializer)].Serialize(this, stream);
        }

        /// <summary>
        /// Serializes the NDF data into ASCII form.
        /// </summary>
        /// <param name="stream">The stream.</param>
        private void SerializeASCII(Stream stream) {
            Serializers[typeof(NDFASCIISerializer)].Serialize(this, stream);
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

        /// <summary>
        /// Initializes the <see cref="NDF"/> class.
        /// </summary>
        static NDF() {
            Serializers = new Dictionary<Type, NDFSerializer>() {
                {typeof(NDFASCIISerializer), new NDFASCIISerializer()},
                {typeof(NDFBinarySerializer), new NDFBinarySerializer()}
            };
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
