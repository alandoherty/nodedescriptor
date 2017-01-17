using NodeDescriptor.Nodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NodeDescriptor.Serializers
{
    /// <summary>
    /// The Binary serializer for the NDF file format.
    /// </summary>
    public class NDFBinarySerializer : NDFSerializer
    {
        #region Methods        
        /// <summary>
        /// Serializes a Node Descriptor File to the specified stream.
        /// </summary>
        /// <param name="ndf">The ndf.</param>
        /// <param name="stream">The stream.</param>
        /// <exception cref="System.NotImplementedException">Node not implemented for binary serialization</exception>
        public void Serialize(NDF ndf, System.IO.Stream stream)
        {
            // writer
            BinaryWriter writer = new BinaryWriter(stream);

            // header
            writer.Write(Encoding.ASCII.GetBytes(NDF.MAGIC));
            writer.Write(NDF.VERSION);

            // commands
            writer.Write((short)0);

            // nodes
            writer.Write(ndf.Nodes.Count);

            // flush
            writer.Flush();

            // serialize nodes
            foreach (KeyValuePair<string, NDFNode> kv in ndf.Nodes)
            {
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
        /// Deserializes a Node Descriptor File from the specified stream.
        /// </summary>
        /// <param name="ndf">The NDF.</param>
        /// <param name="stream">The stream.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Deserialize(NDF ndf, Stream stream)
        {
            // reader
            BinaryReader reader = new BinaryReader(stream);

            // header
            if (Encoding.ASCII.GetString(reader.ReadBytes(3)) != NDF.MAGIC)
                throw new InvalidDataException("The NDF magic is invalid");

            if (reader.ReadInt32() != NDF.VERSION)
                throw new InvalidDataException("The NDF version is invalid");
        }
        #endregion
    }
}
