using System;
using System.IO;

namespace NodeDescriptor
{
    /// <summary>
    /// Interface for serializing NDF files.
    /// </summary>
    public interface NDFSerializer
    {
        /// <summary>
        /// Serializes a Node Descriptor File to the specified stream.
        /// </summary>
        /// <param name="ndf">The ndf.</param>
        /// <param name="stream">The stream.</param>
        void Serialize(NDF ndf, Stream stream);

        /// <summary>
        /// Deserializes a Node Descriptor File from the specified stream.
        /// </summary>
        /// <param name="ndf">The NDF.</param>
        /// <param name="stream">The stream.</param>
        void Deserialize(NDF ndf, Stream stream);
    }
}
