namespace OnForkHub.CrossCutting.Tests.Middleware;

[TestClass]
[TestCategory("Unit")]
public class SecurityHeadersMiddlewareTests
{
    [TestMethod]
    [TestCategory("Middleware")]
    public async Task InvokeAddsAllRequiredSecurityHeaders()
    {
        var httpContext = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(async (ctx) => await Task.CompletedTask);

        await middleware.InvokeAsync(httpContext);

        var headers = httpContext.Response.Headers;

        Assert.IsTrue(headers.ContainsKey("X-Content-Type-Options"));
        Assert.IsTrue(headers.ContainsKey("X-Frame-Options"));
        Assert.IsTrue(headers.ContainsKey("X-XSS-Protection"));
        Assert.IsTrue(headers.ContainsKey("Referrer-Policy"));
        Assert.IsTrue(headers.ContainsKey("Permissions-Policy"));
        Assert.IsTrue(headers.ContainsKey("Content-Security-Policy"));
        Assert.IsTrue(headers.ContainsKey("Strict-Transport-Security"));
    }

    [TestMethod]
    [TestCategory("Middleware")]
    public async Task InvokeSetsCorrectHeaderValues()
    {
        var httpContext = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(async (ctx) => await Task.CompletedTask);

        await middleware.InvokeAsync(httpContext);

        var headers = httpContext.Response.Headers;

        Assert.AreEqual("nosniff", headers.XContentTypeOptions.ToString());
        Assert.AreEqual("DENY", headers.XFrameOptions.ToString());
        Assert.AreEqual("1; mode=block", headers.XXSSProtection.ToString());
        Assert.AreEqual("strict-origin-when-cross-origin", headers["Referrer-Policy"].ToString());
    }

    [TestMethod]
    [TestCategory("Middleware")]
    public async Task InvokeCallsNextMiddleware()
    {
        var httpContext = new DefaultHttpContext();
        var nextCalled = false;

        async Task Next(HttpContext ctx)
        {
            nextCalled = true;
            await Task.CompletedTask;
        }

        var middleware = new SecurityHeadersMiddleware(Next);

        await middleware.InvokeAsync(httpContext);

        Assert.IsTrue(nextCalled);
    }

    [TestMethod]
    [TestCategory("Middleware")]
    public void ConstructorWithNullNextThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new SecurityHeadersMiddleware(null!));
    }

    [TestMethod]
    [TestCategory("Middleware")]
    public async Task InvokeStrictTransportSecurityHeaderHasCorrectValue()
    {
        var httpContext = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(async (ctx) => await Task.CompletedTask);

        await middleware.InvokeAsync(httpContext);

        var hstsHeader = httpContext.Response.Headers.StrictTransportSecurity.ToString();
        Assert.IsTrue(hstsHeader.Contains("max-age=31536000"));
        Assert.IsTrue(hstsHeader.Contains("includeSubDomains"));
    }
}
