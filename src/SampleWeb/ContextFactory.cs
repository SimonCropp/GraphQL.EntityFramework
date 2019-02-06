
public class ContextFactory
{
    public MyDataContext BuildContext() => DataContextBuilder.BuildDataContext();
}