# NodeDescriptor
An object notation utility with a preprocessor and OOP-style format, wrote for C#. I'm going to implement proper node traversal methods soon, so the example below is not here to stay.

# Example

Program.cs:
```csharp
class Program {
  static void Main(string[] args) {
    NDF ndf = new NDF();
    ndf.Import("test.ndf");
    
    foreach(NDFNode node in ndf.Nodes) {
      if (node is NDFKeyValueNode) {
        Console.WriteLine("Key: " + node.Key + " Value: " + node.Value);
      } else if (node is NDFObjectNode) {
        NDFObjectNode objNode = (NDFObjectNode)objNode;
        if (objNode.Type == "Language") {
          Console.WriteLine("Object: " + objNode.Key + " Field Count: " + objNode.Table.Count);
        }
      }
    }
  }
}
```

test.ndf:
```csharp
#version 1
val Name = "Blah";
var Languages = ["English", "German"];

object English : Language {
  val Author = "Alan";
  val Strings = {
    "hello" = "Hello",
    "howdoing" = "How are you doing?"
  }
}

object German : Language {
  val Author = "Alan";
  val Strings = {
    "hello" = "Hallo",
    "howdoing" = "Wie geht es Ihnen?"
  }
}
```