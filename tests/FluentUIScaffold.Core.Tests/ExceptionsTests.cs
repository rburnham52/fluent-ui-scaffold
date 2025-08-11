using System;
using FluentUIScaffold.Core.Exceptions;
using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class ExceptionsTests
    {
        [Test]
        public void FluentUIScaffoldException_Constructors_Work()
        {
            var exTimeout = new FluentUIScaffold.Core.Exceptions.TimeoutException();
            var exInvalidPage = new InvalidPageException();
            var exElementNotFound = new ElementNotFoundException();
            Assert.That(exTimeout, Is.Not.Null);
            Assert.That(exInvalidPage, Is.Not.Null);
            Assert.That(exElementNotFound, Is.Not.Null);
        }

        [Test]
        public void FluentUIScaffoldBaseException_Constructors_Work()
        {
            var ex1 = new FluentUIScaffoldException();
            var ex2 = new FluentUIScaffoldException("msg");
            var inner = new InvalidOperationException("inner");
            var ex3 = new FluentUIScaffoldException("msg", inner);

            Assert.Multiple(() =>
            {
                Assert.That(ex2.Message, Is.EqualTo("msg"));
                Assert.That(ex3.InnerException, Is.EqualTo(inner));
            });
        }

        [Test]
        public void FluentUIScaffoldValidationException_Constructors_Work()
        {
            var ex1 = new FluentUIScaffoldValidationException();
            var ex2 = new FluentUIScaffoldValidationException("msg");
            var inner = new ArgumentException("bad");
            var ex3 = new FluentUIScaffoldValidationException("msg", inner);

            Assert.Multiple(() =>
            {
                Assert.That(ex2.Message, Is.EqualTo("msg"));
                Assert.That(ex3.InnerException, Is.EqualTo(inner));
            });
        }

        [Test]
        public void VerificationException_Constructors_Work()
        {
            var ex2 = new VerificationException("msg");
            var inner = new Exception("x");
            var ex3 = new VerificationException("msg", inner);

            Assert.Multiple(() =>
            {
                Assert.That(ex2.Message, Is.EqualTo("msg"));
                Assert.That(ex3.InnerException, Is.EqualTo(inner));
            });
        }

        [Test]
        public void ElementValidationException_Constructors_Work()
        {
            var ex1 = new ElementValidationException();
            var ex2 = new ElementValidationException("msg");
            var inner = new Exception("x");
            var ex3 = new ElementValidationException("msg", inner);

            Assert.Multiple(() =>
            {
                Assert.That(ex2.Message, Is.EqualTo("msg"));
                Assert.That(ex3.InnerException, Is.EqualTo(inner));
            });
        }

        [Test]
        public void ElementTimeoutException_Constructors_Work()
        {
            var ex1 = new ElementTimeoutException();
            var ex2 = new ElementTimeoutException("msg");
            var inner = new System.TimeoutException("x");
            var ex3 = new ElementTimeoutException("msg", inner);

            Assert.Multiple(() =>
            {
                Assert.That(ex2.Message, Is.EqualTo("msg"));
                Assert.That(ex3.InnerException, Is.EqualTo(inner));
            });
        }

        [Test]
        public void FluentUIScaffoldPluginException_Constructors_Work()
        {
            var ex1 = new FluentUIScaffoldPluginException();
            var ex2 = new FluentUIScaffoldPluginException("msg");
            var inner = new Exception("x");
            var ex3 = new FluentUIScaffoldPluginException("msg", inner);

            Assert.Multiple(() =>
            {
                Assert.That(ex2.Message, Is.EqualTo("msg"));
                Assert.That(ex3.InnerException, Is.EqualTo(inner));
            });
        }
    }
}