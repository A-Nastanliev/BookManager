namespace ServiceLayer.Dto.Reading
{
	public class BookRatingSummaryDto
	{
		public int Count { get; set; }
		public double Average { get; set; }

		public BookRatingSummaryDto() { }

		public BookRatingSummaryDto(int count, double average)
		{
			Count = count;
			Average = average;
		}
	}
}