using FluentAssertions;
using Xunit;

namespace EventSauceApi.Tests
{
    public class ValuesControllerTests
    {
        [Fact]
        public void TrueIsTrue()
        {
            true.Should().BeTrue();
        }
    }
}
