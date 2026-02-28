#:package TestableIO.System.IO.Abstractions@*

using System.IO.Abstractions;
using System.Text;

var stringType = typeof(string);

var builder = new StringBuilder();

foreach (var item in typeof(IDirectory).GetMethods())
{
	builder.Append("public static ");
	
	if (item.ReturnType == typeof(string))
	{
		builder.Append("AbsolutePath");
	}
	else if (item.ReturnType == typeof(IEnumerable<string>))
	{
		builder.Append("IEnumerable<string>");
	}
	else if (item.ReturnType == typeof(string[]))
	{
		builder.Append("AbsolutePath[]");
	}
}