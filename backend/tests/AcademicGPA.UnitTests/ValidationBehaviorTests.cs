using AcademicGPA.Application.Common.Behaviors;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using Xunit;

namespace AcademicGPA.UnitTests;

public class ValidationBehaviorTests
{
    public record TestRequest(string Name) : IRequest<string>;

    [Fact]
    public async Task Handle_WithNoValidators_ShouldCallNext()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var request = new TestRequest("Test");
        var nextMock = new Mock<RequestHandlerDelegate<string>>();
        nextMock.Setup(x => x()).ReturnsAsync("Success");

        // Act
        var result = await behavior.Handle(request, nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be("Success");
        nextMock.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidatorsPassing_ShouldCallNext()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<TestRequest>>();
        validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult()); // Passing result with no failures

        var validators = new[] { validatorMock.Object };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var request = new TestRequest("Test");
        var nextMock = new Mock<RequestHandlerDelegate<string>>();
        nextMock.Setup(x => x()).ReturnsAsync("Success");

        // Act
        var result = await behavior.Handle(request, nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be("Success");
        nextMock.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidatorsFailing_ShouldThrowValidationExceptionAndNotCallNext()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<TestRequest>>();
        var failures = new[] { new ValidationFailure("Name", "Name is invalid") };
        validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        var validators = new[] { validatorMock.Object };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var request = new TestRequest("Test");
        var nextMock = new Mock<RequestHandlerDelegate<string>>();

        // Act
        var act = () => behavior.Handle(request, nextMock.Object, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.And.Errors.Should().ContainSingle().Which.PropertyName.Should().Be("Name");
        nextMock.Verify(x => x(), Times.Never);
    }
}
