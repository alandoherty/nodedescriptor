using NodeDescriptor.Nodes;
using System;
using System.Collections.Generic;
using System.IO;

namespace NodeDescriptor.Serializers
{
    /// <summary>
    /// The ASCII serializer for the NDF file format.
    /// </summary>
    public class NDFASCIISerializer : NDFSerializer
    {
        #region Methods        
        /// <summary>
        /// Serializes a Node Descriptor File to the specified stream.
        /// </summary>
        /// <param name="ndf">The ndf.</param>
        /// <param name="stream">The stream.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Serialize(NDF ndf, Stream stream)
        {
            // writer
            StreamWriter writer = new StreamWriter(stream);

            // version header
            writer.WriteLine("#version " + NDF.VERSION);

            // serialize nodes
            foreach (KeyValuePair<string, NDFNode> kv in ndf.Nodes)
            {
                if (kv.Value is NDFKeyValueNode)
                    writer.Write("val ");

                kv.Value.Serialize(writer);
                writer.Write(';');
            }

            // flush
            writer.Flush();
        }

        /// <summary>
        /// Deserializes a Node Descriptor File from the specified stream.
        /// </summary>
        /// <param name="ndf">The NDF.</param>
        /// <param name="stream">The stream.</param>
        public void Deserialize(NDF ndf, Stream stream)
        {
            // reader
            StreamReader reader = new StreamReader(stream);

            // get importer
            NDFImporter ppImporter = (ndf.Importer == null) ? new NDFEmptyImporter() : null;

            if (ndf.Importer == null && ndf.Path != null)
                ppImporter = new NDFFileImporter(Path.GetDirectoryName(Path.GetFullPath(ndf.Path)));

            // source
            string source = (ndf.Path == null) ? "[Stream]" : Path.GetFileName(ndf.Path);

            // preprocess
            NDFPreprocessor preprocessor = new NDFPreprocessor(reader.ReadToEnd(), ppImporter, source);
            preprocessor.Preprocess();

            // tokenize
            NDFTokenizer tokenizer = new NDFTokenizer(preprocessor.Output, source);
            tokenizer.Tokenize();

            // parse
            NDFParser parser = new NDFParser(tokenizer.Output, source);
            parser.Parse();

            // nodes
            ndf.Nodes = new Dictionary<string, NDFNode>();

            for (int i = 0; i < parser.Output.Length; i++)
                ndf.Nodes.Add(parser.Output[i].Key, parser.Output[i]);
        }
        #endregion
    }
}
