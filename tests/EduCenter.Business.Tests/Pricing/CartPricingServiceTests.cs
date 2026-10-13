using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EduCenter.Business.Models;
using EduCenter.Business.Pricing;
using EduCenter.Business.Services;
using Xunit;

namespace EduCenter.Business.Tests.Pricing
{
    public sealed class CartPricingServiceTests
    {
        private static readonly DateTimeOffset NowUtc =
            new(2026, 10, 14, 12, 0, 0, TimeSpan.Zero);

        public static TheoryData<CartPriceCalculateRequest, string> InvalidRequests => new()
        {
            {
                new CartPriceCalculateRequest(Array.Empty<string>(), "user-1"),
                nameof(CartPriceCalculateRequest.CourseIds)
            },
            {
                new CartPriceCalculateRequest(null!, "user-1"),
                nameof(CartPriceCalculateRequest.CourseIds)
            },
            {
                new CartPriceCalculateRequest([" "], "user-1"),
                "CourseIds[0]"
            },
            {
                new CartPriceCalculateRequest([new string('x', 65)], "user-1"),
                "CourseIds[0]"
            },
            {
                new CartPriceCalculateRequest(["same", "same"], "user-1"),
                nameof(CartPriceCalculateRequest.CourseIds)
            },
            {
                new CartPriceCalculateRequest(
                    Enumerable.Range(1, CartPricingLimits.MaximumCourseCount + 1)
                        .Select(index => $"course-{index}")
                        .ToArray(),
                    "user-1"),
                nameof(CartPriceCalculateRequest.CourseIds)
            },
            {
                new CartPriceCalculateRequest(["course-1"], " "),
                nameof(CartPriceCalculateRequest.UserId)
            }
        };

        [Fact]
        public async Task AppliesHotGroupDiscountWhenStartIsLessThanFiveDaysAway()
        {
            var line = await CalculateSingleLineAsync(
                isGraduate: false,
                CreateCourse(startDateUtc: NowUtc.AddDays(5).AddTicks(-1)));

            Assert.Equal(20m, line.DiscountPercent);
            Assert.Equal(20m, line.DiscountAmount);
            Assert.Equal(80m, line.FinalPrice);
            Assert.Equal("hot_group", line.AppliedReluCode);
        }

        [Fact]
        public async Task DoesNotApplyHotGroupDiscountWhenExactlyFiveDaysRemain()
        {
            var line = await CalculateSingleLineAsync(
                isGraduate: false,
                CreateCourse(startDateUtc: NowUtc.AddDays(5)));

            Assert.Equal(0m, line.DiscountPercent);
            Assert.Equal(0m, line.DiscountAmount);
            Assert.Equal(100m, line.FinalPrice);
            Assert.Null(line.AppliedReluCode);
        }

        [Fact]
        public async Task DoesNotApplyHotGroupDiscountWhenCourseHasAlreadyStarted()
        {
            var line = await CalculateSingleLineAsync(
                isGraduate: false,
                CreateCourse(startDateUtc: NowUtc.AddTicks(-1)));

            Assert.Equal(0m, line.DiscountPercent);
            Assert.Null(line.AppliedReluCode);
        }

        [Fact]
        public async Task AppliesGraduateDiscount()
        {
            var line = await CalculateSingleLineAsync(
                isGraduate: true,
                CreateCourse(startDateUtc: NowUtc.AddDays(10)));

            Assert.Equal(30m, line.DiscountPercent);
            Assert.Equal(30m, line.DiscountAmount);
            Assert.Equal(70m, line.FinalPrice);
            Assert.Equal("graduate", line.AppliedReluCode);
        }

        [Fact]
        public async Task SelectsGraduateDiscountInsteadOfAddingDiscounts()
        {
            var line = await CalculateSingleLineAsync(
                isGraduate: true,
                CreateCourse(startDateUtc: NowUtc.AddDays(4)));

            Assert.Equal(30m, line.DiscountPercent);
            Assert.Equal(30m, line.DiscountAmount);
            Assert.Equal(70m, line.FinalPrice);
            Assert.Equal("graduate", line.AppliedReluCode);
        }

        [Fact]
        public async Task CalculatesDiscountsPerCourseAndAggregatesTotals()
        {
            var nearCourse = CreateCourse(
                id: "near",
                price: 99.99m,
                startDateUtc: NowUtc.AddDays(4));
            var farCourse = CreateCourse(
                id: "far",
                price: 199.99m,
                startDateUtc: NowUtc.AddDays(10));

            var service = CreateService(false, nearCourse, farCourse);
            var response = await service.CalculateAsync(
                new CartPriceCalculateRequest(["near", "far"], "user-1"));

            var linesById = response.Items.ToDictionary(line => line.CourseId);

            Assert.Equal(20m, linesById["near"].DiscountPercent);
            Assert.Equal(20m, linesById["near"].DiscountAmount);
            Assert.Equal(79.99m, linesById["near"].FinalPrice);

            Assert.Equal(0m, linesById["far"].DiscountPercent);
            Assert.Equal(199.99m, linesById["far"].FinalPrice);

            Assert.Equal(299.98m, response.TotalBeforeDiscount);
            Assert.Equal(20m, response.TotalDiscount);
            Assert.Equal(279.98m, response.TotalAfterDiscount);
        }

        [Fact]
        public async Task RoundsDiscountMidpointAwayFromZero()
        {
            var line = await CalculateSingleLineAsync(
                isGraduate: true,
                CreateCourse(price: 0.05m, startDateUtc: NowUtc.AddDays(10)));

            Assert.Equal(0.015m, 0.05m * 30m / 100m);
            Assert.Equal(0.02m, line.DiscountAmount);
            Assert.Equal(0.03m, line.FinalPrice);
        }

        [Fact]
        public async Task RejectsCartWhenAnyCourseIdIsUnknown()
        {
            var service = CreateService(false, CreateCourse(id: "known"));

            var exception = await Assert.ThrowsAsync<UnknownCourseIdsException>(
                () => service.CalculateAsync(
                    new CartPriceCalculateRequest(["known", "missing"], "user-1")));

            Assert.Equal(new[] { "missing" }, exception.CourseIds);
        }


        [Fact]
        public async Task AcceptsMaximumNumberOfCourses()
        {
            var courses = Enumerable.Range(1, CartPricingLimits.MaximumCourseCount)
                .Select(index => CreateCourse(
                    id: $"course-{index}",
                    startDateUtc: NowUtc.AddDays(10)))
                .ToArray();

            var service = CreateService(false, courses);
            var request = new CartPriceCalculateRequest(
                courses.Select(course => course.Id).ToArray(),
                "user-1");

            var response = await service.CalculateAsync(request);

            Assert.Equal(CartPricingLimits.MaximumCourseCount, response.Items.Count);
        }

        [Theory]
        [MemberData(nameof(InvalidRequests))]
        public async Task RejectsInvalidRequests(
            CartPriceCalculateRequest request,
            string expectedField)
        {
            var service = CreateService(false, Array.Empty<Course>());

            var exception = await Assert.ThrowsAsync<InvalidCartPriceCalculateRequestException>(
                () => service.CalculateAsync(request));

            Assert.Equal(expectedField, exception.Field);
        }

        private static async Task<CartPriceLine> CalculateSingleLineAsync(
            bool isGraduate, 
            Course course)
        {
            var service = CreateService(isGraduate, course);
            var response = await service.CalculateAsync(
                new CartPriceCalculateRequest([course.Id], "user-1"));

            return Assert.Single(response.Items);
        }

        private static CartPricingService CreateService(
            bool isGraduate,
            params Course[] courses)
        {
            return new CartPricingService(
                new StubUserService(new User
                {
                    Id = "user-1",
                    IsGraduate = isGraduate
                }),
                new StubCourseService(courses),
                new FixedTimeProvider(NowUtc));
        }

        private static Course CreateCourse(
            string id = "course-1",
            decimal price = 100m,
            DateTimeOffset? startDateUtc = null)
        {
            return new Course
            {
                Id = id,
                Title = $"Course {id}",
                Description = string.Empty,
                Price = price,
                StartDateUtc = startDateUtc ?? NowUtc.AddDays(10)
            };
        }

        private sealed class FixedTimeProvider : TimeProvider
        {
            private readonly DateTimeOffset _utcNow;

            public FixedTimeProvider(DateTimeOffset utcNow)
            {
                _utcNow = utcNow;
            }

            public override DateTimeOffset GetUtcNow()
            {
                return _utcNow;
            }
        }

        private sealed class StubUserService : IUserService
        {
            private readonly User? _user;

            public StubUserService(User? user)
            {
                _user = user;
            }

            public Task<User?> GetByUserIdAsync(
                string id,
                CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(_user);
            }
        }

        private sealed class StubCourseService : ICourseService
        {
            private readonly IReadOnlyCollection<Course> _courses;

            public StubCourseService(IReadOnlyCollection<Course> courses)
            {
                _courses = courses;
            }

            public Task<IReadOnlyList<Course>> GetByCourseIdsAsync(
                IReadOnlyCollection<string> ids,
                CancellationToken cancellationToken = default)
            {
                cancellationToken.ThrowIfCancellationRequested();

                IReadOnlyList<Course> matchingCourses = _courses
                    .Where(course => ids.Contains(course.Id, StringComparer.Ordinal))
                    .ToArray();

                return Task.FromResult(matchingCourses);
            }
        }
    }
}
