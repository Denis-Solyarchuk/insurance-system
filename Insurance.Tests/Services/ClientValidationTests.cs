using Insurance.Domain;
using Xunit;

namespace Insurance.Tests;

public class ClientValidationTests
{
    private static Client ValidClient() => new()
    {
        FullName = "Іваненко Іван",
        Phone = "+380501234567",
        Email = "ivan@mail.com",
        Type = ClientType.Individual
    };

    [Fact]
    public void Validate_ValidClient_ReturnsNoErrors()
    {
        Assert.Empty(ValidClient().Validate());
    }

    [Theory]
    [InlineData("")]
    [InlineData("Ів")]
    [InlineData("   ")]
    public void Validate_InvalidName_ReturnsError(string name)
    {
        var client = ValidClient();
        client.FullName = name;

        Assert.NotEmpty(client.Validate());
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("abcdefghijk")]
    [InlineData("")]
    public void Validate_InvalidPhone_ReturnsError(string phone)
    {
        var client = ValidClient();
        client.Phone = phone;

        Assert.NotEmpty(client.Validate());
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("a@b")]
    [InlineData("")]
    public void Validate_InvalidEmail_ReturnsError(string email)
    {
        var client = ValidClient();
        client.Email = email;

        Assert.NotEmpty(client.Validate());
    }
}