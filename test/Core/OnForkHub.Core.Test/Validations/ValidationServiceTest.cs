namespace OnForkHub.Core.Test.Validations;

public class ValidationServiceTest
{
    public class TestEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    public class TestValidationService : ValidationService<TestEntity>
    {
        public TestValidationService(IValidationBuilder<TestEntity> builder, IEntityValidator<TestEntity> validator)
            : base(builder, validator) { }

        public ValidationResult ValidatePropertyPublic<TProperty>(
            TestEntity entity,
            Expression<Func<TestEntity, TProperty>> propertyExpression,
            Action<IValidationBuilder<TestEntity>> validationAction
        )
        {
            return ValidateProperty(entity, propertyExpression, validationAction);
        }
    }

    private readonly IValidationBuilder<TestEntity> _builder;
    private readonly IEntityValidator<TestEntity> _validator;
    private readonly TestValidationService _service;

    public ValidationServiceTest()
    {
        _builder = Substitute.For<IValidationBuilder<TestEntity>>();
        _validator = Substitute.For<IEntityValidator<TestEntity>>();
        _builder.Validate().Returns(ValidationResult.Success());
        _validator.Validate(Arg.Any<TestEntity>()).Returns(ValidationResult.Success());
        _service = new TestValidationService(_builder, _validator);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should validate entity successfully")]
    public void ShouldValidateEntitySuccessfully()
    {
        var entity = new TestEntity();

        var result = _service.Validate(entity);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return failure when entity is null")]
    public void ShouldReturnFailureWhenEntityIsNull()
    {
        var result = _service.Validate(null);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("cannot be null");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should validate update with null entity")]
    public void ShouldValidateUpdateWithNullEntity()
    {
        TestEntity? entity = null;

        var result = _service.ValidateUpdate(entity!);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("TestEntity cannot be null").And.Contain("TestEntity ID is required for updates");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should execute custom validation rules")]
    public void ShouldExecuteCustomValidationRules()
    {
        var entity = new TestEntity();
        var rule = Substitute.For<IValidationRule<TestEntity>>();
        rule.Validate(entity).Returns(ValidationResult.Failure("Custom rule error"));
        _service.AddRule(rule);

        var result = _service.Validate(entity);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Custom rule error");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should execute added validations")]
    public void ShouldExecuteAddedValidations()
    {
        var entity = new TestEntity();
        _service.AddValidation(e => ValidationResult.Failure("Validation error"));

        var result = _service.Validate(entity);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Validation error");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should execute error handlers when validation fails")]
    public void ShouldExecuteErrorHandlersWhenValidationFails()
    {
        var entity = new TestEntity();
        var handlerCalled = false;

        _service.AddValidation(e => ValidationResult.Failure("Error")).WithErrorHandler(_ => handlerCalled = true);

        _service.Validate(entity);

        handlerCalled.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should validate property using expression")]
    public void ShouldValidatePropertyUsingExpression()
    {
        var entity = new TestEntity { Name = "Test" };

        var result = _service.ValidatePropertyPublic(entity, e => e.Name, b => b.NotNull());

        result.IsValid.Should().BeTrue();
        _builder.Received().WithField(nameof(TestEntity.Name), entity.Name);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should merge multiple validation results")]
    public void ShouldMergeMultipleValidationResults()
    {
        var entity = new TestEntity();
        _validator.Validate(entity).Returns(ValidationResult.Failure("Validator error"));
        _service.AddValidation(e => ValidationResult.Failure("Custom error"));

        var result = _service.Validate(entity);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.ErrorMessage.Should().Contain("Validator error");
        result.ErrorMessage.Should().Contain("Custom error");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should execute all error handlers for each error")]
    public void ShouldExecuteAllErrorHandlersForEachError()
    {
        var entity = new TestEntity();
        var handlerCount = 0;

        _service.AddValidation(e => ValidationResult.Failure("Error")).WithErrorHandler(_ => handlerCount++).WithErrorHandler(_ => handlerCount++);

        _service.Validate(entity);

        handlerCount.Should().Be(2);
    }
}
