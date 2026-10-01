using dotnet_cie2.Models;

namespace dotnet_cie2.Extensions
{
    public static class CourseExtensions
    {
        /// <summary>
        /// Calculates the final fees for a course after applying the discount percentage.
        /// </summary>
        public static decimal CalculateFinalFees(this Course course)
        {
            if (course == null)
            {
                return 0m;
            }

            decimal discountAmount = course.Fees * (course.DiscountPercentage / 100m);
            decimal finalFees = course.Fees - discountAmount;
            return Math.Round(finalFees, 2);
        }

        /// <summary>
        /// Extension method to format discount savings text.
        /// </summary>
        public static decimal GetDiscountAmount(this Course course)
        {
            if (course == null) return 0m;
            return Math.Round(course.Fees * (course.DiscountPercentage / 100m), 2);
        }
    }
}
