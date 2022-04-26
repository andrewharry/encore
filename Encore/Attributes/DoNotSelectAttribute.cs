namespace Encore
{
    /// <summary>
    /// 'Do Not Select' is used to exclude a constructor method from being used in the Dependency Graph Creation
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor)]
    public class DoNotSelectAttribute : Attribute
    {

    }
}
