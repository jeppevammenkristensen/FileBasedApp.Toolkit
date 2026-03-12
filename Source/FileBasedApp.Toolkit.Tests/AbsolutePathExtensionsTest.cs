using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices; 
using FluentAssertions;
using JetBrains.Annotations;
using TruePath;
using Xunit;

namespace FileBasedApp.Toolkit.Tests;

[TestSubject(typeof(IO))]
public class AbsolutePathExtensionsTest
{
    [Fact]
    
    public void GetAncestors_ShouldGenerateExpectedResult()
    {
        // check is runtime is windows
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        if (isWindows)
        {
            var path = AbsolutePath.Create(@"C:\Users\user\Documents\SomeOther\test.txt");
            var ancestors = path.GetAncestors(true).ToList();
            ancestors.Should().HaveCount(6);
            ancestors[0].FileName.Should().Be("test.txt");
            ancestors[1].FileName.Should().Be("SomeOther");
            ancestors[2].FileName.Should().Be("Documents");
            ancestors[3].FileName.Should().Be("user");
            ancestors[4].FileName.Should().Be("Users");
            ancestors[5].Value.Should().Be("C:\\");
        }
        else
        {
            // var path = AbsolutePath.Create("/home/user/Documents/SomeOther/test.txt");
            // var ancestors = path.GetAncestors(true).ToList();
            // ancestors.Should().HaveCount(5);
            // ancestors[0].FileName.Should().Be("test.txt");
            // ancestors[1].FileName.Should().Be("SomeOther");
            // ancestors[2].FileName.Should().Be("Documents");
            // ancestors[3].FileName.Should().Be("user");
            // ancestors[4].Value.Should().Be("/");
        }
    }

    
}