/*
 * QR code generator library (.NET)
 *
 * Copyright (c) Manuel Bleichenbacher (MIT License)
 * https://github.com/manuelbl/QrCodeGenerator
 */

using System;
using Xunit;

namespace Net.Codecrete.QrCodeGenerator.Test;

public class ObjectsTest
{
    [Fact]
    public void RequireNonNull_Succeeds()
    {
        Objects.RequireNonNull("test", "param");
        Assert.True(true);
    }

    [Fact]
    public void RequireNonNull_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => Objects.RequireNonNull<string>(null, "param"));
    }
}