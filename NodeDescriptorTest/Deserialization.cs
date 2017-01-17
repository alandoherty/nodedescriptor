using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NodeDescriptor;
using System.Text;
using System.IO;
using System.Collections.Generic;
using NodeDescriptor.Nodes;

namespace NodeDescriptorTest
{
    [TestClass]
    public class Deserialization
    {
        public string GenerateExample()
        {
            // builder
            StringBuilder builder = new StringBuilder();

            // build
            builder.AppendLine("#vesion 1");
            builder.AppendLine("val Wow1 = 5;");
            builder.AppendLine("val Wow2 = \"Test\";");
            builder.AppendLine("object Test : TestType {");
            builder.AppendLine("    val Test1 = 2.22;");
            builder.AppendLine("};");

            return builder.ToString();
        }

        [TestMethod]
        public void Test1()
        {
            // deserializea
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(GenerateExample())))
            {
                // import
                NDF ndf = new NDF();
                ndf.Import(ms, NDFMode.ASCII);

                // val checks
                Assert.IsInstanceOfType(ndf.Nodes["Wow1"], typeof(NDFKeyValueNode));
                Assert.AreEqual(((NDFKeyValueNode)ndf.Nodes["Wow1"]).Value, (double)5);
                Assert.IsInstanceOfType(ndf.Nodes["Wow2"], typeof(NDFKeyValueNode));
                Assert.AreEqual(((NDFKeyValueNode)ndf.Nodes["Wow2"]).Value, "Test");
                
                // object checks
                Assert.IsInstanceOfType(ndf.Nodes["Test"], typeof(NDFObjectNode));
                NDFObjectNode obj = (NDFObjectNode)ndf.Nodes["Test"];
                Assert.AreEqual(obj.Type, "TestType");
                Assert.IsInstanceOfType(obj.Table["Test1"], typeof(NDFKeyValueNode));
                Assert.AreEqual(obj.Table["Test1"].Value, (double)2.22);
            }
        }
    }
}
