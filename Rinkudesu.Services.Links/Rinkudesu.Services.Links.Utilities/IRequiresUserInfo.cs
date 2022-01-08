namespace Rinkudesu.Services.Links.Utilities;

public interface IRequiresUserInfo<out TType>
{
    /// <summary>
    /// Indicates that the implementer needs to receive <see cref="UserInfo"/> before any other operation is called.
    /// </summary>
    TType SetUserInfo(UserInfo userInfo);
}