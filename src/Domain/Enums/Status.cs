namespace Domain.Enums
{
	public enum Status : short
	{
		Draft = -10,
		Unknown = 0,
		Created = 10,
		Schedule = 20,
		Documents = 30,
		Records = 40,
		Training = 50,
		Completed = 60,
		Approved = 201,
		Rejected = 202,
	}
}
