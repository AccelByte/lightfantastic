using UnityEngine.Assertions;

namespace AccelByte.Core
{
    public interface IServerSession
    {
        string AuthorizationToken { get; }
    }
    
    public static class ServerSessionExtension
    {
        public static bool IsValid(this IServerSession session)
        {
            return !string.IsNullOrEmpty(session.AuthorizationToken);
        }

        public static void AssertValid(this IServerSession session)
        {
            Assert.IsNotNull(session);
            Assert.IsFalse(string.IsNullOrEmpty(session.AuthorizationToken));
        }
    }
}