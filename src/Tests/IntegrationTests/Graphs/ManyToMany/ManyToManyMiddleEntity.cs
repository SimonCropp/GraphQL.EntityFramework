public class ManyToManyMiddleEntity
{
    public ManyToManyLeftEntity ManyToManyLeftEntity { get; set; } = null!;
    public ManyToManyRightEntity ManyToManyRightEntity { get; set; } = null!;
}